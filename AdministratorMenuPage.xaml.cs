using Npgsql;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
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
using static Kursovaya2.AttendantMenu;
using static Kursovaya2.StudentMenuPage;

namespace Kursovaya2
{
    /// <summary>
    /// Логика взаимодействия для AdministratorMenuPage.xaml
    /// </summary>
    public partial class AdministratorMenuPage : Page, IPageN
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

        public void UpdateOnLangChange()
        {
            UpdatePersonalInfo();
        }

        public void OnNavigatingFrom(object sender, NavigatingCancelEventArgs e)
        {
            App.LanguageChanged -= UpdateOnLangChange;
            UnsubscribeMainWindow();
        }

        static readonly string selectAdministratorData = "WITH LoginID AS (SELECT \"ID\" FROM \"People\" WHERE \"Login\" = @login) SELECT \"Administrator ID\", \"Login\",\"Name\", \"Surname\", \"Patronymic\" FROM \"Administrators\", \"People\" WHERE \"ID\" = \"Administrator ID\" AND \"Administrator ID\" = (SELECT * FROM LoginID LIMIT 1);";

        

        public AdministratorMenuPage()
        {
            InitializeComponent();

            App.LanguageChanged += UpdateOnLangChange;

            UpdatePersonalInfo();
        }

        List<PersonalDataT> GetListPersonalDataT(AdministratorInfo inf)
        {

            List<PersonalDataT> ll = new List<PersonalDataT>();

            PersonalDataT p = new PersonalDataT();
            p.PersonalData = inf.Surname;
            p.PersonalDataCat = (string)TryFindResource("m_studentStartPersonalSurname");
            ll.Add(p);

            p = new PersonalDataT();
            p.PersonalData = inf.Name;
            p.PersonalDataCat = (string)TryFindResource("m_studentStartPersonalName");
            ll.Add(p);

            p = new PersonalDataT();
            p.PersonalData = inf.Patronymic;
            p.PersonalDataCat = (string)TryFindResource("m_studentStartPersonalPatronymic");
            ll.Add(p);

            p = new PersonalDataT();
            p.PersonalData = inf.Login;
            p.PersonalDataCat = (string)TryFindResource("m_studentStartPersonalLogin");
            ll.Add(p);

            return ll;
        }

        void UpdatePersonalInfo()
        {
            AdministratorInfo info = new AdministratorInfo();

            using (NpgsqlCommand cmd = new NpgsqlCommand(selectAdministratorData, App.workConnection))
            {
                cmd.Parameters.AddWithValue("@login", App.login);
                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        info.Name = reader["Name"].ToString();
                        info.Surname = reader["Surname"].ToString();
                        info.Patronymic = reader["Patronymic"].ToString();
                        info.Login = reader["Login"].ToString();
                    }
                }
            }

            List<PersonalDataT> infoList = GetListPersonalDataT(info);

            AttendantPersonalDataGrid.ItemsSource = infoList;
        }

        private void AdministratorsResidentsButton_Click(object sender, RoutedEventArgs e)
        {
            SubscribeMainWindow();
            forAdminRedact = UserCategories.Resident;
            NavigationRequested(new Uri("AdministratorRedactingPage.xaml", UriKind.Relative));
        }

        private void AdministratorsAttendantButton_Click(object sender, RoutedEventArgs e)
        {
            SubscribeMainWindow();
            forAdminRedact = UserCategories.Attendant;
            NavigationRequested(new Uri("AdministratorRedactingPage.xaml", UriKind.Relative));
        }

        private void AdministratorsSecurityButton_Click(object sender, RoutedEventArgs e)
        {
            SubscribeMainWindow();
            forAdminRedact = UserCategories.Security;
            NavigationRequested(new Uri("AdministratorRedactingPage.xaml", UriKind.Relative));
        }

        private void AdministratorsAdministratorsButton_Click(object sender, RoutedEventArgs e)
        {
            SubscribeMainWindow();
            forAdminRedact = UserCategories.Administrator;
            NavigationRequested(new Uri("AdministratorRedactingPage.xaml", UriKind.Relative));
        }

        private void AdministratorsRequestButton_Click(object sender, RoutedEventArgs e)
        {
            AdministratorRequestWindow arw = new AdministratorRequestWindow();
            arw.Show();
        }
    }
}
