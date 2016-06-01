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
            public string Channel;
        }

        static void Main(string[] args)
        {            
            bool sendList = false;
            String serverName = "";
            int serverPort = 15000;
            List<Client> clientList = new List<Client>();
            List<String> channelList = new List<String>();
            channelList.Add("Kanal Powitalny");
            do {
                Console.WriteLine("Podaj nazwe serwera");
                serverName = Console.ReadLine();
                if(serverName == "")
                    Console.WriteLine("Nazwa serwera nie może być pusta!");
            } while (serverName == "");
            do {
                Console.WriteLine("Podaj port serwera (Domyslnie 15000)");
                serverPort = Convert.ToInt32(Console.ReadLine());
            } while (serverPort < 1);
            Console.WriteLine("Serwer \"" + serverName + "\" startuje . . .");
            UdpClient server = new UdpClient(serverPort);
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            Console.WriteLine("Serwer \"" + serverName + "\" wystartował . . .");
            byte[] data = new byte[1024];

            new Thread(() =>
            {
                Console.WriteLine("Wpisz /help, aby uzyskać listę komend");
                while(true)
                {
                    String command = Console.ReadLine();
                    String[] commandWords = command.Split('<','>');
                    if(command == "/help")
                    {
                        Console.WriteLine("/change<nazwa> - zmienia nazwę serwera");
                        Console.WriteLine("/changeDefault<nazwa> - zmienia nazwę kanału domyślnego");
                        Console.WriteLine("/create<nazwa> - tworzy nowy kanał");
                        Console.WriteLine("/remove<nazwa> - usuwa podany kanał");
                        Console.WriteLine("/kick<nazwa><powód> - wyrzuca z serwera podanego użytkownika");
                        Console.WriteLine("/ban<nazwa><powód> - banuje pernamentnie podanego uzytkownika z serwera");
                        Console.WriteLine("/users - pokazuje aktualnie podłączonych do serwera użytkowników");
                        Console.WriteLine("/channels - pokazuje liste utworzonych kanałów");
                        Console.WriteLine("/close - wyłącza serwer");                     
                    } else if(commandWords[0] == "/change") {
                        serverName = commandWords[1];
                        Console.WriteLine("Nazwa serwera zmieniona na " + commandWords[1]);
                    } else if(commandWords[0] == "/changeDefault") {
                        channelList[0] = commandWords[1];
                        Console.WriteLine("Zmieniono nazwę kanału domyślnego na " + commandWords[1]);
                    } else if(command == "/users") {
                        if (clientList.Count == 0)
                        {
                            Console.WriteLine("Serwer jest pusty");
                        } else {
                            int i = 1;
                            foreach (Client svrClient in clientList)
                            {
                                Console.WriteLine(i + ". " + svrClient.ClientName + " - " + svrClient.endpoint);
                                i++;
                            }
                        }
                    } else if(command == "/channels") {
                        foreach (String channel in channelList)
                        {
                            Console.WriteLine(channel.ToString());
                        }
                    } else if(commandWords[0] == "/create") {
                        channelList.Add(commandWords[1]);
                    } else if(commandWords[0] == "/remove") {
                        if (commandWords[1] == channelList[0]) {
                            Console.WriteLine("Nie można usunąć domyślnego kanału");
                        } else {
                            channelList.Remove(commandWords[1]);
                        }
                    } else if(commandWords[0] == "/kick") {
                        foreach(Client svrClient in clientList)
                        {
                            if(svrClient.ClientName == commandWords[1]) { }
                                //rozłącz kliena
                        }
                    } else if(commandWords[0] == "/ban") {
                        foreach (Client svrClient in clientList)
                        {
                            if (svrClient.ClientName == commandWords[1]) { }
                            //rozłącz kliena
                        }
                    } else if(command == "/close")
                    {
                        server.Close();
                        Console.WriteLine("Serwer wyłączony");
                        Environment.Exit(0);
                    } else {
                        Console.WriteLine("Niepoprawna komenda");
                    }
                }
            }).Start();
            
            while (true)
            {
                data = server.Receive(ref sender);
                
                //Jeśli serwer odbiera wiadomość z początkiem "1" user się połączył do serwera
                //Dodaje go do listy klientów oraz wysyła wiadomość powitalną
                if (data[0] == Convert.ToByte(1))
                {
                    String welcome = "Witamy na serwerze " + serverName;
                    Client serverClient = new Client();
                    serverClient.endpoint = sender;
                    byte[] user = new byte[data[1]];
                    for(int i = 0; i < data[1]; i++)
                    {
                        user[i] = data[i + 2];
                    }
                    String userNickname = Encoding.ASCII.GetString(user);
                    serverClient.ClientName = userNickname;
                    serverClient.Channel = channelList[0];
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
    }

        
    }
}
