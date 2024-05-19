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

namespace Kursovaya2
{
    /// <summary>
    /// Логика взаимодействия для SecurityPage.xaml
    /// </summary>
    public partial class SecurityPage : Page, IPageN
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
            UpdateActualBookingTable();
            UpdatePreparedBookingTable();
            UpdateFreeLaundriesTable();
        }

        public void OnNavigatingFrom(object sender, NavigatingCancelEventArgs e)
        {
            App.LanguageChanged -= UpdateOnLangChange;
            UnsubscribeMainWindow();
        }

        static readonly string selectActiveBookings = "SELECT \"Active_Bookings\".\"ID\", \"Laundry ID\", \"{0}\" AS \"Lname\", \"Resident ID\", \"People\".\"Name\", \"People\".\"Surname\", \"People\".\"Patronymic\",\r\n\"Student card\", \"Room\" FROM \"Active_Bookings\", \"Residents\" , \"Booking_list\", \"People\", \"Laundries\" WHERE \"Laundries\".\"Building\" = @post AND \"People\".\"ID\" = \"Resident ID\" AND \"Active_Bookings\".\"ID\" = \"Booking ID\" AND \"Resident ID\" = \"Resident\" ORDER BY \"Lname\" ASC;";
        static readonly string selectFreeLaundries = "SELECT \"Laundry ID\", \"{0}\" AS \"Name\" FROM \"Laundries\" WHERE \"Work_status\" = TRUE AND \"Laundries\".\"Building\" = @post EXCEPT (SELECT \"Laundry\", \"{0}\" FROM \"Active_Bookings\", \"Laundries\") ORDER BY \"Name\" ASC;";
        static readonly string fillFreeLaundry = "SELECT CheckFreeLaundry( @laundryID )";
        static readonly string checkViolation = "SELECT * FROM CheckAndInsertViolation( @booking );";
        static readonly string selectPreparedBookings = "SELECT \"id\", \"Booking ID\", \"Laundry ID\", \"NameRU\" AS \"Lname\", \"Resident ID\", \"People\".\"Name\", \"People\".\"Surname\", \"People\".\"Patronymic\", \"Student card\", \"Room\" FROM \"Prepared_bookings\", \"Booking_list\", \"Residents\", \"People\", \"Laundries\" WHERE  \"Laundries\".\"Building\" = @post AND \"People\".\"ID\" = \"Resident ID\" AND \"id\" = \"Booking ID\" AND \"Resident ID\" = \"Resident\" ORDER BY \"Lname\" ASC;";
        static readonly string deleteFromActiveBookings = "CALL DeleteFromActualTable(@laundryID);";
        static readonly string movePreparedToActualBookings = "SELECT MovePreparedToActual( @book );";
        static readonly string selectPrepared = "SELECT \"Booking ID\" FROM \"Booking_list\" WHERE \"Laundry\" = @laundryID INTERSECT SELECT \"id\" FROM \"Prepared_bookings\" LIMIT 1;";


        public SecurityPage()
        {
            InitializeComponent();

            UpdateActualBookingTable();
            UpdatePreparedBookingTable();
            UpdateFreeLaundriesTable();

            App.LanguageChanged += UpdateOnLangChange;
        }

        class BookingsTableData
        {
            public string Resident { get; set; }

            public string Laundry { get; set; }

            public string Booking { get; set; }

            public string FullName { get; set; }
            public string StudentCard { get; set; }
            public string Room { get; set; }

            public int laundryID;

            public BookingsTableData()
            {

            }
        }

        class LaundryOnly
        {
            public string Laundry { get; set; }
            public int laundryID;
        }

        public void UpdateActualBookingTable()
        {
            List< BookingsTableData> tableData = new List< BookingsTableData>();

            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(selectActiveBookings, App.GetLocalisedKey("Name")), App.workConnection))
            {
                cmd.Parameters.AddWithValue("@post", Int32.Parse(App.login));

                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    while (reader.Read())
                    {
                        var id = ((int)reader["ID"]);
                        var laundry = ((int)reader["Laundry ID"]);
                        var laundryName = ((string)reader["Lname"]);
                        var name = ((string)reader["Name"]);
                        var surname = ((string) reader["Surname"]);
                        var patr = ((string)reader["Patronymic"]);
                        var card = ((string)reader["Student card"]);
                        var room = ((short)reader["Room"]);

                        BookingsTableData btd = new BookingsTableData();
                        btd.laundryID = laundry;
                        btd.Laundry = laundryName;
                        btd.Booking = id.ToString();
                        btd.Resident = surname + " " + name;
                        btd.FullName = surname + " " + name + " " + patr;
                        btd.StudentCard = TryFindResource("m_studentStartPersonalStudentCard") + " " + card;
                        btd.Room = TryFindResource("m_studentStartPersonalRoom") + " " + room;

                        tableData.Add(btd);
                    }
            }

            ActualBookingsTable.ItemsSource = tableData;
        }

        public void UpdatePreparedBookingTable()
        {
            List<BookingsTableData> tableData = new List<BookingsTableData>();

            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(selectPreparedBookings, App.GetLocalisedKey("Name")), App.workConnection))
            {
                cmd.Parameters.AddWithValue("@post", Int32.Parse(App.login));

                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    while (reader.Read())
                    {
                        var id = ((int)reader["id"]);
                        var laundry = ((int)reader["Laundry ID"]);
                        var laundryName = ((string)reader["Lname"]);
                        var name = ((string)reader["Name"]);
                        var surname = ((string)reader["Surname"]);
                        var patr = ((string)reader["Patronymic"]);
                        var card = ((string)reader["Student card"]);
                        var room = ((short)reader["Room"]);

                        BookingsTableData btd = new BookingsTableData();
                        btd.laundryID = laundry;
                        btd.Laundry = laundryName;
                        btd.Booking = id.ToString();
                        btd.Resident = surname + " " + name;
                        btd.FullName = surname  + " " + name + " " + patr;
                        btd.StudentCard = TryFindResource("m_studentStartPersonalStudentCard") + " " + card;
                        btd.Room = TryFindResource("m_studentStartPersonalRoom") + " " + room;

                        tableData.Add(btd);
                    }
            }

            PreparedBookingsTable.ItemsSource = tableData;
        }

        public void UpdateFreeLaundriesTable()
        {
            List<LaundryOnly> tableData = new List<LaundryOnly>();

            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(selectFreeLaundries, App.GetLocalisedKey("Name")), App.workConnection))
            {
                cmd.Parameters.AddWithValue("@post", Int32.Parse(App.login));

                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    while (reader.Read())
                    {
                        var laundry = ((int)reader["Laundry ID"]);
                        var name = ((string)reader["Name"]);

                        LaundryOnly lo = new LaundryOnly();

                        lo.laundryID = laundry;
                        lo.Laundry = name;

                        tableData.Add(lo);
                    }
            }

            FreeLaundriesTable.ItemsSource = tableData;
        }

        private void ActualBookingsTableButton_Click(object sender, RoutedEventArgs e)
        {
            int toDeleteId;
            int r=-1;
            for (var v = sender as Visual; v != null; v = VisualTreeHelper.GetParent(v) as Visual)
            {
                if (v is DataGridRow)
                {
                    var row = (DataGridRow)v;
                    r = row.GetIndex();
                    break;
                }
            }

            var vv = ActualBookingsTable.ItemsSource as List<BookingsTableData>;

            if (r == -1 || vv.Count==0) return;

            toDeleteId = vv[r].laundryID;
            int bookingID = Int32.Parse(vv[r].Booking);

            bool flag = false;
            string message="";

            using (NpgsqlCommand cmd = new NpgsqlCommand(checkViolation, App.workConnection))
            {
                cmd.Parameters.AddWithValue("@booking", bookingID);

                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var name = ((string)reader["name"]);
                        var surname = ((string)reader["surname"]);
                        var patr = ((string)reader["patronymic"]);
                        var time = ((TimeSpan)reader["violationsize"]);

                        string t = App.TimeSpanToString(time);
                        message = string.Format((string)TryFindResource("m_timeViolationTemplate"), surname, name, patr, t.Remove(3),t.Substring(4));

                        flag = true;
                    }
                }
            }

            if (flag)
            {
                MessageBox.Show(message, (string)TryFindResource("m_warning"), MessageBoxButton.OK,MessageBoxImage.Warning);
            }

            using (NpgsqlCommand cmd = new NpgsqlCommand(deleteFromActiveBookings, App.workConnection))
            {
                cmd.Parameters.AddWithValue("@laundryID", toDeleteId);

                cmd.ExecuteNonQuery();
            }

            UpdateActualBookingTable();
            UpdateFreeLaundriesTable();
        }
        
        private void PreparedBookingsTableButton_Click(object sender, RoutedEventArgs e)
        {
            int toDeleteId;
            int r = -1;
            for (var v = sender as Visual; v != null; v = VisualTreeHelper.GetParent(v) as Visual)
            {
                if (v is DataGridRow)
                {
                    var row = (DataGridRow)v;
                    r = row.GetIndex();
                    break;
                }
            }

            var vv = PreparedBookingsTable.ItemsSource as List<BookingsTableData>;

            if (r == -1 || vv.Count == 0) return;

            toDeleteId = vv[r].laundryID;

            short res;

            int book;

            using (NpgsqlCommand cmd = new NpgsqlCommand(selectPrepared, App.workConnection))
            {
                cmd.Parameters.AddWithValue("@laundryID", toDeleteId);

                book = (int)cmd.ExecuteScalar();
            }


            using (NpgsqlCommand cmd = new NpgsqlCommand(movePreparedToActualBookings, App.workConnection))
            {
                cmd.Parameters.AddWithValue("@book", book);

                res = (short)cmd.ExecuteScalar();
            }

            if (res == -1) // если готовящаяся запись прошла
            {
                MessageBox.Show((string)TryFindResource("m_securityErrorNoPrepared"), (string)TryFindResource("m_error"), MessageBoxButton.OK, MessageBoxImage.Error);
                UpdatePreparedBookingTable();
            }
            else if (res == 0) // если прачечная ещё занята
            {
                MessageBox.Show((string)TryFindResource("m_securityErrorLaundryOccupied"), (string)TryFindResource("m_error"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else // если всё прошло хорошо
            {
                MessageBox.Show((string)TryFindResource("m_securityMoveSuccessful"), (string)TryFindResource("m_success"), MessageBoxButton.OK, MessageBoxImage.Information);
                UpdatePreparedBookingTable();
                UpdateActualBookingTable();
            }

            
        }

        private void FreeLaundriesTableButton_Click(object sender, RoutedEventArgs e)
        {
            int toDeleteId;
            int r = -1;
            for (var v = sender as Visual; v != null; v = VisualTreeHelper.GetParent(v) as Visual)
            {
                if (v is DataGridRow)
                {
                    var row = (DataGridRow)v;
                    r = row.GetIndex();
                    break;
                }
            }

            var vv = FreeLaundriesTable.ItemsSource as List<LaundryOnly>;

            if (r == -1 || vv.Count==0) return;

            toDeleteId = vv[r].laundryID;

            bool result;

            using (NpgsqlCommand cmd = new NpgsqlCommand(fillFreeLaundry, App.workConnection))
            {
                cmd.Parameters.AddWithValue("@laundryID", toDeleteId);

                result = (bool)cmd.ExecuteScalar();
            }

            if (result)
            {
                MessageBox.Show((string)TryFindResource("m_securityMoveSuccessful"), (string)TryFindResource("m_success"), MessageBoxButton.OK, MessageBoxImage.Information);
                UpdatePreparedBookingTable();
                UpdateActualBookingTable();
                UpdateFreeLaundriesTable();
            }
            else
            {
                MessageBox.Show((string)TryFindResource("m_securityErrorNoBooking"), (string)TryFindResource("m_error"), MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void SecurityUpdatePrepared_Click(object sender, RoutedEventArgs e)
        {
            UpdatePreparedBookingTable();
        }
    }
}
