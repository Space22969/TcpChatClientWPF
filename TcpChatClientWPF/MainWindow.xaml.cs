using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TcpChatClientWPF
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static string yourName;//Имя пользователя
        static TcpClient client;
        static NetworkStream stream;
        static ObservableCollection<string> UsersOnline = new ObservableCollection<string>();//Онлайн пользователи

        public MainWindow(string personName, TcpClient iNclient, NetworkStream iNstream, ObservableCollection<string> inUsersOnline)
        {
            InitializeComponent();


            Binding bind = new Binding();
            bind.Source = UsersOnline;
            Online.ItemsSource = UsersOnline;




            yourName = personName;
            client = iNclient;
            stream = iNstream;
            UsersOnline = inUsersOnline;
            Thread receiveThread = new Thread(new ThreadStart(ReceiveMessage));
            receiveThread.SetApartmentState(ApartmentState.STA);
            receiveThread.Start();
            ChatBody.AppendText(DateTime.Now.ToShortTimeString() + ": Добро пожаловать в чат!" + Environment.NewLine);
        }

        private void Send_Click(object sender, EventArgs e)
        {
            if (SendBody.Text.Length != 0)
            {
                //Отправка сообщения
                string date = DateTime.Now.ToString();
                SendMessage(SendBody.Text, date);
                SendToBody(date, "ВЫ", SendBody.Text, 0);
                SendBody.Clear();
            }
            else MessageBox.Show("Введите сообщение!");
        }

        static void SendMessage(string message, string DateTime)//Метод отправки сообщения серверу
        {
            CoreServer.Message tempMes = new CoreServer.Message("mess", yourName, message, DateTime, null);
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, tempMes);
        }


        public void ReceiveMessage()//Получение сообщений и их обработка
        {
            CoreServer.Message RecMes;
            IFormatter ReqOn = new BinaryFormatter();
            while (true)
            {
                try
                {
                    do
                    {
                        RecMes = (CoreServer.Message)ReqOn.Deserialize(stream);
                    }
                    while (stream.DataAvailable);

                    string userName = RecMes.UserName;

                    if (RecMes.type == "mess")//Сообщение "Сообщение"
                    {
                        UsersOnline = new ObservableCollection<string>(RecMes.OnlineUser);
                        SendToBody(RecMes.DateTime, userName, RecMes.body, 1);
                        OnlineUsersShow();
                    }

                    else if (RecMes.type == "signIn")//Сообщение "Вход в чат"
                    {
                        UsersOnline = new ObservableCollection<string>(RecMes.OnlineUser);
                        SendToBody(RecMes.DateTime, userName, "вошёл в чат!", 2);
                        OnlineUsersShow();
                    }
                    else if (RecMes.type == "exit")//Сообщение "Выход"
                    {
                        UsersOnline = new ObservableCollection<string>(RecMes.OnlineUser);
                        SendToBody(RecMes.DateTime, userName, "покинул чат!", 2);
                        OnlineUsersShow();
                    }

                    else if (RecMes.type == "online")//Сообщение "Онлайн"
                    {
                        if (RecMes.OnlineUser != null)
                        {
                            UsersOnline = new ObservableCollection<string>(RecMes.OnlineUser);
                        }

                        OnlineUsersShow();
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Подключение прервано!"); //соединение было прервано
                    Disconnect();
                    break;
                }
            }
        }

        static void Disconnect()
        {
            if (stream != null)
                stream.Close();//Отключение потока
            if (client != null)
                client.Close();//Отключение клиента
            Environment.Exit(0);//Завершение процесса
        }

        public void OnlineUsersShow()//Вывод списка онлайн пользователей
        {
            if (UsersOnline != null)

                Online.Dispatcher.Invoke(new Action(() =>
                {
                    Online.ItemsSource = UsersOnline;
                    Online.Items.Refresh();
                    for (int i = 0; i < Online.Items.Count; i++)
                    {
                        if (!UsersOnline.Contains(Online.Items[i]))
                        {
                            //List<string> templst = (List<string>)Online.ItemsSource;
                            //templst.Remove(UsersOnline[i]);
                            //Online.ItemsSource = templst;
                        }
                    }

                    for (int i = 0; i < UsersOnline.Count; i++)
                    {
                        if (!Online.Items.Contains(UsersOnline[i]))
                        {

                            //List<string> templst = (List<string>)Online.ItemsSource ?? new List<string>();
                            //templst.Add(UsersOnline[i]);
                            //Online.ItemsSource = templst;
                        }
                            
                    }
                    
                }));
        }

        public void SendToBody(string date, string name, string body, int type)//Вывод сообщений в главное окно чата
        {
            
            ChatBody.Dispatcher.Invoke(new Action(() =>
            {
                ChatBody.AppendText(Environment.NewLine);
                ChatBody.FontWeight = FontWeights.Bold;
                ChatBody.AppendText(date.Remove(0, 11).Remove(5)); ChatBody.AppendText(": ");
                if (type == 1)
                {
                    ChatBody.Foreground = new SolidColorBrush(Colors.Red);
                }
                ChatBody.AppendText(name);
                ChatBody.Foreground = new SolidColorBrush(Colors.Black);
                if (type == 0)
                {
                    ChatBody.AppendText(": ");
                    ChatBody.FontWeight = FontWeights.Regular;
                }
                else ChatBody.AppendText(" ");
                ChatBody.AppendText(body);
                ChatBody.ScrollToEnd();
            }));
        }

        private void MainWindowShown(object sender, EventArgs e)
        {
            OnlineUsersShow();
        }

        private void SendBody_KeyDown(object sender, KeyEventArgs e)//Отправка сообщений по нажатию кнопки.
        {
            
            if (e.Key == Key.Return)
            {
                if (SendBody.Text.Length != 0)
                {
                    string date = DateTime.Now.ToString();
                    SendMessage(SendBody.Text, date);
                    SendToBody(date, "ВЫ", SendBody.Text, 0);
                    SendBody.Clear();
                }
                else MessageBox.Show("Введите сообщение!");
            }
        }



        private void MainWindowClosed(object sender, EventArgs e)
        {
            Disconnect();
            Environment.Exit(0);
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (SendBody.Text.Length != 0)
            {
                string date = DateTime.Now.ToString();
                SendMessage(SendBody.Text, date);
                SendToBody(date, "ВЫ", SendBody.Text, 0);
                SendBody.Clear();
            }
            else MessageBox.Show("Введите сообщение!");
        }

        private void Choice_User(object sender, MouseButtonEventArgs e)
        {
            var item = Online.SelectedItem;
            SendBody.Text += " " + item.ToString() + " ";
        }
    }
}
