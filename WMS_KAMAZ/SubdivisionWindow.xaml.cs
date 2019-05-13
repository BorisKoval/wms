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
    /// Логика взаимодействия для SubdivisionWindow.xaml
    /// </summary>
    public partial class SubdivisionWindow : Window
    {
        MySqlConnection conn;

        public SubdivisionWindow()
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

        WMS_Kamaz_ds dsKamaz = new WMS_Kamaz_ds();

        Binding b1, b2, b3, b4, b5, b6, b7, b8;

        WMS_Kamaz_dsTableAdapters.тбподразделениеTableAdapter taPodrazd = new WMS_Kamaz_dsTableAdapters.тбподразделениеTableAdapter();

        private void Wms_KamazFill()
        {
            try
            {
                taPodrazd.Fill(dsKamaz.тбподразделение);
            }
            catch (Exception ex) { MessageBox.Show("Ошибка в работе метода Fill DataAdapter: " + ex); }
        }

        private void Undo()
        {
            dsKamaz.тбподразделение.RejectChanges();
            SetEditing(false);
        }

        private void New() // COMBOBOX - КОД ПОДРАЗДЕЛЕНИЯ НЕ РОБИТ !
        {
            SetEditing(true);

            DataRowView drv = (DataRowView)listBox1.Items[listBox1.Items.Count - 1];
            int kodPodrazd = Convert.ToInt32(drv[0]);

            DataRow rowPodrazd = this.dsKamaz.тбподразделение.NewтбподразделениеRow();
            rowPodrazd["Код_подразделения"] = kodPodrazd + 1;
            rowPodrazd["Наименование"] = "";
            dsKamaz.тбподразделение.Rows.Add(rowPodrazd);

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

            WMS_Kamaz_ds.тбподразделениеDataTable ds2 = (WMS_Kamaz_ds.тбподразделениеDataTable)dsKamaz.тбподразделение.GetChanges(DataRowState.Added);

            if (ds2 != null)
                try
                {
                    taPodrazd.Update(ds2);
                    ds2.Dispose();
                    dsKamaz.тбподразделение.AcceptChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка вставки записи в базу данных wms_kamaz " + ex, "Предупреждение");
                    this.dsKamaz.тбподразделение.RejectChanges();
                }

            WMS_Kamaz_ds.тбподразделениеDataTable ds3 = (WMS_Kamaz_ds.тбподразделениеDataTable)dsKamaz.тбподразделение.GetChanges(DataRowState.Modified);

            if (ds3 != null)
                try
                {
                    taPodrazd.Update(ds3);
                    ds3.Dispose();
                    dsKamaz.тбподразделение.AcceptChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка изменения записи в базе данных wms_kamaz " + ex, "Предупреждение");
                    this.dsKamaz.тбподразделение.RejectChanges();
                }

        }

        private void Delete()
        {
            int pos = -1;
            pos = Convert.ToInt32(listBox1.SelectedIndex);

            string mes = listBox1.SelectedValue.ToString();

            SetEditing(false);

            MessageBoxResult result = MessageBox.Show("Удалить данные по подразделению " + mes + "?", "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    {
                        //MessageBox.Show("Удаление данных");
                        this.dsKamaz.тбподразделение.Rows[pos].Delete();
                        if (this.dsKamaz.тбподразделение.GetChanges(DataRowState.Deleted) != null)
                        {
                            try
                            {
                                this.taPodrazd.Update(dsKamaz.тбподразделение);
                                this.dsKamaz.тбподразделение.AcceptChanges();
                            }
                            catch (Exception x)
                            {
                                string er = x.Message.ToString();
                                MessageBox.Show("Ошибка удаления записи в таблице тбподразделение " + er, "Предупреждение");
                                this.dsKamaz.тбподразделение.RejectChanges();
                            }
                        }
                        break;
                    }
                case MessageBoxResult.No:
                    {
                        //MessageBox.Show("Отмена удаления данных");
                        this.dsKamaz.тбподразделение.RejectChanges();
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
                b1.Source = dsKamaz.тбподразделение;
                b1.Path = new PropertyPath("Rows[" + listBox1.SelectedIndex + "][Наименование]");
                BindingOperations.SetBinding(textBox1, TextBox.TextProperty, b1);

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
