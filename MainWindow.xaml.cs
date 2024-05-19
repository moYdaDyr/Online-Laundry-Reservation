using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Policy;
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
using static Kursovaya2.App;

namespace Kursovaya2
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public static RoutedCommand F1Command = new RoutedCommand();

        public MainWindow()
        {
            InitializeComponent();

            this.Icon = new BitmapImage(new Uri("icon.ico", UriKind.Relative));

            CommandBinding F1Binding = new CommandBinding();

            F1Binding.Command = F1Command;

            F1Binding.Executed += HelpButton_Click;

            F1Command.InputGestures.Add(new KeyGesture(Key.F1, ModifierKeys.Control));

            CommandBindings.Add(F1Binding);

            MainMenuFrame.Navigate(new Uri("StartPage.xaml", UriKind.Relative));

            this.Closing += OnClose;
        }

        public void NavigateFunc(Uri uri)
        {
            
            MainMenuFrame.Navigate(uri);

            

            MainMenuFrame.ContentRendered += OnNewPageLoaded;
        }

        private void OnNewPageLoaded(object sender, EventArgs e)
        {
            var p = MainMenuFrame.Content as IPageN;

            if (p.IsStart)
            {
                GoBack_Button.Visibility = Visibility.Hidden;
                GoQuit_Button.Visibility = Visibility.Visible;

                App.login = string.Empty;

                if (workConnection != null)
                {
                    App.workConnection.Close();
                }

                workConnection = null;
            }
            else
            {
                GoBack_Button.Visibility = Visibility.Visible;
                GoQuit_Button.Visibility = Visibility.Hidden;
            }

            MainMenuFrame.ContentRendered -= OnNewPageLoaded;
        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            var p = MainMenuFrame.Content as IPageN;
            p.ReturnBack();
        }

        private void GoQuit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public string GetWeekDayName(short day)
        {
            return (string)TryFindResource("m_DayWeek" + day);
        }

        private void ChangeLanguage_Click(object sender, RoutedEventArgs e)
        {
            App.Language = App.Languages[App.GetNextLanguage()];
            
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            string helpPath = "help\\"+GetLocalisedKey("help") + ".chm";
            Process.Start(helpPath);
        }

        private void OnClose(object sender, CancelEventArgs e)
        {
            var a = MessageBox.Show((string)TryFindResource("m_exitConfirmation"), (string)TryFindResource("m_confirmatioRequired"),MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (a == MessageBoxResult.No)
            {
                e.Cancel = true;
                return;
            }

            foreach (Window w in this.OwnedWindows)
            {
                w.Close();
            }
        }
    }
}
