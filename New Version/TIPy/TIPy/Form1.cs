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
        Thread thread, thread2, thread3;
        int bitRate = 0, bitDepth = 0, deviceID = 0;
        byte[] serverData;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            GetInputDevices();
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 7;
            InitializeButtons();
        }

        private void connect_Click(object sender, EventArgs e)
        {
            textBox1.Enabled = false;
            connect_btn.Enabled = false;

            initializeWaveInfo();
            client = new UdpClient();
            iep = new IPEndPoint(IPAddress.Parse(textBox2.Text), 15000);
            serverResponse = new IPEndPoint(IPAddress.Any, 0);
            //InitializeWaveIn();
            client.Connect(iep);
            SendWelcomeMessage();
            ReceiveWelcomeMessage();
            InitializeWaveIn();
            initializeWaveOut();
            thread = new Thread(new ThreadStart(listening));
            thread.Start();
            thread2 = new Thread(new ThreadStart(voiceStreaming));
            thread2.Start();
            //voiceStreaming();
            //thread3 = new Thread(new ThreadStart(voiceListening));
            //thread3.Start();
        }

        private void initializeWaveInfo()
        {
            bitRate = int.Parse(comboBox2.SelectedItem.ToString());
            bitDepth = int.Parse(comboBox1.SelectedItem.ToString());
            deviceID = comboBox3.SelectedIndex;
        }

        private void initializeWaveOut()
        {
            waveProvider = null;
            sourceStream = null;
            waveOut = new WaveOut();
            sourceStream = new WaveIn();
            sourceStream.BufferMilliseconds = 50;
            sourceStream.DeviceNumber = 0;
            sourceStream.WaveFormat = new WaveFormat(bitRate, bitDepth, WaveIn.GetCapabilities(deviceID).Channels);
            waveProvider = new BufferedWaveProvider(sourceStream.WaveFormat);
        }

        private void voiceListening()
        {
            waveOut.Init(waveProvider);
            waveOut.Play();

            while (true)
            {
                byte[] buffer = client.Receive(ref serverResponse);
                if (buffer[0] == Convert.ToByte(9))
                {
                    waveProvider.AddSamples(buffer, 0, buffer.Length);
                }
            }
        }

        private void listening()
        {
            try {
                waveOut.Init(waveProvider);
                waveOut.Play();
                while (true)
                {
                    serverData = client.Receive(ref serverResponse);
                    if (serverData[0] == Convert.ToByte(2)) {
                        ReceiveChatMessage();
                    } else if (serverData[0] == Convert.ToByte(5))  {
                        ReceiveUsersList();
                    } else if (serverData[0] == Convert.ToByte(9))
                    {
                        waveProvider.AddSamples(serverData, 0, serverData.Length);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        delegate void SetTextCallback(string text);
        delegate void SetUserListCallback(string text);

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

        private void sentmsg_Click(object sender, EventArgs e)
        {
            byte[] data = new byte[3 + textBox3.Text.Length + textBox1.Text.Length];
            byte[] userNickname = Encoding.ASCII.GetBytes(textBox1.Text);
            byte[] message = Encoding.ASCII.GetBytes(textBox3.Text);
            data[0] = Convert.ToByte(2);
            data[1] = Convert.ToByte(textBox1.Text.Length);
            data[2] = Convert.ToByte(textBox3.Text.Length);
            for (int i = 0; i < userNickname.Length; i++)
            {
                data[i + 3] = userNickname[i];
            }
            for (int i = 0; i < message.Length; i++)
            {
                data[i + 3 + userNickname.Length] = message[i];
            }

            client.Send(data, data.Length);
            textBox3.Text = "Wiadomość wysłana";
        }

        private void disconnect_Click(object sender, EventArgs e)
        {
            byte[] userNickname = Encoding.ASCII.GetBytes(textBox1.Text);
            byte[] data = new byte[2 + userNickname.Length];
            data[0] = Convert.ToByte(3);
            data[1] = Convert.ToByte(userNickname.Length);
            for (int i = 0; i < userNickname.Length; i++)
            {
                data[i + 2] = userNickname[i];
            }
            client.Send(data, data.Length);
            thread.Abort();
            client.Close();
            listBox1.Items.Clear();
            textBox1.Enabled = true;
            connect_btn.Enabled = true;
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
            textBox3.Text = "Twoja wiadomość...";
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
            data[0] = Convert.ToByte(1);
            data[1] = Convert.ToByte(userNickname.Length);
            for (int i = 0; i < userNickname.Length; i++)
            {
                data[i + 2] = userNickname[i];
            }
            client.Send(data, data.Length);
        }

        private void ReceiveWelcomeMessage()
        {
            byte[] serverData = client.Receive(ref serverResponse);
            string welcomeMessage = Encoding.ASCII.GetString(serverData);
            textBox4.Text += welcomeMessage + Environment.NewLine;
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

        private void voiceStreaming()
        {
            //InitializeWaveIn();
            waveIn.DataAvailable += sourcestream_DataAvailable;
            waveIn.StartRecording();
            while (true)
            {

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

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
