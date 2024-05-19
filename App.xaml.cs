using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Navigation;
using static Kursovaya2.App;

namespace Kursovaya2
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        static public readonly string ipAddress = "localhost";
        static public readonly string port = "5432";
        static public readonly string dbName = "Kursovaya";

        static public NpgsqlConnection workConnection;
        static public string login;

        static public string GetTimeFromSlots(short slots, bool needToDecrease = true)
        {
            if (needToDecrease) slots--;
            return (slots / 4 < 10 ? "0" + (slots / 4).ToString() : (slots / 4).ToString()) + ":" + ((slots % 4) * 15 == 0 ? ((slots % 4) * 15).ToString() + "0" : ((slots % 4) * 15).ToString());
        }

        static public DateTime GetDateTimeFromSlots(short slots)
        {
            DateTime result;
            result = DateTime.Today;
            result = result.AddHours(slots / 4);
            result = result.AddMinutes((slots % 4) * 15);
            return result;
        }

        static public string GetConnectionString(string login, string password)
        {
            return "Host=" + ipAddress + ";Port=" + port + ";Username=" + login + ";Password=" + password + ";Database=" + dbName + ";";
        }

        static public string GetLocalisedKey(string s)
        {
            if (currentLanguage == 0)
            {
                return s + "RU";
            }
            else
            {
                return s + "EN";
            }
        }

        public class TimetablePart
        {
            public short start;
            public short end;

            public TimetablePart()
            {
                this.start=0;
                this.end = 0;
            }

            public TimetablePart(short start, short end)
            {
                this.start = start;
                this.end = end;
            }
        }

        static public string GetTimetable(List<TimetablePart> workTime)
        {
            char[] timetable = new char[96];
            for (int i = 0; i <96; i++)
            {
                timetable[i] = 'f';
            }

            for (int i =0;i<workTime.Count;i++)
            {
                for (int j = workTime[i].start; j <= workTime[i].end; j++)
                {
                    timetable[j] = 't';
                }
            }

            string res = "\'{";
            for (int i = 0; i < 95; i++)
            {
                res += timetable[i] + ", ";

            }
            res += timetable[95];
            res += "}\'";
            return res;
        }

        public delegate void NavigationRequest(Uri uri);


        private static List<CultureInfo> languages = new List<CultureInfo>();

        public static short GetNextLanguage()
        {
            currentLanguage++;
            currentLanguage%=(short)languages.Count;
            return currentLanguage;
        }

        private static short currentLanguage;

        public static List<CultureInfo> Languages
        {
            get
            {
                return languages;
            }
            
        }
        public delegate void LangChandgeExample();

        public static event LangChandgeExample LanguageChanged;
        public static CultureInfo Language
        {
            get
            {
                return System.Threading.Thread.CurrentThread.CurrentUICulture;
            }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                //if (value == System.Threading.Thread.CurrentThread.CurrentUICulture) return;

                //1. Меняем язык приложения:
                System.Threading.Thread.CurrentThread.CurrentUICulture = value;

                //2. Создаём ResourceDictionary для новой культуры
                ResourceDictionary dict = new ResourceDictionary();
                switch (value.Name)
                {
                    case "en-US":
                        dict.Source = new Uri("langEnglish.xaml", UriKind.Relative);
                        
                        break;
                    default:
                        dict.Source = new Uri("langRussian.xaml", UriKind.Relative);
                        break;
                }

                //3. Находим старую ResourceDictionary и удаляем его и добавляем новую ResourceDictionary
                ResourceDictionary oldDict = (from d in Application.Current.Resources.MergedDictionaries
                                              where d.Source != null && d.Source.OriginalString.StartsWith("lang")
                                              select d).First();
                if (oldDict != null)
                {
                    int ind = Application.Current.Resources.MergedDictionaries.IndexOf(oldDict);
                    Application.Current.Resources.MergedDictionaries.Remove(oldDict);
                    Application.Current.Resources.MergedDictionaries.Insert(ind, dict);
                }
                else
                {
                    Application.Current.Resources.MergedDictionaries.Add(dict);
                }

                LanguageChanged();
            }
        }

        void AppOnLangChange()
        {

        }

        App()
        {
            languages.Clear();
            languages.Add(new CultureInfo("ru-RU"));
            languages.Add(new CultureInfo("en-US"));
            currentLanguage = 0;

            LanguageChanged += AppOnLangChange;
        }

        public delegate void ChangeQuitGoBack(bool isQuit);

        public static List<T> GetVisualChildCollection<T>(object parent) where T : Visual
        {
            List<T> visualCollection = new List<T>();
            GetVisualChildCollection(parent as DependencyObject, visualCollection);
            return visualCollection;
        }

        private static void GetVisualChildCollection<T>(DependencyObject parent, List<T> visualCollection) where T : Visual
        {
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is T)
                {
                    visualCollection.Add(child as T);
                }
                else if (child != null)
                {
                    GetVisualChildCollection(child, visualCollection);
                }
            }
        }

        public class ResidentInf
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public string Login { get; set; }
            public string Surname { get; set; }
            public string Patronymic { get; set; }
            public string Room { get; set; }
            public string Building { get; set; }
            public bool Status { get; set; }
            public string StatusText { get; set; }
            public string StudentCard { get; set; }

        }

        public class AttendantInf
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public string Login { get; set; }
            public string Surname { get; set; }
            public string Patronymic { get; set; }
            public List<int> BuildingsID { get; set; }
            public List<string> BuildingNames { get; set; }
        }

        public class AdministratorInfo
        {
            public int ID { get; set; }
            public string Login { get; set; }
            public string Name { get; set; }
            public string Surname { get; set; }
            public string Patronymic { get; set; }
        }

        public class SecurityInfo
        {
            public int ID { get; set; }
            public string Name { get; set; }
        }

        public static string GetSecurityLogin(string BuildingID)
        {
            return "security_" + BuildingID;
        }

        static public string TimeSpanToString(TimeSpan t)
        {
            string s;
            if (t.Hours < 10) s = "0" + t.Hours;
            else s = t.Hours.ToString();
            s += ":";
            if (t.Minutes < 10) s += "0" + t.Minutes;
            else s += t.Minutes.ToString();
            return s;
        }

        public static UserCategories forAdminRedact;
    }

    public partial interface IPageN
    {
        event NavigationRequest NavigationRequested;

        void SubscribeMainWindow();
        void UnsubscribeMainWindow();
        void ReturnBack();
        void UpdateOnLangChange();
        bool IsStart
        {
            get;
        }
        void OnNavigatingFrom(object sender, NavigatingCancelEventArgs e);
    }


    public enum UserCategories
    {
        Resident, Attendant, Security, Administrator
    }

    public class AllForFilters
    {
        public class FilterList
        {
            public List<string> Values { get; set; }
            public List<bool> IsEnabled { get; set; }
            public FilterList()
            {
                Values = new List<string>();
                IsEnabled = new List<bool>();
            }
        }

        static public void DoFilter<T>(ObservableCollection<T> list, DataGrid table, PassFunctionT func, FilterList[] filters, int numberOfFilters)
        {
            var sourceList = new CollectionViewSource() { Source = list };

            ICollectionView viewList = sourceList.View;

            var filterFunc = new Predicate<object>(r => IsPassFilter<T>((T)r, func, filters, numberOfFilters));

            viewList.Filter = filterFunc;

            table.ItemsSource = viewList;
        }

        abstract public class PassFunctionT
        {
            abstract public bool Pass<T>(T row, FilterList filter, int index);
        }

        static bool IsPassFilter<T>(T row, PassFunctionT func, FilterList[] filters, int numberOfFilters)
        {
            for (int i = 0; i < numberOfFilters; i++)
            {
                if (!func.Pass<T>(row, filters[i], i)) return false;
            }
            return true;
        }

        static public void UpdateFilterFunc(object sender, FilterList[] fl)
        {
            int index = -1;

            ListBox parent = null;

            for (var v = sender as Visual; v != null; v = VisualTreeHelper.GetParent(v) as Visual)
            {
                if (v is ListBox)
                {
                    parent = v as ListBox;

                    string s = parent.Name.Substring(4);

                    index = Int32.Parse(s);

                    break;
                }
            }

            if (index == -1) return;

            for (int i = 0; i < parent.Items.Count; i++)
            {
                fl[index].IsEnabled[i] = (bool)(parent.Items[i] as CheckBox).IsChecked;
            }
        }

        public static List<CheckBox> FilterListToCheckBoxConverter(FilterList l, RoutedEventHandler rr)
        {
            List<CheckBox> result = new List<CheckBox>();

            for (int i = 0; i < l.Values.Count; i++)
            {
                CheckBox cb = new CheckBox();
                cb.Content = l.Values[i];
                cb.IsChecked = l.IsEnabled[i];

                cb.Checked += rr;
                cb.Unchecked += rr;

                result.Add(cb);
            }

            return result;
        }

        public static void BindingPopupOnButton(DataGrid table)
        {
            var ll = GetVisualChildCollection<DataGridColumnHeader>(table);

            for (int i = 0; i < ll.Count; i++)
            {
                if (ll[i].Content == null) continue;

                var template = ll[i].Template;
                ToggleButton tt = (ToggleButton)template.FindName("HeaderButton", ll[i]);
                Popup pp = (Popup)template.FindName("HeaderFilter", ll[i]);

                Binding bind = new Binding();
                bind.Source = tt;
                bind.Path = new PropertyPath("IsChecked");
                bind.Mode = BindingMode.TwoWay;
                bind.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                pp.SetBinding(Popup.IsOpenProperty, bind);
            }
        }

        public static void SetFilterPopupsOnTable(FilterList[] fl, DataGrid table, int numberOfFilters, RoutedEventHandler rr)
        {
            var ll = GetVisualChildCollection<DataGridColumnHeader>(table);

            for (int i = 0; i < numberOfFilters; i++)
            {
                ControlTemplate template = ll[i + 1].Template;
                Popup pp = (Popup)template.FindName("HeaderFilter", ll[i + 1]);

                var filterList = AllForFilters.FilterListToCheckBoxConverter(fl[i], rr);

                ListBox lll = new ListBox();
                lll.Name = "list" + i.ToString();
                lll.ItemsSource = filterList;

                pp.Child = lll;
            }
        }
    }

    public class ForAdminInfo
    {
        public interface IAdminInfo
        {
            int ID { get; set; }

            string Login { get; set; }

            string Password { get; set; }
        }

        public class AdmResidentInfo : IAdminInfo
        {
            public int ID { get; set; }
            public string Login { get; set; }
            public string Password { get; set; }
            public string Building { get; set; }
            public string Name { get; set; }
            public string Surname { get; set; }
            public string Patronymic { get; set; }
            public string Room { get; set; }
            public string StatusText { get; set; }
            public string StudentCard { get; set; }

        }

        public class AdmAttendantInfo : IAdminInfo
        {
            public int ID { get; set; }
            public string Login { get; set; }
            public string Password { get; set; }
            public string Name { get; set; }
            public string Surname { get; set; }
            public string Patronymic { get; set; }

            public NpgsqlRange<int> buildings;
        }

        public class AdmAdministratorInfo : IAdminInfo
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public string Surname { get; set; }
            public string Patronymic { get; set; }
            public string Login { get; set; }
            public string Password { get; set; }
        }

        public class AdmSecurityInfo : IAdminInfo
        {
            public int ID { get; set; }
            public string Login { get; set; }
            public string Password { get; set; }
            public string NameRU { get; set; }
            public string NameEN { get; set; }
        }
    }
}
