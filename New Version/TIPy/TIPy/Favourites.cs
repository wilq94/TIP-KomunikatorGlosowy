using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TIPy
{
    public partial class Favourites : Form
    {
        TextBox setIP;
        TextBox setPort;

        public Favourites(TextBox IP, TextBox Port)
        {
            InitializeComponent();
            this.FormClosed += MyClosedHandler;
            setIP = IP;
            setPort = Port;
        }

        private void MyClosedHandler(object sender, EventArgs e)
        {            
            string directoryPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            using (StreamWriter outputFile = new StreamWriter(directoryPath + @"\FavouriteServers.txt", false))
            {
                 foreach (var listBoxItem in listBox1.Items)
                {
                    outputFile.WriteLine(listBoxItem.ToString());
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                MessageBox.Show("Podaj nazwę serwera!", "Błąd",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (textBox2.Text == "")
            {
                MessageBox.Show("Podaj adres serwera!", "Błąd",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else {
                String fav = textBox1.Text + " | IP -" + textBox2.Text;
                listBox1.Items.Add(fav);
                textBox1.Text = ""; textBox2.Text = "";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItems.Count != 0)
            {
                String srvSelected = listBox1.SelectedItem.ToString();
                String[] srvDetails = srvSelected.Split('-');
                String[] srvIPPort = srvDetails[1].Split(':');
                setIP.Text = srvIPPort[0];
                setPort.Text = srvIPPort[1];
                this.Close();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItems.Count != 0)
            {
                listBox1.Items.Remove(listBox1.SelectedItem);
            }
        }

        private void Favourites_Load(object sender, EventArgs e)
        {
            try {
                int counter = 0;
                string line;
                string directoryPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                StreamReader file = new StreamReader(directoryPath + @"\FavouriteServers.txt");
                while ((line = file.ReadLine()) != null)
                {
                    listBox1.Items.Add(line);
                    counter++;
                }
                file.Close();
            } catch (Exception)
            {
                
            }
        }
    }
}
