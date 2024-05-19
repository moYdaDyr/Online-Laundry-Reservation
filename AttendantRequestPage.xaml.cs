using Npgsql;
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
using static Kursovaya2.App;
using static Kursovaya2.AttendantResidentsPage;

namespace Kursovaya2
{
    /// <summary>
    /// Логика взаимодействия для AttendantRequestPage.xaml
    /// </summary>
    public partial class AttendantRequestPage : Page, IPageN
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
            NavigationRequested(new Uri("AttendantMenuPage.xaml", UriKind.Relative));
            return;
        }

        public bool IsStart
        {
            get
            {
                return false;
            }
        }

        public void UpdateOnLangChange()
        {
            UploadRequestList();
        }

        static readonly string selectBasicRequestInformation = "WITH LoginID AS (SELECT \"ID\" FROM \"People\" WHERE \"Login\" = @login LIMIT 1) SELECT * FROM SelectBasicRequestInformationAtt( (SELECT * FROM LoginID) ) ORDER BY date DESC;";
        static readonly string loadRequest = "SELECT \"Text\" FROM \"Requests\" WHERE \"ID\" = @id;";
        static readonly string selectLoginID = "SELECT \"ID\" FROM \"People\" WHERE \"Login\" = @login LIMIT 1;";
        static readonly string updateRequest = "CALL updaterequest(@id, @loginID, @text);";
        static readonly string deleteRequest = "CALL deleterequest(@id, @loginID);";

        public void OnNavigatingFrom(object sender, NavigatingCancelEventArgs e)
        {
            this.NavigationService.Navigating -= OnClose;
            App.LanguageChanged -= UpdateOnLangChange;
            UnsubscribeMainWindow();
        }

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

        public AttendantRequestPage()
        {
            InitializeComponent();

            App.LanguageChanged += UpdateOnLangChange;

            this.Loaded += AttendantRequestPage_Loaded;
        }

        private void AttendantRequestPage_Loaded(object sender, RoutedEventArgs e)
        {
            UploadRequestList();

            this.NavigationService.Navigating += OnClose;

            if (requests.Count==0)
            {

                AttendantNewRequestWindow aw = new AttendantNewRequestWindow();

                aw.ShowDialog();

                UploadRequestList();
                return;
            }
        }

        void OnClose(object sender, CancelEventArgs e)
        {
            if (!isSaved)
            {
                var res = MessageBox.Show((string)TryFindResource("m_AttendantRequestsOnChange"), (string)TryFindResource("m_confirmatioRequired"), MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation);

                if (res == MessageBoxResult.Yes)
                {
                    SaveCurrentText();
                    
                }
                else if (res == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
            }
            e.Cancel = false;
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
                        var theme = (string)reader[GetLocalisedKey("theme")];
                        var status = (string)reader[GetLocalisedKey("status")];

                        RequestInfo r = new RequestInfo();
                        r.ID = id;

                        r.Header = date.ToString().Remove(10) + " " + theme + " - " + status;

                        requests.Add(r);
                    }
                }
            }

            AttendantRequestsSelector.ItemsSource = from r in requests select r.Header;

            if (requests.Count == 0) return;

            AttendantRequestsSelector.SelectedIndex = 0;

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

            AttendantRequestText.Text = text;
        }

        void SaveCurrentText()
        {
            int loginID;

            using (NpgsqlCommand cmd = new NpgsqlCommand(selectLoginID, App.workConnection))
            {
                cmd.Parameters.AddWithValue("@login", App.login);

                loginID = (int)cmd.ExecuteScalar();
            }

            using (NpgsqlCommand cmd = new NpgsqlCommand(updateRequest, App.workConnection))
            {
                cmd.Parameters.AddWithValue("@id", requests[currentRequest].ID);
                cmd.Parameters.AddWithValue("@loginID", loginID);
                cmd.Parameters.AddWithValue("@text", currentRequestText);

                cmd.ExecuteNonQuery();
            }
        }

        void DeleteRequest()
        {
            int loginID;

            using (NpgsqlCommand cmd = new NpgsqlCommand(selectLoginID, App.workConnection))
            {
                cmd.Parameters.AddWithValue("@login", App.login);

                loginID = (int)cmd.ExecuteScalar();
            }

            using (NpgsqlCommand cmd = new NpgsqlCommand(deleteRequest, App.workConnection))
            {
                cmd.Parameters.AddWithValue("@id", requests[currentRequest].ID);
                cmd.Parameters.AddWithValue("@loginID", loginID);

                cmd.ExecuteNonQuery();
            }
        }

        private void AttendantUpdateRequestButton_Click(object sender, RoutedEventArgs e)
        {
            isSaved = true;

            SaveCurrentText();

            MessageBox.Show((string)TryFindResource("m_AttendantRequestReSended"), (string)TryFindResource("m_success"), MessageBoxButton.OK, MessageBoxImage.Information);

            UploadRequestList();
        }

        private void AttendantDeleteRequestButton_Click(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show((string)TryFindResource("m_AttendantRequestDelete"), (string)TryFindResource("m_confirmatioRequired"), MessageBoxButton.YesNo, MessageBoxImage.Exclamation);

            if (res == MessageBoxResult.No) return;

            DeleteRequest();

            MessageBox.Show((string)TryFindResource("m_AttendantRequestDeleted"), (string)TryFindResource("m_success"), MessageBoxButton.OK, MessageBoxImage.Information);

            UploadRequestList();
        }

        private void AttendantCreateRequestButton_Click(object sender, RoutedEventArgs e)
        {
            AttendantNewRequestWindow aw = new AttendantNewRequestWindow();

            aw.ShowDialog();

            UploadRequestList();
        }

        private void AttendantRequestsSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isSaved && !pleaseNoNeedToWarning)
            {
                var res = MessageBox.Show((string)TryFindResource("m_AttendantRequestsOnChange"), (string)TryFindResource("m_confirmatioRequired"),MessageBoxButton.YesNoCancel,MessageBoxImage.Exclamation);

                if (res == MessageBoxResult.Yes)
                {
                    SaveCurrentText();
                }
                else if (res == MessageBoxResult.Cancel) return;
            }

            
            currentRequest = AttendantRequestsSelector.SelectedIndex;
            UploadCurrentText();

            isSaved = true;
        }

        private void AttendantRequestText_TextChanged(object sender, TextChangedEventArgs e)
        {
            isSaved = false;
            currentRequestText = AttendantRequestText.Text;
        }
    }
}
