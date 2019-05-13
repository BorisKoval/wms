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
    /// Логика взаимодействия для WarehouseWindow.xaml
    /// </summary>
    public partial class WarehouseWindow : Window
    {
        MySqlConnection conn;

        public WarehouseWindow()
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
                taSklad.Connection = conn;
                taPodrazdelenie.Connection = conn;
            }
            DataContext = dsKamaz;
        }

        WMS_Kamaz_ds dsKamaz = new WMS_Kamaz_ds();

        Binding b1, b2, b3, b4, b5, b6, b7, b8;

        WMS_Kamaz_dsTableAdapters.тбскладTableAdapter taSklad = new WMS_Kamaz_dsTableAdapters.тбскладTableAdapter();

        WMS_Kamaz_dsTableAdapters.тбподразделениеTableAdapter taPodrazdelenie = new WMS_Kamaz_dsTableAdapters.тбподразделениеTableAdapter();

        private void Wms_KamazFill()
        {
            try
            {
                taSklad.Fill(dsKamaz.тбсклад);
                taPodrazdelenie.Fill(dsKamaz.тбподразделение);
            }
            catch (Exception ex) { MessageBox.Show("Ошибка в работе метода Fill DataAdapter: " + ex); }
        }

        private void Undo()
        {
            dsKamaz.тбсклад.RejectChanges();
            SetEditing(false);
        }

        private void New() // COMBOBOX - КОД ПОДРАЗДЕЛЕНИЯ НЕ РОБИТ !
        {
            SetEditing(true);

            DataRowView drv = (DataRowView)listBox1.Items[listBox1.Items.Count - 1];
            int kodSklada = Convert.ToInt32(drv[0]);

            DataRow rowSklad = this.dsKamaz.тбсклад.NewтбскладRow();
            rowSklad["Номер_склада"] = kodSklada + 1;
            rowSklad["Код_подразделения"] = Convert.ToInt32(comboBox1.SelectedValue);
            rowSklad["Адрес"] = "";
            rowSklad["Телефон"] = "";
            rowSklad["Площадь"] = 0;
            rowSklad["Количество стеллажей"] = 0;
            dsKamaz.тбсклад.Rows.Add(rowSklad);

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
            WMS_Kamaz_ds.тбскладDataTable ds2 = (WMS_Kamaz_ds.тбскладDataTable)dsKamaz.тбсклад.GetChanges(DataRowState.Added);
            
            if (ds2 != null)
                try
                {
                    taSklad.Update(ds2);
                    ds2.Dispose();
                    dsKamaz.тбсклад.AcceptChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка вставки записи в базу данных wms_kamaz " + ex, "Предупреждение");
                    this.dsKamaz.тбсклад.RejectChanges();
                }

            WMS_Kamaz_ds.тбскладDataTable ds3 = (WMS_Kamaz_ds.тбскладDataTable)dsKamaz.тбсклад.GetChanges(DataRowState.Modified);

            if (ds3 != null)
                try
                {
                    taSklad.Update(ds3);
                    ds3.Dispose();
                    dsKamaz.тбсклад.AcceptChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка изменения записи в базе данных wms_kamaz " + ex, "Предупреждение");
                    this.dsKamaz.тбсклад.RejectChanges();
                }

        }

        private void Delete()
        {
            int pos = -1;
            pos = Convert.ToInt32(listBox1.SelectedIndex); // selectedValue - вместо selectedIndex !

            string mes = listBox1.SelectedValue.ToString();

            SetEditing(false);

            MessageBoxResult result = MessageBox.Show(" Удалить данные по складу № " + mes + "?", "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    {
                        //MessageBox.Show("Удаление данных");
                        this.dsKamaz.тбсклад.Rows[pos].Delete();
                        if (this.dsKamaz.тбсклад.GetChanges(DataRowState.Deleted) != null)
                        {
                            try
                            {
                                this.taSklad.Update(dsKamaz.тбсклад);
                                this.dsKamaz.тбсклад.AcceptChanges();
                            }
                            catch (Exception x)
                            {
                                string er = x.Message.ToString();
                                MessageBox.Show("Ошибка удаления записи в таблице тбсклад " + er, "Предупреждение");
                                this.dsKamaz.тбсклад.RejectChanges();
                            }
                        }
                        break;
                    }
                case MessageBoxResult.No:
                    {
                        //MessageBox.Show("Отмена удаления данных");
                        this.dsKamaz.тбсклад.RejectChanges();
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

                comboBox1.IsEnabled = true;
                textBox1.IsReadOnly = false;
                textBox2.IsReadOnly = false;
                textBox3.IsReadOnly = false;
                textBox4.IsReadOnly = false;

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

                comboBox1.IsEnabled = false;
                textBox1.IsReadOnly = true;
                textBox2.IsReadOnly = true;
                textBox3.IsReadOnly = true;
                textBox4.IsReadOnly = true;

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
            comboBox1.SelectedIndex = 0;
        }


        private void listBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
            {
                int value1 = Convert.ToInt32(dsKamaz.тбсклад[listBox1.SelectedIndex]["Код_подразделения"]);
                int index1 = 0;
                foreach (DataRow dr1 in dsKamaz.тбподразделение)
                {
                    if (Convert.ToInt32(dr1["Код_подразделения"]) == value1)
                    {
                        break;
                    }
                    index1++;
                }
                comboBox1.SelectedIndex = index1;
                
                b1 = new Binding();
                b1.Source = dsKamaz.тбсклад;
                b1.Path = new PropertyPath("Rows[" + listBox1.SelectedIndex + "][Адрес]");
                BindingOperations.SetBinding(textBox1, TextBox.TextProperty, b1);

                b2 = new Binding();
                b2.Source = dsKamaz.тбсклад;
                b2.Path = new PropertyPath("Rows[" + listBox1.SelectedIndex + "][Телефон]");
                BindingOperations.SetBinding(textBox2, TextBox.TextProperty, b2);

                b3 = new Binding();
                b3.Source = dsKamaz.тбсклад;
                b3.Path = new PropertyPath("Rows[" + listBox1.SelectedIndex + "][Площадь]");
                BindingOperations.SetBinding(textBox3, TextBox.TextProperty, b3);

                b4 = new Binding();
                b4.Source = dsKamaz.тбсклад;
                b4.Path = new PropertyPath("Rows[" + listBox1.SelectedIndex + "][Количество стеллажей]");
                BindingOperations.SetBinding(textBox4, TextBox.TextProperty, b4);
                
            }
        }
        
        private void comboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SaveButton.IsEnabled)
            {
                DataRow dr = dsKamaz.тбсклад[listBox1.SelectedIndex];
                dr["Код_подразделения"] = Convert.ToInt32(comboBox1.SelectedValue);
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
