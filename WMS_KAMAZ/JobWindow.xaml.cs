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
    /// Логика взаимодействия для JobWindow.xaml
    /// </summary>
    public partial class JobWindow : Window
    {
        MySqlConnection conn;

        public JobWindow()
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
                taDolgnost.Connection = conn;
            }
            DataContext = dsKamaz;
        }

        WMS_Kamaz_ds dsKamaz = new WMS_Kamaz_ds();

        Binding b1, b2;

        WMS_Kamaz_dsTableAdapters.тбдолжностьTableAdapter taDolgnost = new WMS_Kamaz_dsTableAdapters.тбдолжностьTableAdapter();

        private void Wms_KamazFill()
        {
            try
            {
                taDolgnost.Fill(dsKamaz.тбдолжность);
            }
            catch (Exception ex) { MessageBox.Show("Ошибка в работе метода Fill DataAdapter: " + ex); }
        }

        private void Undo()
        {
            dsKamaz.тбдолжность.RejectChanges();
            SetEditing(false);
        }

        private void New() // COMBOBOX - КОД ПОДРАЗДЕЛЕНИЯ НЕ РОБИТ !
        {
            SetEditing(true);

            int kodDolg = listBox1.Items.Count +1;

            DataRow rowDolgnost = this.dsKamaz.тбдолжность.NewтбдолжностьRow();
            rowDolgnost["Должность"] = "Новая";
            rowDolgnost["Оклад"] = 0;
            dsKamaz.тбдолжность.Rows.Add(rowDolgnost);

            listBox1.SelectedIndex = listBox1.Items.Count - 1;
        }

        private void Edit()
        {
            //listBox1.SelectedIndex = listBox1.Items.Count;
            SetEditing(true);
        }

        private void Save()
        {
            SetEditing(false);

            //int pos = this.dsKamaz.тбсклад.Rows.Count - 1;
            //this.BindingContext[dsKamaz, "тперсонал.ПаспДанПерс"].Position = pos;
            WMS_Kamaz_ds.тбдолжностьDataTable ds2 = (WMS_Kamaz_ds.тбдолжностьDataTable)dsKamaz.тбдолжность.GetChanges(DataRowState.Added);

            if (ds2 != null)
                try
                {
                    taDolgnost.Update(ds2);
                    ds2.Dispose();
                    dsKamaz.тбдолжность.AcceptChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка вставки записи в базу данных wms_kamaz " + ex, "Предупреждение");
                    this.dsKamaz.тбдолжность.RejectChanges();
                }

            WMS_Kamaz_ds.тбдолжностьDataTable ds3 = (WMS_Kamaz_ds.тбдолжностьDataTable)dsKamaz.тбдолжность.GetChanges(DataRowState.Modified);

            if (ds3 != null)
                try
                {
                    taDolgnost.Update(ds3);
                    ds3.Dispose();
                    dsKamaz.тбдолжность.AcceptChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка изменения записи в базе данных wms_kamaz " + ex, "Предупреждение");
                    this.dsKamaz.тбдолжность.RejectChanges();
                }

        }

        private void Delete()
        {
            int pos = -1;
            pos = Convert.ToInt32(listBox1.SelectedIndex); 

            string mes = listBox1.SelectedValue.ToString();

            SetEditing(false);

            MessageBoxResult result = MessageBox.Show(" Удалить данные по должности " + mes + "?", "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    {
                        //MessageBox.Show("Удаление данных");
                        this.dsKamaz.тбдолжность.Rows[pos].Delete();
                        if (this.dsKamaz.тбдолжность.GetChanges(DataRowState.Deleted) != null)
                        {
                            try
                            {
                                this.taDolgnost.Update(dsKamaz.тбдолжность);
                                this.dsKamaz.тбдолжность.AcceptChanges();
                            }
                            catch (Exception x)
                            {
                                string er = x.Message.ToString();
                                MessageBox.Show("Ошибка удаления записи в таблице тбдолжность " + er, "Предупреждение");
                                this.dsKamaz.тбдолжность.RejectChanges();
                            }
                        }
                        break;
                    }
                case MessageBoxResult.No:
                    {
                        //MessageBox.Show("Отмена удаления данных");
                        this.dsKamaz.тбдолжность.RejectChanges();
                        break;
                    }
            }
            listBox1.SelectedIndex = 0;
        }

        private void SetEditing(bool flag)
        {
            if (flag)
            {
                listBox1.IsEnabled = false;
                textBox1.Focus();

                textBox1.IsReadOnly = false;
                textBox2.IsReadOnly = false;

                UndoButton.IsEnabled = true;
                NewButton.IsEnabled = false;
                EditButton.IsEnabled = false;
                SaveButton.IsEnabled = true;
                DeleteButton.IsEnabled = false;
            }
            else
            {
                listBox1.IsEnabled = true;
                listBox1.Focus();

                textBox1.IsReadOnly = true;
                textBox2.IsReadOnly = true;

                UndoButton.IsEnabled = false;
                NewButton.IsEnabled = true;
                EditButton.IsEnabled = true;
                SaveButton.IsEnabled = false;
                DeleteButton.IsEnabled = true;
            }
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Wms_KamazFill();

            SetEditing(false);

            listBox1.SelectedIndex = 0;
        }


        private void listBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
            {
                
                b1 = new Binding();
                b1.Source = dsKamaz.тбдолжность;
                b1.Path = new PropertyPath("Rows[" + listBox1.SelectedIndex + "][Должность]");
                BindingOperations.SetBinding(textBox1, TextBox.TextProperty, b1);

                b2 = new Binding();
                b2.Source = dsKamaz.тбдолжность;
                b2.Path = new PropertyPath("Rows[" + listBox1.SelectedIndex + "][Оклад]");
                BindingOperations.SetBinding(textBox2, TextBox.TextProperty, b2);
            }
        }

        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            DataRow dr = dsKamaz.тбдолжность[listBox1.SelectedIndex];
            dr["Должность"] = textBox1.Text;
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

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            Undo();
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            New();
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            Edit();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            Delete();
        }

    }
}
