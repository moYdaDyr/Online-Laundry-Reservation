using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
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

namespace Kursovaya2
{
    /// <summary>
    /// Логика взаимодействия для StartPage.xaml
    /// </summary>
    public partial class StartPage : Page, IPageN
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

        public bool IsStart
        {
            get
            {
                return true;
            }
        }

        public void ReturnBack()
        {
            return;
        }

        public static NpgsqlConnection nc;

        public NpgsqlCommand cmd;

        public void OnNavigatingFrom(object sender, NavigatingCancelEventArgs e)
        {
            App.LanguageChanged -= UpdateOnLangChange;
            UnsubscribeMainWindow();
        }

        public void UpdateOnLangChange()
        {

        }

        public StartPage()
        {
            InitializeComponent();

            App.LanguageChanged += UpdateOnLangChange;


            /*
            nc = new NpgsqlConnection(App.GetTestConnectionString());
            try
            {
                nc.Open();

                if (nc.FullState == ConnectionState.Broken || nc.FullState == ConnectionState.Closed)
                {
                    throw new NpgsqlException();
                }
            }
            catch (NpgsqlException)
            {
                MessageBox.Show("Не удалось подключится к базе данных!", "Ошибка", MessageBoxButton.OK);
            }

            */
        }

        private void ResidentAuthorization_Click(object sender, RoutedEventArgs e)
        {
            SubscribeMainWindow();

            var aw = new AuthorizationWindow(UserCategories.Resident);
            aw.ShowDialog();

            if (workConnection != null)
            {
                NavigationRequested(new Uri("StudentMenuPage.xaml", UriKind.Relative));
            }
        }

        private void AttendantAuthorization_Click(object sender, RoutedEventArgs e)
        {
            SubscribeMainWindow();

            var aw = new AuthorizationWindow(UserCategories.Attendant);
            aw.ShowDialog();

            if (workConnection != null)
            {
                NavigationRequested(new Uri("AttendantMenuPage.xaml", UriKind.Relative));
            }
        }

        private void SecurityAuthorization_Click(object sender, RoutedEventArgs e)
        {
            SubscribeMainWindow();

            var aw = new AuthorizationWindow(UserCategories.Security);
            aw.ShowDialog();

            if (workConnection != null)
            {
                NavigationRequested(new Uri("SecurityPage.xaml", UriKind.Relative));
            }
        }

        private void AdministratorAuthorization_Click(object sender, RoutedEventArgs e)
        {
            SubscribeMainWindow();

            var aw = new AuthorizationWindow(UserCategories.Administrator);
            aw.ShowDialog();

            if (workConnection != null)
            {
                NavigationRequested(new Uri("AdministratorMenuPage.xaml", UriKind.Relative));
            }
        }

        public void SetWorkConnection(NpgsqlConnection newNC)
        {
            workConnection = newNC;
        }
    }
}
