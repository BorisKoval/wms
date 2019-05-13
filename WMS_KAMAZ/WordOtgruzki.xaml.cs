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
using Microsoft.Office.Interop.Word;
using System.IO;

namespace WMS_KAMAZ
{
    /// <summary>
    /// Логика взаимодействия для WordOtgruzki.xaml
    /// </summary>
    public partial class WordOtgruzki : System.Windows.Window
    {
        MySqlConnection conn;

        public WordOtgruzki()
        {
            InitializeComponent();

            conn = GetDatabaseConnection("WMS_KAMAZ.Properties.Settings.acsm_6fa30819604a3e7ConnectionString");
            if (conn == null)
            {
                MessageBox.Show("Возможно нет соеденения с Интернет !");
                System.Windows.Application.Current.Shutdown();
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
                taTara.Connection = conn;
                taPersonal.Connection = conn;
            }
            DataContext = dsKamaz;
        }

        public int nomerSklada { get; set; }
        public int nomerNakladnoi { get; set; }

        WMS_Kamaz_ds dsKamaz = new WMS_Kamaz_ds();

        WMS_Kamaz_dsTableAdapters.тбноменкл_продукцииTableAdapter taNomenkl = new WMS_Kamaz_dsTableAdapters.тбноменкл_продукцииTableAdapter();

        WMS_Kamaz_dsTableAdapters.тботгрузки_продукцииTableAdapter taOtgruzki = new WMS_Kamaz_dsTableAdapters.тботгрузки_продукцииTableAdapter();

        WMS_Kamaz_dsTableAdapters.тбпоставки_продукцииTableAdapter taPostavki = new WMS_Kamaz_dsTableAdapters.тбпоставки_продукцииTableAdapter();

        WMS_Kamaz_dsTableAdapters.тбпродукция_на_складеTableAdapter taProdNaSklade = new WMS_Kamaz_dsTableAdapters.тбпродукция_на_складеTableAdapter();

        WMS_Kamaz_dsTableAdapters.тбскладTableAdapter taSklad = new WMS_Kamaz_dsTableAdapters.тбскладTableAdapter();

        WMS_Kamaz_dsTableAdapters.тбподразделениеTableAdapter taPodrazdelenie = new WMS_Kamaz_dsTableAdapters.тбподразделениеTableAdapter();

        WMS_Kamaz_dsTableAdapters.тбнакладнаяTableAdapter taNakladnaya = new WMS_Kamaz_dsTableAdapters.тбнакладнаяTableAdapter();

        WMS_Kamaz_dsTableAdapters.тбтараTableAdapter taTara = new WMS_Kamaz_dsTableAdapters.тбтараTableAdapter();

        WMS_Kamaz_dsTableAdapters.тбперсоналTableAdapter taPersonal = new WMS_Kamaz_dsTableAdapters.тбперсоналTableAdapter();

        private void Wms_KamazFill()
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
                taTara.Fill(dsKamaz.тбтара);
                taPersonal.Fill(dsKamaz.тбперсонал);
            }
            catch (Exception ex) { MessageBox.Show("Ошибка в работе метода Fill DataAdapter: " + ex); }
        }

        private void buttonCreate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (listBox1.SelectedValue != null)
                {
                    
                    Microsoft.Office.Interop.Word.Application wordApp = new Microsoft.Office.Interop.Word.Application();
                    Document wordDoc = wordApp.Documents.Add(@"\M-15.docx");

                    DataRow[] dr1 = dsKamaz.тбнакладная.Select("Номер_накладной = " + nomerNakladnoi);

                    string data_nakladnoi = dr1[0][1].ToString();
                    wordDoc.Bookmarks["Data_nakl"].Range.Text = data_nakladnoi;

                    wordDoc.Bookmarks["Dolgnost_personala"].Range.Text = dsKamaz.тбперсонал.Select("ID_персонала = " + Convert.ToInt32(listBox1.SelectedValue))[0][2].ToString();

                    wordDoc.Bookmarks["Dolgnost_personala2"].Range.Text = dsKamaz.тбперсонал.Select("ID_персонала = " + Convert.ToInt32(listBox1.SelectedValue))[0][2].ToString();

                    wordDoc.Bookmarks["FIO_personala"].Range.Text = dsKamaz.тбперсонал.Select("ID_персонала = " + Convert.ToInt32(listBox1.SelectedValue))[0][4].ToString();

                    wordDoc.Bookmarks["FIO_personala2"].Range.Text = dsKamaz.тбперсонал.Select("ID_персонала = " + Convert.ToInt32(listBox1.SelectedValue))[0][4].ToString();

                    wordDoc.Bookmarks["ID_personala"].Range.Text = listBox1.SelectedValue.ToString();

                    wordDoc.Bookmarks["Kod_podrazd_otpravitel"].Range.Text = dsKamaz.тбсклад.Select("Номер_склада = " + nomerSklada)[0][1].ToString();

                    int nomSkladaPoluch = Convert.ToInt32(dsKamaz.тбпоставки_продукции.Select("Номер_накладной = " + nomerNakladnoi)[0][2]);
                    wordDoc.Bookmarks["Kod_podrazd_poluchatel"].Range.Text = dsKamaz.тбсклад.Select("Номер_склада = " + nomSkladaPoluch)[0][1].ToString();

                    int kodPodrazdOtprav = Convert.ToInt32(dsKamaz.тбсклад.Select("Номер_склада = " + nomerSklada)[0][1]);
                    wordDoc.Bookmarks["Naim_podrazd_otpravitel"].Range.Text = dsKamaz.тбподразделение.Select("Код_подразделения = " + kodPodrazdOtprav)[0][1].ToString();

                    int kodPodrazdPoluchatel = Convert.ToInt32(dsKamaz.тбсклад.Select("Номер_склада = " + nomSkladaPoluch)[0][1]);
                    wordDoc.Bookmarks["Naim_podrazd_poluchatel"].Range.Text = dsKamaz.тбподразделение.Select("Код_подразделения = " + kodPodrazdPoluchatel)[0][1].ToString();

                    wordDoc.Bookmarks["Naim_podrazd_poluchatel2"].Range.Text = dsKamaz.тбподразделение.Select("Код_подразделения = " + kodPodrazdPoluchatel)[0][1].ToString();

                    wordDoc.Bookmarks["Nomer_nakladnoi"].Range.Text = nomerNakladnoi.ToString();

                    var prodOtgruzWord = (from nomenkl in dsKamaz.тбноменкл_продукции
                                          join otgruzka in dsKamaz.тботгрузки_продукции
                                          on nomenkl.Код_продукции equals otgruzka.Код_продукции
                                          where otgruzka.Номер_накладной == nomerNakladnoi
                                          select new
                                          {
                                              nomenkl.Наименование,
                                              nomenkl.Код_продукции,
                                              nomenkl.Единицы_измерения,
                                              otgruzka.Количество,
                                              nomenkl.Стоимость_единицы,
                                          }).ToList();

                    int itogo = 0;

                    for (int i = 0; i < prodOtgruzWord.Count; i++)
                    {

                        wordDoc.Bookmarks["Edin_izmereniya"].Range.Text += prodOtgruzWord[i].Единицы_измерения + "\r\n";
                        wordDoc.Bookmarks["Kod_produkcii"].Range.Text += prodOtgruzWord[i].Код_продукции.ToString() + "\r\n";
                        wordDoc.Bookmarks["KolVo_otgruz"].Range.Text += prodOtgruzWord[i].Количество.ToString() + "\r\n";
                        wordDoc.Bookmarks["KolVo_otgruz2"].Range.Text += prodOtgruzWord[i].Количество.ToString() + "\r\n";
                        wordDoc.Bookmarks["Naim_produkcii"].Range.Text += prodOtgruzWord[i].Наименование + "\r\n";
                        wordDoc.Bookmarks["Stoim_edinici"].Range.Text += prodOtgruzWord[i].Стоимость_единицы.ToString() + "\r\n";
                        wordDoc.Bookmarks["Summa"].Range.Text += (prodOtgruzWord[i].Количество * prodOtgruzWord[i].Стоимость_единицы).ToString() + "\r\n";

                        itogo += Convert.ToInt32(prodOtgruzWord[i].Количество * prodOtgruzWord[i].Стоимость_единицы);

                        wordDoc.Bookmarks["Summa_s_NDS"].Range.Text += (prodOtgruzWord[i].Количество * prodOtgruzWord[i].Стоимость_единицы).ToString() + "\r\n";

                        wordDoc.Bookmarks["NDS"].Range.Text += "0,00 \n";
                    }

                    wordDoc.Bookmarks["Itogo_NDS"].Range.Text = "0,00";
                    wordDoc.Bookmarks["Itogo"].Range.Text = itogo.ToString();
                    wordDoc.Bookmarks["Itogo_s_NDS"].Range.Text = itogo.ToString();
                    wordDoc.Bookmarks["Vsego_product"].Range.Text = prodOtgruzWord.Count.ToString() + " ";

                    wordApp.Visible = true;

                    wordDoc.SaveAs2(FileName: textBoxSavePath.Text);
                }
                else { MessageBox.Show("Сотрудник не выбран"); }
            }
            catch (Exception ex) { MessageBox.Show("Ошибка создания !"+ex); }
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Wms_KamazFill();
            listBox1.ItemsSource = dsKamaz.тбперсонал.Select("Номер_склада = " + nomerSklada);
            listBox1.SelectedIndex = 0;
            textBoxNomNakl.Text = nomerNakladnoi.ToString();
            DataRow[] dr1 = dsKamaz.тбнакладная.Select("Номер_накладной = " + nomerNakladnoi);
            textBoxDataNakl.Text = dr1[0][1].ToString();
            textBoxSavePath.Text = string.Format(@"C:\Накладная на отгрузку №{0} [{1}].docx", nomerNakladnoi, DateTime.Today.ToString("d"));

            textBoxSkladPoluch.Text = (dsKamaz.тбпоставки_продукции.Select("Номер_накладной = " + nomerNakladnoi)[0][2]).ToString();

            var prodOtgruzWord = (from nomenkl in dsKamaz.тбноменкл_продукции
                                          join otgruzka in dsKamaz.тботгрузки_продукции
                                          on nomenkl.Код_продукции equals otgruzka.Код_продукции
                                          where otgruzka.Номер_накладной == nomerNakladnoi
                                          select new
                                          {
                                              nomenkl.Наименование,
                                              otgruzka.Количество,
                                              nomenkl.Стоимость_единицы,
                                          }).ToList();
            double itogSum=0;
            foreach(var el in prodOtgruzWord)
            {
                itogSum+=el.Количество*el.Стоимость_единицы;
            }
            textBoxKolVoNaim.Text = prodOtgruzWord.Count.ToString();
            textBoxItogovayaSumma.Text = itogSum.ToString();

        }

        private MySqlConnection GetDatabaseConnection(string name)
        {
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

        private void listBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void buttonSavePath_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveDialog = new Microsoft.Win32.SaveFileDialog();
            saveDialog.DefaultExt = "";
            //saveDialog.Filter = "SQL (*.sql)|*.sql";
            saveDialog.FileName = string.Format(@"C:\Накладная на отгрузку №{0} [{1}].docx", nomerNakladnoi, DateTime.Today.ToString("d"));

            Nullable<bool> result = saveDialog.ShowDialog();

            if (result == true)
            {
                string filename = saveDialog.FileName;
                textBoxSavePath.Text = filename;
            }
        }
    }
}
/*
 Bookmark bmData_nakl = wordDoc.Bookmarks["Data_nakl"];
                    Microsoft.Office.Interop.Word.Range rngData_nakl = bmData_nakl.Range;
                    rngData_nakl.Text = data_nakladnoi;
 
                    Bookmark bmDolgnost_personala = wordDoc.Bookmarks["Dolgnost_personala"];
                    Microsoft.Office.Interop.Word.Range rngDolgnost_personala = bmDolgnost_personala.Range;
                    rngDolgnost_personala.Text = dsKamaz.тбперсонал.Select("ID_персонала = " + Convert.ToInt32(listBox1.SelectedValue))[0][2].ToString();

                    Bookmark bmDolgnost_personala2 = wordDoc.Bookmarks["Dolgnost_personala2"];
                    Microsoft.Office.Interop.Word.Range rngDolgnost_personala2 = bmDolgnost_personala2.Range;
                    rngDolgnost_personala2.Text = dsKamaz.тбперсонал.Select("ID_персонала = " + Convert.ToInt32(listBox1.SelectedValue))[0][2].ToString();

                    Bookmark bmFIO_personala = wordDoc.Bookmarks["FIO_personala"];
                    Microsoft.Office.Interop.Word.Range rngFIO_personala = bmFIO_personala.Range;
                    rngFIO_personala.Text = dsKamaz.тбперсонал.Select("ID_персонала = " + Convert.ToInt32(listBox1.SelectedValue))[0][4].ToString();

                    Bookmark bmFIO_personala2 = wordDoc.Bookmarks["FIO_personala2"];
                    Microsoft.Office.Interop.Word.Range rngFIO_personala2 = bmFIO_personala2.Range;
                    rngFIO_personala2.Text = dsKamaz.тбперсонал.Select("ID_персонала = " + Convert.ToInt32(listBox1.SelectedValue))[0][4].ToString();

                    Bookmark bmID_personala = wordDoc.Bookmarks["ID_personala"];
                    Microsoft.Office.Interop.Word.Range rngID_personala = bmID_personala.Range;
                    rngID_personala.Text = listBox1.SelectedValue.ToString();

                    Bookmark bmKod_podrazd_otpravitel = wordDoc.Bookmarks["Kod_podrazd_otpravitel"];
                    Microsoft.Office.Interop.Word.Range rngKod_podrazd_otpravitel = bmKod_podrazd_otpravitel.Range;
                    rngKod_podrazd_otpravitel.Text = dsKamaz.тбсклад.Select("Номер_склада = " + nomerSklada)[0][1].ToString();

                    Bookmark bmKod_podrazd_poluchatel = wordDoc.Bookmarks["Kod_podrazd_poluchatel"];
                    Microsoft.Office.Interop.Word.Range rngKod_podrazd_poluchatel = bmKod_podrazd_poluchatel.Range;
                    int nomSkladaPoluch = Convert.ToInt32(dsKamaz.тбпоставки_продукции.Select("Номер_накладной = " + nomerNakladnoi)[0][2]);
                    rngKod_podrazd_poluchatel.Text = dsKamaz.тбсклад.Select("Номер_склада = " + nomSkladaPoluch)[0][1].ToString();

                    Bookmark bmNaim_podrazd_otpravitel = wordDoc.Bookmarks["Naim_podrazd_otpravitel"];
                    Microsoft.Office.Interop.Word.Range rngNaim_podrazd_otpravitel = bmNaim_podrazd_otpravitel.Range;
                    int kodPodrazdOtprav = Convert.ToInt32(dsKamaz.тбсклад.Select("Номер_склада = " + nomerSklada)[0][1]);
                    rngNaim_podrazd_otpravitel.Text = dsKamaz.тбподразделение.Select("Код_подразделения = " + kodPodrazdOtprav)[0][1].ToString();

                    Bookmark bmNaim_podrazd_poluchatel = wordDoc.Bookmarks["Naim_podrazd_poluchatel"];
                    Microsoft.Office.Interop.Word.Range rngNaim_podrazd_poluchatel = bmNaim_podrazd_poluchatel.Range;
                    int kodPodrazdPoluchatel = Convert.ToInt32(dsKamaz.тбсклад.Select("Номер_склада = " + nomSkladaPoluch)[0][1]);
                    rngNaim_podrazd_poluchatel.Text = dsKamaz.тбподразделение.Select("Код_подразделения = " + kodPodrazdPoluchatel)[0][1].ToString();

                    Bookmark bmNaim_podrazd_poluchatel2 = wordDoc.Bookmarks["Naim_podrazd_poluchatel2"];
                    Microsoft.Office.Interop.Word.Range rngNaim_podrazd_poluchatel2 = bmNaim_podrazd_poluchatel2.Range;
                    rngNaim_podrazd_poluchatel2.Text = dsKamaz.тбподразделение.Select("Код_подразделения = " + kodPodrazdPoluchatel)[0][1].ToString();

                    Bookmark bmNomer_nakladnoi = wordDoc.Bookmarks["Nomer_nakladnoi"];
                    Microsoft.Office.Interop.Word.Range rngNomer_nakladnoi = bmNomer_nakladnoi.Range;
                    rngNomer_nakladnoi.Text = nomerNakladnoi.ToString();

                    var prodOtgruzWord = (from nomenkl in dsKamaz.тбноменкл_продукции
                                          join otgruzka in dsKamaz.тботгрузки_продукции
                                          on nomenkl.Код_продукции equals otgruzka.Код_продукции
                                          where otgruzka.Номер_накладной == nomerNakladnoi
                                          select new
                                          {
                                              nomenkl.Наименование,
                                              nomenkl.Код_продукции,
                                              nomenkl.Единицы_измерения,
                                              otgruzka.Количество,
                                              nomenkl.Стоимость_единицы,
                                          }).ToList();

                    Bookmark bmEdin_izmereniya = wordDoc.Bookmarks["Edin_izmereniya"];
                    Microsoft.Office.Interop.Word.Range rngEdin_izmereniya = bmEdin_izmereniya.Range;

                    Bookmark bmKod_produkcii = wordDoc.Bookmarks["Kod_produkcii"];
                    Microsoft.Office.Interop.Word.Range rngKod_produkcii = bmKod_produkcii.Range;

                    Bookmark bmKolVo_otgruz = wordDoc.Bookmarks["KolVo_otgruz"];
                    Microsoft.Office.Interop.Word.Range rngKolVo_otgruz = bmKolVo_otgruz.Range;

                    Bookmark bmKolVo_otgruz2 = wordDoc.Bookmarks["KolVo_otgruz2"];
                    Microsoft.Office.Interop.Word.Range rngKolVo_otgruz2 = bmKolVo_otgruz2.Range;

                    Bookmark bmNaim_produkcii = wordDoc.Bookmarks["Naim_produkcii"];
                    Microsoft.Office.Interop.Word.Range rngNaim_produkcii = bmNaim_produkcii.Range;

                    Bookmark bmStoim_edinici = wordDoc.Bookmarks["Stoim_edinici"];
                    Microsoft.Office.Interop.Word.Range rngStoim_edinici = bmStoim_edinici.Range;

                    Bookmark bmSumma = wordDoc.Bookmarks["Summa"];
                    Microsoft.Office.Interop.Word.Range rngSumma = bmSumma.Range;

                    Bookmark bmItogo = wordDoc.Bookmarks["Itogo"];
                    Microsoft.Office.Interop.Word.Range rngItogo = bmItogo.Range;
                    int itogo = 0;

                    Bookmark bmItogo_NDS = wordDoc.Bookmarks["Itogo_NDS"];
                    Microsoft.Office.Interop.Word.Range rngItogo_NDS = bmItogo_NDS.Range;

                    Bookmark bmItogo_s_NDS = wordDoc.Bookmarks["Itogo_s_NDS"];
                    Microsoft.Office.Interop.Word.Range rngIItogo_s_NDS = bmItogo_s_NDS.Range;

                    Bookmark bmNDS = wordDoc.Bookmarks["NDS"];
                    Microsoft.Office.Interop.Word.Range rngNDS = bmNDS.Range;

                    Bookmark bmSumma_s_NDS = wordDoc.Bookmarks["Summa_s_NDS"];
                    Microsoft.Office.Interop.Word.Range rngSumma_s_NDS = bmSumma_s_NDS.Range;

                    Bookmark bmVsego_product = wordDoc.Bookmarks["Vsego_product"];
                    Microsoft.Office.Interop.Word.Range rngVsego_product = bmVsego_product.Range;

                    for (int i = 0; i < prodOtgruzWord.Count; i++)
                    {

                        rngEdin_izmereniya.Text += prodOtgruzWord[i].Единицы_измерения + "\r\n";
                        rngKod_produkcii.Text += prodOtgruzWord[i].Код_продукции.ToString() + "\r\n";
                        rngKolVo_otgruz.Text += prodOtgruzWord[i].Количество.ToString() + "\r\n";
                        rngKolVo_otgruz2.Text += prodOtgruzWord[i].Количество.ToString() + "\r\n";
                        rngNaim_produkcii.Text += prodOtgruzWord[i].Наименование + "\r\n";
                        rngStoim_edinici.Text += prodOtgruzWord[i].Стоимость_единицы.ToString() + "\r\n";
                        rngSumma.Text += (prodOtgruzWord[i].Количество * prodOtgruzWord[i].Стоимость_единицы).ToString() + "\r\n";

                        itogo += Convert.ToInt32(prodOtgruzWord[i].Количество * prodOtgruzWord[i].Стоимость_единицы);

                        rngSumma_s_NDS.Text += (prodOtgruzWord[i].Количество * prodOtgruzWord[i].Стоимость_единицы).ToString() + "\r\n";

                        rngNDS.Text += "0,00 \n";

                    }

                    rngItogo_NDS.Text = "0,00";
                    rngItogo.Text = itogo.ToString();
                    rngIItogo_s_NDS.Text = itogo.ToString();
                    rngVsego_product.Text = prodOtgruzWord.Count.ToString()+" ";*/