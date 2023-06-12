using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BlueTeeApp
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            numericUpDown7.Value = numericUpDown2.Value - numericUpDown1.Value;
            numericUpDown11.Value = numericUpDown1.Value - numericUpDown2.Value;
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            numericUpDown8.Value = numericUpDown3.Value - numericUpDown1.Value;
            numericUpDown16.Value = numericUpDown1.Value - numericUpDown3.Value;

            numericUpDown13.Value = numericUpDown3.Value - numericUpDown2.Value;
            numericUpDown17.Value = numericUpDown2.Value - numericUpDown3.Value;
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            numericUpDown9.Value = numericUpDown4.Value - numericUpDown1.Value;
            numericUpDown21.Value = numericUpDown1.Value - numericUpDown4.Value;

            numericUpDown14.Value = numericUpDown4.Value - numericUpDown2.Value;
            numericUpDown22.Value = numericUpDown2.Value - numericUpDown4.Value;

            numericUpDown19.Value = numericUpDown4.Value - numericUpDown3.Value;
            numericUpDown23.Value = numericUpDown3.Value - numericUpDown4.Value;
        }

        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
        {
            numericUpDown10.Value = numericUpDown5.Value - numericUpDown1.Value;
            numericUpDown26.Value = numericUpDown1.Value - numericUpDown5.Value;

            numericUpDown15.Value = numericUpDown5.Value - numericUpDown2.Value;
            numericUpDown27.Value = numericUpDown2.Value - numericUpDown5.Value;

            numericUpDown20.Value = numericUpDown5.Value - numericUpDown3.Value;
            numericUpDown28.Value = numericUpDown3.Value - numericUpDown5.Value;

            numericUpDown25.Value = numericUpDown5.Value - numericUpDown4.Value;
            numericUpDown29.Value = numericUpDown4.Value - numericUpDown5.Value;

            //totals
            numericUpDown35.Value = numericUpDown26.Value + numericUpDown11.Value + numericUpDown16.Value + numericUpDown21.Value;                                                         
            numericUpDown34.Value = numericUpDown7.Value + numericUpDown27.Value + numericUpDown17.Value + numericUpDown22.Value;
            numericUpDown33.Value = numericUpDown8.Value + numericUpDown13.Value + numericUpDown28.Value + numericUpDown23.Value;                                                         
            numericUpDown32.Value = numericUpDown9.Value + numericUpDown14.Value + numericUpDown19.Value + numericUpDown29.Value;
            numericUpDown31.Value = numericUpDown10.Value + numericUpDown15.Value + numericUpDown20.Value + numericUpDown25.Value;
        }
    }
}
