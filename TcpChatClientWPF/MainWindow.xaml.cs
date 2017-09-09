using System;
using System.Collections.Generic;
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
        static List<string> UsersOnline = new List<string>();//Онлайн пользователи

        public MainWindow(string personName, TcpClient iNclient, NetworkStream iNstream, List<string> inUsersOnline)
        {
            InitializeComponent();
            yourName = personName;
            client = iNclient;
            stream = iNstream;
            UsersOnline = inUsersOnline;
            Thread receiveThread = new Thread(new ThreadStart(ReceiveMessage));
            receiveThread.Start();
            ChatBody.AppendText(DateTime.Now.ToShortTimeString() + ": Добро пожаловать в чат!" + Environment.NewLine);
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
                        UsersOnline = RecMes.OnlineUser;
                        SendToBody(RecMes.DateTime, userName, RecMes.body, 1);
                        OnlineUsersShow();
                    }

                    else if (RecMes.type == "signIn")//Сообщение "Вход в чат"
                    {
                        UsersOnline = RecMes.OnlineUser;
                        SendToBody(RecMes.DateTime, userName, "вошёл в чат!", 2);
                        OnlineUsersShow();
                    }
                    else if (RecMes.type == "exit")//Сообщение "Выход"
                    {
                        UsersOnline = RecMes.OnlineUser;
                        SendToBody(RecMes.DateTime, userName, "покинул чат!", 2);
                        OnlineUsersShow();
                    }

                    else if (RecMes.type == "online")//Сообщение "Онлайн"
                    {
                        if (RecMes.OnlineUser != null)
                        {
                            UsersOnline = RecMes.OnlineUser;
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
                    for (int i = 0; i < Online.Items.Count; i++)
                    {
                        if (!UsersOnline.Contains(Online.Items[i]))
                            Online.Items.Remove(Online.Items[i]);
                    }

                    for (int i = 0; i < UsersOnline.Count; i++)
                    {
                        if (!Online.Items.Contains(UsersOnline[i]))
                            Online.Items.Add(UsersOnline[i]);
                    }

                }));
        }

        public void SendToBody(string date, string name, string body, int type)//Вывод сообщений в главное окно чата
        {
            ChatBody.Dispatcher.Invoke(new Action(() =>
            { 
                ChatBody.FontWeight = FontWeights.Bold;
                ChatBody.AppendText(date.Remove(0, 11).Remove(5)); ChatBody.AppendText(": ");
                if (type == 1)
                {
                    ChatBody.Foreground = new SolidColorBrush(Colors.Red);
                }
                ChatBody.AppendText(name);
                ChatBody.Foreground = new SolidColorBrush(Colors.Black);
                if (type == 1)
                {
                    ChatBody.AppendText(": ");
                    ChatBody.FontWeight = FontWeights.Regular;
                }
                else ChatBody.AppendText(" ");
                ChatBody.AppendText(body + Environment.NewLine);
                ChatBody.ScrollToEnd();
            }));
        }

    }
}
