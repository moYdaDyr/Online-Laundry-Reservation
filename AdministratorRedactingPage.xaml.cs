using Npgsql;
using NpgsqlTypes;
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
using System.Xml.Linq;
using static Kursovaya2.App;
using static Kursovaya2.AttendantResidentsPage;

namespace Kursovaya2
{
    /// <summary>
    /// Логика взаимодействия для AdministratorRedactingPage.xaml
    /// </summary>
    public partial class AdministratorRedactingPage : Page, IPageN
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
            NavigationRequested(new Uri("AdministratorMenuPage.xaml", UriKind.Relative));
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
            UpdateData();
        }

        public void OnNavigatingFrom(object sender, NavigatingCancelEventArgs e)
        {
            App.LanguageChanged -= UpdateOnLangChange;
            UnsubscribeMainWindow();
        }

        static readonly string selectAllBuildings = "SELECT \"ID\", \"{0}\" AS \"Name\" FROM \"Buildings\";";
        static readonly string selectBuildingsTwoLanguages = "SELECT \"ID\", \"NameRU\", \"NameEN\" FROM \"Buildings\";";
        static readonly string selectAllRooms = "SELECT DISTINCT * FROM GetAllRooms() ORDER BY room ASC;";
        static readonly string selectResidents = "SELECT * FROM \"People\", \"Residents\" WHERE \"ID\" = \"Resident ID\";";
        static readonly string selectAttendants = "SELECT * FROM \"People\", \"Attendants\" WHERE \"ID\" = \"Attendant ID\";";
        static readonly string selectAdministrators = "SELECT * FROM \"People\", \"Administrators\" WHERE \"ID\" = \"Administrator ID\";";
        static readonly string selectResidentStatus = "SELECT NOT EXISTS(SELECT \"ID\" FROM \"Blocking_journal\" WHERE \"Person\" = @resident);";

        static readonly string deletePerson = "DELETE FROM \"People\" WHERE \"ID\" = @id";
        static readonly string deleteBuilding = "DELETE FROM \"Buildings\" WHERE \"ID\" = @id";

        static readonly string updatePerson = "UPDATE \"People\" SET \"Name\" = @name, \"Surname\" = @surname, \"Patronymic\" = @patronymic WHERE \"ID\" = @id;";
        static readonly string updateResident = "UPDATE \"Residents\" SET \"Building\" = @buildingID, \"Student card\" = @card, \"Room\" = @room WHERE \"ID\" = @id;";
        static readonly string updateAttendant = "UPDATE \"Attendant\" SET \"Buildings\" = @buildings WHERE \"ID\" = @id;";
        static readonly string updateSecurity = "UPDATE \"Building\" SET \"NameRU\" = @nameru, \"NameEN\" = @nameen WHERE \"ID\" = @id;";

        UserCategories mode;

        AllForFilters.FilterList mainFilterList;
        //ObservableCollection<object> objects;

        ObservableCollection<ForAdminInfo.AdmResidentInfo> residents;
        ObservableCollection<ForAdminInfo.AdmAttendantInfo> attendants;
        ObservableCollection<ForAdminInfo.AdmSecurityInfo> securities;
        ObservableCollection<ForAdminInfo.AdmAdministratorInfo> administrators;

        List<short> allRooms;
        List<string> allBuildingsNames;
        List<int> allBuildingsID;
        string[] statusTexts;

        /*
        ObservableCollection<T> CastToType<T>()
        {
            ObservableCollection<T> result = (ObservableCollection<T>)(from p in objects select (T)p);
            return result;
        }
        */

        private void AdministratorAttendantButton_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;

            int id = (int)b.CommandParameter;

            int rr = -1;


            AttendantBlockingReasonWindow ww = new AttendantBlockingReasonWindow(true);
            

        }

        private DataGridTemplateColumn CreateComboColumn(string name, List<string> values)
        {
            DataGridTemplateColumn col = new DataGridTemplateColumn();

            Binding b1 = new Binding(name);
            b1.Mode = BindingMode.OneWay;
            Binding b2 = new Binding(name);
            b2.Mode = BindingMode.TwoWay;

            FrameworkElementFactory textFactory = new FrameworkElementFactory(typeof(TextBlock));
            textFactory.SetBinding(TextBlock.TextProperty, b1);

            FrameworkElementFactory comboBoxFactory = new FrameworkElementFactory(typeof(ComboBox));
            comboBoxFactory.SetValue(ComboBox.ItemsSourceProperty, values);
            comboBoxFactory.SetBinding(ComboBox.SelectedItemProperty, b2);

            DataTemplate textTemplate = new DataTemplate();
            textTemplate.VisualTree = textFactory;
            DataTemplate comboTemplate = new DataTemplate();
            comboTemplate.VisualTree = comboBoxFactory;

            col.CellTemplate = textTemplate;
            col.CellEditingTemplate = comboTemplate;

            return col;
        }

        void UpdateData()
        {
            AdministratorTable.CanUserAddRows = false;

            allBuildingsID = new List<int>();
            allBuildingsNames = new List<string>();
            allRooms = new List<short>();

            TextBlock tb;

            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(selectAllBuildings, GetLocalisedKey("Name")), App.workConnection))
            {
                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var v1 = (int)reader["ID"];
                        var v2 = reader["Name"].ToString();

                        allBuildingsID.Add(v1);
                        allBuildingsNames.Add(v2);
                    }
                }
            }

            

            if (mode == UserCategories.Resident)
            {
                residents = new ObservableCollection<ForAdminInfo.AdmResidentInfo>();

                using (NpgsqlCommand cmd = new NpgsqlCommand(selectAllRooms, App.workConnection))
                {
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var v1 = (short)reader["room"];
                            allRooms.Add(v1);
                        }
                    }
                }

                statusTexts = new string[2];
                statusTexts[0] = (string)TryFindResource("m_studentStartPersonalStatusUnlocked");
                statusTexts[1] = (string)TryFindResource("m_studentStartPersonalStatusBlocked");

                DataGridTextColumn id = new DataGridTextColumn();
                tb = new TextBlock();
                tb.SetResourceReference(TextBlock.TextProperty, "m_AdministratorID");
                id.Header = tb;
                id.Binding = new Binding("ID");
                id.IsReadOnly = true;
                AdministratorTable.Columns.Add(id);

                DataGridTextColumn login = new DataGridTextColumn();
                login.Binding = new Binding("Login");
                tb = new TextBlock();
                tb.SetResourceReference(TextBlock.TextProperty, "m_AdministratorLogin");
                login.Header = tb;
                login.IsReadOnly = true;
                AdministratorTable.Columns.Add(login);

                DataGridTemplateColumn building = CreateComboColumn("Building", allBuildingsNames);
                tb = new TextBlock();
                tb.SetResourceReference(TextBlock.TextProperty, "m_AttendantResidentsBuildingHeader");
                building.Header = tb;
                AdministratorTable.Columns.Add(building);

                DataGridTextColumn name = new DataGridTextColumn();
                name.Binding = new Binding("Name");
                tb = new TextBlock();
                tb.SetResourceReference(TextBlock.TextProperty, "m_AttendantResidentsNameHeader");
                name.Header = tb;
                name.HeaderStyle = this.FindResource("DataGridFilterHeaderStyle") as Style;
                AdministratorTable.Columns.Add(name);

                DataGridTextColumn surname = new DataGridTextColumn();
                surname.Binding = new Binding("Surname");
                tb = new TextBlock();
                tb.SetResourceReference(TextBlock.TextProperty, "m_AttendantResidentsSurnameHeader");
                surname.Header = tb;
                surname.HeaderStyle = this.FindResource("DataGridFilterHeaderStyle") as Style;
                AdministratorTable.Columns.Add(surname);

                DataGridTextColumn patr = new DataGridTextColumn();
                patr.Binding = new Binding("Patronymic");
                tb = new TextBlock();
                tb.SetResourceReference(TextBlock.TextProperty, "m_AttendantResidentsPatronymicHeader");
                patr.Header = tb;
                patr.HeaderStyle = this.FindResource("DataGridFilterHeaderStyle") as Style;
                AdministratorTable.Columns.Add(patr);

                DataGridComboBoxColumn rooms = new DataGridComboBoxColumn();
                rooms.ItemsSource = allRooms;
                Binding b1 = new Binding("Room");
                tb = new TextBlock();
                tb.SetResourceReference(TextBlock.TextProperty, "m_AttendantResidentsRoomHeader");
                rooms.Header = tb;
                b1.Mode = BindingMode.TwoWay;
                rooms.SelectedItemBinding = b1;
                rooms.HeaderStyle = this.FindResource("DataGridFilterHeaderStyle") as Style;
                AdministratorTable.Columns.Add(rooms);

                DataGridTextColumn status = new DataGridTextColumn();
                status.Binding = new Binding("StatusText");
                tb = new TextBlock();
                tb.SetResourceReference(TextBlock.TextProperty, "m_AttendantResidentsStatusHeader");
                status.Header = tb;
                status.HeaderStyle = this.FindResource("DataGridFilterHeaderStyle") as Style;
                AdministratorTable.Columns.Add(status);

                DataGridTextColumn card = new DataGridTextColumn();
                card.Binding = new Binding("StudentCard");
                tb = new TextBlock();
                tb.SetResourceReference(TextBlock.TextProperty, "m_AttendantResidentsStudentCardHeader");
                card.Header = tb;
                card.HeaderStyle = this.FindResource("DataGridFilterHeaderStyle") as Style;
                AdministratorTable.Columns.Add(card);

                using (NpgsqlCommand cmd = new NpgsqlCommand(selectResidents, App.workConnection))
                {
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ForAdminInfo.AdmResidentInfo res = new ForAdminInfo.AdmResidentInfo();
                            res.Room = ((short)reader["Room"]).ToString();
                            res.StudentCard = (string)reader["Student card"];
                            res.Patronymic = (string)reader["Patronymic"];
                            res.ID = (int)reader["ID"];
                            res.Name = (string)reader["Name"];
                            res.Surname = (string)reader["Surname"];
                            res.Login = (string)reader["Login"];

                            residents.Add(res);
                        }
                    }
                }

                for (int i = 0; i < residents.Count; i++)
                {
                    using (NpgsqlCommand cmd = new NpgsqlCommand(selectResidentStatus, App.workConnection))
                    {
                        cmd.Parameters.AddWithValue("@resident", residents[i].ID);

                        residents[i].StatusText = (bool)cmd.ExecuteScalar() ? (string)TryFindResource("m_studentStartPersonalStatusUnlocked")
                        : (string)TryFindResource("m_studentStartPersonalStatusBlocked");
                    }
                }

                AdministratorTable.ItemsSource = residents;
            }
            else if (mode == UserCategories.Attendant)
            {
                attendants = new ObservableCollection<ForAdminInfo.AdmAttendantInfo>();

                DataGridTextColumn id = new DataGridTextColumn();
                tb = new TextBlock();
                tb.SetResourceReference(TextBlock.TextProperty, "m_AdministratorID");
                id.Header = tb;
                id.Binding = new Binding("ID");
                id.IsReadOnly = true;

                AdministratorTable.Columns.Add(id);

                DataGridTextColumn login = new DataGridTextColumn();
                login.Binding = new Binding("Login");
                tb = new TextBlock();
                tb.SetResourceReference(TextBlock.TextProperty, "m_AdministratorLogin");
                login.Header = tb;
                login.IsReadOnly = true;
                AdministratorTable.Columns.Add(login);

                DataGridTextColumn name = new DataGridTextColumn();
                name.Binding = new Binding("Name");
                tb = new TextBlock();
                tb.SetResourceReference(TextBlock.TextProperty, "m_AttendantResidentsBuildingHeader");
                name.Header = tb;
                name.HeaderStyle = this.FindResource("DataGridFilterHeaderStyle") as Style;
                AdministratorTable.Columns.Add(name);

                DataGridTextColumn surname = new DataGridTextColumn();
                surname.Binding = new Binding("Surname");
                tb = new TextBlock();
                tb.SetResourceReference(TextBlock.TextProperty, "m_AttendantResidentsSurnameHeader");
                surname.Header = tb;
                surname.HeaderStyle = this.FindResource("DataGridFilterHeaderStyle") as Style;
                AdministratorTable.Columns.Add(surname);

                DataGridTextColumn patr = new DataGridTextColumn();
                patr.Binding = new Binding("Patronymic");
                tb = new TextBlock();
                tb.SetResourceReference(TextBlock.TextProperty, "m_AttendantResidentsPatronymicHeader");
                patr.Header = tb;
                patr.HeaderStyle = this.FindResource("DataGridFilterHeaderStyle") as Style;
                AdministratorTable.Columns.Add(patr);

                DataGridTemplateColumn buildings = new DataGridTemplateColumn();
                DataTemplate dt = new DataTemplate();
                FrameworkElementFactory buttonFactory = new FrameworkElementFactory(typeof(Button));
                TextBox tb2 = new TextBox();
                tb2.SetResourceReference(TextBox.TextProperty, "m_AdministratorAttendantBuildingButton");
                //buttonFactory.SetValue(Button.ContentProperty, tb2);
                buttonFactory.SetValue(Button.CommandParameterProperty, "ID");
                buttonFactory.AddHandler(Button.ClickEvent, new RoutedEventHandler(AdministratorAttendantButton_Click));
                tb = new TextBlock();
                tb.SetResourceReference(TextBlock.TextProperty, "m_attendantStartPersonalBuilding");
                buildings.Header = tb;
                dt.VisualTree = buttonFactory;
                AdministratorTable.Columns.Add(buildings);

                using (NpgsqlCommand cmd = new NpgsqlCommand(selectAttendants, App.workConnection))
                {
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ForAdminInfo.AdmAttendantInfo att = new ForAdminInfo.AdmAttendantInfo();
                            att.Patronymic = (string)reader["Patronymic"];
                            att.ID = (int)reader["ID"];
                            att.Name = (string)reader["Name"];
                            att.Surname = (string)reader["Surname"];
                            att.Login = (string)reader["Login"];
                            //att.buildings = (NpgsqlRange<int>)reader["Buildings"];

                            attendants.Add(att);
                        }
                    }
                }

                AdministratorTable.ItemsSource = attendants;
            }
            else if (mode == UserCategories.Security)
            {
                securities = new ObservableCollection<ForAdminInfo.AdmSecurityInfo>();
                
                DataGridTextColumn id = new DataGridTextColumn();
                id.Binding = new Binding("ID");
                id.IsReadOnly = true;
                tb = new TextBlock();
                tb.SetResourceReference(TextBlock.TextProperty, "m_AdministratorID");
                id.Header = tb;
                AdministratorTable.Columns.Add(id);

                DataGridTextColumn login = new DataGridTextColumn();
                login.Binding = new Binding("Login");
                login.IsReadOnly = true;
                tb = new TextBlock();
                tb.SetResourceReference(TextBlock.TextProperty, "m_AdministratorLogin");
                login.Header = tb;
                AdministratorTable.Columns.Add(login);

                DataGridTextColumn nameru = new DataGridTextColumn();
                nameru.Binding = new Binding("NameRU");
                tb = new TextBlock();
                tb.SetResourceReference(TextBlock.TextProperty, "m_AdministratorNameRU");
                nameru.Header = tb;
                AdministratorTable.Columns.Add(nameru);

                DataGridTextColumn nameen = new DataGridTextColumn();
                nameen.Binding = new Binding("NameEN");
                tb = new TextBlock();
                tb.SetResourceReference(TextBlock.TextProperty, "m_AdministratorNameEN");
                nameen.Header = tb;
                AdministratorTable.Columns.Add(nameen);

                using (NpgsqlCommand cmd = new NpgsqlCommand(selectBuildingsTwoLanguages, App.workConnection))
                {
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ForAdminInfo.AdmSecurityInfo sec = new ForAdminInfo.AdmSecurityInfo();
                            sec.NameRU = (string)reader["NameRU"];
                            sec.NameEN = (string)reader["NameEN"];
                            sec.ID = (int)reader["ID"];
                            sec.Login = App.GetSecurityLogin(sec.ID.ToString());

                            securities.Add(sec);
                        }
                    }
                }

                AdministratorTable.ItemsSource = securities;
            }
            else
            {
                administrators = new ObservableCollection<ForAdminInfo.AdmAdministratorInfo>();

                DataGridTextColumn id = new DataGridTextColumn();
                id.Binding = new Binding("ID");
                id.IsReadOnly = true;
                AdministratorTable.Columns.Add(id);

                DataGridTextColumn login = new DataGridTextColumn();
                login.Binding = new Binding("Login");
                login.IsReadOnly = true;
                AdministratorTable.Columns.Add(login);

                DataGridTextColumn name = new DataGridTextColumn();
                name.Binding = new Binding("Name");
                name.HeaderStyle = this.FindResource("DataGridFilterHeaderStyle") as Style;
                AdministratorTable.Columns.Add(name);

                DataGridTextColumn surname = new DataGridTextColumn();
                surname.Binding = new Binding("Surname");
                surname.HeaderStyle = this.FindResource("DataGridFilterHeaderStyle") as Style;
                AdministratorTable.Columns.Add(surname);

                DataGridTextColumn patr = new DataGridTextColumn();
                patr.Binding = new Binding("Patronymic");
                patr.HeaderStyle = this.FindResource("DataGridFilterHeaderStyle") as Style;
                AdministratorTable.Columns.Add(patr);

                using (NpgsqlCommand cmd = new NpgsqlCommand(selectAdministrators, App.workConnection))
                {
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ForAdminInfo.AdmAdministratorInfo adm = new ForAdminInfo.AdmAdministratorInfo();
                            adm.Patronymic = reader["Patronymic"].ToString();
                            adm.ID = (int)reader["ID"];
                            adm.Name = (string)reader["Name"];
                            adm.Surname = (string)reader["Surname"];
                            adm.Login = (string)reader["Login"];

                            administrators.Add(adm);
                        }
                    }
                }

                AdministratorTable.ItemsSource = administrators;
            }
        }

        public AdministratorRedactingPage()
        {
            InitializeComponent();

            mainFilterList = new AllForFilters.FilterList();

            mode = App.forAdminRedact;

            UpdateData();
        }

        private void AdministratorChangePassword_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            int id = (int)b.CommandParameter;

            string log;
            int index = -1;
            log = "";
            
            if (mode == UserCategories.Resident)
            {
                index = FindPositionByID<ForAdminInfo.AdmResidentInfo>(residents, id);
                if (index == -1) return;

                log = residents[index].Login;
            }
            else if (mode == UserCategories.Attendant)
            {
                index = FindPositionByID<ForAdminInfo.AdmAttendantInfo>(attendants, id);
                if (index == -1) return;

                log = attendants[index].Login;
            }
            else if (mode == UserCategories.Security)
            {
                index = FindPositionByID<ForAdminInfo.AdmSecurityInfo>(securities, id);
                if (index == -1) return;

                log = securities[index].Login;
            }
            else
            {
                index = FindPositionByID<ForAdminInfo.AdmAdministratorInfo>(administrators, id);
                if (index == -1) return;

                log = administrators[index].Login;
            }
            AdministratorChangePasswordWindow aw = new AdministratorChangePasswordWindow(log);
            
        }

        private void AdministratorDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            int id = (int)b.CommandParameter;

            var result = MessageBox.Show((string)TryFindResource("m_AdministratorTableDelete"), (string)TryFindResource("m_confirmatioRequired"), MessageBoxButton.YesNo, MessageBoxImage.Question);

            
            if (result == MessageBoxResult.No) return;

            int index = -1;

            if (mode == UserCategories.Resident)
            {
                index = FindPositionByID<ForAdminInfo.AdmResidentInfo>(residents, id);
                if (index == -1) return;

                residents.RemoveAt(index);

                using (NpgsqlCommand cmd = new NpgsqlCommand(deletePerson, App.workConnection))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
            else if (mode == UserCategories.Attendant)
            {
                index = FindPositionByID<ForAdminInfo.AdmAttendantInfo>(attendants, id);
                if (index == -1) return;

                attendants.RemoveAt(index);

                using (NpgsqlCommand cmd = new NpgsqlCommand(deletePerson, App.workConnection))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
            else if (mode == UserCategories.Security)
            {
                index = FindPositionByID<ForAdminInfo.AdmSecurityInfo>(securities, id);
                if (index == -1) return;

                securities.RemoveAt(index);

                using (NpgsqlCommand cmd = new NpgsqlCommand(deleteBuilding, App.workConnection))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
            else
            {
                index = FindPositionByID<ForAdminInfo.AdmAdministratorInfo>(administrators, id);
                if (index == -1) return;

                administrators.RemoveAt(index);

                using (NpgsqlCommand cmd = new NpgsqlCommand(deletePerson, App.workConnection))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        int FindPositionByID<T>(ObservableCollection<T> list, int id) where T : ForAdminInfo.IAdminInfo
        {
            int result = -1;

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].ID == id)
                {
                    result = i;
                    return i;
                }
            }
            return -1;
        }

        private void AdministratorSaveDataButton_Click(object sender, RoutedEventArgs e)
        {
            if (mode == UserCategories.Resident)
            {
                //index = FindPositionByID<ForAdminInfo.AdmResidentInfo>(residents, id);
                //if (index == -1) return;

                for (int i = 0; i < residents.Count; i++)
                {
                    using (NpgsqlCommand cmd = new NpgsqlCommand(updatePerson, App.workConnection))
                    {
                        cmd.Parameters.AddWithValue("@name", residents[i].Name);
                        cmd.Parameters.AddWithValue("@surname", residents[i].Surname);
                        cmd.Parameters.AddWithValue("@patronymics", residents[i].Patronymic);
                        cmd.Parameters.AddWithValue("@id", residents[i].ID);

                        cmd.ExecuteNonQuery();
                    }

                    int buildingID = allBuildingsID[allBuildingsNames.IndexOf(residents[i].Building)];

                    using (NpgsqlCommand cmd = new NpgsqlCommand(updateResident, App.workConnection))
                    {
                        cmd.Parameters.AddWithValue("@card", residents[i].StudentCard);
                        cmd.Parameters.AddWithValue("@room", short.Parse(residents[i].Room));
                        cmd.Parameters.AddWithValue("@buildingID", buildingID);
                        cmd.Parameters.AddWithValue("@id", residents[i].ID);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            else if (mode == UserCategories.Attendant)
            {
                //index = FindPositionByID<ForAdminInfo.AdmAttendantInfo>(attendants, id);
                //if (index == -1) return;

                for (int i = 0; i < attendants.Count; i++)
                {
                    using (NpgsqlCommand cmd = new NpgsqlCommand(updatePerson, App.workConnection))
                    {
                        cmd.Parameters.AddWithValue("@name", attendants[i].Name);
                        cmd.Parameters.AddWithValue("@surname", attendants[i].Surname);
                        cmd.Parameters.AddWithValue("@patronymics", attendants[i].Patronymic);
                        cmd.Parameters.AddWithValue("@id", attendants[i].ID);

                        cmd.ExecuteNonQuery();
                    }

                    using (NpgsqlCommand cmd = new NpgsqlCommand(updateAttendant, App.workConnection))
                    {
                        cmd.Parameters.AddWithValue("@buildings", attendants[i].buildings);
                        cmd.Parameters.AddWithValue("@id", residents[i].ID);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            else if (mode == UserCategories.Security)
            {
                //index = FindPositionByID<ForAdminInfo.AdmSecurityInfo>(securities, id);
                //if (index == -1) return;

                for (int i = 0; i < securities.Count; i++)
                {
                    using (NpgsqlCommand cmd = new NpgsqlCommand(updateSecurity, App.workConnection))
                    {
                        cmd.Parameters.AddWithValue("@nameru", securities[i].NameRU);
                        cmd.Parameters.AddWithValue("@nameen", securities[i].NameEN);
                        cmd.Parameters.AddWithValue("@id", securities[i].ID);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            else
            {
                //index = FindPositionByID<ForAdminInfo.AdmAdministratorInfo>(administrators, id);
                //if (index == -1) return;

                for (int i = 0; i < administrators.Count; i++)
                {
                    using (NpgsqlCommand cmd = new NpgsqlCommand(updatePerson, App.workConnection))
                    {
                        cmd.Parameters.AddWithValue("@name", administrators[i].Name);
                        cmd.Parameters.AddWithValue("@surname", administrators[i].Surname);
                        cmd.Parameters.AddWithValue("@patronymics", administrators[i].Patronymic);
                        cmd.Parameters.AddWithValue("@id", administrators[i].ID);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        private void AdministratorAddNewDataButton_Click(object sender, RoutedEventArgs e)
        {
            AdministratorAddWindow aw = new AdministratorAddWindow(mode);
            aw.ShowDialog();

            UpdateData();
        }
    }
}
