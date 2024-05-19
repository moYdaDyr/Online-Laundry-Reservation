using Npgsql;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using static Kursovaya2.AllForFilters;
using static Kursovaya2.App;

namespace Kursovaya2
{
    /// <summary>
    /// Логика взаимодействия для AttendantResidentsPage.xaml
    /// </summary>
    public partial class AttendantResidentsPage : Page, IPageN
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
            UpdateResidentsTable();
            BuildFilter();
            AllForFilters.DoFilter<ResidentInf>(residents, AttendantResidentsTable, new PassFunctionResident(), residentFilter, numberOfFilters);
        }

        public void OnNavigatingFrom(object sender, NavigatingCancelEventArgs e)
        {
            App.LanguageChanged -= UpdateOnLangChange;
            UnsubscribeMainWindow();
        }

        static readonly int numberOfFilters = 7;

        FilterList[] residentFilter;

        static readonly string selectAllResidentsInBuilding = "WITH ResidentsInBuilding AS (SELECT \"Resident ID\" FROM \"Residents\" WHERE \"Building\" = @building) SELECT \"Resident ID\", \"Name\", \"Surname\", \"Patronymic\", \"Student card\", \"Room\" FROM \"Residents\", \"People\" WHERE \"Resident ID\" = \"ID\" AND \"Resident ID\" IN (SELECT * FROM ResidentsInBuilding);";
        static readonly string selectAttendantBuildings = "WITH LoginID AS (SELECT \"ID\" FROM \"People\" WHERE \"Login\" = @login LIMIT 1) SELECT * FROM GetAttendantBuildings( (SELECT * FROM LoginID LIMIT 1) );";
        static readonly string selectResidentStatus = "SELECT NOT EXISTS(SELECT \"ID\" FROM \"Blocking_journal\" WHERE \"Person\" = @resident);";
        //static readonly string selectBlockingData = "WITH AttendantPeople AS (SELECT \"Name\", \"Surname\", \"Patronymic\" FROM \"People\" WHERE \"ID\" = @loginID)\r\nSELECT \"Reason\", \"NameRU\", \"Name\", \"Surname\", \"Patronymic\", \"Date\" FROM \"Blocking_journal\", AttendantPeople, \"Blocking_reasons\" WHERE \"Person\" = @resident;";
        static readonly string blockResident = "WITH LoginID AS (SELECT \"ID\" FROM \"People\" WHERE \"Login\" = @login) INSERT INTO \"Blocking_journal\" VALUES (default, @reason, @resident, (SELECT * FROM LoginID LIMIT 1), CURRENT_DATE);";
        static readonly string unblockResident = "DELETE FROM \"Blocking_journal\" WHERE \"ID\" = @blocking;";

        ObservableCollection<ResidentInf> residents;
        ListCollectionView residentsView;

        public AttendantResidentsPage()
        {
            InitializeComponent();

            App.LanguageChanged += UpdateOnLangChange;

            residents = new ObservableCollection<ResidentInf>();

            residentFilter = new FilterList[numberOfFilters];

            for (int i = 0; i < numberOfFilters; i++)
            {
                residentFilter[i] = new FilterList();
            }

            UpdateResidentsTable();

            AttendantResidentsTable.ItemsSource = residents;

            residentsView = new ListCollectionView(residents);

            Loaded += OnLoad;
        }

        public class PassFunctionResident : PassFunctionT
        {
            public override bool Pass<T>(T r, FilterList filter, int index)
            {
                var row = r as ResidentInf;
                string ss = "";
                switch (index)
                {
                    case 0: ss = row.Building; break;
                    case 1: ss = row.Name; break;
                    case 2: ss = row.Surname; break;
                    case 3: ss = row.Patronymic; break;
                    case 4: ss = row.StudentCard; break;
                    case 5: ss = row.Room; break;
                    case 6: ss = row.StatusText; break;
                }
                int pos = filter.Values.IndexOf(ss);
                if (pos == -1) return false;

                return filter.IsEnabled[pos];
            }
        }

        private void AttendantResidentsTableStatusButton_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;

            int index = (int)b.CommandParameter;

            int rr = -1;

            void SetRR(int id) { rr = id; }

            AttendantBlockingReasonWindow ww = new AttendantBlockingReasonWindow(true);
            ww.AttendantBlockingReasonWindowReturnEvent += SetRR;

            ww.ShowDialog();

            if (rr == -1)
            {
                return;
            }

            using (NpgsqlCommand cmd = new NpgsqlCommand(blockResident, App.workConnection))
            {
                cmd.Parameters.AddWithValue("@reason", rr);
                cmd.Parameters.AddWithValue("@resident", index);
                cmd.Parameters.AddWithValue("@login", App.login);

                cmd.ExecuteNonQuery();
            }

            UpdateResidentsTable();
            AllForFilters.DoFilter<ResidentInf>(residents, AttendantResidentsTable, new PassFunctionResident(), residentFilter, numberOfFilters);
        }

        private DataGridColumnHeader GetColumnHeaderFromColumn(DataGridColumn column, DataGrid dg)
        {
            // dataGrid is the name of your DataGrid. In this case Name="dataGrid"
            List<DataGridColumnHeader> columnHeaders = GetVisualChildCollection<DataGridColumnHeader>(dg);
            foreach (DataGridColumnHeader columnHeader in columnHeaders)
            {
                if (columnHeader.Column == column)
                {
                    return columnHeader;
                }
            }
            return null;
        }

        

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            BindingPopupOnButton(AttendantResidentsTable);

            BuildFilter();

            AllForFilters.DoFilter<ResidentInf>(residents, AttendantResidentsTable, new PassFunctionResident(), residentFilter, numberOfFilters);
        }

        private void AttendantResidentsTableUnblockButton_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;

            int index = (int)b.CommandParameter;

            int resIndex = -1;
            for (int i = 0; i < residents.Count; i++)
            {
                if (residents[i].ID == index)
                {
                    resIndex = i;
                    break;
                }
            }

            if (resIndex == -1) return;

            if (residents[resIndex].Status)
            {
                MessageBox.Show((string)TryFindResource("m_attendantUnblockingErrorAlready"), (string)TryFindResource("m_error"),MessageBoxButton.OK,MessageBoxImage.Error);
                return;
            }

            int rr = -1;

            void SetRR(int id) { rr = id; }

            AttendantBlockingReasonWindow ww = new AttendantBlockingReasonWindow(false, index);
            ww.AttendantBlockingReasonWindowReturnEvent += SetRR;

            ww.ShowDialog();

            if (rr == -1)
            {
                return;
            }

            using (NpgsqlCommand cmd = new NpgsqlCommand(unblockResident, App.workConnection))
            {
                cmd.Parameters.AddWithValue("@blocking", rr);

                cmd.ExecuteNonQuery();
            }

            UpdateResidentsTable();

            AllForFilters.DoFilter<ResidentInf>(residents, AttendantResidentsTable, new PassFunctionResident(), residentFilter, numberOfFilters);
        }

        private void UpdateResidentsTable()
        {
            residents.Clear();

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

            for (int i=0;i< BuildingsID.Count; i++)
            {
                using (NpgsqlCommand cmd = new NpgsqlCommand(selectAllResidentsInBuilding, App.workConnection))
                {
                    cmd.Parameters.AddWithValue("@building", BuildingsID[i]);
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ResidentInf info = new ResidentInf();
                            info.ID = (int)reader["Resident ID"];
                            info.Name = reader["Name"].ToString();
                            info.Surname = reader["Surname"].ToString();
                            info.Patronymic = reader["Patronymic"].ToString();
                            info.StudentCard = reader["Student card"].ToString();
                            info.Room = ((short)reader["Room"]).ToString();
                            info.Building = BuildingNames[i];

                            residents.Add(info);
                        }
                    }
                }
            }

            for (int i=0;i< residents.Count; i++)
            {
                using (NpgsqlCommand cmd = new NpgsqlCommand(selectResidentStatus, App.workConnection))
                {
                    cmd.Parameters.AddWithValue("@resident", residents[i].ID);
                    residents[i].Status = ((bool)cmd.ExecuteScalar());
                    residents[i].StatusText = residents[i].Status ? (string)TryFindResource("m_studentStartPersonalStatusUnlocked")
                    : (string)TryFindResource("m_studentStartPersonalStatusBlocked");
                }
            }
        }

        private void UpdateFilter(object sender, RoutedEventArgs e)
        {
            UpdateFilterFunc(sender, residentFilter);

            AllForFilters.DoFilter<ResidentInf>(residents, AttendantResidentsTable, new PassFunctionResident(), residentFilter, numberOfFilters);
        }

        void BuildFilter()
        {
            for (int i=0;i< numberOfFilters; i++)
            {
                residentFilter[i].Values.Clear();
                residentFilter[i].IsEnabled.Clear();
            }

            for (int i = 0; i < residents.Count; i++)
            {
                residentFilter[0].Values.Add(residents[i].Building);
                residentFilter[1].Values.Add(residents[i].Name);
                residentFilter[2].Values.Add(residents[i].Surname);
                residentFilter[3].Values.Add(residents[i].Patronymic);
                residentFilter[4].Values.Add(residents[i].StudentCard);
                residentFilter[5].Values.Add(residents[i].Room);
                //residentFilter[6].Values.Add(residents[i].StatusText);
            }

            for (int i = 0; i < numberOfFilters-1; i++)
            {
                residentFilter[i].Values = (from p in residentFilter[i].Values select p).Distinct().OrderBy(p => p).ToList();

                for (int j=0;j< residentFilter[i].Values.Count; j++)
                {
                    residentFilter[i].IsEnabled.Add(true);
                }
            }

            residentFilter[6].Values.Add((string)TryFindResource("m_studentStartPersonalStatusUnlocked"));
            residentFilter[6].Values.Add((string)TryFindResource("m_studentStartPersonalStatusBlocked"));
            residentFilter[6].IsEnabled.Add(true);
            residentFilter[6].IsEnabled.Add(true);

            SetFilterPopupsOnTable(residentFilter,AttendantResidentsTable, numberOfFilters, UpdateFilter);
        }

        private void AttendantResidentsTableReportkButton_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;

            int id = (int)b.CommandParameter;

            AttendantResidentReportWindow rw = new AttendantResidentReportWindow(id);

            rw.Show();
        }
    }
}
