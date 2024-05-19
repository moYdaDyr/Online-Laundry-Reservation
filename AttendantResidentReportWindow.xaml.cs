using Microsoft.Win32;
using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static Kursovaya2.App;
using static Kursovaya2.StudentMenuPage;
using iTextSharp;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Org.BouncyCastle.Asn1.BC;
using System.Data;
using System.IO;

namespace Kursovaya2
{
    /// <summary>
    /// Логика взаимодействия для AttendantResidentReportWindow.xaml
    /// </summary>
    public partial class AttendantResidentReportWindow : Window
    {
        static readonly string selectResidentData = "SELECT \"ID\", \"Name\", \"Surname\", \"Patronymic\", \"Room\", \"Student card\" FROM \"People\", \"Residents\" WHERE \"ID\" = @resident LIMIT 1;";
        static readonly string selectResidentBlockings = "SELECT \"Blocking_journal\".\"ID\" AS BID, \"Reason\", \"{0}\", GetAttendantNameID.Name, GetAttendantNameID.Surname, GetAttendantNameID.Patronymic, \"Date\" FROM \"Blocking_journal\", \"Blocking_reasons\", GetAttendantNameID(\"Attendant\") WHERE \"Person\" = @resident ORDER BY \"Date\" DESC;";
        static readonly string selectResidentTimeViolations = "SELECT \"Size\", \"Date\" FROM \"Time_violations\" WHERE \"Resident\" = @resident ORDER BY \"Date\" DESC";
        static readonly string selectBuilding = "SELECT \"{0}\" FROM \"Buildings\", \"Residents\" WHERE \"Resident ID\" = @resident AND \"Building\"=\"ID\";";

        int residentdID;

        ResidentInf info;

        public AttendantResidentReportWindow(int resident)
        {
            InitializeComponent();

            residentdID = resident;

            CreateResidentPage();

            App.LanguageChanged += OnLangChange;
            this.Closing += OnClose;
        }

        void OnLangChange()
        {
            CreateResidentPage();
        }

        void OnClose(object sender, CancelEventArgs e)
        {
            if (!exitFlag)
            {
                var result = MessageBox.Show((string)TryFindResource("m_AttendantReportOnCancel"), (string)TryFindResource("m_confirmatioRequired"), MessageBoxButton.OKCancel, MessageBoxImage.Question);

                if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
            }

            App.LanguageChanged -= OnLangChange;
            this.Closing -= OnClose;
        }

        void CreateResidentPage()
        {
            info = new ResidentInf();

            using (NpgsqlCommand cmd = new NpgsqlCommand(selectResidentData, App.workConnection))
            {
                cmd.Parameters.AddWithValue("@resident", residentdID);
                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        info.ID = (int)reader["ID"];
                        info.Name = reader["Name"].ToString();
                        info.Surname = reader["Surname"].ToString();
                        info.Patronymic = reader["Patronymic"].ToString();
                        info.StudentCard = reader["Student card"].ToString();
                        info.Room = ((short)reader["Room"]).ToString();
                    }
                }
            }

            info.Status = true;

            string message="";

            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(selectResidentBlockings, App.GetLocalisedKey("Name")), App.workConnection))
            {
                cmd.Parameters.AddWithValue("@resident", residentdID);
                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                {
                    int i = 1;
                    while (reader.Read())
                    {
                        string template = i + ". " + (string)TryFindResource("m_AttendantReportBlocking");

                        var desc = string.Format(template, (string)reader["surname"], (string)reader["name"], (string)reader["patronymic"], (string)reader[App.GetLocalisedKey("Name")], ((DateTime)reader["Date"]).ToString().Remove(10));

                        message += desc + "\n";

                        info.Status = false;
                        i++;
                    }
                }
            }

            if (info.Status)
            {
                message = (string)TryFindResource("m_AttendantReportBlockingNone");
            }
            else
            {
                message = (string)TryFindResource("m_AttendantReportBlockingStart") + "\n" + message;
            }

            ReportBlockings.Text = message;

            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(selectBuilding, GetLocalisedKey("Name")), App.workConnection))
            {
                cmd.Parameters.AddWithValue("@resident", residentdID);
                info.Building = (string)cmd.ExecuteScalar();
            }

            bool flag = true;

            string message2 = "";

            using (NpgsqlCommand cmd = new NpgsqlCommand(selectResidentTimeViolations, App.workConnection))
            {
                cmd.Parameters.AddWithValue("@resident", residentdID);
                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                {
                    int i = 1;
                    while (reader.Read())
                    {
                        string template = i + ". " + (string)TryFindResource("m_AttendantReportViolation");

                        TimeSpan t = (TimeSpan)reader["Size"];

                        var desc = string.Format(template, ((DateTime)reader["Date"]).ToString().Remove(10), App.TimeSpanToString(t));

                        message2 += desc + "\n";

                        flag = false;
                        i++;
                    }

                }
            }

            if (flag)
            {
                message2 = (string)TryFindResource("m_AttendantReportViolationNone");
            }
            else
            {
                message2 = (string)TryFindResource("m_AttendantReportViolationStart") + "\n" + message2;
            }

            ReportBlockings.Text = message;
            ReportViolations.Text = message2;

            List<PersonalDataT> infoList = GetListPersonalDataT(info);

            ReportResidentData.ItemsSource = infoList;

            ReportHeader.Text = (string)TryFindResource("m_AttendantReportHeader");
            ReportResidentsDataHeader.Text = (string)TryFindResource("m_AttendantReportTableHeader");
            ReportDate.Text = (string)TryFindResource("m_AttendantReportDate")+ " " + DateTime.Now.ToString("dd.MM.yyyy");
        }

        void CreatePDFReport(string file)
        {
            iTextSharp.text.Document doc = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 85.7f ,42.9f, 57.2f, 57.2f);

            PdfWriter.GetInstance(doc, new FileStream(file, FileMode.Create));

            doc.Open();

            BaseFont baseFont = BaseFont.CreateFont("C:\\Windows\\Fonts\\Times.ttf", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);

            iTextSharp.text.Font casualFont = new iTextSharp.text.Font(baseFont, 13, iTextSharp.text.Font.NORMAL);
            iTextSharp.text.Font tableFont = new iTextSharp.text.Font(baseFont, 11, iTextSharp.text.Font.NORMAL);
            iTextSharp.text.Font tableHeadFont = new iTextSharp.text.Font(baseFont, 11, iTextSharp.text.Font.BOLD);
            iTextSharp.text.Font headerFont = new iTextSharp.text.Font(baseFont, 16, iTextSharp.text.Font.BOLD);

            iTextSharp.text.Paragraph head = new iTextSharp.text.Paragraph((string)TryFindResource("m_AttendantReportHeader"), headerFont);
            head.Alignment = Element.ALIGN_CENTER;
            head.SpacingBefore = 0;
            head.SpacingAfter = 12;
            doc.Add(head);

            iTextSharp.text.Paragraph tableHead = new iTextSharp.text.Paragraph((string)TryFindResource("m_AttendantReportTableHeader"), casualFont);
            tableHead.Alignment = Element.ALIGN_LEFT;
            tableHead.SpacingAfter = 5;
            doc.Add(tableHead);

            PdfPTable table = new PdfPTable(2);
            List<PersonalDataT> infoList = (List<PersonalDataT>)ReportResidentData.ItemsSource;
            for (int i=0;i< infoList.Count; i++)
            {
                PdfPCell headCell = new PdfPCell(new Phrase(infoList[i].PersonalDataCat, tableHeadFont));
                headCell.VerticalAlignment = 1;
                headCell.HorizontalAlignment = 0;
                headCell.UseAscender = true;
                table.AddCell(headCell);

                PdfPCell cell = new PdfPCell(new Phrase(infoList[i].PersonalData, tableFont));
                cell.VerticalAlignment = 1;
                cell.HorizontalAlignment = 0;
                cell.UseAscender = true;
                table.AddCell(cell);
            }
            doc.Add(table);

            iTextSharp.text.Paragraph violationPara = new iTextSharp.text.Paragraph((string)ReportViolations.Text, casualFont);
            violationPara.Alignment = Element.ALIGN_LEFT;
            violationPara.SpacingBefore = 10;
            violationPara.SpacingAfter = 0;
            doc.Add(violationPara);

            iTextSharp.text.Paragraph blockingPara = new iTextSharp.text.Paragraph((string)ReportBlockings.Text, casualFont);
            blockingPara.Alignment = Element.ALIGN_LEFT;
            blockingPara.SpacingBefore = 10;
            blockingPara.SpacingAfter = 0;
            doc.Add(blockingPara);

            iTextSharp.text.Paragraph datePara = new iTextSharp.text.Paragraph((string)ReportDate.Text, casualFont);
            datePara.Alignment = Element.ALIGN_RIGHT;
            datePara.SpacingBefore = 10;
            datePara.SpacingAfter = 0;
            doc.Add(datePara);

            doc.Close();
        }

        public List<PersonalDataT> GetListPersonalDataT(ResidentInf inf)
        {

            List<PersonalDataT> ll = new List<PersonalDataT>();
            PersonalDataT p = new PersonalDataT();
            p.PersonalData = inf.Name;
            p.PersonalDataCat = (string)TryFindResource("m_studentStartPersonalName");
            ll.Add(p);

            p = new PersonalDataT();
            p.PersonalData = inf.Surname;
            p.PersonalDataCat = (string)TryFindResource("m_studentStartPersonalSurname");
            ll.Add(p);

            p = new PersonalDataT();
            p.PersonalData = inf.Patronymic;
            p.PersonalDataCat = (string)TryFindResource("m_studentStartPersonalPatronymic");
            ll.Add(p);

            p = new PersonalDataT();
            p.PersonalData = inf.Building;
            p.PersonalDataCat = (string)TryFindResource("m_studentStartPersonalBuilding");
            ll.Add(p);

            p = new PersonalDataT();
            p.PersonalData = inf.StudentCard;
            p.PersonalDataCat = (string)TryFindResource("m_studentStartPersonalStudentCard");
            ll.Add(p);

            p = new PersonalDataT();
            p.PersonalData = inf.Room;
            p.PersonalDataCat = (string)TryFindResource("m_studentStartPersonalRoom");
            ll.Add(p);

            return ll;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        bool exitFlag = false;

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "(*.pdf)|*.pdf";

            sfd.FileName = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + (string)TryFindResource("m_AttendantDefaultFileNameStart") + info.Surname + "_" + info.Name;

            if (sfd.ShowDialog() == false)
                return;

            string file = sfd.FileName;

            try
            {
                CreatePDFReport(file);

                MessageBox.Show((string)TryFindResource("m_AttendantReportSuccess"), (string)TryFindResource("m_success"), MessageBoxButton.OK, MessageBoxImage.Information);

                exitFlag = true;

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show((string)TryFindResource("m_AttendantReportError") + ex.Message, (string)TryFindResource("m_error"), MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
        }
    }
}
