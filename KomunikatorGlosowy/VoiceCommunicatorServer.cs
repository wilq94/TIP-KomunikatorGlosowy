using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using OpenTK;
using OpenTK.Audio;

namespace KomunikatorGlosowySerwer
{
    enum Command
    {
        Login,      //logowanie się na serwer
        Logout,     //wylogowyanie się z serwera
        Message,    //wysłanie wiadomości do wszystkich użytkowników korzystających z serwera
        Voice,      //wysłanie głosu do wszystkich użytkowników
        List,       //pobranie listy użytkowników korzystających z serwera
    }

    public partial class VoiceCommunicatorServer : Form
    {
        struct Client
        {
            public EndPoint endpoint;   //gniazdo klienta
            public string ClientName;      //nazwa po jakiej klient zalogował się na serwer
        }

        List<Client> clientList;
        Socket serverSocket;
        byte[] byteData = new byte[8192];

        #region Audio fields from OpenTK

        string selectedRecordDevice = AudioCapture.AvailableDevices[0];
        byte[] buffer = new byte[2048];
        const byte SampleToByte = 2;
        //static System.Windows.Forms.Timer myTimer = new System.Windows.Forms.Timer();

        #endregion

        public VoiceCommunicatorServer()
        {
            clientList = new List <Client>();
            InitializeComponent();
        }

        private void VoiceCommunicatorServer_Load(object sender, EventArgs e)
        {
            try
            {
                CheckForIllegalCrossThreadCalls = false;
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 1000);
                serverSocket.Bind(ipEndPoint);
                IPEndPoint ipeSender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint epSender = (EndPoint)ipeSender;

                //start odbioru danych
                serverSocket.BeginReceiveFrom(byteData, 0, byteData.Length,
                SocketFlags.None, ref epSender, new AsyncCallback(OnReceive), epSender);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "KomunikatorGlosowySerwer",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                IPEndPoint ipeSender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint epSender = (EndPoint)ipeSender;
                serverSocket.EndReceiveFrom(ar, ref epSender);
                byte[] clientNameToByte;
                byte[] messageToByte;
                byte Command = byteData[0];
                //tymczasowo tak
                byte[] tempData = new byte[1];
                byte[] msgToSend = new byte[1];

                switch (Command)
                {
                    //login
                    case 1:
                        Client clientInfo = new Client();
                        clientInfo.endpoint = epSender;
                        tempData = new byte[byteData[1]];
                        for (int i = 0; i < byteData[1]; i++) tempData[i] += byteData[2+i];
                        clientInfo.ClientName = System.Text.Encoding.ASCII.GetString(tempData);;
                        clientList.Add(clientInfo);

                        Invoke((MethodInvoker)delegate()
                        {
                            richTextBoxChatMessages.Text += clientInfo.ClientName + " dolaczyl do pokoju. \r\n";
                        });
                        clientNameToByte = System.Text.Encoding.ASCII.GetBytes(clientInfo.ClientName);
                        msgToSend = new byte[2 + clientNameToByte.Length];
                        msgToSend[0] = Convert.ToByte(1);
                        msgToSend[1] = Convert.ToByte(clientNameToByte.Length);
                        for (int i = 0; i < clientNameToByte.Length; i++) msgToSend[i + 2] = clientNameToByte[i];
                        break;

                    //logout
                    case 2:
                        for(int i = 0; i < clientList.Count; i++)
                        {
                            if (((IPEndPoint)clientList[i].endpoint).Port.ToString() == ((IPEndPoint)epSender).Port.ToString())
                            {
                                Invoke((MethodInvoker)delegate()
                                {
                                    richTextBoxChatMessages.Text += clientList[i].ClientName + " opuscil pokoj.";
                                });
                                clientNameToByte = System.Text.Encoding.ASCII.GetBytes(clientList[i].ClientName);
                                msgToSend = new byte[2 + clientNameToByte.Length];
                                msgToSend[0] = Convert.ToByte(1);
                                msgToSend[1] = Convert.ToByte(clientNameToByte.Length);
                                for (int j = 0; j < clientNameToByte.Length; j++) msgToSend[j + 2] = clientNameToByte[j];
                                break;
                            }
                        }
                        break;

                    //message
                    case 3:
                        byte[] strNameToByte = System.Text.Encoding.ASCII.GetBytes("SERWER");
                        string loginToString = Encoding.ASCII.GetString(byteData, 2, Convert.ToInt32(byteData[1]));
                        string messageToString = Encoding.ASCII.GetString
                            (byteData, 3 + Convert.ToInt32(byteData[1]), Convert.ToInt32(byteData[2 + Convert.ToInt32(byteData[1])]));
                        byte[] fullMessage = Encoding.ASCII.GetBytes(loginToString + ": " + messageToString);
                        msgToSend = new byte[3 + strNameToByte.Length + fullMessage.Length];
                        msgToSend[0] = Convert.ToByte(3);                        
                        msgToSend[1] = Convert.ToByte(strNameToByte.Length);
                        for(int i=0; i < strNameToByte.Length; i++) msgToSend[2+i] = strNameToByte[i];
                        msgToSend[2+strNameToByte.Length] = Convert.ToByte(fullMessage.Length);
                        Buffer.BlockCopy(fullMessage, 0, msgToSend, 3 + strNameToByte.Length, fullMessage.Length);
                        Invoke((MethodInvoker)delegate()
                        {
                            richTextBoxChatMessages.Text += loginToString + ": " + messageToString + "\r\n";
                        });
                        break;
                    
                    //voice
                    case 4:
                        byte[] tempShort = new byte [2];
                        Buffer.BlockCopy(byteData, 2+Convert.ToInt32(byteData[1]), tempShort, 0, 2);
                        short value = BitConverter.ToInt16(tempShort, 0);
                        int dataSize = Convert.ToInt32(value);
                        byte[] sound = new byte[dataSize];
                        Buffer.BlockCopy(byteData, 4 + Convert.ToInt32(byteData[1]), sound, 0, dataSize);
                        using (AudioContext context = new AudioContext())
                        {
                            int buffer = AL.GenBuffer();
                            int source = AL.GenSource();
                            int state;

                            int sample_rate = 22050;
                            //byte[] sound = LoadWave(File.Open("music.wav", FileMode.Open), out channels, out bits_per_sample, out sample_rate);
                            AL.BufferData(buffer, ALFormat.Mono16, sound, sound.Length, sample_rate);

                            AL.Source(source, ALSourcei.Buffer, buffer);
                            if (AL.GetSourceState(source) != ALSourceState.Playing)  AL.SourcePlay(source);

                            System.Console.Write("Playing");

                            // Query the source to find out when it stops playing.
                            do
                            {
                                //Thread.Sleep(250);
                                System.Console.Write(".");
                                AL.GetSource(source, ALGetSourcei.SourceState, out state);
                            }
                            while ((ALSourceState)state == ALSourceState.Playing);

                            System.Console.WriteLine("");

                            AL.SourceStop(source);
                            AL.DeleteSource(source);
                            AL.DeleteBuffer(buffer);
                        }
                        break;
                    
                    //list    
                    case 5:
                        
                        string tempList = "";

                        foreach (Client client in clientList)
                        {
                            tempList += client.ClientName + "*";
                        }
                        messageToByte = System.Text.Encoding.ASCII.GetBytes(tempList);
                        msgToSend = new byte[1 + messageToByte.Length];
                        msgToSend[0] = 5;
                        for(int i=0; i < messageToByte.Length; i++) msgToSend[i+1] = messageToByte[i];

                        serverSocket.BeginSendTo(msgToSend, 0, msgToSend.Length, SocketFlags.None, epSender,
                            new AsyncCallback(OnSend), epSender);
                        break;

                }

                //rozsylanie wiadomosci do wszystkich
                if (Command != 5)
                {
                    foreach (Client clientInfo in clientList)
                    {
                        if (clientInfo.endpoint != epSender)
                        {
                            serverSocket.BeginSendTo(msgToSend, 0, msgToSend.Length, SocketFlags.None, clientInfo.endpoint,
                                new AsyncCallback(OnSend), clientInfo.endpoint);
                        }
                    }
                                                    
                }

                //If the user is logging out then we need not listen from her
                if (Command != 2)
                {
                    //tu sie kopie cos
                    //Start listening to the message send by the user
                    serverSocket.BeginReceiveFrom(byteData, 0, byteData.Length, SocketFlags.None, ref epSender,
                        new AsyncCallback(OnReceive), epSender);
                }
            }
            
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SGSServerUDP1", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }  
        }

        public void OnSend(IAsyncResult ar)
        {
            try
            {
                serverSocket.EndSend(ar);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Serwer - Błąd wysyłania", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

