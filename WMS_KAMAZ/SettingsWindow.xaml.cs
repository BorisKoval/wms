using System;
using System.Collections.Generic;
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
using MySql.Data;
using MySql.Data.MySqlClient;
using MySql.Data.Entity;
using System.Data;
using System.Threading;
using System.Configuration;

namespace WMS_KAMAZ
{
    /// <summary>
    /// Логика взаимодействия для SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        MySqlConnection conn;

        public SettingsWindow()
        {
            InitializeComponent();

            conn = GetDatabaseConnection("WMS_KAMAZ.Properties.Settings.acsm_6fa30819604a3e7ConnectionString");
            if (conn == null)
            {
                MessageBox.Show("Возможно нет соеденения с Интернет !");
                Application.Current.Shutdown();
            }
            else
            {
                taPodrazd.Connection = conn;
            }
            DataContext = dsKamaz;
        }

        static string IP = "127.0.0.1";
        static string Login = "root";
        static string Pass = "12345";
        static string Database = "wms_kamaz";

        private string defaultConnStr = string.Format("server={0};uid={1};pwd={2};database={3};", IP, Login, Pass, Database);

        string currentConnStr = WMS_KAMAZ.Properties.Settings.Default.acsm_6fa30819604a3e7ConnectionString;

        WMS_Kamaz_ds dsKamaz = new WMS_Kamaz_ds();

        WMS_Kamaz_dsTableAdapters.тбподразделениеTableAdapter taPodrazd = new WMS_Kamaz_dsTableAdapters.тбподразделениеTableAdapter();

        private void Wms_KamazFill()
        {
            try
            {
                taPodrazd.Fill(dsKamaz.тбподразделение);
            }
            catch (Exception ex) { MessageBox.Show("Ошибка в работе метода Fill DataAdapter: " + ex); }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Wms_KamazFill();
            comboBox1.SelectedIndex = 0;

            textBoxCurrentConnstr.Text = currentConnStr;
            Editing(false);
        }

        private void buttonEditConnStr_Click(object sender, RoutedEventArgs e)
        {
            Editing(true);
        }


        private void Editing(bool flag)
        {
            if (flag)
            {
                label3.IsEnabled = true;
                label4.IsEnabled = true;
                label5.IsEnabled = true;
                label6.IsEnabled = true;
                textBox1.IsEnabled = true;
                textBox2.IsEnabled = true;
                textBox3.IsEnabled = true;
                textBox4.IsEnabled = true;
                buttonSaveConnStr.IsEnabled = true;
                buttonEditConnStr.IsEnabled = false;
            }
            else
            {
                label3.IsEnabled = false;
                label4.IsEnabled = false;
                label5.IsEnabled = false;
                label6.IsEnabled = false;
                textBox1.IsEnabled = false;
                textBox2.IsEnabled = false;
                textBox3.IsEnabled = false;
                textBox4.IsEnabled = false;
                buttonSaveConnStr.IsEnabled = false;
                buttonEditConnStr.IsEnabled = true;
            }
        }

        private void buttonSaveConnStr_Click(object sender, RoutedEventArgs e)
        {
            if (textBox1.Text != "" && textBox2.Text != "" && textBox3.Text != "" && textBox4.Text != "")
            {
                string ConnStr = string.Format("server={0};uid={1};persistsecurityinfo=True;database={2};pwd={3}", textBox1.Text, textBox3.Text, textBox2.Text, textBox4.Text);
                
                System.Configuration.Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                config.ConnectionStrings.ConnectionStrings[1].ConnectionString = ConnStr;
                config.ConnectionStrings.ConnectionStrings[1].Name = "WMS_KAMAZ.Properties.Settings.acsm_6fa30819604a3e7ConnectionString";
                config.ConnectionStrings.ConnectionStrings[1].ProviderName = "MySql.Data.MySqlClient";
                config.Save(ConfigurationSaveMode.Full, true);
                ConfigurationManager.RefreshSection("connectionStrings");

                Editing(false);
            }
        }

        private MySqlConnection GetDatabaseConnection(string name)
        {

            MySqlConnection conn = null;
            ConnectionStringSettings setting = ConfigurationManager.ConnectionStrings[name];
            if (setting != null)
                try
                {
                    conn = new MySqlConnection(setting.ConnectionString);
                    /// Проверка соединения с сервером базы данных
                    conn.Open();
                    conn.Close();
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("Прерывание при соединении с базой данных:\n\n" + ex + " Проверьте строку соединения в конфигурационном файле", "Предупреждение!", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                    conn = null;
                }
            else
            {
                MessageBox.Show("Отсутствует соединение с базой данных \n\n Проверьте имя строки соединения в конфигурационном файле",
              "Предупреждение!", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            }
            return conn;
        }

        private void buttonEditPodrazd_Click(object sender, RoutedEventArgs e)
        {

        }

        private void buttonSavePodrazd_Click(object sender, RoutedEventArgs e)
        {
            if (comboBox1.SelectedValue !=null)
            {

                Configuration con = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                
                //int asd = con.AppSettings.Properties.Settings.Count;
                //con.AppSettings.Settings["DefaultPodrazd"].Value = comboBox1.SelectedIndex.ToString();
                //con.Save();
            }
        }
    }
}
