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
    /// Логика взаимодействия для PersonalWindow.xaml
    /// </summary>
    public partial class PersonalWindow : Window
    {
        MySqlConnection conn;

        public PersonalWindow()
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
                taPersonal.Connection = conn;
                taSklad.Connection = conn;
                taDolgnost.Connection = conn;
                taPodrazdelenie.Connection = conn;
            }
            DataContext = dsKamaz;
        }

        WMS_Kamaz_ds dsKamaz = new WMS_Kamaz_ds();

        Binding b1, b2, b3, b4, b5, b6, b7, b8;

        WMS_Kamaz_dsTableAdapters.тбперсоналTableAdapter taPersonal = new WMS_Kamaz_dsTableAdapters.тбперсоналTableAdapter();

        WMS_Kamaz_dsTableAdapters.тбскладTableAdapter taSklad = new WMS_Kamaz_dsTableAdapters.тбскладTableAdapter();

        WMS_Kamaz_dsTableAdapters.тбдолжностьTableAdapter taDolgnost = new WMS_Kamaz_dsTableAdapters.тбдолжностьTableAdapter();

        WMS_Kamaz_dsTableAdapters.тбподразделениеTableAdapter taPodrazdelenie = new WMS_Kamaz_dsTableAdapters.тбподразделениеTableAdapter();

        private void Wms_KamazFill()
        {
            try
            {
                taPersonal.Fill(dsKamaz.тбперсонал);
                taSklad.Fill(dsKamaz.тбсклад);
                taDolgnost.Fill(dsKamaz.тбдолжность);
                taPodrazdelenie.Fill(dsKamaz.тбподразделение);
            }
            catch (Exception ex) { MessageBox.Show("Ошибка в работе метода Fill DataAdapter: " + ex); }
        }

        private void Undo()
        {
            dsKamaz.тбперсонал.RejectChanges();
            SetEditing(false);
        }

        private void New() // COMBOBOX - КОД ПОДРАЗДЕЛЕНИЯ НЕ РОБИТ !
        {
            SetEditing(true);

            DataRowView drv = (DataRowView)listBox1.Items[listBox1.Items.Count - 1];
            int id_Personala = Convert.ToInt32(drv[0]);

            DataRow rowPersonal = this.dsKamaz.тбперсонал.NewтбперсоналRow();
            rowPersonal["ID_персонала"] = id_Personala + 1;
            rowPersonal["Номер_склада"] = Convert.ToInt32(comboBox1.SelectedValue);
            rowPersonal["Должность"] = comboBox2.SelectedValue;
            rowPersonal["Код_подразделения"] = Convert.ToInt32(comboBox3.SelectedValue); ;
            rowPersonal["Фамилия"] = "";
            rowPersonal["Имя"] = "";
            rowPersonal["Отчество"] = "";
            rowPersonal["Дата рождения"] = datePicker1.SelectedDate;
            rowPersonal["Пол"] = "м";
            rowPersonal["Телефон"] = "";
            rowPersonal["Дата приема"] = datePicker1.SelectedDate;
            dsKamaz.тбперсонал.Rows.Add(rowPersonal);

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
            WMS_Kamaz_ds.тбперсоналDataTable ds2 = (WMS_Kamaz_ds.тбперсоналDataTable)dsKamaz.тбперсонал.GetChanges(DataRowState.Added);

            if (ds2 != null)
                try
                {
                    taPersonal.Update(ds2);
                    ds2.Dispose();
                    dsKamaz.тбперсонал.AcceptChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка вставки записи в базу данных wms_kamaz " + ex, "Предупреждение");
                    this.dsKamaz.тбперсонал.RejectChanges();
                }

            WMS_Kamaz_ds.тбперсоналDataTable ds3 = (WMS_Kamaz_ds.тбперсоналDataTable)dsKamaz.тбперсонал.GetChanges(DataRowState.Modified);

            if (ds3 != null)
                try
                {
                    taPersonal.Update(ds3);
                    ds3.Dispose();
                    dsKamaz.тбперсонал.AcceptChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка изменения записи в базе данных wms_kamaz " + ex, "Предупреждение");
                    this.dsKamaz.тбперсонал.RejectChanges();
                }

        }

        private void Delete()
        {
            int pos = -1;
            //pos = this.BindingContext[dsTechtrans, "тперсонал"].Position;
            pos = Convert.ToInt32(listBox1.SelectedIndex); // selectedValue - вместо selectedIndex !

            string mes = listBox1.SelectedValue.ToString();

            SetEditing(false);

            MessageBoxResult result = MessageBox.Show(" Удалить данные по персоналу ID " + mes + "?", "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    {
                        //MessageBox.Show("Удаление данных");
                        this.dsKamaz.тбперсонал.Rows[pos].Delete();
                        if (this.dsKamaz.тбперсонал.GetChanges(DataRowState.Deleted) != null)
                        {
                            try
                            {
                                this.taPersonal.Update(dsKamaz.тбперсонал);
                                this.dsKamaz.тбперсонал.AcceptChanges();
                            }
                            catch (Exception x)
                            {
                                string er = x.Message.ToString();
                                MessageBox.Show("Ошибка удаления записи в таблице тбперсонал " + er, "Предупреждение");
                                this.dsKamaz.тбперсонал.RejectChanges();
                            }
                        }
                        break;
                    }
                case MessageBoxResult.No:
                    {
                        //MessageBox.Show("Отмена удаления данных");
                        this.dsKamaz.тбперсонал.RejectChanges();
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
                comboBox3.IsEnabled = true;
                textBox1.IsReadOnly = false;
                textBox2.IsReadOnly = false;
                textBox3.IsReadOnly = false;
                textBox4.IsReadOnly = false;
                textBox4.Visibility = Visibility.Hidden;
                textBox5.IsReadOnly = false;
                textBox6.IsReadOnly = false;
                textBox7.IsReadOnly = false;
                textBox7.Visibility = Visibility.Hidden;
                datePicker1.Visibility = Visibility.Visible;
                datePicker2.Visibility = Visibility.Visible;

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
                comboBox3.IsEnabled = false;
                textBox1.IsReadOnly = true;
                textBox2.IsReadOnly = true;
                textBox3.IsReadOnly = true;
                textBox4.IsReadOnly = true;
                textBox4.Visibility = Visibility.Visible;
                textBox5.IsReadOnly = true;
                textBox6.IsReadOnly = true;
                textBox7.IsReadOnly = true;
                textBox7.Visibility = Visibility.Visible;
                datePicker1.Visibility = Visibility.Hidden;
                datePicker2.Visibility = Visibility.Hidden;

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
            comboBox3.SelectedIndex = 0;
            datePicker1.SelectedDate = DateTime.Now;
            datePicker2.SelectedDate = DateTime.Now;
        }


        private void listBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
            {
                int value1 = Convert.ToInt32(dsKamaz.тбперсонал[listBox1.SelectedIndex]["Номер_склада"]);
                int index1 = 0;
                foreach (DataRow dr1 in dsKamaz.тбсклад)
                {
                    if (Convert.ToInt32(dr1["Номер_склада"]) == value1)
                    {
                        break;
                    }
                    index1++;
                }
                comboBox1.SelectedIndex = index1;

                string value2 = dsKamaz.тбперсонал[listBox1.SelectedIndex]["Должность"].ToString();
                int index2 = 0;
                foreach (DataRow dr2 in dsKamaz.тбдолжность)
                {
                    if (dr2["Должность"].ToString() == value2)
                    {
                        break;
                    }
                    index2++;
                }
                comboBox2.SelectedIndex = index2;

                int value3 = Convert.ToInt32(dsKamaz.тбперсонал[listBox1.SelectedIndex]["Код_подразделения"]);
                int index3 = 0;
                foreach (DataRow dr3 in dsKamaz.тбподразделение)
                {
                    if (Convert.ToInt32(dr3["Код_подразделения"]) == value3)
                    {
                        break;
                    }
                    index3++;
                }
                comboBox3.SelectedIndex = index3;

                b1 = new Binding();
                b1.Source = dsKamaz.тбперсонал;
                b1.Path = new PropertyPath("Rows[" + listBox1.SelectedIndex + "][Фамилия]");
                BindingOperations.SetBinding(textBox1, TextBox.TextProperty, b1);

                b2 = new Binding();
                b2.Source = dsKamaz.тбперсонал;
                b2.Path = new PropertyPath("Rows[" + listBox1.SelectedIndex + "][Имя]");
                BindingOperations.SetBinding(textBox2, TextBox.TextProperty, b2);

                b3 = new Binding();
                b3.Source = dsKamaz.тбперсонал;
                b3.Path = new PropertyPath("Rows[" + listBox1.SelectedIndex + "][Отчество]");
                BindingOperations.SetBinding(textBox3, TextBox.TextProperty, b3);

                b4 = new Binding();
                b4.Source = dsKamaz.тбперсонал;
                b4.Path = new PropertyPath("Rows[" + listBox1.SelectedIndex + "][Дата рождения]");
                BindingOperations.SetBinding(textBox4, TextBox.TextProperty, b4);

                b5 = new Binding();
                b5.Source = dsKamaz.тбперсонал;
                b5.Path = new PropertyPath("Rows[" + listBox1.SelectedIndex + "][Пол]");
                BindingOperations.SetBinding(textBox5, TextBox.TextProperty, b5);

                b6 = new Binding();
                b6.Source = dsKamaz.тбперсонал;
                b6.Path = new PropertyPath("Rows[" + listBox1.SelectedIndex + "][Телефон]");
                BindingOperations.SetBinding(textBox6, TextBox.TextProperty, b6);

                b7 = new Binding();
                b7.Source = dsKamaz.тбперсонал;
                b7.Path = new PropertyPath("Rows[" + listBox1.SelectedIndex + "][Дата приема]");
                BindingOperations.SetBinding(textBox7, TextBox.TextProperty, b7);

            }
        }

        private void comboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SaveButton.IsEnabled)
            {
                DataRow dr = dsKamaz.тбперсонал[listBox1.SelectedIndex];
                dr["Номер_склада"] = Convert.ToInt32(comboBox1.SelectedValue);
            }
        }

        private void comboBox2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SaveButton.IsEnabled)
            {
                DataRow dr = dsKamaz.тбперсонал[listBox1.SelectedIndex];
                dr["Должность"] = comboBox2.SelectedValue;
            }
        }

        private void comboBox3_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SaveButton.IsEnabled)
            {
                DataRow dr = dsKamaz.тбперсонал[listBox1.SelectedIndex];
                dr["Код_подразделения"] = Convert.ToInt32(comboBox3.SelectedValue);
            }
        }

        private void datePicker1_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DataRow dr = dsKamaz.тбперсонал[listBox1.SelectedIndex];
            dr["Дата рождения"] = datePicker1.SelectedDate;
        }

        private void datePicker2_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DataRow dr = dsKamaz.тбперсонал[listBox1.SelectedIndex];
            dr["Дата приема"] = datePicker2.SelectedDate;
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
