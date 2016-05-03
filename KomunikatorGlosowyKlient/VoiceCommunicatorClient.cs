//na podstawie przykładu http://www.codeproject.com/Articles/16935/A-Chat-Application-Using-Asynchronous-UDP-sockets

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace KomunikatorGlosowyKlient
{
    enum Command
    {
        Login,      //logowanie się na serwer
        Logout,     //wylogowyanie się z serwera
        Message,    //wysłanie wiadomości do wszystkich użytkowników korzystających z serwera
        List,       //pobranie listy użytkowników korzystających z serwera
        Null        //brak komendy
    }

    public partial class VoiceCommunicatorClient : Form
    {

        public Socket clientSocket;
        public EndPoint epServer;
        public string strName;
        byte[] byteData = new byte[1024];
        bool connectButtonUsed = false;

        public VoiceCommunicatorClient()
        {
            InitializeComponent();
        }

        //na razie praktycznie nie dziala - laczenie na sztywno, problem z eventami
        private void buttonConnect_Click(object sender, EventArgs e)
        {
            strName = textBoxNickname.Text;
            try
            {
                //Using UDP sockets
                clientSocket = new Socket(AddressFamily.InterNetwork,
                    SocketType.Dgram, ProtocolType.Udp);

                //IP address of the server machine
                IPAddress ipAddress = IPAddress.Parse(textBoxIPAddress.Text);
                //Server is listening on port 1000
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 1000);

                epServer = (EndPoint)ipEndPoint;

                connectButtonUsed = true;

                Data msgToSend = new Data();
                msgToSend.cmdCommand = Command.Login;
                msgToSend.strMessage = null;
                msgToSend.strName = strName;

                byte[] byteData = msgToSend.ToByte();

                //Login to the server
                clientSocket.BeginSendTo(byteData, 0, byteData.Length,
                    SocketFlags.None, epServer, new AsyncCallback(OnSend), null);
                this.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "KOMUNIKATOR GŁOSOWY",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void textBoxNickname_TextChanged(object sender, EventArgs e)
        {
            if (textBoxNickname.Text.Length > 0 && textBoxIPAddress.Text.Length > 0)
                buttonConnect.Enabled = true;
            else
                buttonConnect.Enabled = false;
        }

        private void textBoxIPAddress_TextChanged(object sender, EventArgs e)
        {
            if (textBoxNickname.Text.Length > 0 && textBoxIPAddress.Text.Length > 0)
                buttonConnect.Enabled = true;
            else
                buttonConnect.Enabled = false;
        }

    
        private void OnSend(IAsyncResult ar)
        {
            try
            {
                clientSocket.EndSend(ar);
            }
            catch (ObjectDisposedException)
            { }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "KOMUNIKATOR GŁOSOWY - " + strName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnReceive(IAsyncResult ar)
        {            
            try
            {                
                clientSocket.EndReceive(ar);

                Data msgReceived = new Data(byteData);

                switch (msgReceived.cmdCommand)
                {
                    case Command.Login:
                        listBoxChatters.Items.Add(msgReceived.strName);
                        break;

                    case Command.Logout:
                        listBoxChatters.Items.Remove(msgReceived.strName);
                        break;

                    case Command.Message:
                        break;

                    case Command.List:
                        listBoxChatters.Items.AddRange(msgReceived.strMessage.Split('*'));
                        listBoxChatters.Items.RemoveAt(listBoxChatters.Items.Count - 1);
                        Invoke((MethodInvoker)delegate()
                        {
                            textBox1.Text += "<<<" + strName + " dołączył do pokoju>>>\r\n";
                        });
                        break;
                }

                if (msgReceived.strMessage != null && msgReceived.cmdCommand != Command.List)
                {
                    Invoke((MethodInvoker)delegate()
                    {
                        textBox1.Text += msgReceived.strMessage + "\r\n";
                    });

                }

                byteData = new byte[1024];                

                //rozpoczecie nasluchiwania
                clientSocket.BeginReceiveFrom(byteData, 0, byteData.Length, SocketFlags.None, ref epServer,
                                           new AsyncCallback(OnReceive), null);
            }
            catch (ObjectDisposedException)
            { }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "KOMUNIKATOR GŁOSOWY - " + strName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void VoiceCommunicatorClient_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;

            //this.Text = "KOMUNIKATOR GŁOSOWY - " + strName;

            //The user has logged into the system so we now request the server to send
            //the names of all users who are in the chat room

            if (!connectButtonUsed)
            {
                strName = "AdamMalysz";
                try
                {

                    //Using UDP sockets
                    clientSocket = new Socket(AddressFamily.InterNetwork,
                        SocketType.Dgram, ProtocolType.Udp);

                    //IP address of the server machine
                    IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
                    //Server is listening on port 1000
                    IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 1000);

                    epServer = (EndPoint)ipEndPoint;

                    Data loginmsgToSend = new Data();
                    loginmsgToSend.cmdCommand = Command.Login;
                    loginmsgToSend.strMessage = null;
                    loginmsgToSend.strName = strName;

                    byte[] loginbyteData = loginmsgToSend.ToByte();

                    //Login to the server
                    clientSocket.BeginSendTo(loginbyteData, 0, loginbyteData.Length,
                        SocketFlags.None, epServer, new AsyncCallback(OnSend), null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "KOMUNIKATOR GŁOSOWY",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }

            Data msgToSend = new Data();
            msgToSend.cmdCommand = Command.List;
            msgToSend.strName = strName;
            msgToSend.strMessage = null;

            byteData = msgToSend.ToByte();

                clientSocket.BeginSendTo(byteData, 0, byteData.Length, SocketFlags.None, epServer,
                    new AsyncCallback(OnSend), null);

                byteData = new byte[1024];

                //Start listening to the data asynchronously
                clientSocket.BeginReceiveFrom(byteData,
                                           0, byteData.Length,
                                           SocketFlags.None,
                                           ref epServer,
                                           new AsyncCallback(OnReceive),
                                           null);
        }

        private void textBoxSend_TextChanged(object sender, EventArgs e)
        {
            if (textBoxSend.Text.Length == 0)
                buttonSend.Enabled = false;
            else
                buttonSend.Enabled = true;
        }

        private void KomunikatorGlosowyKlient_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("Czy na pewno chcesz opuścić aplikację?", "KOMUNIKATOR GŁOSOWY - " + strName,
                MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No)
            {
                e.Cancel = true;
                return;
            }

            try
            {
                //Send a message to logout of the server
                Data msgToSend = new Data ();
                msgToSend.cmdCommand = Command.Logout;
                msgToSend.strName = strName;
                msgToSend.strMessage = null;

                byte[] b = msgToSend.ToByte ();
                clientSocket.SendTo(b, 0, b.Length, SocketFlags.None, epServer);
                clientSocket.Close();
            }
            catch (ObjectDisposedException)
            { }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "KOMUNIKATOR GŁOSOWY - " + strName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void textBoxSend_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                buttonSend_Click_1(sender, null);
            }
        }

        private void buttonSend_Click_1(object sender, EventArgs e)
        {
            try
            {
                //Fill the info for the message to be send
                Data msgToSend = new Data();

                msgToSend.strName = strName;
                msgToSend.strMessage = textBoxSend.Text;
                msgToSend.cmdCommand = Command.Message;

                byte[] byteData = msgToSend.ToByte();

                //Send it to the server
                clientSocket.BeginSendTo(byteData, 0, byteData.Length, SocketFlags.None, epServer, new AsyncCallback(OnSend), null);
                textBoxSend.Text = null;
            }
            catch (Exception)
            {
                MessageBox.Show("Nie można wysłać wiadomości do serwera.", "KOMUNIKATOR GŁOSOWY - " + strName, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
