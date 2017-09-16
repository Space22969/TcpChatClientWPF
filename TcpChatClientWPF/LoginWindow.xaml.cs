using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace TcpChatClientWPF
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        static string userName;
        static public string host = "";
        private const int port = 8888;
        static TcpClient client;
        static NetworkStream stream;
        static Thread receiveThread;
        static List<string> UsersOnline = new List<string>();

        public LoginWindow()
        {
            InitializeComponent();
            StreamReader sr = new StreamReader(Environment.CurrentDirectory + @"\Settings.txt", Encoding.GetEncoding("windows-1251"));
            host = sr.ReadLine();
            sr.Close();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Settings setForm = new Settings();
            setForm.Show();
        }

        private void RegButton_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow RegForm = new RegisterWindow(client, stream, host, port);
            RegForm.Show();
        }

        private void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            if ((Login.Text.Length != 0) && (Password.Password.Length != 0))
            {
                userName = Login.Text;
                string pass = Password.Password;
                try
                {
                    client = new TcpClient();
                    client.Connect(host, port); //подключение клиента
                    stream = client.GetStream(); // получаем поток

                    CoreServer.Message tempMes = new CoreServer.Message("connection", userName, "", "", null);
                    IFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, tempMes);

                    tempMes = new CoreServer.Message("signIn", userName, pass, DateTime.Now.ToString(), null);
                    formatter.Serialize(stream, tempMes);
                    //Зпускаем новый поток для получения данных
                    receiveThread = new Thread(new ThreadStart(ReceiveMessage));
                    receiveThread.SetApartmentState(ApartmentState.STA);
                    receiveThread.Start();
                }
                catch (Exception ex)
                {
                }

            }
        }

        public void ReceiveMessage()
        {
            bool flag = true;
            while (flag)
            {
                try
                {
                    CoreServer.Message RecMes;

                    IFormatter formatter = new BinaryFormatter();
                    RecMes = (CoreServer.Message)formatter.Deserialize(stream);

                    if (RecMes.type == "signAnNo")
                    {
                        MessageBox.Show(RecMes.body);//вывод сообщения
                        flag = false;

                    }
                    else if (RecMes.type == "signAnYes")
                    {
                        MessageBox.Show("Вы успешно авторизировались!");//вывод сообщения

                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (ThreadStart)delegate ()
                {
                    this.Visibility = Visibility.Hidden;
                   
                    MainWindow mainForm = new MainWindow(RecMes.UserName, client, stream, new ObservableCollection<string>(UsersOnline));
          
                    mainForm.Show();
                   // this.Close();
                }
            );

                      
                        flag = false;
                    }
                    else if (RecMes.type == "online")
                    { UsersOnline = RecMes.OnlineUser; }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    MessageBox.Show("Подключение прервано!");
                    Disconnect();
                    break;
                }
            }



        }

        static void Disconnect()
        {
            if (stream != null)
                stream.Close();
            if (client != null)
                client.Close();

        }

        private void LoginWindowClosed(object sender, EventArgs e)
        {
            Disconnect();
            Environment.Exit(0);
        }
    }
}
