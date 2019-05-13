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
using System.Windows.Navigation;
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
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        LoadingWindow loadingWindow;
        Thread newLoadingWindowThread;

        static string IP = "127.0.0.1";
        static string Login = "root";
        static string Pass = "12345";
        static string Database = "wms_kamaz";

        private string defaultConStr = string.Format("server={0};uid={1};pwd={2};database={3};", IP, Login, Pass, Database);

        //="server=127.0.0.1;user id=root;database=wms_kamaz;persistsecurityinfo=True;password=12345"

        //private string ConStr = "Database=acsm_6fa30819604a3e7;Data Source=eu-cdbr-azure-west-d.cloudapp.net;User Id=b45cbca2ee4357;Password=c7982230;";

        MySqlConnection conn;

        DataView dv1, dv2, dv3, dv4;

        DataTable tableDG2andDG4;

        int tekushNomNakladn = 0;

        System.Windows.Threading.DispatcherTimer timerUpdateOtgruzki;

        public MainWindow()
        {
            InitializeComponent();
            OpenLoadingWindow();

            conn = GetDatabaseConnection("WMS_KAMAZ.Properties.Settings.acsm_6fa30819604a3e7ConnectionString");
            if (conn == null)
            {
                MessageBox.Show("Нет соединения с БД !");
                Application.Current.Shutdown();
            }
            else
            {
                taOtgruzki.Connection = conn;
                taNomenkl.Connection = conn;
                taPostavki.Connection = conn;
                taProdNaSklade.Connection = conn;
                taSklad.Connection = conn;
                taPodrazdelenie.Connection = conn;
                taNakladnaya.Connection = conn;
                taPostavshik.Connection = conn;
                taTara.Connection = conn;
            }

            DataContext = dsKamaz;

            //tableDG3 = GetDataGrid2Table();

            //dv1 = new DataView(dsKamaz.тботгрузки_продукции);
            //dv2 = new DataView(tableDG3);
            //dv3 = new DataView(dsKamaz.тбпоставки_продукции);
            //dv4 = new DataView(dsKamaz.тбпродукция_на_складе);

            //dataGrid1.DataContext = dv1;
            //dataGrid2.DataContext = dv2;
            //dataGrid3.DataContext = dv3;
            //dataGrid4.DataContext = dv4;


            //проверка - если соединения с БД нет, вывести предупреждение и окно с настройками


            //лог ошибок
            
            //При нажатии на F2 размер колонок по умполчанию
            this.KeyDown += (sender, e) =>
            {
                if (e.Key == Key.F2)
                {
                    column0.Width = new GridLength(338);
                    column2.Width = new GridLength(338);
                    row2.Height = new GridLength(271);
                }
            };
        }

        WMS_Kamaz_ds dsKamaz = new WMS_Kamaz_ds();

        WMS_Kamaz_dsTableAdapters.тбноменкл_продукцииTableAdapter taNomenkl = new WMS_Kamaz_dsTableAdapters.тбноменкл_продукцииTableAdapter();

        WMS_Kamaz_dsTableAdapters.тботгрузки_продукцииTableAdapter taOtgruzki = new WMS_Kamaz_dsTableAdapters.тботгрузки_продукцииTableAdapter();

        WMS_Kamaz_dsTableAdapters.тбпоставки_продукцииTableAdapter taPostavki = new WMS_Kamaz_dsTableAdapters.тбпоставки_продукцииTableAdapter();

        WMS_Kamaz_dsTableAdapters.тбпродукция_на_складеTableAdapter taProdNaSklade = new WMS_Kamaz_dsTableAdapters.тбпродукция_на_складеTableAdapter();

        WMS_Kamaz_dsTableAdapters.тбскладTableAdapter taSklad = new WMS_Kamaz_dsTableAdapters.тбскладTableAdapter();

        WMS_Kamaz_dsTableAdapters.тбподразделениеTableAdapter taPodrazdelenie = new WMS_Kamaz_dsTableAdapters.тбподразделениеTableAdapter();

        WMS_Kamaz_dsTableAdapters.тбнакладнаяTableAdapter taNakladnaya = new WMS_Kamaz_dsTableAdapters.тбнакладнаяTableAdapter();

        WMS_Kamaz_dsTableAdapters.тбпоставщик_продукцииTableAdapter taPostavshik = new WMS_Kamaz_dsTableAdapters.тбпоставщик_продукцииTableAdapter();

        WMS_Kamaz_dsTableAdapters.тбтараTableAdapter taTara = new WMS_Kamaz_dsTableAdapters.тбтараTableAdapter();

        public void Wms_KamazFill()
        {
            try
            {
                taOtgruzki.Fill(dsKamaz.тботгрузки_продукции);
                taPostavki.Fill(dsKamaz.тбпоставки_продукции);
                taProdNaSklade.Fill(dsKamaz.тбпродукция_на_складе);
                taNomenkl.Fill(dsKamaz.тбноменкл_продукции);
                taSklad.Fill(dsKamaz.тбсклад);
                taPodrazdelenie.Fill(dsKamaz.тбподразделение);
                taNakladnaya.Fill(dsKamaz.тбнакладная);
                taPostavshik.Fill(dsKamaz.тбпоставщик_продукции);
                taTara.Fill(dsKamaz.тбтара);
            }
            catch (Exception ex) { MessageBox.Show("Ошибка во время заполнения таблиц: \n" + ex); }
        }

        //Получить таблицу состающию из 2-х
        private DataTable GetDG2andDG4Table()
        {
            taProdNaSklade.Fill(dsKamaz.тбпродукция_на_складе);
            DataTable dt = new DataTable();

            dt.Columns.Add("Наименование", typeof(string));
            dt.Columns.Add("Код_продукции", typeof(long));
            dt.Columns.Add("Номер_склада", typeof(long));
            dt.Columns.Add("Количество", typeof(long));
            dt.Columns.Add("Дата получения");
            dt.Columns.Add("Стоимость единицы");
            dt.Columns.Add("Единицы измерения");
            dt.Columns.Add("Масса брутто");
            //IEnumerable<DataRow> Join
            try
            {
                var Join = (from prodNaSklade in dsKamaz.тбпродукция_на_складе.AsEnumerable()
                            join nomeklProd in dsKamaz.тбноменкл_продукции.AsEnumerable()
                            on prodNaSklade.Field<long>("Код_продукции") equals nomeklProd.Field<long>("Код_продукции")
                            where nomeklProd.Field<long>("Код_продукции") != 0
                            select dt.LoadDataRow(new object[]
                        {
                            nomeklProd.Field<string>("Наименование"),
                            nomeklProd.Field<long>("Код_продукции"),
                            prodNaSklade.Field<sbyte>("Номер_склада"),
                            prodNaSklade.Field<ushort>("Количество"),
                            prodNaSklade.Field<DateTime>("Дата получения"),
                            nomeklProd.Field<float>("Стоимость единицы"),
                            nomeklProd.Field<string>("Единицы измерения"),
                            nomeklProd.Field<float>("Масса брутто")

                        }, false));

                Join.CopyToDataTable();
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
            return dt;
        }
        
        private void updateDG2andDG4()
        {
            tableDG2andDG4 = GetDG2andDG4Table();
            dv2 = new DataView(tableDG2andDG4);
            dv2.RowFilter = "Номер_склада =" + comboBox2.SelectedValue;
            dataGrid2.DataContext = dv2;
            dv4 = new DataView(tableDG2andDG4);
            dv4.RowFilter = "Номер_склада <>" + comboBox2.SelectedValue + " AND " + "Номер_склада =" + comboBox4.SelectedValue;
            dataGrid4.DataContext = dv4;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Wms_KamazFill();


            //updateDataGrid2();

            dv1 = new DataView(dsKamaz.тботгрузки_продукции);
            dv2 = new DataView(GetDG2andDG4Table());
            dv3 = new DataView(dsKamaz.тбпоставки_продукции);
            //dv4 = new DataView(dsKamaz.тбпродукция_на_складе);
            dv4 = new DataView(GetDG2andDG4Table());

            dataGrid1.DataContext = dv1;
            dataGrid2.DataContext = dv2;
            dataGrid3.DataContext = dv3;
            dataGrid4.DataContext = dv4;

            int defualtPodrazd = Convert.ToInt32(WMS_KAMAZ.Properties.Settings.Default.DefaultPodrazd);
            if (comboBox1.Items.Count >= defualtPodrazd)
            {
                comboBox1.SelectedIndex = defualtPodrazd;
            }
            else
                comboBox1.SelectedIndex = 0;

            comboBox2.SelectedIndex = 0;
            comboBox3.SelectedIndex = 0;
            comboBox4.SelectedIndex = 0;

            dataGrid1.SelectedIndex = 0;
            dataGrid2.SelectedIndex = 0;
            dataGrid3.SelectedIndex = 0;

            UstanovitRedaktZayavki(false);

            timerUpdateOtgruzki = new System.Windows.Threading.DispatcherTimer();
            timerUpdateOtgruzki.Interval = new TimeSpan(0, 0, 5);
            timerUpdateOtgruzki.Tick +=new EventHandler(timerUpdateOtgruzki_Tick);

            CloseLoadingWindow();

        }
        
        private void comboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            if (comboBox1.SelectedValue != null)
            {
                //comboBox2.ItemsSource = dsKamaz.тбсклад.Rows["Номер_склада"][comboBox1.SelectedValue];
                var sklady = (from podrazd in dsKamaz.тбподразделение
                            join sklad in dsKamaz.тбсклад
                            on podrazd.Код_подразделения equals sklad.Код_подразделения
                            where sklad.Код_подразделения == Convert.ToInt32(comboBox1.SelectedValue)
                            select new
                            {
                                sklad.Номер_склада
                            }).ToList();
                if (sklady.Count != 0)
                {
                    comboBox2.ItemsSource = sklady;
                    comboBox2.SelectedIndex = 0;
                }

            }
            else
            {

            }
        }

        private void comboBox2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //dv2 = new DataView(GetDataGrid2Table());
            //dataGrid2.DataContext = dv2;
            textBox1.Text = "";
            //МУЛЬТИФИЛЬТР dataView ДЕЛАЕТСЯ ЧЕРЕЗ " AND " !!!
            if (comboBox2.SelectedValue != null || comboBox2.SelectedIndex !=-1)
            {

                dv1.RowFilter = "Номер_склада =" + comboBox2.SelectedValue + " AND " + "Отгружен = false";
                dv2.RowFilter = "Номер_склада =" + comboBox2.SelectedValue;
                dv3.RowFilter = "Номер_склада =" + comboBox2.SelectedValue + " AND " + "Получен = false"; ;
                dv4.RowFilter = "Номер_склада <> " + comboBox2.SelectedValue;
                if (comboBox4.SelectedValue != null)
                    dv4.RowFilter = "Номер_склада <> " + comboBox2.SelectedValue + " AND " + "Номер_склада =" + comboBox4.SelectedValue;

                if (comboBox3.SelectedValue != null)
                {
                    var sklady = (from podrazd in dsKamaz.тбподразделение
                                  join sklad in dsKamaz.тбсклад
                                  on podrazd.Код_подразделения equals sklad.Код_подразделения
                                  where sklad.Код_подразделения == Convert.ToInt32(comboBox3.SelectedValue) && sklad.Номер_склада != Convert.ToInt32(comboBox2.SelectedValue)
                                  select new
                                  {
                                      sklad.Номер_склада
                                  }).ToList();
                    if (sklady.Count != 0)
                    {
                        comboBox4.ItemsSource = sklady;
                        comboBox4.SelectedIndex = 0;
                    }
                    else
                    {
                        comboBox4.ItemsSource = null;
                        comboBox4.IsEnabled = false;
                    }
                }

            }
            else
            {
                dv1.RowFilter = null;
                dv2.RowFilter = null;
                dv3.RowFilter = null;
                dv4.RowFilter = null;
            }
        }


        private void comboBox3_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBox3.SelectedValue != null)
            {
                //comboBox2.ItemsSource = dsKamaz.тбсклад.Rows["Номер_склада"][comboBox1.SelectedValue];
                var sklady = (from podrazd in dsKamaz.тбподразделение
                            join sklad in dsKamaz.тбсклад
                            on podrazd.Код_подразделения equals sklad.Код_подразделения
                            where sklad.Код_подразделения == Convert.ToInt32(comboBox3.SelectedValue) && sklad.Номер_склада != Convert.ToInt32(comboBox2.SelectedValue)
                            select new
                            {
                                sklad.Номер_склада
                            }).ToList();
                if (sklady.Count != 0)
                {
                    comboBox4.IsEnabled = true;
                    comboBox4.ItemsSource = sklady;
                    comboBox4.SelectedIndex = 0;
                }
                else
                {
                    comboBox4.ItemsSource = null;
                    comboBox4.IsEnabled = false;
                }
            }
        }
        
        private void comboBox4_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBox4.SelectedValue != null)
            {
                if (textBox2.Text != "" && textBox2.Text != null)
                {
                    dv4.RowFilter = "Код_продукции =" + textBox2.Text.Trim() + " AND " + "Номер_склада <>" + comboBox2.SelectedValue + " AND Номер_склада =" + comboBox4.SelectedValue; ;
                }
                else
                {
                    dv4.RowFilter = "Номер_склада <>" + comboBox2.SelectedValue + " AND " + "Номер_склада =" + comboBox4.SelectedValue;
                }
            }
        }
        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            double num = 0;
            if (textBox1.Text != "" && (double.TryParse(textBox1.Text, out num)))
            {
                dv2.RowFilter = "Код_продукции = " + textBox1.Text.Trim() + " AND " + "Номер_склада =" + comboBox2.SelectedValue;
            }
            else
            {
                dv2.RowFilter = "Номер_склада =" + comboBox2.SelectedValue;
            }
        }

        private void textBox4_TextChanged(object sender, TextChangedEventArgs e)
        {
            double num = 0;
            if (textBox4.Text != "" && !(double.TryParse(textBox1.Text, out num)))
            {
                dv2.RowFilter = "Наименование LIKE '" + textBox4.Text.Trim() + "*' AND " + "Номер_склада =" + comboBox2.SelectedValue;
            }
            else
            {
                dv2.RowFilter = "Номер_склада =" + comboBox2.SelectedValue;
            }
        }

        private void buttonSozdatZayavku_Click(object sender, RoutedEventArgs e)
        {
            UstanovitRedaktZayavki(true);
            DataRow rowNakladnaya = this.dsKamaz.тбнакладная.NewтбнакладнаяRow();

            tekushNomNakladn = (int)(dsKamaz.тбнакладная.Rows[dsKamaz.тбнакладная.Count - 1][0]) + 1;
            rowNakladnaya["Номер_накладной"] = tekushNomNakladn;
            rowNakladnaya["Дата оформления"] = DateTime.Now;
            dsKamaz.тбнакладная.Rows.Add(rowNakladnaya);

            WMS_Kamaz_ds.тбнакладнаяDataTable dsnakl = (WMS_Kamaz_ds.тбнакладнаяDataTable)dsKamaz.тбнакладная.GetChanges();

            if (dsnakl != null)
                try
                {
                    taNakladnaya.Update(dsnakl);
                    dsnakl.Dispose();
                    dsKamaz.тбнакладная.AcceptChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка создания заявки [создать]" + ex, "Предупреждение");
                    this.dsKamaz.тбнакладная.RejectChanges();
                }
        }

        private void buttonOtmenZayavku_Click(object sender, RoutedEventArgs e)
        {
            dsKamaz.тбпоставки_продукции.RejectChanges();
            dsKamaz.тботгрузки_продукции.RejectChanges();
            dsKamaz.тбпродукция_на_складе.RejectChanges();

            DataRow[] dr = dsKamaz.тбнакладная.Select("Номер_накладной = " + tekushNomNakladn);
            int pos = dsKamaz.тбнакладная.Rows.IndexOf(dr[0]);

            this.dsKamaz.тбнакладная.Rows[pos].Delete();
            if (this.dsKamaz.тбнакладная.GetChanges(DataRowState.Deleted) != null)
            {
                try
                {
                    this.taNakladnaya.Update(dsKamaz.тбнакладная);
                    this.dsKamaz.тбнакладная.AcceptChanges();
                }
                catch (Exception x)
                {
                    string er = x.Message.ToString();
                    MessageBox.Show("Ошибка отмены заявки " + er, "Предупреждение");
                    this.dsKamaz.тбнакладная.RejectChanges();
                }
            }

            UstanovitRedaktZayavki(false);
        }

        private void buttonDobavVZayavku_Click(object sender, RoutedEventArgs e)
        {
            buttonGotovoZayavka.IsEnabled = true;
            int kodProd = 1000001;
            int kolVo = 0;
            int nomSkladaOtgruzki = 0;

            if (dataGrid4.Items.Count == 1)
            {
                if (textBox3.Text != "")
                {
                    kodProd = Convert.ToInt32((dataGrid4.Items[0] as DataRowView).Row.ItemArray[1]);

                    kolVo = Convert.ToInt32((dataGrid4.Items[0] as DataRowView).Row.ItemArray[3]);
                    //textBox3.Text = kolVo.ToString();

                    ///////////////////////////Данные в таблицу Поставки
                    DataRow rowPostavka = this.dsKamaz.тбпоставки_продукции.Newтбпоставки_продукцииRow();

                    rowPostavka["ID_поставки"] = (int)(dsKamaz.тбпоставки_продукции.Rows[dsKamaz.тбпоставки_продукции.Count - 1][0]) + 1;
                    rowPostavka["Номер_накладной"] = tekushNomNakladn;
                    rowPostavka["Номер_склада"] = comboBox2.SelectedValue;
                    rowPostavka["Код_продукции"] = kodProd;
                    rowPostavka["Количество"] = Convert.ToInt32(textBox3.Text);

                    //rowPostavka["Дата поставки"]
                    rowPostavka["Получен"] = 0;
                    dsKamaz.тбпоставки_продукции.Rows.Add(rowPostavka);

                    /////////////////////////////Данные в таблицу Отгрузки
                    DataRow rowOtgruzka = this.dsKamaz.тботгрузки_продукции.Newтботгрузки_продукцииRow();

                    nomSkladaOtgruzki = Convert.ToInt32((dataGrid4.Items[0] as DataRowView).Row.ItemArray[2]);

                    rowOtgruzka["ID_отгрузки"] = (int)(dsKamaz.тботгрузки_продукции.Rows[dsKamaz.тботгрузки_продукции.Count - 1][0]) + 1;
                    rowOtgruzka["Номер_накладной"] = tekushNomNakladn;
                    rowOtgruzka["Номер_склада"] = nomSkladaOtgruzki;
                    rowOtgruzka["Код_продукции"] = kodProd;
                    rowOtgruzka["Количество"] = Convert.ToInt32(textBox3.Text);

                    //rowOtgruzka["Дата поставки"]
                    rowOtgruzka["Отгружен"] = 0;

                    dsKamaz.тботгрузки_продукции.Rows.Add(rowOtgruzka);
                    textBox2.Text = "";
                }
                else
                    MessageBox.Show("Выбранное количество = 0 !");
            }

        }

        private void buttonGotovoZayavka_Click(object sender, RoutedEventArgs e)
        {
            tekushNomNakladn = 0;

            WMS_Kamaz_ds.тбпоставки_продукцииDataTable dsPost = (WMS_Kamaz_ds.тбпоставки_продукцииDataTable)dsKamaz.тбпоставки_продукции.GetChanges();

            if (dsPost != null)
                try
                {
                    taPostavki.Update(dsPost);
                    dsPost.Dispose();
                    dsKamaz.тбпоставки_продукции.AcceptChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка создания заявки [готово] (добавление поставки)" + ex, "Предупреждение");
                    this.dsKamaz.тбпоставки_продукции.RejectChanges();
                }

            WMS_Kamaz_ds.тботгрузки_продукцииDataTable dsOtgruz = (WMS_Kamaz_ds.тботгрузки_продукцииDataTable)dsKamaz.тботгрузки_продукции.GetChanges();
            if (dsOtgruz != null)
            {
                try
                {
                    taOtgruzki.Update(dsOtgruz);
                    dsOtgruz.Dispose();
                    dsKamaz.тботгрузки_продукции.AcceptChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка создания заявки [готово] (добавление отгрузки)" + ex, "Предупреждение");
                    this.dsKamaz.тботгрузки_продукции.RejectChanges();
                }
            }

            UstanovitRedaktZayavki(false);

        }

        private void UstanovitRedaktZayavki(bool flag)
        {
            if (flag)
            {
                buttonSozdatZayavku.IsEnabled = false;
                buttonOtmenZayavku.IsEnabled = true;
                buttonDobavVZayavku.IsEnabled = true;
                buttonGotovoZayavka.IsEnabled = false;
                dataGrid4.IsEnabled = true;
                buttonZayavkaClearFilter.IsEnabled = true;


                textBox3.Text = "";
                textBox2.Text = "";
                label6.IsEnabled = true;
                label7.IsEnabled = true;
                comboBox3.IsEnabled = true;
                comboBox4.IsEnabled = true;
                label11.IsEnabled = true;
                textBox3.IsEnabled = true;
                textBox2.IsEnabled = true;
                label8.IsEnabled = true;
                label27.IsEnabled = true;
                textBoxZayavkaNameFilter.IsEnabled = true;
            }
            else
            {
                buttonSozdatZayavku.IsEnabled = true;
                buttonOtmenZayavku.IsEnabled = false;
                buttonDobavVZayavku.IsEnabled = false;
                buttonGotovoZayavka.IsEnabled = false;
                dataGrid4.IsEnabled = false;
                buttonZayavkaClearFilter.IsEnabled=false;

                textBox3.Text = "";
                textBox2.Text = "";
                label6.IsEnabled = false;
                label7.IsEnabled = false;
                comboBox3.IsEnabled = false;
                comboBox4.IsEnabled = false;
                label11.IsEnabled = false;
                textBox3.IsEnabled = false;
                textBox2.IsEnabled = false;
                label8.IsEnabled = false;
                label27.IsEnabled = false;
                textBoxZayavkaNameFilter.IsEnabled = false;
            }

        }

        private void textBox2_TextChanged(object sender, TextChangedEventArgs e)
        {
            double n = 0;
            if (textBox2.Text != "" && textBox2.Text != null && double.TryParse(textBox2.Text,out n))
            {
                dv4.RowFilter = "Код_продукции = " + textBox2.Text.Trim() + " AND Номер_склада <> " + comboBox2.SelectedValue + " AND Номер_склада = " + comboBox4.SelectedValue; ;
            }
            else
            {
                dv4.RowFilter = "Номер_склада <> " + comboBox2.SelectedValue;
                if (comboBox4.SelectedValue != null)
                    dv4.RowFilter = "Номер_склада <> " + comboBox2.SelectedValue + " AND " + "Номер_склада = " + comboBox4.SelectedValue;
            }
        }

        private void textBoxZayavkaNameFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (textBoxZayavkaNameFilter.Text != "" && textBoxZayavkaNameFilter.Text != null)
            {
                dv4.RowFilter = "Наименование LIKE '" + textBoxZayavkaNameFilter.Text.Trim() + "*' AND Номер_склада <> " + comboBox2.SelectedValue + " AND Номер_склада = " + comboBox4.SelectedValue; ;
            }
            else
            {
                dv4.RowFilter = "Номер_склада <> " + comboBox2.SelectedValue;
                if (comboBox4.SelectedValue != null)
                    dv4.RowFilter = "Номер_склада <> " + comboBox2.SelectedValue + " AND " + "Номер_склада = " + comboBox4.SelectedValue;
            }
        }

        //Сохранить изменения в отгрузках
        private void saveChangesOtgruzki()
        {
            WMS_Kamaz_ds.тботгрузки_продукцииDataTable ds2 = (WMS_Kamaz_ds.тботгрузки_продукцииDataTable)dsKamaz.тботгрузки_продукции.GetChanges();

            if (ds2 != null)
                try
                {
                    taOtgruzki.Update(ds2);
                    ds2.Dispose();
                    dsKamaz.тботгрузки_продукции.AcceptChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка вставки/изменения записи в тыблице отгрузки" + ex, "Предупреждение");
                    this.dsKamaz.тботгрузки_продукции.RejectChanges();
                }
        }

        //Сохранить изменения в текущей продукции
        private void saveChangesTekushProd()
        {
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
        }

        private void saveChangesPostavki()
        {
            WMS_Kamaz_ds.тбпоставки_продукцииDataTable dsPost = (WMS_Kamaz_ds.тбпоставки_продукцииDataTable)dsKamaz.тбпоставки_продукции.GetChanges();

            if (dsPost != null)
                try
                {
                    taPostavki.Update(dsPost);
                    dsPost.Dispose();
                    dsKamaz.тбпоставки_продукции.AcceptChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка принятия продукции [поставки]" + ex, "Предупреждение");
                    this.dsKamaz.тбпоставки_продукции.RejectChanges();
                }
        }

        private void buttonOtgruzkiOtgruzit_Click(object sender, RoutedEventArgs e)
        {//отгрузить продукцию для отгрузки, если количество отгрузки меньше имеющегося - то вычисть кол-во, иначе если равно - удалить этот груз из текущих
            try
            {
                if (dataGrid1.Items.Count != 0)
                {
                    DataRow rowOtgruzkiProd = this.dsKamaz.тботгрузки_продукции.Newтботгрузки_продукцииRow();
                    bool delItem = true;
                    int kodProd = 0, nomSklada = 0, kolVo = 0, pos = 0;
                    for (int i = 0; i < dataGrid1.Items.Count; i++)
                    {
                        delItem = true;
                        foreach (DataRow dr in dsKamaz.тбпродукция_на_складе)
                        {
                            if (Convert.ToInt32(dr["Код_продукции"]) == Convert.ToInt32((dataGrid1.Items[i] as DataRowView).Row.ItemArray[3]) && Convert.ToInt32(dr["Номер_склада"]) == Convert.ToInt32(comboBox2.SelectedValue))
                            {
                                if (Convert.ToInt32(dr["Количество"]) > Convert.ToInt32((dataGrid1.Items[i] as DataRowView).Row.ItemArray[4]))
                                {
                                    int tekushKolVo = Convert.ToInt32(dr["Количество"]);
                                    int otgrKolVo = Convert.ToInt32((dataGrid1.Items[i] as DataRowView).Row.ItemArray[4]);
                                    dr["Количество"] = tekushKolVo - otgrKolVo;
                                    delItem = false;
                                    break;
                                }
                            }
                        }
                        if (delItem)
                        {
                            kodProd = Convert.ToInt32((dataGrid1.Items[i] as DataRowView).Row.ItemArray[3]);
                            nomSklada = Convert.ToInt32(comboBox2.SelectedValue);
                            kolVo = Convert.ToInt32((dataGrid1.Items[i] as DataRowView).Row.ItemArray[4]);

                            foreach (DataRow drNaSklade in dsKamaz.тбпродукция_на_складе)
                            {
                                pos++;
                                if (Convert.ToInt32(drNaSklade["Код_продукции"]) == kodProd && Convert.ToInt32(drNaSklade["Номер_склада"]) == nomSklada && Convert.ToInt32(drNaSklade["Количество"]) == kolVo)
                                {
                                    break;
                                }
                            }

                            this.dsKamaz.тбпродукция_на_складе.Rows[pos - 1].Delete();
                            if (this.dsKamaz.тбпродукция_на_складе.GetChanges(DataRowState.Deleted) != null)
                            {
                                try
                                {
                                    this.taProdNaSklade.Update(dsKamaz.тбпродукция_на_складе);
                                    this.dsKamaz.тбпродукция_на_складе.AcceptChanges();
                                }
                                catch (Exception x)
                                {
                                    string er = x.Message.ToString();
                                    MessageBox.Show("Ошибка удаления продукции из текущих при отгрузке " + er, "Предупреждение");
                                    this.dsKamaz.тбпродукция_на_складе.RejectChanges();
                                }
                            }
                            pos = 0;
                        }

                    }

                    //установить получен = true и дату (в тбпоставки)
                    //КОСТЫЛЬ - ОТГРУЖАЕТ ВСЕ (!) ОТГРУЗКИ С ВЫБРАННОЙ ВВЕРХУ БАЗЫ
                    foreach (DataRow drOtgr in dsKamaz.тботгрузки_продукции)
                    {

                        // && Convert.ToInt32(drOtgr["Код_продукции"]) == Convert.ToInt32((dataGrid1.Items[i] as DataRowView).Row.ItemArray[3])

                        if (Convert.ToInt32(drOtgr["Номер_склада"]) == Convert.ToInt32(comboBox2.SelectedValue) && Convert.ToInt32(drOtgr["Отгружен"]) != 1)
                        {
                            drOtgr["Дата отгрузки"] = DateTime.Now;
                            drOtgr["Отгружен"] = 1;
                        }
                    }

                    saveChangesOtgruzki();
                    saveChangesTekushProd();

                    updateDG2andDG4();

                }
            }
            catch (Exception ex) { MessageBox.Show("Ошибка отгрузки" + ex.ToString()); }

        }

        //проверка отгружена ли продукция с требуемого склада
        private bool isOtgruzkiOtgrugeny(int nomSklada)
        {
            var kolvoOtgrugenyh = (from otgr in dsKamaz.тботгрузки_продукции
                        join post in dsKamaz.тбпоставки_продукции
                        on otgr.Номер_накладной equals post.Номер_накладной
                        where otgr.Отгружен == false
                        select new
                        {
                            otgr.ID_отгрузки,

                        }).ToList();


            if (kolvoOtgrugenyh.Count == 0)
                return true;
            else
                return false;
        }

        private void buttonPostavkiPrinyat_Click(object sender, RoutedEventArgs e)
        {//принять продукцию из поставки, если груз уже есть на складе - добавить к нему новое количество, иначе создать новый
            try
            {
                if (dataGrid3.Items.Count != 0)
                {
                    if (isOtgruzkiOtgrugeny(Convert.ToInt32(comboBox2.SelectedValue)))
                    {

                        DataRow rowTekushProd = this.dsKamaz.тбпродукция_на_складе.Newтбпродукция_на_складеRow();
                        bool addItem = true;

                        for (int i = 0; i < dataGrid3.Items.Count; i++)
                        {
                            addItem = true;
                            foreach (DataRow dr in dsKamaz.тбпродукция_на_складе)
                            {
                                if (Convert.ToInt32(dr["Код_продукции"]) == Convert.ToInt32((dataGrid3.Items[i] as DataRowView).Row.ItemArray[3]) && Convert.ToInt32(dr["Номер_склада"]) == Convert.ToInt32(comboBox2.SelectedValue))
                                {
                                    dr["Количество"] = Convert.ToInt32(dr["Количество"]) + Convert.ToInt32((dataGrid3.Items[i] as DataRowView).Row.ItemArray[4]);
                                    addItem = false;

                                    saveChangesTekushProd();

                                    break;
                                }
                            }
                            if (addItem)
                            {
                                rowTekushProd["ID_хранения"] = Convert.ToInt32((dsKamaz.тбпродукция_на_складе.Rows[dsKamaz.тбпродукция_на_складе.Count - 1][0])) + 1;
                                rowTekushProd["Код_продукции"] = Convert.ToInt32((dataGrid3.Items[i] as DataRowView).Row.ItemArray[3]);
                                rowTekushProd["Номер_склада"] = comboBox2.SelectedValue;
                                rowTekushProd["Количество"] = Convert.ToInt32((dataGrid3.Items[i] as DataRowView).Row.ItemArray[4]);
                                rowTekushProd["Дата получения"] = DateTime.Now;
                                dsKamaz.тбпродукция_на_складе.Rows.Add(rowTekushProd);

                                saveChangesTekushProd();
                            }
                        }

                        //установить получен = true и дату (в тбпоставки)
                        //КОСТЫЛЬ - ПРИНИМАЕТ ВСЕ (!) ПОСТАВКИ НА ВЫБРАННУЮ ВВЕРХУ БАЗУ
                        foreach (DataRow drPost in dsKamaz.тбпоставки_продукции)
                        {
                            if (Convert.ToInt32(drPost["Номер_склада"]) == Convert.ToInt32(comboBox2.SelectedValue) && Convert.ToInt32(drPost["Получен"]) != 1)
                            {
                                drPost["Дата поставки"] = DateTime.Now;
                                drPost["Получен"] = 1;
                            }
                        }

                        saveChangesPostavki();

                        updateDG2andDG4();

                    }
                    else MessageBox.Show("Продукция еще не отгружена !");
                }
            }
            catch (Exception ex) { MessageBox.Show("Ошибка принятия поставки" + ex.ToString()); }
        }

        //Добавить продукцию напримую из нуменклатуры на склад
        private void buttonDobProdIzNumenk_Click(object sender, RoutedEventArgs e)
        {
            ProductsWindow productsWindow = new ProductsWindow();
            productsWindow.Owner = Application.Current.MainWindow;
            productsWindow.nomerSklada = Convert.ToInt32(this.comboBox2.SelectedValue);
            productsWindow.Closing +=new System.ComponentModel.CancelEventHandler(productsWindow_Closing);

            productsWindow.buttonAddToCurrentProd.Visibility = System.Windows.Visibility.Visible;
            productsWindow.labelDobprod.Visibility = System.Windows.Visibility.Visible;
            productsWindow.labelKolVo.Visibility = System.Windows.Visibility.Visible;
            productsWindow.textBoxKolVo.Visibility = System.Windows.Visibility.Visible;

            productsWindow.ShowDialog();

        }

        //Добавить продукцию в нуменклатуру а затем на склад
        private void buttonDobProdNovyi_Click(object sender, RoutedEventArgs e)
        {
            ProductsWindow productsWindow = new ProductsWindow();
            productsWindow.Owner = Application.Current.MainWindow;
            productsWindow.nomerSklada = Convert.ToInt32(this.comboBox2.SelectedValue);
            productsWindow.Closing +=new System.ComponentModel.CancelEventHandler(productsWindow_Closing);

            productsWindow.buttonAddToCurrentProd.Visibility = System.Windows.Visibility.Visible;
            productsWindow.labelDobprod.Visibility = System.Windows.Visibility.Visible;
            productsWindow.labelKolVo.Visibility = System.Windows.Visibility.Visible;
            productsWindow.textBoxKolVo.Visibility = System.Windows.Visibility.Visible;
            productsWindow.buttonAddToCurrentProd.IsEnabled = false;
            productsWindow.ShowDialog();
        }

        void productsWindow_Closing(object sender, EventArgs e)
        {
            //MessageBox.Show("ASDASDDASS");
            updateDG2andDG4();
        }


        private void dataGrid3_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataGrid3.SelectedItems.Count == 1)
            {
                int nomNakladn = Convert.ToInt32((dataGrid3.Items[dataGrid3.SelectedIndex] as DataRowView).Row.ItemArray[1]);
                int kodProduct = Convert.ToInt32((dataGrid3.Items[dataGrid3.SelectedIndex] as DataRowView).Row.ItemArray[3]);

                DataRow[] dr1 = dsKamaz.тбнакладная.Select("Номер_накладной = " + nomNakladn);
                textBoxDataNakladn3.Text = dr1[0][1].ToString();

                var prodNaim = (from nomenkl in dsKamaz.тбноменкл_продукции
                                join postav in dsKamaz.тбпоставщик_продукции
                                on nomenkl.Код_поставщика equals postav.Код_поставщика
                                where nomenkl.Код_продукции == kodProduct
                                select new { nomenkl.Наименование }).ToList();
                textBoxNaimProd3.Text = prodNaim[0].Наименование.ToString();

                var postavNameAndEmail = (from nomekl in dsKamaz.тбноменкл_продукции
                            join postav in dsKamaz.тбпоставщик_продукции
                            on nomekl.Код_поставщика equals postav.Код_поставщика
                            where nomekl.Код_продукции == kodProduct
                            select postav).ToList() ;
                textBoxPostav3.Text = postavNameAndEmail[0][2].ToString();
                textBoxEmailPostav3.Text = postavNameAndEmail[0][4].ToString();

                var nomeklTaraMatStoim = (from nomekl in dsKamaz.тбноменкл_продукции
                                          join tara in dsKamaz.тбтара
                                          on nomekl.Код_тары equals tara.Код_тары
                                          where nomekl.Код_продукции == kodProduct
                                          select new { tara.Наименование, nomekl.Материал, nomekl.Стоимость_единицы }).ToList();
                textBoxNaimTari3.Text = nomeklTaraMatStoim[0].Наименование.ToString();
                textBoxMaterial3.Text = nomeklTaraMatStoim[0].Материал.ToString();
                textBoxStoimost3.Text = nomeklTaraMatStoim[0].Стоимость_единицы.ToString();               
            }
            else
            {
                textBoxNaimProd3.Text = "";
                textBoxDataNakladn3.Text = "";
                textBoxPostav3.Text = "";
                textBoxEmailPostav3.Text = "";
                textBoxNaimTari3.Text = "";
                textBoxMaterial3.Text = "";
                textBoxStoimost3.Text= "";
            }
        }
        
        //изменение БД при изменении данных в dataGrid1
        private void dataGrid1_CurrentCellChanged(object sender, EventArgs e)
        {
            WMS_Kamaz_ds.тботгрузки_продукцииDataTable ds2 = (WMS_Kamaz_ds.тботгрузки_продукцииDataTable)dsKamaz.тботгрузки_продукции.GetChanges();

            if (ds2 != null)
                try
                {
                    taOtgruzki.Update(ds2);
                    ds2.Dispose();
                    dsKamaz.тботгрузки_продукции.AcceptChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка вставки записи в базу данных wms_kamaz " + ex, "Предупреждение");
                    this.dsKamaz.тботгрузки_продукции.RejectChanges();
                }
        }

        //КОСТЫЛЬ - заменяет подчеркивания в загаловке столбца на пробел
        //СКРЫВАТЬ НЕ НУЖНЫЕ СТОЛБЦЫ ТУТ !
        private void dataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            string header = e.Column.Header.ToString();
            e.Column.Header = header.Replace("_", " ");

            if (e.Column.Header.ToString() == "ID поставки" || e.Column.Header.ToString() == "ID хранения" || e.Column.Header.ToString() == "ID отгрузки" || e.Column.Header.ToString() == "Дата отгрузки" || e.Column.Header.ToString() == "Отгружен" || e.Column.Header.ToString() == "Дата поставки" || e.Column.Header.ToString() == "Получен")
            {
                e.Column.Visibility = Visibility.Hidden;
            }
        }

        public void OpenLoadingWindow() //открытие окна загрузки (LoadingWindow) в новом потоке
        {
            newLoadingWindowThread = new Thread(new ThreadStart(() =>
            {
                // Создать и показать окно
                loadingWindow = new LoadingWindow();
                loadingWindow.ShowDialog();
                // Запустить выполнение Dispatcher
                System.Windows.Threading.Dispatcher.Run();
            }));
            // Установить apartment state
            newLoadingWindowThread.SetApartmentState(ApartmentState.STA);
            // Сделать поток фоновым
            newLoadingWindowThread.IsBackground = true;
            // Запустить поток
            newLoadingWindowThread.Start();
        }

        public void CloseLoadingWindow()//закрываем поток окна загрузки (LoadingWindow)
        {
            if (loadingWindow != null)
            {
                //loadingWindow.Close();
                newLoadingWindowThread.Abort();
                this.Activate();
            }
        }

        private MySqlConnection GetDatabaseConnection(string name)
        {// Проверка соединения с сервером базы данных
            MySqlConnection conn = null;
            
            ConnectionStringSettings setting = ConfigurationManager.ConnectionStrings[name];
            if (setting != null)
                try
                {
                    conn = new MySqlConnection(setting.ConnectionString);
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

        private void PersonalMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PersonalWindow personalWindow = new PersonalWindow();
            personalWindow.Owner = Application.Current.MainWindow;
            personalWindow.ShowDialog();
        }
        
        private void NomenklProductsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ProductsWindow productsWindow = new ProductsWindow();
            productsWindow.Owner = Application.Current.MainWindow;
            productsWindow.ShowDialog();
        }

        private void WarehouseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            WarehouseWindow warehouseWindow = new WarehouseWindow();
            warehouseWindow.Owner = Application.Current.MainWindow;
            warehouseWindow.ShowDialog();
        }

        private void TaraMenuItem_Click(object sender, RoutedEventArgs e)
        {
            TareWindow tareWindow = new TareWindow();
            tareWindow.Owner = Application.Current.MainWindow;
            tareWindow.ShowDialog();
        }

        private void ProviderMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ProviderWindow providerWindow = new ProviderWindow();
            providerWindow.Owner = Application.Current.MainWindow;
            providerWindow.ShowDialog();
        }

        private void JobMenuItem_Click(object sender, RoutedEventArgs e)
        {
            JobWindow jobWindow = new JobWindow();
            jobWindow.Owner = Application.Current.MainWindow;
            jobWindow.ShowDialog();
        }

        private void SubDivisionMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SubdivisionWindow subdivisionWindow = new SubdivisionWindow();
            subdivisionWindow.Owner = Application.Current.MainWindow;
            subdivisionWindow.ShowDialog();
        }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.Owner = Application.Current.MainWindow;
            aboutWindow.ShowDialog();
        }

        private void BackupMenuItem_Click(object sender, RoutedEventArgs e)
        {
            BackupWindow backupWindow = new BackupWindow();
            backupWindow.Owner = Application.Current.MainWindow;
            backupWindow.ShowDialog();
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Выйти из программы ?", "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                if (timerUpdateOtgruzki.IsEnabled)
                    timerUpdateOtgruzki.Stop();
                Application.Current.Shutdown();
            }
        }

        private void HelpMenuItem_Click(object sender, RoutedEventArgs e)
        {
            //HelpWindow helpWindow = new HelpWindow();
            //helpWindow.Owner = Application.Current.MainWindow;
            //helpWindow.ShowDialog();
            try
            {
                System.Diagnostics.Process.Start(@"wms_kamaz.chm");
            }
            catch { }
        
        }

        private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingWindow = new SettingsWindow();
            settingWindow.Owner = Application.Current.MainWindow;
            settingWindow.ShowDialog();
        }

        private void dataGrid4_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataGrid4.SelectedIndex != -1)
            {
                textBoxZayavkaNameFilter.Text = ((dataGrid4.Items[dataGrid4.SelectedIndex] as DataRowView).Row.ItemArray[0]).ToString();
                textBox2.Text = ((dataGrid4.Items[dataGrid4.SelectedIndex] as DataRowView).Row.ItemArray[1]).ToString();
            }
        }

        private void dataGrid1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataGrid1.SelectedItems.Count == 1)
            {
                int nomNakladn = Convert.ToInt32((dataGrid1.Items[dataGrid1.SelectedIndex] as DataRowView).Row.ItemArray[1]);
                int kodProduct = Convert.ToInt32((dataGrid1.Items[dataGrid1.SelectedIndex] as DataRowView).Row.ItemArray[3]);

                DataRow[] dr1 = dsKamaz.тбнакладная.Select("Номер_накладной = " + nomNakladn);
                textBoxDataNakladn1.Text = dr1[0][1].ToString();

                var prodNaim = (from nomenkl in dsKamaz.тбноменкл_продукции
                                join postav in dsKamaz.тбпоставщик_продукции
                                on nomenkl.Код_поставщика equals postav.Код_поставщика
                                where nomenkl.Код_продукции == kodProduct
                                select new { nomenkl.Наименование }).ToList();
                textBoxNaimProd1.Text = prodNaim[0].Наименование.ToString();

                var postavNameAndEmail = (from nomenkl in dsKamaz.тбноменкл_продукции
                                          join postav in dsKamaz.тбпоставщик_продукции
                                          on nomenkl.Код_поставщика equals postav.Код_поставщика
                                          where nomenkl.Код_продукции == kodProduct
                                          select postav).ToList();
                textBoxPostav1.Text = postavNameAndEmail[0][2].ToString();
                textBoxEmailPostav1.Text = postavNameAndEmail[0][4].ToString();

                var nomeklTaraMatStoim = (from nomenkl in dsKamaz.тбноменкл_продукции
                                          join tara in dsKamaz.тбтара
                                          on nomenkl.Код_тары equals tara.Код_тары
                                          where nomenkl.Код_продукции == kodProduct
                                          select new { tara.Наименование, nomenkl.Материал, nomenkl.Стоимость_единицы }).ToList();
                textBoxNaimTari1.Text = nomeklTaraMatStoim[0].Наименование.ToString();
                textBoxMaterial1.Text = nomeklTaraMatStoim[0].Материал.ToString();
                textBoxStoimost1.Text = nomeklTaraMatStoim[0].Стоимость_единицы.ToString();

            }
            else
            {
                textBoxNaimProd1.Text = "";
                textBoxDataNakladn1.Text = "";
                textBoxPostav1.Text = "";
                textBoxEmailPostav1.Text = "";
                textBoxNaimTari1.Text = "";
                textBoxMaterial1.Text = "";
                textBoxStoimost1.Text = "";
            }
        }

        private void buttonZayavkaClearFilter_Click(object sender, RoutedEventArgs e)
        {
            textBoxZayavkaNameFilter.Text = "";
            textBox2.Text = "";
        }

        private void DefaultMenuSizeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (DefaultMenuSize.IsChecked)
            {
                column0.Width = new GridLength(338);
                column2.Width = new GridLength(338);
                row2.Height = new GridLength(271);

                Splitter1.IsEnabled = false;
                Splitter2.IsEnabled = false;
                Splitter3.IsEnabled = false;
            }
            else
            {
                Splitter1.IsEnabled = true;
                Splitter2.IsEnabled = true;
                Splitter3.IsEnabled = true;
            }
        }

        private void AutoUpdateMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (AutoUpdate.IsChecked && !timerUpdateOtgruzki.IsEnabled)
            {
                timerUpdateOtgruzki.Start();
            }
            else if (timerUpdateOtgruzki.IsEnabled)
                timerUpdateOtgruzki.Stop();
        }

        private void timerUpdateOtgruzki_Tick(object sender, EventArgs e)
        {
            try
            {
                int index = comboBox1.SelectedIndex;

                taOtgruzki.Fill(dsKamaz.тботгрузки_продукции);
                taNakladnaya.Fill(dsKamaz.тбнакладная);

                comboBox1.SelectedIndex = index;
            }
            catch (Exception ex) { MessageBox.Show("Ошибка обновление \n" + ex); }

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (timerUpdateOtgruzki.IsEnabled)
                    timerUpdateOtgruzki.Stop();
            }
            catch { }
        }

        private void WordMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid1.SelectedItems.Count == 1)
            {
                WordOtgruzki wordOtgruzki = new WordOtgruzki();
                wordOtgruzki.Owner = Application.Current.MainWindow;
                wordOtgruzki.nomerSklada = Convert.ToInt32(comboBox2.SelectedValue);

                wordOtgruzki.nomerNakladnoi = Convert.ToInt32((dataGrid1.Items[dataGrid1.SelectedIndex] as DataRowView).Row.ItemArray[1]);

                wordOtgruzki.ShowDialog();
            }
            else
                MessageBox.Show("Не выбрана продукция для создания накладной");
        }

        private void buttonWordNakl_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid1.SelectedItems.Count == 1)
            {
                WordOtgruzki wordOtgruzki = new WordOtgruzki();
                wordOtgruzki.Owner = Application.Current.MainWindow;
                wordOtgruzki.nomerSklada = Convert.ToInt32(comboBox2.SelectedValue);

                wordOtgruzki.nomerNakladnoi = Convert.ToInt32((dataGrid1.Items[dataGrid1.SelectedIndex] as DataRowView).Row.ItemArray[1]);

                wordOtgruzki.ShowDialog();
            }
            else
                MessageBox.Show("Не выбрана продукция для создания накладной");
        }

        
    }
}
