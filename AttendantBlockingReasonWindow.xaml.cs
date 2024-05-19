using Npgsql;
using System;
using System.Collections.Generic;
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

namespace Kursovaya2
{
    /// <summary>
    /// Логика взаимодействия для AttendantBlockingReasonWindow.xaml
    /// </summary>
    public partial class AttendantBlockingReasonWindow : Window
    {
        readonly string selectBlockingReasons = "SELECT \"ID\", \"{0}\" FROM \"Blocking_reasons\";";
        readonly string selectPersonBlockings = "SELECT \"Blocking_journal\".\"ID\" AS BID, \"Reason\", \"NameRU\", GetAttendantNameID.Name, GetAttendantNameID.Surname, GetAttendantNameID.Patronymic, \"Date\" FROM \"Blocking_journal\", \"Blocking_reasons\", GetAttendantNameID(\"Attendant\") WHERE \"Person\" = @person ORDER BY \"Date\" DESC;";

        bool isBlocking;

        List<string> description;
        List<int> ids;
        int personId;

        int selected;

        public delegate void AttendantBlockingReasonWindowReturn(int id);

        public event AttendantBlockingReasonWindowReturn AttendantBlockingReasonWindowReturnEvent;

        public AttendantBlockingReasonWindow(bool isBlocking, int personId = -1)
        {
            InitializeComponent();

            description = new List<string>();
            ids = new List<int>();

            this.isBlocking = isBlocking;

            if (isBlocking)
            {
                AttendantBlockingReasonsBlockLabel.Visibility = Visibility.Visible;
                AttendantBlockingReasonsUnblockLabel.Visibility = Visibility.Hidden;

                using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(selectBlockingReasons, App.GetLocalisedKey("Name")), App.workConnection))
                {
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var id = (short)reader["ID"];
                            var desc = (string)reader[App.GetLocalisedKey("Name")];

                            ids.Add(id);
                            description.Add(desc);
                        }
                    }
                }

                AttendantBlockingReasonsSelector.ItemsSource = description;

                return;
            }

            AttendantBlockingReasonsBlockLabel.Visibility = Visibility.Hidden;
            AttendantBlockingReasonsUnblockLabel.Visibility = Visibility.Visible;

            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(selectPersonBlockings, App.GetLocalisedKey("Name")), App.workConnection))
            {
                cmd.Parameters.AddWithValue("@person", personId);
                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string template = (string)TryFindResource("m_attendantBlockingTemplate"); 

                        var id = (int)reader["BID"];
                        var desc = string.Format(template,(string)reader["surname"], (string)reader["name"], (string)reader["patronymic"], (string)reader[App.GetLocalisedKey("Name")], ((DateTime)reader["Date"]).ToString().Remove(10));

                        ids.Add(id);
                        description.Add(desc);
                    }
                }
            }

            AttendantBlockingReasonsSelector.ItemsSource = description;
        }

        private void AttendantBlockingReasonsCancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void AttendantBlockingReasonsOkButton_Click(object sender, RoutedEventArgs e)
        {
            AttendantBlockingReasonWindowReturnEvent(ids[selected]);
            this.Close();
        }

        private void AttendantBlockingReasonsSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selected = AttendantBlockingReasonsSelector.SelectedIndex;
        }
    }
}
