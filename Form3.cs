using System;
using static System.Math;
using System.Windows.Forms;

namespace BlueTeeApp
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            CalcPointsNpayout();
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            CalcPointsNpayout();
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            CalcPointsNpayout();
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            CalcPointsNpayout();
        }

        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
        {
            CalcPointsNpayout();
        }

        private void numericUpDown41_ValueChanged(object sender, EventArgs e)
        {
            // UPDATE ALL DOLLAR amounts if the point value changes
            numericUpDown40.Value = (int)(numericUpDown35.Value * numericUpDown41.Value);
            numericUpDown39.Value = (int)(numericUpDown34.Value * numericUpDown41.Value);
            numericUpDown38.Value = (int)(numericUpDown33.Value * numericUpDown41.Value);
            numericUpDown37.Value = (int)(numericUpDown32.Value * numericUpDown41.Value);
            numericUpDown36.Value = (int)(numericUpDown31.Value * numericUpDown41.Value);
        }

        private void numericUpDown35_ValueChanged(object sender, EventArgs e)
        {
            //$ AMOUNT
            numericUpDown40.Value = (int)(numericUpDown35.Value * numericUpDown41.Value);
        }
        private void numericUpDown34_ValueChanged(object sender, EventArgs e)
        {
            //$ AMOUNT
            numericUpDown39.Value = (int)(numericUpDown34.Value * numericUpDown41.Value);
        }

        private void numericUpDown33_ValueChanged(object sender, EventArgs e)
        {
            //$ AMOUNT
            numericUpDown38.Value = (int)(numericUpDown33.Value * numericUpDown41.Value);
        }

        private void numericUpDown32_ValueChanged(object sender, EventArgs e)
        {
            //$ AMOUNT
            numericUpDown37.Value = (int)(numericUpDown32.Value * numericUpDown41.Value);
        }

        private void numericUpDown31_ValueChanged(object sender, EventArgs e)
        {
            //$ AMOUNT
            numericUpDown36.Value = (int)(numericUpDown31.Value * numericUpDown41.Value);
        }

        private void CalcPointsNpayout ()
        {
            //1st two players
            numericUpDown7.Value = numericUpDown2.Value - numericUpDown1.Value;
            numericUpDown11.Value = numericUpDown1.Value - numericUpDown2.Value;

            //1st three players
            numericUpDown8.Value = numericUpDown3.Value - numericUpDown1.Value;
            numericUpDown16.Value = numericUpDown1.Value - numericUpDown3.Value;

            numericUpDown13.Value = numericUpDown3.Value - numericUpDown2.Value;
            numericUpDown17.Value = numericUpDown2.Value - numericUpDown3.Value;

            //1st Four players            
            numericUpDown9.Value = numericUpDown4.Value - numericUpDown1.Value;
            numericUpDown21.Value = numericUpDown1.Value - numericUpDown4.Value;

            numericUpDown14.Value = numericUpDown4.Value - numericUpDown2.Value;
            numericUpDown22.Value = numericUpDown2.Value - numericUpDown4.Value;

            numericUpDown19.Value = numericUpDown4.Value - numericUpDown3.Value;
            numericUpDown23.Value = numericUpDown3.Value - numericUpDown4.Value;

            //All Five players
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
