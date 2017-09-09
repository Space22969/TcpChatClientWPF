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
    }
}
