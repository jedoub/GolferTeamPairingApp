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
    public partial class Form2 : Form
    {
        static decimal[] playerScores, teamScoresFront, teamScoresBack, teamScoresTotal;
        static bool lowScorerHit = false;
        static string nameOfLastPlayer = null, nameOfLowScorer = null;
        private static decimal maxScore = 0;

        private static int playerCnt = 0;
        public Form2()
        {
            InitializeComponent();

            //numericUpDown7.Controls[0].Visible = false;
            //numericUpDown8.Controls[0].Visible = false;
            //numericUpDown9.Controls[0].Visible = false;

        }
        private void Form2_Load(object sender, EventArgs e)
        {
            // create array with the correct size
            //Since the PlayerCnt has already been incremented subtract ONE
            playerCnt = Form1.playerCnt - 1;
            playerScores = new decimal[playerCnt];
            
            teamScoresFront = new decimal[playerCnt];
            teamScoresBack = new decimal[playerCnt];
            teamScoresTotal = new decimal[playerCnt];

            if (!((playerCnt % 2) == 0))
            {
                //Odd number of players                
                button1.Visible = true;

                //and fill in a bogus front and back 9 score
                if (Form1.playerCnt == 8)
                {
                    //numericUpDown51.Value = 25;
                    //numericUpDown50.Value = 25;
                    numericUpDown51.Visible = false;
                    numericUpDown50.Visible = false;
                    numericUpDown49.Visible = false;

                    numericUpDown48.Visible = false;
                    numericUpDown47.Visible = false;
                    numericUpDown46.Visible = false;
                    nameOfLastPlayer = twoManRTB.Lines[16];
                }
                else if (Form1.playerCnt == 10)
                {
                    //numericUpDown42.Value = 25;
                    //numericUpDown41.Value = 25;
                    numericUpDown42.Visible = false;
                    numericUpDown41.Visible = false;
                    numericUpDown40.Visible = false;

                    numericUpDown39.Visible = false;
                    numericUpDown38.Visible = false;
                    numericUpDown37.Visible = false;
                    nameOfLastPlayer = twoManRTB.Lines[21];
                }
                else if (Form1.playerCnt == 12)
                {
                    //numericUpDown33.Value = 25;
                    //numericUpDown32.Value = 25;

                    numericUpDown33.Visible = false;
                    numericUpDown32.Visible = false;
                    numericUpDown31.Visible = false;

                    numericUpDown30.Visible = false;
                    numericUpDown29.Visible = false;
                    numericUpDown28.Visible = false;
                    nameOfLastPlayer = twoManRTB.Lines[26];
                }
                else if (Form1.playerCnt == 14)
                {
                    //numericUpDown60.Value = 25;
                    //numericUpDown59.Value = 25;
                    numericUpDown60.Visible = false;
                    numericUpDown59.Visible = false;
                    numericUpDown58.Visible = false;

                    numericUpDown57.Visible = false;
                    numericUpDown56.Visible = false;
                    numericUpDown55.Visible = false;
                    nameOfLastPlayer = twoManRTB.Lines[31];
                }
            }
            else
            {
                //Even number of Players
                button1.Visible = false;
                button2.Visible = true;
                button3.Visible = true;
            }

            //Initialize the values to zero
            for (int i = 0; i < playerScores.Length; i++)
            {
                playerScores[i] = 0;
                
                teamScoresFront[i] = 0;
                teamScoresBack[i] = 0;
                teamScoresTotal[i] = 0;
            }

            //Reset the colors
            numericUpDown3.ForeColor = Color.Black;
            numericUpDown3.BackColor = Color.White;
            numericUpDown4.ForeColor = Color.Black;
            numericUpDown4.BackColor = Color.White;
            numericUpDown7.ForeColor = Color.Black;
            numericUpDown7.BackColor = Color.White;
            numericUpDown8.ForeColor = Color.Black;
            numericUpDown8.BackColor = Color.White;
            numericUpDown9.ForeColor = Color.Black;
            numericUpDown9.BackColor = Color.White;

            numericUpDown16.ForeColor = Color.Black;
            numericUpDown16.BackColor = Color.White;
            numericUpDown13.ForeColor = Color.Black;
            numericUpDown13.BackColor = Color.White;
            numericUpDown10.ForeColor = Color.Black;
            numericUpDown10.BackColor = Color.White;
            numericUpDown11.ForeColor = Color.Black;
            numericUpDown11.BackColor = Color.White;
            numericUpDown12.ForeColor = Color.Black;
            numericUpDown12.BackColor = Color.White;

            numericUpDown25.ForeColor = Color.Black;
            numericUpDown25.BackColor = Color.White;
            numericUpDown22.ForeColor = Color.Black;
            numericUpDown22.BackColor = Color.White;
            numericUpDown19.ForeColor = Color.Black;
            numericUpDown19.BackColor = Color.White;
            numericUpDown21.ForeColor = Color.Black;
            numericUpDown21.BackColor = Color.White;
            numericUpDown20.ForeColor = Color.Black;
            numericUpDown20.BackColor = Color.White;

            numericUpDown52.ForeColor = Color.Black;
            numericUpDown52.BackColor = Color.White;
            numericUpDown49.ForeColor = Color.Black;
            numericUpDown49.BackColor = Color.White;
            numericUpDown46.ForeColor = Color.Black;
            numericUpDown46.BackColor = Color.White;
            numericUpDown47.ForeColor = Color.Black;
            numericUpDown47.BackColor = Color.White;
            numericUpDown48.ForeColor = Color.Black;
            numericUpDown4.BackColor = Color.White;

            numericUpDown43.ForeColor = Color.Black;
            numericUpDown43.BackColor = Color.White;
            numericUpDown40.ForeColor = Color.Black;
            numericUpDown40.BackColor = Color.White;
            numericUpDown37.ForeColor = Color.Black;
            numericUpDown37.BackColor = Color.White;
            numericUpDown38.ForeColor = Color.Black;
            numericUpDown38.BackColor = Color.White;
            numericUpDown39.ForeColor = Color.Black;
            numericUpDown39.BackColor = Color.White;

            numericUpDown34.ForeColor = Color.Black;
            numericUpDown34.BackColor = Color.White;
            numericUpDown31.ForeColor = Color.Black;
            numericUpDown31.BackColor = Color.White;
            numericUpDown28.ForeColor = Color.Black;
            numericUpDown28.BackColor = Color.White;
            numericUpDown29.ForeColor = Color.Black;
            numericUpDown29.BackColor = Color.White;
            numericUpDown30.ForeColor = Color.Black;
            numericUpDown30.BackColor = Color.White;

            numericUpDown61.ForeColor = Color.Black;
            numericUpDown61.BackColor = Color.White;
            numericUpDown58.ForeColor = Color.Black;
            numericUpDown58.BackColor = Color.White;
            numericUpDown55.ForeColor = Color.Black;
            numericUpDown55.BackColor = Color.White;
            numericUpDown56.ForeColor = Color.Black;
            numericUpDown56.BackColor = Color.White;
            numericUpDown57.ForeColor = Color.Black;
            numericUpDown57.BackColor = Color.White;
        }

        private void OKbtn_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void cancelbtn_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void changeLine(RichTextBox RTB, int line, string text)
        {
            int s1 = RTB.GetFirstCharIndexFromLine(line);
            int s2 = line < RTB.Lines.Count() - 1 ?
                      RTB.GetFirstCharIndexFromLine(line + 1) - 1 :
                      RTB.Text.Length;
            RTB.Select(s1, s2 - s1);
            RTB.SelectedText = text;
        }

        # region "NumericUpDn ValueChanged"
        // ADD UP THE INDIVIDUAL AND TEAMS SCORES FRONT BACK AND TOTAL
        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                    numericUpDown9.Value = numericUpDown6.Value + numericUpDown1.Value;
                    numericUpDown3.Value = numericUpDown1.Value + numericUpDown2.Value;
            }
            catch (System.Exception except)
            {
                MessageBox.Show(except.Message, "Error");
            }
        }
        private void numericUpDown6_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                    numericUpDown9.Value = numericUpDown6.Value + numericUpDown1.Value;
                    numericUpDown4.Value = numericUpDown5.Value + numericUpDown6.Value;
            }
            catch (System.Exception except)
            {
                MessageBox.Show(except.Message, "Error");
            }
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            try
                {
                    numericUpDown8.Value = numericUpDown5.Value + numericUpDown2.Value;
                    numericUpDown3.Value = numericUpDown1.Value + numericUpDown2.Value;
                }
            catch (System.Exception except)
            {
                MessageBox.Show(except.Message, "Error");
            }
        }
        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                numericUpDown8.Value = numericUpDown5.Value + numericUpDown2.Value;
                numericUpDown4.Value = numericUpDown5.Value + numericUpDown6.Value;
            }
            catch (System.Exception except) { MessageBox.Show(except.Message, "Error"); }
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            try { //TEAM1 Total
            numericUpDown7.Value = numericUpDown4.Value + numericUpDown3.Value;
            playerScores[0] = numericUpDown3.Value;
            }
            catch (System.Exception except) { MessageBox.Show(except.Message, "Error"); }
        }
        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            try { //TEAM1 Total
            numericUpDown7.Value = numericUpDown4.Value + numericUpDown3.Value;
            playerScores[1] = numericUpDown4.Value;
                }
                catch (System.Exception except) { MessageBox.Show(except.Message, "Error"); }
            }

        private void numericUpDown18_ValueChanged(object sender, EventArgs e)
        {try
            {
                numericUpDown12.Value = numericUpDown15.Value + numericUpDown18.Value;
                numericUpDown16.Value = numericUpDown17.Value + numericUpDown18.Value;
            }
            catch (System.Exception except) { MessageBox.Show(except.Message, "Error"); }
        }
        private void numericUpDown15_ValueChanged(object sender, EventArgs e)
        {try
            {
                numericUpDown12.Value = numericUpDown15.Value + numericUpDown18.Value;
                numericUpDown13.Value = numericUpDown15.Value + numericUpDown14.Value;
            }
            catch (System.Exception except) { MessageBox.Show(except.Message, "Error"); }
        }

        private void numericUpDown17_ValueChanged(object sender, EventArgs e)
        {try
            {
                numericUpDown11.Value = numericUpDown14.Value + numericUpDown17.Value;
                numericUpDown16.Value = numericUpDown17.Value + numericUpDown18.Value;
            }
            catch (System.Exception except) { MessageBox.Show(except.Message, "Error"); }
        }
        private void numericUpDown14_ValueChanged(object sender, EventArgs e)
        {try
            {
                numericUpDown11.Value = numericUpDown14.Value + numericUpDown17.Value;
                numericUpDown13.Value = numericUpDown15.Value + numericUpDown14.Value;
            }
            catch (System.Exception except) { MessageBox.Show(except.Message, "Error"); }
        }

        private void numericUpDown13_ValueChanged(object sender, EventArgs e)
        {
            try { //TEAM2 Total
            numericUpDown10.Value = numericUpDown13.Value + numericUpDown16.Value;
            playerScores[3] = numericUpDown13.Value;
            }
            catch (System.Exception except) { MessageBox.Show(except.Message, "Error"); }
        }
        private void numericUpDown16_ValueChanged(object sender, EventArgs e)
        {
            try { //TEAM2 Total
            numericUpDown10.Value = numericUpDown13.Value + numericUpDown16.Value;
            playerScores[2] = numericUpDown16.Value;
            }
            catch (System.Exception except) { MessageBox.Show(except.Message, "Error"); }
        }

        private void numericUpDown27_ValueChanged(object sender, EventArgs e)
        {try
            {
                numericUpDown21.Value = numericUpDown24.Value + numericUpDown27.Value;
                numericUpDown25.Value = numericUpDown26.Value + numericUpDown27.Value;
            }
            catch (System.Exception except) { MessageBox.Show(except.Message, "Error"); }
        }
        private void numericUpDown24_ValueChanged(object sender, EventArgs e)
        {try
            {
                numericUpDown21.Value = numericUpDown24.Value + numericUpDown27.Value;
                numericUpDown22.Value = numericUpDown24.Value + numericUpDown23.Value;
            }
            catch (System.Exception except) { MessageBox.Show(except.Message, "Error"); }
        }

        private void numericUpDown26_ValueChanged(object sender, EventArgs e)
        {try
            {
                numericUpDown20.Value = numericUpDown26.Value + numericUpDown23.Value;
                numericUpDown25.Value = numericUpDown26.Value + numericUpDown27.Value;
            }
            catch (System.Exception except) { MessageBox.Show(except.Message, "Error"); }
        }
        private void numericUpDown23_ValueChanged(object sender, EventArgs e)
        {try
            {
                numericUpDown20.Value = numericUpDown23.Value + numericUpDown26.Value;
                numericUpDown22.Value = numericUpDown24.Value + numericUpDown23.Value;
            }
            catch (System.Exception except) { MessageBox.Show(except.Message, "Error"); }
        }

        private void numericUpDown22_ValueChanged(object sender, EventArgs e)
        {
            try { //TEAM3 Total
            numericUpDown19.Value = numericUpDown22.Value + numericUpDown25.Value;
            playerScores[5] = numericUpDown22.Value;
            }
            catch (System.Exception except) { MessageBox.Show(except.Message, "Error"); }
        }
        private void numericUpDown25_ValueChanged(object sender, EventArgs e)
        {
            try { //TEAM3 Total
            numericUpDown19.Value = numericUpDown22.Value + numericUpDown25.Value;
            playerScores[4] = numericUpDown25.Value;
            }
            catch (System.Exception except) { MessageBox.Show(except.Message, "Error"); }
        }

        private void numericUpDown54_ValueChanged(object sender, EventArgs e)
        {try
            {
                numericUpDown48.Value = numericUpDown51.Value + numericUpDown54.Value;
                numericUpDown52.Value = numericUpDown53.Value + numericUpDown54.Value;
            }
            catch (System.Exception except) { MessageBox.Show(except.Message, "Error"); }
        }
        private void numericUpDown51_ValueChanged(object sender, EventArgs e)
        {try
            {
                numericUpDown48.Value = numericUpDown51.Value + numericUpDown54.Value;
                numericUpDown49.Value = numericUpDown51.Value + numericUpDown50.Value;
            }
            catch (System.Exception except) { MessageBox.Show(except.Message, "Error"); }
        }

        private void numericUpDown53_ValueChanged(object sender, EventArgs e)
        {try
            {
                numericUpDown47.Value = numericUpDown50.Value + numericUpDown53.Value;
                numericUpDown52.Value = numericUpDown53.Value + numericUpDown54.Value;
            }
            catch (System.Exception except) { MessageBox.Show(except.Message, "Error"); }
        }
        private void numericUpDown50_ValueChanged(object sender, EventArgs e)
        {try
            {
                numericUpDown47.Value = numericUpDown50.Value + numericUpDown53.Value;
                numericUpDown49.Value = numericUpDown51.Value + numericUpDown50.Value;
            }
            catch (System.Exception except) { MessageBox.Show(except.Message, "Error"); }
        }

        private void numericUpDown52_ValueChanged(object sender, EventArgs e)
        {
            try { //TEAM4 Total
            numericUpDown46.Value = numericUpDown49.Value + numericUpDown52.Value;
            playerScores[6] = numericUpDown52.Value;
            }
            catch (System.Exception except) { MessageBox.Show(except.Message, "Error"); }
        }
        private void numericUpDown49_ValueChanged(object sender, EventArgs e)
        {
            try { //TEAM4 Total
            numericUpDown46.Value = numericUpDown49.Value + numericUpDown52.Value;
            playerScores[7] = numericUpDown49.Value;
            }
            catch (System.Exception except) { MessageBox.Show(except.Message, "Error"); }
        }

        private void numericUpDown45_ValueChanged(object sender, EventArgs e)
        {try
            {
                numericUpDown39.Value = numericUpDown42.Value + numericUpDown45.Value;
                numericUpDown43.Value = numericUpDown44.Value + numericUpDown45.Value;
            }
            catch (System.Exception except) { MessageBox.Show(except.Message, "Error"); }
        }
        private void numericUpDown42_ValueChanged(object sender, EventArgs e)
        {try
            {
                numericUpDown39.Value = numericUpDown42.Value + numericUpDown45.Value;
                numericUpDown40.Value = numericUpDown41.Value + numericUpDown42.Value;
            }
            catch (System.Exception except) { MessageBox.Show(except.Message, "Error"); }
        }

        private void numericUpDown44_ValueChanged(object sender, EventArgs e)
        {try
            {
                numericUpDown38.Value = numericUpDown41.Value + numericUpDown44.Value;
                numericUpDown43.Value = numericUpDown44.Value + numericUpDown45.Value;
            }
            catch (System.Exception except) { MessageBox.Show(except.Message, "Error"); }
        }
        private void numericUpDown41_ValueChanged(object sender, EventArgs e)
        {try
            {
                numericUpDown38.Value = numericUpDown41.Value + numericUpDown44.Value;
                numericUpDown40.Value = numericUpDown41.Value + numericUpDown42.Value;
            }
            catch (System.Exception except) { MessageBox.Show(except.Message, "Error"); }
        }

        private void numericUpDown43_ValueChanged(object sender, EventArgs e)
        {
            try { //TEAM5 Total
            numericUpDown37.Value = numericUpDown40.Value + numericUpDown43.Value;
            playerScores[8] = numericUpDown43.Value;
            }
            catch (System.Exception except) { MessageBox.Show(except.Message, "Error"); }
        }
        private void numericUpDown40_ValueChanged(object sender, EventArgs e)
        {
            try { //TEAM5 Total
            numericUpDown37.Value = numericUpDown40.Value + numericUpDown43.Value;
            playerScores[9] = numericUpDown40.Value;
            }
            catch (System.Exception except) { MessageBox.Show(except.Message, "Error"); }
        }


        private void numericUpDown36_ValueChanged(object sender, EventArgs e)
        {try
            {
                numericUpDown30.Value = numericUpDown33.Value + numericUpDown36.Value;
                numericUpDown34.Value = numericUpDown35.Value + numericUpDown36.Value;
            }
            catch (System.Exception except) { MessageBox.Show(except.Message, "Error"); }
        }
        private void numericUpDown33_ValueChanged(object sender, EventArgs e)
        {try
            {
                numericUpDown30.Value = numericUpDown33.Value + numericUpDown36.Value;
                numericUpDown31.Value = numericUpDown32.Value + numericUpDown33.Value;
            }
            catch (System.Exception except) { MessageBox.Show(except.Message, "Error"); }
        }

        private void numericUpDown35_ValueChanged(object sender, EventArgs e)
        {try
            {
                numericUpDown29.Value = numericUpDown32.Value + numericUpDown35.Value;
                numericUpDown34.Value = numericUpDown35.Value + numericUpDown36.Value;
            }
            catch (System.Exception except) { MessageBox.Show(except.Message, "Error"); }
        }
        private void numericUpDown32_ValueChanged(object sender, EventArgs e)
        {try
            {
                numericUpDown29.Value = numericUpDown32.Value + numericUpDown35.Value;
                numericUpDown31.Value = numericUpDown32.Value + numericUpDown33.Value;
            }
            catch (System.Exception except) { MessageBox.Show(except.Message, "Error"); }
        }

        private void numericUpDown34_ValueChanged(object sender, EventArgs e)
        {
            try { //TEAM6 Total
            numericUpDown28.Value = numericUpDown31.Value + numericUpDown34.Value;
            playerScores[10] = numericUpDown34.Value;
            }
            catch (System.Exception except) { MessageBox.Show(except.Message, "Error"); }
        }
        private void numericUpDown31_ValueChanged(object sender, EventArgs e)
        {
            try { //TEAM6 Total
            numericUpDown28.Value = numericUpDown31.Value + numericUpDown34.Value;
            playerScores[11] = numericUpDown31.Value;
            }
            catch (System.Exception except) { MessageBox.Show(except.Message, "Error"); }
        }


        private void numericUpDown63_ValueChanged(object sender, EventArgs e)
        {try
            {
                numericUpDown57.Value = numericUpDown60.Value + numericUpDown63.Value;
                numericUpDown61.Value = numericUpDown62.Value + numericUpDown63.Value;
            }
            catch (System.Exception except) { MessageBox.Show(except.Message, "Error"); }
        }
        private void numericUpDown60_ValueChanged(object sender, EventArgs e)
        {try
            {
                numericUpDown57.Value = numericUpDown60.Value + numericUpDown63.Value;
                numericUpDown58.Value = numericUpDown60.Value + numericUpDown59.Value;
            }
            catch (System.Exception except) { MessageBox.Show(except.Message, "Error"); }
        }

        private void numericUpDown62_ValueChanged(object sender, EventArgs e)
        {try
            {
                numericUpDown56.Value = numericUpDown59.Value + numericUpDown62.Value;
                numericUpDown61.Value = numericUpDown62.Value + numericUpDown63.Value;
            }
            catch (System.Exception except) { MessageBox.Show(except.Message, "Error"); }
        }
        private void numericUpDown59_ValueChanged(object sender, EventArgs e)
        {try
            {
                numericUpDown56.Value = numericUpDown59.Value + numericUpDown62.Value;
                numericUpDown58.Value = numericUpDown60.Value + numericUpDown59.Value;
            }
            catch (System.Exception except) { MessageBox.Show(except.Message, "Error"); }
        }

        private void numericUpDown61_ValueChanged(object sender, EventArgs e)
        {
            try { //TEAM7 Total
            numericUpDown55.Value = numericUpDown58.Value + numericUpDown61.Value;
            playerScores[12] = numericUpDown61.Value;
            }
            catch (System.Exception except) { MessageBox.Show(except.Message, "Error"); }
        }
        private void numericUpDown58_ValueChanged(object sender, EventArgs e)
        {
            try { //TEAM7 Total
            numericUpDown55.Value = numericUpDown58.Value + numericUpDown61.Value;
            playerScores[13] = numericUpDown58.Value;
            }
            catch (System.Exception except) { MessageBox.Show(except.Message, "Error"); }
        }

        private void numericUpDown9_ValueChanged(object sender, EventArgs e)
        {
            teamScoresFront[0] = numericUpDown9.Value;
        }

        private void numericUpDown8_ValueChanged(object sender, EventArgs e)
        {
            teamScoresBack[0] = numericUpDown8.Value;
        }

        private void numericUpDown7_ValueChanged(object sender, EventArgs e)
        {
            teamScoresTotal[0] = numericUpDown7.Value;
        }

        private void numericUpDown12_ValueChanged(object sender, EventArgs e)
        {
            teamScoresFront[1] = numericUpDown12.Value;
        }

        private void numericUpDown11_ValueChanged(object sender, EventArgs e)
        {
            teamScoresBack[1] = numericUpDown11.Value;
        }

        private void numericUpDown10_ValueChanged(object sender, EventArgs e)
        {
            teamScoresTotal[1] = numericUpDown10.Value;
        }

        private void numericUpDown21_ValueChanged(object sender, EventArgs e)
        {
            teamScoresFront[2] = numericUpDown21.Value;
        }

        private void numericUpDown20_ValueChanged(object sender, EventArgs e)
        {
            teamScoresBack[2] = numericUpDown20.Value;
        }

        private void numericUpDown19_ValueChanged(object sender, EventArgs e)
        {
            teamScoresTotal[2] = numericUpDown19.Value;
        }

        private void numericUpDown48_ValueChanged(object sender, EventArgs e)
        {
            teamScoresFront[3] = numericUpDown48.Value;
        }

        private void numericUpDown47_ValueChanged(object sender, EventArgs e)
        {
            teamScoresBack[3] = numericUpDown47.Value;
        }

        private void numericUpDown46_ValueChanged(object sender, EventArgs e)
        {
            teamScoresTotal[3] = numericUpDown46.Value;
        }

        private void numericUpDown39_ValueChanged(object sender, EventArgs e)
        {
            teamScoresFront[4] = numericUpDown39.Value;
        }

        private void numericUpDown38_ValueChanged(object sender, EventArgs e)
        {
            teamScoresBack[4] = numericUpDown38.Value;
        }

        private void numericUpDown37_ValueChanged(object sender, EventArgs e)
        {
            teamScoresTotal[4] = numericUpDown37.Value;
        }

        private void numericUpDown30_ValueChanged(object sender, EventArgs e)
        {
            teamScoresFront[5] = numericUpDown30.Value;
        }

        private void numericUpDown29_ValueChanged(object sender, EventArgs e)
        {
            teamScoresBack[5] = numericUpDown29.Value;
        }

        private void numericUpDown28_ValueChanged(object sender, EventArgs e)
        {
            teamScoresTotal[5] = numericUpDown28.Value;
        }

        private void numericUpDown57_ValueChanged(object sender, EventArgs e)
        {
            teamScoresFront[6] = numericUpDown57.Value;
        }

        private void numericUpDown56_ValueChanged(object sender, EventArgs e)
        {
            teamScoresBack[6] = numericUpDown56.Value;
        }

        private void numericUpDown55_ValueChanged(object sender, EventArgs e)
        {
            teamScoresTotal[6] = numericUpDown55.Value;
        }
        #endregion

        // SHOW THE HIGH SCORER
        private void button3_Click(object sender, EventArgs e)
        {
            if ((playerCnt % 2) == 0)
            {
                // With an even number the Kick out of the Low Player wasn't run
                Array.Sort(playerScores);

                maxScore = playerScores.Max();
            }

            //Highlight the High individual score
            if (numericUpDown3.Value == maxScore)
            {
                numericUpDown3.ForeColor = Color.White;
                numericUpDown3.BackColor = Color.LightGreen;
            }
            if (numericUpDown4.Value == maxScore)
            {
                numericUpDown4.ForeColor = Color.White;
                numericUpDown4.BackColor = Color.LightGreen;
            }/*
            if (numericUpDown7.Value == maxScore)
            {
                numericUpDown7.ForeColor = Color.White;
                numericUpDown7.BackColor = Color.LightGreen;
            }*/
            if (numericUpDown16.Value == maxScore)
            {
                numericUpDown16.ForeColor = Color.White;
                numericUpDown16.BackColor = Color.LightGreen;
            }
            if (numericUpDown13.Value == maxScore)
            {
                numericUpDown13.ForeColor = Color.White;
                numericUpDown13.BackColor = Color.LightGreen;
            }/*
            if (numericUpDown10.Value == maxScore)
            {
                numericUpDown10.ForeColor = Color.White;
                numericUpDown10.BackColor = Color.LightGreen;
            }*/
            if (numericUpDown25.Value == maxScore)
            {
                numericUpDown25.ForeColor = Color.White;
                numericUpDown25.BackColor = Color.LightGreen;
            }
            if (numericUpDown22.Value == maxScore)
            {
                numericUpDown22.ForeColor = Color.White;
                numericUpDown22.BackColor = Color.LightGreen;
            }/*
            if (numericUpDown19.Value == maxScore)
            {
                numericUpDown19.ForeColor = Color.White;
                numericUpDown19.BackColor = Color.LightGreen;
            }*/
            if (numericUpDown52.Value == maxScore)
            {
                numericUpDown52.ForeColor = Color.White;
                numericUpDown52.BackColor = Color.LightGreen;
            }
            if (numericUpDown49.Value == maxScore)
            {
                numericUpDown49.ForeColor = Color.White;
                numericUpDown49.BackColor = Color.LightGreen;
            }/*
            if (numericUpDown46.Value == maxScore)
            {
                numericUpDown46.ForeColor = Color.White;
                numericUpDown46.BackColor = Color.LightGreen;
            }*/
            if (numericUpDown43.Value == maxScore)
            {
                numericUpDown43.ForeColor = Color.White;
                numericUpDown43.BackColor = Color.LightGreen;
            }
            if (numericUpDown40.Value == maxScore)
            {
                numericUpDown40.ForeColor = Color.White;
                numericUpDown40.BackColor = Color.LightGreen;
            }/*
            if (numericUpDown37.Value == maxScore)
            {
                numericUpDown37.ForeColor = Color.White;
                numericUpDown37.BackColor = Color.LightGreen;
            }*/
            if (numericUpDown34.Value == maxScore)
            {
                numericUpDown34.ForeColor = Color.White;
                numericUpDown34.BackColor = Color.LightGreen;
            }
            if (numericUpDown31.Value == maxScore)
            {
                numericUpDown31.ForeColor = Color.White;
                numericUpDown31.BackColor = Color.LightGreen;
            }/*
            if (numericUpDown28.Value == maxScore)
            {
                numericUpDown28.ForeColor = Color.White;
                numericUpDown28.BackColor = Color.LightGreen;
            }*/
            if (numericUpDown61.Value == maxScore)
            {
                numericUpDown61.ForeColor = Color.White;
                numericUpDown61.BackColor = Color.LightGreen;
            }
            if (numericUpDown58.Value == maxScore)
            {
                numericUpDown58.ForeColor = Color.White;
                numericUpDown58.BackColor = Color.LightGreen;
            }/*
            if (numericUpDown55.Value == maxScore)
            {
                numericUpDown55.ForeColor = Color.White;
                numericUpDown55.BackColor = Color.LightGreen;
            }*/

        }

        // KICK OUT THE LOW PLAYER
        private void button1_Click(object sender, EventArgs e)
        {           
            // Kick out the Low Player
            Array.Sort(playerScores);
            
            maxScore = playerScores.Max();

            //Highlight the Low individual score
            if (numericUpDown3.Value == playerScores[0] && lowScorerHit == false)
            {
                lowScorerHit = true;
                decimal lowFront = numericUpDown1.Value;
                decimal lowBack = numericUpDown2.Value;
                nameOfLowScorer = twoManRTB.Lines[1].TrimStart('1', '2', '3', '4', '5', '6', '7', '8', '9', '0', ' ');

                if (!(playerCnt % 2 == 0))
                {
                    if (Form1.playerCnt == 8)
                    {
                        //Move the ODD MAN's Scores into the LowMan's position
                        numericUpDown1.Value = numericUpDown54.Value;
                        numericUpDown2.Value = numericUpDown53.Value;

                        //Move the low FRONT AND BACK scores to the TEAM 4 Player FRONT AND BACK numeric UpDn
                        numericUpDown54.Value = lowFront;
                        numericUpDown53.Value = lowBack;
                        numericUpDown52.ForeColor = Color.White;
                        numericUpDown52.BackColor = Color.Red;

                        changeLine(twoManRTB, 1, nameOfLastPlayer);
                        changeLine(twoManRTB, 16, nameOfLowScorer);
                        MessageBox.Show(nameOfLowScorer + " with a score of " + numericUpDown52.Value.ToString() + "\nis randomly selected as the low Player", "Information", MessageBoxButtons.OK);
                    }
                    else if (Form1.playerCnt == 10)
                    {
                        //Move the ODD MAN's Scores into the LowMan's position
                        numericUpDown1.Value = numericUpDown45.Value;
                        numericUpDown2.Value = numericUpDown44.Value;

                        //Move the low FRONT AND BACK scores to the TEAM 5 Player FRONT AND BACK numeric UpDn
                        numericUpDown45.Value = lowFront;
                        numericUpDown44.Value = lowBack;
                        numericUpDown43.ForeColor = Color.White;
                        numericUpDown43.BackColor = Color.Red;

                        changeLine(twoManRTB, 1, nameOfLastPlayer);
                        changeLine(twoManRTB, 21, nameOfLowScorer);
                        MessageBox.Show(nameOfLowScorer + " with a score of " + numericUpDown43.Value.ToString() + "\nis randomly selected as the low Player", "Information", MessageBoxButtons.OK);
                    }
                    else if (Form1.playerCnt == 12)
                    {
                        //Move the ODD MAN's Scores into the LowMan's position
                        numericUpDown1.Value = numericUpDown36.Value;
                        numericUpDown2.Value = numericUpDown35.Value;

                        //Move the low FRONT AND BACK scores to the TEAM 6 Player FRONT AND BACK numeric UpDn
                        numericUpDown36.Value = lowFront;
                        numericUpDown35.Value = lowBack;
                        numericUpDown34.ForeColor = Color.White;
                        numericUpDown34.BackColor = Color.Red;

                        changeLine(twoManRTB, 1, nameOfLastPlayer);
                        changeLine(twoManRTB, 26, nameOfLowScorer);
                        MessageBox.Show(nameOfLowScorer + " with a score of " + numericUpDown34.Value.ToString() + "\nis randomly selected as the low Player", "Information", MessageBoxButtons.OK);

                    }
                    else if (Form1.playerCnt == 14)
                    {
                        //Move the ODD MAN's Scores into the LowMan's position
                        numericUpDown1.Value = numericUpDown63.Value;
                        numericUpDown2.Value = numericUpDown62.Value;

                        //Move the low FRONT AND BACK scores to the TEAM 7 Player FRONT AND BACK numeric UpDn
                        numericUpDown63.Value = lowFront;
                        numericUpDown62.Value = lowBack;
                        numericUpDown61.ForeColor = Color.White;
                        numericUpDown61.BackColor = Color.Red;

                        changeLine(twoManRTB, 1, nameOfLastPlayer);
                        changeLine(twoManRTB, 31, nameOfLowScorer);
                        MessageBox.Show(nameOfLowScorer + " with a score of " + numericUpDown61.Value.ToString() + "\nis randomly selected as the low Player", "Information", MessageBoxButtons.OK);

                    }
                }
            }
            if (numericUpDown4.Value == playerScores[0] && lowScorerHit == false)
            {
                lowScorerHit = true;
                decimal lowFront = numericUpDown6.Value;
                decimal lowBack = numericUpDown5.Value;
                nameOfLowScorer = twoManRTB.Lines[2].TrimStart('1', '2', '3', '4', '5', '6', '7', '8', '9', '0', ' ');

                if (!(playerCnt % 2 == 0))
                {
                    if (Form1.playerCnt == 8)
                    {
                        //Move the ODD MAN's Scores into the LowMan's position
                        numericUpDown6.Value = numericUpDown54.Value;
                        numericUpDown5.Value = numericUpDown53.Value;

                        //Move the low FRONT AND BACK scores to the TEAM 4 Player FRONT AND BACK numeric UpDn
                        numericUpDown54.Value = lowFront;
                        numericUpDown53.Value = lowBack;
                        numericUpDown52.ForeColor = Color.White;
                        numericUpDown52.BackColor = Color.Red;

                        changeLine(twoManRTB, 2, nameOfLastPlayer);
                        changeLine(twoManRTB, 16, nameOfLowScorer);
                        MessageBox.Show(nameOfLowScorer + " with a score of " + numericUpDown52.Value.ToString() + "\nis randomly selected as the low Player", "Information", MessageBoxButtons.OK);
                    }
                    else if (Form1.playerCnt == 10)
                    {
                        //Move the ODD MAN's Scores into the LowMan's position
                        numericUpDown6.Value = numericUpDown45.Value;
                        numericUpDown5.Value = numericUpDown44.Value;

                        //Move the low FRONT AND BACK scores to the TEAM 5 Player FRONT AND BACK numeric UpDn
                        numericUpDown45.Value = lowFront;
                        numericUpDown44.Value = lowBack;
                        numericUpDown43.ForeColor = Color.White;
                        numericUpDown43.BackColor = Color.Red;

                        changeLine(twoManRTB, 2, nameOfLastPlayer);
                        changeLine(twoManRTB, 21, nameOfLowScorer);
                        MessageBox.Show(nameOfLowScorer + " with a score of " + numericUpDown43.Value.ToString() + "\nis randomly selected as the low Player", "Information", MessageBoxButtons.OK);
                    }
                    else if (Form1.playerCnt == 12)
                    {
                        //Move the ODD MAN's Scores into the LowMan's position
                        numericUpDown6.Value = numericUpDown36.Value;
                        numericUpDown5.Value = numericUpDown35.Value;

                        //Move the low FRONT AND BACK scores to the TEAM 6 Player FRONT AND BACK numeric UpDn
                        numericUpDown36.Value = lowFront;
                        numericUpDown35.Value = lowBack;
                        numericUpDown34.ForeColor = Color.White;
                        numericUpDown34.BackColor = Color.Red;

                        changeLine(twoManRTB, 2, nameOfLastPlayer);
                        changeLine(twoManRTB, 26, nameOfLowScorer);
                        MessageBox.Show(nameOfLowScorer + " with a score of " + numericUpDown34.Value.ToString() + "\nis randomly selected as the low Player", "Information", MessageBoxButtons.OK);
                    }
                    else if (Form1.playerCnt == 14)
                    {
                        //Move the ODD MAN's Scores into the LowMan's position
                        numericUpDown6.Value = numericUpDown63.Value;
                        numericUpDown5.Value = numericUpDown62.Value;

                        //Move the low FRONT AND BACK scores to the TEAM 7 Player FRONT AND BACK numeric UpDn
                        numericUpDown63.Value = lowFront;
                        numericUpDown62.Value = lowBack;
                        numericUpDown61.ForeColor = Color.White;
                        numericUpDown61.BackColor = Color.Red;

                        changeLine(twoManRTB, 2, nameOfLastPlayer);
                        changeLine(twoManRTB, 31, nameOfLowScorer);
                        MessageBox.Show(nameOfLowScorer + " with a score of " + numericUpDown61.Value.ToString() + "\nis randomly selected as the low Player", "Information", MessageBoxButtons.OK);
                    }
                }
            }/*
            if (numericUpDown7.Value == playerScores[0])
            {
                TEAM 1 ^^^^^^^^^^^^^^^^^^^^^
                numericUpDown7.ForeColor = Color.White;
                numericUpDown7.BackColor = Color.Red;
            }*/
            if (numericUpDown16.Value == playerScores[0] && lowScorerHit == false)
            {
                lowScorerHit = true;
                //Change these lines to be the NumericUpDn's found on the same line as the one in the IF STATEMENT
                decimal lowFront = numericUpDown18.Value;
                decimal lowBack = numericUpDown17.Value;

                //Move the ODD MAN's Scores into the LowMan's position
                //Change this line to be the RTB line that equates with the NumericUpDnXX found in the IF STATEMENT
                nameOfLowScorer = twoManRTB.Lines[6].TrimStart('1', '2', '3', '4', '5', '6', '7', '8', '9', '0', ' ');

                if (!(playerCnt % 2 == 0))
                {
                    if (Form1.playerCnt == 8)
                    {                        
                        //Current numericUpDn(FRONT) that is on the same line as the NumericUpDn(TOTAL) on the same line found in the 1st IF STATEMENT
                        numericUpDown18.Value = numericUpDown54.Value;
                        //Current numericUpDn(BACK) that is on the same line as the NumericUpDn(TOTAL) on the same line found in the 1st IF STATEMENT
                        numericUpDown17.Value = numericUpDown53.Value;

                        //Move the low FRONT AND BACK scores to the TEAM 4 Player FRONT AND BACK numeric UpDn
                        numericUpDown54.Value = lowFront;
                        numericUpDown53.Value = lowBack;
                        numericUpDown52.ForeColor = Color.White;
                        numericUpDown52.BackColor = Color.Red;

                        //Change this line to be the RTB line that equates with the NumericUpDnXX found in the IF STATEMENT
                        changeLine(twoManRTB, 6, nameOfLastPlayer);
                        // This number equates with the RTB Line of the last player of Team
                        changeLine(twoManRTB, 16, nameOfLowScorer);
                        //NumericUpDn is the NumericUpDn(TOTAL) of the NumericUpDn found in the IF STATEMENT for TEAM 4
                        MessageBox.Show(nameOfLowScorer + " with a score of " + numericUpDown52.Value.ToString() + "\nis randomly selected as the low Player", "Information", MessageBoxButtons.OK);
                    }
                    else if (Form1.playerCnt == 10)
                    {
                        //Current numericUpDn(FRONT) that is on the same line as the NumericUpDn(TOTAL) on the same line found in the 1st IF STATEMENT
                        numericUpDown18.Value = numericUpDown45.Value;
                        //Current numericUpDn(BACK) that is on the same line as the NumericUpDn(TOTAL) on the same line found in the 1st IF STATEMENT
                        numericUpDown17.Value = numericUpDown44.Value;

                        //Move the low FRONT AND BACK scores to the TEAM 5 Player FRONT AND BACK numeric UpDn
                        numericUpDown45.Value = lowFront;
                        numericUpDown44.Value = lowBack;
                        numericUpDown43.ForeColor = Color.White;
                        numericUpDown43.BackColor = Color.Red;

                        //Change this line to be the RTB line that equates with the NumericUpDnXX found in the IF STATEMENT
                        changeLine(twoManRTB, 6, nameOfLastPlayer);
                        //Current numericUpDn(BACK) that is on the same line as the NumericUpDn(TOTAL) on the same line found in the 1st IF STATEMENT for TEAM 5
                        changeLine(twoManRTB, 21, nameOfLowScorer);
                        //NumericUpDn is the NumericUpDn(TOTAL) of the NumericUpDn found in the IF STATEMENT for TEAM 5
                        MessageBox.Show(nameOfLowScorer + " with a score of " + numericUpDown43.Value.ToString() + "\nis randomly selected as the low Player", "Information", MessageBoxButtons.OK);
                    }
                    else if (Form1.playerCnt == 12)
                    {
                        //Current numericUpDn(FRONT) that is on the same line as the NumericUpDn(TOTAL) on the same line found in the 1st IF STATEMENT
                        numericUpDown18.Value = numericUpDown36.Value;
                        //Current numericUpDn(BACK) that is on the same line as the NumericUpDn(TOTAL) on the same line found in the 1st IF STATEMENT
                        numericUpDown17.Value = numericUpDown35.Value;

                        //Move the low FRONT AND BACK scores to the TEAM 6 Player FRONT AND BACK numeric UpDn
                        numericUpDown36.Value = lowFront;
                        numericUpDown35.Value = lowBack;
                        numericUpDown34.ForeColor = Color.White;
                        numericUpDown34.BackColor = Color.Red;

                        //Change this line to be the RTB line that equates with the NumericUpDnXX found in the IF STATEMENT
                        changeLine(twoManRTB, 6, nameOfLastPlayer);
                        //Current numericUpDn(BACK) that is on the same line as the NumericUpDn(TOTAL) on the same line found in the 1st IF STATEMENT for TEAM 6
                        changeLine(twoManRTB, 26, nameOfLowScorer);
                        //NumericUpDn is the NumericUpDn(TOTAL) of the NumericUpDn found in the IF STATEMENT for TEAM 6
                        MessageBox.Show(nameOfLowScorer + " with a score of " + numericUpDown34.Value.ToString() + "\nis randomly selected as the low Player", "Information", MessageBoxButtons.OK);
                    }
                    else if (Form1.playerCnt == 14)
                    {
                        //Current numericUpDn(FRONT) that is on the same line as the NumericUpDn(TOTAL) on the same line found in the 1st IF STATEMENT
                        numericUpDown18.Value = numericUpDown62.Value;
                        //Current numericUpDn(BACK) that is on the same line as the NumericUpDn(TOTAL) on the same line found in the 1st IF STATEMENT
                        numericUpDown17.Value = numericUpDown62.Value;

                        //Move the low FRONT AND BACK scores to the TEAM 7 Player FRONT AND BACK numeric UpDn
                        numericUpDown63.Value = lowFront;
                        numericUpDown62.Value = lowBack;
                        numericUpDown61.ForeColor = Color.White;
                        numericUpDown61.BackColor = Color.Red;

                        //Change this line to be the RTB line that equates with the NumericUpDn found in the IF STATEMENT
                        changeLine(twoManRTB, 6, nameOfLastPlayer);
                        //Current numericUpDn(BACK) that is on the same line as the NumericUpDn(TOTAL) on the same line found in the 1st IF STATEMENT for TEAM 7
                        changeLine(twoManRTB, 31, nameOfLowScorer);
                        //NumericUpDn is the NumericUpDn(TOTAL) of the NumericUpDn found in the IF STATEMENT for TEAM 7
                        MessageBox.Show(nameOfLowScorer + " with a score of " + numericUpDown61.Value.ToString() + "\nis randomly selected as the low Player", "Information", MessageBoxButtons.OK);
                    }
                }
            }
            if (numericUpDown13.Value == playerScores[0] && lowScorerHit == false)
            {
                lowScorerHit = true;
                //Change these lines to be the NumericUpDn's found on the same line as the one in the IF STATEMENT
                decimal lowFront = numericUpDown15.Value;
                decimal lowBack = numericUpDown14.Value;
                //Change this line to be the RTB line that equates with the NumericUpDnXX found in the IF STATEMENT
                nameOfLowScorer = twoManRTB.Lines[7].TrimStart('1', '2', '3', '4', '5', '6', '7', '8', '9', '0', ' ');

                if (!(playerCnt % 2 == 0))
                {
                    if (Form1.playerCnt == 8)
                    {
                        //Change numericUpDn(FRONT) to be the one associated with the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown15.Value = numericUpDown54.Value;
                        //Change numericUpDn(BACK) to be the one associated with the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown14.Value = numericUpDown53.Value;

                        //Move the low FRONT AND BACK scores to the TEAM X Player FRONT AND BACK numeric UpDn
                        numericUpDown54.Value = lowFront;
                        numericUpDown53.Value = lowBack;
                        numericUpDown52.ForeColor = Color.White;
                        numericUpDown52.BackColor = Color.Red;

                        //Change this line to be the RTB line that equates with the NumericUpDn found in the IF STATEMENT
                        changeLine(twoManRTB, 7, nameOfLastPlayer);
                        // This number equates with the RTB Line of the last player of Team 4
                        changeLine(twoManRTB, 16, nameOfLowScorer);
                        //NumericUpDn is the NumericUpDn(TOTAL) of the NumericUpDn found in the IF STATEMENT for TEAM 4
                        MessageBox.Show(nameOfLowScorer + " with a score of " + numericUpDown52.Value.ToString() + "\nis randomly selected as the low Player", "Information", MessageBoxButtons.OK);
                    }
                    else if (Form1.playerCnt == 10)
                    {
                        //Current numericUpDn(FRONT) that is on the same line as the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown15.Value = numericUpDown45.Value;
                        //Current numericUpDn(BACK) that is on the same line as the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown14.Value = numericUpDown44.Value;

                        //Move the low FRONT AND BACK scores to the TEAM X Player FRONT AND BACK numeric UpDn
                        numericUpDown45.Value = lowFront;
                        numericUpDown44.Value = lowBack;
                        numericUpDown43.ForeColor = Color.White;
                        numericUpDown43.BackColor = Color.Red;

                        //Change this line to be the RTB line that equates with the NumericUpDnXX found in the IF STATEMENT
                        changeLine(twoManRTB, 7, nameOfLastPlayer);
                        //Current numericUpDn(BACK) that is on the same line as the NumericUpDn(TOTAL) on the same line found in the 1st IF STATEMENT for TEAM 5
                        changeLine(twoManRTB, 21, nameOfLowScorer);
                        //NumericUpDn is the NumericUpDn(TOTAL) of the NumericUpDn found in the IF STATEMENT for TEAM 5
                        MessageBox.Show(nameOfLowScorer + " with a score of " + numericUpDown43.Value.ToString() + "\nis randomly selected as the low Player", "Information", MessageBoxButtons.OK);
                    }
                    else if (Form1.playerCnt == 12)
                    {
                        //Current numericUpDn(FRONT) that is on the same line as the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown15.Value = numericUpDown36.Value;
                        //Current numericUpDn(BACK) that is on the same line as the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown14.Value = numericUpDown35.Value;

                        //Move the low FRONT AND BACK scores to the TEAM X Player FRONT AND BACK numeric UpDn
                        numericUpDown36.Value = lowFront;
                        numericUpDown35.Value = lowBack;
                        numericUpDown34.ForeColor = Color.White;
                        numericUpDown34.BackColor = Color.Red;

                        //Change this line to be the RTB line that equates with the NumericUpDnXX found in the IF STATEMENT
                        changeLine(twoManRTB, 7, nameOfLastPlayer);
                        //Current numericUpDn(BACK) that is on the same line as the NumericUpDn(TOTAL) on the same line found in the 1st IF STATEMENT for TEAM 6
                        changeLine(twoManRTB, 26, nameOfLowScorer);
                        //NumericUpDn is the NumericUpDn(TOTAL) of the NumericUpDn found in the IF STATEMENT for TEAM 6
                        MessageBox.Show(nameOfLowScorer + " with a score of " + numericUpDown34.Value.ToString() + "\nis randomly selected as the low Player", "Information", MessageBoxButtons.OK);
                    }
                    else if (Form1.playerCnt == 14)
                    {
                        //Current numericUpDn(FRONT) that is on the same line as the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown15.Value = numericUpDown63.Value;
                        //Current numericUpDn(BACK) that is on the same line as the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown14.Value = numericUpDown62.Value;

                        //Move the low FRONT AND BACK scores to the TEAM X Player FRONT AND BACK numeric UpDn
                        numericUpDown63.Value = lowFront;
                        numericUpDown62.Value = lowBack;
                        numericUpDown61.ForeColor = Color.White;
                        numericUpDown61.BackColor = Color.Red;

                        //Change this line to be the RTB line that equates with the NumericUpDn found in the IF STATEMENT
                        changeLine(twoManRTB, 7, nameOfLastPlayer);
                        //Current numericUpDn(BACK) that is on the same line as the NumericUpDn(TOTAL) on the same line found in the 1st IF STATEMENT for TEAM 7
                        changeLine(twoManRTB, 31, nameOfLowScorer);
                        //NumericUpDn is the NumericUpDn(TOTAL) of the NumericUpDn found in the IF STATEMENT for TEAM 7
                        MessageBox.Show(nameOfLowScorer + " with a score of " + numericUpDown61.Value.ToString() + "\nis randomly selected as the low Player", "Information", MessageBoxButtons.OK);
                    }
                }
            }/*
            if (numericUpDown10.Value == playerScores[0])
            {
                TEAM 2 ^^^^^^^^^^^^^^^^^^^^^
                numericUpDown10.ForeColor = Color.White;
                numericUpDown10.BackColor = Color.Red;
            }*/
            if (numericUpDown25.Value == playerScores[0] && lowScorerHit == false)
            {
                lowScorerHit = true;
                //Change these lines to be the NumericUpDn's found on the same line as the one in the IF STATEMENT
                decimal lowFront = numericUpDown27.Value;
                decimal lowBack = numericUpDown26.Value;
                //Change this line to be the RTB line that equates with the NumericUpDnXX found in the IF STATEMENT
                nameOfLowScorer = twoManRTB.Lines[11].TrimStart('1', '2', '3', '4', '5', '6', '7', '8', '9', '0', ' ');

                if (!(playerCnt % 2 == 0))
                {
                    if (Form1.playerCnt == 8)
                    {
                        //Move the ODD MAN's Scores into the LowMan's position
                        //Change numericUpDn(FRONT) to be the one associated with the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown27.Value = numericUpDown54.Value;
                        //Change numericUpDn(BACK) to be the one associated with the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown26.Value = numericUpDown53.Value;

                        //Move the low FRONT AND BACK scores to the TEAM X Player FRONT AND BACK numeric UpDn
                        numericUpDown54.Value = lowFront;
                        numericUpDown53.Value = lowBack;
                        numericUpDown52.ForeColor = Color.White;
                        numericUpDown52.BackColor = Color.Red;

                        //Change this line to be the RTB line that equates with the NumericUpDn found in the IF STATEMENT
                        changeLine(twoManRTB, 11, nameOfLastPlayer);
                        // This number equates with the RTB Line of the last player of Team 4
                        changeLine(twoManRTB, 16, nameOfLowScorer);
                        //NumericUpDn is the NumericUpDn(TOTAL) of the NumericUpDn found in the IF STATEMENT for TEAM 4
                        MessageBox.Show(nameOfLowScorer + " with a score of " + numericUpDown52.Value.ToString() + "\nis randomly selected as the low Player", "Information", MessageBoxButtons.OK);
                    }
                    else if (Form1.playerCnt == 10)
                    {
                        //Current numericUpDn(FRONT) that is on the same line as the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown27.Value = numericUpDown45.Value;
                        //Current numericUpDn(BACK) that is on the same line as the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown26.Value = numericUpDown44.Value;

                        //Move the low FRONT AND BACK scores to the TEAM X Player FRONT AND BACK numeric UpDn
                        numericUpDown45.Value = lowFront;
                        numericUpDown44.Value = lowBack;
                        numericUpDown43.ForeColor = Color.White;
                        numericUpDown43.BackColor = Color.Red;

                        //Change this line to be the RTB line that equates with the NumericUpDnXX found in the IF STATEMENT
                        changeLine(twoManRTB, 11, nameOfLastPlayer);
                        //Current numericUpDn(BACK) that is on the same line as the NumericUpDn(TOTAL) on the same line found in the 1st IF STATEMENT for TEAM 5
                        changeLine(twoManRTB, 21, nameOfLowScorer);
                        //NumericUpDn is the NumericUpDn(TOTAL) of the NumericUpDn found in the IF STATEMENT for TEAM 5
                        MessageBox.Show(nameOfLowScorer + " with a score of " + numericUpDown43.Value.ToString() + "\nis randomly selected as the low Player", "Information", MessageBoxButtons.OK);
                    }
                    else if (Form1.playerCnt == 12)
                    {
                        //Current numericUpDn(FRONT) that is on the same line as the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown27.Value = numericUpDown36.Value;
                        //Current numericUpDn(BACK) that is on the same line as the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown26.Value = numericUpDown35.Value;

                        //Move the low FRONT AND BACK scores to the TEAM X Player FRONT AND BACK numeric UpDn
                        numericUpDown36.Value = lowFront;
                        numericUpDown35.Value = lowBack;
                        numericUpDown34.ForeColor = Color.White;
                        numericUpDown34.BackColor = Color.Red;

                        //Change this line to be the RTB line that equates with the NumericUpDnXX found in the IF STATEMENT
                        changeLine(twoManRTB, 11, nameOfLastPlayer);
                        //Current numericUpDn(BACK) that is on the same line as the NumericUpDn(TOTAL) on the same line found in the 1st IF STATEMENT for TEAM 6
                        changeLine(twoManRTB, 26, nameOfLowScorer);
                        //NumericUpDn is the NumericUpDn(TOTAL) of the NumericUpDn found in the IF STATEMENT for TEAM 6
                        MessageBox.Show(nameOfLowScorer + " with a score of " + numericUpDown34.Value.ToString() + "\nis randomly selected as the low Player", "Information", MessageBoxButtons.OK);
                    }
                    else if (Form1.playerCnt == 14)
                    {
                        //Current numericUpDn(FRONT) that is on the same line as the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown27.Value = numericUpDown63.Value;
                        //Current numericUpDn(BACK) that is on the same line as the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown26.Value = numericUpDown62.Value;

                        //Move the low FRONT AND BACK scores to the TEAM X Player FRONT AND BACK numeric UpDn
                        numericUpDown63.Value = lowFront;
                        numericUpDown62.Value = lowBack;
                        numericUpDown61.ForeColor = Color.White;
                        numericUpDown61.BackColor = Color.Red;

                        //Change this line to be the RTB line that equates with the NumericUpDn found in the IF STATEMENT
                        changeLine(twoManRTB, 11, nameOfLastPlayer);
                        //Current numericUpDn(BACK) that is on the same line as the NumericUpDn(TOTAL) on the same line found in the 1st IF STATEMENT for TEAM 7
                        changeLine(twoManRTB, 31, nameOfLowScorer);
                        //NumericUpDn is the NumericUpDn(TOTAL) of the NumericUpDn found in the IF STATEMENT for TEAM 7
                        MessageBox.Show(nameOfLowScorer + " with a score of " + numericUpDown61.Value.ToString() + "\nis randomly selected as the low Player", "Information", MessageBoxButtons.OK);
                    }
                }
            }
            if (numericUpDown22.Value == playerScores[0] && lowScorerHit == false)
            {
                lowScorerHit = true;
                //Change these lines to be the NumericUpDn's found on the same line as the one in the IF STATEMENT
                decimal lowFront = numericUpDown24.Value;
                decimal lowBack = numericUpDown23.Value;
                //Change this line to be the RTB line that equates with the NumericUpDnXX found in the IF STATEMENT
                nameOfLowScorer = twoManRTB.Lines[12].TrimStart('1', '2', '3', '4', '5', '6', '7', '8', '9', '0', ' ');

                if (!(playerCnt % 2 == 0))
                {
                    if (Form1.playerCnt == 8)
                    {
                        //Move the ODD MAN's Scores into the LowMan's position
                        //Change numericUpDn(FRONT) to be the one associated with the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown24.Value = numericUpDown54.Value;
                        //Change numericUpDn(BACK) to be the one associated with the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown23.Value = numericUpDown53.Value;

                        //Move the low FRONT AND BACK scores to the TEAM X Player FRONT AND BACK numeric UpDn
                        numericUpDown54.Value = lowFront;
                        numericUpDown53.Value = lowBack;
                        numericUpDown52.ForeColor = Color.White;
                        numericUpDown52.BackColor = Color.Red;

                        //Change this line to be the RTB line that equates with the NumericUpDn found in the IF STATEMENT
                        changeLine(twoManRTB, 12, nameOfLastPlayer);
                        // This number equates with the RTB Line of the last player of Team 4
                        changeLine(twoManRTB, 16, nameOfLowScorer);
                        //NumericUpDn is the NumericUpDn(TOTAL) of the NumericUpDn found in the IF STATEMENT for TEAM 4
                        MessageBox.Show(nameOfLowScorer + " with a score of " + numericUpDown52.Value.ToString() + "\nis randomly selected as the low Player", "Information", MessageBoxButtons.OK);
                    }
                    else if (Form1.playerCnt == 10)
                    {
                        //Current numericUpDn(FRONT) that is on the same line as the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown24.Value = numericUpDown45.Value;
                        //Current numericUpDn(BACK) that is on the same line as the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown23.Value = numericUpDown44.Value;

                        //Move the low FRONT AND BACK scores to the TEAM X Player FRONT AND BACK numeric UpDn
                        numericUpDown45.Value = lowFront;
                        numericUpDown44.Value = lowBack;
                        numericUpDown43.ForeColor = Color.White;
                        numericUpDown43.BackColor = Color.Red;

                        //Change this line to be the RTB line that equates with the NumericUpDnXX found in the IF STATEMENT
                        changeLine(twoManRTB, 12, nameOfLastPlayer);
                        //Current numericUpDn(BACK) that is on the same line as the NumericUpDn(TOTAL) on the same line found in the 1st IF STATEMENT for TEAM 5
                        changeLine(twoManRTB, 21, nameOfLowScorer);
                        //NumericUpDn is the NumericUpDn(TOTAL) of the NumericUpDn found in the IF STATEMENT for TEAM 5
                        MessageBox.Show(nameOfLowScorer + " with a score of " + numericUpDown43.Value.ToString() + "\nis randomly selected as the low Player", "Information", MessageBoxButtons.OK);
                    }
                    else if (Form1.playerCnt == 12)
                    {
                        //Current numericUpDn(FRONT) that is on the same line as the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown24.Value = numericUpDown36.Value;
                        //Current numericUpDn(BACK) that is on the same line as the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown23.Value = numericUpDown35.Value;

                        //Move the low FRONT AND BACK scores to the TEAM X Player FRONT AND BACK numeric UpDn
                        numericUpDown36.Value = lowFront;
                        numericUpDown35.Value = lowBack;
                        numericUpDown34.ForeColor = Color.White;
                        numericUpDown34.BackColor = Color.Red;

                        //Change this line to be the RTB line that equates with the NumericUpDnXX found in the IF STATEMENT
                        changeLine(twoManRTB, 12, nameOfLastPlayer);
                        //Current numericUpDn(BACK) that is on the same line as the NumericUpDn(TOTAL) on the same line found in the 1st IF STATEMENT for TEAM 6
                        changeLine(twoManRTB, 26, nameOfLowScorer);
                        //NumericUpDn is the NumericUpDn(TOTAL) of the NumericUpDn found in the IF STATEMENT for TEAM 6
                        MessageBox.Show(nameOfLowScorer + " with a score of " + numericUpDown34.Value.ToString() + "\nis randomly selected as the low Player", "Information", MessageBoxButtons.OK);
                    }
                    else if (Form1.playerCnt == 14)
                    {
                        //Current numericUpDn(FRONT) that is on the same line as the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown24.Value = numericUpDown63.Value;
                        //Current numericUpDn(BACK) that is on the same line as the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown23.Value = numericUpDown62.Value;

                        //Move the low FRONT AND BACK scores to the TEAM X Player FRONT AND BACK numeric UpDn
                        numericUpDown63.Value = lowFront;
                        numericUpDown62.Value = lowBack;
                        numericUpDown61.ForeColor = Color.White;
                        numericUpDown61.BackColor = Color.Red;

                        //Change this line to be the RTB line that equates with the NumericUpDn found in the IF STATEMENT
                        changeLine(twoManRTB, 12, nameOfLastPlayer);
                        //Current numericUpDn(BACK) that is on the same line as the NumericUpDn(TOTAL) on the same line found in the 1st IF STATEMENT for TEAM 7
                        changeLine(twoManRTB, 31, nameOfLowScorer);
                        //NumericUpDn is the NumericUpDn(TOTAL) of the NumericUpDn found in the IF STATEMENT for TEAM 7
                        MessageBox.Show(nameOfLowScorer + " with a score of " + numericUpDown61.Value.ToString() + "\nis randomly selected as the low Player", "Information", MessageBoxButtons.OK);
                    }
                }
            }/*
            if (numericUpDown19.Value == playerScores[0])
            {
                TEAM 3 ^^^^^^^^^^^^^^^^^^^^^
                numericUpDown19.ForeColor = Color.White;
                numericUpDown19.BackColor = Color.Red;
            }*/
            if (numericUpDown52.Value == playerScores[0] && lowScorerHit == false)
            {
                lowScorerHit = true;
                //Change these lines to be the NumericUpDn's found on the same line as the one in the IF STATEMENT
                decimal lowFront = numericUpDown54.Value;
                decimal lowBack = numericUpDown53.Value;
                //Change this line to be the RTB line that equates with the NumericUpDnXX found in the IF STATEMENT
                nameOfLowScorer = twoManRTB.Lines[16].TrimStart('1', '2', '3', '4', '5', '6', '7', '8', '9', '0', ' ');

                if (!(playerCnt % 2 == 0))
                {
                    if (Form1.playerCnt == 8)
                    {
                        //Move the ODD MAN's Scores into the LowMan's position
                        //Change numericUpDn(FRONT) to be the one associated with the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown54.Value = numericUpDown54.Value;
                        //Change numericUpDn(BACK) to be the one associated with the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown53.Value = numericUpDown53.Value;

                        //Move the low FRONT AND BACK scores to the TEAM X Player FRONT AND BACK numeric UpDn
                        numericUpDown54.Value = lowFront;
                        numericUpDown53.Value = lowBack;
                        numericUpDown52.ForeColor = Color.White;
                        numericUpDown52.BackColor = Color.Red;

                        //Change this line to be the RTB line that equates with the NumericUpDn found in the IF STATEMENT
                        changeLine(twoManRTB, 16, nameOfLastPlayer);
                        // This number equates with the RTB Line of the last player of Team 4
                        changeLine(twoManRTB, 16, nameOfLowScorer);
                        //NumericUpDn is the NumericUpDn(TOTAL) of the NumericUpDn found in the IF STATEMENT for TEAM 4
                        MessageBox.Show(nameOfLowScorer + " with a score of " + numericUpDown52.Value.ToString() + "\nis randomly selected as the low Player", "Information", MessageBoxButtons.OK);
                    }
                    else if (Form1.playerCnt == 10)
                    {
                        //Change numericUpDn(FRONT) to be the one associated with the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown54.Value = numericUpDown45.Value;
                        //Change numericUpDn(BACK) to be the one associated with the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown53.Value = numericUpDown44.Value;

                        //Move the low FRONT AND BACK scores to the TEAM X Player FRONT AND BACK numeric UpDn
                        numericUpDown45.Value = lowFront;
                        numericUpDown44.Value = lowBack;
                        numericUpDown43.ForeColor = Color.White;
                        numericUpDown43.BackColor = Color.Red;

                        //Change this line to be the RTB line that equates with the NumericUpDnXX found in the IF STATEMENT
                        changeLine(twoManRTB, 16, nameOfLastPlayer);
                        //Current numericUpDn(BACK) that is on the same line as the NumericUpDn(TOTAL) on the same line found in the 1st IF STATEMENT for TEAM 5
                        changeLine(twoManRTB, 21, nameOfLowScorer);
                        //NumericUpDn is the NumericUpDn(TOTAL) of the NumericUpDn found in the IF STATEMENT for TEAM 5
                        MessageBox.Show(nameOfLowScorer + " with a score of " + numericUpDown43.Value.ToString() + "\nis randomly selected as the low Player", "Information", MessageBoxButtons.OK);
                    }
                    else if (Form1.playerCnt == 12)
                    {
                        //Change numericUpDn(FRONT) to be the one associated with the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown54.Value = numericUpDown36.Value;
                        //Change numericUpDn(BACK) to be the one associated with the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown53.Value = numericUpDown37.Value;

                        //Move the low FRONT AND BACK scores to the TEAM X Player FRONT AND BACK numeric UpDn
                        numericUpDown36.Value = lowFront;
                        numericUpDown35.Value = lowBack;
                        numericUpDown34.ForeColor = Color.White;
                        numericUpDown34.BackColor = Color.Red;

                        //Change this line to be the RTB line that equates with the NumericUpDnXX found in the IF STATEMENT
                        changeLine(twoManRTB, 16, nameOfLastPlayer);
                        //Current numericUpDn(BACK) that is on the same line as the NumericUpDn(TOTAL) on the same line found in the 1st IF STATEMENT for TEAM 6
                        changeLine(twoManRTB, 26, nameOfLowScorer);
                        //NumericUpDn is the NumericUpDn(TOTAL) of the NumericUpDn found in the IF STATEMENT for TEAM 6
                        MessageBox.Show(nameOfLowScorer + " with a score of " + numericUpDown34.Value.ToString() + "\nis randomly selected as the low Player", "Information", MessageBoxButtons.OK);
                    }
                    else if (Form1.playerCnt == 14)
                    {
                        //Change numericUpDn(FRONT) to be the one associated with the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown54.Value = numericUpDown63.Value;
                        //Change numericUpDn(BACK) to be the one associated with the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown53.Value = numericUpDown62.Value;

                        //Move the low FRONT AND BACK scores to the TEAM X Player FRONT AND BACK numeric UpDn
                        numericUpDown63.Value = lowFront;
                        numericUpDown62.Value = lowBack;
                        numericUpDown61.ForeColor = Color.White;
                        numericUpDown61.BackColor = Color.Red;

                        //Change this line to be the RTB line that equates with the NumericUpDn found in the IF STATEMENT
                        changeLine(twoManRTB, 16, nameOfLastPlayer);
                        //Current numericUpDn(BACK) that is on the same line as the NumericUpDn(TOTAL) on the same line found in the 1st IF STATEMENT for TEAM 7
                        changeLine(twoManRTB, 31, nameOfLowScorer);
                        //NumericUpDn is the NumericUpDn(TOTAL) of the NumericUpDn found in the IF STATEMENT for TEAM 7
                        MessageBox.Show(nameOfLowScorer + " with a score of " + numericUpDown61.Value.ToString() + "\nis randomly selected as the low Player", "Information", MessageBoxButtons.OK);
                    }
                }
            }
            if (numericUpDown49.Value == playerScores[0] && lowScorerHit == false)
            {
                lowScorerHit = true;
                //Change these lines to be the NumericUpDn's found on the same line as the one in the IF STATEMENT
                decimal lowFront = numericUpDown51.Value;
                decimal lowBack = numericUpDown50.Value;
                //Change this line to be the RTB line that equates with the NumericUpDnXX found in the IF STATEMENT
                nameOfLowScorer = twoManRTB.Lines[17].TrimStart('1', '2', '3', '4', '5', '6', '7', '8', '9', '0', ' ');

                if (!(playerCnt % 2 == 0))
                {
                    if (Form1.playerCnt == 10)
                    {
                        //Change numericUpDn(FRONT) to be the one associated with the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown51.Value = numericUpDown45.Value;
                        //Change numericUpDn(BACK) to be the one associated with the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown50.Value = numericUpDown44.Value;

                        //Move the low FRONT AND BACK scores to the TEAM X Player FRONT AND BACK numeric UpDn
                        numericUpDown45.Value = lowFront;
                        numericUpDown44.Value = lowBack;
                        numericUpDown43.ForeColor = Color.White;
                        numericUpDown43.BackColor = Color.Red;

                        //Change this line to be the RTB line that equates with the NumericUpDnXX found in the IF STATEMENT
                        changeLine(twoManRTB, 17, nameOfLastPlayer);
                        //Current numericUpDn(BACK) that is on the same line as the NumericUpDn(TOTAL) on the same line found in the 1st IF STATEMENT for TEAM 5
                        changeLine(twoManRTB, 21, nameOfLowScorer);
                        //NumericUpDn is the NumericUpDn(TOTAL) of the NumericUpDn found in the IF STATEMENT for TEAM 5
                        MessageBox.Show(nameOfLowScorer + " with a score of " + numericUpDown43.Value.ToString() + "\nis randomly selected as the low Player", "Information", MessageBoxButtons.OK);
                    }
                    else if (Form1.playerCnt == 12)
                    {
                        //Change numericUpDn(FRONT) to be the one associated with the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown51.Value = numericUpDown36.Value;
                        //Change numericUpDn(BACK) to be the one associated with the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown50.Value = numericUpDown35.Value;

                        //Move the low FRONT AND BACK scores to the TEAM X Player FRONT AND BACK numeric UpDn
                        numericUpDown36.Value = lowFront;
                        numericUpDown35.Value = lowBack;
                        numericUpDown34.ForeColor = Color.White;
                        numericUpDown34.BackColor = Color.Red;

                        //Change this line to be the RTB line that equates with the NumericUpDnXX found in the IF STATEMENT
                        changeLine(twoManRTB, 16, nameOfLastPlayer);
                        //Current numericUpDn(BACK) that is on the same line as the NumericUpDn(TOTAL) on the same line found in the 1st IF STATEMENT for TEAM 6
                        changeLine(twoManRTB, 26, nameOfLowScorer);
                        //NumericUpDn is the NumericUpDn(TOTAL) of the NumericUpDn found in the IF STATEMENT for TEAM 6
                        MessageBox.Show(nameOfLowScorer + " with a score of " + numericUpDown34.Value.ToString() + "\nis randomly selected as the low Player", "Information", MessageBoxButtons.OK);
                    }
                    else if (Form1.playerCnt == 14)
                    {
                        //Change numericUpDn(FRONT) to be the one associated with the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown51.Value = numericUpDown63.Value;
                        //Change numericUpDn(BACK) to be the one associated with the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown50.Value = numericUpDown62.Value;

                        numericUpDown63.Value = lowFront;
                        numericUpDown62.Value = lowBack;
                        numericUpDown61.ForeColor = Color.White;
                        numericUpDown61.BackColor = Color.Red;

                        //Change this line to be the RTB line that equates with the NumericUpDn found in the IF STATEMENT
                        changeLine(twoManRTB, 16, nameOfLastPlayer);
                        //Current numericUpDn(BACK) that is on the same line as the NumericUpDn(TOTAL) on the same line found in the 1st IF STATEMENT for TEAM 7
                        changeLine(twoManRTB, 31, nameOfLowScorer);
                        //NumericUpDn is the NumericUpDn(TOTAL) of the NumericUpDn found in the IF STATEMENT for TEAM 7
                        MessageBox.Show(nameOfLowScorer + " with a score of " + numericUpDown61.Value.ToString() + "\nis randomly selected as the low Player", "Information", MessageBoxButtons.OK);
                    }
                }
            }/*
            if (numericUpDown46.Value == playerScores[0])
            {
                TEAM 4 ^^^^^^^^^^^^^^^^^^^^^
                numericUpDown46.ForeColor = Color.White;
                numericUpDown46.BackColor = Color.Red;
            }*/
            if (numericUpDown43.Value == playerScores[0] && lowScorerHit == false)
            {
                lowScorerHit = true;
                //Change these lines to be the NumericUpDn's found on the same line as the one in the IF STATEMENT
                decimal lowFront = numericUpDown45.Value;
                decimal lowBack = numericUpDown44.Value;
                //Change this line to be the RTB line that equates with the NumericUpDnXX found in the IF STATEMENT
                nameOfLowScorer = twoManRTB.Lines[21].TrimStart('1', '2', '3', '4', '5', '6', '7', '8', '9', '0', ' ');

                if (!(playerCnt % 2 == 0))
                {
                    if (Form1.playerCnt == 10)
                    {
                        //Change numericUpDn(FRONT) to be the one associated with the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown45.Value = numericUpDown45.Value;
                        //Change numericUpDn(BACK) to be the one associated with the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown44.Value = numericUpDown44.Value;

                        //Move the low FRONT AND BACK scores to the TEAM X Player FRONT AND BACK numeric UpDn
                        numericUpDown45.Value = lowFront;
                        numericUpDown44.Value = lowBack;
                        numericUpDown43.ForeColor = Color.White;
                        numericUpDown43.BackColor = Color.Red;

                        //Change this line to be the RTB line that equates with the NumericUpDnXX found in the IF STATEMENT
                        changeLine(twoManRTB, 21, nameOfLastPlayer);
                        //Current numericUpDn(BACK) that is on the same line as the NumericUpDn(TOTAL) on the same line found in the 1st IF STATEMENT for TEAM 5
                        changeLine(twoManRTB, 21, nameOfLowScorer);
                        //NumericUpDn is the NumericUpDn(TOTAL) of the NumericUpDn found in the IF STATEMENT for TEAM 5
                        MessageBox.Show(nameOfLowScorer + " with a score of " + numericUpDown43.Value.ToString() + "\nis randomly selected as the low Player", "Information", MessageBoxButtons.OK);
                    }
                    else if (Form1.playerCnt == 12)
                    {
                        //Change numericUpDn(FRONT) to be the one associated with the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown45.Value = numericUpDown36.Value;
                        //Change numericUpDn(BACK) to be the one associated with the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown44.Value = numericUpDown37.Value;

                        //Move the low FRONT AND BACK scores to the TEAM X Player FRONT AND BACK numeric UpDn
                        numericUpDown36.Value = lowFront;
                        numericUpDown35.Value = lowBack;
                        numericUpDown34.ForeColor = Color.White;
                        numericUpDown34.BackColor = Color.Red;

                        //Change this line to be the RTB line that equates with the NumericUpDnXX found in the IF STATEMENT
                        changeLine(twoManRTB, 21, nameOfLastPlayer);
                        //Current numericUpDn(BACK) that is on the same line as the NumericUpDn(TOTAL) on the same line found in the 1st IF STATEMENT for TEAM 6
                        changeLine(twoManRTB, 26, nameOfLowScorer);
                        //NumericUpDn is the NumericUpDn(TOTAL) of the NumericUpDn found in the IF STATEMENT for TEAM 6
                        MessageBox.Show(nameOfLowScorer + " with a score of " + numericUpDown34.Value.ToString() + "\nis randomly selected as the low Player", "Information", MessageBoxButtons.OK);
                    }
                    else if (Form1.playerCnt == 14)
                    {
                        //Change numericUpDn(FRONT) to be the one associated with the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown45.Value = numericUpDown63.Value;
                        //Change numericUpDn(BACK) to be the one associated with the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown44.Value = numericUpDown62.Value;

                        //Move the low FRONT AND BACK scores to the TEAM X Player FRONT AND BACK numeric UpDn
                        numericUpDown63.Value = lowFront;
                        numericUpDown62.Value = lowBack;
                        numericUpDown61.ForeColor = Color.White;
                        numericUpDown61.BackColor = Color.Red;

                        //Change this line to be the RTB line that equates with the NumericUpDn found in the IF STATEMENT
                        changeLine(twoManRTB, 21, nameOfLastPlayer);
                        //Current numericUpDn(BACK) that is on the same line as the NumericUpDn(TOTAL) on the same line found in the 1st IF STATEMENT for TEAM 7
                        changeLine(twoManRTB, 31, nameOfLowScorer);
                        //NumericUpDn is the NumericUpDn(TOTAL) of the NumericUpDn found in the IF STATEMENT for TEAM 7
                        MessageBox.Show(nameOfLowScorer + " with a score of " + numericUpDown61.Value.ToString() + "\nis randomly selected as the low Player", "Information", MessageBoxButtons.OK);
                    }
                }
            }
            if (numericUpDown40.Value == playerScores[0] && lowScorerHit == false)
            {
                lowScorerHit = true;
                //Change these lines to be the NumericUpDn's found on the same line as the one in the IF STATEMENT
                decimal lowFront = numericUpDown42.Value;
                decimal lowBack = numericUpDown41.Value;
                //Change this line to be the RTB line that equates with the NumericUpDnXX found in the IF STATEMENT
                nameOfLowScorer = twoManRTB.Lines[22].TrimStart('1', '2', '3', '4', '5', '6', '7', '8', '9', '0', ' ');

                if (!(playerCnt % 2 == 0))
                {
                    if (Form1.playerCnt == 12)
                    {
                        //Change numericUpDn(FRONT) to be the one associated with the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown42.Value = numericUpDown36.Value;
                        //Change numericUpDn(BACK) to be the one associated with the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown41.Value = numericUpDown37.Value;

                        //Move the low FRONT AND BACK scores to the TEAM X Player FRONT AND BACK numeric UpDn
                        numericUpDown36.Value = lowFront;
                        numericUpDown35.Value = lowBack;
                        numericUpDown34.ForeColor = Color.White;
                        numericUpDown34.BackColor = Color.Red;

                        //Change this line to be the RTB line that equates with the NumericUpDnXX found in the IF STATEMENT
                        changeLine(twoManRTB, 22, nameOfLastPlayer);
                        //Current numericUpDn(BACK) that is on the same line as the NumericUpDn(TOTAL) on the same line found in the 1st IF STATEMENT for TEAM 6
                        changeLine(twoManRTB, 26, nameOfLowScorer);
                        //NumericUpDn is the NumericUpDn(TOTAL) of the NumericUpDn found in the IF STATEMENT for TEAM 6
                        MessageBox.Show(nameOfLowScorer + " with a score of " + numericUpDown34.Value.ToString() + "\nis randomly selected as the low Player", "Information", MessageBoxButtons.OK);
                    }
                    else if (Form1.playerCnt == 14)
                    {
                        //Change numericUpDn(FRONT) to be the one associated with the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown42.Value = numericUpDown63.Value;
                        //Change numericUpDn(BACK) to be the one associated with the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown41.Value = numericUpDown62.Value;

                        //Move the low FRONT AND BACK scores to the TEAM X Player FRONT AND BACK numeric UpDn
                        numericUpDown63.Value = lowFront;
                        numericUpDown62.Value = lowBack;
                        numericUpDown61.ForeColor = Color.White;
                        numericUpDown61.BackColor = Color.Red;

                        //Change this line to be the RTB line that equates with the NumericUpDn found in the IF STATEMENT
                        changeLine(twoManRTB, 22, nameOfLastPlayer);
                        //Current numericUpDn(BACK) that is on the same line as the NumericUpDn(TOTAL) on the same line found in the 1st IF STATEMENT for TEAM 7
                        changeLine(twoManRTB, 31, nameOfLowScorer);
                        //NumericUpDn is the NumericUpDn(TOTAL) of the NumericUpDn found in the IF STATEMENT for TEAM 7
                        MessageBox.Show(nameOfLowScorer + " with a score of " + numericUpDown61.Value.ToString() + "\nis randomly selected as the low Player", "Information", MessageBoxButtons.OK);
                    }
                }
            }/*
            if (numericUpDown37.Value == playerScores[0])
            {
                TEAM 5 ^^^^^^^^^^^^^^^^^^^^^
                numericUpDown37.ForeColor = Color.White;
                numericUpDown37.BackColor = Color.Red;
            }*/
            if (numericUpDown34.Value == playerScores[0] && lowScorerHit == false)
            {
                lowScorerHit = true;
                //Change these lines to be the NumericUpDn's found on the same line as the one in the IF STATEMENT
                decimal lowFront = numericUpDown36.Value;
                decimal lowBack = numericUpDown35.Value;
                //Change this line to be the RTB line that equates with the NumericUpDnXX found in the IF STATEMENT
                nameOfLowScorer = twoManRTB.Lines[25].TrimStart('1', '2', '3', '4', '5', '6', '7', '8', '9', '0', ' ');

                if (!(playerCnt % 2 == 0))
                {
                    if (Form1.playerCnt == 12)
                    {
                        //Change numericUpDn(FRONT) to be the one associated with the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown36.Value = numericUpDown36.Value;
                        //Change numericUpDn(BACK) to be the one associated with the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown35.Value = numericUpDown37.Value;

                        //Move the low FRONT AND BACK scores to the TEAM X Player FRONT AND BACK numeric UpDn
                        numericUpDown36.Value = lowFront;
                        numericUpDown35.Value = lowBack;
                        numericUpDown34.ForeColor = Color.White;
                        numericUpDown34.BackColor = Color.Red;

                        //Change this line to be the RTB line that equates with the NumericUpDnXX found in the IF STATEMENT
                        changeLine(twoManRTB, 26, nameOfLastPlayer);
                        //Current numericUpDn(BACK) that is on the same line as the NumericUpDn(TOTAL) on the same line found in the 1st IF STATEMENT for TEAM 6
                        changeLine(twoManRTB, 26, nameOfLowScorer);
                        //NumericUpDn is the NumericUpDn(TOTAL) of the NumericUpDn found in the IF STATEMENT for TEAM 6
                        MessageBox.Show(nameOfLowScorer + " with a score of " + numericUpDown34.Value.ToString() + "\nis randomly selected as the low Player", "Information", MessageBoxButtons.OK);
                    }
                    else if (Form1.playerCnt == 14)
                    {
                        //Change numericUpDn(FRONT) to be the one associated with the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown36.Value = numericUpDown63.Value;
                        //Change numericUpDn(BACK) to be the one associated with the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown35.Value = numericUpDown62.Value;

                        //Move the low FRONT AND BACK scores to the TEAM X Player FRONT AND BACK numeric UpDn
                        numericUpDown63.Value = lowFront;
                        numericUpDown62.Value = lowBack;
                        numericUpDown61.ForeColor = Color.White;
                        numericUpDown61.BackColor = Color.Red;

                        //Change this line to be the RTB line that equates with the NumericUpDn found in the IF STATEMENT
                        changeLine(twoManRTB, 26, nameOfLastPlayer);
                        //Current numericUpDn(BACK) that is on the same line as the NumericUpDn(TOTAL) on the same line found in the 1st IF STATEMENT for TEAM 7
                        changeLine(twoManRTB, 31, nameOfLowScorer);
                        //NumericUpDn is the NumericUpDn(TOTAL) of the NumericUpDn found in the IF STATEMENT for TEAM 7
                        MessageBox.Show(nameOfLowScorer + " with a score of " + numericUpDown61.Value.ToString() + "\nis randomly selected as the low Player", "Information", MessageBoxButtons.OK);
                    }
                }
            }
            if (numericUpDown31.Value == playerScores[0] && lowScorerHit == false)
            {
                lowScorerHit = true;
                //Change these lines to be the NumericUpDn's found on the same line as the one in the IF STATEMENT
                decimal lowFront = numericUpDown33.Value;
                decimal lowBack = numericUpDown32.Value;
                //Change this line to be the RTB line that equates with the NumericUpDnXX found in the IF STATEMENT
                nameOfLowScorer = twoManRTB.Lines[26].TrimStart('1', '2', '3', '4', '5', '6', '7', '8', '9', '0', ' ');

                if (!(playerCnt % 2 == 0))
                {
                    if (Form1.playerCnt == 14)
                    {
                        //Change numericUpDn(FRONT) to be the one associated with the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown33.Value = numericUpDown63.Value;
                        //Change numericUpDn(BACK) to be the one associated with the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown32.Value = numericUpDown62.Value;

                        //Move the low FRONT AND BACK scores to the TEAM X Player FRONT AND BACK numeric UpDn
                        numericUpDown63.Value = lowFront;
                        numericUpDown62.Value = lowBack;
                        numericUpDown61.ForeColor = Color.White;
                        numericUpDown61.BackColor = Color.Red;

                        //Change this line to be the RTB line that equates with the NumericUpDn found in the IF STATEMENT
                        changeLine(twoManRTB, 27, nameOfLastPlayer);
                        //Current numericUpDn(BACK) that is on the same line as the NumericUpDn(TOTAL) on the same line found in the 1st IF STATEMENT for TEAM 7
                        changeLine(twoManRTB, 31, nameOfLowScorer);
                        //NumericUpDn is the NumericUpDn(TOTAL) of the NumericUpDn found in the IF STATEMENT for TEAM 7
                        MessageBox.Show(nameOfLowScorer + " with a score of " + numericUpDown61.Value.ToString() + "\nis randomly selected as the low Player", "Information", MessageBoxButtons.OK);
                    }
                }
            }/*
            if (numericUpDown28.Value == playerScores[0])
            {
                TEAM 6 ^^^^^^^^^^^^^^^^^^^^^
                numericUpDown28.ForeColor = Color.White;
                numericUpDown28.BackColor = Color.Red;
            }*/
            if (numericUpDown61.Value == playerScores[0] && lowScorerHit == false)
            {
                lowScorerHit = true;
                //Change these lines to be the NumericUpDn's found on the same line as the one in the IF STATEMENT
                decimal lowFront = numericUpDown63.Value;
                decimal lowBack = numericUpDown62.Value;
                //Change this line to be the RTB line that equates with the NumericUpDnXX found in the IF STATEMENT
                nameOfLowScorer = twoManRTB.Lines[30].TrimStart('1', '2', '3', '4', '5', '6', '7', '8', '9', '0', ' '); 
                
                if (!(playerCnt % 2 == 0))
                {
                    if (Form1.playerCnt == 14)
                    {
                        //Change numericUpDn(FRONT) to be the one associated with the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown63.Value = numericUpDown63.Value;
                        //Change numericUpDn(BACK) to be the one associated with the NumericUpDn(TOTAL) on the same line (reference the 1st IF STATEMENT)
                        numericUpDown62.Value = numericUpDown62.Value;

                        //Move the low FRONT AND BACK scores to the TEAM X Player FRONT AND BACK numeric UpDn
                        numericUpDown63.Value = lowFront;
                        numericUpDown62.Value = lowBack;
                        numericUpDown61.ForeColor = Color.White;
                        numericUpDown61.BackColor = Color.Red;

                        //Change this line to be the RTB line that equates with the NumericUpDn found in the IF STATEMENT
                        changeLine(twoManRTB, 31, nameOfLastPlayer);
                        //Current numericUpDn(BACK) that is on the same line as the NumericUpDn(TOTAL) on the same line found in the 1st IF STATEMENT for TEAM 7
                        changeLine(twoManRTB, 31, nameOfLowScorer);
                        //NumericUpDn is the NumericUpDn(TOTAL) of the NumericUpDn found in the IF STATEMENT for TEAM 7
                        MessageBox.Show(nameOfLowScorer + " with a score of " + numericUpDown61.Value.ToString() + "\nis randomly selected as the low Player", "Information", MessageBoxButtons.OK);
                    }
                }
            }
            if (numericUpDown58.Value == playerScores[0] && lowScorerHit == false)
            {
                lowScorerHit = true;
                /************ BLID DRAW FOR 12 PLAYERS ********************/
            }/*
            if (numericUpDown55.Value == playerScores[0])
            {
                TEAM 7 ^^^^^^^^^^^^^^^^^^^^^
                numericUpDown55.ForeColor = Color.White;
                numericUpDown55.BackColor = Color.Red;
            }*/
            button2.Visible = true;
            button3.Visible = true;
        }
        
        // SHOW THE TEAM WINNERS
        private void button2_Click(object sender, EventArgs e)
        {
            // Zero Out the Last Two rows to prevent false winners
            //numericUpDown63.Value = 0;
            //numericUpDown62.Value = 0; 
            //numericUpDown61.Value = 0;
            //numericUpDown60.Value = 0;
            //numericUpDown59.Value = 0;
            //numericUpDown58.Value = 0;


            if (numericUpDown9.Value == teamScoresFront.Max())
            {
                numericUpDown9.Enabled = true;
                numericUpDown9.ForeColor = Color.DarkBlue;
                numericUpDown9.BackColor = Color.Yellow;
            }
            if (numericUpDown12.Value == teamScoresFront.Max())
            {
                numericUpDown12.Enabled = true;
                numericUpDown12.ForeColor = Color.DarkBlue;
                numericUpDown12.BackColor = Color.Yellow;
            }
            if (numericUpDown21.Value == teamScoresFront.Max()) 
            {
                numericUpDown21.Enabled = true;
                numericUpDown21.ForeColor = Color.DarkBlue;
                numericUpDown21.BackColor = Color.Yellow;
            }
            if (numericUpDown48.Value == teamScoresFront.Max())
            {
                numericUpDown48.Enabled = true;
                numericUpDown48.ForeColor = Color.DarkBlue;
                numericUpDown48.BackColor = Color.Yellow;
            }
            if (numericUpDown39.Value == teamScoresFront.Max())
            {
                numericUpDown39.Enabled = true;
                numericUpDown39.ForeColor = Color.DarkBlue;
                numericUpDown39.BackColor = Color.Yellow;
            }
            if (numericUpDown30.Value == teamScoresFront.Max())
            {
                numericUpDown30.Enabled = true;
                numericUpDown30.ForeColor = Color.DarkBlue;
                numericUpDown30.BackColor = Color.Yellow;
            }
            if (numericUpDown57.Value == teamScoresFront.Max())
            {
                numericUpDown57.Enabled = true;
                numericUpDown57.ForeColor = Color.DarkBlue;
                numericUpDown57.BackColor = Color.Yellow;
            }

            if (numericUpDown8.Value == teamScoresBack.Max())
            {
                numericUpDown8.Enabled = true;
                numericUpDown8.ForeColor = Color.DarkBlue;
                numericUpDown8.BackColor = Color.Yellow;
            }
            if (numericUpDown11.Value == teamScoresBack.Max())
            {
                numericUpDown11.Enabled = true;
                numericUpDown11.ForeColor = Color.DarkBlue;
                numericUpDown11.BackColor = Color.Yellow;
            }
            if (numericUpDown20.Value == teamScoresBack.Max())
            {
                numericUpDown20.Enabled = true;
                numericUpDown20.ForeColor = Color.DarkBlue;
                numericUpDown20.BackColor = Color.Yellow;
            }
            if (numericUpDown47.Value == teamScoresBack.Max())
            {
                numericUpDown47.Enabled = true;
                numericUpDown47.ForeColor = Color.DarkBlue;
                numericUpDown47.BackColor = Color.Yellow;
            }
            if (numericUpDown38.Value == teamScoresBack.Max())
            {
                numericUpDown38.Enabled = true;
                numericUpDown38.ForeColor = Color.DarkBlue;
                numericUpDown38.BackColor = Color.Yellow;
            }
            if (numericUpDown29.Value == teamScoresBack.Max())
            {
                numericUpDown29.Enabled = true;
                numericUpDown29.ForeColor = Color.DarkBlue;
                numericUpDown29.BackColor = Color.Yellow;
            }
            if (numericUpDown56.Value == teamScoresBack.Max())
            {
                numericUpDown56.Enabled = true;
                numericUpDown56.ForeColor = Color.DarkBlue;
                numericUpDown56.BackColor = Color.Yellow;
            }

            if (numericUpDown7.Value == teamScoresTotal.Max())
            {
                numericUpDown7.Enabled = true;
                numericUpDown7.ForeColor = Color.DarkBlue;
                numericUpDown7.BackColor = Color.Yellow;
            }
            if (numericUpDown10.Value == teamScoresTotal.Max())
            {
                numericUpDown10.Enabled = true;
                numericUpDown10.ForeColor = Color.DarkBlue;
                numericUpDown10.BackColor = Color.Yellow;
            }
            if (numericUpDown19.Value == teamScoresTotal.Max())
            {
                numericUpDown19.Enabled = true;
                numericUpDown19.ForeColor = Color.DarkBlue;
                numericUpDown19.BackColor = Color.Yellow;
            }
            if (numericUpDown46.Value == teamScoresTotal.Max())
            {
                numericUpDown46.Enabled = true;
                numericUpDown46.ForeColor = Color.DarkBlue;
                numericUpDown46.BackColor = Color.Yellow;
            }
            if (numericUpDown37.Value == teamScoresTotal.Max())
            {
                numericUpDown37.Enabled = true;
                numericUpDown37.ForeColor = Color.DarkBlue;
                numericUpDown37.BackColor = Color.Yellow;
            }
            if (numericUpDown28.Value == teamScoresTotal.Max())
            {
                numericUpDown28.Enabled = true;
                numericUpDown28.ForeColor = Color.DarkBlue;
                numericUpDown28.BackColor = Color.Yellow;
            }
            if (numericUpDown55.Value == teamScoresTotal.Max())
            {
                numericUpDown55.Enabled = true;
                numericUpDown55.ForeColor = Color.DarkBlue;
                numericUpDown55.BackColor = Color.Yellow;
            }

            MessageBox.Show("Winners of the FRONT " + teamScoresFront.Max() + "\nWinners of the BACK " + teamScoresBack.Max() + "\nWinners of the 18 Total " + teamScoresTotal.Max());
        }
    }
}
