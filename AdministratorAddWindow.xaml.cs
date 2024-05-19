using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace Kursovaya2
{
    /// <summary>
    /// Логика взаимодействия для AdministratorAddWindow.xaml
    /// </summary>
    public partial class AdministratorAddWindow : Window
    {
        static readonly string selectAllRooms = "SELECT DISTINCT * FROM GetAllRooms() ORDER BY room ASC;";
        static readonly string selectAllBuildings = "SELECT \"ID\", \"{0}\" AS \"Name\" FROM \"Buildings\";";

        static readonly string insertPerson = "INSERT INTO \"People\" VALUES(default, @name, @surname, @patronymic, @login) RETURNING \"ID\";";
        static readonly string insertResident = "INSERT INTO \"Residents\" VALUES(@id, @building, @card, @room);";
        static readonly string insertResidentUser = "CREATE USER @login WITH PASSWORD @password IN ROLE 'Resident';";
        static readonly string insertAttendant = "INSERT INTO \"Attendants\" VALUES(@id) ;";
        static readonly string insertAttendantBuilding = "INSERT INTO \"Attendant_building\" VALUES(default, @id, @building);";
        static readonly string insertAttendantUser = "CREATE USER @login WITH PASSWORD @password IN ROLE 'Attendant';";
        static readonly string insertSecurity = "INSERT INTO \"Building\" VALUES( default, @nameru, @nameen) RETURNING \"ID\";";
        static readonly string insertSecurityUser = "CREATE USER @login WITH PASSWORD @password IN ROLE 'Security';";
        static readonly string insertAdministrator = "INSERT INTO \"Administrators\" VALUES(@id) ;";
        static readonly string insertAdministratorUser = "CREATE USER @login WITH PASSWORD @password IN ROLE 'Administrator';";

        ObservableCollection<ForAdminInfo.AdmResidentInfo> residents;
        ObservableCollection<ForAdminInfo.AdmAttendantInfo> attendants;
        ObservableCollection<ForAdminInfo.AdmSecurityInfo> securities;
        ObservableCollection<ForAdminInfo.AdmAdministratorInfo> administrators;

        List<short> allRooms;
        List<string> allBuildingsNames;
        List<int> allBuildingsID;
        string[] statusTexts;

        UserCategories mode;
        public AdministratorAddWindow(UserCategories uc)
        {
            InitializeComponent();

            mode = uc;

            UpdateData();
        }

        void UpdateData()
        {
            allBuildingsID = new List<int>();
            allBuildingsNames = new List<string>();
            allRooms = new List<short>();

            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(selectAllBuildings, App.GetLocalisedKey("Name")), App.workConnection))
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

            TextBlock tb;

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

                DataGridTextColumn login = new DataGridTextColumn();
                login.Binding = new Binding("Login");
                tb = new TextBlock();
                tb.SetResourceReference(TextBlock.TextProperty, "m_AdministratorLogin");
                login.Header = tb;
                AdministratorTable.Columns.Add(login);

                DataGridTextColumn name = new DataGridTextColumn();
                name.Binding = new Binding("Name");
                tb = new TextBlock();
                tb.SetResourceReference(TextBlock.TextProperty, "m_AttendantResidentsNameHeader");
                name.Header = tb;
                AdministratorTable.Columns.Add(name);

                DataGridTextColumn surname = new DataGridTextColumn();
                surname.Binding = new Binding("Surname");
                tb = new TextBlock();
                tb.SetResourceReference(TextBlock.TextProperty, "m_AttendantResidentsSurnameHeader");
                surname.Header = tb;
                AdministratorTable.Columns.Add(surname);

                DataGridTextColumn patr = new DataGridTextColumn();
                patr.Binding = new Binding("Patronymic");
                tb = new TextBlock();
                tb.SetResourceReference(TextBlock.TextProperty, "m_AttendantResidentsPatronymicHeader");
                patr.Header = tb;
                AdministratorTable.Columns.Add(patr);

                DataGridComboBoxColumn building = new DataGridComboBoxColumn();
                building.ItemsSource = allBuildingsNames;
                building.SelectedItemBinding = new Binding("Building");
                tb = new TextBlock();
                tb.SetResourceReference(TextBlock.TextProperty, "m_AttendantResidentsBuildingHeader");
                building.Header = tb;
                AdministratorTable.Columns.Add(building);

                DataGridComboBoxColumn rooms = new DataGridComboBoxColumn();
                rooms.ItemsSource = allRooms;
                tb = new TextBlock();
                rooms.SelectedItemBinding = new Binding("Room");
                tb.SetResourceReference(TextBlock.TextProperty, "m_AttendantResidentsRoomHeader");
                rooms.Header = tb;
                AdministratorTable.Columns.Add(rooms);

                DataGridTextColumn card = new DataGridTextColumn();
                card.Binding = new Binding("StudentCard");
                tb = new TextBlock();
                tb.SetResourceReference(TextBlock.TextProperty, "m_AttendantResidentsStudentCardHeader");
                card.Header = tb;
                AdministratorTable.Columns.Add(card);

                AdministratorTable.ItemsSource = residents;
            }
            else if (mode == UserCategories.Attendant)
            {
                attendants = new ObservableCollection<ForAdminInfo.AdmAttendantInfo>();

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
                AdministratorTable.Columns.Add(name);

                DataGridTextColumn surname = new DataGridTextColumn();
                surname.Binding = new Binding("Surname");
                AdministratorTable.Columns.Add(surname);

                DataGridTextColumn patr = new DataGridTextColumn();
                patr.Binding = new Binding("Patronymic");
                AdministratorTable.Columns.Add(patr);

                DataGridTemplateColumn buildings = new DataGridTemplateColumn();
                DataTemplate dt = new DataTemplate();
                FrameworkElementFactory buttonFactory2 = new FrameworkElementFactory(typeof(Button));
                TextBlock tb3 = new TextBlock();
                tb3.SetResourceReference(TextBox.TextProperty, "m_AdministratorAttendantBuildingButton");
                buttonFactory2.SetValue(Button.ContentProperty, tb3);
                buttonFactory2.SetValue(Button.CommandParameterProperty, "ID");
                buttonFactory2.AddHandler(Button.ClickEvent, new RoutedEventHandler(AdministratorAttendantButton_Click));
                dt.VisualTree = buttonFactory2;
                AdministratorTable.Columns.Add(buildings);

                AdministratorTable.ItemsSource = attendants;
            }
            else if (mode == UserCategories.Security)
            {
                securities = new ObservableCollection<ForAdminInfo.AdmSecurityInfo>();

                DataGridTextColumn id = new DataGridTextColumn();
                id.Binding = new Binding("ID");
                id.IsReadOnly = true;
                AdministratorTable.Columns.Add(id);

                DataGridTextColumn login = new DataGridTextColumn();
                login.Binding = new Binding("Login");
                login.IsReadOnly = true;
                AdministratorTable.Columns.Add(login);

                DataGridTextColumn nameru = new DataGridTextColumn();
                nameru.Binding = new Binding("NameRU");
                AdministratorTable.Columns.Add(nameru);

                DataGridTextColumn nameen = new DataGridTextColumn();
                nameen.Binding = new Binding("NameEN");
                AdministratorTable.Columns.Add(nameen);

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
                AdministratorTable.Columns.Add(name);

                DataGridTextColumn surname = new DataGridTextColumn();
                surname.Binding = new Binding("Surname");
                AdministratorTable.Columns.Add(surname);

                DataGridTextColumn patr = new DataGridTextColumn();
                patr.Binding = new Binding("Patronymic");
                AdministratorTable.Columns.Add(patr);

                AdministratorTable.ItemsSource = administrators;
            }

            DataGridTextColumn password = new DataGridTextColumn();
            password.Binding = new Binding("Password");
            tb = new TextBlock();
            tb.SetResourceReference(TextBlock.TextProperty, "m_AdministratorPassword");
            password.Header = tb;
            AdministratorTable.Columns.Add(password);

            DataGridTemplateColumn deleteButtons = new DataGridTemplateColumn();
            DataTemplate dt2 = new DataTemplate();
            FrameworkElementFactory buttonFactory = new FrameworkElementFactory(typeof(Button));
            FrameworkElementFactory textBlockFactory = new FrameworkElementFactory(typeof(TextBlock));

            textBlockFactory.SetResourceReference(TextBlock.TextProperty, "m_AdministratorDeleteButton");
            buttonFactory.AppendChild(textBlockFactory);
            buttonFactory.SetValue(Button.CommandParameterProperty, "ID");
            buttonFactory.AddHandler(Button.ClickEvent, new RoutedEventHandler(AdministratorDeleteButton_Click));
            dt2.VisualTree = buttonFactory;
            AdministratorTable.Columns.Add(deleteButtons);
        }

        private void AdministratorDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            DependencyObject dataGridRow = sender as DependencyObject;
            while (dataGridRow != null && !(dataGridRow is DataGridRow)) dataGridRow = VisualTreeHelper.GetParent(dataGridRow);

            int index;

            if (dataGridRow != null)
            {
                index = (dataGridRow as DataGridRow).GetIndex();
            }
            else return;

            if (mode == UserCategories.Resident)
            {
                residents.RemoveAt(index);
                AdministratorTable.ItemsSource = residents;
            }
            else if (mode == UserCategories.Attendant)
            {
                attendants.RemoveAt(index);
                AdministratorTable.ItemsSource = attendants;
            }
            else if (mode == UserCategories.Security)
            {
                securities.RemoveAt(index);
                AdministratorTable.ItemsSource = securities;
            }
            else
            {
                administrators.RemoveAt(index);
                AdministratorTable.ItemsSource = administrators;
            }


        }

        private void AdministratorAttendantButton_Click(object sender, RoutedEventArgs e)
        {
            DependencyObject dataGridRow = sender as DependencyObject;
            while (dataGridRow != null && !(dataGridRow is DataGridRow)) dataGridRow = VisualTreeHelper.GetParent(dataGridRow);

            int index;

            if (dataGridRow != null)
            {
                index = (dataGridRow as DataGridRow).GetIndex();
            }
            else return;

            int id = attendants[index].ID;

            AdministratorSetAttendantBuildingsWindow aw = new AdministratorSetAttendantBuildingsWindow();
            aw.ShowDialog();
        }

        private void AdministratorAddCancelButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show((string)TryFindResource("m_saveQuestion"), (string)TryFindResource("m_confirmatioRequired"), MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes) this.Close();

        }

        private void AdministratorAddlButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (mode == UserCategories.Resident)
                {
                    for (int i = 0; i < residents.Count;)
                    {
                        int id;
                        if (residents[i].Patronymic == null) residents[i].Patronymic = "";
                        using (NpgsqlCommand cmd = new NpgsqlCommand(insertPerson, App.workConnection))
                        {
                            cmd.Parameters.AddWithValue("@login", residents[i].Login);
                            cmd.Parameters.AddWithValue("@name", residents[i].Name);
                            cmd.Parameters.AddWithValue("@surname", residents[i].Surname);
                            cmd.Parameters.AddWithValue("@patronymic", residents[i].Patronymic);

                            id = (int)cmd.ExecuteScalar();
                        }
                        using (NpgsqlCommand cmd = new NpgsqlCommand(insertResident, App.workConnection))
                        {
                            cmd.Parameters.AddWithValue("@id", id);
                            cmd.Parameters.AddWithValue("@room", Int16.Parse(residents[i].Room));
                            cmd.Parameters.AddWithValue("building", allBuildingsID[allBuildingsNames.FindIndex(x => x==residents[i].Building)] );
                            cmd.Parameters.AddWithValue("@card", residents[i].StudentCard);
                            cmd.Parameters.AddWithValue("@patronymic", residents[i].Patronymic);

                            cmd.ExecuteNonQuery();
                        }
                        using (NpgsqlCommand cmd = new NpgsqlCommand(insertResidentUser, App.workConnection))
                        {
                            cmd.Parameters.AddWithValue("@login", residents[i].Login);
                            cmd.Parameters.AddWithValue("@password", residents[i].Password);

                            cmd.ExecuteNonQuery();
                        }

                        residents.RemoveAt(0);
                    }
                    
                }
                else if (mode == UserCategories.Attendant)
                {
                    for (int i = 0; i < attendants.Count;)
                    {
                        int id;
                        if (attendants[i].Patronymic == null) attendants[i].Patronymic = "";
                        using (NpgsqlCommand cmd = new NpgsqlCommand(insertPerson, App.workConnection))
                        {
                            cmd.Parameters.AddWithValue("@login", attendants[i].Login);
                            cmd.Parameters.AddWithValue("@name", attendants[i].Name);
                            cmd.Parameters.AddWithValue("@surname", attendants[i].Surname);
                            cmd.Parameters.AddWithValue("@patronymic", attendants[i].Patronymic);

                            id = (int)cmd.ExecuteScalar();
                        }
                        using (NpgsqlCommand cmd = new NpgsqlCommand(insertAttendant, App.workConnection))
                        {
                            cmd.Parameters.AddWithValue("@id", id);

                            cmd.ExecuteNonQuery();
                        }
                        using (NpgsqlCommand cmd = new NpgsqlCommand(insertAttendantUser, App.workConnection))
                        {
                            cmd.Parameters.AddWithValue("@login", attendants[i].Login);
                            cmd.Parameters.AddWithValue("@password", attendants[i].Password);

                            cmd.ExecuteNonQuery();
                        }

                        attendants.RemoveAt(0);
                    }
                }
                else if (mode == UserCategories.Security)
                {
                    for (int i = 0; i < securities.Count;)
                    {
                        int id;
                        using (NpgsqlCommand cmd = new NpgsqlCommand(insertSecurity, App.workConnection))
                        {
                            cmd.Parameters.AddWithValue("@nameru", securities[i].NameRU);
                            cmd.Parameters.AddWithValue("@nameen", securities[i].NameEN);

                            id = (int)cmd.ExecuteScalar();
                        }
                        using (NpgsqlCommand cmd = new NpgsqlCommand(insertSecurityUser, App.workConnection))
                        {
                            cmd.Parameters.AddWithValue("@login", App.GetSecurityLogin(id.ToString()));
                            cmd.Parameters.AddWithValue("@password", securities[i].Password);

                            cmd.ExecuteNonQuery();
                        }

                        administrators.RemoveAt(0);
                    }
                }
                else if (mode == UserCategories.Administrator)
                {
                    for (int i = 0; i < administrators.Count;)
                    {
                        int id;
                        if (administrators[i].Patronymic == null) administrators[i].Patronymic = "";
                        using (NpgsqlCommand cmd = new NpgsqlCommand(insertPerson, App.workConnection))
                        {
                            cmd.Parameters.AddWithValue("@login", administrators[i].Login);
                            cmd.Parameters.AddWithValue("@name", administrators[i].Name);
                            cmd.Parameters.AddWithValue("@surname", administrators[i].Surname);
                            cmd.Parameters.AddWithValue("@patronymic", administrators[i].Patronymic);

                            id = (int)cmd.ExecuteScalar();
                        }
                        using (NpgsqlCommand cmd = new NpgsqlCommand(insertAdministrator, App.workConnection))
                        {
                            cmd.Parameters.AddWithValue("@id", id);

                            cmd.ExecuteNonQuery();
                        }
                        using (NpgsqlCommand cmd = new NpgsqlCommand(insertAdministratorUser, App.workConnection))
                        {
                            cmd.Parameters.AddWithValue("@login", administrators[i].Login);
                            cmd.Parameters.AddWithValue("@password", administrators[i].Password);

                            cmd.ExecuteNonQuery();
                        }

                        administrators.RemoveAt(0);
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show(ex.Message, (string)TryFindResource("m_error"), MessageBoxButton.OK,MessageBoxImage.Error);
                UpdateData();
            }

            this.Close();
        }
    }
}
