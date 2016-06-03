using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TIPy
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            comboBox2.SelectedItem = Form1.bitDepth.ToString();
            comboBox3.SelectedItem = Form1.bitRate.ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form1.bitDepth = int.Parse(comboBox2.SelectedItem.ToString());
            Form1.bitRate = int.Parse(comboBox3.SelectedItem.ToString());
            this.Close();
        }
    }
}
