using Npgsql;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Kursovaya2
{
    /// <summary>
    /// Логика взаимодействия для AdministratorRequestWindow.xaml
    /// </summary>
    public partial class AdministratorRequestWindow : Window
    {
        public AdministratorRequestWindow()
        {
            InitializeComponent();
            App.LanguageChanged += UpdateOnLangChange;
            this.Loaded += AttendantRequestPage_Loaded;
        }

        public void UpdateOnLangChange()
        {
            UploadRequestList();
        }

        enum RequestStatuses
        {
            NotInWork, InProgress, Done
        }

        static readonly string selectBasicRequestInformation = "SELECT * FROM SelectBasicRequestInformation( ) ORDER BY date ASC;";
        static readonly string loadRequest = "SELECT \"Text\" FROM \"Requests\" WHERE \"ID\" = @id;";
        static readonly string updateRequestStatus = "UPDATE \"Requests\" WHERE \"ID\" = @id SET \"Status\" = @status;";

        class RequestInfo
        {
            public int ID { get; set; }

            public string Header { get; set; }
        }

        ObservableCollection<RequestInfo> requests;
        int currentRequest = -1;
        string currentRequestText;

        bool isSaved = true;
        bool pleaseNoNeedToWarning = false;

        private void AttendantRequestPage_Loaded(object sender, RoutedEventArgs e)
        {
            UploadRequestList();
        }


        void UploadRequestList()
        {
            pleaseNoNeedToWarning = true;
            requests = new ObservableCollection<RequestInfo>();

            using (NpgsqlCommand cmd = new NpgsqlCommand(selectBasicRequestInformation, App.workConnection))
            {
                cmd.Parameters.AddWithValue("@login", App.login);
                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var id = (int)reader["id"];
                        var date = (DateTime)reader["date"];
                        var theme = (string)reader[App.GetLocalisedKey("theme")];
                        var status = (string)reader[App.GetLocalisedKey("status")];

                        RequestInfo r = new RequestInfo();
                        r.ID = id;

                        r.Header = date.ToString().Remove(10) + " " + theme + " - " + status;

                        requests.Add(r);
                    }
                }
            }

            AdministratorRequestsSelector.ItemsSource = from r in requests select r.Header;

            if (requests.Count == 0) return;

            AdministratorRequestsSelector.SelectedIndex = 0;

            currentRequest = 0;
            UploadCurrentText();

            pleaseNoNeedToWarning = false;
        }

        void UploadCurrentText()
        {
            string text;

            using (NpgsqlCommand cmd = new NpgsqlCommand(loadRequest, App.workConnection))
            {
                cmd.Parameters.AddWithValue("@id", requests[currentRequest].ID);

                text = (string)cmd.ExecuteScalar();
            }

            AdministratorRequestText.Text = text;
        }

        private void AdministratorQuitRequestButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void AdministratorTakeRequestButton_Click(object sender, RoutedEventArgs e)
        {
            using (NpgsqlCommand cmd = new NpgsqlCommand(updateRequestStatus, App.workConnection))
            {
                cmd.Parameters.AddWithValue("@id", requests[currentRequest].ID);
                cmd.Parameters.AddWithValue("Status", RequestStatuses.InProgress);

                cmd.ExecuteNonQuery();
            }
            UploadRequestList();
        }

        private void AdministratorCompleteRequestButton_Click(object sender, RoutedEventArgs e)
        {
            using (NpgsqlCommand cmd = new NpgsqlCommand(updateRequestStatus, App.workConnection))
            {
                cmd.Parameters.AddWithValue("@id", requests[currentRequest].ID);
                cmd.Parameters.AddWithValue("Status", RequestStatuses.Done);

                cmd.ExecuteNonQuery();
            }
            UploadRequestList();
        }

        private void AdministratorRequestsSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            currentRequest = AdministratorRequestsSelector.SelectedIndex;
            UploadCurrentText();
        }
    }
}
