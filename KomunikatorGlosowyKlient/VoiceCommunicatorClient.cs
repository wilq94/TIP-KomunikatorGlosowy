//komunikacja na podstawie przykładu http://www.codeproject.com/Articles/16935/A-Chat-Application-Using-Asynchronous-UDP-sockets
//obsluga audio na podstawie przykładow http://pastebin.com/SugdrNcV, http://pastebin.com/vUjSehUg
//tu OpenAL to pobrania https://www.openal.org/downloads/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Xml;
using OpenTK;
using OpenTK.Audio;
//using OpenTK.Audio.OpenAL;

namespace KomunikatorGlosowyKlient
{
    enum Command
    {
        Login,      //logowanie się na serwer
        Logout,     //wylogowyanie się z serwera
        Message,    //wysłanie wiadomości do wszystkich użytkowników korzystających z serwera
        Voice,      //wysłanie głosu do wszystkich użytkowników
        List,       //pobranie listy użytkowników korzystających z serwera
    }

    public partial class VoiceCommunicatorClient : Form
    {

        public Socket clientSocket;
        public EndPoint epServer;
        public string strName;
        byte[] byteData = new byte[1024];
        bool connectButtonUsed = false;
        bool nasluchuj = true;

        #region Audio fields from OpenTK

        AudioContext audio_context;
        AudioCapture audio_capture;

        string selectedRecordDevice = AudioCapture.AvailableDevices[0];
        int src;
        byte[] buffer = new byte[2048];
        const byte SampleToByte = 2;
        bool started = false;
        static System.Windows.Forms.Timer myTimer = new System.Windows.Forms.Timer();

        #endregion

        public VoiceCommunicatorClient()
        {
            InitializeComponent();
        }

        //na razie praktycznie nie dziala - laczenie na sztywno, problem z eventami chyba
        private void buttonConnect_Click(object sender, EventArgs e)
        {
            strName = textBoxNickname.Text;
            try
            {
                //Using UDP sockets
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                //IP address of the server machine
                IPAddress ipAddress = IPAddress.Parse(textBoxIPAddress.Text);
                //Server is listening on port 1000
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 1000);

                epServer = (EndPoint)ipEndPoint;

                connectButtonUsed = true;

                byte[] bytesName = Encoding.ASCII.GetBytes(strName);
                byte[] byteData = new byte[2+bytesName.Length];
                byteData[0] = Convert.ToByte(1);
                byteData[1] = Convert.ToByte(bytesName.Length);
                for (int i = 0; i < bytesName.Length; i++) byteData[i+2] = bytesName[i];                
                
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
                string tempClient;

                switch (byteData[0])
                {
                    case 1: //login
                        tempClient = System.Text.Encoding.ASCII.GetString(byteData, 2, byteData[1]);
                        Invoke((MethodInvoker)delegate()
                        {
                            textBox1.Text += "<<<" + strName + " dołączył do pokoju>>>" + "\r\n";
                        });
                        listBoxChatters.Items.Add(tempClient);
                        break;

                    case 2: //logout
                        tempClient = System.Text.Encoding.ASCII.GetString(byteData, 2, byteData[1]);
                        Invoke((MethodInvoker)delegate()
                        {
                            textBox1.Text += "<<<" + strName + " wyszedł z pokoju>>>" + "\r\n";
                        });
                        listBoxChatters.Items.Remove(tempClient);
                        break;

                    case 3:
                        Invoke((MethodInvoker)delegate()
                        {
                        byte[] temp = new byte[Convert.ToInt32(byteData[2 + Convert.ToInt32(byteData[1])])];
                        Buffer.BlockCopy(byteData, 3+Convert.ToInt32(byteData[1]), temp, 0, Convert.ToInt32(byteData[2+Convert.ToInt32(byteData[1])]));
                        textBox1.Text += System.Text.Encoding.ASCII.GetString(temp) + "\r\n";
                        });
                        break;

                    case 4: //voice
                        //odbiór dzwięku od innych użytkowników jest do zaimplementowania
                        break;

                    case 5: //list
                        string tempList = System.Text.Encoding.ASCII.GetString(byteData, 2, byteData[1]);                
                        listBoxChatters.Items.AddRange(tempList.Split('*'));
                        listBoxChatters.Items.RemoveAt(listBoxChatters.Items.Count - 1);
                        break;
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

            if (!connectButtonUsed)
            {
                //Login klienta na sztywno
                strName = "User";
                try
                {
                    //Using UDP sockets
                    clientSocket = new Socket(AddressFamily.InterNetwork,SocketType.Dgram, ProtocolType.Udp);
                    //IP address of the server machine
                    IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
                    //Server is listening on port 1000
                    IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 1000);

                    epServer = (EndPoint)ipEndPoint;

                    byte[] bytesName = Encoding.ASCII.GetBytes(strName);
                    byte[] loginmsgToSend = new byte[2+bytesName.Length];
                    loginmsgToSend[0] = Convert.ToByte(1);
                    loginmsgToSend[1] = Convert.ToByte(bytesName.Length);
                    for (int i = 0; i < bytesName.Length; i++) loginmsgToSend[i+2] = bytesName[i];

                    //Login to the server
                    clientSocket.BeginSendTo(loginmsgToSend, 0, loginmsgToSend.Length,
                        SocketFlags.None, epServer, new AsyncCallback(OnSend), null);

                    //Pobierz aktualną listę
                    byte[] listRequest = new byte[1];
                    listRequest[0] = Convert.ToByte(5);
                    clientSocket.BeginSendTo(listRequest, 0, listRequest.Length,
                        SocketFlags.None, epServer, new AsyncCallback(OnSend), null);

                    if(!started) StartRecording();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "KOMUNIKATOR GŁOSOWY", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            clientSocket.BeginSendTo(byteData, 0, byteData.Length, SocketFlags.None, epServer, new AsyncCallback(OnSend), null);
            byteData = new byte[1024];
            clientSocket.BeginReceiveFrom(byteData, 0, byteData.Length, SocketFlags.None, ref epServer, new AsyncCallback(OnReceive), null);
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
                byte[] msgToSend = new byte[1];
                msgToSend[0] = Convert.ToByte(2);
                clientSocket.SendTo(msgToSend, 0, msgToSend.Length, SocketFlags.None, epServer);
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
                byte[] strNameToByte = System.Text.Encoding.ASCII.GetBytes(strName);
                byte[] textToByte = System.Text.Encoding.ASCII.GetBytes(textBoxSend.Text);
                byteData = new byte[3+strNameToByte.Length+textToByte.Length];
                byteData[0] = Convert.ToByte(3);
                byteData[1] = Convert.ToByte(strNameToByte.Length);
                for(int i=0; i < strNameToByte.Length; i++) byteData[2+i] = strNameToByte[i];
                byteData[2+strNameToByte.Length] = Convert.ToByte(textToByte.Length);
                for(int i=0; i < textToByte.Length; i++) byteData[strNameToByte.Length+3+i] = textToByte[i];

                clientSocket.BeginSendTo(byteData, 0, byteData.Length, SocketFlags.None, epServer, new AsyncCallback(OnSend), null);
                textBoxSend.Text = null;
               }
            catch (Exception)
            {
                MessageBox.Show("Nie można wysłać wiadomości do serwera.", "KOMUNIKATOR GŁOSOWY - " + strName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //potem
        private void listBoxChatters_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedRecordDevice = AudioCapture.AvailableDevices[0];
            this.StartRecording();
        }

        void StartRecording()
        {
            started = true;
            try
            {
                audio_context = new AudioContext();
            }
            catch (AudioException ae)
            {
                MessageBox.Show("Fatal: Cannot continue without a playback device.\nException caught when opening playback device.\n" + ae.Message);
                Application.Exit();
            }

            AL.Listener(ALListenerf.Gain, (float)4);
            src = AL.GenSource();

            int sampling_rate = (int)22050;
            double buffer_length_ms = (double)50;
            int buffer_length_samples = (int)((double)buffer_length_ms * sampling_rate * 0.001 / BlittableValueType.StrideOf(buffer));

            try
            {
                audio_capture = new AudioCapture((string)selectedRecordDevice, sampling_rate,
                    OpenTK.Audio.OpenAL.ALFormat.Mono16, buffer_length_samples);
            }
            catch (AudioDeviceException ade)
            {
                MessageBox.Show("Exception caught when opening recording device.\n" + ade.Message);
                audio_capture = null;
            }

            if (audio_capture == null)
                return;

            audio_capture.Start();
            myTimer.Tick += new EventHandler(UpdateSamples);
            myTimer.Start();
            myTimer.Interval = (int)(buffer_length_ms / 2 + 0.5);
        }

        void StopRecording()
        {
            if (audio_capture != null)
            {
                audio_capture.Stop();
                audio_capture.Dispose();
                audio_capture = null;
            }

            if (audio_context != null)
            {
                int r;
                AL.GetSource(src, ALGetSourcei.BuffersQueued, out r);
                ClearBuffers(r);

                AL.DeleteSource(src);

                audio_context.Dispose();
                audio_context = null;
            }
        }

        void UpdateSamples(Object myObject, EventArgs myEventArgs)
        {
            if (audio_capture == null)
                return;

            int available_samples = audio_capture.AvailableSamples;

            if (available_samples * SampleToByte > buffer.Length * BlittableValueType.StrideOf(buffer))
            {
                buffer = new byte[MathHelper.NextPowerOfTwo(
                    (int)(available_samples * SampleToByte / (double)BlittableValueType.StrideOf(buffer) + 0.5))];
            }

            if (available_samples > 0)
            {
                buffer = new byte[available_samples*2];
                audio_capture.ReadSamples(buffer, available_samples);
            
                int buf = AL.GenBuffer();
                AL.BufferData(buf, ALFormat.Mono16, buffer, buffer.Length, audio_capture.SampleFrequency);
                AL.SourceQueueBuffer(src, buf);
                System.Console.Write(buf + " " + audio_capture.SampleFrequency + "\r\n");

                byte[] strNameToByte = System.Text.Encoding.ASCII.GetBytes(strName);
                byte[] byteData = new byte[4 + buffer.Length + strNameToByte.Length];
                byteData[0] = 4;
                byteData[1] = Convert.ToByte(strName.Length);
                Buffer.BlockCopy(strNameToByte, 0, byteData, 2, strNameToByte.Length);
                byte[] byteBufferLength = BitConverter.GetBytes((short)buffer.Length);
                byteData[2 + strNameToByte.Length] = byteBufferLength[0];
                byteData[3 + strNameToByte.Length] = byteBufferLength[1];
                Buffer.BlockCopy(buffer, 0, byteData, 4 + strNameToByte.Length, buffer.Length);

                if (AL.GetSourceState(src) != ALSourceState.Playing && nasluchuj == true)
                {
                    AL.SourcePlay(src);
                    System.Console.Write(buf + " " + audio_capture.SampleFrequency + "\r\n");                
                }
            }

            ClearBuffers(0);
        }
        
        void ClearBuffers(int input)
        {
            if (audio_context == null || audio_context == null)
                return;

            int[] freedbuffers;
            if (input == 0)
            {
                int BuffersProcessed;
                AL.GetSource(src, ALGetSourcei.BuffersProcessed, out BuffersProcessed);
                if (BuffersProcessed == 0)
                    return;
                freedbuffers = AL.SourceUnqueueBuffers(src, BuffersProcessed);
            }
            else
            {
                freedbuffers = AL.SourceUnqueueBuffers(src, input);
            }
            AL.DeleteBuffers(freedbuffers);
        }

        private void dźwiękToolStripMenuItem_Click(object sender, EventArgs e)
        {
            nasluchuj = false;
            Form2 frm = new Form2();
            frm.Show();
        }
    }
}