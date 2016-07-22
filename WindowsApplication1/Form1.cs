using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace WindowsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            double d = double.Parse(textBox1.Text);
            label1.Text = DateTime.FromOADate(d).ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Text = System.Convert.ToDouble(DateTime.Now).ToString();
        }
    }
}