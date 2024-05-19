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
using System.Windows.Navigation;
using System.Windows.Shapes;
using static Kursovaya2.App;
using static Kursovaya2.StudentMenuPage;

namespace Kursovaya2
{
    /// <summary>
    /// Логика взаимодействия для AttendantMenu.xaml
    /// </summary>
    public partial class AttendantMenu : Page, IPageN
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

        static readonly string selectAttendantPersonalData = "WITH LoginID AS (SELECT \"ID\" FROM \"People\" WHERE \"Login\" = @login) SELECT \"Attendant ID\", \"Login\", \"Name\", \"Surname\", \"Patronymic\" FROM \"Attendants\", \"People\" WHERE \"ID\" = \"Attendant ID\" AND \"Attendant ID\" = (SELECT * FROM LoginID LIMIT 1);";
        static readonly string selectAttendantBuildings = "WITH LoginID AS (SELECT \"ID\" FROM \"People\" WHERE \"Login\" = @login) SELECT * FROM GetAttendantBuildings( (SELECT * FROM LoginID LIMIT 1) );";

        public AttendantMenu()
        {
            InitializeComponent();

            UpdatePersonalInfo();

            App.LanguageChanged += UpdateOnLangChange;
        }

        public List<PersonalDataT> GetListPersonalDataT(AttendantInf inf)
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

            p = new PersonalDataT();
            string s="";

            for (int i = 0; i < inf.BuildingNames.Count; i++)
            {
                s += inf.BuildingNames[i] + "; ";
            }

            p.PersonalData = s;
            p.PersonalDataCat = (string)TryFindResource("m_attendantStartPersonalBuilding");
            ll.Add(p);

            return ll;
        }

        void UpdatePersonalInfo()
        {
            AttendantInf info = new AttendantInf();

            using (NpgsqlCommand cmd = new NpgsqlCommand(selectAttendantPersonalData, App.workConnection))
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

            info.BuildingsID = new List<int>();
            info.BuildingNames = new List<string>();

            using (NpgsqlCommand cmd = new NpgsqlCommand(selectAttendantBuildings, App.workConnection))
            {
                cmd.Parameters.AddWithValue("@login", App.login);
                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var id = (int)reader["building_id"];
                        var name = reader[App.GetLocalisedKey("Name")].ToString();

                        info.BuildingsID.Add(id);
                        info.BuildingNames.Add(name);
                    }
                }
            }

            List<PersonalDataT> infoList = GetListPersonalDataT(info);

            //PersonalDataGrid.Items.Clear();
            AttendantPersonalDataGrid.ItemsSource = infoList;
        }

        private void AttendantResidentsButton_Click(object sender, RoutedEventArgs e)
        {
            SubscribeMainWindow();

            NavigationRequested(new Uri("AttendantResidentsPage.xaml", UriKind.Relative));
        }

        private void AttendantLaundriesButton_Click(object sender, RoutedEventArgs e)
        {
            SubscribeMainWindow();

            NavigationRequested(new Uri("AttendantLaundriesPage.xaml", UriKind.Relative));
        }

        private void AttendantRequestsButton_Click(object sender, RoutedEventArgs e)
        {
            SubscribeMainWindow();

            NavigationRequested(new Uri("AttendantRequestPage.xaml", UriKind.Relative));
        }

    }
}
