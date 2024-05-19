using Npgsql;
using Org.BouncyCastle.Asn1.Ocsp;
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
using System.Windows.Shapes;

namespace Kursovaya2
{
    /// <summary>
    /// Логика взаимодействия для AttendantNewRequestWindow.xaml
    /// </summary>
    public partial class AttendantNewRequestWindow : Window
    {
        static readonly string selectThemes = "SELECT \"ID\", \"{0}\" FROM \"Request_themes\" WHERE \"ID\" != 0;";
        static readonly string select0ThemeName = "SELECT \"{0}\" AS \"Name\" FROM \"Request_themes\" WHERE \"ID\" = 0;";
        static readonly string selectLoginID = "SELECT \"ID\" FROM \"People\" WHERE \"Login\" = @login LIMIT 1;";
        static readonly string insertNewRequest = "CALL insertrequest( @loginID, @text, @theme);";

        ObservableCollection<string> themesList;
        ObservableCollection<int> themesID;

        int currentTheme=-1;
        string message="";

        public AttendantNewRequestWindow()
        {
            InitializeComponent();

            LoadThemes();
        }

        bool isSaved = false;

        void LoadThemes()
        {
            themesID = new ObservableCollection<int>();
            themesList = new ObservableCollection<string>();

            themesID.Add(0);

            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(select0ThemeName, App.GetLocalisedKey("Name")), App.workConnection))
            {
                themesList.Add((string)cmd.ExecuteScalar());
            }

            using (NpgsqlCommand cmd = new NpgsqlCommand(string.Format(selectThemes, App.GetLocalisedKey("Name")), App.workConnection))
            {
                cmd.Parameters.AddWithValue("@login", App.login);
                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var id = (int)reader["ID"];
                        var name = (string)reader["Name"];

                        themesID.Add(id);
                        themesList.Add(name);
                    }
                }
            }

            AttendantRequestsThemeSelector.ItemsSource = themesList;

            App.LanguageChanged += UpdateOnLangChange;
            this.Closing += OnClose;

            isSaved = true;
        }

        void UpdateOnLangChange()
        {
            LoadThemes();
        }

        void AddNewRequest()
        {
            int loginID;

            using (NpgsqlCommand cmd = new NpgsqlCommand(selectLoginID, App.workConnection))
            {
                cmd.Parameters.AddWithValue("@login", App.login);

                loginID = (int)cmd.ExecuteScalar();
            }

            using (NpgsqlCommand cmd = new NpgsqlCommand(insertNewRequest, App.workConnection))
            {
                cmd.Parameters.AddWithValue("@loginID", loginID);
                cmd.Parameters.AddWithValue("@text", message);
                cmd.Parameters.AddWithValue("@theme", themesID[currentTheme]);

                cmd.ExecuteNonQuery();
            }
        }

        void OnClose(object sender, CancelEventArgs e)
        {
            if (!isSaved)
            {
                var result = MessageBox.Show((string)TryFindResource("m_AttendantRequestsOnCloseNew"), (string)TryFindResource("m_confirmatioRequired"), MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation);

                if (result == MessageBoxResult.Yes)
                {
                    AddNewRequest();
                    return;
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
            }

            e.Cancel = false;

            App.LanguageChanged -= UpdateOnLangChange;
        }

        private void AttendantSaveRequestButton_Click(object sender, RoutedEventArgs e)
        {
            isSaved = true;
            AddNewRequest();
            MessageBox.Show((string)TryFindResource("m_AttendantRequestSended"), (string)TryFindResource("m_success"), MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }

        private void AttendantCancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void AttendantRequestsThemeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            currentTheme = AttendantRequestsThemeSelector.SelectedIndex;
        }

        private void AttendantRequestText_TextChanged(object sender, TextChangedEventArgs e)
        {
            AttendantRequestWriteHere.Visibility = Visibility.Hidden;

            message = AttendantRequestText.Text;
        }
    }
}
