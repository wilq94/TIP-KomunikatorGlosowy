using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using System.Threading;

namespace TIPUDPServer
{
    class Program
    {
        struct Client
        {
            public IPEndPoint endpoint;   
            public string ClientName;
        }

        static void Main(string[] args)
        {            
            bool sendList = false;
            String serverName = "";
            List<Client> clientList = new List<Client>();
            Console.WriteLine("Podaj nazwe serwera");
            serverName = Console.ReadLine();
            Console.WriteLine("Serwer startuje . . .");
            UdpClient server = new UdpClient(15000);
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            Console.WriteLine("Serwer wystartował . . .");
            String welcome = "Witamy na serwerze " + serverName;
            byte[] data = new byte[1024];
            
            while (true)
            {
                data = server.Receive(ref sender);
                
                //Jeśli serwer odbiera wiadomość z początkiem "1" user się połączył do serwera
                //Dodaje go do listy klientów oraz wysyła wiadomość powitalną
                if (data[0] == Convert.ToByte(1))
                {                    
                    Client serverClient = new Client();
                    serverClient.endpoint = sender;
                    byte[] user = new byte[data[1]];
                    for(int i = 0; i < data[1]; i++)
                    {
                        user[i] = data[i + 2];
                    }
                    String userNickname = Encoding.ASCII.GetString(user);
                    serverClient.ClientName = userNickname;
                    clientList.Add(serverClient);
                    Console.WriteLine("Połączył się " + userNickname + 
                        " z adresu " + sender.Address);

                    data = Encoding.ASCII.GetBytes(welcome);
                    server.SendAsync(data, data.Length, sender);

                    sendList = true;                     
                }
                //Jeśli serwer odbiera wiadomość z początkiem "2" jest to wiadomość z czatu
                //Przekazuje ją dalej wszystkim klientów na serwerze
                else if (data[0] == Convert.ToByte(2))
                {
                    byte[] user = new byte[data[1]];
                    byte[] message = new byte[data[2]];
                    for (int i = 0; i < data[1]; i++)
                    {
                        user[i] = data[i + 3];
                    }
                    for (int i = 0; i < data[2]; i++)
                    {
                        message[i] = data[i + 3 + data[1]];
                    }
                    String userNickname = Encoding.ASCII.GetString(user);
                    String userMessage = Encoding.ASCII.GetString(message);
                    Console.WriteLine(userNickname + ": " + userMessage);
                    foreach(Client svrClient in clientList)
                    {
                        server.SendAsync(data, data.Length, svrClient.endpoint);
                    }
                //Jeśli serwer odbiera wiadomość z początkiem "3" user się rozłączył z serwera
                //Usuwa go z listy klientów
                }
                else if (data[0] == Convert.ToByte(3))
                {
                    byte[] user = new byte[data[1]];
                    for (int i = 0; i < data[1]; i++)
                    {
                        user[i] = data[i + 2];
                    }
                    String userNickname = Encoding.ASCII.GetString(user);

                    clientList.RemoveAll(Client => Client.ClientName == userNickname);
                    sendList = true;

                    Console.WriteLine("Użytkownik " + userNickname +
                        " wyszedł z serwera.");
                }
                //Jeśli serwer odbiera wiadomość z początkiem "9" jest to wiadomość audio
                //Rozsyła ją dalej do wszystkich klientów z wyjątkiem klienta od którego przyszła ta wiadomość
                else if (data[0] == Convert.ToByte(9))
                {
                    foreach(Client svrClient in clientList)
                    {
                        if(sender.ToString() != svrClient.endpoint.ToString())
                        {
                            server.SendAsync(data, data.Length, svrClient.endpoint);
                        }
                    }
                }
                //Jeśli ktoś się łączy lub rozłącza z serwera zostaje wywołana ta "funkcja"
                //Pobiera liste aktualnych klientów i rozsyła do wszystkich klientów na serwerze
                if (sendList == true)
                {
                    String clientNames = "";
                    foreach (Client svrClient in clientList)
                    {
                        clientNames += svrClient.ClientName + "*";
                    }

                    byte[] clientNamesBytes = new byte[clientNames.Length + 1];
                    clientNamesBytes[0] = Convert.ToByte(5);
                    byte[] tempClientNameBytes = new byte[clientNames.Length];
                    tempClientNameBytes = Encoding.ASCII.GetBytes(clientNames);
                    for (int i = 0; i < tempClientNameBytes.Length; i++)
                    {
                        clientNamesBytes[i + 1] = tempClientNameBytes[i];
                    }

                    foreach (Client svrClient in clientList)
                    {
                        server.Send(clientNamesBytes, clientNamesBytes.Length, svrClient.endpoint);
                    }
                    sendList = false;
                }
                
            }
            Console.ReadLine();
            Console.WriteLine("Serwer zatrzymany . . .");
    }

        
    }
}
