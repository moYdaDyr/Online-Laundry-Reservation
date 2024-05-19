using Npgsql;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
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
using static Kursovaya2.AllForFilters;
using static Kursovaya2.App;
using static Kursovaya2.AttendantResidentsPage;

namespace Kursovaya2
{
    /// <summary>
    /// Логика взаимодействия для AttendantLaundriesPage.xaml
    /// </summary>
    public partial class AttendantLaundriesPage : Page, IPageN
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
        public void OnNavigatingFrom(object sender, NavigatingCancelEventArgs e)
        {
            App.LanguageChanged -= UpdateOnLangChange;
            UnsubscribeMainWindow();
        }

        public void UpdateOnLangChange()
        {
            UpdateData();
            BuildFilter();
            DoFilter<LaundryInfo>(laundries, AttendantLaundriesTable, new PassFunctionLaundry(), laundryFilter, numberOfFilters);
        }

        public bool IsStart
        {
            get
            {
                return false;
            }
        }
        public void ReturnBack()
        {
            SubscribeMainWindow();
            NavigationRequested(new Uri("AttendantMenuPage.xaml", UriKind.Relative));
            return;
        }

        static readonly string selectAllLaundriesInBuilding = "SELECT \"Laundry ID\", \"Work_status\", \"Booking_slot_limit\", \"{0}\" AS \"Name\" FROM \"Laundries\" WHERE \"Building\" = @building;";
        static readonly string selectAttendantBuildings = "WITH LoginID AS (SELECT \"ID\" FROM \"People\" WHERE \"Login\" = @login) SELECT * FROM GetAttendantBuildings( (SELECT * FROM LoginID LIMIT 1) );";
        static readonly string updateLaundryState = "UPDATE \"Laundries\" SET \"Work_status\" = @status WHERE \"Laundry ID\" = @laundry;";

        class LaundryInfo
        {
            public int ID { get; set; }
            public bool Status { get; set; }


            public string Building { get; set; }
            public string Name { get; set; }
            public string Limit { get; set; }
            public string StatusText { get; set; }
        }

        ObservableCollection<LaundryInfo> laundries;
        static readonly int numberOfFilters = 4;
        FilterList[] laundryFilter;

        public class PassFunctionLaundry : PassFunctionT
        {
            public override bool Pass<T>(T r, FilterList filter, int index)
            {
                var row = r as LaundryInfo;
                string ss = "";
                switch (index)
                {
                    case 0: ss = row.Building; break;
                    case 1: ss = row.Name; break;
                    case 2: ss = row.Limit; break;
                    case 3: ss = row.StatusText; break;
                }
                int pos = filter.Values.IndexOf(ss);

                //MessageBox.Show(ss + " " + pos + " " + filter.IsEnabled[pos]);

                if (pos == -1) return false;
                return filter.IsEnabled[pos];
            }
        }

        public AttendantLaundriesPage()
        {
            InitializeComponent();

            laundries = new ObservableCollection<LaundryInfo>();
            laundryFilter = new FilterList[numberOfFilters];

            for (int i = 0; i < numberOfFilters; i++)
            {
                laundryFilter[i] = new FilterList();
            }

            UpdateData();

            AttendantLaundriesTable.ItemsSource = laundries;

            Loaded += OnLoad;
            App.LanguageChanged += UpdateOnLangChange;
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            BindingPopupOnButton(AttendantLaundriesTable);

            BuildFilter();

            DoFilter<LaundryInfo>(laundries, AttendantLaundriesTable, new PassFunctionLaundry(), laundryFilter, numberOfFilters);
        }

        void UpdateData()
        {
            laundries.Clear();

            List<int> BuildingsID = new List<int>();
            List<string> BuildingNames = new List<string>();

            using (NpgsqlCommand cmd = new NpgsqlCommand(selectAttendantBuildings, App.workConnection))
            {
                cmd.Parameters.AddWithValue("@login", App.login);
                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var id = (int)reader["building_id"];
                        var name = reader[GetLocalisedKey("name")].ToString();

                        BuildingsID.Add(id);
                        BuildingNames.Add(name);
                    }
                }
            }

            for (int i = 0; i < BuildingsID.Count; i++)
            {
                using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(selectAllLaundriesInBuilding, GetLocalisedKey("Name")), App.workConnection))
                {
                    cmd.Parameters.AddWithValue("@building", BuildingsID[i]);
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            LaundryInfo info = new LaundryInfo();
                            info.ID = (int)reader["Laundry ID"];
                            info.Name = reader["Name"].ToString();
                            info.Limit = App.GetTimeFromSlots((short)reader["Booking_slot_limit"], false);
                            info.Building = BuildingNames[i];
                            info.Status = (bool)reader["Work_status"];
                            info.StatusText = info.Status ? (string)TryFindResource("m_laundriesStatusOpen")
                    : (string)TryFindResource("m_laundriesStatusClosed");

                            laundries.Add(info);
                        }
                    }
                }
            }
        }

        private void UpdateFilter(object sender, RoutedEventArgs e)
        {
            UpdateFilterFunc(sender, laundryFilter);

            DoFilter<LaundryInfo>(laundries, AttendantLaundriesTable, new PassFunctionLaundry(), laundryFilter, numberOfFilters);
        }

        void BuildFilter()
        {
            for (int i = 0; i < numberOfFilters; i++)
            {
                laundryFilter[i].Values.Clear();
                laundryFilter[i].IsEnabled.Clear();
            }

            for (int i = 0; i < laundries.Count; i++)
            {
                laundryFilter[0].Values.Add(laundries[i].Building);
                laundryFilter[1].Values.Add(laundries[i].Name);
                laundryFilter[2].Values.Add(laundries[i].Limit);
                //laundryFilter[3].Values.Add(laundries[i].StatusText);
            }

            for (int i = 0; i < numberOfFilters - 1; i++)
            {
                laundryFilter[i].Values = (from p in laundryFilter[i].Values select p).Distinct().OrderBy(p => p).ToList();

                for (int j = 0; j < laundryFilter[i].Values.Count; j++)
                {
                    laundryFilter[i].IsEnabled.Add(true);
                }
            }

            laundryFilter[3].Values.Add((string)TryFindResource("m_laundriesStatusOpen"));
            laundryFilter[3].Values.Add((string)TryFindResource("m_laundriesStatusClosed"));
            laundryFilter[3].IsEnabled.Add(true);
            laundryFilter[3].IsEnabled.Add(true);

            SetFilterPopupsOnTable(laundryFilter, AttendantLaundriesTable, numberOfFilters, UpdateFilter);
        }

        private void AttendantResidentsTableStatusButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show((string)TryFindResource("m_attendantLaundriesStatusQuestion"), (string)TryFindResource("m_needChoose"), MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Cancel);

            if (result == MessageBoxResult.Cancel || result == MessageBoxResult.None) return;

            bool status = (result == MessageBoxResult.Yes);

            Button b = sender as Button;

            int id = (int)b.CommandParameter;

            int resIndex = -1;
            for (int i = 0; i < laundries.Count; i++)
            {
                if (laundries[i].ID == id)
                {
                    resIndex = i;
                    break;
                }
            }

            if (resIndex == -1) return;

            if (status == laundries[resIndex].Status) return;

            using (NpgsqlCommand cmd = new NpgsqlCommand(updateLaundryState, App.workConnection))
            {
                cmd.Parameters.AddWithValue("@laundry", id);
                cmd.Parameters.AddWithValue("@status", status);

                cmd.ExecuteNonQuery();
            }

            laundries[resIndex].Status = status;
            laundries[resIndex].StatusText = status ? (string)TryFindResource("m_laundriesStatusOpen")
                    : (string)TryFindResource("m_laundriesStatusClosed");
        }
    }
}
