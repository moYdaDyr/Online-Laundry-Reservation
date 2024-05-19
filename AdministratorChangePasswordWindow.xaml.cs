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
using System.Windows.Shapes;

namespace Kursovaya2
{
    /// <summary>
    /// Логика взаимодействия для AdministratorChangePasswordWindow.xaml
    /// </summary>
    public partial class AdministratorChangePasswordWindow : Window
    {
        static readonly string updatePassword = "ALTER USER @login PASSORD @password;";

        public AdministratorChangePasswordWindow(string login)
        {
            InitializeComponent();
            log = login;
            newPassword = "";
        }

        string log;
        string newPassword;

        private void AdministratorChangePasswordBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            newPassword = AdministratorChangePasswordBox.Text;
        }

        private void AdministratorChangePasswordCancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void AdministratorChangePasswordDoneButton_Click(object sender, RoutedEventArgs e)
        {
            if (newPassword.Length == 0)
            {
                MessageBox.Show((string)TryFindResource("m_errorZeroLength"), (string)TryFindResource("m_error"), MessageBoxButton.OK,MessageBoxImage.Error);
                return;
            }
            
            using (NpgsqlCommand cmd = new NpgsqlCommand(updatePassword, App.workConnection))
            {
                cmd.Parameters.AddWithValue("@login", log);
                cmd.Parameters.AddWithValue("@password", newPassword);

                cmd.ExecuteNonQuery();
            }
            

            MessageBox.Show((string)TryFindResource("m_successPassword"), (string)TryFindResource("m_success"), MessageBoxButton.OK, MessageBoxImage.Information);

            this.Close();
        }
    }
}
