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
    /// Логика взаимодействия для BackupWindow.xaml
    /// </summary>
    public partial class BackupWindow : Window
    {
        public BackupWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            radioButton1.IsChecked = true;
            int lenghtConn=0;
            lenghtConn = ConfigurationManager.ConnectionStrings["WMS_KAMAZ.Properties.Settings.acsm_6fa30819604a3e7ConnectionString"].ToString().IndexOf(';');
            textBlock1.Text = "URL/IP: " + ConfigurationManager.ConnectionStrings["WMS_KAMAZ.Properties.Settings.acsm_6fa30819604a3e7ConnectionString"].ToString().Substring(0,lenghtConn);
            SetExport(true);
        }

        private string ConStr = ConfigurationManager.ConnectionStrings["WMS_KAMAZ.Properties.Settings.acsm_6fa30819604a3e7ConnectionString"].ToString();

        private void Export(string path)
        {
            MessageBox.Show("Экспорт в файл из базы");
            
            string constring = ConStr;
            string file = path;
            using (MySqlConnection conn = new MySqlConnection(constring))
            {
                using (MySqlCommand cmd = new MySqlCommand())
                {
                    using (MySqlBackup mb = new MySqlBackup(cmd))
                    {
                        cmd.Connection = conn;
                        conn.Open();
                        mb.ExportToFile(file);
                        conn.Close();
                    }
                }
            }
        }

        private void Import(string path)
        {
            MessageBox.Show("Импорт из файла");
            string constring = ConStr;
            string file = path;
            using (MySqlConnection conn = new MySqlConnection(constring))
            {
                using (MySqlCommand cmd = new MySqlCommand())
                {
                    using (MySqlBackup mb = new MySqlBackup(cmd))
                    {
                        cmd.Connection = conn;
                        conn.Open();
                        mb.ImportFromFile(file);
                        conn.Close();
                    }
                }
            }
        }

        private void buttonExportPath_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveDialog = new Microsoft.Win32.SaveFileDialog();
            saveDialog.DefaultExt = ".sql";
            saveDialog.Filter = "SQL (*.sql)|*.sql";
            saveDialog.FileName = "Backup WMS_KAMAZ (" + DateTime.Today.ToString("d")+")";
            
            Nullable<bool> result = saveDialog.ShowDialog();
            
            if (result == true)
            {
                string filename = saveDialog.FileName;
                textBoxExportPath.Text = filename;
            }
        }

        private void buttonImportPath_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openDialog = new Microsoft.Win32.OpenFileDialog();
            openDialog.DefaultExt = ".sql";
            openDialog.Filter = "SQL (*.sql)|*.sql";

            Nullable<bool> result = openDialog.ShowDialog();

            if (result == true)
            {
                string filename = openDialog.FileName;
                textBoxImportPath.Text = filename;
            }
        }

        private void buttonExport_Click(object sender, RoutedEventArgs e)
        {
            Export(textBoxExportPath.Text);
            MessageBox.Show("Экспорт завершен");
        }

        private void buttonImport_Click(object sender, RoutedEventArgs e)
        {
            Import(textBoxImportPath.Text);
            MessageBox.Show("Импорт завершен");
        }

        private void SetExport(bool flag)
        {
            if (flag)
            {
                textBlock1.IsEnabled = true;
                label2.IsEnabled = true;
                textBoxExportPath.IsEnabled = true;
                buttonExportPath.IsEnabled = true;
                buttonExport.IsEnabled = true;

                textBoxDBConnStr.IsEnabled = false;
                label3.IsEnabled = false;
                label4.IsEnabled = false;
                textBoxImportPath.IsEnabled = false;
                buttonImportPath.IsEnabled = false;
                buttonImport.IsEnabled = false;
            }
            else
            {
                textBlock1.IsEnabled = false;
                label2.IsEnabled = false;
                textBoxExportPath.IsEnabled = false;
                buttonExportPath.IsEnabled = false;
                buttonExport.IsEnabled = false;

                textBoxDBConnStr.IsEnabled = true;
                label3.IsEnabled = true;
                label4.IsEnabled = true;
                textBoxImportPath.IsEnabled = true;
                buttonImportPath.IsEnabled = true;
                buttonImport.IsEnabled = true;
            }


        }

        private void radioButton1_Checked(object sender, RoutedEventArgs e)
        {
            SetExport(true);
        }

        private void radioButton2_Checked(object sender, RoutedEventArgs e)
        {
            SetExport(false);
        }

    }
}
