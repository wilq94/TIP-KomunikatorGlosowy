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
        Null        //brak komendy
    }

    public partial class VoiceCommunicatorServer : Form
    {
        struct Client
        {
            public EndPoint endpoint;   //gniazdo klienta
            public string ClientName;      //nazwa po jakiej klient zalogował się na serwer
        }

        ArrayList clientList;
        Socket serverSocket;
        byte[] byteData = new byte[1024];

        #region Audio fields from OpenTK

        AudioContext audio_context;
        AudioCapture audio_capture;

        string selectedRecordDevice = AudioCapture.AvailableDevices[0];
        int src;
        short[] buffer = new short[512];
        const byte SampleToByte = 2;
        bool started = false;
        static System.Windows.Forms.Timer myTimer = new System.Windows.Forms.Timer();

        #endregion

        public VoiceCommunicatorServer()
        {
            clientList = new ArrayList();
            InitializeComponent();
          
        }

        private void VoiceCommunicatorServer_Load(object sender, EventArgs e)
        {
            try
            {
                CheckForIllegalCrossThreadCalls = false;

                serverSocket = new Socket(AddressFamily.InterNetwork,
                    SocketType.Dgram, ProtocolType.Udp);

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
            //try
            //{
                IPEndPoint ipeSender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint epSender = (EndPoint)ipeSender;

                serverSocket.EndReceiveFrom(ar, ref epSender);

                //Transform the array of bytes received from the user into an
                //intelligent form of object Data
                Data msgReceived = new Data(byteData);

                //We will send this object in response the users request
                Data msgToSend = new Data();

                byte[] message;

                //If the message is to login, logout, or simple text message
                //then when send to others the type of the message remains the same
                msgToSend.cmdCommand = msgReceived.cmdCommand;
                msgToSend.strName = msgReceived.strName;

                switch (msgReceived.cmdCommand)
                {
                    case Command.Login:

                        //When a user logs in to the server then we add her to our
                        //list of clients

                        Client clientInfo = new Client();
                        clientInfo.endpoint = epSender;
                        clientInfo.ClientName = msgReceived.strName;

                        clientList.Add(clientInfo);

                        //Set the text of the message that we will broadcast to all users
                        msgToSend.strMessage = "<<<" + msgReceived.strName + " dolaczyl do pokoju.>>>";
                        break;

                    case Command.Logout:

                        //When a user wants to log out of the server then we search for her 
                        //in the list of clients and close the corresponding connection

                        int nIndex = 0;
                        foreach (Client client in clientList)
                        {

                            string port1 = ((IPEndPoint)client.endpoint).Port.ToString();
                            string port2 = ((IPEndPoint)epSender).Port.ToString();
                            if (port1 == port2)
                            {
                                clientList.RemoveAt(nIndex);
                                break;
                            }
                            ++nIndex;
                        }
                           msgToSend.strMessage = "<<<" + msgReceived.strName + " opuscil pokoj.>>>";
                        break;

                        //cos poknocilem - najlepiej od nowa zrobic tak, zeby oddzielic "Data" od wysylania dzwieku
                        /*case Command.Voice:
                        byte[] array = Encoding.ASCII.GetBytes(msgReceived.strMessage);
                        short[] result = new short[array.Length / sizeof(short)];
                        Buffer.BlockCopy(array, 0, result, 0, result.Length);
                        //int available_samples = audio_capture.AvailableSamples;
                        //audio_capture.ReadSamples(result, available_samples);

                        int buf = AL.GenBuffer();
                        AL.BufferData(buf, ALFormat.Mono16, result, (int)(250 * BlittableValueType.StrideOf(result)), audio_capture.SampleFrequency);
                        AL.SourceQueueBuffer(src, buf);
                        if (AL.GetSourceState(src) != ALSourceState.Playing) AL.SourcePlay(src);

                        break;
                        */

                    case Command.Message:

                        //Set the text of the message that we will broadcast to all users
                        msgToSend.strMessage = msgReceived.strName + ": " + msgReceived.strMessage;
                        break;

                    case Command.List:

                        //Send the names of all users in the chat room to the new user
                        msgToSend.cmdCommand = Command.List;
                        msgToSend.strName = null;
                        msgToSend.strMessage = null;

                        //Collect the names of the user in the chat room
                        foreach (Client client in clientList)
                        {
                            //To keep things simple we use asterisk as the marker to separate the user names
                            msgToSend.strMessage += client.ClientName + "*";
                        }

                        message = msgToSend.ToByte();

                        //Send the name of the users in the chat room
                        serverSocket.BeginSendTo(message, 0, message.Length, SocketFlags.None, epSender,
                                new AsyncCallback(OnSend), epSender);
                        break;
                }

                if (msgToSend.cmdCommand != Command.List)   //List messages are not broadcasted
                {
                    message = msgToSend.ToByte();

                    foreach (Client clientInfo in clientList)
                    {
                        if (clientInfo.endpoint != epSender ||
                            msgToSend.cmdCommand != Command.Login)
                        {
                            //Send the message to all users
                            serverSocket.BeginSendTo(message, 0, message.Length, SocketFlags.None, clientInfo.endpoint,
                                new AsyncCallback(OnSend), clientInfo.endpoint);
                        }
                    }

                    Invoke((MethodInvoker)delegate()
                    {
                        richTextBoxChatMessages.Text += msgToSend.strMessage + "\r\n";
                    });
                    
                }

                //If the user is logging out then we need not listen from her
                if (msgReceived.cmdCommand != Command.Logout)
                {
                    //Start listening to the message send by the user
                    serverSocket.BeginReceiveFrom(byteData, 0, byteData.Length, SocketFlags.None, ref epSender,
                        new AsyncCallback(OnReceive), epSender);
                }
            //}
            
            //catch (Exception ex)
            //{
             //   MessageBox.Show(ex.Message, "SGSServerUDP1", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}
            
        }

        public void OnSend(IAsyncResult ar)
        {
            try
            {
                serverSocket.EndSend(ar);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SGSServerUDP2", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    class Data
    {
        //Default constructor
        public Data()
        {
            this.cmdCommand = Command.Null;
            this.strMessage = null;
            this.strName = null;
        }

        //Converts the bytes into an object of type Data
        public Data(byte[] data)
        {
            //The first four bytes are for the Command
            this.cmdCommand = (Command)BitConverter.ToInt32(data, 0);

            //The next four store the length of the name
            int nameLen = BitConverter.ToInt32(data, 4);

            //The next four store the length of the message
            int msgLen = BitConverter.ToInt32(data, 8);

            //This check makes sure that strName has been passed in the array of bytes
            if (nameLen > 0)
                this.strName = Encoding.UTF8.GetString(data, 12, nameLen);
            else
                this.strName = null;

            //This checks for a null message field
            if (msgLen > 0)
                this.strMessage = Encoding.UTF8.GetString(data, 12 + nameLen, msgLen);
            else
                this.strMessage = null;
        }

        //Converts the Data structure into an array of bytes
        public byte[] ToByte()
        {
            List<byte> result = new List<byte>();

            //First four are for the Command
            result.AddRange(BitConverter.GetBytes((int)cmdCommand));

            //Add the length of the name
            if (strName != null)
                result.AddRange(BitConverter.GetBytes(strName.Length));
            else
                result.AddRange(BitConverter.GetBytes(0));

            //Length of the message
            if (strMessage != null)
                result.AddRange(BitConverter.GetBytes(strMessage.Length));
            else
                result.AddRange(BitConverter.GetBytes(0));

            //Add the name
            if (strName != null)
                result.AddRange(Encoding.UTF8.GetBytes(strName));

            //And, lastly we add the message text to our array of bytes
            if (strMessage != null)
                result.AddRange(Encoding.UTF8.GetBytes(strMessage));

            return result.ToArray();
        }

        public string strName;      //Name by which the client logs into the room
        public string strMessage;   //Message text
        public Command cmdCommand;  //Command type (login, logout, send message, etcetera)
    }

}

