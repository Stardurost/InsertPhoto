using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using File = System.IO.File;
using Npgsql;
using System.IO;
using System.Windows.Media.Animation;

namespace InsertPhoto
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        static byte[] _mainImageData = null;
        static string pathFile;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image | *.png; *.jpg; *.jpeg";
            if (ofd.ShowDialog() == true)
            {
                pathFile = ofd.FileName;
                _mainImageData = File.ReadAllBytes(ofd.FileName);
                ImageService.Source = new ImageSourceConverter()
                    .ConvertFrom(_mainImageData) as ImageSource;
            }
        }

        async private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            string connstr = String.Format("Server={0};Port={1};" + "User Id={2};Password={3};Database={4};",
            "localhost", 5432, "postgres", "1701", "PersonalAccount");
            string sqlInsert, sqlSelect, sqlUpdate;
            var now = DateTime.Today;
            var currentYear = now.Year.ToString();
            try
            {
                    NpgsqlConnection conn = new NpgsqlConnection(connstr);
                
                    sqlInsert = @"insert into pa.""VacationImage"" (""Picture"" , ""Year"") values (@PhotoByte, '" + currentYear + @"')";
                    sqlSelect = @"select Count(*) from pa.""VacationImage"" where ""Year"" = '" + currentYear + @"'";
                    sqlUpdate = @"update pa.""VacationImage"" set ""Picture"" = null , ""Year"" = '" + currentYear + @"'";
                    await conn.OpenAsync();
                    NpgsqlCommand cmd1 = new NpgsqlCommand(sqlSelect, conn);
                    var result = cmd1.ExecuteReader();
                    var count = 0;
                    if (result.HasRows) // если есть данные
                    {
                        while (result.Read())   // построчно считываем данные
                        {
                            count = Convert.ToInt32(result.GetValue(0));
                           
                        }
                    }
                    conn.Close();

                    if (count == 0)
                    {
                    await conn.OpenAsync();
                    using (NpgsqlCommand cmd = new NpgsqlCommand(sqlInsert, conn))
                        {
                            using (FileStream file = File.Open(pathFile, FileMode.Open))
                            {
                                try { cmd.Parameters.Add("@PhotoByte", NpgsqlTypes.NpgsqlDbType.Bytea, -1).Value = file; }
                                catch { }
                                await cmd.ExecuteNonQueryAsync();
                                file.Close();
                                conn.Close();
                            }
                        }
                    }
                    else {
                    await conn.OpenAsync();
                    using (NpgsqlCommand cmd = new NpgsqlCommand(sqlUpdate, conn))
                        {
                            using (FileStream file = File.Open(pathFile, FileMode.Open))
                            {
                                try { cmd.Parameters.Add("@PhotoByte", NpgsqlTypes.NpgsqlDbType.Bytea, -1).Value = file; }
                                catch { }
                                await cmd.ExecuteNonQueryAsync();
                                file.Close();
                                conn.Close();
                            }
                        }
                    }
                       
                MessageBox.Show("Картинка успешно загружена в базу данных");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Картинка не была загружена в базу данных");
            }

        }
    
    }


}
