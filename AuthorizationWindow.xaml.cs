using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
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
    /// Логика взаимодействия для AuthorizationWindow.xaml
    /// </summary>
    public partial class AuthorizationWindow : Window
    {
        string login;
        string password;
        List<int> BuildingsID = new List<int>();
        List<string> Buildings = new List<string>();
        UserCategories user;

        NpgsqlConnection nc;

        static readonly string chooseBuildingsQuery = "SELECT * FROM GetBuildingNameForLogin();";
        static readonly string checkResident = "SELECT CheckLoginResident(@login);";
        static readonly string checkAttendant = "SELECT CheckLoginAttendant(@login);";
        static readonly string checkAdmin = "SELECT CheckLoginAdmin(@login);";

        public AuthorizationWindow(UserCategories user)
        {
            InitializeComponent();

            this.user = user;

            nc = new NpgsqlConnection(App.GetConnectionString("unauthorized", "1234"));

            try
            {
                //Открываем соединение.

                nc.Open();
                //MessageBox.Show("Подключение", "Сообщение", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                if (nc.FullState == ConnectionState.Broken || nc.FullState == ConnectionState.Closed)
                {
                    throw new NpgsqlException();
                }
            }
            catch (NpgsqlException)
            {
                MessageBox.Show((string)this.TryFindResource("m_errorAccessDB"), (string)this.TryFindResource("m_error"), MessageBoxButton.OK);
            }

            if (this.user == UserCategories.Security)
            {
                AuthLoginLabel.Visibility = Visibility.Hidden;
                AuthLoginTextBox.Visibility = Visibility.Hidden;
                AuthCorpusLabel.Visibility = Visibility.Visible;
                AuthCorpusSelector.Visibility = Visibility.Visible;

                NpgsqlCommand cmd = new NpgsqlCommand(chooseBuildingsQuery, nc);

                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var s = ((int)reader["ID"]);
                        var name = ((string)reader[App.GetLocalisedKey("Name")]);
                        BuildingsID.Add(s);
                        Buildings.Add(name);
                    }
                }

                AuthCorpusSelector.ItemsSource = Buildings;
                AuthCorpusSelector.SelectedIndex = 0;
            }
            else
            {
                AuthCorpusLabel.Visibility = Visibility.Hidden;
                AuthCorpusSelector.Visibility = Visibility.Hidden;
                AuthLoginLabel.Visibility = Visibility.Visible;
                AuthLoginTextBox.Visibility = Visibility.Visible;
            }

            
        }

        //public delegate void SetWorkConnection(NpgsqlConnection newNC);
        //public event SetWorkConnection SetWorkConnectionEvent;

        private void AuthLoginTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            login = AuthLoginTextBox.Text;
        }

        private void AuthCorpusSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            login = BuildingsID[AuthCorpusSelector.SelectedIndex].ToString();
        }

        private void AuthCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void AuthAuthor_Click(object sender, RoutedEventArgs e)
        {
            bool b;

            NpgsqlCommand cmd=null;

            switch (user)
            {
                case UserCategories.Resident: cmd = new NpgsqlCommand(checkResident, nc); break;
                case UserCategories.Attendant: cmd = new NpgsqlCommand(checkAttendant, nc); break;
                case UserCategories.Administrator: cmd = new NpgsqlCommand(checkAdmin, nc); break;
                case UserCategories.Security: cmd = new NpgsqlCommand(checkAdmin, nc); break;
            }
            
            if (cmd == null)
            {
                return;
            }

            if (user != UserCategories.Security)
            {
                cmd.Parameters.AddWithValue("@login", login);
                b = (bool)cmd.ExecuteScalar();

                cmd.Dispose();

                if (!b)
                {
                    MessageBox.Show((string)this.TryFindResource("m_errorAuthorDB"), (string)this.TryFindResource("m_error"), MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            

            NpgsqlConnection wc;
            
            if (user != UserCategories.Security)
            {
                wc = new NpgsqlConnection(App.GetConnectionString(login, password));
            }
            else
            {
                wc = new NpgsqlConnection(App.GetConnectionString(App.GetSecurityLogin(login), password));
            }

            try
            {
                //Открываем соединение.

                wc.Open();
                //MessageBox.Show("Подключение", "Сообщение", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                if (wc.FullState == ConnectionState.Broken || wc.FullState == ConnectionState.Closed)
                {
                    throw new NpgsqlException();
                }

                App.workConnection = wc;
                App.login = login;
                //SetWorkConnectionEvent(wc);
                this.Close();
            }
            catch (NpgsqlException)
            {
                MessageBox.Show((string)this.TryFindResource("m_errorAuthorDB"), (string)this.TryFindResource("m_error"), MessageBoxButton.OK);
            }

        }

        private void AuthPasswordTextBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            password = AuthPasswordTextBox.Password;
        }
    }
}
