using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
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
    /// Логика взаимодействия для RegisterWindow.xaml
    /// </summary>
    public partial class RegisterWindow : Window
    {
        static string userName;//Имя пользователя
        static TcpClient client;//Объект tcp клиента
        static NetworkStream stream;//Объект сетевого потока
        static Thread receiveThread;//Поток обработки сообщений
        static string host;//адрес хоста
        static int port;//Порт хоста
        static CoreServer.Message tempMes;//Объект сообщения Message
        static IFormatter formatter = new BinaryFormatter();

        public RegisterWindow(TcpClient iNclient, NetworkStream iNstream, string inHost, int inPort)//Конструктор объекта формы
        {
            InitializeComponent();
            client = iNclient;
            stream = iNstream;
            host = inHost;
            port = inPort;
        }

        private void EndRegBut_Click(object sender, RoutedEventArgs e)
        {
            if ((Login.Text.Length != 0) && (Password.Text.Length != 0))//Проверка полей
            {
                userName = Login.Text;

                if (userName.Length < 3)
                    MessageBox.Show("Слишком короткое имя!");
                else
                {
                    string pass = Password.Text;

                    try
                    {
                        client = new TcpClient();
                        client.Connect(host, port); //Подключение к хосту

                        stream = client.GetStream(); //Получение потока сети

                        tempMes = new CoreServer.Message("connection", userName, "", "", null);//Отправка сообщения на запрос подключения
                        formatter.Serialize(stream, tempMes);

                        tempMes = new CoreServer.Message("regg", userName, pass, DateTime.Now.ToString(), null);//Отправка сообщения на запрос регистрации
                        formatter.Serialize(stream, tempMes);


                        //Запускаем новый поток для получения сообщений
                        receiveThread = new Thread(new ThreadStart(ReceiveMessage));
                        
                        receiveThread.Start(); //Старт потока
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }

        public void ReceiveMessage()//Получение и обработка входящих сообщений
        {
            bool flag = true;
            while (flag)
            {
                try
                {
                    CoreServer.Message RecMes;

                    IFormatter formatter = new BinaryFormatter();
                    RecMes = (CoreServer.Message)formatter.Deserialize(stream);//Десериализация сообщения

                    if (RecMes.type == "regansvNo")//Отрицательных ответ на запрос регистрации
                    {
                        MessageBox.Show(RecMes.body);//вывод 
                        Disconnect();
                        flag = false;
                    }
                    else if (RecMes.type == "regansvYes")//Положительных ответ на запрос регистрации
                    {
                        MessageBox.Show(RecMes.body);//Вывод сообщения

                        //Закрытие формы и отключение от сервера
                        //this.Invoke(new Action(() =>
                        // {
                        this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
          (ThreadStart)delegate ()
          {
              this.Visibility = Visibility.Hidden;
              this.Close();
              // this.Close();
          });

                     
                       // }));
                        Disconnect();
                        flag = false;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    MessageBox.Show("Подключение прервано!"); //соединение было разрвано
                    break;
                }
            }
        }

        static void Disconnect()
        {
            if (stream != null)
                stream.Close();//отключение потока
            if (client != null)
                client.Close();//отключение клиента 
        }

        private void ExitRegBut_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
