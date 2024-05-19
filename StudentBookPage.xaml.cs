using iTextSharp.text.log;
using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
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
using static Kursovaya2.App;

namespace Kursovaya2
{
    /// <summary>
    /// Логика взаимодействия для StudentBookPage.xaml
    /// </summary>
    public partial class StudentBookPage : Page, IPageN
    {
        //static public List<int> SelectedTime, FreeTime;

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
            NavigationRequested(new Uri("StudentMenuPage.xaml", UriKind.Relative));
            return;
        }

        public bool IsStart
        {
            get
            {
                return false;
            }
        }
        public void OnNavigatingFrom(object sender, NavigatingCancelEventArgs e)
        {
            App.LanguageChanged -= UpdateOnLangChange;
            UnsubscribeMainWindow();
        }
        public void UpdateOnLangChange()
        {
            UpdateConstantData();
        }

        string laundry;
        string date;
        string time;
        string duration;
        List<DateTime> currentDate;
        List<short> selectedSlots;
        List<short> durations;
        List<int> laundries;
        List<string> laundriesNames;
        int currentDateIndex;
        int currentSlot;
        int currentDuration;
        int currentLaundry;

        static readonly string SelectLaundries = "WITH LoginID AS (SELECT \"ID\" FROM \"People\" WHERE \"Login\" = @login), Needed_building AS (SELECT \"Building\" FROM \"Residents\" WHERE \"Resident ID\" = (SELECT * FROM LoginID) LIMIT 1)\r\nSELECT DISTINCT \"Laundry ID\", \"{0}\" AS \"Name\" FROM \"Laundries\" WHERE \"Work_status\" = TRUE AND \"Building\" IN (SELECT * FROM Needed_building);";
        static readonly string SelectDates = "SELECT * FROM GetWeekForBooking(CURRENT_DATE);";
        static readonly string SelectTimes = "SELECT * FROM GetFreeTimeTSP( @date, @laundry);";
        static readonly string SelectDurations = "SELECT * FROM GetDurationTSP(@date, @laundry, @slot);";
        static readonly string AddBooking = "SELECT InsertBooking(@date, @laundry, @login, @slot, @duration);";
        static readonly string CheckTimes = "SELECT * FROM CheckFreeSlots( @date, @laundry )";
        static readonly string checkExistBooks = "WITH LoginID AS (SELECT \"ID\" FROM \"People\" WHERE \"Login\" = @login) SELECT EXISTS( SELECT \"Booking ID\" FROM \"Booking_list\" WHERE \"Resident\" = (SELECT * FROM LoginID LIMIT 1) AND \"Date\" = @date );";


        public StudentBookPage()
        {
            InitializeComponent();

            

            App.LanguageChanged += UpdateOnLangChange;
            this.Loaded += StudentBookPage_Loaded;
        }

        private void StudentBookPage_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateConstantData();
        }

        void UpdateConstantData()
        {
            List<string> student_book_laundry_domainUpDownTexts = new List<string>();
            laundries = new List<int>();

            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(SelectLaundries, GetLocalisedKey("Name")), App.workConnection))
            {
                cmd.Parameters.AddWithValue("@login", App.login);

                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    while (reader.Read())
                    {
                        var s = (string)reader["Name"];
                        var i = (int)reader["Laundry ID"];
                        student_book_laundry_domainUpDownTexts.Add(s);
                        laundries.Add(i);
                    }
            }
            currentLaundry = 0;

            date = null;// StudentBookDateSelector.Items.Clear();

            List<string> student_book_date_domainUpDownTexts = new List<string>();

            using (NpgsqlCommand cmd = new NpgsqlCommand(SelectDates, App.workConnection))
            {

                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                {
                    currentDate = new List<DateTime>();
                    currentDateIndex = 0;

                    while (reader.Read())
                    {
                        currentDate.Add((DateTime)reader["nextday"]);
                        var s = currentDate[currentDate.Count - 1].ToString().Remove(10) + " " + (Window.GetWindow(this) as MainWindow).GetWeekDayName((short)reader["nextdow"]);
                        student_book_date_domainUpDownTexts.Add(s);
                    }
                }
            }

            //StudentBookLaundrySelector.Items.Clear();
            StudentBookLaundrySelector.ItemsSource = student_book_laundry_domainUpDownTexts;

            laundriesNames = student_book_laundry_domainUpDownTexts;

            StudentBookDateSelector.ItemsSource = student_book_date_domainUpDownTexts;
        }

        private void ClearTimeDur()
        {
            time = null; //StudentBookTimeSelector.Items.Clear();
            duration = null; //StudentBookContSelector.Items.Clear();

            StudentBookTimeSelector.ItemsSource = null;
            StudentBookContSelector.ItemsSource = null;
            if (selectedSlots != null)
                selectedSlots.Clear();
            if (durations != null)
                durations.Clear();
        }

        private void StudentBookLaundrySelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            currentLaundry = StudentBookLaundrySelector.SelectedIndex;
            laundry = StudentBookLaundrySelector.Text;

            //ClearTimeDur();

            PrepareTime();
        }

        

        //public static Dictionary<string, short> cellState = new Dictionary<string, short>();

        private void UpdateTimetable()
        {
            FreeTimeWarnContainer.Visibility = Visibility.Hidden;

            List<Label> ll = new List<Label>();
            Label l;

            short counter = 0; bool b;

            using (NpgsqlCommand cmd = new NpgsqlCommand(CheckTimes, App.workConnection))
            {
                cmd.Parameters.AddWithValue("@date", currentDate[currentDateIndex]);
                cmd.Parameters.AddWithValue("@laundry", laundries[currentLaundry]);

                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        b = (bool)reader["isFree"] && (currentDate[currentDateIndex].Date != DateTime.Today || App.GetDateTimeFromSlots(counter) >= DateTime.Now);

                        l = new Label();

                        l.HorizontalAlignment = HorizontalAlignment.Stretch; 
                        l.VerticalAlignment = VerticalAlignment.Stretch;

                        l.Content = App.GetTimeFromSlots(counter, false);

                        l.Background = b ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.Red);

                        l.Foreground = b ? new SolidColorBrush(Colors.Black) : new SolidColorBrush(Colors.White);

                        l.BorderBrush = new SolidColorBrush(Colors.Black);
                        l.BorderThickness = new Thickness(1, 1, 1, 1);

                        ll.Add(l);

                        counter++;
                    }
                }
            }

            if ((selectedSlots != null && selectedSlots.Count>0) && (durations != null && durations.Count > 0))
            {
                for (int i = selectedSlots[currentSlot]-1; i < selectedSlots[currentSlot] + durations[currentDuration]; i++)
                {
                    ll[i].Background = new SolidColorBrush(Colors.Yellow);
                    ll[i].Foreground = new SolidColorBrush(Colors.Black);
                }
            }

            List<StackPanel> columns = new List<StackPanel>(8);

            for (int i = 0; i < 8; i++)
            {
                columns.Add(new StackPanel());
                columns[i].Orientation = Orientation.Vertical;
                
                for (int j = 0; j < 12; j++)
                {
                    var p = ll[j*8+i];
                    if (p.Parent != null)
                    {
                        var parent = (Panel)p.Parent;
                        parent.Children.Remove(p);
                    }
                    columns[i].Children.Add( p);
                }
            }

            FreeTimeTable.Children.Clear();
            FreeTimeTable.ColumnDefinitions.Clear();

            for (int i=0;i< 8; i++)
            {
                FreeTimeTable.ColumnDefinitions.Add(new ColumnDefinition());
                Grid.SetColumn(columns[i], i);
                Grid.SetRow(columns[i], 0);

                FreeTimeTable.Children.Add(columns[i]);
            }

        }

        private void PrepareTime()
        {
            if (laundry == null || date == null) return;

            time = null;// StudentBookTimeSelector.Items.Clear();

            List<string> student_book_time_domainUpDownTexts = new List<string>();

            using (NpgsqlCommand cmd = new NpgsqlCommand(SelectTimes, App.workConnection))
            {
                cmd.Parameters.AddWithValue("@date", currentDate[currentDateIndex]);
                cmd.Parameters.AddWithValue("@laundry", laundries[currentLaundry]);

                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                {
                    selectedSlots = new List<short>();
                    currentSlot = 0;

                    while (reader.Read())
                    {
                        var ss = (short)reader["freeslots"];
                        var s = App.GetTimeFromSlots(ss);
                        if ((currentDate[currentDateIndex].Date != DateTime.Today || App.GetDateTimeFromSlots(ss) >= DateTime.Now))
                        {
                            selectedSlots.Add(ss);
                            student_book_time_domainUpDownTexts.Add(s);
                        }
                    }
                }
            }

            //StudentBookTimeSelector.Items.Clear();
            StudentBookTimeSelector.ItemsSource = student_book_time_domainUpDownTexts;
            StudentBookTimeSelector.SelectedIndex = 0;

            time = StudentBookTimeSelector.Text;
            currentSlot = StudentBookTimeSelector.SelectedIndex;

            duration = null;// StudentBookContSelector.Items.Clear();

            UpdateDurations();
        }

        private void StudentBookDateSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            date = StudentBookDateSelector.Text;
            currentDateIndex = StudentBookDateSelector.SelectedIndex;

            //ClearTimeDur();

            PrepareTime();

        }

        private void UpdateDurations()
        {
            List<string> student_book_duration_domainUpDownTexts = new List<string>();

            using (NpgsqlCommand cmd = new NpgsqlCommand(SelectDurations, App.workConnection))
            {
                cmd.Parameters.AddWithValue("@date", currentDate[currentDateIndex]);
                cmd.Parameters.AddWithValue("@laundry", laundries[currentLaundry]);
                cmd.Parameters.AddWithValue("@slot", selectedSlots[currentSlot]);

                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                {
                    durations = new List<short>();
                    while (reader.Read())
                    {
                        durations.Add((short)reader["durations"]);
                        var s = App.GetTimeFromSlots(durations[durations.Count - 1], false);
                        student_book_duration_domainUpDownTexts.Add(s);
                    }
                }
            }

            //StudentBookContSelector.Items.Clear();
            StudentBookContSelector.ItemsSource = student_book_duration_domainUpDownTexts;
            StudentBookContSelector.SelectedIndex = 0;

            duration = StudentBookContSelector.Text;
            currentDuration = StudentBookContSelector.SelectedIndex;

            UpdateTimetable();
        }

        private void StudentBookTimeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            time = StudentBookTimeSelector.Text;
            currentSlot = StudentBookTimeSelector.SelectedIndex;

            duration = null;// StudentBookContSelector.Items.Clear();

            UpdateDurations();
        }

        private void StudentBookContSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            duration = StudentBookContSelector.Text;
            currentDuration = StudentBookContSelector.SelectedIndex;

            UpdateTimetable();
        }

        private void StudentBookBookButton_Click(object sender, RoutedEventArgs e)
        {
            if (laundry == null || date == null || time == null || duration == null)
            {
                MessageBox.Show((string)this.TryFindResource("m_errorResBookNeedMore"), (string)this.TryFindResource("m_error"), MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            bool b;

            using (NpgsqlCommand cmd = new NpgsqlCommand(checkExistBooks, App.workConnection))
            {
                cmd.Parameters.AddWithValue("@date", currentDate[currentDateIndex]);
                cmd.Parameters.AddWithValue("@login", App.login);

                b = (bool)cmd.ExecuteScalar();
            }

            if (b)
            {
                MessageBox.Show((string)this.TryFindResource("m_unsuccessBookingAlreadyExists"), (string)this.TryFindResource("m_error"), MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using (NpgsqlCommand cmd = new NpgsqlCommand(AddBooking, App.workConnection))
            {
                cmd.Parameters.AddWithValue("@date", currentDate[currentDateIndex]);
                cmd.Parameters.AddWithValue("@laundry", laundries[currentLaundry]);
                cmd.Parameters.AddWithValue("@slot", selectedSlots[currentSlot]);
                cmd.Parameters.AddWithValue("@login", App.login);
                cmd.Parameters.AddWithValue("@duration", durations[currentDuration]);

                int logNumber = (int)cmd.ExecuteScalar();

                if (logNumber == -1)
                {
                    MessageBox.Show((string)this.TryFindResource("m_unsuccessBooking"), (string)this.TryFindResource("m_error"), MessageBoxButton.OK,MessageBoxImage.Error);
                    ClearTimeDur();
                    UpdateTimetable();
                    return;
                }

                MessageBox.Show((string)this.TryFindResource("m_successBooking")+ " " + logNumber, (string)this.TryFindResource("m_success"), MessageBoxButton.OK, MessageBoxImage.Information);

                SubscribeMainWindow();
                NavigationRequested(new Uri("StudentMenuPage.xaml", UriKind.Relative));
            }
        }
    }

}
