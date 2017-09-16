using System;
using System.Collections.Generic;
using System.IO;
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

namespace TcpChatClientWPF
{
    /// <summary>
    /// Логика взаимодействия для Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        static string nameFile = Environment.CurrentDirectory + @"\Settings.txt";

        public Settings()
        {
            InitializeComponent();

            if (!File.Exists(nameFile))
                File.Create(nameFile);

            StreamReader sr = new StreamReader(nameFile, Encoding.GetEncoding("windows-1251"));
            inputAddress.Text = sr.ReadLine();
            sr.Close();
        }

        private void CloseSettings_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
         
            if (inputAddress.Text.Trim().Length != 0)
            {
                StreamWriter sw = new StreamWriter(nameFile, false, Encoding.GetEncoding("windows-1251"));
                sw.WriteLine(inputAddress.Text);
                sw.Close();

                LoginWindow.host = inputAddress.Text;
                this.Close();
            }
            else MessageBox.Show("Заполните поле с адресом сервера!");
        }
    }
}
