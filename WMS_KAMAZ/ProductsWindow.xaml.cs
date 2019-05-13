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
    /// Логика взаимодействия для ProductsWindow.xaml
    /// </summary>
    public partial class ProductsWindow : Window
    {
        MySqlConnection conn;

        public ProductsWindow()
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
                taNomenkl.Connection = conn;
                taPostav.Connection = conn;
                taTara.Connection = conn;
                taProdNaSklade.Connection = conn;
            }
            DataContext = dsKamaz;
        }

        WMS_Kamaz_ds dsKamaz = new WMS_Kamaz_ds();

        Binding b1, b2, b3, b4, b5, b6, b7, b8;

        WMS_Kamaz_dsTableAdapters.тбноменкл_продукцииTableAdapter taNomenkl = new WMS_Kamaz_dsTableAdapters.тбноменкл_продукцииTableAdapter();

        WMS_Kamaz_dsTableAdapters.тбпоставщик_продукцииTableAdapter taPostav = new WMS_Kamaz_dsTableAdapters.тбпоставщик_продукцииTableAdapter();

        WMS_Kamaz_dsTableAdapters.тбтараTableAdapter taTara = new WMS_Kamaz_dsTableAdapters.тбтараTableAdapter();

        WMS_Kamaz_dsTableAdapters.тбпродукция_на_складеTableAdapter taProdNaSklade = new WMS_Kamaz_dsTableAdapters.тбпродукция_на_складеTableAdapter();
        private void Wms_KamazFill()
        {
            try
            {
                taNomenkl.Fill(dsKamaz.тбноменкл_продукции);
                taPostav.Fill(dsKamaz.тбпоставщик_продукции);
                taTara.Fill(dsKamaz.тбтара);
                taProdNaSklade.Fill(dsKamaz.тбпродукция_на_складе);
            }
            catch (Exception ex) { MessageBox.Show("Ошибка в работе метода Fill DataAdapter: " + ex); }
        }

        private void Undo()
        {
            dsKamaz.тбноменкл_продукции.RejectChanges();
            SetEditing(false);
        }

        private void New() // COMBOBOX - КОД ПОДРАЗДЕЛЕНИЯ НЕ РОБИТ !
        {
            SetEditing(true);

            DataRowView drv = (DataRowView)listBox1.Items[listBox1.Items.Count - 1];
            int kodProdukcii = Convert.ToInt32(drv[0]);

            DataRow rowProd = this.dsKamaz.тбноменкл_продукции.Newтбноменкл_продукцииRow();
            rowProd["Код_продукции"] = kodProdukcii + 1;
            rowProd["Код_поставщика"] = Convert.ToInt32(comboBox1.SelectedValue);
            rowProd["Код_тары"] = Convert.ToInt32(comboBox2.SelectedValue);
            rowProd["Наименование"] = "";
            rowProd["Стоимость единицы"] = 0;
            rowProd["Единицы измерения"] = "";
            rowProd["Масса брутто"] = 0;
            rowProd["Длина"] = 0;
            rowProd["Ширина"] = 0;
            rowProd["Высота"] = 0;
            rowProd["Материал"] = "";
            dsKamaz.тбноменкл_продукции.Rows.Add(rowProd);

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
            WMS_Kamaz_ds.тбноменкл_продукцииDataTable ds2 = (WMS_Kamaz_ds.тбноменкл_продукцииDataTable)dsKamaz.тбноменкл_продукции.GetChanges(DataRowState.Added);
            
            if (ds2 != null)
                try
                {
                    taNomenkl.Update(ds2);
                    ds2.Dispose();
                    dsKamaz.тбноменкл_продукции.AcceptChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка вставки записи в базу данных wms_kamaz " + ex, "Предупреждение");
                    this.dsKamaz.тбноменкл_продукции.RejectChanges();
                }

            WMS_Kamaz_ds.тбноменкл_продукцииDataTable ds3 = (WMS_Kamaz_ds.тбноменкл_продукцииDataTable)dsKamaz.тбноменкл_продукции.GetChanges(DataRowState.Modified);

            if (ds3 != null)
                try
                {
                    taNomenkl.Update(ds3);
                    ds3.Dispose();
                    dsKamaz.тбноменкл_продукции.AcceptChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка изменения записи в базе данных wms_kamaz " + ex, "Предупреждение");
                    this.dsKamaz.тбноменкл_продукции.RejectChanges();
                }

        }

        private void Delete()
        {
            int pos = -1;
            //pos = this.BindingContext[dsTechtrans, "тперсонал"].Position;
            pos = Convert.ToInt32(listBox1.SelectedIndex); // selectedValue - вместо selectedIndex !

            string mes = listBox1.SelectedValue.ToString();

            SetEditing(false);

            MessageBoxResult result = MessageBox.Show(" Удалить данные по продукции " + mes + "?", "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    {
                        //MessageBox.Show("Удаление данных");
                        this.dsKamaz.тбноменкл_продукции.Rows[pos].Delete();
                        if (this.dsKamaz.тбноменкл_продукции.GetChanges(DataRowState.Deleted) != null)
                        {
                            try
                            {
                                this.taNomenkl.Update(dsKamaz.тбноменкл_продукции);
                                this.dsKamaz.тбноменкл_продукции.AcceptChanges();
                            }
                            catch (Exception x)
                            {
                                string er = x.Message.ToString();
                                MessageBox.Show("Ошибка удаления записи в таблице тбноменкл_продукции " + er, "Предупреждение");
                                this.dsKamaz.тбноменкл_продукции.RejectChanges();
                            }
                        }
                        break;
                    }
                case MessageBoxResult.No:
                    {
                        //MessageBox.Show("Отмена удаления данных");
                        this.dsKamaz.тбноменкл_продукции.RejectChanges();
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
                comboBox2.IsEnabled = true;
                textBox1.IsReadOnly = false;
                textBox2.IsReadOnly = false;
                textBox3.IsReadOnly = false;
                textBox4.IsReadOnly = false;
                textBox5.IsReadOnly = false;
                textBox6.IsReadOnly = false;
                textBox7.IsReadOnly = false;
                textBox8.IsReadOnly = false;

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
                comboBox2.IsEnabled = false;
                textBox1.IsReadOnly = true;
                textBox2.IsReadOnly = true;
                textBox3.IsReadOnly = true;
                textBox4.IsReadOnly = true;
                textBox5.IsReadOnly = true;
                textBox6.IsReadOnly = true;
                textBox7.IsReadOnly = true;
                textBox8.IsReadOnly = true;

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
            comboBox2.SelectedIndex = 0;
        }


        private void listBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
            {
                int value1 = Convert.ToInt32(dsKamaz.тбноменкл_продукции[listBox1.SelectedIndex]["Код_поставщика"]);
                int index1 = 0;
                foreach (DataRow dr1 in dsKamaz.тбпоставщик_продукции)
                {
                    if (Convert.ToInt32(dr1["Код_поставщика"]) == value1)
                    {
                        break;
                    }
                    index1++;
                }
                comboBox1.SelectedIndex = index1;

                int value2 = Convert.ToInt32(dsKamaz.тбноменкл_продукции[listBox1.SelectedIndex]["Код_тары"]);
                int index2 = 0;
                foreach (DataRow dr2 in dsKamaz.тбтара)
                {
                    if (Convert.ToInt32(dr2["Код_тары"]) == value2)
                    {
                        break;
                    }
                    index2++;
                }
                comboBox2.SelectedIndex = index2;

                b1 = new Binding();
                b1.Source = dsKamaz.тбноменкл_продукции;
                b1.Path = new PropertyPath("Rows[" + listBox1.SelectedIndex + "][Наименование]");
                BindingOperations.SetBinding(textBox1, TextBox.TextProperty, b1);

                b2 = new Binding();
                b2.Source = dsKamaz.тбноменкл_продукции;
                b2.Path = new PropertyPath("Rows[" + listBox1.SelectedIndex + "][Стоимость единицы]");
                BindingOperations.SetBinding(textBox2, TextBox.TextProperty, b2);

                b3 = new Binding();
                b3.Source = dsKamaz.тбноменкл_продукции;
                b3.Path = new PropertyPath("Rows[" + listBox1.SelectedIndex + "][Единицы измерения]");
                BindingOperations.SetBinding(textBox3, TextBox.TextProperty, b3);

                b4 = new Binding();
                b4.Source = dsKamaz.тбноменкл_продукции;
                b4.Path = new PropertyPath("Rows[" + listBox1.SelectedIndex + "][Масса брутто]");
                BindingOperations.SetBinding(textBox4, TextBox.TextProperty, b4);

                b5 = new Binding();
                b5.Source = dsKamaz.тбноменкл_продукции;
                b5.Path = new PropertyPath("Rows[" + listBox1.SelectedIndex + "][Длина]");
                BindingOperations.SetBinding(textBox5, TextBox.TextProperty, b5);

                b6 = new Binding();
                b6.Source = dsKamaz.тбноменкл_продукции;
                b6.Path = new PropertyPath("Rows[" + listBox1.SelectedIndex + "][Ширина]");
                BindingOperations.SetBinding(textBox6, TextBox.TextProperty, b6);

                b7 = new Binding();
                b7.Source = dsKamaz.тбноменкл_продукции;
                b7.Path = new PropertyPath("Rows[" + listBox1.SelectedIndex + "][Высота]");
                BindingOperations.SetBinding(textBox7, TextBox.TextProperty, b7);

                b8 = new Binding();
                b8.Source = dsKamaz.тбноменкл_продукции;
                b8.Path = new PropertyPath("Rows[" + listBox1.SelectedIndex + "][Материал]");
                BindingOperations.SetBinding(textBox8, TextBox.TextProperty, b8);

            }
        }
        
        private void comboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SaveButton.IsEnabled)
            {
                DataRow dr = dsKamaz.тбноменкл_продукции[listBox1.SelectedIndex];
                dr["Код_поставщика"] = Convert.ToInt32(comboBox1.SelectedValue);
            }
        }

        private void comboBox2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SaveButton.IsEnabled)
            {
                DataRow dr = dsKamaz.тбноменкл_продукции[listBox1.SelectedIndex];
                dr["Код_тары"] = Convert.ToInt32(comboBox2.SelectedValue);
            }
        }

        private void Button_Save(object sender, RoutedEventArgs e)
        {/*
            if (button1.Visibility == System.Windows.Visibility.Visible)
                button1.IsEnabled = true;*/
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
            if (buttonAddToCurrentProd.Visibility == System.Windows.Visibility.Visible)
                buttonAddToCurrentProd.IsEnabled = true;
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            Delete();
        }

        public int nomerSklada { get; set; }

        private void buttonAddToCurrentProd_Click(object sender, RoutedEventArgs e)
        {
            if (textBoxKolVo.Text != "" && listBox1.SelectedValue != null)
            {
                MessageBoxResult result = MessageBox.Show("Добавить продукцию с кодом - " + listBox1.SelectedValue.ToString() + " к продукции на складе ?", "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    DataRow rowTekushProd = this.dsKamaz.тбпродукция_на_складе.Newтбпродукция_на_складеRow();
                    rowTekushProd["ID_хранения"] = Convert.ToInt32((dsKamaz.тбпродукция_на_складе.Rows[dsKamaz.тбпродукция_на_складе.Count - 1][0])) + 1;
                    rowTekushProd["Код_продукции"] = Convert.ToInt32(listBox1.SelectedValue);
                    rowTekushProd["Номер_склада"] = nomerSklada;
                    rowTekushProd["Количество"] = Convert.ToInt32(textBoxKolVo.Text);
                    rowTekushProd["Дата получения"] = DateTime.Now;
                    dsKamaz.тбпродукция_на_складе.Rows.Add(rowTekushProd);
                    
                    WMS_Kamaz_ds.тбпродукция_на_складеDataTable dsProdNaSklade = (WMS_Kamaz_ds.тбпродукция_на_складеDataTable)dsKamaz.тбпродукция_на_складе.GetChanges();

                    if (dsProdNaSklade != null)
                        try
                        {
                            taProdNaSklade.Update(dsProdNaSklade);
                            dsProdNaSklade.Dispose();
                            dsKamaz.тбпродукция_на_складе.AcceptChanges();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Ошибка принятия продукции [тек продукция]" + ex, "Предупреждение");
                            this.dsKamaz.тбпродукция_на_складе.RejectChanges();
                        }
                    this.Close();
                }


            }

        }

    }
}
