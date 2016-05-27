using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KomunikatorGlosowyKlient
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
            Text = "Form2";
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            trackBar1.ValueChanged += new System.EventHandler(trackBar1_ValueChanged);
            //AL.Listener(ALListenerf.Gain, (float)4);
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            textBox1.Text = (trackBar1.Value).ToString() + " db";
        }
    }
}
