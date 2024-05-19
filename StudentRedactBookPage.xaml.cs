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
    /// Логика взаимодействия для StudentRedactBookPage.xaml
    /// </summary>
    public partial class StudentRedactBookPage : Page, IPageN
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
            NavigationRequested(new Uri("StudentMenuPage.xaml", UriKind.Relative));
            return;
        }

        string book;
        int laundry;
        string date;
        string time;
        string duration;
        List<DateTime> currentDate;
        List<short> selectedSlots;
        List<short> selectedDurations;
        List<short> slots;
        List<short> durations;
        List<int> laundries;
        List<string> laundriesNames;
        List<int> books;
        int currentSlot;
        int currentDuration;
        int currentBook;

        static readonly string selectBookInf = "WITH LoginID AS (SELECT \"ID\" FROM \"People\" WHERE \"Login\" = @login)\r\nSELECT \"Booking ID\", \"Laundry\", \"Start time\", \"Duration\", \"Date\" FROM \"Booking_list\" WHERE \"Resident\" = (SELECT * FROM LoginID LIMIT 1);";
        static readonly string selectLaundryName = "SELECT \"{0}\" FROM \"Laundries\" WHERE \"Laundry ID\" = @laundry;";
        static readonly string selectTimesExc = "SELECT * FROM GetFreeTimeTSPExc( @date, @laundry, @book);";
        static readonly string selectDurationsExc = "SELECT * FROM getdurationtspexc(@date, @laundry, @slot, @book);";
        static readonly string checkTimes = "SELECT * FROM CheckFreeSlotsExc( @date, @laundry, @book )";
        static readonly string updateBooking = "SELECT UpdateBooking( @book , @slot, @duration);";
        static readonly string deleteBooking = "SELECT DeleteBooking( @book);";
        static readonly string checkBookingTimeToUpdate = "SELECT EXISTS(SELECT \"Booking ID\" FROM \"Booking_list\" WHERE \"Booking ID\" = @book AND (\"Date\" > CURRENT_DATE OR ( \"Date\" = CURRENT_DATE AND CURRENT_TIME < (SlotsToTime(\"Start time\") - '00:15'::interval) ) ) );";

        public StudentRedactBookPage()
        {
            InitializeComponent();
            
            App.LanguageChanged += UpdateOnLangChange;
            this.Loaded += OnLoad;

            UpdateData();
        }

        private void UpdateData()
        {
            books = new List<int>();
            currentDate = new List<DateTime>();
            selectedSlots = new List<short>();
            selectedDurations = new List<short>();
            laundries = new List<int>();
            laundriesNames = new List<string>();

            using (NpgsqlCommand cmd = new NpgsqlCommand(selectBookInf, App.workConnection))
            {
                cmd.Parameters.AddWithValue("@login", App.login);

                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        books.Add((int)reader["Booking ID"]);
                        laundries.Add((int)reader["Laundry"]);
                        selectedSlots.Add((short)reader["Start time"]);
                        selectedDurations.Add((short)reader["Duration"]);
                        currentDate.Add((DateTime)reader["Date"]);
                    }
                }

            }

            if (books.Count == 0) return;

            for (int i = 0; i < laundries.Count; i++)
            {
                using (NpgsqlCommand cmd = new NpgsqlCommand(String.Format(selectLaundryName, App.GetLocalisedKey("Name")), App.workConnection))
                {
                    cmd.Parameters.AddWithValue("@laundry", laundries[i]);
                    laundriesNames.Add( (string)cmd.ExecuteScalar());
                }
            }

            StudentBookSelector.ItemsSource = books;
            currentBook = 0;
            StudentBookSelector.SelectedIndex = currentBook;
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            if (books.Count == 0)
            {
                MessageBox.Show((string)TryFindResource("m_errorRedactNoBookings"), (string)TryFindResource("m_error"), MessageBoxButton.OK, MessageBoxImage.Error);
                SubscribeMainWindow();
                NavigationRequested(new Uri("StudentMenuPage.xaml", UriKind.Relative));
                return;
            }
        }

        private void UpdateLabels()
        {
            laundry = laundries[currentBook];
            StudentRedactLaundryBox.Text = laundriesNames[currentBook];

            date = currentDate[currentBook].ToString().Remove(10);
            StudentRedactDateBox.Text = date;

            currentSlot = selectedSlots[currentBook];
            currentDuration = selectedDurations[currentBook];

            slots = new List<short>();

            List<string> student_book_time_domainUpDownTexts = new List<string>();

            using (NpgsqlCommand cmd = new NpgsqlCommand(selectTimesExc, App.workConnection))
            {
                cmd.Parameters.AddWithValue("@laundry", laundry);
                cmd.Parameters.AddWithValue("@date", currentDate[currentBook]);
                cmd.Parameters.AddWithValue("@book", books[currentBook]);

                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        slots.Add((short)reader["freeslots"]);
                        var s = App.GetTimeFromSlots(slots[slots.Count - 1]);
                        student_book_time_domainUpDownTexts.Add(s);
                    }
                }

            }

            StudentTimeSelector.ItemsSource = student_book_time_domainUpDownTexts;
            StudentTimeSelector.SelectedIndex = slots.FindIndex(x => x == selectedSlots[currentBook]);

            UpdateDuration();
        }

        private void UpdateDuration(bool isNewDuration = false)
        {
            durations = new List<short>();

            List<string> student_book_duration_domainUpDownTexts = new List<string>();

            using (NpgsqlCommand cmd = new NpgsqlCommand(selectDurationsExc, App.workConnection))
            {
                cmd.Parameters.AddWithValue("@laundry", laundry);
                cmd.Parameters.AddWithValue("@date", currentDate[currentBook]);
                cmd.Parameters.AddWithValue("@slot", slots[currentSlot]);
                cmd.Parameters.AddWithValue("@book", books[currentBook]);

                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        durations.Add((short)reader["durations"]);
                        var s = App.GetTimeFromSlots(durations[durations.Count - 1], false);
                        student_book_duration_domainUpDownTexts.Add(s);
                    }
                }
            }

            StudentDurationSelector.ItemsSource = student_book_duration_domainUpDownTexts;

            if (!isNewDuration)
            {
                StudentDurationSelector.SelectedIndex = durations.FindIndex(x => x == selectedDurations[currentBook]);
            }
            else
            {
                StudentDurationSelector.SelectedIndex = 0;
            }

            UpdateTimetable();
        }

        private void UpdateTimetable()
        {

            List<Label> ll = new List<Label>();
            Label l;

            short counter = 0; bool b;

            using (NpgsqlCommand cmd = new NpgsqlCommand(checkTimes, App.workConnection))
            {
                cmd.Parameters.AddWithValue("@date", currentDate[currentBook]);
                cmd.Parameters.AddWithValue("@laundry", laundry);
                cmd.Parameters.AddWithValue("@book", books[currentBook]);

                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        b = (bool)reader["isFree"] && (currentDate[currentBook].Date != DateTime.Today || App.GetDateTimeFromSlots(counter) >= DateTime.Now);

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

            if ((selectedSlots != null && selectedSlots.Count > 0) && (durations != null && durations.Count > 0))
            {
                for (int i = slots[currentSlot] - 1; i < slots[currentSlot] + durations[currentDuration]; i++)
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
                    var p = ll[j * 8 + i];
                    if (p.Parent != null)
                    {
                        var parent = (Panel)p.Parent;
                        parent.Children.Remove(p);
                    }
                    columns[i].Children.Add(p);
                }
            }

            FreeTimeTable.Children.Clear();
            FreeTimeTable.ColumnDefinitions.Clear();

            for (int i = 0; i < 8; i++)
            {
                FreeTimeTable.ColumnDefinitions.Add(new ColumnDefinition());
                Grid.SetColumn(columns[i], i);
                Grid.SetRow(columns[i], 0);

                FreeTimeTable.Children.Add(columns[i]);
            }

        }

        private void StudentRedactUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            bool isRightTime;


            using (NpgsqlCommand cmd = new NpgsqlCommand(checkBookingTimeToUpdate, App.workConnection))
            {

                cmd.Parameters.AddWithValue("@book", books[currentBook]);

                isRightTime = (bool)cmd.ExecuteScalar();
            }

            if (!isRightTime)
            {
                MessageBox.Show((string)this.TryFindResource("m_unsuccessRedactBookingTooLate"), (string)this.TryFindResource("m_error"), MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            bool b;

            using (NpgsqlCommand cmd = new NpgsqlCommand(updateBooking, App.workConnection))
            {
                
                cmd.Parameters.AddWithValue("@slot", slots[currentSlot]);
                cmd.Parameters.AddWithValue("@duration", durations[currentDuration]);
                cmd.Parameters.AddWithValue("@book", books[currentBook]);

                b = (bool)cmd.ExecuteScalar();
            }

            if (!b)
            {
                MessageBox.Show((string)this.TryFindResource("m_unsuccessBooking"), (string)this.TryFindResource("m_error"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
            MessageBox.Show((string)this.TryFindResource("m_successRedactBooking"), (string)this.TryFindResource("m_success"), MessageBoxButton.OK, MessageBoxImage.Information);

            UpdateData();
        }

        private void StudentReductDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            bool isRightTime;

            using (NpgsqlCommand cmd = new NpgsqlCommand(checkBookingTimeToUpdate, App.workConnection))
            {

                cmd.Parameters.AddWithValue("@book", books[currentBook]);

                isRightTime = (bool)cmd.ExecuteScalar();
            }

            if (!isRightTime)
            {
                MessageBox.Show((string)this.TryFindResource("m_unsuccessRedactBookingTooLate"), (string)this.TryFindResource("m_error"), MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            bool b;

            var c = MessageBox.Show((string)this.TryFindResource("m_doYouWantDeleteBooking"), (string)this.TryFindResource("m_confirmatioRequired"), MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (c == MessageBoxResult.No)
                return;

            using (NpgsqlCommand cmd = new NpgsqlCommand(deleteBooking, App.workConnection))
            {

                cmd.Parameters.AddWithValue("@book", books[currentBook]);

                b = (bool)cmd.ExecuteScalar();
            }

            if (!b)
            {
                MessageBox.Show((string)this.TryFindResource("m_errorNoBooking"), (string)this.TryFindResource("m_error"), MessageBoxButton.OK, MessageBoxImage.Error);
            }

            UpdateData();
        }

        private void StudentBookSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            currentBook = StudentBookSelector.SelectedIndex;
            book = StudentBookSelector.SelectedValue.ToString();
            UpdateLabels();
        }

        private void StudentTimeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            currentSlot = StudentTimeSelector.SelectedIndex;
            time = (string)StudentTimeSelector.SelectedValue;

            duration = null;// StudentBookContSelector.Items.Clear();
            UpdateDuration(true);
        }

        private void StudentDurationSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            currentDuration = StudentDurationSelector.SelectedIndex;
            duration = (string)StudentDurationSelector.SelectedValue;
            UpdateTimetable();
        }
    }
}
