using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using NAudio.Wave;

namespace TIPy
{
    public partial class Form1 : Form
    {
        UdpClient client;
        IPEndPoint iep;
        IPEndPoint serverResponse;
        WaveIn waveIn;
        WaveIn sourceStream;
        WaveOut waveOut;
        BufferedWaveProvider waveProvider;
        Thread thread, thread2;
        int deviceID = 0;
        byte[] serverData;
        bool isConnected = false;
        bool isAllMuted = false, isMeMuted = false;

        private static int _bitRate = 44100;
        public static int bitRate
        {
            get
            {
                return _bitRate;
            }
            set
            {
                _bitRate = value;
            }
        }

        private static int _bitDepth = 16;
        public static int bitDepth
        {
            get
            {
                return _bitDepth;
            }
            set
            {
                _bitDepth = value;
            }
        }

        public Form1()
        {
            InitializeComponent();
            menuStrip1.Renderer = new MyRenderer();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            GetInputDevices(); // Pobiera dostępne urządzenia audio
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 7;
            InitializeButtons(); // Ustawia tekst w przyciskach i polach tekstowych
            dataGridView2.BackgroundColor = Color.Red;
        }

        private void connect_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                MessageBox.Show("Wpisz swój pseudonim", "Błąd",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            } else if (textBox2.Text == "")
            {
                MessageBox.Show("Wpisz adres IP", "Błąd",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else {
                initializeConnection();
            }
        }        

        private void sentmsg_Click(object sender, EventArgs e)
        {
            if (isConnected == true)
            {
                try {
                    byte[] data = new byte[3 + textBox3.Text.Length + textBox1.Text.Length];
                    byte[] userNickname = Encoding.ASCII.GetBytes(textBox1.Text);
                    byte[] message = Encoding.ASCII.GetBytes(textBox3.Text);
                    data[0] = Convert.ToByte(2); //Wiadomości z czatu mają na początku tablicy wartość 2
                    data[1] = Convert.ToByte(textBox1.Text.Length); //2 pole tablicy to długość nazwy usera
                    data[2] = Convert.ToByte(textBox3.Text.Length); //3 pole tablicy to długość wiadomości
                    for (int i = 0; i < userNickname.Length; i++)
                    {
                        data[i + 3] = userNickname[i];
                    }
                    for (int i = 0; i < message.Length; i++)
                    {
                        data[i + 3 + userNickname.Length] = message[i];
                    }

                    client.Send(data, data.Length);
                    textBox3.Clear();
                }
                catch (Exception ex)
                {
                    var result = MessageBox.Show("Wystąpił błąd", "Błąd",
                        MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
                    if(result == DialogResult.OK)
                    {
                        MessageBox.Show(ex.ToString(), "Błąd",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        private void disconnect_Click(object sender, EventArgs e)
        {
            disconnect();   
        }

        private void disconnect()
        {
            if (isConnected == true)
            {
                try
                {
                    byte[] userNickname = Encoding.ASCII.GetBytes(textBox1.Text);
                    byte[] data = new byte[2 + userNickname.Length];
                    data[0] = Convert.ToByte(3); //Wiadomość do serwera o rozłączeniu, ma na początku tablicy wartość 3
                    data[1] = Convert.ToByte(userNickname.Length);
                    for (int i = 0; i < userNickname.Length; i++)
                    {
                        data[i + 2] = userNickname[i];
                    }
                    client.Send(data, data.Length);
                    client.Close();
                    thread.Suspend();
                    thread2.Suspend();
                    sourceStream.Dispose();
                    waveIn.Dispose();
                    waveOut.Dispose();
                    waveProvider.ClearBuffer();
                    listBox1.Items.Clear();
                    textBox1.Enabled = true;
                    connect_btn.Enabled = true;
                    dataGridView2.BackgroundColor = Color.Red;
                    isConnected = false;
                }
                catch (Exception ex)
                {
                    var result = MessageBox.Show("Wystąpił błąd", "Błąd",
                        MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
                    if (result == DialogResult.OK)
                    {
                        MessageBox.Show(ex.ToString(), "Błąd",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        private void GetInputDevices()
        {
            for (int i = 0; i < WaveIn.DeviceCount; i++)
            {
                var capabilities = WaveIn.GetCapabilities(i);
                comboBox3.Items.Add(capabilities.ProductName);
            }

            if (comboBox3.Items.Count > 0)
            {
                comboBox3.SelectedIndex = 0;
            }
        }

        private void InitializeButtons()
        {
            connect_btn.Text = "POŁĄCZ";
            disconnect_btn.Text = "ROZŁĄCZ";
            button3.Text = "CZAT GŁÓWNY";
            sentmsg_btn.Text = "WYŚLIJ";
            textBox1.Text = "Nickname...";
            textBox2.Text = "127.0.0.1";
            textBox5.Text = "15000";
            textBox3.Text = "Twoja wiadomość...";
            textBox4.ReadOnly = true;
        }

        private void InitializeWaveIn()
        {
            waveIn = new WaveIn();
            waveIn.BufferMilliseconds = 50;
            waveIn.DeviceNumber = deviceID;
            waveIn.WaveFormat = new WaveFormat(bitRate, bitDepth, WaveIn.GetCapabilities(deviceID).Channels);
        }

        private void SendWelcomeMessage()
        {
            byte[] userNickname = Encoding.ASCII.GetBytes(textBox1.Text);
            byte[] data = new byte[2 + userNickname.Length];
            data[0] = Convert.ToByte(1); //Wiadomość o dołączeniu do serwera ma na początku tablicy wartość 1
            data[1] = Convert.ToByte(userNickname.Length);
            for (int i = 0; i < userNickname.Length; i++)
            {
                data[i + 2] = userNickname[i];
            }
            client.Send(data, data.Length);
        }

        private int ReceiveWelcomeMessage()
        {
            byte[] serverData = client.Receive(ref serverResponse);
            if (serverData[0] == Convert.ToByte(3))
            {
                textBox4.Text += "Nie można się połączyć z serwerem. Zostałeś zbanowany na tym serwerze." + Environment.NewLine;
                return 5;
            }
            else {
                string welcomeMessage = Encoding.ASCII.GetString(serverData);
                textBox4.Text += welcomeMessage + Environment.NewLine;
                return 4;
            }
        }

        private void ReceiveChatMessage()
        {
            byte[] user = new byte[serverData[1]];
            byte[] message = new byte[serverData[2]];
            for (int i = 0; i < serverData[1]; i++)
            {
                user[i] = serverData[i + 3];
            }
            for (int i = 0; i < serverData[2]; i++)
            {
                message[i] = serverData[i + 3 + serverData[1]];
            }
            String userNickname = Encoding.ASCII.GetString(user);
            String userMessage = Encoding.ASCII.GetString(message);
            string serverMessage = userNickname + ": " + userMessage;
            this.SetText(serverMessage);
        }

        private void ReceiveUsersList()
        {
            string usersList = Encoding.ASCII.GetString(serverData, 1, serverData.Length - 1);
            this.SetUserList(usersList);
        }

        private void voiceStreaming() {
            if (isConnected == true)
            {
                int recording = 2;
                waveIn.DataAvailable += sourcestream_DataAvailable;
                waveIn.StartRecording();
                while (true)
                {
                    if (isMeMuted == true)
                    {
                        if (recording == 2)
                        {
                            waveIn.StopRecording();
                            recording = 1;
                        }
                    } else if(isMeMuted == false)
                    {
                        if (recording == 1)
                        {
                            waveIn.StartRecording();
                            recording = 2;
                        }
                    }
                }
            }
        }

        private void initializeConnection()
        {
            try
            {
                initializeWaveInfo();
                client = new UdpClient();
                iep = new IPEndPoint(IPAddress.Parse(textBox2.Text), Convert.ToInt32(textBox5.Text));
                serverResponse = new IPEndPoint(IPAddress.Any, 0);
                client.Connect(iep);
                SendWelcomeMessage();
                if (ReceiveWelcomeMessage() == 5)
                {
                    client.Close();
                }
                else {
                    isConnected = true;
                    if (isConnected == true)
                    {
                        textBox1.Enabled = false;
                        connect_btn.Enabled = false;
                        dataGridView2.BackgroundColor = Color.Green;
                        InitializeWaveIn();
                        initializeWaveOut();
                        thread = new Thread(new ThreadStart(listening));
                        thread.Name = "ReceivedMessagesListener";
                        thread.Start();
                        thread2 = new Thread(new ThreadStart(voiceStreaming));
                        thread2.Name = "VoiceStreamer";
                        thread2.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                isConnected = false;
                var result = MessageBox.Show("Nie można połączyć się z serwerem", "Błąd",
                    MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning);
                if (result == DialogResult.Retry)
                {
                    initializeConnection();
                }
            }
        }

        private void initializeWaveInfo()
        {
            //bitRate = int.Parse(comboBox2.SelectedItem.ToString());
            //bitDepth = int.Parse(comboBox1.SelectedItem.ToString());
            deviceID = comboBox3.SelectedIndex;
        }

        private void initializeWaveOut()
        {
            waveProvider = null;
            sourceStream = null;
            waveOut = new WaveOut();
            sourceStream = new WaveIn();
            sourceStream.BufferMilliseconds = 150;
            sourceStream.DeviceNumber = 0;
            sourceStream.WaveFormat = new WaveFormat(bitRate, bitDepth, WaveIn.GetCapabilities(deviceID).Channels);
            waveProvider = new BufferedWaveProvider(sourceStream.WaveFormat);
        }

        private void listening()
        {
            if (isConnected == true)
            {
                try
                {
                    waveOut.Init(waveProvider);
                    waveOut.Play(); //Odtwarza odebrane audio
                    
                    while (true)
                    {
                        serverData = client.Receive(ref serverResponse);
                        //Podział odebranych wiadomości ze wzglęgu na zawartość pierwszego bajta w tablicy
                        //Jesli 2 to jest to wiadomość z czatu
                        //Jeśli 5 to jest to lista userów
                        //Jeśli 9 to jest to wiadomość audio
                        if (serverData[0] == Convert.ToByte(2))
                        {
                            ReceiveChatMessage();
                        }
                        else if (serverData[0] == Convert.ToByte(5))
                        {
                            ReceiveUsersList();
                        }
                        else if (serverData[0] == Convert.ToByte(6))
                        {
                            String reason = Encoding.ASCII.GetString(serverData, 1, (serverData.Length - 1));
                            String kickMsg = "Zostałeś wyrzucony z serwera. Powód: " + reason;
                            SetText(kickMsg);
                            DisconnectKick();
                        }
                        else if (serverData[0] == Convert.ToByte(7))
                        {
                            String reason = Encoding.ASCII.GetString(serverData, 1, (serverData.Length - 1));
                            String banMsg = "Zostałeś zbanowany na serwerze. Powód: " + reason;
                            SetText(banMsg);
                            DisconnectKick();
                        }
                        else if (serverData[0] == Convert.ToByte(9))
                        {
                            if (isAllMuted == false)
                            {
                                waveProvider.AddSamples(serverData, 0, serverData.Length);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(e.ToString());
                }
            }
        }

        delegate void SetTextCallback(string text);
        delegate void SetUserListCallback(string text);
        delegate void DisconnectKickCallback();

        private void DisconnectKick()
        {
            if(this.InvokeRequired)
            {
                DisconnectKickCallback d = new DisconnectKickCallback(DisconnectKick);
                this.Invoke(d, new object[] { });
            } else
            {
                disconnect();
            }
        }

        //Ta metoda dodaje nowy tekst na czacie
        //Trzeba taką metodą bo dodaje innym wątkiem niż ten, który stworzył textboxa
        private void SetText(string text)
        {
            if (this.textBox4.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.textBox4.Text += text + Environment.NewLine;
            }
        }

        //Ta metoda aktualizuje liste userów
        //Trzeba taką metodą bo aktualizuje innym wątkiem niż ten, który stworzył listboxa
        private void SetUserList(string text)
        {
            if (this.listBox1.InvokeRequired)
            {
                SetUserListCallback d = new SetUserListCallback(SetUserList);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                listBox1.Items.Clear(); 
                listBox1.Items.AddRange(text.Split('*'));
                listBox1.Items.RemoveAt(listBox1.Items.Count - 1);
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            isMeMuted = !isMeMuted;
            if(isMeMuted == true)
            {
                pictureBox4.Visible = true;
            }
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            isMeMuted = !isMeMuted;
            if (isMeMuted == false)
            {
                pictureBox4.Visible = false;
            }
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            isAllMuted = !isAllMuted;
            if (isAllMuted == false)
            {
                pictureBox3.Visible = false;
            }
        }

        private void ulubioneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Favourites form = new Favourites();
            form.Show();
        }

        private void kontaktToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Wszelkie pytania i uwagi można zgłaszać na" + Environment.NewLine + "filip.kaszczynski@gmail.com", "Kontakt",
                MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private void instrukcjaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Help form = new Help();
            form.Show();
        }

        private void opcjeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings form = new Settings();
            form.Show();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            isAllMuted = !isAllMuted;
            if (isAllMuted == true)
            {
                pictureBox3.Visible = true;
            }
        }

        private void sourcestream_DataAvailable(object notUsed, WaveInEventArgs e)
        {
            try
            {
                byte[] buffer = (e.Buffer);
                buffer[0] = Convert.ToByte(9);
                client.Send(buffer, buffer.Length);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private class MyRenderer : ToolStripProfessionalRenderer
        {
            public MyRenderer() : base(new MyColors()) { }
        }

        private class MyColors : ProfessionalColorTable
        {
            public override Color MenuItemSelected
            {
                get { return Color.DarkCyan; }
            }
            public override Color MenuItemSelectedGradientBegin
            {
                get { return Color.DarkCyan; }
            }
            public override Color MenuItemSelectedGradientEnd
            {
                get { return Color.DarkCyan; }
            }
            public override Color MenuItemPressedGradientBegin
            {
                get { return Color.DarkCyan; }
            }
            public override Color MenuItemPressedGradientEnd
            {
                get { return Color.DarkCyan; }
            }
            public override Color MenuItemBorder
            {
                get { return Color.DarkGray; }
            }
            public override Color MenuBorder
            {
                get { return Color.Cyan; }
            }
        }
    }
}
