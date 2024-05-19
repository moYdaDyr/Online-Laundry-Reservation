using Microsoft.SqlServer.Server;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static Kursovaya2.App;
using Label = System.Windows.Controls.Label;

namespace Kursovaya2
{
    /// <summary>
    /// Логика взаимодействия для StudentMenuPage.xaml
    /// </summary>
    public partial class StudentMenuPage : Page, IPageN
    {
        public event NavigationRequest NavigationRequested;
        public void SubscribeMainWindow()
        {
            this.NavigationService.Navigating += OnNavigatingFrom;
            NavigationRequested += (Window.GetWindow(this) as MainWindow).NavigateFunc;
        }
        public void UnsubscribeMainWindow()
        {
            NavigationRequested -= (Window.GetWindow(this) as MainWindow).NavigateFunc;
            this.NavigationService.Navigating -= OnNavigatingFrom;
        }

        public void ReturnBack()
        {
            SubscribeMainWindow();
            NavigationRequested(new Uri("StartPage.xaml", UriKind.Relative));
            return;
        }

        public bool IsStart
        {
            get
            {
                return false;
            }
        }

        public void OnNavigatingFrom(object sender, NavigatingCancelEventArgs e)
        {
            App.LanguageChanged -= UpdateOnLangChange;
            UnsubscribeMainWindow();
        }

        public void UpdateOnLangChange()
        {
            UpdatePersonalInfo();
        }

        static readonly string selectResidentPersonalData = "WITH LoginID AS (SELECT \"ID\" FROM \"People\" WHERE \"Login\" = @login) SELECT \"Resident ID\", \"Login\", \"Name\", \"Surname\", \"Patronymic\", \"Room\", \"Student card\" FROM \"People\", \"Residents\" WHERE \"ID\"=\"Resident ID\" AND \"ID\" = (SELECT * FROM LoginID) LIMIT 1;";
        static readonly string selectResidentStatus = "WITH LoginID AS (SELECT \"ID\" FROM \"People\" WHERE \"Login\" = @login) SELECT EXISTS(SELECT \"ID\" FROM \"Blocking_journal\" WHERE \"Person\" = (SELECT * FROM LoginID));";
        static readonly string selectPersonBlockings = "SELECT \"Blocking_journal\".\"ID\" AS BID, \"Reason\", \"{0}\", GetAttendantNameID.Name, GetAttendantNameID.Surname, GetAttendantNameID.Patronymic, \"Date\" FROM \"Blocking_journal\", \"Blocking_reasons\", GetAttendantNameID(\"Attendant\") WHERE \"Person\" = @person ORDER BY \"Date\" DESC;";
        static readonly string selectBuilding = "WITH LoginID AS (SELECT \"ID\" FROM \"People\" WHERE \"Login\" = @login) SELECT \"{0}\" FROM \"Buildings\", \"Residents\" WHERE \"Resident ID\" = (SELECT * FROM LoginID LIMIT 1) AND \"Building\"=\"ID\";";

        ResidentInf info;

        void UpdatePersonalInfo()
        {
            info = new ResidentInf();

            using (NpgsqlCommand cmd = new NpgsqlCommand(selectResidentPersonalData, App.workConnection))
            {
                cmd.Parameters.AddWithValue("@login", App.login);
                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        info.Name = reader["Name"].ToString();
                        info.Surname = reader["Surname"].ToString();
                        info.Patronymic = reader["Patronymic"].ToString();
                        info.StudentCard = reader["Student card"].ToString();
                        info.Room = ((short)reader["Room"]).ToString();
                        info.Login = reader["Login"].ToString();
                    }
                }
            }
            using (NpgsqlCommand cmd = new NpgsqlCommand(selectResidentStatus, App.workConnection))
            {
                cmd.Parameters.AddWithValue("@login", App.login);
                info.Status = !((bool)cmd.ExecuteScalar());
                info.StatusText = info.Status ? (string)TryFindResource("m_studentStartPersonalStatusUnlocked")
                : (string)TryFindResource("m_studentStartPersonalStatusBlocked");
            }
            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(selectBuilding, GetLocalisedKey("Name")), App.workConnection))
            {
                cmd.Parameters.AddWithValue("@login", App.login);
                info.Building = (string)cmd.ExecuteScalar();
            }

            List<PersonalDataT> infoList = GetListPersonalDataT(info);

            //PersonalDataGrid.Items.Clear();
            PersonalDataGrid.ItemsSource = infoList;

            
            if (!info.Status)
            {
                ShowBlockingMessage();
            }
            
        }

        void ShowBlockingMessage()
        {
            string message = (string)TryFindResource("m_residentBlockingMessageStart") + "\n";

            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(selectPersonBlockings, App.GetLocalisedKey("Name")), App.workConnection))
            {
                cmd.Parameters.AddWithValue("@person", info.ID);
                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                {
                    int i = 1;
                    while (reader.Read())
                    {
                        string template = i + ". " + (string)TryFindResource("m_residentBlockingMessage");

                        var id = (int)reader["BID"];
                        var desc = string.Format(template, (string)reader["surname"], (string)reader["name"], (string)reader["patronymic"], (string)reader[App.GetLocalisedKey("Name")], ((DateTime)reader["Date"]).ToString().Remove(10));

                        message += desc + "\n";
                    }
                }
            }

            message += (string)TryFindResource("m_residentBlockingMessageEnd");

            MessageBox.Show(message, (string)this.TryFindResource("m_error"), MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public StudentMenuPage()
        {
            InitializeComponent();

            UpdatePersonalInfo();

            App.LanguageChanged += UpdateOnLangChange;

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
            p.PersonalData = inf.Login;
            p.PersonalDataCat = (string)TryFindResource("m_studentStartPersonalLogin");
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

            p = new PersonalDataT();
            p.PersonalData = inf.StatusText;
            p.PersonalDataCat = (string)TryFindResource("m_studentStartPersonalStatus");
            ll.Add(p);

            return ll;
        }

        public class PersonalDataT
        {
            public string PersonalDataCat { get; set; }

            public string PersonalData { get; set; }

        }

        private void StudentGoBook_Click(object sender, RoutedEventArgs e)
        {
            if (!info.Status)
            {
                ShowBlockingMessage();
                return;
            }

            SubscribeMainWindow();

            NavigationRequested(new Uri("StudentBookPage.xaml", UriKind.Relative));
        }

        private void StudentGoCheck_Click(object sender, RoutedEventArgs e)
        {
            if (!info.Status)
            {
                ShowBlockingMessage();
                return;
            }

            SubscribeMainWindow();

            NavigationRequested(new Uri("StudentRedactBookPage.xaml", UriKind.Relative));
        }

        
    }
}
