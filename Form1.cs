using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace BlueTeeApp
{
    public partial class Form1 : Form
    {
        static public Int32 playerCnt = 1, teamCnt = 0, totalHcp = 0;
        static public bool hideLowManOutBtn = false;
        static public int avgPlayerHcp = 0;

        static Single teeHCP = 0;
            
        static string hcpIndex = "", courseName = null;

        static Hashtable NamePlusCourseHcp = new Hashtable();

        Form2 twoManForm = new Form2();
        Form3 wolfForm = new Form3();
        FormTeeSelc teeBoxSelc = new FormTeeSelc();
        EnterSubnetForm guestGolferName = new EnterSubnetForm();
        
        private static int team1g = 0, team2g = 0, team3g = 0, team4g = 0, team5g = 0, team6g = 0, team7g = 0, teamHcp = 0;
        private static int[] goldGolfers = { 0, 0, 0, 0, 0, 0, 0 };
        private static int[] teamsHcpArry = { 0, 0, 0, 0, 0, 0, 0 };

        private static string dbcFilePath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\WccGolfers.txt";
        private static string hcpFilePath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\SlopeRating.txt";

        private static string minGoldTeamA = null, minGoldTeamB = null, minGoldTeamC = null, maxGoldTeamA = null, maxGoldTeamB = null, maxGoldTeamC = null;
        private static string goldplayerToReplace = null;
        
        string whiteplayerToReplace = null;

        private static Single whtRating = 70.6F, goldRating = 66.8F, redRating = 71.3F, whtSlope = 128, goldSlope = 121, redSlope = 119, whtPar = 72, goldPar = 72, redPar = 72, greenRating = 62.1F, greenSlope = 112, greenPar = 72;

        static readonly string SWver = "SWver: 1_6_22";

        /// <summary>
        /// Revision History
        /// 6_17 Added the Course Name to the SlopeRating Txt and displayed it in the APP along with the date.
        /// 6_20 Added a form to be able to enter the Guest Player's Name.
        /// 6_21 Corrected a problem; if a person had a last name of Williams it was changing it to Wills
        /// 6_22 Added A SYNCHRONIZED listBox7 that contains the fist occurence of the first letter of the last name to aid in finding the players name in lisBox1.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Created to assist the group leader build teams on a given day." +
                "\nTeams can be random or sorted based on each golfer's course hcp." +
                "\nTeams are also balanced based on different tees used per team." +
                "\nRandom blinds are used if there are an odd number of players and 3-man teams aren't an option." +
                "\nFor smaller groups random 2-man teams is also a possibility." +
                "\nFinally, scoring for an individual game with 5 golfers is provided." +
                "\nAuthor, johnedoub@gmail.com. Weeks of work led to this.", "APP Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public Form1()
        {
            InitializeComponent();

            // I found this solution on StackOverflow. This triggers the form to close when clicking on the main Form's upper-right corner [X].
            // Delegate the event to a handler using this style of statement
            FormClosing += MainWin_FormClosing;
            
            // I found this solution on StackOverflow. The scroll bar event doesn't sychronize the listBoxes until
            // the Mouse Button is released. so I put a TextBox over the vertical scoll bar of listBox1 and the user slides listBox7 scollbar to find the players name.
            EventHandler handler = (s, e) => {
                if (s == listBox1)
                    listBox7.TopIndex = listBox1.TopIndex;
                if (s == listBox7)
                    listBox1.TopIndex = listBox7.TopIndex;
            };

            listBox1.MouseCaptureChanged += handler;
            listBox7.MouseCaptureChanged += handler;
            listBox1.SelectedIndexChanged += handler;
            listBox7.SelectedIndexChanged += handler;
        }

        /// <summary>
        /// Kill all the processes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWin_FormClosing(object sender, FormClosingEventArgs e)
        {
            Process.GetCurrentProcess().Kill();
            Application.Exit();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            numericUpDown1.Controls[0].Visible = false;

            swLbl.Text = SWver;            

            if (MessageBox.Show("Is the GHIN Database up-to-date?", "Check Information", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                // READ the DBCconfig File
                transferGHINtoListBox();
            }
            else
            {
                MessageBox.Show("1) Open GHIN APP and export WCC Golfers,\n" +
                    "2) Open the newly exported EXCEL file,\n" +
                    "3) Sort on Gender with the MEN on top (Extended Sort Z->a),\n" +
                    "4) Copy the NAMES and HCP Index columns down to SARA BREED,\n" +
                    "5) Click on the [OK] msgBtn, that opens the current file,\n" +
                    "6) Cut-N-Paste the new info into the old File. Save and Exit.", "Check Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                try
                {
                    var process = Process.Start("notepad.exe", dbcFilePath);
                    process.WaitForExit();
                }
                catch (Exception except)
                {
                    System.Windows.Forms.MessageBox.Show(except.Message, "NotePadHelper");
                }
                // READ the DBCconfig File
                transferGHINtoListBox();
            }
            if (courseName != null)
                label7.Text += "  for " + courseName + " on " + DateTime.Now.ToString("dddd, dd MMMM yyyy");
            else
                label7.Text += "  for Unknown on " + DateTime.Now.ToString("dddd, dd MMMM yyyy");
        }

        /// <summary>
        /// Read two files. First the Club members list (sorted a-Z Already), 2nd the Course details (Rating,Slope,Par)
        /// Make the names all CAPS and last name First to aid in finding a given player.
        /// also add 3 guest players to the master list.
        /// </summary>
        private void transferGHINtoListBox()
        {
            string textline = null, golferName;

            //Refresh the list boxes with the latest NAME/HCP INFO
            listBox1.Items.Clear();
            listBox7.Items.Clear();
            listBox5.Items.Clear();

            // READ the DBC File
            if (File.Exists(dbcFilePath))
            {
                // Read the file into memory
                using (FileStream fs = new FileStream(dbcFilePath, FileMode.Open, FileAccess.Read, share: FileShare.Read))
                using (var sr = new StreamReader(fs, Encoding.UTF8))
                {
                    // Cycle through all the Golfers that are on the Master List. add all the Info into the HASH TABLE
                    do
                    {
                        textline = sr.ReadLine();
                        textline = textline.ToUpper();

                        if (textline.Length > 0)
                        {
                            // the endpt is the tab character
                            int endpt = textline.IndexOf("\t");

                            // Strip off the handicap Index number
                            hcpIndex = textline.Substring(endpt + 1, textline.Length - (endpt + 1));

                            // Strip off the Players Name
                            golferName = textline.Substring(0, endpt);

                            // Separate the First and Last name
                            string[] names = golferName.Trim().Split(new char[] { ' ' }, 3);

                            // Shorten some common first names
                            if (names[0].Contains("PATRICK"))
                                names[0] = names[0].Replace("PATRICK", "PAT");
                            else if (names[0].Contains("WILLIAM"))
                                names[0] = names[0].Replace("WILLIAM", "WILL");
                            else if (names[0].Contains("RICHARD"))
                                names[0] = names[0].Replace("RICHARD", "RICH");
                            else if (names[0].Contains("EDWARD"))
                                names[0] = names[0].Replace("EDWARD", "ED");

                            // Look for middle initials and SR designation in the 2nd string to put it at the end.
                            if (names.Length == 3)
                            {
                                if (names[1].Length == 1 || names[1].Contains(".") || names[1].Contains("SR"))
                                    golferName = names[2] + " " + names[0] + " " + names[1];
                                else if (names[2].Length <= 3 && names[2].Length > 0)
                                    golferName = names[1] + " " + names[0] + " " + names[2];
                            }
                            else
                                golferName = names[1] + " " + names[0];

                            // Write them out to a ListBox
                            listBox1.Items.Add(golferName);

                            var firstLetter = golferName.Substring(0, 1);

                            if (listBox7.Items.Contains(firstLetter))
                            {
                                listBox7.Items.Add(" ");
                            }
                            else
                                listBox7.Items.Add(firstLetter);

                            listBox5.Items.Add(hcpIndex);
                            
                            //listBox1.DrawMode = DrawMode.OwnerDrawFixed;                            
                            //listBox1.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.ListBox1_DrawItem);                            
                        }
                    } while (sr.Peek() != -1);
                }
                
                // Make 5 dummy players to allow for guests. The user is prompted later to enter their actual names and HCP index
                golferName = "Guest Player I";
                golferName = golferName.ToUpper();
                listBox5.Items.Add(12);
                // Write them out to a ListBox
                listBox1.Items.Add(golferName);
                listBox7.Items.Add(" ");            //To keep the list Box items the same for scrolling purposes.

                golferName = "Guest Player II";
                golferName = golferName.ToUpper();
                listBox5.Items.Add(15);
                // Write them out to a ListBox
                listBox1.Items.Add(golferName);
                listBox7.Items.Add(" ");

                golferName = "Guest Player III";
                golferName = golferName.ToUpper();
                listBox5.Items.Add(10);
                // Write them out to a ListBox
                listBox1.Items.Add(golferName);
                listBox7.Items.Add(" ");

                golferName = "Guest Player IV";
                golferName = golferName.ToUpper();
                listBox5.Items.Add(10);
                // Write them out to a ListBox
                listBox1.Items.Add(golferName);
                listBox7.Items.Add(" ");

                golferName = "Guest Player V";
                golferName = golferName.ToUpper();
                listBox5.Items.Add(10);
                // Write them out to a ListBox
                listBox1.Items.Add(golferName);
                listBox7.Items.Add(" ");
            }
            else
            {
                // The File is missing                            
                MessageBox.Show("Error: " + dbcFilePath + " FILE NOT FOUND\n" + "\nYou must Associate a WccGolfers.txt file.", "Check Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                MainWin_FormClosing(null, null);
            }

            // READ the Course Rating/Slope/Par File
            if (File.Exists(hcpFilePath))
            {
                // Read the file into memory
                using (FileStream fs = new FileStream(hcpFilePath, FileMode.Open, FileAccess.Read, share: FileShare.Read))
                using (var sr = new StreamReader(fs, Encoding.UTF8))
                {
                    do
                    {
                        textline = sr.ReadLine();
                        // the endpt is the '=' character
                        int endpt = textline.IndexOf("=");

                        // Strip off the course handicap Rating/Slope/Par value for 4 sets of tees Wht,Gld,Red,Grn
                        if (textline.Contains("whiteRating"))                        
                            whtRating = Convert.ToSingle(textline.Substring(endpt + 2, textline.Length - (endpt + 2)));
                        else if (textline.Contains("whiteSlope"))
                            whtSlope = Convert.ToSingle(textline.Substring(endpt + 2, textline.Length - (endpt + 2)));
                        else if (textline.Contains("goldRating"))
                            goldRating = Convert.ToSingle(textline.Substring(endpt + 2, textline.Length - (endpt + 2)));
                        else if (textline.Contains("goldSlope"))
                            goldSlope = Convert.ToSingle(textline.Substring(endpt + 2, textline.Length - (endpt + 2)));
                        else if (textline.Contains("redRating"))
                            redRating = Convert.ToSingle(textline.Substring(endpt + 2, textline.Length - (endpt + 2)));
                        else if (textline.Contains("redSlope"))
                            redSlope = Convert.ToSingle(textline.Substring(endpt + 2, textline.Length - (endpt + 2)));
                        else if (textline.Contains("whitePar"))
                            whtPar = Convert.ToSingle(textline.Substring(endpt + 2, textline.Length - (endpt + 2)));
                        else if (textline.Contains("goldPar"))
                            goldPar = Convert.ToSingle(textline.Substring(endpt + 2, textline.Length - (endpt + 2)));
                        else if (textline.Contains("redPar"))
                            redPar = Convert.ToSingle(textline.Substring(endpt + 2, textline.Length - (endpt + 2)));
                        else if (textline.Contains("greenRating"))
                            greenRating = Convert.ToSingle(textline.Substring(endpt + 2, textline.Length - (endpt + 2)));
                        else if (textline.Contains("greenSlope"))
                            greenSlope = Convert.ToSingle(textline.Substring(endpt + 2, textline.Length - (endpt + 2)));
                        else if (textline.Contains("greenPar"))
                            greenPar = Convert.ToSingle(textline.Substring(endpt + 2, textline.Length - (endpt + 2)));
                        else if (textline.Contains("Course"))
                            courseName = textline.Substring(endpt + 2, textline.Length - (endpt + 2));
                    } while (sr.Peek() != -1);
                }
            }
            else
            {
                // The File is missing so create it from WCC default values
                MessageBox.Show("Error: " + hcpFilePath + " FILE NOT FOUND\n" + "\nBy default WCC INFO will used.", "Check Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                string[] wccInfo = new string[13];
                               
                wccInfo[0] = "whiteRating = " + whtRating.ToString();
                wccInfo[1] = "whiteSlope = " + whtSlope.ToString();
                wccInfo[2] = "whitePar = " + whtPar.ToString();
                wccInfo[3] = "goldRating = " + goldRating.ToString();
                wccInfo[4] = "goldSlope = " + goldSlope.ToString();
                wccInfo[5] = "goldPar = " + goldPar.ToString();
                wccInfo[6] = "redRating = " + redRating.ToString();
                wccInfo[7] = "redSlope =  " + redSlope.ToString();
                wccInfo[8] = "redPar = " + redPar.ToString();
                wccInfo[9] = "greenRating = " + greenRating.ToString();
                wccInfo[10] = "greenSlope = " + greenSlope.ToString();
                wccInfo[11] = "greenPar = " + greenPar.ToString();
                wccInfo[12] = "Course = WCC";

                File.WriteAllLines(hcpFilePath, wccInfo);
            }
        }
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Launch NOTEPAD and open the surrent GHIN Database File
                var process = Process.Start("notepad.exe", dbcFilePath);
                process.WaitForExit();
            }
            catch (Exception except)
            {
                System.Windows.Forms.MessageBox.Show(except.Message, "NotePadHelper");
            }
            // READ the DBCconfig File
            transferGHINtoListBox();
        }
        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            teeHCP = Convert.ToSingle(numericUpDown1.Value);
            teeHCP = (float)Math.Round(teeHCP);

            // Update the "dummy" Index of the GUEST PLAYER with the value entered HERE
            listBox5.Items[listBox1.SelectedIndex] = teeHCP.ToString();

            numericUpDown1.Visible = false;
            guestLbl.Visible = false;
            instrLbl.Visible = false;
            textBox1.Visible = false;
            
            playerTeeSelection();
        }

        /// <summary>
        /// listBox1_SelectedIndexChanged - When the Master list's Item is Selected it is copied into the Active List.
        /// The user is also prompted to enter the tees the golfer will play from and a course hcp is calculated. Bothe the tees and the hcp and listed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            // The test Item's value is determined by the listed order. 
            // The list order is determined by the physical wiring of the relay board.
            // This way there is a one-to-one relationship of the RELAY ID on the PCB to the List.
            //playerID[playerCnt] = listBox1.SelectedIndex;

            try
            {
                //Check that the Golfer's name and the Golfer's name + '*' aren't in the list already. This prevents multiple selections/entries on mis-clicks                
                if (!NamePlusCourseHcp.ContainsKey((string)listBox1.SelectedItem) && !NamePlusCourseHcp.ContainsKey((string)(listBox1.SelectedItem + "*")))
                {
                    string playerID = (string)listBox1.SelectedItem;

                    if (playerID.Contains("GUEST"))
                    {
                        numericUpDown1.Visible = true;  // Enter the players index and then calculate the course HCP based on what tee box is selected

                        textBox1.Visible = true;
                        guestLbl.Visible = true;
                        instrLbl.Visible = true;
                    }
                    else
                    {
                        playerTeeSelection();
                    }
                }

                if (playerCnt - 1 == 5)     //Since it has already been incremented
                    wolfPointCalcToolStripMenuItem.Visible = true;
                else
                    wolfPointCalcToolStripMenuItem.Visible = false;
            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString(), "Check Information");
            }
            
        }
        private void playerTeeSelection ()
        {
            string golferName = "", golfersIndex = "";
            int indx = listBox1.SelectedIndex;

            DialogResult selectionDialog;

            selectionDialog = teeBoxSelc.ShowDialog();

            listBox6.Items.Add(playerCnt);

            if (selectionDialog == DialogResult.Cancel)
            {
                listBox6.Items.Remove(playerCnt);
            }
            else if (selectionDialog == DialogResult.OK)
            {
                golferName = (string)listBox1.SelectedItem;

                // Separate the First and Last name
                string[] names = golferName.Trim().Split(new char[] { ' ' }, 3);

                //Reverse the names to show first name last name.
                if (names.Length == 3)
                {
                    if (names[2].Length == 1 || names[2].Contains(".") || names[2].Contains("SR"))
                        golferName = names[1] + " " + names[2] + " " + names[0];
                    else if (names[2].Length <= 3 && names[2].Length > 0)
                        golferName = names[1] + " " + names[0] + " " + names[2];
                }
                else
                    golferName = names[1] + " " + names[0];

                // Calculate the course HCP based on what tee box is selected
                if (FormTeeSelc.playersTeeBox == 1)
                {
                    if (golferName.Contains("GUEST"))
                    {
                        DialogResult selcDialog;

                        selcDialog = guestGolferName.ShowDialog();

                        golferName = guestGolferName.guestPlayerID;
                    }

                    listBox2.Items.Add(golferName);

                    listBox3.Items.Add("WHITE");

                    golfersIndex = (string)listBox5.GetItemValue(listBox5.Items[indx]);

                    if (golfersIndex.Contains("NH"))
                    {
                        teeHCP = 18;
                        listBox4.Items.Add(teeHCP);
                    }
                    else if (golfersIndex.Contains("+"))
                    {
                        golfersIndex = golfersIndex.Replace('+', '-');

                        teeHCP = Convert.ToSingle(golfersIndex) * whtSlope / 113 + (whtRating - whtPar);
                        teeHCP = Math.Abs(teeHCP);
                        teeHCP = (float)Math.Round(teeHCP);
                        listBox4.Items.Add("+" + teeHCP);
                    }
                    else
                    {
                        teeHCP = Convert.ToSingle(golfersIndex) * whtSlope / 113 + (whtRating - whtPar);
                        teeHCP = (float)Math.Round(teeHCP);

                        if (teeHCP < 0)
                        {
                            teeHCP = Math.Abs(teeHCP);
                            listBox4.Items.Add("+" + teeHCP);
                        }
                        else
                            listBox4.Items.Add(teeHCP);
                    }
                }
                else if (FormTeeSelc.playersTeeBox == 2)
                {
                    if (golferName.Contains("GUEST"))
                    {
                        DialogResult selcDialog;

                        selcDialog = guestGolferName.ShowDialog();

                        golferName = guestGolferName.guestPlayerID;
                    }
                    // Add the ASTERISK for the different Tee Box
                    golferName = golferName + " *";

                    listBox2.Items.Add(golferName);

                    listBox3.Items.Add("GOLD");

                    golfersIndex = (string)listBox5.GetItemValue(listBox5.Items[indx]);

                    if (golfersIndex.Contains("NH"))
                    {
                        teeHCP = 18;
                        listBox4.Items.Add(teeHCP);
                    }
                    else if (golfersIndex.Contains("+"))
                    {
                        golfersIndex = golfersIndex.Replace('+', '-');

                        teeHCP = Convert.ToSingle(golfersIndex) * goldSlope / 113 + (goldRating - goldPar);
                        teeHCP = Math.Abs(teeHCP);
                        teeHCP = (float)Math.Round(teeHCP);
                        listBox4.Items.Add("+" + teeHCP);
                    }
                    else
                    {
                        teeHCP = Convert.ToSingle(golfersIndex) * goldSlope / 113 + (goldRating - goldPar);
                        teeHCP = (float)Math.Round(teeHCP);

                        if (teeHCP < 0)
                        {
                            teeHCP = Math.Abs(teeHCP);
                            listBox4.Items.Add("+" + teeHCP);
                        }
                        else
                            listBox4.Items.Add(teeHCP);

                    }
                }
                else if (FormTeeSelc.playersTeeBox == 3)
                {
                    if (golferName.Contains("GUEST"))
                    {
                        DialogResult selcDialog;

                        selcDialog = guestGolferName.ShowDialog();

                        golferName = guestGolferName.guestPlayerID;
                    }
                    // Add the ASTERISK for the different Tee Box
                    golferName = golferName + " *";

                    listBox2.Items.Add(golferName);

                    listBox3.Items.Add("RED");

                    golfersIndex = (string)listBox5.GetItemValue(listBox5.Items[indx]);

                    if (golfersIndex.Contains("NH"))
                    {
                        teeHCP = 18;
                        listBox4.Items.Add(teeHCP);
                    }
                    else if (golfersIndex.Contains("+"))
                    {
                        golfersIndex = golfersIndex.Replace('+', '-');

                        teeHCP = Convert.ToSingle(golfersIndex) * redSlope / 113 + (redRating - redPar);
                        teeHCP = Math.Abs(teeHCP);
                        teeHCP = (float)Math.Round(teeHCP);
                        listBox4.Items.Add("+" + teeHCP);
                    }
                    else
                    {
                        teeHCP = Convert.ToSingle(golfersIndex) * redSlope / 113 + (redRating - redPar);
                        teeHCP = (float)Math.Round(teeHCP);

                        if (teeHCP < 0)
                        {
                            teeHCP = Math.Abs(teeHCP);
                            listBox4.Items.Add("+" + teeHCP);
                        }
                        else
                            listBox4.Items.Add(teeHCP);

                    }
                }
                else if (FormTeeSelc.playersTeeBox == 4)
                {
                    if (golferName.Contains("GUEST"))
                    {
                        DialogResult selcDialog;

                        selcDialog = guestGolferName.ShowDialog();

                        golferName = guestGolferName.guestPlayerID;
                    }
                    // Add the ASTERISK for the different Tee Box
                    golferName = golferName + " *";

                    listBox2.Items.Add(golferName);

                    listBox3.Items.Add("GREEN");

                    golfersIndex = (string)listBox5.GetItemValue(listBox5.Items[indx]);

                    if (golfersIndex.Contains("NH"))
                    {
                        teeHCP = 18;
                        listBox4.Items.Add(teeHCP);
                    }
                    else if (golfersIndex.Contains("+"))
                    {
                        golfersIndex = golfersIndex.Replace('+', '-');

                        teeHCP = Convert.ToSingle(golfersIndex) * greenSlope / 113 + (greenRating - greenPar);
                        teeHCP = Math.Abs(teeHCP);
                        teeHCP = (float)Math.Round(teeHCP);
                        listBox4.Items.Add("+" + teeHCP);
                    }
                    else
                    {
                        teeHCP = Convert.ToSingle(golfersIndex) * greenSlope / 113 + (greenRating - greenPar);
                        teeHCP = (float)Math.Round(teeHCP);

                        if (teeHCP < 0)
                        {
                            teeHCP = Math.Abs(teeHCP);
                            listBox4.Items.Add("+" + teeHCP);
                        }
                        else
                            listBox4.Items.Add(teeHCP);

                    }
                }
                playerCnt++;
                NamePlusCourseHcp.Add((string)listBox1.SelectedItem, teeHCP);
            }
        }
        
        private void SortRTBlines()
        {
            string playerInfo = null;

            richTextBox1.HideSelection = false; //for showing selection

            /*Saving current selection*/
                string selectedText = richTextBox1.SelectedText;

            /*Saving current line*/
            int firstCharInLineIndex = richTextBox1.GetFirstCharIndexOfCurrentLine();
            int currLineIndex = richTextBox1.Text.Substring(0, firstCharInLineIndex).Count(c => c == '\n');
            string currLine = richTextBox1.Lines[currLineIndex];
            int offset = richTextBox1.SelectionStart - firstCharInLineIndex;


            /*Sorting richTextBox requires putting the STRING into a STRING ARRAY LINE BY LINE*/
            string[] lines = richTextBox1.Lines;
            List<string> LineOfText = lines.ToList<string>();

            LineOfText.RemoveAll(str => String.IsNullOrEmpty(str));

            lines = LineOfText.ToArray();

            // Split lines into separate string arrays
            string[][] grid = new string[lines.Length][];
            for (int i = 0; i < lines.Length; i++)
            {
                grid[i] = lines[i].Split('\t', (char)StringSplitOptions.RemoveEmptyEntries); // split on whitespace
            }
            
            //Array.Sort(lines, delegate (string str1, string str2) { return str1.CompareTo(str2); });
            IEnumerable<string[]> results = grid
        .OrderBy(gridRow => Int16.Parse(gridRow[0]))
        .ThenBy(gridRow => gridRow[1]);

            List<string[]> joinedText = results.ToList();

            for (int cnt = 0; cnt < joinedText.Count; cnt++)
            {
                playerInfo += String.Join(" ", joinedText[cnt]) + "\n";             
            }

            richTextBox1.Text = playerInfo;
            richTextBox1.Text = richTextBox1.Text.Replace("-", "+");

            lines = richTextBox1.Lines;
            LineOfText = lines.ToList<string>();

            LineOfText.RemoveAll(str => String.IsNullOrEmpty(str));

            richTextBox3.Clear();

            for (int i = 0; i < richTextBox1.Lines.Count(); i++)
            {
                if (richTextBox1.Lines[i].Contains("*")) 
                {
                    richTextBox3.Text += "*" + richTextBox1.Lines[i] + "\n";
                }
            }
            for (int i = 0; i < richTextBox1.Lines.Count(); i++)
            {
                if (!richTextBox1.Lines[i].Contains("*"))
                {
                    richTextBox3.Text += richTextBox1.Lines[i] + "\n";
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Capture the index for removing items
            int indx = listBox2.SelectedIndex;

            // Check to see that a Player has been selected in the List
            if (indx != -1)
            {
                string golfersName = (String)listBox2.SelectedItem;

                NamePlusCourseHcp.Remove(golfersName);
                //Remove the Player
                listBox2.Items.Remove(listBox2.SelectedItem);

                //Capture the items in the Tee HCP listBox4
                var teeHCPlist = new List<string>();
                foreach (var item in listBox4.Items)
                    teeHCPlist.Add(item.ToString());

                //Remove the Player's tee HCP
                teeHCPlist.Remove(teeHCPlist[indx]);

                //After Deletion of the Player; Refresh the listBox4 displaying the TEE Handicap for each player
                listBox4.Items.Clear();
                foreach (var item in teeHCPlist)
                    listBox4.Items.Add(item);

                //After Deletion of the Player; Refresh the number of players in case the player removed was somewhere in the "middle"
                listBox6.Items.Clear();                
                //Display the number of players in a column
                playerCnt--;
                for (int i = 1; i < playerCnt; i++)
                {
                    listBox6.Items.Add(i);
                }

                //After Deletion of the Player; Refresh the listBox3 displaying the TEE TYPE
                listBox3.Items.Clear();
                for (int i = 0; i < playerCnt - 1; i++)
                {
                    if (!listBox2.Items[i].ToString().Contains("*"))
                        listBox3.Items.Add("WHITE");
                    else
                        listBox3.Items.Add("GOLD");
                }
                richTextBox1.Clear();
                richTextBox2.Clear();
                twoManTeamsToolStripMenuItem.Visible = false;
            }
            else
            {
                MessageBox.Show("Error: not Item Selected.\n", "Check Information");
            }
        }

        private void aBCDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //for integers
            Random rnd = new Random();

            int rIntF = 0, rIntB = 0;
            string blindString = null;

            teamCnt = 0; teamHcp = 0;
            
            avgPlayerHcp = 0;
            team1g = 0; team2g = 0; team3g = 0; team4g = 0; team5g = 0; team6g = 0; team7g = 0;            

            richTextBox1.Clear();
            richTextBox2.Clear();

            totalHcp = 0;
            for (int cntr = 0; cntr < (playerCnt - 1); cntr++)
            {
                richTextBox1.Text += listBox4.Items[cntr] + "\t" + listBox2.Items[cntr] + "\r\n";

                // Take a PLUS HCP number and make it a NEG number to calculate the total HCP correctly.
                if (listBox4.Items[cntr].ToString().Contains("+"))
                {
                    string revSignOfValue = listBox4.Items[cntr].ToString().Replace("+", "-");
                    Int32 plusHCP = Convert.ToInt32(revSignOfValue);
                    totalHcp += plusHCP; 
                }
                else
                    totalHcp += Convert.ToInt32(listBox4.Items[cntr]);
                
                // Initialize the gold Tee Golfers Array to zero. This holds the number of players per team playing from the golds.
                if (cntr < 7) goldGolfers[cntr] = 0;
            }
            richTextBox1.Text.Trim();
            // Restore the PLUS HCP number
            richTextBox1.Text = richTextBox1.Text.Replace("+", "-");
            
            // Prep for the SORT RTB function
            richTextBox1.SelectAll();

            // Function call to Sort the lines in the richTextBox1
            SortRTBlines();

            richTextBox2.ForeColor = Color.DarkBlue;

            richTextBox2.Font = new Font("Lucida Console", 10);

            switch (playerCnt - 1)
            {
                case 6:
                    teamCnt = 2;
                    
                    richTextBox2.Text = "A-B-C-D DRAW for " + (playerCnt-1).ToString() + " golfers\n";
                    richTextBox2.Text += "TEAM 1\n";
                    richTextBox2.Text += richTextBox1.Lines[0];
                    if (richTextBox1.Lines[0].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[3];
                    if (richTextBox1.Lines[3].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[5];
                    if (richTextBox1.Lines[5].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[0] = team1g;
                    
                    richTextBox2.Text += Environment.NewLine;

                    richTextBox2.Text += "TEAM 2\n";
                    richTextBox2.Text += richTextBox1.Lines[1];
                    if (richTextBox1.Lines[1].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[2];
                    if (richTextBox1.Lines[2].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[4];
                    if (richTextBox1.Lines[4].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[1] = team2g;
                    
                    //Check the goldGolfer deficit between the MAX and MIN TEAMS
                    while (((goldGolfers[0] >= 2 || goldGolfers[1] >= 2) && (goldGolfers[0] == 0 || goldGolfers[1] == 0)) 
                        || ((goldGolfers[0] == 3 || goldGolfers[1] == 3) && (goldGolfers[0] == 1 || goldGolfers[1] == 1)))
                    {
                        determineTeamsToSwap(teamCnt);

                        swapGoldForWhiteThreesome();
                    }
                    //Use the +10 to force the program path dow a 3-Person team display
                    displayTeamHCPcalc(teamCnt + 10);

                    break;
                case 7:
                    teamCnt = 2;
                    
                    richTextBox2.Text = "A-B-C-D DRAW for " + (playerCnt - 1).ToString() + " golfers\n";
                    richTextBox2.Text += "TEAM 1\n";                                                            //1
                    richTextBox2.Text += richTextBox1.Lines[1];                                                 //2
                    if (richTextBox1.Lines[1].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[2];                                 //rtb Line3
                    if (richTextBox1.Lines[2].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[5];                                 //rtb Line4
                    if (richTextBox1.Lines[5].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[6];                                 //rtb Line5
                    if (richTextBox1.Lines[6].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[0] = team1g;

                    richTextBox2.Text += Environment.NewLine;                                   //6              

                    richTextBox2.Text += "TEAM 2\n";                                            //7
                    richTextBox2.Text += richTextBox1.Lines[0];                                 //rtb Line8
                    if (richTextBox1.Lines[0].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[3];                                 //rtb Line9
                    if (richTextBox1.Lines[3].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[4];                                 //rtb Line10
                    if (richTextBox1.Lines[4].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += "BLIND\n";                                             //11
                    
                    goldGolfers[1] = team2g;

                    //Check the goldGolfer deficit between the MAX and MIN TEAMS
                    while (((goldGolfers[0] >= 2 || goldGolfers[1] >= 2) && (goldGolfers[0] == 0 || goldGolfers[1] == 0)) 
                        || ((goldGolfers[0] >= 3 || goldGolfers[1] >= 3) && (goldGolfers[0] == 1 || goldGolfers[1] == 1)) 
                        || ((goldGolfers[0] == 4 || goldGolfers[1] == 4) && (goldGolfers[0] == 2 || goldGolfers[1] == 2)))
                    {
                        determineTeamsToSwap(teamCnt);

                        swapGoldForWhiteFoursome(minGoldTeamA, maxGoldTeamA);
                    }
                    blindString = richTextBox2.Lines[4] + "(F) " +                      //rtb Line11
                            richTextBox2.Lines[5].TrimStart('1', '2', '3', '4', '5', '6', '7', '8', '9', '0', ' ') + "(B)";

                    changeLine(richTextBox2, 11, blindString);                    

                    displayTeamHCPcalc(teamCnt);
                    
                    break;
                case 8:
                    teamCnt = 2;
                    
                    richTextBox2.Text = "A-B-C-D DRAW for " + (playerCnt - 1).ToString() + " golfers\n";
                    richTextBox2.Text += "TEAM 1\n";                                            //rtb Line1
                    richTextBox2.Text += richTextBox1.Lines[0];                                 //rtb Line2
                    if (richTextBox1.Lines[0].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;                                 
                    richTextBox2.Text += richTextBox1.Lines[3];                                 //rtb Line3
                    if (richTextBox1.Lines[3].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;                                 
                    richTextBox2.Text += richTextBox1.Lines[4];                                 //rtb Line4
                    if (richTextBox1.Lines[4].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;                                 
                    richTextBox2.Text += richTextBox1.Lines[7];                                 //rtb Line5
                    if (richTextBox1.Lines[7].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[0] = team1g;

                    richTextBox2.Text += Environment.NewLine;                                 //rtb Line6

                    richTextBox2.Text += "TEAM 2\n";                                            //rtb Line7
                    richTextBox2.Text += richTextBox1.Lines[1];                                 //rtb Line8
                    if (richTextBox1.Lines[1].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;                                 
                    richTextBox2.Text += richTextBox1.Lines[2];                                 //rtb Line9
                    if (richTextBox1.Lines[2].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;                                 
                    richTextBox2.Text += richTextBox1.Lines[5];                                 //rtb Line10
                    if (richTextBox1.Lines[5].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;                                 
                    richTextBox2.Text += richTextBox1.Lines[6];                                 //rtb Line11
                    if (richTextBox1.Lines[6].Contains("*")) team2g++;
                    goldGolfers[1] = team2g;

                    //Check the goldGolfer deficit between the MAX and MIN TEAMS
                    while (((goldGolfers[0] >= 2 || goldGolfers[1] >= 2) && (goldGolfers[0] == 0 || goldGolfers[1] == 0))
                        || ((goldGolfers[0] >= 3 || goldGolfers[1] >= 3) && (goldGolfers[0] == 1 || goldGolfers[1] == 1))
                        || ((goldGolfers[0] == 4 || goldGolfers[1] == 4) && (goldGolfers[0] == 2 || goldGolfers[1] == 2)))
                    {
                        determineTeamsToSwap(teamCnt);
                        
                        swapGoldForWhiteFoursome(minGoldTeamA, maxGoldTeamA);
                    }

                    displayTeamHCPcalc(teamCnt);

                    break;
                case 9:
                    teamCnt = 3;
                    
                    richTextBox2.Text = "A-B-C-D DRAW for " + (playerCnt - 1).ToString() + " golfers\n";
                    richTextBox2.Text += "TEAM 1\n";                                        //1
                    richTextBox2.Text += richTextBox1.Lines[0];                             //2
                    if (richTextBox1.Lines[0].Contains("*")) team1g++;                      
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[5];                             //3
                    if (richTextBox1.Lines[5].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[6];                             //4
                    if (richTextBox1.Lines[6].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[0] = team1g;

                    richTextBox2.Text += Environment.NewLine;                               //5

                    richTextBox2.Text += "TEAM 2\n";                                        //6
                    richTextBox2.Text += richTextBox1.Lines[1];
                    if (richTextBox1.Lines[1].Contains("*")) team2g++;                      //7
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[4];                             //8
                    if (richTextBox1.Lines[4].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[7];                              //9
                    if (richTextBox1.Lines[7].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[1] = team2g;

                    richTextBox2.Text += Environment.NewLine;                               //10

                    richTextBox2.Text += "TEAM 3\n";                                        //11
                    richTextBox2.Text += richTextBox1.Lines[2];                             //12
                    if (richTextBox1.Lines[2].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[3];                             //13
                    if (richTextBox1.Lines[3].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[8];                             //14
                    if (richTextBox1.Lines[8].Contains("*")) team3g++;
                    goldGolfers[2] = team3g;

                    //Check the goldGolfer deficit between the MAX and MIN TEAMS
                    while (((goldGolfers[0] >= 2 || goldGolfers[1] >= 2 || goldGolfers[2] >= 2) && (goldGolfers[0] == 0 || goldGolfers[1] == 0 || goldGolfers[2] == 0))
                        || ((goldGolfers[0] >= 3 || goldGolfers[1] >= 3 || goldGolfers[2] >= 3) && (goldGolfers[0] == 1 || goldGolfers[1] == 1 || goldGolfers[2] == 1))
                        || ((goldGolfers[0] == 4 || goldGolfers[1] == 4 || goldGolfers[2] == 4) && (goldGolfers[0] == 2 || goldGolfers[1] == 2 || goldGolfers[2] == 2)))
                    {
                        determineTeamsToSwap(teamCnt);                     
                        
                        swapGoldForWhiteThreesome();
                    }
                    //Use the +10 to force the program path dow a 3-Person team display
                    displayTeamHCPcalc(teamCnt + 10);

                    break;
                case 10:
                    teamCnt = 3;                    

                    richTextBox2.Text = "A-B-C-D DRAW for " + (playerCnt - 1).ToString() + " golfers\n";
                    richTextBox2.Text += "TEAM 1\n";                                            //1
                    richTextBox2.Text += richTextBox1.Lines[0];                                 //2
                    if (richTextBox1.Lines[0].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[5];                                 //3
                    if (richTextBox1.Lines[5].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[6];                                 //4
                    if (richTextBox1.Lines[6].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += "BLIND\n";                                             //5
                    goldGolfers[0] = team1g;

                    richTextBox2.Text += Environment.NewLine;                                   //6

                    richTextBox2.Text += "TEAM 2\n";                                            //7
                    richTextBox2.Text += richTextBox1.Lines[1];                                 //8
                    if (richTextBox1.Lines[1].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[4];                                 //9
                    if (richTextBox1.Lines[4].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[7];                                 //10
                    if (richTextBox1.Lines[7].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += "BLIND\n";                                             //11
                    goldGolfers[1] = team2g;

                    richTextBox2.Text += Environment.NewLine;                                   //12

                    richTextBox2.Text += "TEAM 3\n";                                            //13
                    richTextBox2.Text += richTextBox1.Lines[2];                                 //14
                    if (richTextBox1.Lines[2].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[3];                                 //15
                    if (richTextBox1.Lines[3].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[8];                                 //16
                    if (richTextBox1.Lines[8].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[9];                                 //17
                    if (richTextBox1.Lines[9].Contains("*")) team3g++;
                    goldGolfers[2] = team3g;

                    blindString = richTextBox2.Lines[16] + "(F) " +
                        richTextBox2.Lines[17].TrimStart('1', '2', '3', '4', '5', '6', '7', '8', '9', '0', ' ') + "(B)";

                    changeLine(richTextBox2, 11, blindString);
                    changeLine(richTextBox2, 5, blindString);

                    //Check the goldGolfer deficit between the MAX and MIN TEAMS
                    while (((goldGolfers[0] >= 2 || goldGolfers[1] >= 2 || goldGolfers[2] >= 2) && (goldGolfers[0] == 0 || goldGolfers[1] == 0 || goldGolfers[2] == 0))
                        || ((goldGolfers[0] >= 3 || goldGolfers[1] >= 3 || goldGolfers[2] >= 3) && (goldGolfers[0] == 1 || goldGolfers[1] == 1 || goldGolfers[2] == 1))
                        || ((goldGolfers[0] == 4 || goldGolfers[1] == 4 || goldGolfers[2] == 4) && (goldGolfers[0] == 2 || goldGolfers[1] == 2 || goldGolfers[2] == 2)))
                    {
                        determineTeamsToSwap(teamCnt);
                        
                        swapGoldForWhiteFoursome(minGoldTeamA, maxGoldTeamA);
                    }

                    displayTeamHCPcalc(teamCnt);

                    break;
                case 11:
                    teamCnt = 3;

                    richTextBox2.Text = "A-B-C-D DRAW for " + (playerCnt - 1).ToString() + " golfers\n"; 
                    richTextBox2.Text += "TEAM 1\n";                                        //1
                    richTextBox2.Text += richTextBox1.Lines[2];                             //2
                    if (richTextBox1.Lines[2].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[3];                             //3
                    if (richTextBox1.Lines[3].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[8];                             //4
                    if (richTextBox1.Lines[8].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[9];                             //5
                    if (richTextBox1.Lines[9].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[0] = team1g;

                    richTextBox2.Text += Environment.NewLine;                               //6

                    richTextBox2.Text += "TEAM 2\n";                                        //7
                    richTextBox2.Text += richTextBox1.Lines[1];                             //8
                    if (richTextBox1.Lines[1].Contains("*")) team2g++;                      
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[4];                             //9
                    if (richTextBox1.Lines[4].Contains("*")) team2g++;                      
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[7];                             //10
                    if (richTextBox1.Lines[7].Contains("*")) team2g++;                      
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[10];                            //11
                    if (richTextBox1.Lines[10].Contains("*")) team2g++;                     
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[1] = team2g;
                                                                                            
                    richTextBox2.Text += Environment.NewLine;                               //12

                    richTextBox2.Text += "TEAM 3\n";                                        //13
                    richTextBox2.Text += richTextBox1.Lines[0];                             //14
                    if (richTextBox1.Lines[0].Contains("*")) team3g++;                      
                    richTextBox2.Text += Environment.NewLine;                               
                    richTextBox2.Text += richTextBox1.Lines[5];                             //15
                    if (richTextBox1.Lines[5].Contains("*")) team3g++;                      
                    richTextBox2.Text += Environment.NewLine;                               
                    richTextBox2.Text += richTextBox1.Lines[6];                             //16
                    if (richTextBox1.Lines[6].Contains("*")) team3g++;                      
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += "BLIND\n";                                         //17
                    goldGolfers[2] = team3g;

                    //Check the goldGolfer deficit between the MAX and MIN TEAMS
                    while (((goldGolfers[0] >= 2 || goldGolfers[1] >= 2 || goldGolfers[2] >= 2) && (goldGolfers[0] == 0 || goldGolfers[1] == 0 || goldGolfers[2] == 0))
                        || ((goldGolfers[0] >= 3 || goldGolfers[1] >= 3 || goldGolfers[2] >= 3) && (goldGolfers[0] == 1 || goldGolfers[1] == 1 || goldGolfers[2] == 1))
                        || ((goldGolfers[0] == 4 || goldGolfers[1] == 4 || goldGolfers[2] == 4) && (goldGolfers[0] == 2 || goldGolfers[1] == 2 || goldGolfers[2] == 2)))
                    {
                        determineTeamsToSwap(teamCnt);
                        
                        swapGoldForWhiteFoursome(minGoldTeamA, maxGoldTeamA);
                    }
                    blindString= richTextBox2.Lines[5] + "(F) " +
                            richTextBox2.Lines[11].TrimStart('1', '2', '3', '4', '5', '6', '7', '8', '9', '0', ' ') + "(B)";

                    changeLine(richTextBox2, 17, blindString);
                    teamHcp = 0;
                    for (int cntr = 2; cntr < 6; cntr++)                    //4man team
                        teamHcp += calcTeamHcp(richTextBox2.Lines[cntr]);
                    changeLine(richTextBox2, 1, richTextBox2.Lines[1] + " HCP = " + teamHcp);

                    teamHcp = 0;
                    for (int cntr = 8; cntr < 12; cntr++)
                        teamHcp += calcTeamHcp(richTextBox2.Lines[cntr]);
                    changeLine(richTextBox2, 7, richTextBox2.Lines[7] + " HCP = " + teamHcp);
                    teamHcp = 0;
                    for (int cntr = 14; cntr < 17; cntr++)
                        teamHcp += calcTeamHcp(richTextBox2.Lines[cntr]);
                    
                    teamHcp += calcTeamHcp(richTextBox2.Lines[10]);
                    changeLine(richTextBox2, 13, richTextBox2.Lines[13] + " HCP = " + teamHcp);

                    displayTeamHCPcalc(teamCnt);

                    break;
                case 12:
                    if (MessageBox.Show("Do you want to play as THREESOMES?", "Check Information", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        richTextBox2.Text = "A-B-C-D DRAW for " + (playerCnt - 1).ToString() + " golfers\n";
                        richTextBox2.Text += "TEAM 1\n";                                        //1
                        richTextBox2.Text += richTextBox1.Lines[0];                             //2
                        if (richTextBox1.Lines[0].Contains("*")) team1g = 1;                    
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[7];                             //3
                        if (richTextBox1.Lines[7].Contains("*")) team1g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[8];                             //4
                        if (richTextBox1.Lines[8].Contains("*")) team1g++;
                        richTextBox2.Text += Environment.NewLine;
                        goldGolfers[0] = team1g;                                                

                        richTextBox2.Text += Environment.NewLine;                               //5

                        richTextBox2.Text += "TEAM 2\n";                                        //6 
                        richTextBox2.Text += richTextBox1.Lines[1];                             //7
                        if (richTextBox1.Lines[1].Contains("*")) team2g = 1;                    
                        richTextBox2.Text += Environment.NewLine;                               
                        richTextBox2.Text += richTextBox1.Lines[6];                             //8
                        if (richTextBox1.Lines[6].Contains("*")) team2g++;                      
                        richTextBox2.Text += Environment.NewLine;                               
                        richTextBox2.Text += richTextBox1.Lines[9];                             //9
                        if (richTextBox1.Lines[9].Contains("*")) team2g++;                      
                        richTextBox2.Text += Environment.NewLine;                               
                        goldGolfers[1] = team2g;                                               
                                                                                               
                        richTextBox2.Text += Environment.NewLine;                               //10

                        richTextBox2.Text += "TEAM 3\n";                                        //11 
                        richTextBox2.Text += richTextBox1.Lines[2];                             //12  
                        if (richTextBox1.Lines[2].Contains("*")) team3g = 1;                     
                        richTextBox2.Text += Environment.NewLine;                                
                        richTextBox2.Text += richTextBox1.Lines[5];                             //13 
                        if (richTextBox1.Lines[5].Contains("*")) team3g++;                       
                        richTextBox2.Text += Environment.NewLine;                               
                        richTextBox2.Text += richTextBox1.Lines[10];                            //14
                        if (richTextBox1.Lines[10].Contains("*")) team3g++;                       
                        richTextBox2.Text += Environment.NewLine;                                
                        goldGolfers[2] = team3g;                                                
                                                                                                  
                        richTextBox2.Text += Environment.NewLine;                               //15                               
                                                                                                
                        richTextBox2.Text += "TEAM 4\n";                                        //16  
                        richTextBox2.Text += richTextBox1.Lines[3];                             //17 
                        if (richTextBox1.Lines[3].Contains("*")) team4g = 1;                     
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[4];                             //18
                        if (richTextBox1.Lines[4].Contains("*")) team4g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[11];                            //19
                        if (richTextBox1.Lines[11].Contains("*")) team4g++;
                        richTextBox2.Text += Environment.NewLine;
                        goldGolfers[3] = team4g;
                        teamCnt = 4;

                        while (((goldGolfers[0] >= 2 || goldGolfers[1] >= 2 || goldGolfers[2] >= 2) && (goldGolfers[0] == 0 || goldGolfers[1] == 0 || goldGolfers[2] == 0))
                        || ((goldGolfers[0] >= 3 || goldGolfers[1] >= 3 || goldGolfers[2] >= 3) && (goldGolfers[0] == 1 || goldGolfers[1] == 1 || goldGolfers[2] == 1))
                        || ((goldGolfers[0] == 4 || goldGolfers[1] == 4 || goldGolfers[2] == 4) && (goldGolfers[0] == 2 || goldGolfers[1] == 2 || goldGolfers[2] == 2)))
                        {
                            determineTeamsToSwap(teamCnt);
                            
                            swapGoldForWhiteThreesome();
                        }
                        //Use the +10 to force the program path dow a 3-Person team display
                        displayTeamHCPcalc(teamCnt + 10);
                    }
                    else
                    {
                        teamCnt = 3;
                        richTextBox2.Text = "A-B-C-D DRAW for " + (playerCnt - 1).ToString() + " golfers\n";
                        richTextBox2.Text += "TEAM 1\n";                                            //1
                        richTextBox2.Text += richTextBox1.Lines[0];                                 //2
                        if (richTextBox1.Lines[0].Contains("*")) team1g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[5];                                 //3
                        if (richTextBox1.Lines[5].Contains("*")) team1g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[6];                                 //4
                        if (richTextBox1.Lines[6].Contains("*")) team1g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[11];                                //5
                        if (richTextBox1.Lines[11].Contains("*")) team1g++;
                        richTextBox2.Text += Environment.NewLine;                                   
                        goldGolfers[0] = team1g;
                                                                                                     
                        richTextBox2.Text += Environment.NewLine;                                   //6

                        richTextBox2.Text += "TEAM 2\n";                                            //7
                        richTextBox2.Text += richTextBox1.Lines[1];                                 //8
                        if (richTextBox1.Lines[1].Contains("*")) team2g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[4];                                 //9
                        if (richTextBox1.Lines[4].Contains("*")) team2g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[7];                                 //10
                        if (richTextBox1.Lines[7].Contains("*")) team2g++;
                        richTextBox2.Text += Environment.NewLine;                                   
                        richTextBox2.Text += richTextBox1.Lines[10];                                //11
                        if (richTextBox1.Lines[10].Contains("*")) team2g++;                         
                        richTextBox2.Text += Environment.NewLine;                                   
                        goldGolfers[1] = team2g;                                                     
                                                                                                     
                        richTextBox2.Text += Environment.NewLine;                                   //12
                                                                                                     
                        richTextBox2.Text += "TEAM 3\n";                                            //13
                        richTextBox2.Text += richTextBox1.Lines[2];                                 //14
                        if (richTextBox1.Lines[2].Contains("*")) team3g++;                            
                        richTextBox2.Text += Environment.NewLine;                                    
                        richTextBox2.Text += richTextBox1.Lines[3];                                 //15
                        if (richTextBox1.Lines[3].Contains("*")) team3g++;                            
                        richTextBox2.Text += Environment.NewLine;                                   
                        richTextBox2.Text += richTextBox1.Lines[8];                                 //16
                        if (richTextBox1.Lines[8].Contains("*")) team3g++;                           
                        richTextBox2.Text += Environment.NewLine;                                   
                        richTextBox2.Text += richTextBox1.Lines[9];                                 //17
                        if (richTextBox1.Lines[9].Contains("*")) team3g++;
                        goldGolfers[2] = team3g;                                                                                                                                     

                        while (((goldGolfers[0] >= 2 || goldGolfers[1] >= 2 || goldGolfers[2] >= 2) && (goldGolfers[0] == 0 || goldGolfers[1] == 0 || goldGolfers[2] == 0))
                        || ((goldGolfers[0] >= 3 || goldGolfers[1] >= 3 || goldGolfers[2] >= 3) && (goldGolfers[0] == 1 || goldGolfers[1] == 1 || goldGolfers[2] == 1))
                        || ((goldGolfers[0] == 4 || goldGolfers[1] == 4 || goldGolfers[2] == 4) && (goldGolfers[0] == 2 || goldGolfers[1] == 2 || goldGolfers[2] == 2)))
                        {
                            determineTeamsToSwap(teamCnt);

                            
                            swapGoldForWhiteFoursome(minGoldTeamA, maxGoldTeamA);                            
                        }
                        displayTeamHCPcalc(teamCnt);
                    }
                    break;
                case 13:
                    richTextBox2.Text = "A-B-C-D DRAW for " + (playerCnt - 1).ToString() + " golfers\n";
                    richTextBox2.Text += "TEAM 1\n";                                            //1
                    richTextBox2.Text += richTextBox1.Lines[0];                                 //2
                    if (richTextBox1.Lines[0].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[7];                                 //3
                    if (richTextBox1.Lines[7].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[8];                                 //4
                    if (richTextBox1.Lines[8].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += "BLIND\n";                                             //5
                    goldGolfers[0] = team1g;                                                    

                    richTextBox2.Text += Environment.NewLine;                                   //6                

                    richTextBox2.Text += "TEAM 2\n";                                            //7
                    richTextBox2.Text += richTextBox1.Lines[1];                                 //8
                    if (richTextBox1.Lines[1].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;                                   
                    richTextBox2.Text += richTextBox1.Lines[6];                                 //9
                    if (richTextBox1.Lines[6].Contains("*")) team2g++;                          
                    richTextBox2.Text += Environment.NewLine;                                   
                    richTextBox2.Text += richTextBox1.Lines[9];                                 //10
                    if (richTextBox1.Lines[9].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;                                   
                    richTextBox2.Text += "BLIND\n";                                             //11
                    goldGolfers[1] = team2g;
                                                                                                //12
                    richTextBox2.Text += Environment.NewLine;                                   

                    richTextBox2.Text += "TEAM 3\n";                                            //13
                    richTextBox2.Text += richTextBox1.Lines[2];                                 //14
                    if (richTextBox1.Lines[2].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;                                   
                    richTextBox2.Text += richTextBox1.Lines[5];                                 //15
                    if (richTextBox1.Lines[5].Contains("*")) team3g++;                          
                    richTextBox2.Text += Environment.NewLine;                                   
                    richTextBox2.Text += richTextBox1.Lines[10];                                //16
                    if (richTextBox1.Lines[10].Contains("*")) team3g++;                         
                    richTextBox2.Text += Environment.NewLine;                                   
                    richTextBox2.Text += "BLIND\n";                                             //17
                    goldGolfers[2] = team3g;                                                    
                                                                                                
                    richTextBox2.Text += Environment.NewLine;                                   //18
                                                                                                
                    richTextBox2.Text += "TEAM 4\n";                                            //19
                    richTextBox2.Text += richTextBox1.Lines[3];                                 //20
                    if (richTextBox1.Lines[3].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[4];                                 //21
                    if (richTextBox1.Lines[4].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[11];                                //22
                    if (richTextBox1.Lines[11].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[12];                                //23
                    if (richTextBox1.Lines[12].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[3] = team4g;                                                    //24
                    teamCnt = 4;

                    // If there are multiple teams that have 3 Gold and One White run the swap again
                    while (((goldGolfers[0] >= 2 || goldGolfers[1] >= 2 || goldGolfers[2] >= 2 || goldGolfers[3] >= 2) && (goldGolfers[0] == 0 || goldGolfers[1] == 0 || goldGolfers[2] == 0 || goldGolfers[3] == 0))
                        || ((goldGolfers[0] >= 3 || goldGolfers[1] >= 3 || goldGolfers[2] >= 3 || goldGolfers[3] >= 3) && (goldGolfers[0] == 1 || goldGolfers[1] == 1 || goldGolfers[2] == 1 || goldGolfers[3] == 1))
                        || ((goldGolfers[0] == 4 || goldGolfers[1] == 4 || goldGolfers[2] == 4 || goldGolfers[3] == 4) && (goldGolfers[0] == 2 || goldGolfers[1] == 2 || goldGolfers[2] == 2 || goldGolfers[3] == 2)))
                    {
                        determineTeamsToSwap(teamCnt);

                        swapGoldForWhiteFoursome(minGoldTeamA, maxGoldTeamA);
                    }

                    blindString = richTextBox2.Lines[22] + "(F) " +
                        richTextBox2.Lines[23].TrimStart('1', '2', '3', '4', '5', '6', '7', '8', '9', '0', ' ') + "(B)";
                    changeLine(richTextBox2, 5, blindString);
                    changeLine(richTextBox2, 11, blindString);
                    changeLine(richTextBox2, 17, blindString);

                    displayTeamHCPcalc(teamCnt);
                    break;
                case 14:
                    richTextBox2.Text = "A-B-C-D DRAW for " + (playerCnt - 1).ToString() + " golfers\n";
                    richTextBox2.Text += "TEAM 1\n";                                //1
                    richTextBox2.Text += richTextBox1.Lines[0];                     //2
                    if (richTextBox1.Lines[0].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[7];                     //3
                    if (richTextBox1.Lines[7].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[8];                     //4
                    if (richTextBox1.Lines[8].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += "BLIND\n";                                 //5            
                    goldGolfers[0] = team1g;

                    richTextBox2.Text += Environment.NewLine;                       //6 

                    richTextBox2.Text += "TEAM 2\n";                                //7
                    richTextBox2.Text += richTextBox1.Lines[1];                     //8
                    if (richTextBox1.Lines[1].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[6];                     //9
                    if (richTextBox1.Lines[6].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[9];                     //10
                    if (richTextBox1.Lines[9].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += "BLIND\n";                                 //11            
                    goldGolfers[1] = team2g;
                    teamCnt = 2;                                                    

                    richTextBox2.Text += Environment.NewLine;                       //12

                    richTextBox2.Text += "TEAM 3\n";                                //13
                    richTextBox2.Text += richTextBox1.Lines[2];                     //14
                    if (richTextBox1.Lines[2].Contains("*")) team3g++;              
                    richTextBox2.Text += Environment.NewLine;                       
                    richTextBox2.Text += richTextBox1.Lines[5];                     //15
                    if (richTextBox1.Lines[5].Contains("*")) team3g++;              
                    richTextBox2.Text += Environment.NewLine;                       
                    richTextBox2.Text += richTextBox1.Lines[10];                    //16
                    if (richTextBox1.Lines[10].Contains("*")) team3g++;             
                    richTextBox2.Text += Environment.NewLine;                       
                    richTextBox2.Text += richTextBox1.Lines[13];                    //17
                    if (richTextBox1.Lines[13].Contains("*")) team3g++;             
                    richTextBox2.Text += Environment.NewLine;                       
                    goldGolfers[2] = team3g;                                        
                                                                                    
                    richTextBox2.Text += Environment.NewLine;                       //18

                    richTextBox2.Text += "TEAM 4\n";                                //19
                    richTextBox2.Text += richTextBox1.Lines[3];                     //20
                    if (richTextBox1.Lines[3].Contains("*")) team4g++;              
                    richTextBox2.Text += Environment.NewLine;                       
                    richTextBox2.Text += richTextBox1.Lines[4];                     //21
                    if (richTextBox1.Lines[4].Contains("*")) team4g++;              
                    richTextBox2.Text += Environment.NewLine;                       
                    richTextBox2.Text += richTextBox1.Lines[11];                    //22
                    if (richTextBox1.Lines[11].Contains("*")) team4g++;             
                    richTextBox2.Text += Environment.NewLine;                       
                    richTextBox2.Text += richTextBox1.Lines[12];                    //23
                    if (richTextBox1.Lines[12].Contains("*")) team4g++;             
                    richTextBox2.Text += Environment.NewLine;                       
                    goldGolfers[3] = team4g;
                    teamCnt = 4;

                    // If there are multiple teams that have 3 Gold and One White run the swap again
                    while (((goldGolfers[0] >= 2 || goldGolfers[1] >= 2 || goldGolfers[2] >= 2 || goldGolfers[3] >= 2) && (goldGolfers[0] == 0 || goldGolfers[1] == 0 || goldGolfers[2] == 0 || goldGolfers[3] == 0))
                        || ((goldGolfers[0] >= 3 || goldGolfers[1] >= 3 || goldGolfers[2] >= 3 || goldGolfers[3] >= 3) && (goldGolfers[0] == 1 || goldGolfers[1] == 1 || goldGolfers[2] == 1 || goldGolfers[3] == 1))
                        || ((goldGolfers[0] == 4 || goldGolfers[1] == 4 || goldGolfers[2] == 4 || goldGolfers[3] == 4) && (goldGolfers[0] == 2 || goldGolfers[1] == 2 || goldGolfers[2] == 2 || goldGolfers[3] == 2)))
                    {
                        determineTeamsToSwap(teamCnt);

                        swapGoldForWhiteFoursome(minGoldTeamA, maxGoldTeamA);
                    }

                    blindString = richTextBox2.Lines[17] + "(F) " +
                            richTextBox2.Lines[23].TrimStart('1', '2', '3', '4', '5', '6', '7', '8', '9', '0', ' ') + "(B)";

                    changeLine(richTextBox2, 5, blindString);
                    changeLine(richTextBox2, 11, blindString);

                    displayTeamHCPcalc(teamCnt);
                    break;
                case 15:
                    richTextBox2.Text = "A-B-C-D DRAW for " + (playerCnt - 1).ToString() + " golfers\n";
                    richTextBox2.Text += "TEAM 1\n";                            //1
                    richTextBox2.Text += richTextBox1.Lines[0];                 //2
                    if (richTextBox1.Lines[0].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[9];                 //3
                    if (richTextBox1.Lines[9].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[10];                //4
                    if (richTextBox1.Lines[10].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[0] = team1g;

                    richTextBox2.Text += Environment.NewLine;                   //5

                    richTextBox2.Text += "TEAM 2\n";                            //6 
                    richTextBox2.Text += richTextBox1.Lines[1];                 //7
                    if (richTextBox1.Lines[1].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[8];                 //8
                    if (richTextBox1.Lines[8].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[11];                //9
                    if (richTextBox1.Lines[11].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[1] = team2g;

                    richTextBox2.Text += Environment.NewLine;                   //10

                    richTextBox2.Text += "TEAM 3\n";                            //11 
                    richTextBox2.Text += richTextBox1.Lines[2];                 //12 
                    if (richTextBox1.Lines[2].Contains("*")) team3g++;           
                    richTextBox2.Text += Environment.NewLine;                    
                    richTextBox2.Text += richTextBox1.Lines[7];                 //13 
                    if (richTextBox1.Lines[7].Contains("*")) team3g++;           
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[12];                //14
                    if (richTextBox1.Lines[12].Contains("*")) team3g++;           
                    richTextBox2.Text += Environment.NewLine;                    
                    goldGolfers[2] = team3g;
                                                                                  
                    richTextBox2.Text += Environment.NewLine;                   //15 
                    
                    richTextBox2.Text += "TEAM 4\n";                            //16 
                    richTextBox2.Text += richTextBox1.Lines[3];                 //17 
                    if (richTextBox1.Lines[3].Contains("*")) team4g++;           
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[6];                 //18
                    if (richTextBox1.Lines[6].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[13];                //19
                    if (richTextBox1.Lines[13].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[3] = team4g;

                    richTextBox2.Text += Environment.NewLine;                   //20

                    richTextBox2.Text += "TEAM 5\n";                            //21
                    richTextBox2.Text += richTextBox1.Lines[4];                 //22
                    if (richTextBox1.Lines[4].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[5];                 //23
                    if (richTextBox1.Lines[5].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[14];                //24
                    if (richTextBox1.Lines[14].Contains("*")) team5g++;
                    goldGolfers[4] = team5g;
                    teamCnt = 5;

                    // If there are multiple teams that have 3 Gold and One White run the swap again
                    while (((goldGolfers[0] >= 2 || goldGolfers[1] >= 2 || goldGolfers[2] >= 2 || goldGolfers[3] >= 2 || goldGolfers[4] >= 2) && (goldGolfers[0] == 0 || goldGolfers[1] == 0 || goldGolfers[2] == 0 || goldGolfers[3] == 0 || goldGolfers[4] == 0))
                        || ((goldGolfers[0] >= 3 || goldGolfers[1] >= 3 || goldGolfers[2] >= 3 || goldGolfers[3] >= 3 || goldGolfers[4] >= 3) && (goldGolfers[0] == 1 || goldGolfers[1] == 1 || goldGolfers[2] == 1 || goldGolfers[3] == 1 || goldGolfers[4] == 1))
                        || ((goldGolfers[0] == 4 || goldGolfers[1] == 4 || goldGolfers[2] == 4 || goldGolfers[3] == 4 || goldGolfers[4] == 4) && (goldGolfers[0] == 2 || goldGolfers[1] == 2 || goldGolfers[2] == 2 || goldGolfers[3] == 2 || goldGolfers[4] == 2)))
                    {
                        determineTeamsToSwap(teamCnt);
                        swapGoldForWhiteThreesome();
                    }
                    //Use the +10 to force the program path dow a 3-Person team display
                    displayTeamHCPcalc(teamCnt + 10);
                    break;
                case 16:
                    richTextBox2.Text = "A-B-C-D DRAW for " + (playerCnt - 1).ToString() + " golfers\n";
                    richTextBox2.Text += "TEAM 1\n";
                    richTextBox2.Text += richTextBox1.Lines[0];
                    if (richTextBox1.Lines[0].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[7];
                    if (richTextBox1.Lines[7].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[8];
                    if (richTextBox1.Lines[8].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[15];
                    if (richTextBox1.Lines[15].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[0] = team1g;

                    richTextBox2.Text += Environment.NewLine;

                    richTextBox2.Text += "TEAM 2\n";
                    richTextBox2.Text += richTextBox1.Lines[1];
                    if (richTextBox1.Lines[1].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[6];
                    if (richTextBox1.Lines[6].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[9];
                    if (richTextBox1.Lines[9].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[14];
                    if (richTextBox1.Lines[14].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[1] = team2g;

                    richTextBox2.Text += Environment.NewLine;

                    richTextBox2.Text += "TEAM 3\n";
                    richTextBox2.Text += richTextBox1.Lines[2];
                    if (richTextBox1.Lines[2].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[5];
                    if (richTextBox1.Lines[5].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[10];
                    if (richTextBox1.Lines[10].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[13];
                    if (richTextBox1.Lines[13].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine; 
                    goldGolfers[2] = team3g;

                    richTextBox2.Text += Environment.NewLine;

                    richTextBox2.Text += "TEAM 4\n";
                    richTextBox2.Text += richTextBox1.Lines[3];
                    if (richTextBox1.Lines[3].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[4];
                    if (richTextBox1.Lines[4].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[11];
                    if (richTextBox1.Lines[11].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[12];
                    if (richTextBox1.Lines[12].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[3] = team4g;
                    teamCnt = 4;

                    // If there are multiple teams that have 3 Gold and One White run the swap again
                    while (((goldGolfers[0] >= 2 || goldGolfers[1] >= 2 || goldGolfers[2] >= 2 || goldGolfers[3] >= 2) && (goldGolfers[0] == 0 || goldGolfers[1] == 0 || goldGolfers[2] == 0 || goldGolfers[3] == 0))
                        || ((goldGolfers[0] >= 3 || goldGolfers[1] >= 3 || goldGolfers[2] >= 3 || goldGolfers[3] >= 3) && (goldGolfers[0] == 1 || goldGolfers[1] == 1 || goldGolfers[2] == 1 || goldGolfers[3] == 1))
                        || ((goldGolfers[0] == 4 || goldGolfers[1] == 4 || goldGolfers[2] == 4 || goldGolfers[3] == 4) && (goldGolfers[0] == 2 || goldGolfers[1] == 2 || goldGolfers[2] == 2 || goldGolfers[3] == 2)))
                    {
                        determineTeamsToSwap(teamCnt);

                        swapGoldForWhiteFoursome(minGoldTeamA, maxGoldTeamA);
                    }

                    displayTeamHCPcalc(teamCnt);
                    break;
                case 17:
                    richTextBox2.Text = "A-B-C-D DRAW for " + (playerCnt - 1).ToString() + " golfers\n";
                    richTextBox2.Text += "TEAM 1\n";                                    //1
                    richTextBox2.Text += richTextBox1.Lines[0];                         //2
                    if (richTextBox1.Lines[0].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[9];                         //3
                    if (richTextBox1.Lines[9].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[10];                        //4
                    if (richTextBox1.Lines[10].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += "BLIND\n";                                     //5
                    goldGolfers[0] = team1g;

                    richTextBox2.Text += Environment.NewLine;                           //6

                    richTextBox2.Text += "TEAM 2\n";                                    //7
                    richTextBox2.Text += richTextBox1.Lines[1];                         //8
                    if (richTextBox1.Lines[1].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[8];                         //9
                    if (richTextBox1.Lines[8].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[11];                        //10
                    if (richTextBox1.Lines[11].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += "BLIND\n";                                     //11       
                    goldGolfers[1] = team2g;

                    richTextBox2.Text += Environment.NewLine;                           //12

                    richTextBox2.Text += "TEAM 3\n";                                    //13
                    richTextBox2.Text += richTextBox1.Lines[2];                         //14
                    if (richTextBox1.Lines[2].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[7];                         //15
                    if (richTextBox1.Lines[7].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[12];                        //16
                    if (richTextBox1.Lines[12].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += "BLIND\n";                                     //17       
                    goldGolfers[2] = team3g;
                    teamCnt = 3;
                                                                                        //18
                    richTextBox2.Text += Environment.NewLine;

                    richTextBox2.Text += "TEAM 4\n";                                    //19
                    richTextBox2.Text += richTextBox1.Lines[3];                         //20
                    if (richTextBox1.Lines[3].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[6];                         //21
                    if (richTextBox1.Lines[6].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[13];                        //22
                    if (richTextBox1.Lines[13].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[16];                        //23
                    if (richTextBox1.Lines[16].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[3] = team4g;                                            //24
                    teamCnt = 4;

                    richTextBox2.Text += Environment.NewLine;                           //25

                    richTextBox2.Text += "TEAM 5\n";                                    //26
                    richTextBox2.Text += richTextBox1.Lines[4];                         //27
                    if (richTextBox1.Lines[4].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[5];                         //28
                    if (richTextBox1.Lines[5].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[14];                        //29
                    if (richTextBox1.Lines[14].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[15];                        //30
                    if (richTextBox1.Lines[15].Contains("*")) team5g++;
                    goldGolfers[4] = team5g;
                    teamCnt = 5;

                    // If there are multiple teams that have 3 Gold and One White run the swap again
                    while (((goldGolfers[0] >= 2 || goldGolfers[1] >= 2 || goldGolfers[2] >= 2 || goldGolfers[3] >= 2 || goldGolfers[4] >= 2) && (goldGolfers[0] == 0 || goldGolfers[1] == 0 || goldGolfers[2] == 0 || goldGolfers[3] == 0 || goldGolfers[4] == 0))
                        || ((goldGolfers[0] >= 3 || goldGolfers[1] >= 3 || goldGolfers[2] >= 3 || goldGolfers[3] >= 3 || goldGolfers[4] >= 3) && (goldGolfers[0] == 1 || goldGolfers[1] == 1 || goldGolfers[2] == 1 || goldGolfers[3] == 1 || goldGolfers[4] == 1))
                        || ((goldGolfers[0] == 4 || goldGolfers[1] == 4 || goldGolfers[2] == 4 || goldGolfers[3] == 4 || goldGolfers[4] == 4) && (goldGolfers[0] == 2 || goldGolfers[1] == 2 || goldGolfers[2] == 2 || goldGolfers[3] == 2 || goldGolfers[4] == 2)))
                    {
                        determineTeamsToSwap(teamCnt);

                        swapGoldForWhiteFoursome(minGoldTeamA, maxGoldTeamA);
                    }

                    //Make the BLIND after the teams are balanced for GOLD/WHITE
                    blindString = richTextBox2.Lines[23] + "(F) " +
                            richTextBox2.Lines[24].TrimStart('1', '2', '3', '4', '5', '6', '7', '8', '9', '0', ' ') + "(B)";

                    changeLine(richTextBox2, 5, blindString);
                    changeLine(richTextBox2, 11, blindString);
                    changeLine(richTextBox2, 17, blindString);

                    displayTeamHCPcalc(teamCnt); 
                    break;
                case 18:
                    richTextBox2.Text = "A-B-C-D DRAW for " + (playerCnt - 1).ToString() + " golfers\n";
                    richTextBox2.Text += "TEAM 1\n";                                //1
                    richTextBox2.Text += richTextBox1.Lines[0];                     //2
                    if (richTextBox1.Lines[0].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[11];                    //3
                    if (richTextBox1.Lines[11].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[12];                    //4
                    if (richTextBox1.Lines[12].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[0] = team1g;

                    richTextBox2.Text += Environment.NewLine;                       //5

                    richTextBox2.Text += "TEAM 2\n";                                //6 
                    richTextBox2.Text += richTextBox1.Lines[1];                     //7
                    if (richTextBox1.Lines[1].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[10];                    //8
                    if (richTextBox1.Lines[10].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[13];                    //9
                    if (richTextBox1.Lines[13].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[1] = team2g;

                    richTextBox2.Text += Environment.NewLine;                       //10

                    richTextBox2.Text += "TEAM 3\n";                                //11
                    richTextBox2.Text += richTextBox1.Lines[2];                     //12
                    if (richTextBox1.Lines[2].Contains("*")) team3g++;               
                    richTextBox2.Text += Environment.NewLine;                        
                    richTextBox2.Text += richTextBox1.Lines[9];                     //13
                    if (richTextBox1.Lines[9].Contains("*")) team3g++;               
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[14];                    //14
                    if (richTextBox1.Lines[14].Contains("*")) team3g++;               
                    richTextBox2.Text += Environment.NewLine;                        
                    goldGolfers[2] = team3g;
                                                                                      
                    richTextBox2.Text += Environment.NewLine;                       //15

                    richTextBox2.Text += "TEAM 4\n";                                //16
                    richTextBox2.Text += richTextBox1.Lines[3];                     //17
                    if (richTextBox1.Lines[3].Contains("*")) team4g++;               
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[8];                     //18
                    if (richTextBox1.Lines[8].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[15];                    //19
                    if (richTextBox1.Lines[15].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[3] = team4g;

                    richTextBox2.Text += Environment.NewLine;                       //20

                    richTextBox2.Text += "TEAM 5\n";                                //21
                    richTextBox2.Text += richTextBox1.Lines[4];                     //22
                    if (richTextBox1.Lines[4].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[7];                     //23
                    if (richTextBox1.Lines[7].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[16];                    //24
                    if (richTextBox1.Lines[16].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine; 
                    goldGolfers[4] = team5g;

                    richTextBox2.Text += Environment.NewLine;                       //25

                    richTextBox2.Text += "TEAM 6\n";                                //26
                    richTextBox2.Text += richTextBox1.Lines[5];                     //27
                    if (richTextBox1.Lines[5].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[6];                     //28
                    if (richTextBox1.Lines[6].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[17];                    //29
                    if (richTextBox1.Lines[17].Contains("*")) team6g++;
                    goldGolfers[5] = team6g;
                    teamCnt = 6;

                    // If there are multiple teams that have 3 Gold and One White run the swap again
                    while (((goldGolfers[0] >= 2 || goldGolfers[1] >= 2 || goldGolfers[2] >= 2 || goldGolfers[3] >= 2 || goldGolfers[4] >= 2 || goldGolfers[5] >= 2) && (goldGolfers[0] == 0 || goldGolfers[1] == 0 || goldGolfers[2] == 0 || goldGolfers[3] == 0 || goldGolfers[4] == 0 || goldGolfers[5] == 0))
                        || ((goldGolfers[0] >= 3 || goldGolfers[1] >= 3 || goldGolfers[2] >= 3 || goldGolfers[3] >= 3 || goldGolfers[4] >= 3 || goldGolfers[5] >= 3) && (goldGolfers[0] == 1 || goldGolfers[1] == 1 || goldGolfers[2] == 1 || goldGolfers[3] == 1 || goldGolfers[4] == 1 || goldGolfers[5] == 1))
                        || ((goldGolfers[0] == 4 || goldGolfers[1] == 4 || goldGolfers[2] == 4 || goldGolfers[3] == 4 || goldGolfers[4] == 4 || goldGolfers[5] == 4) && (goldGolfers[0] == 2 || goldGolfers[1] == 2 || goldGolfers[2] == 2 || goldGolfers[3] == 2 || goldGolfers[4] == 2 || goldGolfers[5] == 2)))
                    {
                        determineTeamsToSwap(teamCnt);

                        swapGoldForWhiteThreesome();
                    }
                    displayTeamHCPcalc(teamCnt + 10);

                    break;
                case 19:
                    richTextBox2.Text = "A-B-C-D DRAW for " + (playerCnt - 1).ToString() + " golfers\n";
                    richTextBox2.Text += "TEAM 1\n";                                        //1
                    richTextBox2.Text += richTextBox1.Lines[4];                             //2
                    if (richTextBox1.Lines[4].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[5];                             //3
                    if (richTextBox1.Lines[5].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[14];                            //4
                    if (richTextBox1.Lines[14].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[15];                            //5
                    if (richTextBox1.Lines[15].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[0] = team1g;                                                

                    richTextBox2.Text += Environment.NewLine;                               //6

                    richTextBox2.Text += "TEAM 2\n";                                        //7
                    richTextBox2.Text += richTextBox1.Lines[1];                             //8
                    if (richTextBox1.Lines[1].Contains("*")) team2g++;                      
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[8];                             //9
                    if (richTextBox1.Lines[8].Contains("*")) team2g++;                      
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[11];                            //10
                    if (richTextBox1.Lines[11].Contains("*")) team2g++;                     
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[18];                            //11 
                    if (richTextBox1.Lines[18].Contains("*")) team2g++;                     
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[1] = team2g;                                                
                                                                                            
                    richTextBox2.Text += Environment.NewLine;                               //12

                    richTextBox2.Text += "TEAM 3\n";                                        //13
                    richTextBox2.Text += richTextBox1.Lines[2];                             //14
                    if (richTextBox1.Lines[2].Contains("*")) team3g++;                      
                    richTextBox2.Text += Environment.NewLine;                               
                    richTextBox2.Text += richTextBox1.Lines[7];                             //15
                    if (richTextBox1.Lines[7].Contains("*")) team3g++;                      
                    richTextBox2.Text += Environment.NewLine;                               
                    richTextBox2.Text += richTextBox1.Lines[12];                            //16
                    if (richTextBox1.Lines[12].Contains("*")) team3g++;                     
                    richTextBox2.Text += Environment.NewLine;                               
                    richTextBox2.Text += richTextBox1.Lines[17];                            //17 
                    if (richTextBox1.Lines[17].Contains("*")) team3g++;                     
                    richTextBox2.Text += Environment.NewLine;                               
                    goldGolfers[2] = team3g;                                                
                                                                                            
                    richTextBox2.Text += Environment.NewLine;                               //18
                                                                                            
                    richTextBox2.Text += "TEAM 4\n";                                        //19
                    richTextBox2.Text += richTextBox1.Lines[3];                             //20
                    if (richTextBox1.Lines[3].Contains("*")) team4g++;                      
                    richTextBox2.Text += Environment.NewLine;                               
                    richTextBox2.Text += richTextBox1.Lines[6];                             //21
                    if (richTextBox1.Lines[6].Contains("*")) team4g++;                      
                    richTextBox2.Text += Environment.NewLine;                               
                    richTextBox2.Text += richTextBox1.Lines[13];                            //22
                    if (richTextBox1.Lines[13].Contains("*")) team4g++;                     
                    richTextBox2.Text += Environment.NewLine;                               
                    richTextBox2.Text += richTextBox1.Lines[16];                            //23
                    if (richTextBox1.Lines[16].Contains("*")) team4g++;                     
                    richTextBox2.Text += Environment.NewLine;                               
                    goldGolfers[3] = team4g;                                                
                                                                                            
                    richTextBox2.Text += Environment.NewLine;                               //24

                    richTextBox2.Text += "TEAM 5\n";                                        //25
                    richTextBox2.Text += richTextBox1.Lines[0];                             //26
                    if (richTextBox1.Lines[0].Contains("*")) team5g++;                      
                    richTextBox2.Text += Environment.NewLine;                               
                    richTextBox2.Text += richTextBox1.Lines[9];                             //27
                    if (richTextBox1.Lines[9].Contains("*")) team5g++;                      
                    richTextBox2.Text += Environment.NewLine;                               
                    richTextBox2.Text += richTextBox1.Lines[10];                            //28
                    if (richTextBox1.Lines[10].Contains("*")) team5g++;                     
                    richTextBox2.Text += Environment.NewLine;                               
                                                                                            //29
                    rIntF = rnd.Next(15, 19);
                    do
                    {
                        rIntB = rnd.Next(15, 19);
                    } while (rIntF == rIntB);

                    //Make the BLIND before the teams are balanced for GOLD/WHITEE                    
                    richTextBox2.Text += richTextBox1.Lines[rIntF] + "(F) " +
                            richTextBox1.Lines[rIntB].TrimStart('1', '2', '3', '4', '5', '6', '7', '8', '9', '0', ' ') + "(B)";
                    //Change the BLIND LINE and Correct for GOLD player if necessary                    
                    if (richTextBox2.Lines[29].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[4] = team5g;
                    teamCnt = 5;

                    // If there are multiple teams that have 3 Gold and One White run the swap again
                    while (((goldGolfers[0] >= 2 || goldGolfers[1] >= 2 || goldGolfers[2] >= 2 || goldGolfers[3] >= 2 || goldGolfers[4] >= 2) && (goldGolfers[0] == 0 || goldGolfers[1] == 0 || goldGolfers[2] == 0 || goldGolfers[3] == 0 || goldGolfers[4] == 0))
                        || ((goldGolfers[0] >= 3 || goldGolfers[1] >= 3 || goldGolfers[2] >= 3 || goldGolfers[3] >= 3 || goldGolfers[4] >= 3) && (goldGolfers[0] == 1 || goldGolfers[1] == 1 || goldGolfers[2] == 1 || goldGolfers[3] == 1 || goldGolfers[4] == 1))
                        || ((goldGolfers[0] == 4 || goldGolfers[1] == 4 || goldGolfers[2] == 4 || goldGolfers[3] == 4 || goldGolfers[4] == 4) && (goldGolfers[0] == 2 || goldGolfers[1] == 2 || goldGolfers[2] == 2 || goldGolfers[3] == 2 || goldGolfers[4] == 2)))
                    {
                        determineTeamsToSwap(teamCnt);

                        swapGoldForWhiteFoursome(minGoldTeamA, maxGoldTeamA);
                    }


                    displayTeamHCPcalc(teamCnt);

                    break;
                case 20:
                    richTextBox2.Text = "A-B-C-D DRAW for " + (playerCnt - 1).ToString() + " golfers\n";
                    richTextBox2.Text += "TEAM 1\n";                            //1
                    richTextBox2.Text += richTextBox1.Lines[0];                 //2
                    if (richTextBox1.Lines[0].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[9];                 //3
                    if (richTextBox1.Lines[9].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[10];                //4
                    if (richTextBox1.Lines[10].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[19];                //5
                    if (richTextBox1.Lines[19].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[0] = team1g;

                    richTextBox2.Text += Environment.NewLine;                   //6

                    richTextBox2.Text += "TEAM 2\n";                            //7
                    richTextBox2.Text += richTextBox1.Lines[1];                 //8
                    if (richTextBox1.Lines[1].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[8];                 //9
                    if (richTextBox1.Lines[8].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[11];                //10
                    if (richTextBox1.Lines[11].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[18];                //11 
                    if (richTextBox1.Lines[18].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[1] = team2g;

                    richTextBox2.Text += Environment.NewLine;                   //12

                    richTextBox2.Text += "TEAM 3\n";                            //13
                    richTextBox2.Text += richTextBox1.Lines[2];                 //14
                    if (richTextBox1.Lines[2].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[7];                 //15
                    if (richTextBox1.Lines[7].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[12];                //16
                    if (richTextBox1.Lines[12].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[17];                //17 
                    if (richTextBox1.Lines[17].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[2] = team3g;

                    richTextBox2.Text += Environment.NewLine;                   //18

                    richTextBox2.Text += "TEAM 4\n";                            //19
                    richTextBox2.Text += richTextBox1.Lines[3];                 //20
                    if (richTextBox1.Lines[3].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[6];                 //21
                    if (richTextBox1.Lines[6].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[13];                //22
                    if (richTextBox1.Lines[13].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[16];                //23
                    if (richTextBox1.Lines[16].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[3] = team4g;

                    richTextBox2.Text += Environment.NewLine;                   //24

                    richTextBox2.Text += "TEAM 5\n";                            //25
                    richTextBox2.Text += richTextBox1.Lines[4];                 //26
                    if (richTextBox1.Lines[4].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[5];                 //27
                    if (richTextBox1.Lines[5].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[14];                //28
                    if (richTextBox1.Lines[14].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[15];                //29
                    if (richTextBox1.Lines[15].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[4] = team5g;
                    teamCnt = 5;

                    // If there are multiple teams that have 3 Gold and One White run the swap again
                    while (((goldGolfers[0] >= 2 || goldGolfers[1] >= 2 || goldGolfers[2] >= 2 || goldGolfers[3] >= 2 || goldGolfers[4] >= 2) && (goldGolfers[0] == 0 || goldGolfers[1] == 0 || goldGolfers[2] == 0 || goldGolfers[3] == 0 || goldGolfers[4] == 0))
                        || ((goldGolfers[0] >= 3 || goldGolfers[1] >= 3 || goldGolfers[2] >= 3 || goldGolfers[3] >= 3 || goldGolfers[4] >= 3) && (goldGolfers[0] == 1 || goldGolfers[1] == 1 || goldGolfers[2] == 1 || goldGolfers[3] == 1 || goldGolfers[4] == 1))
                        || ((goldGolfers[0] == 4 || goldGolfers[1] == 4 || goldGolfers[2] == 4 || goldGolfers[3] == 4 || goldGolfers[4] == 4) && (goldGolfers[0] == 2 || goldGolfers[1] == 2 || goldGolfers[2] == 2 || goldGolfers[3] == 2 || goldGolfers[4] == 2)))
                    {
                        determineTeamsToSwap(teamCnt);

                        swapGoldForWhiteFoursome(minGoldTeamA, maxGoldTeamA);
                    }

                    displayTeamHCPcalc(teamCnt);
                    break;
                case 21:
                    richTextBox2.Text = "A-B-C-D DRAW for " + (playerCnt - 1).ToString() + " golfers\n";
                    richTextBox2.Text += "TEAM 1\n";                                //1
                    richTextBox2.Text += richTextBox1.Lines[0];                     //2
                    if (richTextBox1.Lines[0].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[13];                    //3
                    if (richTextBox1.Lines[13].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[14];                    //4
                    if (richTextBox1.Lines[14].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[0] = team1g;

                    richTextBox2.Text += Environment.NewLine;                       //5

                    richTextBox2.Text += "TEAM 2\n";                                //6 
                    richTextBox2.Text += richTextBox1.Lines[1];                     //7
                    if (richTextBox1.Lines[1].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[12];                    //8
                    if (richTextBox1.Lines[12].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[15];                    //9
                    if (richTextBox1.Lines[15].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[1] = team2g;

                    richTextBox2.Text += Environment.NewLine;                       //10

                    richTextBox2.Text += "TEAM 3\n";                                //11
                    richTextBox2.Text += richTextBox1.Lines[2];                     //12
                    if (richTextBox1.Lines[2].Contains("*")) team3g++;               
                    richTextBox2.Text += Environment.NewLine;                        
                    richTextBox2.Text += richTextBox1.Lines[11];                    //13
                    if (richTextBox1.Lines[11].Contains("*")) team3g++;              
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[16];                    //14
                    if (richTextBox1.Lines[16].Contains("*")) team3g++;               
                    richTextBox2.Text += Environment.NewLine;                        
                    goldGolfers[2] = team3g;
                                                                                      
                    richTextBox2.Text += Environment.NewLine;                       //15

                    richTextBox2.Text += "TEAM 4\n";                                //16
                    richTextBox2.Text += richTextBox1.Lines[3];                     //17
                    if (richTextBox1.Lines[3].Contains("*")) team4g++;               
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[10];                    //18
                    if (richTextBox1.Lines[10].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[17];                    //19
                    if (richTextBox1.Lines[17].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[3] = team4g;

                    richTextBox2.Text += Environment.NewLine;                       //20

                    richTextBox2.Text += "TEAM 5\n";                                //21
                    richTextBox2.Text += richTextBox1.Lines[4];                     //22
                    if (richTextBox1.Lines[4].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[9];                     //23
                    if (richTextBox1.Lines[9].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[18];                    //24
                    if (richTextBox1.Lines[18].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[4] = team5g;

                    richTextBox2.Text += Environment.NewLine;                       //25

                    richTextBox2.Text += "TEAM 6\n";                                //26
                    richTextBox2.Text += richTextBox1.Lines[5];                     //27
                    if (richTextBox1.Lines[5].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[8];                     //28
                    if (richTextBox1.Lines[8].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[19];                    //29
                    if (richTextBox1.Lines[19].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[5] = team6g;

                    richTextBox2.Text += Environment.NewLine;                       //30

                    richTextBox2.Text += "TEAM 7\n";                                //31
                    richTextBox2.Text += richTextBox1.Lines[6];                     //32
                    if (richTextBox1.Lines[6].Contains("*")) team7g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[7];                     //33
                    if (richTextBox1.Lines[7].Contains("*")) team7g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[20];                    //34
                    if (richTextBox1.Lines[20].Contains("*")) team7g++;
                    goldGolfers[6] = team7g;
                    teamCnt = 7;

                    // If there are multiple teams that have 3 Gold and One White run the swap again
                    while (((goldGolfers[0] >= 2 || goldGolfers[1] >= 2 || goldGolfers[2] >= 2 || goldGolfers[3] >= 2 || goldGolfers[4] >= 2 || goldGolfers[5] >= 2 || goldGolfers[6] >= 2) && (goldGolfers[0] == 0 || goldGolfers[1] == 0 || goldGolfers[2] == 0 || goldGolfers[3] == 0 || goldGolfers[4] == 0 || goldGolfers[5] == 0 || goldGolfers[6] == 0))
                        || ((goldGolfers[0] >= 3 || goldGolfers[1] >= 3 || goldGolfers[2] >= 3 || goldGolfers[3] >= 3 || goldGolfers[4] >= 3 || goldGolfers[5] >= 3 || goldGolfers[6] >= 3) && (goldGolfers[0] == 1 || goldGolfers[1] == 1 || goldGolfers[2] == 1 || goldGolfers[3] == 1 || goldGolfers[4] == 1 || goldGolfers[5] == 1 || goldGolfers[6] == 1))
                        || ((goldGolfers[0] == 4 || goldGolfers[1] == 4 || goldGolfers[2] == 4 || goldGolfers[3] == 4 || goldGolfers[4] == 4 || goldGolfers[5] == 4 || goldGolfers[6] == 4) && (goldGolfers[0] == 2 || goldGolfers[1] == 2 || goldGolfers[2] == 2 || goldGolfers[3] == 2 || goldGolfers[4] == 2 || goldGolfers[5] == 2 || goldGolfers[6] == 2)))
                    {

                        determineTeamsToSwap(teamCnt);

                        swapGoldForWhiteThreesome();
                    }
                    //Use the +10 to force the program path dow a 3-Person team display
                    displayTeamHCPcalc(teamCnt +10);
                    break;
                case 22:
                    richTextBox2.Text = "A-B-C-D DRAW for " + (playerCnt - 1).ToString() + " golfers\n";
                    richTextBox2.Text += "TEAM 1\n";                            //1
                    richTextBox2.Text += richTextBox1.Lines[0];                 //2
                    if (richTextBox1.Lines[0].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[11];                //3
                    if (richTextBox1.Lines[11].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[12];                //4
                    if (richTextBox1.Lines[12].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += "BLIND";                               //5
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[0] = team1g;

                    richTextBox2.Text += Environment.NewLine;                   //6

                    richTextBox2.Text += "TEAM 2\n";                            //7
                    richTextBox2.Text += richTextBox1.Lines[1];                 //8
                    if (richTextBox1.Lines[1].Contains("*")) team2g++;          
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[10];                //9
                    if (richTextBox1.Lines[10].Contains("*")) team2g++;         
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[13];                //10
                    if (richTextBox1.Lines[13].Contains("*")) team2g++;         
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += "BLIND";                               //11
                    richTextBox2.Text += Environment.NewLine;                    
                    goldGolfers[1] = team2g;

                    richTextBox2.Text += Environment.NewLine;                   //12

                    richTextBox2.Text += "TEAM 3\n";                            //13
                    richTextBox2.Text += richTextBox1.Lines[2];                 //14
                    if (richTextBox1.Lines[2].Contains("*")) team3g++;          
                    richTextBox2.Text += Environment.NewLine;                   
                    richTextBox2.Text += richTextBox1.Lines[9];                 //15
                    if (richTextBox1.Lines[9].Contains("*")) team3g++; 
                    richTextBox2.Text += Environment.NewLine;                   
                    richTextBox2.Text += richTextBox1.Lines[14];                //16
                    if (richTextBox1.Lines[14].Contains("*")) team3g++; 
                    richTextBox2.Text += Environment.NewLine;                   
                    richTextBox2.Text += richTextBox1.Lines[21];                //17
                    if (richTextBox1.Lines[21].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;                    
                    goldGolfers[2] = team3g;

                    richTextBox2.Text += Environment.NewLine;                   //18
                    
                    richTextBox2.Text += "TEAM 4\n";                            //19
                    richTextBox2.Text += richTextBox1.Lines[3];                 //20
                    if (richTextBox1.Lines[3].Contains("*")) team4g++;          
                    richTextBox2.Text += Environment.NewLine;                   
                    richTextBox2.Text += richTextBox1.Lines[8];                 //21
                    if (richTextBox1.Lines[8].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;                   
                    richTextBox2.Text += richTextBox1.Lines[15];                //22
                    if (richTextBox1.Lines[15].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;                   
                    richTextBox2.Text += richTextBox1.Lines[20];                //23
                    if (richTextBox1.Lines[20].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;                   
                    goldGolfers[3] = team4g;

                    richTextBox2.Text += Environment.NewLine;                   //24

                    richTextBox2.Text += "TEAM 5\n";                            //25
                    richTextBox2.Text += richTextBox1.Lines[4];                 //26
                    if (richTextBox1.Lines[4].Contains("*")) team5g++;          
                    richTextBox2.Text += Environment.NewLine;                   
                    richTextBox2.Text += richTextBox1.Lines[7];                 //27
                    if (richTextBox1.Lines[7].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;                   
                    richTextBox2.Text += richTextBox1.Lines[16];                //28
                    if (richTextBox1.Lines[16].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;                   
                    richTextBox2.Text += richTextBox1.Lines[19];                //29
                    if (richTextBox1.Lines[19].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;                   
                    goldGolfers[4] = team5g;

                    richTextBox2.Text += Environment.NewLine;                   //30

                    richTextBox2.Text += "TEAM 6\n";                            //31
                    richTextBox2.Text += richTextBox1.Lines[5];                 //32
                    if (richTextBox1.Lines[5].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[6];                 //33
                    if (richTextBox1.Lines[6].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[17];                //34
                    if (richTextBox1.Lines[17].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[18];                //35
                    if (richTextBox1.Lines[18].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[5] = team6g;
                    teamCnt = 6;

                    rIntF = rnd.Next(18, 22);
                    do
                    {
                        rIntB = rnd.Next(18, 22);
                    } while (rIntF == rIntB);

                    //Make the BLIND before the teams are balanced for GOLD/WHITE
                    blindString = richTextBox1.Lines[rIntF] + "(F) " +
                            richTextBox1.Lines[rIntB].TrimStart('1', '2', '3', '4', '5', '6', '7', '8', '9', '0', ' ') + "(B)";
                    //Change the BLIND LINE and Correct for GOLD player if necessary
                    changeLine(richTextBox2, 5, blindString);
                    if (richTextBox2.Lines[5].Contains("*")) team1g++;
                    changeLine(richTextBox2, 11, blindString);
                    if (richTextBox2.Lines[11].Contains("*")) team2g++;

                    // If there are multiple teams that have 3 Gold and One White run the swap again
                    while (((goldGolfers[0] >= 2 || goldGolfers[1] >= 2 || goldGolfers[2] >= 2 || goldGolfers[3] >= 2 || goldGolfers[4] >= 2 || goldGolfers[5] >= 2) && (goldGolfers[0] == 0 || goldGolfers[1] == 0 || goldGolfers[2] == 0 || goldGolfers[3] == 0 || goldGolfers[4] == 0 || goldGolfers[5] == 0))
                        || ((goldGolfers[0] >= 3 || goldGolfers[1] >= 3 || goldGolfers[2] >= 3 || goldGolfers[3] >= 3 || goldGolfers[4] >= 3 || goldGolfers[5] >= 3) && (goldGolfers[0] == 1 || goldGolfers[1] == 1 || goldGolfers[2] == 1 || goldGolfers[3] == 1 || goldGolfers[4] == 1 || goldGolfers[5] == 1))
                        || ((goldGolfers[0] == 4 || goldGolfers[1] == 4 || goldGolfers[2] == 4 || goldGolfers[3] == 4 || goldGolfers[4] == 4 || goldGolfers[5] == 4) && (goldGolfers[0] == 2 || goldGolfers[1] == 2 || goldGolfers[2] == 2 || goldGolfers[3] == 2 || goldGolfers[4] == 2 || goldGolfers[5] == 2)))
                    {
                        determineTeamsToSwap(teamCnt);

                        swapGoldForWhiteFoursome(minGoldTeamA, maxGoldTeamA);
                    }
                    displayTeamHCPcalc(teamCnt);
                    break;
                case 23:
                    richTextBox2.Text = "A-B-C-D DRAW for " + (playerCnt - 1).ToString() + " golfers\n";
                    richTextBox2.Text += "TEAM 1\n";                            //1
                    richTextBox2.Text += richTextBox1.Lines[5];                 //2
                    if (richTextBox1.Lines[5].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[6];                 //3
                    if (richTextBox1.Lines[6].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[17];                //4
                    if (richTextBox1.Lines[17].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[18];                //5
                    if (richTextBox1.Lines[18].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[0] = team1g;
                                                                               
                    richTextBox2.Text += Environment.NewLine;                    //6

                    richTextBox2.Text += "TEAM 2\n";                             //7
                    richTextBox2.Text += richTextBox1.Lines[1];                  //8
                    if (richTextBox1.Lines[1].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;                  
                    richTextBox2.Text += richTextBox1.Lines[10];                 //9
                    if (richTextBox1.Lines[10].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;                  
                    richTextBox2.Text += richTextBox1.Lines[13];                 //10
                    if (richTextBox1.Lines[13].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;                  
                    richTextBox2.Text += richTextBox1.Lines[22];                 //11
                    if (richTextBox1.Lines[22].Contains("*")) team2g++;           
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[1] = team2g;                                   
                                                                                 
                    richTextBox2.Text += Environment.NewLine;                    //12

                    richTextBox2.Text += "TEAM 3\n";                             //13
                    richTextBox2.Text += richTextBox1.Lines[2];                  //14
                    if (richTextBox1.Lines[2].Contains("*")) team3g++;         
                    richTextBox2.Text += Environment.NewLine;                    
                    richTextBox2.Text += richTextBox1.Lines[9];                  //15
                    if (richTextBox1.Lines[9].Contains("*")) team3g++;         
                    richTextBox2.Text += Environment.NewLine;                    
                    richTextBox2.Text += richTextBox1.Lines[14];                 //16
                    if (richTextBox1.Lines[14].Contains("*")) team3g++;        
                    richTextBox2.Text += Environment.NewLine;                    
                    richTextBox2.Text += richTextBox1.Lines[21];                 //17
                    if (richTextBox1.Lines[21].Contains("*")) team3g++;          
                    richTextBox2.Text += Environment.NewLine;                     
                    goldGolfers[2] = team3g;                                   
                                                                                 
                    richTextBox2.Text += Environment.NewLine;                    //18
                                                                                 
                    richTextBox2.Text += "TEAM 4\n";                             //19
                    richTextBox2.Text += richTextBox1.Lines[3];                  //20
                    if (richTextBox1.Lines[3].Contains("*")) team4g++;         
                    richTextBox2.Text += Environment.NewLine;                    
                    richTextBox2.Text += richTextBox1.Lines[8];                  //21
                    if (richTextBox1.Lines[8].Contains("*")) team4g++;         
                    richTextBox2.Text += Environment.NewLine;                    
                    richTextBox2.Text += richTextBox1.Lines[15];                 //22
                    if (richTextBox1.Lines[15].Contains("*")) team4g++;        
                    richTextBox2.Text += Environment.NewLine;                    
                    richTextBox2.Text += richTextBox1.Lines[20];                 //23
                    if (richTextBox1.Lines[20].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[3] = team4g;                                   
                                                                                 
                    richTextBox2.Text += Environment.NewLine;                    //24
                                                                                 
                    richTextBox2.Text += "TEAM 5\n";                             //25
                    richTextBox2.Text += richTextBox1.Lines[4];                  //26
                    if (richTextBox1.Lines[4].Contains("*")) team5g++;         
                    richTextBox2.Text += Environment.NewLine;                    
                    richTextBox2.Text += richTextBox1.Lines[7];                  //27
                    if (richTextBox1.Lines[7].Contains("*")) team5g++;         
                    richTextBox2.Text += Environment.NewLine;                    
                    richTextBox2.Text += richTextBox1.Lines[16];                 //28
                    if (richTextBox1.Lines[16].Contains("*")) team5g++;        
                    richTextBox2.Text += Environment.NewLine;                    
                    richTextBox2.Text += richTextBox1.Lines[19];                 //29
                    if (richTextBox1.Lines[19].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[4] = team5g;                                   
                                                                                 
                    richTextBox2.Text += Environment.NewLine;                    //30
                                                                                 
                    richTextBox2.Text += "TEAM 6\n";                             //31
                    richTextBox2.Text += richTextBox1.Lines[0];                  //32
                    if (richTextBox1.Lines[0].Contains("*")) team6g++;         
                    richTextBox2.Text += Environment.NewLine;                    
                    richTextBox2.Text += richTextBox1.Lines[11];                 //33
                    if (richTextBox1.Lines[11].Contains("*")) team6g++;        
                    richTextBox2.Text += Environment.NewLine;                    
                    richTextBox2.Text += richTextBox1.Lines[12];                 //34
                    if (richTextBox1.Lines[12].Contains("*")) team6g++;         
                    richTextBox2.Text += Environment.NewLine;                    
                    
                    rIntF = rnd.Next(18, 23);
                    do
                    {
                        rIntB = rnd.Next(18, 23);
                    } while (rIntF == rIntB);                                   //35

                    richTextBox2.Text += richTextBox1.Lines[rIntF] + "(F) " + 
                        richTextBox1.Lines[rIntB].TrimStart('1', '2', '3', '4', '5', '6', '7', '8', '9', '0', ' ') + "(B)";
                    if (richTextBox2.Lines[35].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[5] = team6g;
                    teamCnt = 6;

                    // If there are multiple teams that have 3 Gold and One White run the swap again
                    while (((goldGolfers[0] >= 2 || goldGolfers[1] >= 2 || goldGolfers[2] >= 2 || goldGolfers[3] >= 2 || goldGolfers[4] >= 2 || goldGolfers[5] >= 2) && (goldGolfers[0] == 0 || goldGolfers[1] == 0 || goldGolfers[2] == 0 || goldGolfers[3] == 0 || goldGolfers[4] == 0 || goldGolfers[5] == 0))
                        || ((goldGolfers[0] >= 3 || goldGolfers[1] >= 3 || goldGolfers[2] >= 3 || goldGolfers[3] >= 3 || goldGolfers[4] >= 3 || goldGolfers[5] >= 3) && (goldGolfers[0] == 1 || goldGolfers[1] == 1 || goldGolfers[2] == 1 || goldGolfers[3] == 1 || goldGolfers[4] == 1 || goldGolfers[5] == 1))
                        || ((goldGolfers[0] == 4 || goldGolfers[1] == 4 || goldGolfers[2] == 4 || goldGolfers[3] == 4 || goldGolfers[4] == 4 || goldGolfers[5] == 4) && (goldGolfers[0] == 2 || goldGolfers[1] == 2 || goldGolfers[2] == 2 || goldGolfers[3] == 2 || goldGolfers[4] == 2 || goldGolfers[5] == 2)))
                    {
                        determineTeamsToSwap(teamCnt);

                        swapGoldForWhiteFoursome(minGoldTeamA, maxGoldTeamA);
                    }
                    displayTeamHCPcalc(teamCnt);
                    break;
                case 24:
                    richTextBox2.Text = "A-B-C-D DRAW for " + (playerCnt - 1).ToString() + " golfers\n";
                    richTextBox2.Text += "TEAM 1\n";                            //1
                    richTextBox2.Text += richTextBox1.Lines[0];                 //2
                    if (richTextBox1.Lines[0].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[11];                //3
                    if (richTextBox1.Lines[11].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[12];                //4
                    if (richTextBox1.Lines[12].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[23];                //5
                    if (richTextBox1.Lines[23].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[0] = team1g;
                                                                                
                    richTextBox2.Text += Environment.NewLine;                   //6

                    richTextBox2.Text += "TEAM 2\n";                            //7
                    richTextBox2.Text += richTextBox1.Lines[1];                 //8
                    if (richTextBox1.Lines[1].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;                   
                    richTextBox2.Text += richTextBox1.Lines[10];                //9
                    if (richTextBox1.Lines[10].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;                   
                    richTextBox2.Text += richTextBox1.Lines[13];                //10
                    if (richTextBox1.Lines[13].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;                   
                    richTextBox2.Text += richTextBox1.Lines[22];                //11
                    if (richTextBox1.Lines[22].Contains("*")) team2g++;          
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[1] = team2g;                                    
                                                                                
                    richTextBox2.Text += Environment.NewLine;                   //12                  

                    richTextBox2.Text += "TEAM 3\n";                            //13
                    richTextBox2.Text += richTextBox1.Lines[2];                 //14
                    if (richTextBox1.Lines[2].Contains("*")) team3g++;          
                    richTextBox2.Text += Environment.NewLine;                   
                    richTextBox2.Text += richTextBox1.Lines[9];                 //15
                    if (richTextBox1.Lines[9].Contains("*")) team3g++;          
                    richTextBox2.Text += Environment.NewLine;                   
                    richTextBox2.Text += richTextBox1.Lines[14];                //16
                    if (richTextBox1.Lines[14].Contains("*")) team3g++;         
                    richTextBox2.Text += Environment.NewLine;                   
                    richTextBox2.Text += richTextBox1.Lines[21];                //17
                    if (richTextBox1.Lines[21].Contains("*")) team3g++;         
                    richTextBox2.Text += Environment.NewLine;                    
                    goldGolfers[2] = team3g;                                    
                                                                                
                    richTextBox2.Text += Environment.NewLine;                   //18

                    richTextBox2.Text += "TEAM 4\n";                            //19
                    richTextBox2.Text += richTextBox1.Lines[3];                 //20
                    if (richTextBox1.Lines[3].Contains("*")) team4g++;          
                    richTextBox2.Text += Environment.NewLine;                   
                    richTextBox2.Text += richTextBox1.Lines[8];                 //21
                    if (richTextBox1.Lines[8].Contains("*")) team4g++;          
                    richTextBox2.Text += Environment.NewLine;                   
                    richTextBox2.Text += richTextBox1.Lines[15];                //22
                    if (richTextBox1.Lines[15].Contains("*")) team4g++;         
                    richTextBox2.Text += Environment.NewLine;                   
                    richTextBox2.Text += richTextBox1.Lines[20];                //23
                    if (richTextBox1.Lines[20].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;                   
                    goldGolfers[3] = team4g;                                    
                                                                                
                    richTextBox2.Text += Environment.NewLine;                   //24
                                                                                
                    richTextBox2.Text += "TEAM 5\n";                            //25
                    richTextBox2.Text += richTextBox1.Lines[4];                 //26
                    if (richTextBox1.Lines[4].Contains("*")) team5g++;          
                    richTextBox2.Text += Environment.NewLine;                   
                    richTextBox2.Text += richTextBox1.Lines[7];                 //27
                    if (richTextBox1.Lines[7].Contains("*")) team5g++;          
                    richTextBox2.Text += Environment.NewLine;                   
                    richTextBox2.Text += richTextBox1.Lines[16];                //28
                    if (richTextBox1.Lines[16].Contains("*")) team5g++;         
                    richTextBox2.Text += Environment.NewLine;                   
                    richTextBox2.Text += richTextBox1.Lines[19];                //29
                    if (richTextBox1.Lines[19].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;                   
                    goldGolfers[4] = team5g;                                    
                                                                                
                    richTextBox2.Text += Environment.NewLine;                   //30
                                                                                
                    richTextBox2.Text += "TEAM 6\n";                            //31
                    richTextBox2.Text += richTextBox1.Lines[5];                 //32
                    if (richTextBox1.Lines[5].Contains("*")) team6g++;          
                    richTextBox2.Text += Environment.NewLine;                   
                    richTextBox2.Text += richTextBox1.Lines[6];                 //33
                    if (richTextBox1.Lines[6].Contains("*")) team6g++;          
                    richTextBox2.Text += Environment.NewLine;                   
                    richTextBox2.Text += richTextBox1.Lines[17];                //34
                    if (richTextBox1.Lines[17].Contains("*")) team6g++;         
                    richTextBox2.Text += Environment.NewLine;                   
                    richTextBox2.Text += richTextBox1.Lines[18];                //35
                    if (richTextBox1.Lines[18].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[5] = team6g;
                    teamCnt = 6;

                    // If there are multiple teams that have 3 Gold and One White run the swap again
                    while (((goldGolfers[0] >= 2 || goldGolfers[1] >= 2 || goldGolfers[2] >= 2 || goldGolfers[3] >= 2 || goldGolfers[4] >= 2 || goldGolfers[5] >= 2) && (goldGolfers[0] == 0 || goldGolfers[1] == 0 || goldGolfers[2] == 0 || goldGolfers[3] == 0 || goldGolfers[4] == 0 || goldGolfers[5] == 0))
                        || ((goldGolfers[0] >= 3 || goldGolfers[1] >= 3 || goldGolfers[2] >= 3 || goldGolfers[3] >= 3 || goldGolfers[4] >= 3 || goldGolfers[5] >= 3) && (goldGolfers[0] == 1 || goldGolfers[1] == 1 || goldGolfers[2] == 1 || goldGolfers[3] == 1 || goldGolfers[4] == 1 || goldGolfers[5] == 1))
                        || ((goldGolfers[0] == 4 || goldGolfers[1] == 4 || goldGolfers[2] == 4 || goldGolfers[3] == 4 || goldGolfers[4] == 4 || goldGolfers[5] == 4) && (goldGolfers[0] == 2 || goldGolfers[1] == 2 || goldGolfers[2] == 2 || goldGolfers[3] == 2 || goldGolfers[4] == 2 || goldGolfers[5] == 2)))
                    {
                        determineTeamsToSwap(teamCnt);
                        
                        swapGoldForWhiteFoursome(minGoldTeamA, maxGoldTeamA);
                    }
                    displayTeamHCPcalc(teamCnt);
                    break;
                case 25:
                    richTextBox2.Text = "A-B-C-D DRAW for " + (playerCnt - 1).ToString() + " golfers\n";
                    richTextBox2.Text += "TEAM 1\n";
                    richTextBox2.Text += richTextBox1.Lines[0];
                    if (richTextBox1.Lines[0].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[13];
                    if (richTextBox1.Lines[13].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[14];
                    if (richTextBox1.Lines[14].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    rIntF = rnd.Next(17, 25); 
                    do
                    {
                        rIntB = rnd.Next(17, 25);
                    } while (rIntF == rIntB);

                    richTextBox2.Text += richTextBox1.Lines[rIntF] + "(F) " + 
                        richTextBox1.Lines[rIntB].TrimStart('1', '2', '3', '4', '5', '6', '7', '8', '9', '0', ' ') + "(B)";
                    if (richTextBox2.Lines[5].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[0] = team1g;

                    richTextBox2.Text += Environment.NewLine;

                    richTextBox2.Text += "TEAM 2\n";
                    richTextBox2.Text += richTextBox1.Lines[1];
                    if (richTextBox1.Lines[1].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[12];
                    if (richTextBox1.Lines[12].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[15];
                    if (richTextBox1.Lines[15].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[rIntF] + "(F) " + 
                        richTextBox1.Lines[rIntB].TrimStart('1', '2', '3', '4', '5', '6', '7', '8', '9', '0', ' ') + "(B)";
                    if (richTextBox2.Lines[11].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[1] = team2g;

                    richTextBox2.Text += Environment.NewLine;

                    richTextBox2.Text += "TEAM 3\n";
                    richTextBox2.Text += richTextBox1.Lines[2];
                    if (richTextBox1.Lines[2].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[11];
                    if (richTextBox1.Lines[11].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[16];
                    if (richTextBox1.Lines[16].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[rIntF] + "(F) " +
                        richTextBox1.Lines[rIntB].TrimStart('1', '2', '3', '4', '5', '6', '7', '8', '9', '0', ' ') + "(B)";
                    if (richTextBox2.Lines[17].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[2] = team3g;

                    richTextBox2.Text += Environment.NewLine;

                    richTextBox2.Text += "TEAM 4\n";
                    richTextBox2.Text += richTextBox1.Lines[3];
                    if (richTextBox1.Lines[3].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[10];
                    if (richTextBox1.Lines[10].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[17];
                    if (richTextBox1.Lines[17].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[24];
                    if (richTextBox1.Lines[24].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[3] = team4g;

                    richTextBox2.Text += Environment.NewLine;

                    richTextBox2.Text += "TEAM 5\n";
                    richTextBox2.Text += richTextBox1.Lines[4];
                    if (richTextBox1.Lines[4].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[9];
                    if (richTextBox1.Lines[9].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[18];
                    if (richTextBox1.Lines[18].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[23];
                    if (richTextBox1.Lines[23].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[4] = team5g;

                    richTextBox2.Text += Environment.NewLine;

                    richTextBox2.Text += "TEAM 6\n";
                    richTextBox2.Text += richTextBox1.Lines[5];
                    if (richTextBox1.Lines[5].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[8];
                    if (richTextBox1.Lines[8].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[19];
                    if (richTextBox1.Lines[19].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[22];
                    if (richTextBox1.Lines[22].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[5] = team6g;

                    richTextBox2.Text += Environment.NewLine;

                    richTextBox2.Text += "TEAM 7\n";
                    richTextBox2.Text += richTextBox1.Lines[6];
                    if (richTextBox1.Lines[6].Contains("*")) team7g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[7];
                    if (richTextBox1.Lines[7].Contains("*")) team7g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[20];
                    if (richTextBox1.Lines[20].Contains("*")) team7g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[21];
                    if (richTextBox1.Lines[21].Contains("*")) team7g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[6] = team7g;
                    teamCnt = 7;

                    // If there are multiple teams that have 3 Gold and One White run the swap again
                    while (((goldGolfers[0] >= 2 || goldGolfers[1] >= 2 || goldGolfers[2] >= 2 || goldGolfers[3] >= 2 || goldGolfers[4] >= 2 || goldGolfers[5] >= 2 || goldGolfers[6] >= 2) && (goldGolfers[0] == 0 || goldGolfers[1] == 0 || goldGolfers[2] == 0 || goldGolfers[3] == 0 || goldGolfers[4] == 0 || goldGolfers[5] == 0 || goldGolfers[6] == 0))
                        || ((goldGolfers[0] >= 3 || goldGolfers[1] >= 3 || goldGolfers[2] >= 3 || goldGolfers[3] >= 3 || goldGolfers[4] >= 3 || goldGolfers[5] >= 3 || goldGolfers[6] >= 3) && (goldGolfers[0] == 1 || goldGolfers[1] == 1 || goldGolfers[2] == 1 || goldGolfers[3] == 1 || goldGolfers[4] == 1 || goldGolfers[5] == 1 || goldGolfers[6] == 1))
                        || ((goldGolfers[0] == 4 || goldGolfers[1] == 4 || goldGolfers[2] == 4 || goldGolfers[3] == 4 || goldGolfers[4] == 4 || goldGolfers[5] == 4 || goldGolfers[6] == 4) && (goldGolfers[0] == 2 || goldGolfers[1] == 2 || goldGolfers[2] == 2 || goldGolfers[3] == 2 || goldGolfers[4] == 2 || goldGolfers[5] == 2 || goldGolfers[6] == 2)))
                    {
                        determineTeamsToSwap(teamCnt);

                        swapGoldForWhiteFoursome(minGoldTeamA, maxGoldTeamA);
                    }
                    displayTeamHCPcalc(teamCnt);
                    break;
                case 26:
                    richTextBox2.Text = "A-B-C-D DRAW for " + (playerCnt - 1).ToString() + " golfers\n";
                    richTextBox2.Text += "TEAM 1\n";
                    richTextBox2.Text += richTextBox1.Lines[0];
                    if (richTextBox1.Lines[0].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[13];
                    if (richTextBox1.Lines[13].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[14];
                    if (richTextBox1.Lines[14].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    
                    rIntF = rnd.Next(16, 26);
                    do
                    {
                        rIntB = rnd.Next(16, 26);
                    } while (rIntF == rIntB);
                    richTextBox2.Text += richTextBox1.Lines[rIntF] + "(F) " +
                        richTextBox1.Lines[rIntB].TrimStart('1', '2', '3', '4', '5', '6', '7', '8', '9', '0', ' ') + "(B)";
                    if (richTextBox2.Lines[5].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[0] = team1g;

                    richTextBox2.Text += Environment.NewLine;                   //6

                    richTextBox2.Text += "TEAM 2\n";                            //7
                    richTextBox2.Text += richTextBox1.Lines[1];
                    if (richTextBox1.Lines[1].Contains("*")) team2g++;          //8
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[12];
                    if (richTextBox1.Lines[12].Contains("*")) team2g++;         //9
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[15];
                    if (richTextBox1.Lines[15].Contains("*")) team2g++;         //10
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[rIntF] + "(F) " + 
                        richTextBox1.Lines[rIntB].TrimStart('1', '2', '3', '4', '5', '6', '7', '8', '9', '0', ' ') + "(B)";
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[1] = team2g;
                    
                    richTextBox2.Text += Environment.NewLine;                   //12 

                    richTextBox2.Text += "TEAM 3\n";                            //13
                    richTextBox2.Text += richTextBox1.Lines[2];                 //14
                    if (richTextBox1.Lines[2].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[11];                //15
                    if (richTextBox1.Lines[11].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[16];                //16
                    if (richTextBox1.Lines[16].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[25];                //17
                    if (richTextBox1.Lines[25].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;                    
                    goldGolfers[2] = team3g;

                    richTextBox2.Text += Environment.NewLine;                   //18
 
                    richTextBox2.Text += "TEAM 4\n";                            //19
                    richTextBox2.Text += richTextBox1.Lines[3];                 //20
                    if (richTextBox1.Lines[3].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[10];                //21
                    if (richTextBox1.Lines[10].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[17];                //22
                    if (richTextBox1.Lines[17].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[24];                //23
                    if (richTextBox1.Lines[24].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[3] = team4g;

                    richTextBox2.Text += Environment.NewLine;                   //24

                    richTextBox2.Text += "TEAM 5\n";                            //25
                    richTextBox2.Text += richTextBox1.Lines[4];                 //26
                    if (richTextBox1.Lines[4].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[9];                 //27
                    if (richTextBox1.Lines[9].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[18];                //28
                    if (richTextBox1.Lines[18].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[23];                //29
                    if (richTextBox1.Lines[23].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[4] = team5g;

                    richTextBox2.Text += Environment.NewLine;                   //30

                    richTextBox2.Text += "TEAM 6\n";                            //31
                    richTextBox2.Text += richTextBox1.Lines[5];                 //32
                    if (richTextBox1.Lines[5].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[8];                 //33
                    if (richTextBox1.Lines[8].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[19];                //34
                    if (richTextBox1.Lines[19].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[22];                //35
                    if (richTextBox1.Lines[22].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[5] = team6g;

                    richTextBox2.Text += Environment.NewLine;

                    richTextBox2.Text += "TEAM 7\n";
                    richTextBox2.Text += richTextBox1.Lines[6];
                    if (richTextBox1.Lines[6].Contains("*")) team7g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[7];
                    if (richTextBox1.Lines[7].Contains("*")) team7g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[20];
                    if (richTextBox1.Lines[20].Contains("*")) team7g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[21];
                    if (richTextBox1.Lines[21].Contains("*")) team7g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[6] = team7g;
                    teamCnt = 7;

                    // If there are multiple teams that have 3 Gold and One White run the swap again
                    while (((goldGolfers[0] >= 2 || goldGolfers[1] >= 2 || goldGolfers[2] >= 2 || goldGolfers[3] >= 2 || goldGolfers[4] >= 2 || goldGolfers[5] >= 2 || goldGolfers[6] >= 2) && (goldGolfers[0] == 0 || goldGolfers[1] == 0 || goldGolfers[2] == 0 || goldGolfers[3] == 0 || goldGolfers[4] == 0 || goldGolfers[5] == 0 || goldGolfers[6] == 0))
                        || ((goldGolfers[0] >= 3 || goldGolfers[1] >= 3 || goldGolfers[2] >= 3 || goldGolfers[3] >= 3 || goldGolfers[4] >= 3 || goldGolfers[5] >= 3 || goldGolfers[6] >= 3) && (goldGolfers[0] == 1 || goldGolfers[1] == 1 || goldGolfers[2] == 1 || goldGolfers[3] == 1 || goldGolfers[4] == 1 || goldGolfers[5] == 1 || goldGolfers[6] == 1))
                        || ((goldGolfers[0] == 4 || goldGolfers[1] == 4 || goldGolfers[2] == 4 || goldGolfers[3] == 4 || goldGolfers[4] == 4 || goldGolfers[5] == 4 || goldGolfers[6] == 4) && (goldGolfers[0] == 2 || goldGolfers[1] == 2 || goldGolfers[2] == 2 || goldGolfers[3] == 2 || goldGolfers[4] == 2 || goldGolfers[5] == 2 || goldGolfers[6] == 2)))
                    {
                        determineTeamsToSwap(teamCnt);
                        
                        swapGoldForWhiteFoursome(minGoldTeamA, maxGoldTeamA);
                    }
                    displayTeamHCPcalc(teamCnt);
                    break;
                case 27:
                    richTextBox2.Text = "A-B-C-D DRAW for " + (playerCnt - 1).ToString() + " golfers\n";
                    richTextBox2.Text += "TEAM 1\n";                            //1
                    richTextBox2.Text += richTextBox1.Lines[6];                 //2
                    if (richTextBox1.Lines[6].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[7];                 //3
                    if (richTextBox1.Lines[7].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[20];                //4
                    if (richTextBox1.Lines[20].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[21];                //5
                    if (richTextBox1.Lines[21].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[0] = team1g;

                    richTextBox2.Text += Environment.NewLine;                   //6

                    richTextBox2.Text += "TEAM 2\n";                            //7
                    richTextBox2.Text += richTextBox1.Lines[1];                 //8
                    if (richTextBox1.Lines[1].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[12];                //9
                    if (richTextBox1.Lines[12].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[15];                //10
                    if (richTextBox1.Lines[15].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[26];                //11
                    if (richTextBox1.Lines[26].Contains("*")) team2g++;          
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[1] = team2g;

                    richTextBox2.Text += Environment.NewLine;                   //12

                    richTextBox2.Text += "TEAM 3\n";                            //13
                    richTextBox2.Text += richTextBox1.Lines[2];                 //14
                    if (richTextBox1.Lines[2].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[11];                //15
                    if (richTextBox1.Lines[11].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[16];                //16
                    if (richTextBox1.Lines[16].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[25];                //17
                    if (richTextBox1.Lines[25].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;                    
                    goldGolfers[2] = team3g;

                    richTextBox2.Text += Environment.NewLine;                   //18

                    richTextBox2.Text += "TEAM 4\n";                            //19
                    richTextBox2.Text += richTextBox1.Lines[3];                 //20
                    if (richTextBox1.Lines[3].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[10];                //21
                    if (richTextBox1.Lines[10].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[17];                //22
                    if (richTextBox1.Lines[17].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[24];                //23
                    if (richTextBox1.Lines[24].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[3] = team4g;

                    richTextBox2.Text += Environment.NewLine;                   //24

                    richTextBox2.Text += "TEAM 5\n";                            //25
                    richTextBox2.Text += richTextBox1.Lines[4];                 //26
                    if (richTextBox1.Lines[4].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[9];                 //27
                    if (richTextBox1.Lines[9].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[18];                //28
                    if (richTextBox1.Lines[18].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[23];                //29
                    if (richTextBox1.Lines[23].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[4] = team5g;

                    richTextBox2.Text += Environment.NewLine;                   //30

                    richTextBox2.Text += "TEAM 6\n";                            //31
                    richTextBox2.Text += richTextBox1.Lines[5];                 //32
                    if (richTextBox1.Lines[5].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[8];                 //33
                    if (richTextBox1.Lines[8].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[19];                //34
                    if (richTextBox1.Lines[19].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[22];                //35
                    if (richTextBox1.Lines[22].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[5] = team6g;

                    richTextBox2.Text += Environment.NewLine;                   //36

                    richTextBox2.Text += "TEAM 7\n";                            //37
                    richTextBox2.Text += richTextBox1.Lines[0];                 //38
                    if (richTextBox1.Lines[0].Contains("*")) team7g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[13];                //39
                    if (richTextBox1.Lines[13].Contains("*")) team7g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[14];                //40
                    if (richTextBox1.Lines[14].Contains("*")) team7g++;
                    richTextBox2.Text += Environment.NewLine;
                    
                    rIntF = rnd.Next(21, 27);
                    do
                    {
                        rIntB = rnd.Next(21, 27);
                    } while (rIntF == rIntB);
                    richTextBox2.Text += richTextBox1.Lines[rIntF] + "(F) " + 
                        richTextBox1.Lines[rIntB].TrimStart('1', '2', '3', '4', '5', '6', '7', '8', '9', '0', ' ') + "(B)";
                    if (richTextBox2.Lines[richTextBox2.Lines.Length - 1].Contains("*")) team7g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[6] = team7g;
                    teamCnt = 7;

                    // If there are multiple teams that have 3 Gold and One White run the swap again
                    while (((goldGolfers[0] >= 2 || goldGolfers[1] >= 2 || goldGolfers[2] >= 2 || goldGolfers[3] >= 2 || goldGolfers[4] >= 2 || goldGolfers[5] >= 2 || goldGolfers[6] >= 2) && (goldGolfers[0] == 0 || goldGolfers[1] == 0 || goldGolfers[2] == 0 || goldGolfers[3] == 0 || goldGolfers[4] == 0 || goldGolfers[5] == 0 || goldGolfers[6] == 0))
                        || ((goldGolfers[0] >= 3 || goldGolfers[1] >= 3 || goldGolfers[2] >= 3 || goldGolfers[3] >= 3 || goldGolfers[4] >= 3 || goldGolfers[5] >= 3 || goldGolfers[6] >= 3) && (goldGolfers[0] == 1 || goldGolfers[1] == 1 || goldGolfers[2] == 1 || goldGolfers[3] == 1 || goldGolfers[4] == 1 || goldGolfers[5] == 1 || goldGolfers[6] == 1))
                        || ((goldGolfers[0] == 4 || goldGolfers[1] == 4 || goldGolfers[2] == 4 || goldGolfers[3] == 4 || goldGolfers[4] == 4 || goldGolfers[5] == 4 || goldGolfers[6] == 4) && (goldGolfers[0] == 2 || goldGolfers[1] == 2 || goldGolfers[2] == 2 || goldGolfers[3] == 2 || goldGolfers[4] == 2 || goldGolfers[5] == 2 || goldGolfers[6] == 2)))
                    {

                        determineTeamsToSwap(teamCnt);

                        swapGoldForWhiteFoursome(minGoldTeamA, maxGoldTeamA);
                    }
                    displayTeamHCPcalc(teamCnt);
                    break;
                case 28:
                    richTextBox2.Text = "A-B-C-D DRAW for " + (playerCnt - 1).ToString() + " golfers\n";
                    richTextBox2.Text += "TEAM 1\n";                            //1
                    richTextBox2.Text += richTextBox1.Lines[0];                 //2
                    if (richTextBox1.Lines[0].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[13];                //3
                    if (richTextBox1.Lines[13].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[14];                //4
                    if (richTextBox1.Lines[14].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[27];                //5
                    if (richTextBox1.Lines[27].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[0] = team1g;

                    richTextBox2.Text += Environment.NewLine;                   //6

                    richTextBox2.Text += "TEAM 2\n";                            //7
                    richTextBox2.Text += richTextBox1.Lines[1];                 //8
                    if (richTextBox1.Lines[1].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[12];                //9
                    if (richTextBox1.Lines[12].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[15];                //10
                    if (richTextBox1.Lines[15].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[26];                //11
                    if (richTextBox1.Lines[26].Contains("*")) team2g++;          
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[1] = team2g;

                    richTextBox2.Text += Environment.NewLine;                   //12

                    richTextBox2.Text += "TEAM 3\n";                            //13
                    richTextBox2.Text += richTextBox1.Lines[2];                 //14
                    if (richTextBox1.Lines[2].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[11];                //15
                    if (richTextBox1.Lines[11].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[16];                //16
                    if (richTextBox1.Lines[16].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[25];                //17
                    if (richTextBox1.Lines[25].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;                    
                    goldGolfers[2] = team3g;

                    richTextBox2.Text += Environment.NewLine;                   //18

                    richTextBox2.Text += "TEAM 4\n";                            //19
                    richTextBox2.Text += richTextBox1.Lines[3];                 //20
                    if (richTextBox1.Lines[3].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[10];                //21
                    if (richTextBox1.Lines[10].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[17];                //22
                    if (richTextBox1.Lines[17].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[24];                //23
                    if (richTextBox1.Lines[24].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[3] = team4g;

                    richTextBox2.Text += Environment.NewLine;                   //24

                    richTextBox2.Text += "TEAM 5\n";                            //25
                    richTextBox2.Text += richTextBox1.Lines[4];                 //26
                    if (richTextBox1.Lines[4].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[9];                 //27
                    if (richTextBox1.Lines[9].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[18];                //28
                    if (richTextBox1.Lines[18].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[23];                //29
                    if (richTextBox1.Lines[23].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[4] = team5g;

                    richTextBox2.Text += Environment.NewLine;                   //30

                    richTextBox2.Text += "TEAM 6\n";                            //31
                    richTextBox2.Text += richTextBox1.Lines[5];                 //32
                    if (richTextBox1.Lines[5].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[8];                 //33
                    if (richTextBox1.Lines[8].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[19];                //34
                    if (richTextBox1.Lines[19].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[22];                //35
                    if (richTextBox1.Lines[22].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[5] = team6g;

                    richTextBox2.Text += Environment.NewLine;                   //36
                    
                    richTextBox2.Text += "TEAM 7\n";                            //37
                    richTextBox2.Text += richTextBox1.Lines[6];                 //38
                    if (richTextBox1.Lines[6].Contains("*")) team7g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[7];                 //39
                    if (richTextBox1.Lines[7].Contains("*")) team7g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[20];                //40
                    if (richTextBox1.Lines[20].Contains("*")) team7g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[21];                //41
                    if (richTextBox1.Lines[21].Contains("*")) team7g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[6] = team7g;
                    teamCnt = 7;

                    // If there are multiple teams that have 3 Gold and One White run the swap again
                    while (((goldGolfers[0] >= 2 || goldGolfers[1] >= 2 || goldGolfers[2] >= 2 || goldGolfers[3] >= 2 || goldGolfers[4] >= 2 || goldGolfers[5] >= 2 || goldGolfers[6] >= 2) && (goldGolfers[0] == 0 || goldGolfers[1] == 0 || goldGolfers[2] == 0 || goldGolfers[3] == 0 || goldGolfers[4] == 0 || goldGolfers[5] == 0 || goldGolfers[6] == 0))
                        || ((goldGolfers[0] >= 3 || goldGolfers[1] >= 3 || goldGolfers[2] >= 3 || goldGolfers[3] >= 3 || goldGolfers[4] >= 3 || goldGolfers[5] >= 3 || goldGolfers[6] >= 3) && (goldGolfers[0] == 1 || goldGolfers[1] == 1 || goldGolfers[2] == 1 || goldGolfers[3] == 1 || goldGolfers[4] == 1 || goldGolfers[5] == 1 || goldGolfers[6] == 1))
                        || ((goldGolfers[0] == 4 || goldGolfers[1] == 4 || goldGolfers[2] == 4 || goldGolfers[3] == 4 || goldGolfers[4] == 4 || goldGolfers[5] == 4 || goldGolfers[6] == 4) && (goldGolfers[0] == 2 || goldGolfers[1] == 2 || goldGolfers[2] == 2 || goldGolfers[3] == 2 || goldGolfers[4] == 2 || goldGolfers[5] == 2 || goldGolfers[6] == 2)))
                    {
                        determineTeamsToSwap(teamCnt);
                    
                        swapGoldForWhiteFoursome(minGoldTeamA, maxGoldTeamA);
                    }
                    displayTeamHCPcalc(teamCnt);
                    break;
                default:
                    MessageBox.Show("Error: The A,B,C,D process is not supported for " + (playerCnt - 1) + " players.", "Check Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
            }
        }

        private void displayTeamHCPcalc(int teamsCnt)
        {
            //FOR 3-Player Teams use 17-11; FOR 4-Player Teams use 7-1
            switch (teamsCnt)
            {
                case 17:
                    teamHcp = 0;
                    for (int cntr = 32; cntr < 35; cntr++)
                        teamHcp += calcTeamHcp(richTextBox2.Lines[cntr]);
                    changeLine(richTextBox2, 31, richTextBox2.Lines[31] + " HCP = " + teamHcp);
                    teamsHcpArry[6] = teamHcp;
                    goto case 16;
                case 16:
                    teamHcp = 0;
                    for (int cntr = 27; cntr < 30; cntr++)
                        teamHcp += calcTeamHcp(richTextBox2.Lines[cntr]);
                    changeLine(richTextBox2, 26, richTextBox2.Lines[26] + " HCP = " + teamHcp);
                    teamsHcpArry[5] = teamHcp;
                    goto case 15;
                case 15:
                    teamHcp = 0;
                    for (int cntr = 22; cntr < 25; cntr++)
                        teamHcp += calcTeamHcp(richTextBox2.Lines[cntr]);
                    changeLine(richTextBox2, 21, richTextBox2.Lines[21] + " HCP = " + teamHcp);
                    teamsHcpArry[4] = teamHcp;
                    goto case 14;
                case 14:
                    teamHcp = 0;
                    for (int cntr = 17; cntr < 20; cntr++)
                        teamHcp += calcTeamHcp(richTextBox2.Lines[cntr]);
                    changeLine(richTextBox2, 16, richTextBox2.Lines[16] + " HCP = " + teamHcp);
                    teamsHcpArry[3] = teamHcp;
                    goto case 13;
                case 13:
                    teamHcp = 0;
                    for (int cntr = 12; cntr < 15; cntr++)
                        teamHcp += calcTeamHcp(richTextBox2.Lines[cntr]);
                    changeLine(richTextBox2, 11, richTextBox2.Lines[11] + " HCP = " + teamHcp);
                    teamsHcpArry[2] = teamHcp; 
                    goto case 12;
                case 12:
                    teamHcp = 0;
                    for (int cntr = 7; cntr < 10; cntr++)
                        teamHcp += calcTeamHcp(richTextBox2.Lines[cntr]);
                    changeLine(richTextBox2, 6, richTextBox2.Lines[6] + " HCP = " + teamHcp);
                    teamsHcpArry[1] = teamHcp;
                    goto case 11;
                case 11:
                    teamHcp = 0;
                    for (int cntr = 2; cntr < 5; cntr++)
                        teamHcp += calcTeamHcp(richTextBox2.Lines[cntr]);
                    changeLine(richTextBox2, 1, richTextBox2.Lines[1] + " HCP = " + teamHcp);
                    teamsHcpArry[0] = teamHcp;
                    break;                    
                case 7:
                    teamHcp = 0;
                    for (int cntr = 38; cntr < 42; cntr++)
                        teamHcp += calcTeamHcp(richTextBox2.Lines[cntr]);
                    changeLine(richTextBox2, 37, richTextBox2.Lines[37] + " HCP = " + teamHcp);
                    teamsHcpArry[6] = teamHcp;
                    goto case 6;
                case 6:
                    teamHcp = 0;
                    for (int cntr = 32; cntr < 36; cntr++)
                        teamHcp += calcTeamHcp(richTextBox2.Lines[cntr]);
                    changeLine(richTextBox2, 31, richTextBox2.Lines[31] + " HCP = " + teamHcp);
                    teamsHcpArry[5] = teamHcp;
                    goto case 5;
                case 5:
                    teamHcp = 0;
                    for (int cntr = 26; cntr < 30; cntr++)
                        teamHcp += calcTeamHcp(richTextBox2.Lines[cntr]);
                    changeLine(richTextBox2, 25, richTextBox2.Lines[25] + " HCP = " + teamHcp);
                    teamsHcpArry[4] = teamHcp;
                    goto case 4;
                case 4:
                    teamHcp = 0;
                    for (int cntr = 20; cntr < 24; cntr++)
                        teamHcp += calcTeamHcp(richTextBox2.Lines[cntr]);
                    changeLine(richTextBox2, 19, richTextBox2.Lines[19] + " HCP = " + teamHcp);
                    teamsHcpArry[3] = teamHcp; 
                    goto case 3;
                case 3:
                    teamHcp = 0;
                    for (int cntr = 14; cntr < 18; cntr++)
                        teamHcp += calcTeamHcp(richTextBox2.Lines[cntr]);
                    changeLine(richTextBox2, 13, richTextBox2.Lines[13] + " HCP = " + teamHcp);
                    teamsHcpArry[2] = teamHcp; 
                    goto case 2;
                case 2:
                    teamHcp = 0;
                    for (int cntr = 8; cntr < 12; cntr++)
                        teamHcp += calcTeamHcp(richTextBox2.Lines[cntr]);
                    changeLine(richTextBox2, 7, richTextBox2.Lines[7] + " HCP = " + teamHcp);
                    teamsHcpArry[1] = teamHcp; 
                    goto case 1;
                case 1:
                    teamHcp = 0;
                    for (int cntr = 2; cntr < 6; cntr++)
                        teamHcp += calcTeamHcp(richTextBox2.Lines[cntr]);
                    changeLine(richTextBox2, 1, richTextBox2.Lines[1] + " HCP = " + teamHcp);
                    teamsHcpArry[0] = teamHcp;
                    break;
                default:

                    break;
            }
        }

        private void randomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int team1g = 0, team2g = 0, team3g = 0, team4g = 0, team5g = 0;
            
            string blindString = null;
            
            //for integers
            Random rnd = new Random();

            int rIntF = 0, rIntB = 0;

            richTextBox1.Clear();

            for (int cntr = 0; cntr < (playerCnt - 1); cntr++)
            {
                richTextBox1.Text += listBox4.Items[cntr] + " " + listBox2.Items[cntr] + "\r\n";
                // Initialize the gold Tee Golfers Array to zero. This holds the number of players per team playing from the golds.
                if (cntr < 7) goldGolfers[cntr] = 0;
            }

            richTextBox1.Text.Trim();

            richTextBox1.SelectAll();

            richTextBox1.Text = richTextBox1.Text.Replace("+", "-");

            richTextBox2.Clear();
            richTextBox3.Clear();

            richTextBox2.ForeColor = Color.DarkBlue;
            
            richTextBox2.Font = new Font("Lucida Console", 10);

            switch (playerCnt - 1)
            {
                case 6:
                    var nums = Enumerable.Range(0, 6).ToArray();
                    var shuffle = new Random();

                    // Shuffle the array
                    for (int i = 0; i < nums.Length; ++i)
                    {
                        int randomIndex = shuffle.Next(nums.Length);
                        int temp = nums[randomIndex];
                        nums[randomIndex] = nums[i];
                        nums[i] = temp;
                    }
                    // Now your array is randomized and you can simply print them in order

                    if (MessageBox.Show("Do you want to play as THREESOMES?", "Check Information", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        richTextBox2.Text = "RANDOM DRAW for " + (playerCnt - 1).ToString() + " golfers\n";
                        richTextBox2.Text += "TEAM 1\n";
                        richTextBox2.Text += richTextBox1.Lines[nums[0]];
                        if (richTextBox1.Lines[0].Contains("*")) team1g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[3]];
                        if (richTextBox1.Lines[3].Contains("*")) team1g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[5]];
                        if (richTextBox1.Lines[5].Contains("*")) team1g++;
                        richTextBox2.Text += Environment.NewLine;
                        goldGolfers[0] = team1g;

                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += "TEAM 2\n";
                        richTextBox2.Text += richTextBox1.Lines[nums[1]];
                        if (richTextBox1.Lines[1].Contains("*")) team2g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[2]];
                        if (richTextBox1.Lines[2].Contains("*")) team2g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[4]];
                        if (richTextBox1.Lines[4].Contains("*")) team2g++;
                        richTextBox2.Text += Environment.NewLine;
                        goldGolfers[1] = team2g;

                        teamCnt = 2;

                        //Check the goldGolfer deficit between the MAX and MIN TEAMS
                        while (((goldGolfers[0] >= 2 || goldGolfers[1] >= 2) && (goldGolfers[0] == 0 || goldGolfers[1] == 0))
                            || ((goldGolfers[0] == 3 || goldGolfers[1] == 3) && (goldGolfers[0] == 1 || goldGolfers[1] == 1)))
                        {
                            determineTeamsToSwap(teamCnt);

                            swapGoldForWhiteThreesome();
                        }
                        //FOR 3-Player Teams use 17-11; FOR 4-Player Teams use 7-1
                        displayTeamHCPcalc(teamCnt + 10);
                    }
                    else
                    {
                        // TWO MAN TEAMS
                        richTextBox2.Text = "Match Teams for BLIND TWOSOMES\n";                            //0
                        richTextBox2.Text += "TEAM 1\n";                                //1
                        richTextBox2.Text += richTextBox1.Lines[nums[0]];               //2
                        if (richTextBox1.Lines[nums[0]].Contains("*")) team1g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[1]];               //3
                        if (richTextBox1.Lines[nums[1]].Contains("*")) team1g++;
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += Environment.NewLine;                       //4
                        richTextBox2.Text += Environment.NewLine;                       //5

                        richTextBox2.Text += "TEAM 2\n";                                //6
                        richTextBox2.Text += richTextBox1.Lines[nums[2]];               //7
                        if (richTextBox1.Lines[nums[2]].Contains("*")) team2g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[3]];               //8
                        if (richTextBox1.Lines[nums[3]].Contains("*")) team2g++;
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += Environment.NewLine;                       //9
                        richTextBox2.Text += Environment.NewLine;                       //10

                        richTextBox2.Text += "TEAM 3\n";                                //11
                        richTextBox2.Text += richTextBox1.Lines[nums[4]];               //12
                        if (richTextBox1.Lines[nums[4]].Contains("*")) team3g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[5]];               //13
                        if (richTextBox1.Lines[nums[5]].Contains("*")) team3g++;
                        richTextBox2.Text += Environment.NewLine;                        

                        twoManTeamsToolStripMenuItem.Visible = true;
                        hideLowManOutBtn = true;
                        twoManTeamsToolStripMenuItem_Click(null, null);
                    }

                    break;
                case 7:
                    // You have 7 Players and will use a blind draw
                    nums = Enumerable.Range(0, 7).ToArray();
                    shuffle = new Random();

                    // Shuffle the array
                    for (int i = 0; i < nums.Length; ++i)
                    {
                        int randomIndex = shuffle.Next(nums.Length);
                        int temp = nums[randomIndex];
                        nums[randomIndex] = nums[i];
                        nums[i] = temp;
                    }
                    // Now your array is randomized and you can simply print them in order
                    // TWO MAN TEAMS
                    richTextBox2.Text = "Match Teams for BLIND TWOSOMES\n";                            //0
                    richTextBox2.Text += "TEAM 1\n";                                //1
                    richTextBox2.Text += richTextBox1.Lines[nums[0]];               //2
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[1]];               //3
                    richTextBox2.Text += Environment.NewLine;

                    richTextBox2.Text += Environment.NewLine;                       //4
                    richTextBox2.Text += Environment.NewLine;                       //5

                    richTextBox2.Text += "TEAM 2\n";                                //6
                    richTextBox2.Text += richTextBox1.Lines[nums[2]];               //7
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[3]];               //8
                    richTextBox2.Text += Environment.NewLine;

                    richTextBox2.Text += Environment.NewLine;                       //9
                    richTextBox2.Text += Environment.NewLine;                       //10

                    richTextBox2.Text += "TEAM 3\n";                                //11
                    richTextBox2.Text += richTextBox1.Lines[nums[4]];               //12
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[5]];               //13
                    richTextBox2.Text += Environment.NewLine;

                    richTextBox2.Text += Environment.NewLine;                       //14
                    richTextBox2.Text += Environment.NewLine;                       //15

                    richTextBox2.Text += "TEAM 4\n";                                //16
                    richTextBox2.Text += richTextBox1.Lines[nums[6]];               //17
                    richTextBox2.Text += Environment.NewLine;

                    //Two man Teams and an ODD number of Players. There isn't a Draw, Kick Out Low man
                    twoManTeamsToolStripMenuItem.Visible = true;                    
                    twoManTeamsToolStripMenuItem_Click(null, null);
                    break;
                case 8:
                    nums = Enumerable.Range(0, 8).ToArray();
                    shuffle = new Random();

                    // Shuffle the array
                    for (int i = 0; i < nums.Length; ++i)
                    {
                        int randomIndex = shuffle.Next(nums.Length);
                        int temp = nums[randomIndex];
                        nums[randomIndex] = nums[i];
                        nums[i] = temp;
                    }

                    if (MessageBox.Show("Do you want to play as Foursomes?", "Check Information", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        teamCnt = 2;

                        richTextBox2.Text = "RANDOM DRAW for " + (playerCnt - 1).ToString() + " golfers\n";
                        richTextBox2.Text += "TEAM 1\n";                                            //rtb Line1
                        richTextBox2.Text += richTextBox1.Lines[nums[0]];                                 //rtb Line2
                        if (richTextBox1.Lines[0].Contains("*")) team1g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[3]];                                 //rtb Line3
                        if (richTextBox1.Lines[3].Contains("*")) team1g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[4]];                                 //rtb Line4
                        if (richTextBox1.Lines[4].Contains("*")) team1g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[7]];                                 //rtb Line5
                        if (richTextBox1.Lines[7].Contains("*")) team1g++;
                        richTextBox2.Text += Environment.NewLine;
                        goldGolfers[0] = team1g;

                        richTextBox2.Text += Environment.NewLine;                                 //rtb Line6

                        richTextBox2.Text += "TEAM 2\n";                                            //rtb Line7
                        richTextBox2.Text += richTextBox1.Lines[nums[1]];                                 //rtb Line8
                        if (richTextBox1.Lines[1].Contains("*")) team2g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[2]];                                 //rtb Line9
                        if (richTextBox1.Lines[2].Contains("*")) team2g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[5]];                                 //rtb Line10
                        if (richTextBox1.Lines[5].Contains("*")) team2g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[6]];                                 //rtb Line11
                        if (richTextBox1.Lines[6].Contains("*")) team2g++;
                        goldGolfers[1] = team2g;

                        //Check the goldGolfer deficit between the MAX and MIN TEAMS
                        while (((goldGolfers[0] >= 2 || goldGolfers[1] >= 2) && (goldGolfers[0] == 0 || goldGolfers[1] == 0))
                            || ((goldGolfers[0] >= 3 || goldGolfers[1] >= 3) && (goldGolfers[0] == 1 || goldGolfers[1] == 1))
                            || ((goldGolfers[0] == 4 || goldGolfers[1] == 4) && (goldGolfers[0] == 2 || goldGolfers[1] == 2)))
                        {
                            determineTeamsToSwap(teamCnt);

                            swapGoldForWhiteFoursome(minGoldTeamA, maxGoldTeamA);
                        }
                        displayTeamHCPcalc(teamCnt);
                    }
                    else
                    {
                        // Now your array is randomized and you can simply print them in order

                        // TWO MAN TEAMS FOR 8 PLAYERS
                        richTextBox2.Text = "Match Teams for BLIND TWOSOMES\n";                            //0
                        richTextBox2.Text += "TEAM 1\n";
                        richTextBox2.Text += richTextBox1.Lines[nums[0]];
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[1]];
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += "TEAM 2\n";
                        richTextBox2.Text += richTextBox1.Lines[nums[2]];
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[3]];
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += "TEAM 3\n";
                        richTextBox2.Text += richTextBox1.Lines[nums[4]];
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[5]];

                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += "TEAM 4\n";
                        richTextBox2.Text += richTextBox1.Lines[nums[6]];
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[7]];

                        twoManTeamsToolStripMenuItem.Visible = true;
                        
                        twoManTeamsToolStripMenuItem_Click(null, null);
                    }

                    break;
                case 9:
                    if (MessageBox.Show("Do you want to play as THREESOMES?", "Check Information", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        nums = Enumerable.Range(0, 9).ToArray();
                        shuffle = new Random();

                        // Shuffle the array
                        for (int i = 0; i < nums.Length; ++i)
                        {
                            int randomIndex = shuffle.Next(nums.Length);
                            int temp = nums[randomIndex];
                            nums[randomIndex] = nums[i];
                            nums[i] = temp;
                        }
                        // Now your array is randomized and you can simply print them in order
                        richTextBox2.Text = "RANDOM DRAW for " + (playerCnt - 1).ToString() + " golfers\n";
                        richTextBox2.Text += "TEAM 1\n";                                //1
                        richTextBox2.Text += richTextBox1.Lines[nums[0]];               //2
                        if (richTextBox1.Lines[nums[0]].Contains("*")) team1g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[5]];               //3
                        if (richTextBox1.Lines[nums[5]].Contains("*")) team1g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[6]];               //4
                        if (richTextBox1.Lines[nums[6]].Contains("*")) team1g++;
                        richTextBox2.Text += Environment.NewLine;
                        goldGolfers[0] = team1g;
                        richTextBox2.Text += Environment.NewLine;                       //5

                        richTextBox2.Text += "TEAM 2\n";                                //6
                        richTextBox2.Text += richTextBox1.Lines[nums[1]];               //7
                        if (richTextBox1.Lines[nums[1]].Contains("*")) team2g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[4]];               //8
                        if (richTextBox1.Lines[nums[4]].Contains("*")) team2g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[7]];               //9
                        if (richTextBox1.Lines[nums[7]].Contains("*")) team2g++;
                        richTextBox2.Text += Environment.NewLine;
                        goldGolfers[1] = team2g;
                        richTextBox2.Text += Environment.NewLine;                       //10

                        richTextBox2.Text += "TEAM 3\n";                                //11
                        richTextBox2.Text += richTextBox1.Lines[nums[2]];               //12
                        if (richTextBox1.Lines[nums[2]].Contains("*")) team3g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[3]];               //13
                        if (richTextBox1.Lines[nums[3]].Contains("*")) team3g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[8]];               //14
                        if (richTextBox1.Lines[nums[8]].Contains("*")) team3g++;
                        goldGolfers[2] = team3g;

                        teamCnt = 3;

                        //Check the goldGolfer deficit between the MAX and MIN TEAMS
                        while (((goldGolfers[0] >= 2 || goldGolfers[1] >= 2 || goldGolfers[2] >= 2) && (goldGolfers[0] == 0 || goldGolfers[1] == 0 || goldGolfers[2] == 0))
                            || ((goldGolfers[0] >= 3 || goldGolfers[1] >= 3 || goldGolfers[2] >= 3) && (goldGolfers[0] == 1 || goldGolfers[1] == 1 || goldGolfers[2] == 1))
                            || ((goldGolfers[0] == 4 || goldGolfers[1] == 4 || goldGolfers[2] == 4) && (goldGolfers[0] == 2 || goldGolfers[1] == 2 || goldGolfers[2] == 2)))
                        {
                            determineTeamsToSwap(teamCnt);

                            swapGoldForWhiteThreesome();
                        }
                        //FOR 3-Player Teams use 17-11; FOR 4-Player Teams use 7-1
                        displayTeamHCPcalc(teamCnt + 10);
                    }
                    else
                    {
                        nums = Enumerable.Range(0, 9).ToArray();

                        shuffle = new Random();

                        // Shuffle the array
                        for (int i = 0; i < nums.Length; ++i)
                        {
                            int randomIndex = shuffle.Next(nums.Length);
                            int temp = nums[randomIndex];
                            nums[randomIndex] = nums[i];
                            nums[i] = temp;
                        }
                        // Now your array is randomized and you can simply print them in order

                        richTextBox2.Text = "Match Teams for BLIND TWOSOMES\n";                            //0
                        richTextBox2.Text += "TEAM 1\n";
                        richTextBox2.Text += richTextBox1.Lines[nums[0]];
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[1]];
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += "TEAM 2\n";
                        richTextBox2.Text += richTextBox1.Lines[nums[2]];
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[3]];
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += "TEAM 3\n";
                        richTextBox2.Text += richTextBox1.Lines[nums[4]];
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[5]];
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += "TEAM 4\n";
                        richTextBox2.Text += richTextBox1.Lines[nums[6]];
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[7]];
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += "TEAM 5\n";
                        richTextBox2.Text += richTextBox1.Lines[nums[8]];
                        richTextBox2.Text += Environment.NewLine;

                        twoManTeamsToolStripMenuItem.Visible = true;
                        twoManTeamsToolStripMenuItem_Click(null, null);
                    }
                    break;
                case 10:
                    nums = Enumerable.Range(0, 10).ToArray();

                    shuffle = new Random();

                    // Shuffle the array
                    for (int i = 0; i < nums.Length; ++i)
                    {
                        int randomIndex = shuffle.Next(nums.Length);
                        int temp = nums[randomIndex];
                        nums[randomIndex] = nums[i];
                        nums[i] = temp;
                    }
                    // Now your array is randomized and you can simply print them in order
                    
                    if (MessageBox.Show("Do you want to play as BLIND TWOSOMES?", "Check Information", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {                        
                        richTextBox2.Text = "Match Teams for BLIND TWOSOMES\n";                            //0
                        richTextBox2.Text += "TEAM 1\n";
                        richTextBox2.Text += richTextBox1.Lines[nums[0]];
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[9]];
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += "TEAM 2\n";
                        richTextBox2.Text += richTextBox1.Lines[nums[1]];
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[8]];
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += "TEAM 3\n";
                        richTextBox2.Text += richTextBox1.Lines[nums[2]];
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[7]];
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += "TEAM 4\n";
                        richTextBox2.Text += richTextBox1.Lines[nums[3]];
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[6]];
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += "TEAM 5\n";
                        richTextBox2.Text += richTextBox1.Lines[nums[4]];
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[5]];


                        twoManTeamsToolStripMenuItem.Visible = true;
                        twoManTeamsToolStripMenuItem_Click(null, null);
                    }
                    else
                    {
                        MessageBox.Show("Four Man Teams will be selected using a BLIND DRAW.", "Information", MessageBoxButtons.OK);
                        teamCnt = 3;

                        richTextBox2.Text = "RANDOM DRAW for " + (playerCnt - 1).ToString() + " golfers\n";
                        richTextBox2.Text += "TEAM 1\n";                                            //1
                        richTextBox2.Text += richTextBox1.Lines[nums[0]];                                 //2
                        if (richTextBox1.Lines[nums[0]].Contains("*")) team1g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[5]];                                 //3
                        if (richTextBox1.Lines[nums[5]].Contains("*")) team1g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[6]];                                 //4
                        if (richTextBox1.Lines[nums[6]].Contains("*")) team1g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += "BLIND\n";                                             //5
                        goldGolfers[0] = team1g;

                        richTextBox2.Text += Environment.NewLine;                                   //6

                        richTextBox2.Text += "TEAM 2\n";                                            //7
                        richTextBox2.Text += richTextBox1.Lines[nums[1]];                                 //8
                        if (richTextBox1.Lines[nums[1]].Contains("*")) team2g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[4]];                                 //9
                        if (richTextBox1.Lines[nums[4]].Contains("*")) team2g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[7]];                                 //10
                        if (richTextBox1.Lines[nums[7]].Contains("*")) team2g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += "BLIND\n";                                             //11
                        goldGolfers[1] = team2g;

                        richTextBox2.Text += Environment.NewLine;                                   //12

                        richTextBox2.Text += "TEAM 3\n";                                            //13
                        richTextBox2.Text += richTextBox1.Lines[nums[2]];                                 //14
                        if (richTextBox1.Lines[nums[2]].Contains("*")) team3g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[3]];                                 //15
                        if (richTextBox1.Lines[nums[3]].Contains("*")) team3g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[8]];                                 //16
                        if (richTextBox1.Lines[nums[8]].Contains("*")) team3g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[9]];                                 //17
                        if (richTextBox1.Lines[nums[9]].Contains("*")) team3g++;
                        goldGolfers[2] = team3g;

                        blindString = richTextBox2.Lines[16] + "(F) " +
                            richTextBox2.Lines[17].TrimStart('1', '2', '3', '4', '5', '6', '7', '8', '9', '0', ' ') + "(B)";

                        changeLine(richTextBox2, 11, blindString);
                        changeLine(richTextBox2, 5, blindString);

                        //Check the goldGolfer deficit between the MAX and MIN TEAMS
                        while (((goldGolfers[0] >= 2 || goldGolfers[1] >= 2 || goldGolfers[2] >= 2) && (goldGolfers[0] == 0 || goldGolfers[1] == 0 || goldGolfers[2] == 0))
                            || ((goldGolfers[0] >= 3 || goldGolfers[1] >= 3 || goldGolfers[2] >= 3) && (goldGolfers[0] == 1 || goldGolfers[1] == 1 || goldGolfers[2] == 1))
                            || ((goldGolfers[0] == 4 || goldGolfers[1] == 4 || goldGolfers[2] == 4) && (goldGolfers[0] == 2 || goldGolfers[1] == 2 || goldGolfers[2] == 2)))
                        {
                            determineTeamsToSwap(teamCnt);

                            swapGoldForWhiteFoursome(minGoldTeamA, maxGoldTeamA);
                        }

                        displayTeamHCPcalc(teamCnt);
                    }
                    break;
                case 11:
                    if (MessageBox.Show("Do you want to play as BLIND TWOSOMES?", "Check Information", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        nums = Enumerable.Range(0, 11).ToArray();
                        shuffle = new Random();

                        // Shuffle the array
                        for (int i = 0; i < nums.Length; ++i)
                        {
                            int randomIndex = shuffle.Next(nums.Length);
                            int temp = nums[randomIndex];
                            nums[randomIndex] = nums[i];
                            nums[i] = temp;
                        }
                        // Now your array is randomized and you can simply print them in order
                        richTextBox2.Text = "Match Teams for BLIND TWOSOMES\n";                            //0
                        richTextBox2.Text += "TEAM 1\n";
                        richTextBox2.Text += richTextBox1.Lines[nums[0]];
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[9]];
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += "TEAM 2\n";
                        richTextBox2.Text += richTextBox1.Lines[nums[1]];
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[8]];
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += "TEAM 3\n";
                        richTextBox2.Text += richTextBox1.Lines[nums[2]];
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[7]];
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += "TEAM 4\n";
                        richTextBox2.Text += richTextBox1.Lines[nums[3]];
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[6]];
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += "TEAM 5\n";
                        richTextBox2.Text += richTextBox1.Lines[nums[4]];
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[5]];
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += "TEAM 6\n";
                        richTextBox2.Text += richTextBox1.Lines[nums[10]];
                        richTextBox2.Text += Environment.NewLine;

                        //Two man Teams and an ODD number of Players. There isn't a Draw, Kick Out Low man
                        twoManTeamsToolStripMenuItem.Visible = true;
                        twoManTeamsToolStripMenuItem_Click(null, null);
                    }
                    else
                    {
                        MessageBox.Show("Four Man Teams will be selected using a BLIND DRAW.", "Information", MessageBoxButtons.OK);
                        //Odd number of players                        
                        //so add a BLIND DRAW
                        avgPlayerHcp = totalHcp / 11;
                        
                        nums = Enumerable.Range(0, 11).ToArray();
                        shuffle = new Random();

                        // Shuffle the array
                        for (int i = 0; i < nums.Length; ++i)
                        {
                            int randomIndex = shuffle.Next(nums.Length);
                            int temp = nums[randomIndex];
                            nums[randomIndex] = nums[i];
                            nums[i] = temp;
                        }
                        // Now your array is randomized and you can simply print them in order

                        teamCnt = 3;

                        richTextBox2.Text = "RANDOM DRAW for " + (playerCnt - 1).ToString() + " golfers\n";
                        richTextBox2.Text += "TEAM 1\n";                                        //1
                        richTextBox2.Text += richTextBox1.Lines[nums[2]];                             //2
                        if (richTextBox1.Lines[nums[2]].Contains("*")) team1g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[3]];                             //3
                        if (richTextBox1.Lines[nums[3]].Contains("*")) team1g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[8]];                             //4
                        if (richTextBox1.Lines[nums[8]].Contains("*")) team1g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[9]];                             //5
                        if (richTextBox1.Lines[nums[9]].Contains("*")) team1g++;
                        richTextBox2.Text += Environment.NewLine;
                        goldGolfers[0] = team1g;

                        richTextBox2.Text += Environment.NewLine;                               //6

                        richTextBox2.Text += "TEAM 2\n";                                        //7
                        richTextBox2.Text += richTextBox1.Lines[nums[1]];                             //8
                        if (richTextBox1.Lines[nums[1]].Contains("*")) team2g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[4]];                             //9
                        if (richTextBox1.Lines[nums[4]].Contains("*")) team2g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[7]];                             //10
                        if (richTextBox1.Lines[nums[7]].Contains("*")) team2g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[10]];                            //11
                        if (richTextBox1.Lines[nums[10]].Contains("*")) team2g++;
                        richTextBox2.Text += Environment.NewLine;
                        goldGolfers[1] = team2g;

                        richTextBox2.Text += Environment.NewLine;                               //12

                        richTextBox2.Text += "TEAM 3\n";                                        //13
                        richTextBox2.Text += richTextBox1.Lines[nums[0]];                             //14
                        if (richTextBox1.Lines[nums[0]].Contains("*")) team3g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[5]];                             //15
                        if (richTextBox1.Lines[nums[5]].Contains("*")) team3g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[6]];                             //16
                        if (richTextBox1.Lines[nums[6]].Contains("*")) team3g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += "BLIND\n";                                         //17
                        goldGolfers[2] = team3g;

                        //Check the goldGolfer deficit between the MAX and MIN TEAMS
                        while (((goldGolfers[0] >= 2 || goldGolfers[1] >= 2 || goldGolfers[2] >= 2) && (goldGolfers[0] == 0 || goldGolfers[1] == 0 || goldGolfers[2] == 0))
                            || ((goldGolfers[0] >= 3 || goldGolfers[1] >= 3 || goldGolfers[2] >= 3) && (goldGolfers[0] == 1 || goldGolfers[1] == 1 || goldGolfers[2] == 1))
                            || ((goldGolfers[0] == 4 || goldGolfers[1] == 4 || goldGolfers[2] == 4) && (goldGolfers[0] == 2 || goldGolfers[1] == 2 || goldGolfers[2] == 2)))
                        {
                            determineTeamsToSwap(teamCnt);

                            swapGoldForWhiteFoursome(minGoldTeamA, maxGoldTeamA);
                        }
                        blindString = richTextBox2.Lines[5] + "(F) " +
                                richTextBox2.Lines[11].TrimStart('1', '2', '3', '4', '5', '6', '7', '8', '9', '0', ' ') + "(B)";

                        changeLine(richTextBox2, 17, blindString);
                        teamHcp = 0;
                        for (int cntr = 2; cntr < 6; cntr++)                    //4man team
                            teamHcp += calcTeamHcp(richTextBox2.Lines[cntr]);
                        changeLine(richTextBox2, 1, richTextBox2.Lines[1] + " HCP = " + teamHcp);

                        teamHcp = 0;
                        for (int cntr = 8; cntr < 12; cntr++)
                            teamHcp += calcTeamHcp(richTextBox2.Lines[cntr]);
                        changeLine(richTextBox2, 7, richTextBox2.Lines[7] + " HCP = " + teamHcp);
                        teamHcp = 0;
                        for (int cntr = 14; cntr < 17; cntr++)
                            teamHcp += calcTeamHcp(richTextBox2.Lines[cntr]);

                        teamHcp += calcTeamHcp(richTextBox2.Lines[10]);
                        changeLine(richTextBox2, 13, richTextBox2.Lines[13] + " HCP = " + teamHcp);

                        displayTeamHCPcalc(teamCnt);
                    }
                    break;
                case 12:
                    nums = Enumerable.Range(0, 12).ToArray();
                    shuffle = new Random();

                    // Shuffle the array
                    for (int i = 0; i < nums.Length; ++i)
                    {
                        int randomIndex = shuffle.Next(nums.Length);
                        int temp = nums[randomIndex];
                        nums[randomIndex] = nums[i];
                        nums[i] = temp;
                    }
                    // Now your array is randomized and you can simply print them in order
                    // You know you have 12 Players since there isn't a BLIND Draw
                    if (MessageBox.Show("Do you want to play as THREESOMES?", "Check Information", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        richTextBox2.Text = "RANDOM DRAW for " + (playerCnt - 1).ToString() + " golfers\n";
                        richTextBox2.Text += "TEAM 1\n";                                        //1
                        richTextBox2.Text += richTextBox1.Lines[nums[0]];                             //2
                        if (richTextBox1.Lines[nums[0]].Contains("*")) team1g = 1;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[7]];                             //3
                        if (richTextBox1.Lines[nums[7]].Contains("*")) team1g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[8]];                             //4
                        if (richTextBox1.Lines[nums[8]].Contains("*")) team1g++;
                        richTextBox2.Text += Environment.NewLine;
                        goldGolfers[0] = team1g;

                        richTextBox2.Text += Environment.NewLine;                               //5

                        richTextBox2.Text += "TEAM 2\n";                                        //6 
                        richTextBox2.Text += richTextBox1.Lines[nums[1]];                             //7
                        if (richTextBox1.Lines[nums[1]].Contains("*")) team2g = 1;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[6]];                             //8
                        if (richTextBox1.Lines[nums[6]].Contains("*")) team2g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[9]];                             //9
                        if (richTextBox1.Lines[nums[9]].Contains("*")) team2g++;
                        richTextBox2.Text += Environment.NewLine;
                        goldGolfers[1] = team2g;

                        richTextBox2.Text += Environment.NewLine;                               //10

                        richTextBox2.Text += "TEAM 3\n";                                        //11 
                        richTextBox2.Text += richTextBox1.Lines[nums[2]];                             //12  
                        if (richTextBox1.Lines[nums[2]].Contains("*")) team3g = 1;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[5]];                             //13 
                        if (richTextBox1.Lines[nums[5]].Contains("*")) team3g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[10]];                            //14
                        if (richTextBox1.Lines[nums[10]].Contains("*")) team3g++;
                        richTextBox2.Text += Environment.NewLine;
                        goldGolfers[2] = team3g;

                        richTextBox2.Text += Environment.NewLine;                               //15                               

                        richTextBox2.Text += "TEAM 4\n";                                        //16  
                        richTextBox2.Text += richTextBox1.Lines[nums[3]];                             //17 
                        if (richTextBox1.Lines[nums[3]].Contains("*")) team4g = 1;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[4]];                             //18
                        if (richTextBox1.Lines[nums[4]].Contains("*")) team4g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[11]];                            //19
                        if (richTextBox1.Lines[nums[11]].Contains("*")) team4g++;
                        richTextBox2.Text += Environment.NewLine;
                        goldGolfers[3] = team4g;
                        teamCnt = 4;

                        while (((goldGolfers[0] >= 2 || goldGolfers[1] >= 2 || goldGolfers[2] >= 2) && (goldGolfers[0] == 0 || goldGolfers[1] == 0 || goldGolfers[2] == 0))
                        || ((goldGolfers[0] >= 3 || goldGolfers[1] >= 3 || goldGolfers[2] >= 3) && (goldGolfers[0] == 1 || goldGolfers[1] == 1 || goldGolfers[2] == 1))
                        || ((goldGolfers[0] == 4 || goldGolfers[1] == 4 || goldGolfers[2] == 4) && (goldGolfers[0] == 2 || goldGolfers[1] == 2 || goldGolfers[2] == 2)))
                        {
                            determineTeamsToSwap(teamCnt);

                            swapGoldForWhiteThreesome();
                        }
                        displayTeamHCPcalc(teamCnt + 10);
                    }
                    else if (MessageBox.Show("Do you want to play as FOURSOMES?", "Check Information", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {                        
                        teamCnt = 3;
                        richTextBox2.Text = "RANDOM DRAW for " + (playerCnt - 1).ToString() + " golfers\n";
                        richTextBox2.Text += "TEAM 1\n";                                            //1
                        richTextBox2.Text += richTextBox1.Lines[nums[0]];                                 //2
                        if (richTextBox1.Lines[nums[0]].Contains("*")) team1g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[5]];                                 //3
                        if (richTextBox1.Lines[nums[5]].Contains("*")) team1g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[6]];                                 //4
                        if (richTextBox1.Lines[nums[6]].Contains("*")) team1g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[11]];                                //5
                        if (richTextBox1.Lines[nums[11]].Contains("*")) team1g++;
                        richTextBox2.Text += Environment.NewLine;
                        goldGolfers[0] = team1g;

                        richTextBox2.Text += Environment.NewLine;                                   //6

                        richTextBox2.Text += "TEAM 2\n";                                            //7
                        richTextBox2.Text += richTextBox1.Lines[nums[1]];                                 //8
                        if (richTextBox1.Lines[nums[1]].Contains("*")) team2g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[4]];                                 //9
                        if (richTextBox1.Lines[nums[4]].Contains("*")) team2g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[7]];                                 //10
                        if (richTextBox1.Lines[nums[7]].Contains("*")) team2g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[10]];                                //11
                        if (richTextBox1.Lines[nums[10]].Contains("*")) team2g++;
                        richTextBox2.Text += Environment.NewLine;
                        goldGolfers[1] = team2g;

                        richTextBox2.Text += Environment.NewLine;                                   //12

                        richTextBox2.Text += "TEAM 3\n";                                            //13
                        richTextBox2.Text += richTextBox1.Lines[nums[2]];                                 //14
                        if (richTextBox1.Lines[nums[2]].Contains("*")) team3g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[3]];                                 //15
                        if (richTextBox1.Lines[nums[3]].Contains("*")) team3g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[8]];                                 //16
                        if (richTextBox1.Lines[nums[8]].Contains("*")) team3g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[9]];                                 //17
                        if (richTextBox1.Lines[nums[9]].Contains("*")) team3g++;
                        goldGolfers[2] = team3g;

                        while (((goldGolfers[0] >= 2 || goldGolfers[1] >= 2 || goldGolfers[2] >= 2) && (goldGolfers[0] == 0 || goldGolfers[1] == 0 || goldGolfers[2] == 0))
                        || ((goldGolfers[0] >= 3 || goldGolfers[1] >= 3 || goldGolfers[2] >= 3) && (goldGolfers[0] == 1 || goldGolfers[1] == 1 || goldGolfers[2] == 1))
                        || ((goldGolfers[0] == 4 || goldGolfers[1] == 4 || goldGolfers[2] == 4) && (goldGolfers[0] == 2 || goldGolfers[1] == 2 || goldGolfers[2] == 2)))
                        {
                            determineTeamsToSwap(teamCnt);


                            swapGoldForWhiteFoursome(minGoldTeamA, maxGoldTeamA);
                        }
                        displayTeamHCPcalc(teamCnt);
                    }
                    else
                    {
                        AutoClosingMessageBox.Show("TWOSOMES using a Random Draw.", "APP INFO", 2500);
                        richTextBox2.Text = "Match Teams for BLIND TWOSOMES\n";                            //0
                        richTextBox2.Text += "TEAM 1\n";
                        richTextBox2.Text += richTextBox1.Lines[nums[0]];
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[1]];
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += "TEAM 2\n";
                        richTextBox2.Text += richTextBox1.Lines[nums[2]];
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[3]];
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += "TEAM 3\n";
                        richTextBox2.Text += richTextBox1.Lines[nums[4]];
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[5]];
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += "TEAM 4\n";
                        richTextBox2.Text += richTextBox1.Lines[nums[6]];
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[7]];
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += "TEAM 5\n";
                        richTextBox2.Text += richTextBox1.Lines[nums[8]];
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[9]];
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += "TEAM 6\n";
                        richTextBox2.Text += richTextBox1.Lines[nums[10]];
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[11]];
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += Environment.NewLine;

                        twoManTeamsToolStripMenuItem.Visible = true;
                        twoManTeamsToolStripMenuItem_Click(null, null);
                    }
                    break;
                case 13:
                    nums = Enumerable.Range(0, 13).ToArray();
                    shuffle = new Random();                    

                    // Shuffle the array
                    for (int i = 0; i < nums.Length; ++i)
                    {
                        int randomIndex = shuffle.Next(nums.Length);
                        int temp = nums[randomIndex];
                        nums[randomIndex] = nums[i];
                        nums[i] = temp;
                    }
                    // Now your array is randomized and you can simply print them in order
                    if (MessageBox.Show("Do you want to play as a Foursomes with 3 Threesomes getting a blind draw?", "Check Information", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        richTextBox2.Text = "RANDOM DRAW for " + (playerCnt - 1).ToString() + " golfers\n";
                        richTextBox2.Text += "TEAM 1\n";                                            //1
                        richTextBox2.Text += richTextBox1.Lines[0];                                 //2
                        if (richTextBox1.Lines[0].Contains("*")) team1g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[7];                                 //3
                        if (richTextBox1.Lines[7].Contains("*")) team1g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[8];                                 //4
                        if (richTextBox1.Lines[8].Contains("*")) team1g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += "BLIND\n";                                             //5
                        goldGolfers[0] = team1g;

                        richTextBox2.Text += Environment.NewLine;                                   //6                

                        richTextBox2.Text += "TEAM 2\n";                                            //7
                        richTextBox2.Text += richTextBox1.Lines[1];                                 //8
                        if (richTextBox1.Lines[1].Contains("*")) team2g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[6];                                 //9
                        if (richTextBox1.Lines[6].Contains("*")) team2g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[9];                                 //10
                        if (richTextBox1.Lines[9].Contains("*")) team2g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += "BLIND\n";                                             //11
                        goldGolfers[1] = team2g;
                        //12
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += "TEAM 3\n";                                            //13
                        richTextBox2.Text += richTextBox1.Lines[2];                                 //14
                        if (richTextBox1.Lines[2].Contains("*")) team3g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[5];                                 //15
                        if (richTextBox1.Lines[5].Contains("*")) team3g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[10];                                //16
                        if (richTextBox1.Lines[10].Contains("*")) team3g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += "BLIND\n";                                             //17
                        goldGolfers[2] = team3g;

                        richTextBox2.Text += Environment.NewLine;                                   //18

                        richTextBox2.Text += "TEAM 4\n";                                            //19
                        richTextBox2.Text += richTextBox1.Lines[3];                                 //20
                        if (richTextBox1.Lines[3].Contains("*")) team4g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[4];                                 //21
                        if (richTextBox1.Lines[4].Contains("*")) team4g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[11];                                //22
                        if (richTextBox1.Lines[11].Contains("*")) team4g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[12];                                //23
                        if (richTextBox1.Lines[12].Contains("*")) team4g++;
                        richTextBox2.Text += Environment.NewLine;
                        goldGolfers[3] = team4g;                                                    //24
                        teamCnt = 4;

                        // If there are multiple teams that have 3 Gold and One White run the swap again
                        while (((goldGolfers[0] >= 2 || goldGolfers[1] >= 2 || goldGolfers[2] >= 2 || goldGolfers[3] >= 2) && (goldGolfers[0] == 0 || goldGolfers[1] == 0 || goldGolfers[2] == 0 || goldGolfers[3] == 0))
                            || ((goldGolfers[0] >= 3 || goldGolfers[1] >= 3 || goldGolfers[2] >= 3 || goldGolfers[3] >= 3) && (goldGolfers[0] == 1 || goldGolfers[1] == 1 || goldGolfers[2] == 1 || goldGolfers[3] == 1))
                            || ((goldGolfers[0] == 4 || goldGolfers[1] == 4 || goldGolfers[2] == 4 || goldGolfers[3] == 4) && (goldGolfers[0] == 2 || goldGolfers[1] == 2 || goldGolfers[2] == 2 || goldGolfers[3] == 2)))
                        {
                            determineTeamsToSwap(teamCnt);

                            swapGoldForWhiteFoursome(minGoldTeamA, maxGoldTeamA);
                        }

                        blindString = richTextBox2.Lines[22] + "(F) " +
                            richTextBox2.Lines[23].TrimStart('1', '2', '3', '4', '5', '6', '7', '8', '9', '0', ' ') + "(B)";
                        changeLine(richTextBox2, 5, blindString);
                        changeLine(richTextBox2, 11, blindString);
                        changeLine(richTextBox2, 17, blindString);

                        displayTeamHCPcalc(teamCnt);
                    }
                    else
                    {
                        MessageBox.Show("Two Man Teams will be selected.", "Information", MessageBoxButtons.OK);

                        richTextBox2.Text = "Match Teams for BLIND TWOSOMES\n";                            //0
                        richTextBox2.Text += "TEAM 1\n";
                        richTextBox2.Text += richTextBox1.Lines[nums[0]];
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[1]];
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += "TEAM 2\n";
                        richTextBox2.Text += richTextBox1.Lines[nums[2]];
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[3]];
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += "TEAM 3\n";
                        richTextBox2.Text += richTextBox1.Lines[nums[4]];
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[5]];
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += "TEAM 4\n";
                        richTextBox2.Text += richTextBox1.Lines[nums[6]];
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[7]];
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += "TEAM 5\n";
                        richTextBox2.Text += richTextBox1.Lines[nums[8]];
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[9]];
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += "TEAM 6\n";
                        richTextBox2.Text += richTextBox1.Lines[nums[10]];
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[11]];
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += "TEAM 7\n";
                        richTextBox2.Text += richTextBox1.Lines[nums[12]];
                        richTextBox2.Text += Environment.NewLine;

                        twoManTeamsToolStripMenuItem.Visible = true;
                        twoManTeamsToolStripMenuItem_Click(null, null);
                    }
                    break;
                case 14:
                    nums = Enumerable.Range(0, 14).ToArray();
                    shuffle = new Random();

                    // Shuffle the array
                    for (int i = 0; i < nums.Length; ++i)
                    {
                        int randomIndex = shuffle.Next(nums.Length);
                        int temp = nums[randomIndex];
                        nums[randomIndex] = nums[i];
                        nums[i] = temp;
                    }
                    // Now your array is randomized and you can simply print them in order

                    if (MessageBox.Show("Do you want to play as Foursomes with a blind draw?", "Check Information", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        richTextBox2.Text = "RANDOM DRAW for " + (playerCnt - 1).ToString() + " golfers\n";
                        richTextBox2.Text += "TEAM 1\n";                                //1
                        richTextBox2.Text += richTextBox1.Lines[nums[0]];                     //2
                        if (richTextBox1.Lines[nums[0]].Contains("*")) team1g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[7]];                     //3
                        if (richTextBox1.Lines[nums[7]].Contains("*")) team1g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[8]];                     //4
                        if (richTextBox1.Lines[nums[8]].Contains("*")) team1g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += "BLIND\n";                                 //5            
                        goldGolfers[0] = team1g;

                        richTextBox2.Text += Environment.NewLine;                       //6 

                        richTextBox2.Text += "TEAM 2\n";                                //7
                        richTextBox2.Text += richTextBox1.Lines[nums[1]];                     //8
                        if (richTextBox1.Lines[nums[1]].Contains("*")) team2g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[6]];                     //9
                        if (richTextBox1.Lines[nums[6]].Contains("*")) team2g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[9]];                     //10
                        if (richTextBox1.Lines[nums[9]].Contains("*")) team2g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += "BLIND\n";                                 //11            
                        goldGolfers[1] = team2g;
                        teamCnt = 2;

                        richTextBox2.Text += Environment.NewLine;                       //12

                        richTextBox2.Text += "TEAM 3\n";                                //13
                        richTextBox2.Text += richTextBox1.Lines[nums[2]];                     //14
                        if (richTextBox1.Lines[nums[2]].Contains("*")) team3g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[5]];                     //15
                        if (richTextBox1.Lines[nums[5]].Contains("*")) team3g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[10]];                    //16
                        if (richTextBox1.Lines[nums[10]].Contains("*")) team3g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[13]];                    //17
                        if (richTextBox1.Lines[nums[13]].Contains("*")) team3g++;
                        richTextBox2.Text += Environment.NewLine;
                        goldGolfers[2] = team3g;

                        richTextBox2.Text += Environment.NewLine;                       //18

                        richTextBox2.Text += "TEAM 4\n";                                //19
                        richTextBox2.Text += richTextBox1.Lines[nums[3]];                     //20
                        if (richTextBox1.Lines[nums[3]].Contains("*")) team4g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[4]];                     //21
                        if (richTextBox1.Lines[nums[4]].Contains("*")) team4g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[11]];                    //22
                        if (richTextBox1.Lines[nums[11]].Contains("*")) team4g++;
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[12]];                    //23
                        if (richTextBox1.Lines[nums[12]].Contains("*")) team4g++;
                        richTextBox2.Text += Environment.NewLine;
                        goldGolfers[3] = team4g;
                        teamCnt = 4;

                        // If there are multiple teams that have 3 Gold and One White run the swap again
                        while (((goldGolfers[0] >= 2 || goldGolfers[1] >= 2 || goldGolfers[2] >= 2 || goldGolfers[3] >= 2) && (goldGolfers[0] == 0 || goldGolfers[1] == 0 || goldGolfers[2] == 0 || goldGolfers[3] == 0))
                            || ((goldGolfers[0] >= 3 || goldGolfers[1] >= 3 || goldGolfers[2] >= 3 || goldGolfers[3] >= 3) && (goldGolfers[0] == 1 || goldGolfers[1] == 1 || goldGolfers[2] == 1 || goldGolfers[3] == 1))
                            || ((goldGolfers[0] == 4 || goldGolfers[1] == 4 || goldGolfers[2] == 4 || goldGolfers[3] == 4) && (goldGolfers[0] == 2 || goldGolfers[1] == 2 || goldGolfers[2] == 2 || goldGolfers[3] == 2)))
                        {
                            determineTeamsToSwap(teamCnt);

                            swapGoldForWhiteFoursome(minGoldTeamA, maxGoldTeamA);
                        }

                        blindString = richTextBox2.Lines[17] + "(F) " +
                                richTextBox2.Lines[23].TrimStart('1', '2', '3', '4', '5', '6', '7', '8', '9', '0', ' ') + "(B)";

                        changeLine(richTextBox2, 5, blindString);
                        changeLine(richTextBox2, 11, blindString);

                        displayTeamHCPcalc(teamCnt);
                    }
                    else
                    {
                        MessageBox.Show("Two Man Teams will be selected.", "Information", MessageBoxButtons.OK);

                        richTextBox2.Text = "Match Teams for BLIND TWOSOMES\n";                            //0
                        richTextBox2.Text += "TEAM 1\n";
                        richTextBox2.Text += richTextBox1.Lines[nums[0]];
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[9]];
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += "TEAM 2\n";
                        richTextBox2.Text += richTextBox1.Lines[nums[1]];
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[8]];
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += "TEAM 3\n";
                        richTextBox2.Text += richTextBox1.Lines[nums[2]];
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[7]];
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += "TEAM 4\n";
                        richTextBox2.Text += richTextBox1.Lines[nums[3]];
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[6]];
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += "TEAM 5\n";
                        richTextBox2.Text += richTextBox1.Lines[nums[4]];
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[5]];
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += "TEAM 6\n";
                        richTextBox2.Text += richTextBox1.Lines[nums[10]];
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[11]];
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += Environment.NewLine;

                        richTextBox2.Text += "TEAM 7\n";
                        richTextBox2.Text += richTextBox1.Lines[nums[12]];
                        richTextBox2.Text += Environment.NewLine;
                        richTextBox2.Text += richTextBox1.Lines[nums[13]];

                        twoManTeamsToolStripMenuItem.Visible = true;
                        twoManTeamsToolStripMenuItem_Click(null, null);
                    }
                    
                    break;
                case 15:
                    nums = Enumerable.Range(0, 15).ToArray();
                    shuffle = new Random();

                    // Shuffle the array
                    for (int i = 0; i < nums.Length; ++i)
                    {
                        int randomIndex = shuffle.Next(nums.Length);
                        int temp = nums[randomIndex];
                        nums[randomIndex] = nums[i];
                        nums[i] = temp;
                    }
                    // Now your array is randomized and you can simply print them in order
                    richTextBox2.Text = "RANDOM DRAW for " + (playerCnt - 1).ToString() + " golfers\n";
                    richTextBox2.Text += "TEAM 1\n";                            //1
                    richTextBox2.Text += richTextBox1.Lines[nums[0]];                 //2
                    if (richTextBox1.Lines[nums[0]].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[9]];                 //3
                    if (richTextBox1.Lines[nums[9]].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[10]];                //4
                    if (richTextBox1.Lines[nums[10]].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[0] = team1g;

                    richTextBox2.Text += Environment.NewLine;                   //5

                    richTextBox2.Text += "TEAM 2\n";                            //6 
                    richTextBox2.Text += richTextBox1.Lines[nums[1]];                 //7
                    if (richTextBox1.Lines[nums[1]].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[8]];                 //8
                    if (richTextBox1.Lines[nums[8]].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[11]];                //9
                    if (richTextBox1.Lines[nums[11]].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[1] = team2g;

                    richTextBox2.Text += Environment.NewLine;                   //10

                    richTextBox2.Text += "TEAM 3\n";                            //11 
                    richTextBox2.Text += richTextBox1.Lines[nums[2]];                 //12 
                    if (richTextBox1.Lines[nums[2]].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[7]];                 //13 
                    if (richTextBox1.Lines[nums[7]].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[12]];                //14
                    if (richTextBox1.Lines[nums[12]].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[2] = team3g;

                    richTextBox2.Text += Environment.NewLine;                   //15 

                    richTextBox2.Text += "TEAM 4\n";                            //16 
                    richTextBox2.Text += richTextBox1.Lines[nums[3]];                 //17 
                    if (richTextBox1.Lines[nums[3]].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[6]];                 //18
                    if (richTextBox1.Lines[nums[6]].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[13]];                //19
                    if (richTextBox1.Lines[nums[13]].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[3] = team4g;

                    richTextBox2.Text += Environment.NewLine;                   //20

                    richTextBox2.Text += "TEAM 5\n";                            //21
                    richTextBox2.Text += richTextBox1.Lines[nums[4]];                 //22
                    if (richTextBox1.Lines[nums[4]].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[5]];                 //23
                    if (richTextBox1.Lines[nums[5]].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[14]];                //24
                    if (richTextBox1.Lines[nums[14]].Contains("*")) team5g++;
                    goldGolfers[4] = team5g;
                    teamCnt = 5;

                    // If there are multiple teams that have 3 Gold and One White run the swap again
                    while (((goldGolfers[0] >= 2 || goldGolfers[1] >= 2 || goldGolfers[2] >= 2 || goldGolfers[3] >= 2 || goldGolfers[4] >= 2) && (goldGolfers[0] == 0 || goldGolfers[1] == 0 || goldGolfers[2] == 0 || goldGolfers[3] == 0 || goldGolfers[4] == 0))
                        || ((goldGolfers[0] >= 3 || goldGolfers[1] >= 3 || goldGolfers[2] >= 3 || goldGolfers[3] >= 3 || goldGolfers[4] >= 3) && (goldGolfers[0] == 1 || goldGolfers[1] == 1 || goldGolfers[2] == 1 || goldGolfers[3] == 1 || goldGolfers[4] == 1))
                        || ((goldGolfers[0] == 4 || goldGolfers[1] == 4 || goldGolfers[2] == 4 || goldGolfers[3] == 4 || goldGolfers[4] == 4) && (goldGolfers[0] == 2 || goldGolfers[1] == 2 || goldGolfers[2] == 2 || goldGolfers[3] == 2 || goldGolfers[4] == 2)))
                    {
                        determineTeamsToSwap(teamCnt);
                        swapGoldForWhiteThreesome();
                    }
                    displayTeamHCPcalc((teamCnt + 10));
                    break;
                case 16:
                    nums = Enumerable.Range(0, 16).ToArray();
                    shuffle = new Random();

                    // Shuffle the array
                    for (int i = 0; i < nums.Length; ++i)
                    {
                        int randomIndex = shuffle.Next(nums.Length);
                        int temp = nums[randomIndex];
                        nums[randomIndex] = nums[i];
                        nums[i] = temp;
                    }
                    // Now your array is randomized and you can simply print them in order
                    richTextBox2.Text = "RANDOM DRAW for " + (playerCnt - 1).ToString() + " golfers\n";
                    richTextBox2.Text += "TEAM 1\n";
                    richTextBox2.Text += richTextBox1.Lines[nums[0]];
                    if (richTextBox1.Lines[nums[0]].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[7]];
                    if (richTextBox1.Lines[nums[7]].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[8]];
                    if (richTextBox1.Lines[nums[8]].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[15]];
                    if (richTextBox1.Lines[nums[15]].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[0] = team1g;

                    richTextBox2.Text += Environment.NewLine;

                    richTextBox2.Text += "TEAM 2\n";
                    richTextBox2.Text += richTextBox1.Lines[nums[1]];
                    if (richTextBox1.Lines[nums[1]].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[6]];
                    if (richTextBox1.Lines[nums[6]].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[9]];
                    if (richTextBox1.Lines[nums[9]].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[14]];
                    if (richTextBox1.Lines[nums[14]].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[1] = team2g;

                    richTextBox2.Text += Environment.NewLine;

                    richTextBox2.Text += "TEAM 3\n";
                    richTextBox2.Text += richTextBox1.Lines[nums[2]];
                    if (richTextBox1.Lines[nums[2]].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[5]];
                    if (richTextBox1.Lines[nums[5]].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[10]];
                    if (richTextBox1.Lines[nums[10]].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[13]];
                    if (richTextBox1.Lines[nums[13]].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[2] = team3g;

                    richTextBox2.Text += Environment.NewLine;

                    richTextBox2.Text += "TEAM 4\n";
                    richTextBox2.Text += richTextBox1.Lines[nums[3]];
                    if (richTextBox1.Lines[nums[3]].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[4]];
                    if (richTextBox1.Lines[nums[4]].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[11]];
                    if (richTextBox1.Lines[nums[11]].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[12]];
                    if (richTextBox1.Lines[nums[12]].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[3] = team4g;
                    teamCnt = 4;

                    // If there are multiple teams that have 3 Gold and One White run the swap again
                    while (((goldGolfers[0] >= 2 || goldGolfers[1] >= 2 || goldGolfers[2] >= 2 || goldGolfers[3] >= 2) && (goldGolfers[0] == 0 || goldGolfers[1] == 0 || goldGolfers[2] == 0 || goldGolfers[3] == 0))
                        || ((goldGolfers[0] >= 3 || goldGolfers[1] >= 3 || goldGolfers[2] >= 3 || goldGolfers[3] >= 3) && (goldGolfers[0] == 1 || goldGolfers[1] == 1 || goldGolfers[2] == 1 || goldGolfers[3] == 1))
                        || ((goldGolfers[0] == 4 || goldGolfers[1] == 4 || goldGolfers[2] == 4 || goldGolfers[3] == 4) && (goldGolfers[0] == 2 || goldGolfers[1] == 2 || goldGolfers[2] == 2 || goldGolfers[3] == 2)))
                    {
                        determineTeamsToSwap(teamCnt);

                        swapGoldForWhiteFoursome(minGoldTeamA, maxGoldTeamA);
                    }

                    displayTeamHCPcalc(teamCnt);
                    break;
                case 17:
                    nums = Enumerable.Range(0, 17).ToArray();
                    shuffle = new Random();

                    // Shuffle the array
                    for (int i = 0; i < nums.Length; ++i)
                    {
                        int randomIndex = shuffle.Next(nums.Length);
                        int temp = nums[randomIndex];
                        nums[randomIndex] = nums[i];
                        nums[i] = temp;
                    }
                    // Now your array is randomized and you can simply print them in order
                    richTextBox2.Text = "RANDOM DRAW for " + (playerCnt - 1).ToString() + " golfers\n";
                    richTextBox2.Text += "TEAM 1\n";                                    //1
                    richTextBox2.Text += richTextBox1.Lines[nums[0]];                         //2
                    if (richTextBox1.Lines[nums[0]].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[9]];                         //3
                    if (richTextBox1.Lines[nums[9]].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[10]];                        //4
                    if (richTextBox1.Lines[nums[10]].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += "BLIND\n";                                     //5
                    goldGolfers[0] = team1g;

                    richTextBox2.Text += Environment.NewLine;                           //6

                    richTextBox2.Text += "TEAM 2\n";                                    //7
                    richTextBox2.Text += richTextBox1.Lines[nums[1]];                         //8
                    if (richTextBox1.Lines[nums[1]].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[8]];                         //9
                    if (richTextBox1.Lines[nums[8]].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[11]];                        //10
                    if (richTextBox1.Lines[nums[11]].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += "BLIND\n";                                     //11       
                    goldGolfers[1] = team2g;

                    richTextBox2.Text += Environment.NewLine;                           //12

                    richTextBox2.Text += "TEAM 3\n";                                    //13
                    richTextBox2.Text += richTextBox1.Lines[nums[2]];                         //14
                    if (richTextBox1.Lines[nums[2]].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[7]];                         //15
                    if (richTextBox1.Lines[nums[7]].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[12]];                        //16
                    if (richTextBox1.Lines[nums[12]].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += "BLIND\n";                                     //17       
                    goldGolfers[2] = team3g;
                    teamCnt = 3;
                    //18
                    richTextBox2.Text += Environment.NewLine;

                    richTextBox2.Text += "TEAM 4\n";                                    //19
                    richTextBox2.Text += richTextBox1.Lines[nums[3]];                         //20
                    if (richTextBox1.Lines[nums[3]].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[6]];                         //21
                    if (richTextBox1.Lines[nums[6]].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[13]];                        //22
                    if (richTextBox1.Lines[nums[13]].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[16]];                        //23
                    if (richTextBox1.Lines[nums[16]].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[3] = team4g;                                            //24
                    teamCnt = 4;

                    richTextBox2.Text += Environment.NewLine;                           //25

                    richTextBox2.Text += "TEAM 5\n";                                    //26
                    richTextBox2.Text += richTextBox1.Lines[nums[4]];                         //27
                    if (richTextBox1.Lines[nums[4]].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[5]];                         //28
                    if (richTextBox1.Lines[nums[5]].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[14]];                        //29
                    if (richTextBox1.Lines[nums[14]].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[15]];                        //30
                    if (richTextBox1.Lines[nums[15]].Contains("*")) team5g++;
                    goldGolfers[4] = team5g;
                    teamCnt = 5;

                    // If there are multiple teams that have 3 Gold and One White run the swap again
                    while (((goldGolfers[0] >= 2 || goldGolfers[1] >= 2 || goldGolfers[2] >= 2 || goldGolfers[3] >= 2 || goldGolfers[4] >= 2) && (goldGolfers[0] == 0 || goldGolfers[1] == 0 || goldGolfers[2] == 0 || goldGolfers[3] == 0 || goldGolfers[4] == 0))
                        || ((goldGolfers[0] >= 3 || goldGolfers[1] >= 3 || goldGolfers[2] >= 3 || goldGolfers[3] >= 3 || goldGolfers[4] >= 3) && (goldGolfers[0] == 1 || goldGolfers[1] == 1 || goldGolfers[2] == 1 || goldGolfers[3] == 1 || goldGolfers[4] == 1))
                        || ((goldGolfers[0] == 4 || goldGolfers[1] == 4 || goldGolfers[2] == 4 || goldGolfers[3] == 4 || goldGolfers[4] == 4) && (goldGolfers[0] == 2 || goldGolfers[1] == 2 || goldGolfers[2] == 2 || goldGolfers[3] == 2 || goldGolfers[4] == 2)))
                    {
                        determineTeamsToSwap(teamCnt);

                        swapGoldForWhiteFoursome(minGoldTeamA, maxGoldTeamA);
                    }

                    //Make the BLIND after the teams are balanced for GOLD/WHITE
                    blindString = richTextBox2.Lines[23] + "(F) " +
                            richTextBox2.Lines[24].TrimStart('1', '2', '3', '4', '5', '6', '7', '8', '9', '0', ' ') + "(B)";

                    changeLine(richTextBox2, 5, blindString);
                    changeLine(richTextBox2, 11, blindString);
                    changeLine(richTextBox2, 17, blindString);

                    displayTeamHCPcalc(teamCnt);
                    break;
                case 18:
                    nums = Enumerable.Range(0, 18).ToArray();
                    shuffle = new Random();

                    // Shuffle the array
                    for (int i = 0; i < nums.Length; ++i)
                    {
                        int randomIndex = shuffle.Next(nums.Length);
                        int temp = nums[randomIndex];
                        nums[randomIndex] = nums[i];
                        nums[i] = temp;
                    }
                    // Now your array is randomized and you can simply print them in order
                    richTextBox2.Text = "RANDOM DRAW for " + (playerCnt - 1).ToString() + " golfers\n";
                    richTextBox2.Text += "TEAM 1\n";                                //1
                    richTextBox2.Text += richTextBox1.Lines[nums[0]];                     //2
                    if (richTextBox1.Lines[nums[0]].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[11]];                    //3
                    if (richTextBox1.Lines[nums[11]].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[12]];                    //4
                    if (richTextBox1.Lines[nums[12]].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[0] = team1g;

                    richTextBox2.Text += Environment.NewLine;                       //5

                    richTextBox2.Text += "TEAM 2\n";                                //6 
                    richTextBox2.Text += richTextBox1.Lines[nums[1]];                     //7
                    if (richTextBox1.Lines[nums[1]].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[10]];                    //8
                    if (richTextBox1.Lines[nums[10]].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[13]];                    //9
                    if (richTextBox1.Lines[nums[13]].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[1] = team2g;

                    richTextBox2.Text += Environment.NewLine;                       //10

                    richTextBox2.Text += "TEAM 3\n";                                //11
                    richTextBox2.Text += richTextBox1.Lines[nums[2]];                     //12
                    if (richTextBox1.Lines[nums[2]].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[9]];                     //13
                    if (richTextBox1.Lines[nums[9]].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[14]];                    //14
                    if (richTextBox1.Lines[nums[14]].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[2] = team3g;

                    richTextBox2.Text += Environment.NewLine;                       //15

                    richTextBox2.Text += "TEAM 4\n";                                //16
                    richTextBox2.Text += richTextBox1.Lines[nums[3]];                     //17
                    if (richTextBox1.Lines[nums[3]].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[8]];                     //18
                    if (richTextBox1.Lines[nums[8]].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[15]];                    //19
                    if (richTextBox1.Lines[nums[15]].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[3] = team4g;

                    richTextBox2.Text += Environment.NewLine;                       //20

                    richTextBox2.Text += "TEAM 5\n";                                //21
                    richTextBox2.Text += richTextBox1.Lines[nums[4]];                     //22
                    if (richTextBox1.Lines[nums[4]].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[7]];                     //23
                    if (richTextBox1.Lines[nums[7]].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[16]];                    //24
                    if (richTextBox1.Lines[nums[16]].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[4] = team5g;

                    richTextBox2.Text += Environment.NewLine;                       //25

                    richTextBox2.Text += "TEAM 6\n";                                //26
                    richTextBox2.Text += richTextBox1.Lines[nums[5]];                     //27
                    if (richTextBox1.Lines[nums[5]].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[6]];                     //28
                    if (richTextBox1.Lines[nums[6]].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[17]];                    //29
                    if (richTextBox1.Lines[nums[17]].Contains("*")) team6g++;
                    goldGolfers[5] = team6g;
                    teamCnt = 6;

                    // If there are multiple teams that have 3 Gold and One White run the swap again
                    while (((goldGolfers[0] >= 2 || goldGolfers[1] >= 2 || goldGolfers[2] >= 2 || goldGolfers[3] >= 2 || goldGolfers[4] >= 2 || goldGolfers[5] >= 2) && (goldGolfers[0] == 0 || goldGolfers[1] == 0 || goldGolfers[2] == 0 || goldGolfers[3] == 0 || goldGolfers[4] == 0 || goldGolfers[5] == 0))
                        || ((goldGolfers[0] >= 3 || goldGolfers[1] >= 3 || goldGolfers[2] >= 3 || goldGolfers[3] >= 3 || goldGolfers[4] >= 3 || goldGolfers[5] >= 3) && (goldGolfers[0] == 1 || goldGolfers[1] == 1 || goldGolfers[2] == 1 || goldGolfers[3] == 1 || goldGolfers[4] == 1 || goldGolfers[5] == 1))
                        || ((goldGolfers[0] == 4 || goldGolfers[1] == 4 || goldGolfers[2] == 4 || goldGolfers[3] == 4 || goldGolfers[4] == 4 || goldGolfers[5] == 4) && (goldGolfers[0] == 2 || goldGolfers[1] == 2 || goldGolfers[2] == 2 || goldGolfers[3] == 2 || goldGolfers[4] == 2 || goldGolfers[5] == 2)))
                    {
                        determineTeamsToSwap(teamCnt);

                        swapGoldForWhiteThreesome();
                    }
                    displayTeamHCPcalc(teamCnt + 10);
                    break;
                case 19:
                    nums = Enumerable.Range(0, 19).ToArray();
                    shuffle = new Random();

                    // Shuffle the array
                    for (int i = 0; i < nums.Length; ++i)
                    {
                        int randomIndex = shuffle.Next(nums.Length);
                        int temp = nums[randomIndex];
                        nums[randomIndex] = nums[i];
                        nums[i] = temp;
                    }
                    // Now your array is randomized and you can simply print them in order
                    richTextBox2.Text = "RANDOM DRAW for " + (playerCnt - 1).ToString() + " golfers\n";
                    richTextBox2.Text += "TEAM 1\n";                                        //1
                    richTextBox2.Text += richTextBox1.Lines[nums[4]];                             //2
                    if (richTextBox1.Lines[nums[4]].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[5]];                             //3
                    if (richTextBox1.Lines[nums[5]].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[14]];                            //4
                    if (richTextBox1.Lines[nums[14]].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[15]];                            //5
                    if (richTextBox1.Lines[nums[15]].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[0] = team1g;

                    richTextBox2.Text += Environment.NewLine;                               //6

                    richTextBox2.Text += "TEAM 2\n";                                        //7
                    richTextBox2.Text += richTextBox1.Lines[nums[1]];                             //8
                    if (richTextBox1.Lines[nums[1]].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[8]];                             //9
                    if (richTextBox1.Lines[nums[8]].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[11]];                            //10
                    if (richTextBox1.Lines[nums[11]].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[18]];                            //11 
                    if (richTextBox1.Lines[nums[18]].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[1] = team2g;

                    richTextBox2.Text += Environment.NewLine;                               //12

                    richTextBox2.Text += "TEAM 3\n";                                        //13
                    richTextBox2.Text += richTextBox1.Lines[nums[2]];                             //14
                    if (richTextBox1.Lines[nums[2]].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[7]];                             //15
                    if (richTextBox1.Lines[nums[7]].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[12]];                            //16
                    if (richTextBox1.Lines[nums[12]].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[17]];                            //17 
                    if (richTextBox1.Lines[nums[17]].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[2] = team3g;

                    richTextBox2.Text += Environment.NewLine;                               //18

                    richTextBox2.Text += "TEAM 4\n";                                        //19
                    richTextBox2.Text += richTextBox1.Lines[nums[3]];                             //20
                    if (richTextBox1.Lines[nums[3]].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[6]];                             //21
                    if (richTextBox1.Lines[nums[6]].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[13]];                            //22
                    if (richTextBox1.Lines[nums[13]].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[16]];                            //23
                    if (richTextBox1.Lines[nums[16]].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[3] = team4g;

                    richTextBox2.Text += Environment.NewLine;                               //24

                    richTextBox2.Text += "TEAM 5\n";                                        //25
                    richTextBox2.Text += richTextBox1.Lines[nums[0]];                             //26
                    if (richTextBox1.Lines[nums[0]].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[9]];                             //27
                    if (richTextBox1.Lines[nums[9]].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[10]];                            //28
                    if (richTextBox1.Lines[nums[10]].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    //29
                    rIntF = rnd.Next(15, 19);
                    do
                    {
                        rIntB = rnd.Next(15, 19);
                    } while (rIntF == rIntB);

                    //Make the BLIND before the teams are balanced for GOLD/WHITEE                    
                    richTextBox2.Text += richTextBox1.Lines[rIntF] + "(F) " +
                            richTextBox1.Lines[rIntB].TrimStart('1', '2', '3', '4', '5', '6', '7', '8', '9', '0', ' ') + "(B)";
                    //Change the BLIND LINE and Correct for GOLD player if necessary                    
                    if (richTextBox2.Lines[29].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[4] = team5g;
                    teamCnt = 5;

                    // If there are multiple teams that have 3 Gold and One White run the swap again
                    while (((goldGolfers[0] >= 2 || goldGolfers[1] >= 2 || goldGolfers[2] >= 2 || goldGolfers[3] >= 2 || goldGolfers[4] >= 2) && (goldGolfers[0] == 0 || goldGolfers[1] == 0 || goldGolfers[2] == 0 || goldGolfers[3] == 0 || goldGolfers[4] == 0))
                        || ((goldGolfers[0] >= 3 || goldGolfers[1] >= 3 || goldGolfers[2] >= 3 || goldGolfers[3] >= 3 || goldGolfers[4] >= 3) && (goldGolfers[0] == 1 || goldGolfers[1] == 1 || goldGolfers[2] == 1 || goldGolfers[3] == 1 || goldGolfers[4] == 1))
                        || ((goldGolfers[0] == 4 || goldGolfers[1] == 4 || goldGolfers[2] == 4 || goldGolfers[3] == 4 || goldGolfers[4] == 4) && (goldGolfers[0] == 2 || goldGolfers[1] == 2 || goldGolfers[2] == 2 || goldGolfers[3] == 2 || goldGolfers[4] == 2)))
                    {
                        determineTeamsToSwap(teamCnt);

                        swapGoldForWhiteFoursome(minGoldTeamA, maxGoldTeamA);
                    }
                    displayTeamHCPcalc(teamCnt);
                    break;
                case 20:
                    nums = Enumerable.Range(0, 20).ToArray();
                    shuffle = new Random();

                    // Shuffle the array
                    for (int i = 0; i < nums.Length; ++i)
                    {
                        int randomIndex = shuffle.Next(nums.Length);
                        int temp = nums[randomIndex];
                        nums[randomIndex] = nums[i];
                        nums[i] = temp;
                    }
                    // Now your array is randomized and you can simply print them in order
                    richTextBox2.Text = "RANDOM DRAW for " + (playerCnt - 1).ToString() + " golfers\n";
                    richTextBox2.Text += "TEAM 1\n";                            //1
                    richTextBox2.Text += richTextBox1.Lines[nums[0]];                 //2
                    if (richTextBox1.Lines[nums[0]].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[9]];                 //3
                    if (richTextBox1.Lines[nums[9]].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[10]];                //4
                    if (richTextBox1.Lines[nums[10]].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[19]];                //5
                    if (richTextBox1.Lines[nums[19]].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[0] = team1g;

                    richTextBox2.Text += Environment.NewLine;                   //6

                    richTextBox2.Text += "TEAM 2\n";                            //7
                    richTextBox2.Text += richTextBox1.Lines[nums[1]];                 //8
                    if (richTextBox1.Lines[nums[1]].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[8]];                 //9
                    if (richTextBox1.Lines[nums[8]].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[11]];                //10
                    if (richTextBox1.Lines[nums[11]].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[18]];                //11 
                    if (richTextBox1.Lines[nums[18]].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[1] = team2g;

                    richTextBox2.Text += Environment.NewLine;                   //12

                    richTextBox2.Text += "TEAM 3\n";                            //13
                    richTextBox2.Text += richTextBox1.Lines[nums[2]];                 //14
                    if (richTextBox1.Lines[nums[2]].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[7]];                 //15
                    if (richTextBox1.Lines[nums[7]].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[12]];                //16
                    if (richTextBox1.Lines[nums[12]].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[17]];                //17 
                    if (richTextBox1.Lines[nums[17]].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[2] = team3g;

                    richTextBox2.Text += Environment.NewLine;                   //18

                    richTextBox2.Text += "TEAM 4\n";                            //19
                    richTextBox2.Text += richTextBox1.Lines[nums[3]];                 //20
                    if (richTextBox1.Lines[nums[3]].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[6]];                 //21
                    if (richTextBox1.Lines[nums[6]].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[13]];                //22
                    if (richTextBox1.Lines[nums[13]].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[16]];                //23
                    if (richTextBox1.Lines[nums[16]].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[3] = team4g;

                    richTextBox2.Text += Environment.NewLine;                   //24

                    richTextBox2.Text += "TEAM 5\n";                            //25
                    richTextBox2.Text += richTextBox1.Lines[nums[4]];                 //26
                    if (richTextBox1.Lines[nums[4]].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[5]];                 //27
                    if (richTextBox1.Lines[nums[5]].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[14]];                //28
                    if (richTextBox1.Lines[nums[14]].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[15]];                //29
                    if (richTextBox1.Lines[nums[15]].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[4] = team5g;
                    teamCnt = 5;

                    // If there are multiple teams that have 3 Gold and One White run the swap again
                    while (((goldGolfers[0] >= 2 || goldGolfers[1] >= 2 || goldGolfers[2] >= 2 || goldGolfers[3] >= 2 || goldGolfers[4] >= 2) && (goldGolfers[0] == 0 || goldGolfers[1] == 0 || goldGolfers[2] == 0 || goldGolfers[3] == 0 || goldGolfers[4] == 0))
                        || ((goldGolfers[0] >= 3 || goldGolfers[1] >= 3 || goldGolfers[2] >= 3 || goldGolfers[3] >= 3 || goldGolfers[4] >= 3) && (goldGolfers[0] == 1 || goldGolfers[1] == 1 || goldGolfers[2] == 1 || goldGolfers[3] == 1 || goldGolfers[4] == 1))
                        || ((goldGolfers[0] == 4 || goldGolfers[1] == 4 || goldGolfers[2] == 4 || goldGolfers[3] == 4 || goldGolfers[4] == 4) && (goldGolfers[0] == 2 || goldGolfers[1] == 2 || goldGolfers[2] == 2 || goldGolfers[3] == 2 || goldGolfers[4] == 2)))
                    {
                        determineTeamsToSwap(teamCnt);

                        swapGoldForWhiteFoursome(minGoldTeamA, maxGoldTeamA);
                    }

                    displayTeamHCPcalc(teamCnt);
                    break;
                case 21:
                    nums = Enumerable.Range(0, 21).ToArray();
                    shuffle = new Random();

                    // Shuffle the array
                    for (int i = 0; i < nums.Length; ++i)
                    {
                        int randomIndex = shuffle.Next(nums.Length);
                        int temp = nums[randomIndex];
                        nums[randomIndex] = nums[i];
                        nums[i] = temp;
                    }
                    // Now your array is randomized and you can simply print them in order
                    richTextBox2.Text = "A-B-C-D DRAW for " + (playerCnt - 1).ToString() + " golfers\n";
                    richTextBox2.Text += "TEAM 1\n";                                //1
                    richTextBox2.Text += richTextBox1.Lines[nums[0]];                     //2
                    if (richTextBox1.Lines[nums[0]].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[13]];                    //3
                    if (richTextBox1.Lines[nums[13]].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[14]];                    //4
                    if (richTextBox1.Lines[nums[14]].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[0] = team1g;

                    richTextBox2.Text += Environment.NewLine;                       //5

                    richTextBox2.Text += "TEAM 2\n";                                //6 
                    richTextBox2.Text += richTextBox1.Lines[nums[1]];                     //7
                    if (richTextBox1.Lines[nums[1]].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[12]];                    //8
                    if (richTextBox1.Lines[nums[12]].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[15]];                    //9
                    if (richTextBox1.Lines[nums[15]].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[1] = team2g;

                    richTextBox2.Text += Environment.NewLine;                       //10

                    richTextBox2.Text += "TEAM 3\n";                                //11
                    richTextBox2.Text += richTextBox1.Lines[nums[2]];                     //12
                    if (richTextBox1.Lines[nums[2]].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[11]];                    //13
                    if (richTextBox1.Lines[nums[11]].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[16]];                    //14
                    if (richTextBox1.Lines[nums[16]].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[2] = team3g;

                    richTextBox2.Text += Environment.NewLine;                       //15

                    richTextBox2.Text += "TEAM 4\n";                                //16
                    richTextBox2.Text += richTextBox1.Lines[nums[3]];                     //17
                    if (richTextBox1.Lines[nums[3]].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[10]];                    //18
                    if (richTextBox1.Lines[nums[10]].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[17]];                    //19
                    if (richTextBox1.Lines[nums[17]].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[3] = team4g;

                    richTextBox2.Text += Environment.NewLine;                       //20

                    richTextBox2.Text += "TEAM 5\n";                                //21
                    richTextBox2.Text += richTextBox1.Lines[nums[4]];                     //22
                    if (richTextBox1.Lines[nums[4]].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[9]];                     //23
                    if (richTextBox1.Lines[nums[9]].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[18]];                    //24
                    if (richTextBox1.Lines[nums[18]].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[4] = team5g;

                    richTextBox2.Text += Environment.NewLine;                       //25

                    richTextBox2.Text += "TEAM 6\n";                                //26
                    richTextBox2.Text += richTextBox1.Lines[nums[5]];                     //27
                    if (richTextBox1.Lines[nums[5]].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[8]];                     //28
                    if (richTextBox1.Lines[nums[8]].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[19]];                    //29
                    if (richTextBox1.Lines[nums[19]].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[5] = team6g;

                    richTextBox2.Text += Environment.NewLine;                       //30

                    richTextBox2.Text += "TEAM 7\n";                                //31
                    richTextBox2.Text += richTextBox1.Lines[nums[6]];                     //32
                    if (richTextBox1.Lines[nums[6]].Contains("*")) team7g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[7]];                     //33
                    if (richTextBox1.Lines[nums[7]].Contains("*")) team7g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[20]];                    //34
                    if (richTextBox1.Lines[nums[20]].Contains("*")) team7g++;
                    goldGolfers[6] = team7g;
                    teamCnt = 7;

                    // If there are multiple teams that have 3 Gold and One White run the swap again
                    while (((goldGolfers[0] >= 2 || goldGolfers[1] >= 2 || goldGolfers[2] >= 2 || goldGolfers[3] >= 2 || goldGolfers[4] >= 2 || goldGolfers[5] >= 2 || goldGolfers[6] >= 2) && (goldGolfers[0] == 0 || goldGolfers[1] == 0 || goldGolfers[2] == 0 || goldGolfers[3] == 0 || goldGolfers[4] == 0 || goldGolfers[5] == 0 || goldGolfers[6] == 0))
                        || ((goldGolfers[0] >= 3 || goldGolfers[1] >= 3 || goldGolfers[2] >= 3 || goldGolfers[3] >= 3 || goldGolfers[4] >= 3 || goldGolfers[5] >= 3 || goldGolfers[6] >= 3) && (goldGolfers[0] == 1 || goldGolfers[1] == 1 || goldGolfers[2] == 1 || goldGolfers[3] == 1 || goldGolfers[4] == 1 || goldGolfers[5] == 1 || goldGolfers[6] == 1))
                        || ((goldGolfers[0] == 4 || goldGolfers[1] == 4 || goldGolfers[2] == 4 || goldGolfers[3] == 4 || goldGolfers[4] == 4 || goldGolfers[5] == 4 || goldGolfers[6] == 4) && (goldGolfers[0] == 2 || goldGolfers[1] == 2 || goldGolfers[2] == 2 || goldGolfers[3] == 2 || goldGolfers[4] == 2 || goldGolfers[5] == 2 || goldGolfers[6] == 2)))
                    {

                        determineTeamsToSwap(teamCnt);

                        swapGoldForWhiteThreesome();
                    }
                    displayTeamHCPcalc(teamCnt + 10);
                    break;
                case 22:
                    nums = Enumerable.Range(0, 22).ToArray();
                    shuffle = new Random();

                    // Shuffle the array
                    for (int i = 0; i < nums.Length; ++i)
                    {
                        int randomIndex = shuffle.Next(nums.Length);
                        int temp = nums[randomIndex];
                        nums[randomIndex] = nums[i];
                        nums[i] = temp;
                    }
                    // Now your array is randomized and you can simply print them in order
                    richTextBox2.Text = "RANDOM DRAW for " + (playerCnt - 1).ToString() + " golfers\n";
                    richTextBox2.Text += "TEAM 1\n";                            //1
                    richTextBox2.Text += richTextBox1.Lines[nums[0]];                 //2
                    if (richTextBox1.Lines[nums[0]].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[11]];                //3
                    if (richTextBox1.Lines[nums[11]].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[12]];                //4
                    if (richTextBox1.Lines[nums[12]].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += "BLIND";                               //5
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[0] = team1g;

                    richTextBox2.Text += Environment.NewLine;                   //6

                    richTextBox2.Text += "TEAM 2\n";                            //7
                    richTextBox2.Text += richTextBox1.Lines[nums[1]];                 //8
                    if (richTextBox1.Lines[nums[1]].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[10]];                //9
                    if (richTextBox1.Lines[nums[10]].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[13]];                //10
                    if (richTextBox1.Lines[nums[13]].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += "BLIND";                               //11
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[1] = team2g;

                    richTextBox2.Text += Environment.NewLine;                   //12

                    richTextBox2.Text += "TEAM 3\n";                            //13
                    richTextBox2.Text += richTextBox1.Lines[nums[2]];                 //14
                    if (richTextBox1.Lines[nums[2]].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[9]];                 //15
                    if (richTextBox1.Lines[nums[9]].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[14]];                //16
                    if (richTextBox1.Lines[nums[14]].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[21]];                //17
                    if (richTextBox1.Lines[nums[21]].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[2] = team3g;

                    richTextBox2.Text += Environment.NewLine;                   //18

                    richTextBox2.Text += "TEAM 4\n";                            //19
                    richTextBox2.Text += richTextBox1.Lines[nums[3]];                 //20
                    if (richTextBox1.Lines[nums[3]].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[8]];                 //21
                    if (richTextBox1.Lines[nums[8]].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[15]];                //22
                    if (richTextBox1.Lines[nums[15]].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[20]];                //23
                    if (richTextBox1.Lines[nums[20]].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[3] = team4g;

                    richTextBox2.Text += Environment.NewLine;                   //24

                    richTextBox2.Text += "TEAM 5\n";                            //25
                    richTextBox2.Text += richTextBox1.Lines[nums[4]];                 //26
                    if (richTextBox1.Lines[nums[4]].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[7]];                 //27
                    if (richTextBox1.Lines[nums[7]].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[16]];                //28
                    if (richTextBox1.Lines[nums[16]].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[19]];                //29
                    if (richTextBox1.Lines[nums[19]].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[4] = team5g;

                    richTextBox2.Text += Environment.NewLine;                   //30

                    richTextBox2.Text += "TEAM 6\n";                            //31
                    richTextBox2.Text += richTextBox1.Lines[nums[5]];                 //32
                    if (richTextBox1.Lines[nums[5]].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[6]];                 //33
                    if (richTextBox1.Lines[nums[6]].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[17]];                //34
                    if (richTextBox1.Lines[nums[17]].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[18]];                //35
                    if (richTextBox1.Lines[nums[18]].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[5] = team6g;
                    teamCnt = 6;

                    rIntF = rnd.Next(18, 22);
                    do
                    {
                        rIntB = rnd.Next(18, 22);
                    } while (rIntF == rIntB);

                    //Make the BLIND before the teams are balanced for GOLD/WHITE
                    blindString = richTextBox1.Lines[rIntF] + "(F) " +
                            richTextBox1.Lines[rIntB].TrimStart('1', '2', '3', '4', '5', '6', '7', '8', '9', '0', ' ') + "(B)";
                    //Change the BLIND LINE and Correct for GOLD player if necessary
                    changeLine(richTextBox2, 5, blindString);
                    if (richTextBox2.Lines[5].Contains("*")) team1g++;
                    changeLine(richTextBox2, 11, blindString);
                    if (richTextBox2.Lines[11].Contains("*")) team2g++;

                    // If there are multiple teams that have 3 Gold and One White run the swap again
                    while (((goldGolfers[0] >= 2 || goldGolfers[1] >= 2 || goldGolfers[2] >= 2 || goldGolfers[3] >= 2 || goldGolfers[4] >= 2 || goldGolfers[5] >= 2) && (goldGolfers[0] == 0 || goldGolfers[1] == 0 || goldGolfers[2] == 0 || goldGolfers[3] == 0 || goldGolfers[4] == 0 || goldGolfers[5] == 0))
                        || ((goldGolfers[0] >= 3 || goldGolfers[1] >= 3 || goldGolfers[2] >= 3 || goldGolfers[3] >= 3 || goldGolfers[4] >= 3 || goldGolfers[5] >= 3) && (goldGolfers[0] == 1 || goldGolfers[1] == 1 || goldGolfers[2] == 1 || goldGolfers[3] == 1 || goldGolfers[4] == 1 || goldGolfers[5] == 1))
                        || ((goldGolfers[0] == 4 || goldGolfers[1] == 4 || goldGolfers[2] == 4 || goldGolfers[3] == 4 || goldGolfers[4] == 4 || goldGolfers[5] == 4) && (goldGolfers[0] == 2 || goldGolfers[1] == 2 || goldGolfers[2] == 2 || goldGolfers[3] == 2 || goldGolfers[4] == 2 || goldGolfers[5] == 2)))
                    {
                        determineTeamsToSwap(teamCnt);

                        swapGoldForWhiteFoursome(minGoldTeamA, maxGoldTeamA);
                    }
                    displayTeamHCPcalc(teamCnt);
                    break;
                case 23:
                    nums = Enumerable.Range(0, 23).ToArray();
                    shuffle = new Random();

                    // Shuffle the array
                    for (int i = 0; i < nums.Length; ++i)
                    {
                        int randomIndex = shuffle.Next(nums.Length);
                        int temp = nums[randomIndex];
                        nums[randomIndex] = nums[i];
                        nums[i] = temp;
                    }
                    // Now your array is randomized and you can simply print them in order
                    richTextBox2.Text = "RANDOM DRAW  for " + (playerCnt - 1).ToString() + " golfers\n";
                    richTextBox2.Text += "TEAM 1\n";                            //1
                    richTextBox2.Text += richTextBox1.Lines[nums[5]];                 //2
                    if (richTextBox1.Lines[nums[5]].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[6]];                 //3
                    if (richTextBox1.Lines[nums[6]].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[17]];                //4
                    if (richTextBox1.Lines[nums[17]].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[18]];                //5
                    if (richTextBox1.Lines[nums[18]].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[0] = team1g;

                    richTextBox2.Text += Environment.NewLine;                    //6

                    richTextBox2.Text += "TEAM 2\n";                             //7
                    richTextBox2.Text += richTextBox1.Lines[nums[1]];                  //8
                    if (richTextBox1.Lines[nums[1]].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[10]];                 //9
                    if (richTextBox1.Lines[nums[10]].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[13]];                 //10
                    if (richTextBox1.Lines[nums[13]].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[22]];                 //11
                    if (richTextBox1.Lines[nums[22]].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[1] = team2g;

                    richTextBox2.Text += Environment.NewLine;                    //12

                    richTextBox2.Text += "TEAM 3\n";                             //13
                    richTextBox2.Text += richTextBox1.Lines[nums[2]];                  //14
                    if (richTextBox1.Lines[nums[2]].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[9]];                  //15
                    if (richTextBox1.Lines[nums[9]].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[14]];                 //16
                    if (richTextBox1.Lines[nums[14]].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[21]];                 //17
                    if (richTextBox1.Lines[nums[21]].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[2] = team3g;

                    richTextBox2.Text += Environment.NewLine;                    //18

                    richTextBox2.Text += "TEAM 4\n";                             //19
                    richTextBox2.Text += richTextBox1.Lines[nums[3]];                  //20
                    if (richTextBox1.Lines[nums[3]].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[8]];                  //21
                    if (richTextBox1.Lines[nums[8]].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[15]];                 //22
                    if (richTextBox1.Lines[nums[15]].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[20]];                 //23
                    if (richTextBox1.Lines[nums[20]].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[3] = team4g;

                    richTextBox2.Text += Environment.NewLine;                    //24

                    richTextBox2.Text += "TEAM 5\n";                             //25
                    richTextBox2.Text += richTextBox1.Lines[nums[4]];                  //26
                    if (richTextBox1.Lines[nums[4]].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[7]];                  //27
                    if (richTextBox1.Lines[nums[7]].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[16]];                 //28
                    if (richTextBox1.Lines[nums[16]].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[19]];                 //29
                    if (richTextBox1.Lines[nums[19]].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[4] = team5g;

                    richTextBox2.Text += Environment.NewLine;                    //30

                    richTextBox2.Text += "TEAM 6\n";                             //31
                    richTextBox2.Text += richTextBox1.Lines[nums[0]];                  //32
                    if (richTextBox1.Lines[nums[0]].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[11]];                 //33
                    if (richTextBox1.Lines[nums[11]].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[12]];                 //34
                    if (richTextBox1.Lines[nums[12]].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;

                    rIntF = rnd.Next(18, 23);
                    do
                    {
                        rIntB = rnd.Next(18, 23);
                    } while (rIntF == rIntB);                                   //35

                    richTextBox2.Text += richTextBox1.Lines[rIntF] + "(F) " +
                        richTextBox1.Lines[rIntB].TrimStart('1', '2', '3', '4', '5', '6', '7', '8', '9', '0', ' ') + "(B)";
                    if (richTextBox2.Lines[35].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[5] = team6g;
                    teamCnt = 6;

                    // If there are multiple teams that have 3 Gold and One White run the swap again
                    while (((goldGolfers[0] >= 2 || goldGolfers[1] >= 2 || goldGolfers[2] >= 2 || goldGolfers[3] >= 2 || goldGolfers[4] >= 2 || goldGolfers[5] >= 2) && (goldGolfers[0] == 0 || goldGolfers[1] == 0 || goldGolfers[2] == 0 || goldGolfers[3] == 0 || goldGolfers[4] == 0 || goldGolfers[5] == 0))
                        || ((goldGolfers[0] >= 3 || goldGolfers[1] >= 3 || goldGolfers[2] >= 3 || goldGolfers[3] >= 3 || goldGolfers[4] >= 3 || goldGolfers[5] >= 3) && (goldGolfers[0] == 1 || goldGolfers[1] == 1 || goldGolfers[2] == 1 || goldGolfers[3] == 1 || goldGolfers[4] == 1 || goldGolfers[5] == 1))
                        || ((goldGolfers[0] == 4 || goldGolfers[1] == 4 || goldGolfers[2] == 4 || goldGolfers[3] == 4 || goldGolfers[4] == 4 || goldGolfers[5] == 4) && (goldGolfers[0] == 2 || goldGolfers[1] == 2 || goldGolfers[2] == 2 || goldGolfers[3] == 2 || goldGolfers[4] == 2 || goldGolfers[5] == 2)))
                    {
                        determineTeamsToSwap(teamCnt);

                        swapGoldForWhiteFoursome(minGoldTeamA, maxGoldTeamA);
                    }
                    displayTeamHCPcalc(teamCnt);
                    break;
                case 24:
                    nums = Enumerable.Range(0, 24).ToArray();
                    shuffle = new Random();

                    // Shuffle the array
                    for (int i = 0; i < nums.Length; ++i)
                    {
                        int randomIndex = shuffle.Next(nums.Length);
                        int temp = nums[randomIndex];
                        nums[randomIndex] = nums[i];
                        nums[i] = temp;
                    }
                    // Now your array is randomized and you can simply print them in order
                    richTextBox2.Text = "RANDOM DRAW for " + (playerCnt - 1).ToString() + " golfers\n";
                    richTextBox2.Text += "TEAM 1\n";                            //1
                    richTextBox2.Text += richTextBox1.Lines[nums[0]];                 //2
                    if (richTextBox1.Lines[nums[0]].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[11]];                //3
                    if (richTextBox1.Lines[nums[11]].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[12]];                //4
                    if (richTextBox1.Lines[nums[12]].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[23]];                //5
                    if (richTextBox1.Lines[nums[23]].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[0] = team1g;

                    richTextBox2.Text += Environment.NewLine;                   //6

                    richTextBox2.Text += "TEAM 2\n";                            //7
                    richTextBox2.Text += richTextBox1.Lines[nums[1]];                 //8
                    if (richTextBox1.Lines[nums[1]].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[10]];                //9
                    if (richTextBox1.Lines[nums[10]].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[13]];                //10
                    if (richTextBox1.Lines[nums[13]].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[22]];                //11
                    if (richTextBox1.Lines[nums[22]].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[1] = team2g;

                    richTextBox2.Text += Environment.NewLine;                   //12                  

                    richTextBox2.Text += "TEAM 3\n";                            //13
                    richTextBox2.Text += richTextBox1.Lines[nums[2]];                 //14
                    if (richTextBox1.Lines[nums[2]].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[9]];                 //15
                    if (richTextBox1.Lines[nums[9]].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[14]];                //16
                    if (richTextBox1.Lines[nums[14]].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[21]];                //17
                    if (richTextBox1.Lines[nums[21]].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[2] = team3g;

                    richTextBox2.Text += Environment.NewLine;                   //18

                    richTextBox2.Text += "TEAM 4\n";                            //19
                    richTextBox2.Text += richTextBox1.Lines[nums[3]];                 //20
                    if (richTextBox1.Lines[nums[3]].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[8]];                 //21
                    if (richTextBox1.Lines[nums[8]].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[15]];                //22
                    if (richTextBox1.Lines[nums[15]].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[20]];                //23
                    if (richTextBox1.Lines[nums[20]].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[3] = team4g;

                    richTextBox2.Text += Environment.NewLine;                   //24

                    richTextBox2.Text += "TEAM 5\n";                            //25
                    richTextBox2.Text += richTextBox1.Lines[nums[4]];                 //26
                    if (richTextBox1.Lines[nums[4]].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[7]];                 //27
                    if (richTextBox1.Lines[nums[7]].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[16]];                //28
                    if (richTextBox1.Lines[nums[16]].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[19]];                //29
                    if (richTextBox1.Lines[nums[19]].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[4] = team5g;

                    richTextBox2.Text += Environment.NewLine;                   //30

                    richTextBox2.Text += "TEAM 6\n";                            //31
                    richTextBox2.Text += richTextBox1.Lines[nums[5]];                 //32
                    if (richTextBox1.Lines[nums[5]].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[6]];                 //33
                    if (richTextBox1.Lines[nums[6]].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[17]];                //34
                    if (richTextBox1.Lines[nums[17]].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[18]];                //35
                    if (richTextBox1.Lines[nums[18]].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[5] = team6g;
                    teamCnt = 6;

                    // If there are multiple teams that have 3 Gold and One White run the swap again
                    while (((goldGolfers[0] >= 2 || goldGolfers[1] >= 2 || goldGolfers[2] >= 2 || goldGolfers[3] >= 2 || goldGolfers[4] >= 2 || goldGolfers[5] >= 2) && (goldGolfers[0] == 0 || goldGolfers[1] == 0 || goldGolfers[2] == 0 || goldGolfers[3] == 0 || goldGolfers[4] == 0 || goldGolfers[5] == 0))
                        || ((goldGolfers[0] >= 3 || goldGolfers[1] >= 3 || goldGolfers[2] >= 3 || goldGolfers[3] >= 3 || goldGolfers[4] >= 3 || goldGolfers[5] >= 3) && (goldGolfers[0] == 1 || goldGolfers[1] == 1 || goldGolfers[2] == 1 || goldGolfers[3] == 1 || goldGolfers[4] == 1 || goldGolfers[5] == 1))
                        || ((goldGolfers[0] == 4 || goldGolfers[1] == 4 || goldGolfers[2] == 4 || goldGolfers[3] == 4 || goldGolfers[4] == 4 || goldGolfers[5] == 4) && (goldGolfers[0] == 2 || goldGolfers[1] == 2 || goldGolfers[2] == 2 || goldGolfers[3] == 2 || goldGolfers[4] == 2 || goldGolfers[5] == 2)))
                    {
                        determineTeamsToSwap(teamCnt);

                        swapGoldForWhiteFoursome(minGoldTeamA, maxGoldTeamA);
                    }
                    displayTeamHCPcalc(teamCnt);
                    break;
                case 25:
                    nums = Enumerable.Range(0, 25).ToArray();
                    shuffle = new Random();

                    // Shuffle the array
                    for (int i = 0; i < nums.Length; ++i)
                    {
                        int randomIndex = shuffle.Next(nums.Length);
                        int temp = nums[randomIndex];
                        nums[randomIndex] = nums[i];
                        nums[i] = temp;
                    }
                    // Now your array is randomized and you can simply print them in order
                    richTextBox2.Text = "RANDOM DRAW for " + (playerCnt - 1).ToString() + " golfers\n";
                    richTextBox2.Text += "TEAM 1\n";
                    richTextBox2.Text += richTextBox1.Lines[nums[0]];
                    if (richTextBox1.Lines[nums[0]].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[13]];
                    if (richTextBox1.Lines[nums[13]].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[14]];
                    if (richTextBox1.Lines[nums[14]].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    rIntF = rnd.Next(17, 25);
                    do
                    {
                        rIntB = rnd.Next(17, 25);
                    } while (rIntF == rIntB);

                    richTextBox2.Text += richTextBox1.Lines[rIntF] + "(F) " +
                        richTextBox1.Lines[rIntB].TrimStart('1', '2', '3', '4', '5', '6', '7', '8', '9', '0', ' ') + "(B)";
                    if (richTextBox2.Lines[5].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[0] = team1g;

                    richTextBox2.Text += Environment.NewLine;

                    richTextBox2.Text += "TEAM 2\n";
                    richTextBox2.Text += richTextBox1.Lines[nums[1]];
                    if (richTextBox1.Lines[nums[1]].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[12]];
                    if (richTextBox1.Lines[nums[12]].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[15]];
                    if (richTextBox1.Lines[nums[15]].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[rIntF] + "(F) " +
                        richTextBox1.Lines[rIntB].TrimStart('1', '2', '3', '4', '5', '6', '7', '8', '9', '0', ' ') + "(B)";
                    if (richTextBox2.Lines[nums[11]].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[1] = team2g;

                    richTextBox2.Text += Environment.NewLine;

                    richTextBox2.Text += "TEAM 3\n";
                    richTextBox2.Text += richTextBox1.Lines[nums[2]];
                    if (richTextBox1.Lines[nums[2]].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[11]];
                    if (richTextBox1.Lines[nums[11]].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[16]];
                    if (richTextBox1.Lines[nums[16]].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[rIntF] + "(F) " +
                        richTextBox1.Lines[rIntB].TrimStart('1', '2', '3', '4', '5', '6', '7', '8', '9', '0', ' ') + "(B)";
                    if (richTextBox2.Lines[nums[17]].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[2] = team3g;

                    richTextBox2.Text += Environment.NewLine;

                    richTextBox2.Text += "TEAM 4\n";
                    richTextBox2.Text += richTextBox1.Lines[nums[3]];
                    if (richTextBox1.Lines[nums[3]].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[10]];
                    if (richTextBox1.Lines[nums[10]].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[17]];
                    if (richTextBox1.Lines[nums[17]].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[24]];
                    if (richTextBox1.Lines[nums[24]].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[3] = team4g;

                    richTextBox2.Text += Environment.NewLine;

                    richTextBox2.Text += "TEAM 5\n";
                    richTextBox2.Text += richTextBox1.Lines[nums[4]];
                    if (richTextBox1.Lines[nums[4]].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[9]];
                    if (richTextBox1.Lines[nums[9]].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[18]];
                    if (richTextBox1.Lines[nums[18]].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[23]];
                    if (richTextBox1.Lines[nums[23]].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[4] = team5g;

                    richTextBox2.Text += Environment.NewLine;

                    richTextBox2.Text += "TEAM 6\n";
                    richTextBox2.Text += richTextBox1.Lines[nums[5]];
                    if (richTextBox1.Lines[nums[5]].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[8]];
                    if (richTextBox1.Lines[nums[8]].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[19]];
                    if (richTextBox1.Lines[nums[19]].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[22]];
                    if (richTextBox1.Lines[nums[22]].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[5] = team6g;

                    richTextBox2.Text += Environment.NewLine;

                    richTextBox2.Text += "TEAM 7\n";
                    richTextBox2.Text += richTextBox1.Lines[nums[6]];
                    if (richTextBox1.Lines[nums[6]].Contains("*")) team7g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[7]];
                    if (richTextBox1.Lines[nums[7]].Contains("*")) team7g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[20]];
                    if (richTextBox1.Lines[nums[20]].Contains("*")) team7g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[21]];
                    if (richTextBox1.Lines[nums[21]].Contains("*")) team7g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[6] = team7g;
                    teamCnt = 7;

                    // If there are multiple teams that have 3 Gold and One White run the swap again
                    while (((goldGolfers[0] >= 2 || goldGolfers[1] >= 2 || goldGolfers[2] >= 2 || goldGolfers[3] >= 2 || goldGolfers[4] >= 2 || goldGolfers[5] >= 2 || goldGolfers[6] >= 2) && (goldGolfers[0] == 0 || goldGolfers[1] == 0 || goldGolfers[2] == 0 || goldGolfers[3] == 0 || goldGolfers[4] == 0 || goldGolfers[5] == 0 || goldGolfers[6] == 0))
                        || ((goldGolfers[0] >= 3 || goldGolfers[1] >= 3 || goldGolfers[2] >= 3 || goldGolfers[3] >= 3 || goldGolfers[4] >= 3 || goldGolfers[5] >= 3 || goldGolfers[6] >= 3) && (goldGolfers[0] == 1 || goldGolfers[1] == 1 || goldGolfers[2] == 1 || goldGolfers[3] == 1 || goldGolfers[4] == 1 || goldGolfers[5] == 1 || goldGolfers[6] == 1))
                        || ((goldGolfers[0] == 4 || goldGolfers[1] == 4 || goldGolfers[2] == 4 || goldGolfers[3] == 4 || goldGolfers[4] == 4 || goldGolfers[5] == 4 || goldGolfers[6] == 4) && (goldGolfers[0] == 2 || goldGolfers[1] == 2 || goldGolfers[2] == 2 || goldGolfers[3] == 2 || goldGolfers[4] == 2 || goldGolfers[5] == 2 || goldGolfers[6] == 2)))
                    {
                        determineTeamsToSwap(teamCnt);

                        swapGoldForWhiteFoursome(minGoldTeamA, maxGoldTeamA);
                    }
                    displayTeamHCPcalc(teamCnt);
                    break;
                case 26:
                    nums = Enumerable.Range(0, 26).ToArray();
                    shuffle = new Random();

                    // Shuffle the array
                    for (int i = 0; i < nums.Length; ++i)
                    {
                        int randomIndex = shuffle.Next(nums.Length);
                        int temp = nums[randomIndex];
                        nums[randomIndex] = nums[i];
                        nums[i] = temp;
                    }
                    // Now your array is randomized and you can simply print them in order
                    richTextBox2.Text = "RANDOM DRAW for " + (playerCnt - 1).ToString() + " golfers\n";
                    richTextBox2.Text += "TEAM 1\n";
                    richTextBox2.Text += richTextBox1.Lines[nums[0]];
                    if (richTextBox1.Lines[nums[0]].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[13]];
                    if (richTextBox1.Lines[nums[13]].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[14]];
                    if (richTextBox1.Lines[nums[14]].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;

                    rIntF = rnd.Next(16, 26);
                    do
                    {
                        rIntB = rnd.Next(16, 26);
                    } while (rIntF == rIntB);
                    richTextBox2.Text += richTextBox1.Lines[rIntF] + "(F) " +
                        richTextBox1.Lines[rIntB].TrimStart('1', '2', '3', '4', '5', '6', '7', '8', '9', '0', ' ') + "(B)";
                    if (richTextBox2.Lines[5].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[0] = team1g;

                    richTextBox2.Text += Environment.NewLine;                   //6

                    richTextBox2.Text += "TEAM 2\n";                            //7
                    richTextBox2.Text += richTextBox1.Lines[nums[1]];
                    if (richTextBox1.Lines[nums[1]].Contains("*")) team2g++;          //8
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[12]];
                    if (richTextBox1.Lines[nums[12]].Contains("*")) team2g++;         //9
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[15]];
                    if (richTextBox1.Lines[nums[15]].Contains("*")) team2g++;         //10
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[rIntF] + "(F) " +
                        richTextBox1.Lines[rIntB].TrimStart('1', '2', '3', '4', '5', '6', '7', '8', '9', '0', ' ') + "(B)";
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[1] = team2g;

                    richTextBox2.Text += Environment.NewLine;                   //12 

                    richTextBox2.Text += "TEAM 3\n";                            //13
                    richTextBox2.Text += richTextBox1.Lines[nums[2]];                 //14
                    if (richTextBox1.Lines[nums[2]].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[11]];                //15
                    if (richTextBox1.Lines[nums[11]].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[16]];                //16
                    if (richTextBox1.Lines[nums[16]].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[25]];                //17
                    if (richTextBox1.Lines[nums[25]].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[2] = team3g;

                    richTextBox2.Text += Environment.NewLine;                   //18

                    richTextBox2.Text += "TEAM 4\n";                            //19
                    richTextBox2.Text += richTextBox1.Lines[nums[3]];                 //20
                    if (richTextBox1.Lines[nums[3]].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[10]];                //21
                    if (richTextBox1.Lines[nums[10]].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[17]];                //22
                    if (richTextBox1.Lines[nums[17]].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[24]];                //23
                    if (richTextBox1.Lines[nums[24]].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[3] = team4g;

                    richTextBox2.Text += Environment.NewLine;                   //24

                    richTextBox2.Text += "TEAM 5\n";                            //25
                    richTextBox2.Text += richTextBox1.Lines[nums[4]];                 //26
                    if (richTextBox1.Lines[nums[4]].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[9]];                 //27
                    if (richTextBox1.Lines[nums[9]].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[18]];                //28
                    if (richTextBox1.Lines[nums[18]].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[23]];                //29
                    if (richTextBox1.Lines[nums[23]].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[4] = team5g;

                    richTextBox2.Text += Environment.NewLine;                   //30

                    richTextBox2.Text += "TEAM 6\n";                            //31
                    richTextBox2.Text += richTextBox1.Lines[nums[5]];                 //32
                    if (richTextBox1.Lines[nums[5]].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[8]];                 //33
                    if (richTextBox1.Lines[nums[8]].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[19]];                //34
                    if (richTextBox1.Lines[nums[19]].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[22]];                //35
                    if (richTextBox1.Lines[nums[22]].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[5] = team6g;

                    richTextBox2.Text += Environment.NewLine;

                    richTextBox2.Text += "TEAM 7\n";
                    richTextBox2.Text += richTextBox1.Lines[nums[6]];
                    if (richTextBox1.Lines[nums[6]].Contains("*")) team7g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[7]];
                    if (richTextBox1.Lines[nums[7]].Contains("*")) team7g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[20]];
                    if (richTextBox1.Lines[nums[20]].Contains("*")) team7g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[21]];
                    if (richTextBox1.Lines[nums[21]].Contains("*")) team7g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[6] = team7g;
                    teamCnt = 7;

                    // If there are multiple teams that have 3 Gold and One White run the swap again
                    while (((goldGolfers[0] >= 2 || goldGolfers[1] >= 2 || goldGolfers[2] >= 2 || goldGolfers[3] >= 2 || goldGolfers[4] >= 2 || goldGolfers[5] >= 2 || goldGolfers[6] >= 2) && (goldGolfers[0] == 0 || goldGolfers[1] == 0 || goldGolfers[2] == 0 || goldGolfers[3] == 0 || goldGolfers[4] == 0 || goldGolfers[5] == 0 || goldGolfers[6] == 0))
                        || ((goldGolfers[0] >= 3 || goldGolfers[1] >= 3 || goldGolfers[2] >= 3 || goldGolfers[3] >= 3 || goldGolfers[4] >= 3 || goldGolfers[5] >= 3 || goldGolfers[6] >= 3) && (goldGolfers[0] == 1 || goldGolfers[1] == 1 || goldGolfers[2] == 1 || goldGolfers[3] == 1 || goldGolfers[4] == 1 || goldGolfers[5] == 1 || goldGolfers[6] == 1))
                        || ((goldGolfers[0] == 4 || goldGolfers[1] == 4 || goldGolfers[2] == 4 || goldGolfers[3] == 4 || goldGolfers[4] == 4 || goldGolfers[5] == 4 || goldGolfers[6] == 4) && (goldGolfers[0] == 2 || goldGolfers[1] == 2 || goldGolfers[2] == 2 || goldGolfers[3] == 2 || goldGolfers[4] == 2 || goldGolfers[5] == 2 || goldGolfers[6] == 2)))
                    {
                        determineTeamsToSwap(teamCnt);

                        swapGoldForWhiteFoursome(minGoldTeamA, maxGoldTeamA);
                    }
                    displayTeamHCPcalc(teamCnt);
                    break;
                case 27:
                    nums = Enumerable.Range(0, 27).ToArray();
                    shuffle = new Random();

                    // Shuffle the array
                    for (int i = 0; i < nums.Length; ++i)
                    {
                        int randomIndex = shuffle.Next(nums.Length);
                        int temp = nums[randomIndex];
                        nums[randomIndex] = nums[i];
                        nums[i] = temp;
                    }
                    // Now your array is randomized and you can simply print them in order
                    richTextBox2.Text = "RANDOM DRAW for " + (playerCnt - 1).ToString() + " golfers\n";
                    richTextBox2.Text += "TEAM 1\n";                            //1
                    richTextBox2.Text += richTextBox1.Lines[nums[6]];                 //2
                    if (richTextBox1.Lines[nums[6]].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[7]];                 //3
                    if (richTextBox1.Lines[nums[7]].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[20]];                //4
                    if (richTextBox1.Lines[nums[20]].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[21]];                //5
                    if (richTextBox1.Lines[nums[21]].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[0] = team1g;

                    richTextBox2.Text += Environment.NewLine;                   //6

                    richTextBox2.Text += "TEAM 2\n";                            //7
                    richTextBox2.Text += richTextBox1.Lines[nums[1]];                 //8
                    if (richTextBox1.Lines[nums[1]].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[12]];                //9
                    if (richTextBox1.Lines[nums[12]].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[15]];                //10
                    if (richTextBox1.Lines[nums[15]].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[26]];                //11
                    if (richTextBox1.Lines[nums[26]].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[1] = team2g;

                    richTextBox2.Text += Environment.NewLine;                   //12

                    richTextBox2.Text += "TEAM 3\n";                            //13
                    richTextBox2.Text += richTextBox1.Lines[nums[2]];                 //14
                    if (richTextBox1.Lines[nums[2]].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[11]];                //15
                    if (richTextBox1.Lines[nums[11]].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[16]];                //16
                    if (richTextBox1.Lines[nums[16]].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[25]];                //17
                    if (richTextBox1.Lines[nums[25]].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[2] = team3g;

                    richTextBox2.Text += Environment.NewLine;                   //18

                    richTextBox2.Text += "TEAM 4\n";                            //19
                    richTextBox2.Text += richTextBox1.Lines[nums[3]];                 //20
                    if (richTextBox1.Lines[nums[3]].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[10]];                //21
                    if (richTextBox1.Lines[nums[10]].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[17]];                //22
                    if (richTextBox1.Lines[nums[17]].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[24]];                //23
                    if (richTextBox1.Lines[nums[24]].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[3] = team4g;

                    richTextBox2.Text += Environment.NewLine;                   //24

                    richTextBox2.Text += "TEAM 5\n";                            //25
                    richTextBox2.Text += richTextBox1.Lines[nums[4]];                 //26
                    if (richTextBox1.Lines[nums[4]].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[9]];                 //27
                    if (richTextBox1.Lines[nums[9]].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[18]];                //28
                    if (richTextBox1.Lines[nums[18]].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[23]];                //29
                    if (richTextBox1.Lines[nums[23]].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[4] = team5g;

                    richTextBox2.Text += Environment.NewLine;                   //30

                    richTextBox2.Text += "TEAM 6\n";                            //31
                    richTextBox2.Text += richTextBox1.Lines[nums[5]];                 //32
                    if (richTextBox1.Lines[nums[5]].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[8]];                 //33
                    if (richTextBox1.Lines[nums[8]].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[19]];                //34
                    if (richTextBox1.Lines[nums[19]].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[22]];                //35
                    if (richTextBox1.Lines[nums[22]].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[5] = team6g;

                    richTextBox2.Text += Environment.NewLine;                   //36

                    richTextBox2.Text += "TEAM 7\n";                            //37
                    richTextBox2.Text += richTextBox1.Lines[nums[0]];                 //38
                    if (richTextBox1.Lines[nums[0]].Contains("*")) team7g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[13]];                //39
                    if (richTextBox1.Lines[nums[13]].Contains("*")) team7g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[14]];                //40
                    if (richTextBox1.Lines[nums[14]].Contains("*")) team7g++;
                    richTextBox2.Text += Environment.NewLine;

                    rIntF = rnd.Next(21, 27);
                    do
                    {
                        rIntB = rnd.Next(21, 27);
                    } while (rIntF == rIntB);
                    richTextBox2.Text += richTextBox1.Lines[rIntF] + "(F) " +
                        richTextBox1.Lines[rIntB].TrimStart('1', '2', '3', '4', '5', '6', '7', '8', '9', '0', ' ') + "(B)";
                    if (richTextBox2.Lines[richTextBox2.Lines.Length - 1].Contains("*")) team7g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[6] = team7g;
                    teamCnt = 7;

                    // If there are multiple teams that have 3 Gold and One White run the swap again
                    while (((goldGolfers[0] >= 2 || goldGolfers[1] >= 2 || goldGolfers[2] >= 2 || goldGolfers[3] >= 2 || goldGolfers[4] >= 2 || goldGolfers[5] >= 2 || goldGolfers[6] >= 2) && (goldGolfers[0] == 0 || goldGolfers[1] == 0 || goldGolfers[2] == 0 || goldGolfers[3] == 0 || goldGolfers[4] == 0 || goldGolfers[5] == 0 || goldGolfers[6] == 0))
                        || ((goldGolfers[0] >= 3 || goldGolfers[1] >= 3 || goldGolfers[2] >= 3 || goldGolfers[3] >= 3 || goldGolfers[4] >= 3 || goldGolfers[5] >= 3 || goldGolfers[6] >= 3) && (goldGolfers[0] == 1 || goldGolfers[1] == 1 || goldGolfers[2] == 1 || goldGolfers[3] == 1 || goldGolfers[4] == 1 || goldGolfers[5] == 1 || goldGolfers[6] == 1))
                        || ((goldGolfers[0] == 4 || goldGolfers[1] == 4 || goldGolfers[2] == 4 || goldGolfers[3] == 4 || goldGolfers[4] == 4 || goldGolfers[5] == 4 || goldGolfers[6] == 4) && (goldGolfers[0] == 2 || goldGolfers[1] == 2 || goldGolfers[2] == 2 || goldGolfers[3] == 2 || goldGolfers[4] == 2 || goldGolfers[5] == 2 || goldGolfers[6] == 2)))
                    {

                        determineTeamsToSwap(teamCnt);

                        swapGoldForWhiteFoursome(minGoldTeamA, maxGoldTeamA);
                    }
                    displayTeamHCPcalc(teamCnt);
                    break;
                case 28:
                    nums = Enumerable.Range(0, 28).ToArray();
                    shuffle = new Random();

                    // Shuffle the array
                    for (int i = 0; i < nums.Length; ++i)
                    {
                        int randomIndex = shuffle.Next(nums.Length);
                        int temp = nums[randomIndex];
                        nums[randomIndex] = nums[i];
                        nums[i] = temp;
                    }
                    // Now your array is randomized and you can simply print them in order
                    richTextBox2.Text = "RANDOM DRAW for " + (playerCnt - 1).ToString() + " golfers\n";
                    richTextBox2.Text += "TEAM 1\n";                            //1
                    richTextBox2.Text += richTextBox1.Lines[nums[0]];                 //2
                    if (richTextBox1.Lines[nums[0]].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[13]];                //3
                    if (richTextBox1.Lines[nums[13]].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[14]];                //4
                    if (richTextBox1.Lines[nums[14]].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[27]];                //5
                    if (richTextBox1.Lines[nums[27]].Contains("*")) team1g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[0] = team1g;

                    richTextBox2.Text += Environment.NewLine;                   //6

                    richTextBox2.Text += "TEAM 2\n";                            //7
                    richTextBox2.Text += richTextBox1.Lines[nums[1]];                 //8
                    if (richTextBox1.Lines[nums[1]].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[12]];                //9
                    if (richTextBox1.Lines[nums[12]].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[15]];                //10
                    if (richTextBox1.Lines[nums[15]].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[26]];                //11
                    if (richTextBox1.Lines[nums[26]].Contains("*")) team2g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[1] = team2g;

                    richTextBox2.Text += Environment.NewLine;                   //12

                    richTextBox2.Text += "TEAM 3\n";                            //13
                    richTextBox2.Text += richTextBox1.Lines[nums[2]];                 //14
                    if (richTextBox1.Lines[nums[2]].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[11]];                //15
                    if (richTextBox1.Lines[nums[11]].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[16]];                //16
                    if (richTextBox1.Lines[nums[16]].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[25]];                //17
                    if (richTextBox1.Lines[nums[25]].Contains("*")) team3g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[2] = team3g;

                    richTextBox2.Text += Environment.NewLine;                   //18

                    richTextBox2.Text += "TEAM 4\n";                            //19
                    richTextBox2.Text += richTextBox1.Lines[nums[3]];                 //20
                    if (richTextBox1.Lines[nums[3]].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[10]];                //21
                    if (richTextBox1.Lines[nums[10]].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[17]];                //22
                    if (richTextBox1.Lines[nums[17]].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[24]];                //23
                    if (richTextBox1.Lines[nums[24]].Contains("*")) team4g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[3] = team4g;

                    richTextBox2.Text += Environment.NewLine;                   //24

                    richTextBox2.Text += "TEAM 5\n";                            //25
                    richTextBox2.Text += richTextBox1.Lines[nums[4]];                 //26
                    if (richTextBox1.Lines[nums[4]].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[9]];                 //27
                    if (richTextBox1.Lines[nums[9]].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[18]];                //28
                    if (richTextBox1.Lines[nums[18]].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[23]];                //29
                    if (richTextBox1.Lines[nums[23]].Contains("*")) team5g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[4] = team5g;

                    richTextBox2.Text += Environment.NewLine;                   //30

                    richTextBox2.Text += "TEAM 6\n";                            //31
                    richTextBox2.Text += richTextBox1.Lines[nums[5]];                 //32
                    if (richTextBox1.Lines[nums[5]].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[8]];                 //33
                    if (richTextBox1.Lines[nums[8]].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[19]];                //34
                    if (richTextBox1.Lines[nums[19]].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[22]];                //35
                    if (richTextBox1.Lines[nums[22]].Contains("*")) team6g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[5] = team6g;

                    richTextBox2.Text += Environment.NewLine;                   //36

                    richTextBox2.Text += "TEAM 7\n";                            //37
                    richTextBox2.Text += richTextBox1.Lines[nums[6]];                 //38
                    if (richTextBox1.Lines[nums[6]].Contains("*")) team7g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[7]];                 //39
                    if (richTextBox1.Lines[nums[7]].Contains("*")) team7g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[20]];                //40
                    if (richTextBox1.Lines[nums[20]].Contains("*")) team7g++;
                    richTextBox2.Text += Environment.NewLine;
                    richTextBox2.Text += richTextBox1.Lines[nums[21]];                //41
                    if (richTextBox1.Lines[nums[21]].Contains("*")) team7g++;
                    richTextBox2.Text += Environment.NewLine;
                    goldGolfers[6] = team7g;
                    teamCnt = 7;

                    // If there are multiple teams that have 3 Gold and One White run the swap again
                    while (((goldGolfers[0] >= 2 || goldGolfers[1] >= 2 || goldGolfers[2] >= 2 || goldGolfers[3] >= 2 || goldGolfers[4] >= 2 || goldGolfers[5] >= 2 || goldGolfers[6] >= 2) && (goldGolfers[0] == 0 || goldGolfers[1] == 0 || goldGolfers[2] == 0 || goldGolfers[3] == 0 || goldGolfers[4] == 0 || goldGolfers[5] == 0 || goldGolfers[6] == 0))
                        || ((goldGolfers[0] >= 3 || goldGolfers[1] >= 3 || goldGolfers[2] >= 3 || goldGolfers[3] >= 3 || goldGolfers[4] >= 3 || goldGolfers[5] >= 3 || goldGolfers[6] >= 3) && (goldGolfers[0] == 1 || goldGolfers[1] == 1 || goldGolfers[2] == 1 || goldGolfers[3] == 1 || goldGolfers[4] == 1 || goldGolfers[5] == 1 || goldGolfers[6] == 1))
                        || ((goldGolfers[0] == 4 || goldGolfers[1] == 4 || goldGolfers[2] == 4 || goldGolfers[3] == 4 || goldGolfers[4] == 4 || goldGolfers[5] == 4 || goldGolfers[6] == 4) && (goldGolfers[0] == 2 || goldGolfers[1] == 2 || goldGolfers[2] == 2 || goldGolfers[3] == 2 || goldGolfers[4] == 2 || goldGolfers[5] == 2 || goldGolfers[6] == 2)))
                    {
                        determineTeamsToSwap(teamCnt);

                        swapGoldForWhiteFoursome(minGoldTeamA, maxGoldTeamA);
                    }
                    displayTeamHCPcalc(teamCnt);
                    break;
                default:
                    MessageBox.Show("Random Draw not supported for " + (playerCnt - 1).ToString() + "at this time.", "Information", MessageBoxButtons.OK);
                    break;
            }
            
        }

        private void twoManTeamsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //get all the lines out as an arry
            string[] lines = richTextBox2.Lines;
            var newlines = lines.Skip(1);

            twoManForm.twoManRTB.Lines = newlines.ToArray();   // richTextBox2.Text;

            twoManForm.ShowDialog();
        }

        private void wolfPointCalcToolStripMenuItem_Click(object sender, EventArgs e)
        {
            wolfForm.richTextBox1.Clear();

            for (int cntr = 0; cntr < (playerCnt - 1); cntr++)
            {
                richTextBox1.Text += listBox2.Items[cntr] + "\n\n";
                
                string playersName = listBox2.Items[cntr].ToString();

                if (playersName.Length >= 12)
                {
                    int indx = playersName.IndexOf(" ");
                    playersName = playersName.Substring(indx + 1, playersName.Length - (indx + 1));
                    playersName = playersName.PadRight(12, ' ');
                }
                else
                {
                    playersName = playersName.PadRight(12, ' ');
                }
                wolfForm.richTextBox1.Text += playersName;
                
                if (cntr == 0)
                    wolfForm.richTextBox1.Text += "\t";
                else if (cntr == 1)
                    wolfForm.richTextBox1.Text += "  ";
                else if (cntr == 2)
                    wolfForm.richTextBox1.Text += "   ";
                else if (cntr == 3)
                    wolfForm.richTextBox1.Text += "    ";
            }
            richTextBox1.Text.Trim();

            //get all the lines out as an array
            string[] lines = richTextBox1.Lines;

            
            wolfForm.wolfRTB.Lines = lines;            

            wolfForm.ShowDialog();
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

        private void swapGoldForWhiteFoursome(string minGoldTeam, string maxGoldTeam)
        {
            //wp => WhitePlayer, gp => GoldPlayer
            int wpIndx = 0, gpIndx = 0;

            if (maxGoldTeamA != null && minGoldTeamA != null)
            {
                //Swap out a Gold player for an Non-Gold player with a similar HCP in the team that has the fewest Gold players.
                if (maxGoldTeamA.Contains("team1"))
                {
                    //Find a Player of the maxGoldTeam to swap
                    for (gpIndx = 3; gpIndx < 6; gpIndx++)
                    {
                        //Break out of the loop once a gold tee player is found
                        if (richTextBox2.Lines[gpIndx].Contains("*")) { goldplayerToReplace = richTextBox2.Lines[gpIndx]; break; }
                    }

                    // Replace a white tee player on minGoldTeam with a golds tee player from maxGoldteam
                    if (minGoldTeamA.Contains("team2"))
                    {
                        //cycle thru white tee players on minGoldTeam
                        for (wpIndx = 9; wpIndx < 12; wpIndx++)
                        {
                            //Break out of the loop once a white tee player is found and the swap of players is done
                            if (!richTextBox2.Lines[wpIndx].Contains("*"))
                            {
                                whiteplayerToReplace = richTextBox2.Lines[wpIndx];
                                changeLine(richTextBox2, wpIndx, goldplayerToReplace);
                                changeLine(richTextBox2, gpIndx, whiteplayerToReplace);
                                //Adjust count of MIN/MAX TEAMs
                                team2g++; team1g--; break;
                            }
                        }
                    }
                    if (minGoldTeamA.Contains("team3"))
                    {
                        //cycle thru white tee players on minGoldTeam
                        for (wpIndx = 15; wpIndx < 18; wpIndx++)
                        {
                            if (!richTextBox2.Lines[wpIndx].Contains("*"))
                            {
                                whiteplayerToReplace = richTextBox2.Lines[wpIndx];
                                changeLine(richTextBox2, wpIndx, goldplayerToReplace);
                                changeLine(richTextBox2, gpIndx, whiteplayerToReplace);

                                team3g++; team1g--; break;
                            }
                        }
                    }
                    if (minGoldTeamA.Contains("team4"))
                    {
                        //cycle thru white tee players on minGoldTeam
                        for (wpIndx = 21; wpIndx < 24; wpIndx++)
                        {
                            if (!richTextBox2.Lines[wpIndx].Contains("*"))
                            {
                                whiteplayerToReplace = richTextBox2.Lines[wpIndx];
                                changeLine(richTextBox2, wpIndx, goldplayerToReplace);
                                changeLine(richTextBox2, gpIndx, whiteplayerToReplace);

                                team4g++; team1g--; break;
                            }
                        }
                    }
                    if (minGoldTeamA.Contains("team5"))
                    {
                        //cycle thru white tee players on minGoldTeam
                        for (wpIndx = 27; wpIndx < 30; wpIndx++)
                        {
                            if (!richTextBox2.Lines[wpIndx].Contains("*"))
                            {
                                whiteplayerToReplace = richTextBox2.Lines[wpIndx];
                                changeLine(richTextBox2, wpIndx, goldplayerToReplace);
                                changeLine(richTextBox2, gpIndx, whiteplayerToReplace);

                                team5g++; team1g--; break;
                            }
                        }
                    }
                    if (minGoldTeamA.Contains("team6"))
                    {
                        //cycle thru white tee players on minGoldTeam
                        for (wpIndx = 33; wpIndx < 36; wpIndx++)
                        {
                            if (!richTextBox2.Lines[wpIndx].Contains("*"))
                            {
                                whiteplayerToReplace = richTextBox2.Lines[wpIndx];
                                changeLine(richTextBox2, wpIndx, goldplayerToReplace);
                                changeLine(richTextBox2, gpIndx, whiteplayerToReplace);

                                team6g++; team1g--; break;
                            }
                        }
                    }
                    if (minGoldTeamA.Contains("team7"))
                    {
                        //cycle thru white tee players on minGoldTeam
                        for (wpIndx = 39; wpIndx < 42; wpIndx++)
                        {
                            if (!richTextBox2.Lines[wpIndx].Contains("*"))
                            {
                                whiteplayerToReplace = richTextBox2.Lines[wpIndx];
                                changeLine(richTextBox2, wpIndx, goldplayerToReplace);
                                changeLine(richTextBox2, gpIndx, whiteplayerToReplace);

                                team7g++; team1g--; break;
                            }
                        }
                    }
                }
                else if (maxGoldTeamA.Contains("team2"))
                {
                    //Find a Player of the maxGoldTeam to swap
                    for (gpIndx = 9; gpIndx < 12; gpIndx++)
                    {
                        if (richTextBox2.Lines[gpIndx].Contains("*")) { goldplayerToReplace = richTextBox2.Lines[gpIndx]; break; }
                    }

                    // Replace a white tee player on minGoldTeam with gold tee player from maxGoldTeam
                    if (minGoldTeamA.Contains("team1"))
                    {
                        //cycle thru white tee players on minGoldTeam
                        for (wpIndx = 3; wpIndx < 6; wpIndx++)
                        {
                            if (!richTextBox2.Lines[wpIndx].Contains("*"))
                            {
                                whiteplayerToReplace = richTextBox2.Lines[wpIndx];
                                changeLine(richTextBox2, wpIndx, goldplayerToReplace);
                                changeLine(richTextBox2, gpIndx, whiteplayerToReplace);

                                team1g++; team2g--; break;
                            }
                        }
                    }
                    if (minGoldTeamA.Contains("team3"))
                    {
                        //cycle thru white tee players on minGoldTeam
                        for (wpIndx = 15; wpIndx < 18; wpIndx++)
                        {
                            if (!richTextBox2.Lines[wpIndx].Contains("*"))
                            {
                                whiteplayerToReplace = richTextBox2.Lines[wpIndx];
                                changeLine(richTextBox2, wpIndx, goldplayerToReplace);
                                changeLine(richTextBox2, gpIndx, whiteplayerToReplace);

                                team3g++; team2g--; break;
                            }
                        }

                    }
                    if (minGoldTeamA.Contains("team4"))
                    {
                        //cycle thru white tee players on minGoldTeam
                        for (wpIndx = 21; wpIndx < 24; wpIndx++)
                        {
                            if (!richTextBox2.Lines[wpIndx].Contains("*"))
                            {
                                whiteplayerToReplace = richTextBox2.Lines[wpIndx];
                                changeLine(richTextBox2, wpIndx, goldplayerToReplace);
                                changeLine(richTextBox2, gpIndx, whiteplayerToReplace);

                                team4g++; team2g--; break;
                            }
                        }

                    }
                    if (minGoldTeamA.Contains("team5"))
                    {
                        //cycle thru white tee players on minGoldTeam
                        for (wpIndx = 27; wpIndx < 30; wpIndx++)
                        {
                            if (!richTextBox2.Lines[wpIndx].Contains("*"))
                            {
                                whiteplayerToReplace = richTextBox2.Lines[wpIndx];
                                changeLine(richTextBox2, wpIndx, goldplayerToReplace);
                                changeLine(richTextBox2, gpIndx, whiteplayerToReplace);

                                team5g++; team2g--; break;
                            }
                        }

                    }
                    if (minGoldTeamA.Contains("team6"))
                    {
                        //cycle thru white tee players on minGoldTeam
                        for (wpIndx = 33; wpIndx < 36; wpIndx++)
                        {
                            if (!richTextBox2.Lines[wpIndx].Contains("*"))
                            {
                                whiteplayerToReplace = richTextBox2.Lines[wpIndx];
                                changeLine(richTextBox2, wpIndx, goldplayerToReplace);
                                changeLine(richTextBox2, gpIndx, whiteplayerToReplace);

                                team6g++; team2g--; break;
                            }
                        }

                    }
                    if (minGoldTeamA.Contains("team7"))
                    {
                        //cycle thru white tee players on minGoldTeam
                        for (wpIndx = 39; wpIndx < 42; wpIndx++)
                        {
                            if (!richTextBox2.Lines[wpIndx].Contains("*"))
                            {
                                whiteplayerToReplace = richTextBox2.Lines[wpIndx];
                                changeLine(richTextBox2, wpIndx, goldplayerToReplace);
                                changeLine(richTextBox2, gpIndx, whiteplayerToReplace);

                                team7g++; team2g--; break;
                            }
                        }
                    }
                }
                else if (maxGoldTeamA.Contains("team3"))
                {
                    //Find a Player of the maxGoldTeam to swap
                    for (gpIndx = 15; gpIndx < 18; gpIndx++)
                    {
                        if (richTextBox2.Lines[gpIndx].Contains("*")) { goldplayerToReplace = richTextBox2.Lines[gpIndx]; break; }
                    }

                    // Replace a white tee player on team X with a gold tee player from team1
                    if (minGoldTeamA.Contains("team1"))
                    {
                        //cycle thru white tee players on minGoldTeam
                        for (wpIndx = 3; wpIndx < 6; wpIndx++)
                        {
                            if (!richTextBox2.Lines[wpIndx].Contains("*"))
                            {
                                whiteplayerToReplace = richTextBox2.Lines[wpIndx];
                                changeLine(richTextBox2, wpIndx, goldplayerToReplace);
                                changeLine(richTextBox2, gpIndx, whiteplayerToReplace);

                                team1g++; team3g--; break;
                            }
                        }

                    }
                    if (minGoldTeamA.Contains("team2"))
                    {
                        //cycle thru white tee players on minGoldTeam
                        for (wpIndx = 9; wpIndx < 12; wpIndx++)
                        {
                            if (!richTextBox2.Lines[wpIndx].Contains("*"))
                            {
                                whiteplayerToReplace = richTextBox2.Lines[wpIndx];
                                changeLine(richTextBox2, wpIndx, goldplayerToReplace);
                                changeLine(richTextBox2, gpIndx, whiteplayerToReplace);

                                team2g++; team3g--; break;
                            }
                        }

                    }
                    if (minGoldTeamA.Contains("team4"))
                    {
                        //cycle thru white tee players on minGoldTeam
                        for (wpIndx = 21; wpIndx < 24; wpIndx++)
                        {
                            if (!richTextBox2.Lines[wpIndx].Contains("*"))
                            {
                                whiteplayerToReplace = richTextBox2.Lines[wpIndx];
                                changeLine(richTextBox2, wpIndx, goldplayerToReplace);
                                changeLine(richTextBox2, gpIndx, whiteplayerToReplace);

                                team4g++; team2g--; break;
                            }
                        }

                    }
                    if (minGoldTeamA.Contains("team5"))
                    {
                        //cycle thru white tee players on minGoldTeam
                        for (wpIndx = 27; wpIndx < 30; wpIndx++)
                        {
                            if (!richTextBox2.Lines[wpIndx].Contains("*"))
                            {
                                whiteplayerToReplace = richTextBox2.Lines[wpIndx];
                                changeLine(richTextBox2, wpIndx, goldplayerToReplace);
                                changeLine(richTextBox2, gpIndx, whiteplayerToReplace);

                                team5g++; team3g--; break;
                            }
                        }
                    }
                    if (minGoldTeamA.Contains("team6"))
                    {
                        //cycle thru white tee players on minGoldTeam
                        for (wpIndx = 33; wpIndx < 36; wpIndx++)
                        {
                            if (!richTextBox2.Lines[wpIndx].Contains("*"))
                            {
                                whiteplayerToReplace = richTextBox2.Lines[wpIndx];
                                changeLine(richTextBox2, wpIndx, goldplayerToReplace);
                                changeLine(richTextBox2, gpIndx, whiteplayerToReplace);

                                team6g++; team3g--; break;
                            }
                        }
                    }
                    if (minGoldTeamA.Contains("team7"))
                    {
                        //cycle thru white tee players on minGoldTeam
                        for (wpIndx = 39; wpIndx < 42; wpIndx++)
                        {
                            if (!richTextBox2.Lines[wpIndx].Contains("*"))
                            {
                                whiteplayerToReplace = richTextBox2.Lines[wpIndx];
                                changeLine(richTextBox2, wpIndx, goldplayerToReplace);
                                changeLine(richTextBox2, gpIndx, whiteplayerToReplace);

                                team7g++; team3g--; break;
                            }
                        }
                    }
                }
                else if (maxGoldTeamA.Contains("team4"))
                {
                    //Find a Player of the maxGoldTeam to swap
                    for (gpIndx = 21; gpIndx < 24; gpIndx++)
                    {
                        if (richTextBox2.Lines[gpIndx].Contains("*")) { goldplayerToReplace = richTextBox2.Lines[gpIndx]; break; }
                    }

                    // Replace a white tee player on team X with a gold tee player from team1
                    if (minGoldTeamA.Contains("team1"))
                    {
                        //cycle thru white tee players on minGoldTeam
                        for (wpIndx = 3; wpIndx < 6; wpIndx++)
                        {
                            if (!richTextBox2.Lines[wpIndx].Contains("*"))
                            {
                                whiteplayerToReplace = richTextBox2.Lines[wpIndx];
                                changeLine(richTextBox2, wpIndx, goldplayerToReplace);
                                changeLine(richTextBox2, gpIndx, whiteplayerToReplace);

                                team1g++; team4g--; break;
                            }
                        }
                    }
                    if (minGoldTeamA.Contains("team2"))
                    {
                        //cycle thru white tee players on minGoldTeam
                        for (wpIndx = 9; wpIndx < 12; wpIndx++)
                        {
                            if (!richTextBox2.Lines[wpIndx].Contains("*"))
                            {
                                whiteplayerToReplace = richTextBox2.Lines[wpIndx];
                                changeLine(richTextBox2, wpIndx, goldplayerToReplace);
                                changeLine(richTextBox2, gpIndx, whiteplayerToReplace);

                                team2g++; team4g--; break;
                            }
                        }
                    }
                    if (minGoldTeamA.Contains("team3"))
                    {
                        //cycle thru white tee players on minGoldTeam
                        for (wpIndx = 15; wpIndx < 18; wpIndx++)
                        {
                            if (!richTextBox2.Lines[wpIndx].Contains("*"))
                            {
                                whiteplayerToReplace = richTextBox2.Lines[wpIndx];
                                changeLine(richTextBox2, wpIndx, goldplayerToReplace);
                                changeLine(richTextBox2, gpIndx, whiteplayerToReplace);

                                team3g++; team4g--; break;
                            }
                        }
                    }
                    if (minGoldTeamA.Contains("team5"))
                    {
                        //cycle thru white tee players on minGoldTeam
                        for (wpIndx = 27; wpIndx < 30; wpIndx++)
                        {
                            if (!richTextBox2.Lines[wpIndx].Contains("*"))
                            {
                                whiteplayerToReplace = richTextBox2.Lines[wpIndx];
                                changeLine(richTextBox2, wpIndx, goldplayerToReplace);
                                changeLine(richTextBox2, gpIndx, whiteplayerToReplace);

                                team5g++; team4g--; break;
                            }
                        }
                    }
                    if (minGoldTeamA.Contains("team6"))
                    {
                        //cycle thru white tee players on minGoldTeam
                        for (wpIndx = 33; wpIndx < 36; wpIndx++)
                        {
                            if (!richTextBox2.Lines[wpIndx].Contains("*"))
                            {
                                whiteplayerToReplace = richTextBox2.Lines[wpIndx];
                                changeLine(richTextBox2, wpIndx, goldplayerToReplace);
                                changeLine(richTextBox2, gpIndx, whiteplayerToReplace);

                                team6g++; team4g--; break;
                            }
                        }
                    }
                    if (minGoldTeamA.Contains("team7"))
                    {
                        //cycle thru white tee players on minGoldTeam
                        for (wpIndx = 39; wpIndx < 42; wpIndx++)
                        {
                            if (!richTextBox2.Lines[wpIndx].Contains("*"))
                            {
                                whiteplayerToReplace = richTextBox2.Lines[wpIndx];
                                changeLine(richTextBox2, wpIndx, goldplayerToReplace);
                                changeLine(richTextBox2, gpIndx, whiteplayerToReplace);

                                team7g++; team4g--; break;
                            }
                        }
                    }
                }
                else if (maxGoldTeamA.Contains("team5"))
                {
                    //Find a Player of the maxGoldTeam to swap
                    for (gpIndx = 27; gpIndx < 30; gpIndx++)
                    {
                        if (richTextBox2.Lines[gpIndx].Contains("*")) { goldplayerToReplace = richTextBox2.Lines[gpIndx]; break; }
                    }

                    // Replace a white tee player on team X with a gold tee player from team1
                    if (minGoldTeamA.Contains("team1"))
                    {
                        //cycle thru white tee players on minGoldTeam
                        for (wpIndx = 3; wpIndx < 6; wpIndx++)
                        {
                            if (!richTextBox2.Lines[wpIndx].Contains("*"))
                            {
                                whiteplayerToReplace = richTextBox2.Lines[wpIndx];
                                changeLine(richTextBox2, wpIndx, goldplayerToReplace);
                                changeLine(richTextBox2, gpIndx, whiteplayerToReplace);

                                team1g++; team5g--; break;
                            }
                        }
                    }
                    if (minGoldTeamA.Contains("team2"))
                    {
                        //cycle thru white tee players on minGoldTeam
                        for (wpIndx = 9; wpIndx < 12; wpIndx++)
                        {
                            if (!richTextBox2.Lines[wpIndx].Contains("*"))
                            {
                                whiteplayerToReplace = richTextBox2.Lines[wpIndx];
                                changeLine(richTextBox2, wpIndx, goldplayerToReplace);
                                changeLine(richTextBox2, gpIndx, whiteplayerToReplace);

                                team2g++; team7g--; break;
                            }
                        }
                    }
                    if (minGoldTeamA.Contains("team3"))
                    {
                        //cycle thru white tee players on minGoldTeam
                        for (wpIndx = 15; wpIndx < 18; wpIndx++)
                        {
                            if (!richTextBox2.Lines[wpIndx].Contains("*"))
                            {
                                whiteplayerToReplace = richTextBox2.Lines[wpIndx];
                                changeLine(richTextBox2, wpIndx, goldplayerToReplace);
                                changeLine(richTextBox2, gpIndx, whiteplayerToReplace);

                                team3g++; team5g--; break;
                            }
                        }
                    }
                    if (minGoldTeamA.Contains("team4"))
                    {
                        //cycle thru white tee players on minGoldTeam
                        for (wpIndx = 21; wpIndx < 24; wpIndx++)
                        {
                            if (!richTextBox2.Lines[wpIndx].Contains("*"))
                            {
                                whiteplayerToReplace = richTextBox2.Lines[wpIndx];
                                changeLine(richTextBox2, wpIndx, goldplayerToReplace);
                                changeLine(richTextBox2, gpIndx, whiteplayerToReplace);

                                team4g++; team5g--; break;
                            }
                        }
                    }
                    if (minGoldTeamA.Contains("team6"))
                    {
                        //cycle thru white tee players on minGoldTeam
                        for (wpIndx = 33; wpIndx < 36; wpIndx++)
                        {
                            if (!richTextBox2.Lines[wpIndx].Contains("*"))
                            {
                                whiteplayerToReplace = richTextBox2.Lines[wpIndx];
                                changeLine(richTextBox2, wpIndx, goldplayerToReplace);
                                changeLine(richTextBox2, gpIndx, whiteplayerToReplace);

                                team6g++; team5g--; break;
                            }
                        }
                    }
                    if (minGoldTeamA.Contains("team7"))
                    {
                        //cycle thru white tee players on minGoldTeam
                        for (wpIndx = 39; wpIndx < 42; wpIndx++)
                        {
                            if (!richTextBox2.Lines[wpIndx].Contains("*"))
                            {
                                whiteplayerToReplace = richTextBox2.Lines[wpIndx];
                                changeLine(richTextBox2, wpIndx, goldplayerToReplace);
                                changeLine(richTextBox2, gpIndx, whiteplayerToReplace);

                                team7g++; team5g--; break;
                            }
                        }
                    }
                }
                else if (maxGoldTeamA.Contains("team6"))
                {
                    //Find a Player of the maxGoldTeam to swap
                    for (gpIndx = 33; gpIndx < 36; gpIndx++)
                    {
                        if (richTextBox2.Lines[gpIndx].Contains("*")) { goldplayerToReplace = richTextBox2.Lines[gpIndx]; break; }
                    }

                    // Replace a white tee player on team X with a gold tee player from team1
                    if (minGoldTeamA.Contains("team1"))
                    {
                        //cycle thru white tee players on minGoldTeam
                        for (wpIndx = 3; wpIndx < 6; wpIndx++)
                        {
                            if (!richTextBox2.Lines[wpIndx].Contains("*"))
                            {
                                whiteplayerToReplace = richTextBox2.Lines[wpIndx];
                                changeLine(richTextBox2, wpIndx, goldplayerToReplace);
                                changeLine(richTextBox2, gpIndx, whiteplayerToReplace);

                                team1g++; team6g--; break;
                            }
                        }
                    }
                    if (minGoldTeamA.Contains("team2"))
                    {
                        //cycle thru white tee players on minGoldTeam
                        for (wpIndx = 9; wpIndx < 12; wpIndx++)
                        {
                            if (!richTextBox2.Lines[wpIndx].Contains("*"))
                            {
                                whiteplayerToReplace = richTextBox2.Lines[wpIndx];
                                changeLine(richTextBox2, wpIndx, goldplayerToReplace);
                                changeLine(richTextBox2, gpIndx, whiteplayerToReplace);

                                team2g++; team7g--; break;
                            }
                        }
                    }
                    if (minGoldTeamA.Contains("team3"))
                    {
                        //cycle thru white tee players on minGoldTeam
                        for (wpIndx = 15; wpIndx < 18; wpIndx++)
                        {
                            if (!richTextBox2.Lines[wpIndx].Contains("*"))
                            {
                                whiteplayerToReplace = richTextBox2.Lines[wpIndx];
                                changeLine(richTextBox2, wpIndx, goldplayerToReplace);
                                changeLine(richTextBox2, gpIndx, whiteplayerToReplace);

                                team3g++; team6g--; break;
                            }
                        }
                    }
                    if (minGoldTeamA.Contains("team4"))
                    {
                        //cycle thru white tee players on minGoldTeam
                        for (wpIndx = 21; wpIndx < 24; wpIndx++)
                        {
                            if (!richTextBox2.Lines[wpIndx].Contains("*"))
                            {
                                whiteplayerToReplace = richTextBox2.Lines[wpIndx];
                                changeLine(richTextBox2, wpIndx, goldplayerToReplace);
                                changeLine(richTextBox2, gpIndx, whiteplayerToReplace);

                                team4g++; team6g--; break;
                            }
                        }
                    }
                    if (minGoldTeamA.Contains("team5"))
                    {
                        //cycle thru white tee players on minGoldTeam
                        for (wpIndx = 27; wpIndx < 30; wpIndx++)
                        {
                            if (!richTextBox2.Lines[wpIndx].Contains("*"))
                            {
                                whiteplayerToReplace = richTextBox2.Lines[wpIndx];
                                changeLine(richTextBox2, wpIndx, goldplayerToReplace);
                                changeLine(richTextBox2, gpIndx, whiteplayerToReplace);

                                team5g++; team6g--; break;
                            }
                        }
                    }
                    if (minGoldTeamA.Contains("team7"))
                    {
                        //cycle thru white tee players on minGoldTeam
                        for (wpIndx = 39; wpIndx < 42; wpIndx++)
                        {
                            if (!richTextBox2.Lines[wpIndx].Contains("*"))
                            {
                                whiteplayerToReplace = richTextBox2.Lines[wpIndx];
                                changeLine(richTextBox2, wpIndx, goldplayerToReplace);
                                changeLine(richTextBox2, gpIndx, whiteplayerToReplace);

                                team7g++; team6g--; break;
                            }
                        }
                    }
                }
                else if (maxGoldTeamA.Contains("team7"))
                {
                    //Find a Player of the maxGoldTeam to swap
                    for (gpIndx = 39; gpIndx < 42; gpIndx++)
                    {
                        if (richTextBox2.Lines[gpIndx].Contains("*")) { goldplayerToReplace = richTextBox2.Lines[gpIndx]; break; }
                    }

                    // Replace a white tee player on team X with a gold tee player from team1
                    if (minGoldTeamA.Contains("team1"))
                    {
                        //cycle thru white tee players on minGoldTeam
                        for (wpIndx = 3; wpIndx < 6; wpIndx++)
                        {
                            if (!richTextBox2.Lines[wpIndx].Contains("*"))
                            {
                                whiteplayerToReplace = richTextBox2.Lines[wpIndx];
                                changeLine(richTextBox2, wpIndx, goldplayerToReplace);
                                changeLine(richTextBox2, gpIndx, whiteplayerToReplace);

                                team1g++; team7g--; break;
                            }
                        }
                    }
                    if (minGoldTeamA.Contains("team2"))
                    {
                        //cycle thru white tee players on minGoldTeam
                        for (wpIndx = 9; wpIndx < 12; wpIndx++)
                        {
                            if (!richTextBox2.Lines[wpIndx].Contains("*"))
                            {
                                whiteplayerToReplace = richTextBox2.Lines[wpIndx];
                                changeLine(richTextBox2, wpIndx, goldplayerToReplace);
                                changeLine(richTextBox2, gpIndx, whiteplayerToReplace);

                                team2g++; team7g--; break;
                            }
                        }
                    }
                    if (minGoldTeamA.Contains("team3"))
                    {
                        //cycle thru white tee players on minGoldTeam
                        for (wpIndx = 15; wpIndx < 18; wpIndx++)
                        {
                            if (!richTextBox2.Lines[wpIndx].Contains("*"))
                            {
                                whiteplayerToReplace = richTextBox2.Lines[wpIndx];
                                changeLine(richTextBox2, wpIndx, goldplayerToReplace);
                                changeLine(richTextBox2, gpIndx, whiteplayerToReplace);

                                team3g++; team7g--; break;
                            }
                        }
                    }
                    if (minGoldTeamA.Contains("team4"))
                    {
                        //cycle thru white tee players on minGoldTeam
                        for (wpIndx = 21; wpIndx < 24; wpIndx++)
                        {
                            if (!richTextBox2.Lines[wpIndx].Contains("*"))
                            {
                                whiteplayerToReplace = richTextBox2.Lines[wpIndx];
                                changeLine(richTextBox2, wpIndx, goldplayerToReplace);
                                changeLine(richTextBox2, gpIndx, whiteplayerToReplace);

                                team4g++; team7g--; break;
                            }
                        }
                    }
                    if (minGoldTeamA.Contains("team5"))
                    {
                        //cycle thru white tee players on minGoldTeam
                        for (wpIndx = 27; wpIndx < 30; wpIndx++)
                        {
                            if (!richTextBox2.Lines[wpIndx].Contains("*"))
                            {
                                whiteplayerToReplace = richTextBox2.Lines[wpIndx];
                                changeLine(richTextBox2, wpIndx, goldplayerToReplace);
                                changeLine(richTextBox2, gpIndx, whiteplayerToReplace);

                                team7g++; team7g--; break;
                            }
                        }
                    }
                    if (minGoldTeamA.Contains("team6"))
                    {
                        //cycle thru white tee players on minGoldTeam
                        for (wpIndx = 33; wpIndx < 36; wpIndx++)
                        {
                            if (!richTextBox2.Lines[wpIndx].Contains("*"))
                            {
                                whiteplayerToReplace = richTextBox2.Lines[wpIndx];
                                changeLine(richTextBox2, wpIndx, goldplayerToReplace);
                                changeLine(richTextBox2, gpIndx, whiteplayerToReplace);

                                team6g++; team7g--; break;
                            }
                        }
                    }
                }
            }

            // Refresh the gold golfers Array with updated values now that the players have been swapped once
            goldGolfers[0] = team1g; goldGolfers[1] = team2g; goldGolfers[2] = team3g;
            goldGolfers[3] = team4g; goldGolfers[4] = team5g; goldGolfers[5] = team6g;
            goldGolfers[6] = team7g;
        }

        private void swapGoldForWhiteThreesome()
        {
            //Swap out a Gold player for an Non-Gold player with a similar HCP in the team that has the fewest Gold players.
            goldplayerToReplace = null;

            if (maxGoldTeamA.Contains("team1"))
            {
                if (richTextBox2.Lines[3].Contains("*"))
                {
                    goldplayerToReplace = richTextBox2.Lines[3];    //2nd Player of the Team with max Golds

                    if (minGoldTeamA.Contains("team2"))
                    {
                        if (!richTextBox2.Lines[8].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[8];
                            changeLine(richTextBox2, 8, goldplayerToReplace);
                            changeLine(richTextBox2, 3, whiteplayerToReplace);
                            //Adjust count of MIN/MAX TEAMs
                            team2g++; team1g--;
                        }
                        else if (!richTextBox2.Lines[9].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[9];
                            changeLine(richTextBox2, 9, goldplayerToReplace);
                            changeLine(richTextBox2, 3, whiteplayerToReplace);
                            //Adjust count of MIN/MAX TEAMs
                            team2g++; team1g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team3"))
                    {
                        if (!richTextBox2.Lines[13].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[13];
                            changeLine(richTextBox2, 13, goldplayerToReplace);
                            changeLine(richTextBox2, 3, whiteplayerToReplace);
                            team3g++; team1g--;
                        }
                        else if (!richTextBox2.Lines[14].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[14];
                            changeLine(richTextBox2, 14, goldplayerToReplace);
                            changeLine(richTextBox2, 3, whiteplayerToReplace);
                            team3g++; team1g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team4"))
                    {
                        if (!richTextBox2.Lines[18].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[18];
                            changeLine(richTextBox2, 18, goldplayerToReplace);
                            changeLine(richTextBox2, 3, whiteplayerToReplace);
                            team4g++; team1g--;
                        }
                        else if (!richTextBox2.Lines[19].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[19];
                            changeLine(richTextBox2, 19, goldplayerToReplace);
                            changeLine(richTextBox2, 3, whiteplayerToReplace);
                            team4g++; team1g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team5"))
                    {
                        if (!richTextBox2.Lines[23].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[23];

                            changeLine(richTextBox2, 23, goldplayerToReplace);
                            changeLine(richTextBox2, 3, whiteplayerToReplace);
                            team5g++; team1g--;
                        }
                        else if (!richTextBox2.Lines[24].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[24];

                            changeLine(richTextBox2, 24, goldplayerToReplace);
                            changeLine(richTextBox2, 3, whiteplayerToReplace);
                            team5g++; team1g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team6"))
                    {
                        if (!richTextBox2.Lines[28].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[28];
                            changeLine(richTextBox2, 28, goldplayerToReplace);
                            changeLine(richTextBox2, 3, whiteplayerToReplace);
                            team6g++; team1g--;
                        }
                        else if (!richTextBox2.Lines[29].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[29];
                            changeLine(richTextBox2, 29, goldplayerToReplace);
                            changeLine(richTextBox2, 3, whiteplayerToReplace);
                            team6g++; team1g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team7"))
                    {
                        if (!richTextBox2.Lines[33].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[33];

                            changeLine(richTextBox2, 33, goldplayerToReplace);
                            changeLine(richTextBox2, 3, whiteplayerToReplace);
                            team7g++; team1g--;
                        }
                        else if (!richTextBox2.Lines[34].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[34];

                            changeLine(richTextBox2, 34, goldplayerToReplace);
                            changeLine(richTextBox2, 3, whiteplayerToReplace);
                            team7g++; team1g--;
                        }
                    }
                }
                else if (richTextBox2.Lines[4].Contains("*"))
                {
                    goldplayerToReplace = richTextBox2.Lines[4];    //3rd Player of the Team with max Golds

                    if (minGoldTeamA.Contains("team2"))
                    {
                        if (!richTextBox2.Lines[8].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[8];
                            changeLine(richTextBox2, 8, goldplayerToReplace);
                            changeLine(richTextBox2, 4, whiteplayerToReplace);
                            //Adjust count of MIN/MAX TEAMs
                            team2g++; team1g--;
                        }
                        else if (!richTextBox2.Lines[9].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[9];
                            changeLine(richTextBox2, 9, goldplayerToReplace);
                            changeLine(richTextBox2, 4, whiteplayerToReplace);
                            //Adjust count of MIN/MAX TEAMs
                            team2g++; team1g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team3"))
                    {
                        if (!richTextBox2.Lines[13].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[13];
                            changeLine(richTextBox2, 13, goldplayerToReplace);
                            changeLine(richTextBox2, 4, whiteplayerToReplace);
                            team3g++; team1g--;
                        }
                        else if (!richTextBox2.Lines[14].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[14];
                            changeLine(richTextBox2, 14, goldplayerToReplace);
                            changeLine(richTextBox2, 4, whiteplayerToReplace);
                            team3g++; team1g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team4"))
                    {
                        if (!richTextBox2.Lines[18].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[18];
                            changeLine(richTextBox2, 18, goldplayerToReplace);
                            changeLine(richTextBox2, 4, whiteplayerToReplace);
                            team4g++; team1g--;
                        }
                        else if (!richTextBox2.Lines[19].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[19];
                            changeLine(richTextBox2, 19, goldplayerToReplace);
                            changeLine(richTextBox2, 4, whiteplayerToReplace);
                            team4g++; team1g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team5"))
                    {
                        if (!richTextBox2.Lines[23].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[23];

                            changeLine(richTextBox2, 23, goldplayerToReplace);
                            changeLine(richTextBox2, 4, whiteplayerToReplace);
                            team5g++; team1g--;
                        }
                        else if (!richTextBox2.Lines[24].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[24];

                            changeLine(richTextBox2, 24, goldplayerToReplace);
                            changeLine(richTextBox2, 4, whiteplayerToReplace);
                            team5g++; team1g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team6"))
                    {
                        if (!richTextBox2.Lines[28].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[28];
                            changeLine(richTextBox2, 28, goldplayerToReplace);
                            changeLine(richTextBox2, 4, whiteplayerToReplace);
                            team6g++; team1g--;
                        }
                        else if (!richTextBox2.Lines[29].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[29];
                            changeLine(richTextBox2, 29, goldplayerToReplace);
                            changeLine(richTextBox2, 4, whiteplayerToReplace);
                            team6g++; team1g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team7"))
                    {
                        if (!richTextBox2.Lines[33].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[33];

                            changeLine(richTextBox2, 33, goldplayerToReplace);
                            changeLine(richTextBox2, 4, whiteplayerToReplace);
                            team7g++; team1g--;
                        }
                        else if (!richTextBox2.Lines[34].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[34];

                            changeLine(richTextBox2, 34, goldplayerToReplace);
                            changeLine(richTextBox2, 4, whiteplayerToReplace);
                            team7g++; team1g--;
                        }
                    }
                }
            }
            else if (maxGoldTeamA.Contains("team2"))
            {
                if (richTextBox2.Lines[8].Contains("*"))
                {
                    //2nd Player of the Team with max Golds
                    goldplayerToReplace = richTextBox2.Lines[8];

                    if (minGoldTeamA.Contains("team1"))
                    {
                        if (!richTextBox2.Lines[3].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[3];
                            changeLine(richTextBox2, 3, goldplayerToReplace);
                            changeLine(richTextBox2, 8, whiteplayerToReplace);
                            team1g++; team2g--;
                        }
                        else if (!richTextBox2.Lines[4].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[4];
                            changeLine(richTextBox2, 4, goldplayerToReplace);
                            changeLine(richTextBox2, 8, whiteplayerToReplace);
                            team1g++; team2g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team3"))
                    {
                        if (!richTextBox2.Lines[13].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[13];
                            changeLine(richTextBox2, 13, goldplayerToReplace);
                            changeLine(richTextBox2, 8, whiteplayerToReplace);
                            team3g++; team2g--;
                        }
                        else if (!richTextBox2.Lines[14].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[14];
                            changeLine(richTextBox2, 14, goldplayerToReplace);
                            changeLine(richTextBox2, 8, whiteplayerToReplace);
                            team3g++; team2g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team4"))
                    {
                        if (!richTextBox2.Lines[18].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[18];
                            changeLine(richTextBox2, 18, goldplayerToReplace);
                            changeLine(richTextBox2, 8, whiteplayerToReplace);
                            team4g++; team2g--;
                        }
                        else if (!richTextBox2.Lines[19].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[19];
                            changeLine(richTextBox2, 19, goldplayerToReplace);
                            changeLine(richTextBox2, 8, whiteplayerToReplace);
                            team4g++; team2g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team5"))
                    {
                        if (!richTextBox2.Lines[23].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[23];
                            changeLine(richTextBox2, 23, goldplayerToReplace);
                            changeLine(richTextBox2, 8, whiteplayerToReplace);
                            team5g++; team2g--;
                        }
                        else if (!richTextBox2.Lines[24].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[24];
                            changeLine(richTextBox2, 24, goldplayerToReplace);
                            changeLine(richTextBox2, 8, whiteplayerToReplace);
                            team5g++; team2g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team6"))
                    {
                        if (!richTextBox2.Lines[28].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[28];
                            changeLine(richTextBox2, 28, goldplayerToReplace);
                            changeLine(richTextBox2, 8, whiteplayerToReplace);
                            team6g++; team2g--;
                        }
                        else if (!richTextBox2.Lines[29].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[29];
                            changeLine(richTextBox2, 29, goldplayerToReplace);
                            changeLine(richTextBox2, 8, whiteplayerToReplace);
                            team6g++; team2g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team7"))
                    {
                        if (!richTextBox2.Lines[33].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[33];
                            changeLine(richTextBox2, 33, goldplayerToReplace);
                            changeLine(richTextBox2, 8, whiteplayerToReplace);
                            team7g++; team2g--;
                        }
                        else if (!richTextBox2.Lines[34].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[34];
                            changeLine(richTextBox2, 34, goldplayerToReplace);
                            changeLine(richTextBox2, 8, whiteplayerToReplace);
                            team7g++; team2g--;
                        }
                    }
                }
                else if (richTextBox2.Lines[9].Contains("*"))
                {
                    goldplayerToReplace = richTextBox2.Lines[9];

                    if (minGoldTeamA.Contains("team1"))
                    {
                        if (!richTextBox2.Lines[3].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[3];
                            changeLine(richTextBox2, 3, goldplayerToReplace);
                            changeLine(richTextBox2, 9, whiteplayerToReplace);
                            team1g++; team2g--;
                        }
                        else if (!richTextBox2.Lines[4].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[4];
                            changeLine(richTextBox2, 4, goldplayerToReplace);
                            changeLine(richTextBox2, 9, whiteplayerToReplace);
                            team1g++; team2g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team3"))
                    {
                        if (!richTextBox2.Lines[13].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[13];
                            changeLine(richTextBox2, 13, goldplayerToReplace);
                            changeLine(richTextBox2, 9, whiteplayerToReplace);
                            team3g++; team2g--;
                        }
                        else if (!richTextBox2.Lines[14].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[14];
                            changeLine(richTextBox2, 14, goldplayerToReplace);
                            changeLine(richTextBox2, 9, whiteplayerToReplace);
                            team3g++; team2g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team4"))
                    {
                        if (!richTextBox2.Lines[18].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[18];
                            changeLine(richTextBox2, 18, goldplayerToReplace);
                            changeLine(richTextBox2, 9, whiteplayerToReplace);
                            team4g++; team2g--;
                        }
                        else if (!richTextBox2.Lines[19].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[19];
                            changeLine(richTextBox2, 19, goldplayerToReplace);
                            changeLine(richTextBox2, 9, whiteplayerToReplace);
                            team4g++; team2g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team5"))
                    {
                        if (!richTextBox2.Lines[23].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[23];
                            changeLine(richTextBox2, 23, goldplayerToReplace);
                            changeLine(richTextBox2, 9, whiteplayerToReplace);
                            team5g++; team2g--;
                        }
                        else if (!richTextBox2.Lines[24].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[24];
                            changeLine(richTextBox2, 24, goldplayerToReplace);
                            changeLine(richTextBox2, 9, whiteplayerToReplace);
                            team5g++; team2g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team6"))
                    {
                        if (!richTextBox2.Lines[28].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[28];
                            changeLine(richTextBox2, 28, goldplayerToReplace);
                            changeLine(richTextBox2, 9, whiteplayerToReplace);
                            team6g++; team2g--;
                        }
                        else if (!richTextBox2.Lines[29].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[29];
                            changeLine(richTextBox2, 29, goldplayerToReplace);
                            changeLine(richTextBox2, 9, whiteplayerToReplace);
                            team6g++; team2g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team7"))
                    {
                        if (!richTextBox2.Lines[33].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[33];
                            changeLine(richTextBox2, 33, goldplayerToReplace);
                            changeLine(richTextBox2, 9, whiteplayerToReplace);
                            team7g++; team2g--;
                        }
                        else if (!richTextBox2.Lines[34].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[34];
                            changeLine(richTextBox2, 34, goldplayerToReplace);
                            changeLine(richTextBox2, 9, whiteplayerToReplace);
                            team7g++; team2g--;
                        }
                    }
                }
            }
            else if (maxGoldTeamA.Contains("team3"))
            {
                if (richTextBox2.Lines[13].Contains("*"))
                {
                    goldplayerToReplace = richTextBox2.Lines[13];       //2nd Player of the Team with max Golds

                    if (minGoldTeamA.Contains("team1"))
                    {
                        if (!richTextBox2.Lines[3].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[3];
                            changeLine(richTextBox2, 3, goldplayerToReplace);
                            changeLine(richTextBox2, 13, whiteplayerToReplace);
                            team1g++; team3g--;
                        }
                        else if (!richTextBox2.Lines[4].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[4];
                            changeLine(richTextBox2, 4, goldplayerToReplace);
                            changeLine(richTextBox2, 13, whiteplayerToReplace);
                            team1g++; team3g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team2"))
                    {
                        if (!richTextBox2.Lines[8].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[8];
                            changeLine(richTextBox2, 8, goldplayerToReplace);
                            changeLine(richTextBox2, 13, whiteplayerToReplace);
                            team2g++; team3g--;
                        }
                        else if (!richTextBox2.Lines[9].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[9];
                            changeLine(richTextBox2, 9, goldplayerToReplace);
                            changeLine(richTextBox2, 13, whiteplayerToReplace);
                            team2g++; team3g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team4"))
                    {
                        if (!richTextBox2.Lines[18].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[18];
                            changeLine(richTextBox2, 18, goldplayerToReplace);
                            changeLine(richTextBox2, 13, whiteplayerToReplace);
                            team4g++; team3g--;
                        }
                        else if (!richTextBox2.Lines[19].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[19];
                            changeLine(richTextBox2, 19, goldplayerToReplace);
                            changeLine(richTextBox2, 13, whiteplayerToReplace);
                            team4g++; team3g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team5"))
                    {
                        if (!richTextBox2.Lines[23].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[23];
                            changeLine(richTextBox2, 23, goldplayerToReplace);
                            changeLine(richTextBox2, 13, whiteplayerToReplace);
                            team5g++; team3g--;
                        }
                        else if (!richTextBox2.Lines[24].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[24];
                            changeLine(richTextBox2, 24, goldplayerToReplace);
                            changeLine(richTextBox2, 13, whiteplayerToReplace);
                            team5g++; team3g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team6"))
                    {
                        if (!richTextBox2.Lines[28].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[28];
                            changeLine(richTextBox2, 28, goldplayerToReplace);
                            changeLine(richTextBox2, 13, whiteplayerToReplace);
                            team6g++; team3g--;
                        }
                        else if (!richTextBox2.Lines[29].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[29];
                            changeLine(richTextBox2, 29, goldplayerToReplace);
                            changeLine(richTextBox2, 13, whiteplayerToReplace);
                            team5g++; team3g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team7"))
                    {
                        if (!richTextBox2.Lines[33].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[33];
                            changeLine(richTextBox2, 33, goldplayerToReplace);
                            changeLine(richTextBox2, 13, whiteplayerToReplace);
                            team7g++; team3g--;
                        }
                        else if (!richTextBox2.Lines[34].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[34];
                            changeLine(richTextBox2, 34, goldplayerToReplace);
                            changeLine(richTextBox2, 13, whiteplayerToReplace);
                            team7g++; team3g--;
                        }
                    }
                }
                else if (richTextBox2.Lines[14].Contains("*"))
                {
                    goldplayerToReplace = richTextBox2.Lines[14];       //3rd Player of the Team with max Golds

                    if (minGoldTeamA.Contains("team1"))
                    {
                        if (!richTextBox2.Lines[3].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[3];
                            changeLine(richTextBox2, 3, goldplayerToReplace);
                            changeLine(richTextBox2, 14, whiteplayerToReplace);
                            team1g++; team3g--;
                        }
                        else if (!richTextBox2.Lines[4].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[4];
                            changeLine(richTextBox2, 4, goldplayerToReplace);
                            changeLine(richTextBox2, 14, whiteplayerToReplace);
                            team1g++; team3g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team2"))
                    {
                        if (!richTextBox2.Lines[8].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[8];
                            changeLine(richTextBox2, 8, goldplayerToReplace);
                            changeLine(richTextBox2, 14, whiteplayerToReplace);
                            team2g++; team3g--;
                        }
                        else if (!richTextBox2.Lines[9].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[9];
                            changeLine(richTextBox2, 9, goldplayerToReplace);
                            changeLine(richTextBox2, 14, whiteplayerToReplace);
                            team2g++; team3g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team4"))
                    {
                        if (!richTextBox2.Lines[18].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[18];
                            changeLine(richTextBox2, 18, goldplayerToReplace);
                            changeLine(richTextBox2, 14, whiteplayerToReplace);
                            team4g++; team3g--;
                        }
                        else if (!richTextBox2.Lines[19].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[19];
                            changeLine(richTextBox2, 19, goldplayerToReplace);
                            changeLine(richTextBox2, 14, whiteplayerToReplace);
                            team4g++; team3g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team5"))
                    {
                        if (!richTextBox2.Lines[23].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[23];
                            changeLine(richTextBox2, 23, goldplayerToReplace);
                            changeLine(richTextBox2, 14, whiteplayerToReplace);
                            team5g++; team3g--;
                        }
                        else if (!richTextBox2.Lines[24].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[24];
                            changeLine(richTextBox2, 24, goldplayerToReplace);
                            changeLine(richTextBox2, 14, whiteplayerToReplace);
                            team5g++; team3g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team6"))
                    {
                        if (!richTextBox2.Lines[28].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[28];
                            changeLine(richTextBox2, 28, goldplayerToReplace);
                            changeLine(richTextBox2, 14, whiteplayerToReplace);
                            team6g++; team3g--;
                        }
                        else if (!richTextBox2.Lines[29].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[29];
                            changeLine(richTextBox2, 29, goldplayerToReplace);
                            changeLine(richTextBox2, 14, whiteplayerToReplace);
                            team6g++; team3g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team7"))
                    {
                        if (!richTextBox2.Lines[33].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[33];
                            changeLine(richTextBox2, 33, goldplayerToReplace);
                            changeLine(richTextBox2, 14, whiteplayerToReplace);
                            team7g++; team3g--;
                        }
                        else if (!richTextBox2.Lines[34].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[34];
                            changeLine(richTextBox2, 34, goldplayerToReplace);
                            changeLine(richTextBox2, 14, whiteplayerToReplace);
                            team7g++; team3g--;
                        }
                    }
                }
            }
            else if (maxGoldTeamA.Contains("team4"))
            {
                if (richTextBox2.Lines[18].Contains("*"))
                {
                    goldplayerToReplace = richTextBox2.Lines[18];       //2nd Player of the Team with max Golds
                    /*
                    */
                    if (minGoldTeamA.Contains("team1"))
                    {
                        if (!richTextBox2.Lines[3].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[3];
                            changeLine(richTextBox2, 3, goldplayerToReplace);
                            changeLine(richTextBox2, 18, whiteplayerToReplace);
                            team1g++; team4g--;
                        }
                        else if (!richTextBox2.Lines[4].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[4];
                            changeLine(richTextBox2, 4, goldplayerToReplace);
                            changeLine(richTextBox2, 18, whiteplayerToReplace);
                            team1g++; team4g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team2"))
                    {
                        if (!richTextBox2.Lines[8].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[8];
                            changeLine(richTextBox2, 8, goldplayerToReplace);
                            changeLine(richTextBox2, 18, whiteplayerToReplace);
                            team2g++; team4g--;
                        }
                        else if (!richTextBox2.Lines[9].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[9];
                            changeLine(richTextBox2, 9, goldplayerToReplace);
                            changeLine(richTextBox2, 18, whiteplayerToReplace);
                            team2g++; team4g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team3"))
                    {
                        if (!richTextBox2.Lines[13].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[13];
                            changeLine(richTextBox2, 13, goldplayerToReplace);
                            changeLine(richTextBox2, 18, whiteplayerToReplace);
                            team3g++; team4g--;
                        }
                        else if (!richTextBox2.Lines[14].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[14];
                            changeLine(richTextBox2, 14, goldplayerToReplace);
                            changeLine(richTextBox2, 18, whiteplayerToReplace);
                            team3g++; team4g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team5"))
                    {
                        if (!richTextBox2.Lines[23].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[23];
                            changeLine(richTextBox2, 23, goldplayerToReplace);
                            changeLine(richTextBox2, 18, whiteplayerToReplace);
                            team5g++; team4g--;
                        }
                        else if (!richTextBox2.Lines[24].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[24];
                            changeLine(richTextBox2, 24, goldplayerToReplace);
                            changeLine(richTextBox2, 18, whiteplayerToReplace);
                            team5g++; team4g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team6"))
                    {
                        if (!richTextBox2.Lines[28].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[28];
                            changeLine(richTextBox2, 28, goldplayerToReplace);
                            changeLine(richTextBox2, 18, whiteplayerToReplace);
                            team6g++; team4g--;
                        }
                        else if (!richTextBox2.Lines[29].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[29];
                            changeLine(richTextBox2, 29, goldplayerToReplace);
                            changeLine(richTextBox2, 18, whiteplayerToReplace);
                            team6g++; team4g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team7"))
                    {
                        if (!richTextBox2.Lines[33].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[33];
                            changeLine(richTextBox2, 33, goldplayerToReplace);
                            changeLine(richTextBox2, 18, whiteplayerToReplace);
                            team7g++; team4g--;
                        }
                        else if (!richTextBox2.Lines[34].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[34];
                            changeLine(richTextBox2, 34, goldplayerToReplace);
                            changeLine(richTextBox2, 18, whiteplayerToReplace);
                            team7g++; team4g--;
                        }
                    }
                }
                else if (richTextBox2.Lines[19].Contains("*"))
                {
                    goldplayerToReplace = richTextBox2.Lines[19];       //3rd Player of the Team with max Golds
                    if (minGoldTeamA.Contains("team1"))
                    {
                        if (!richTextBox2.Lines[3].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[3];
                            changeLine(richTextBox2, 3, goldplayerToReplace);
                            changeLine(richTextBox2, 19, whiteplayerToReplace);
                            team1g++; team4g--;
                        }
                        else if (!richTextBox2.Lines[4].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[4];
                            changeLine(richTextBox2, 4, goldplayerToReplace);
                            changeLine(richTextBox2, 19, whiteplayerToReplace);
                            team1g++; team4g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team2"))
                    {
                        if (!richTextBox2.Lines[8].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[8];
                            changeLine(richTextBox2, 8, goldplayerToReplace);
                            changeLine(richTextBox2, 19, whiteplayerToReplace);
                            team2g++; team4g--;
                        }
                        else if (!richTextBox2.Lines[9].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[9];
                            changeLine(richTextBox2, 9, goldplayerToReplace);
                            changeLine(richTextBox2, 19, whiteplayerToReplace);
                            team2g++; team4g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team3"))
                    {
                        if (!richTextBox2.Lines[13].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[13];
                            changeLine(richTextBox2, 13, goldplayerToReplace);
                            changeLine(richTextBox2, 19, whiteplayerToReplace);
                            team3g++; team4g--;
                        }
                        else if (!richTextBox2.Lines[14].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[14];
                            changeLine(richTextBox2, 14, goldplayerToReplace);
                            changeLine(richTextBox2, 19, whiteplayerToReplace);
                            team3g++; team4g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team5"))
                    {
                        if (!richTextBox2.Lines[23].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[23];
                            changeLine(richTextBox2, 23, goldplayerToReplace);
                            changeLine(richTextBox2, 19, whiteplayerToReplace);
                            team5g++; team4g--;
                        }
                        else if (!richTextBox2.Lines[24].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[24];
                            changeLine(richTextBox2, 24, goldplayerToReplace);
                            changeLine(richTextBox2, 19, whiteplayerToReplace);
                            team5g++; team4g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team6"))
                    {
                        if (!richTextBox2.Lines[28].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[28];
                            changeLine(richTextBox2, 28, goldplayerToReplace);
                            changeLine(richTextBox2, 19, whiteplayerToReplace);
                            team6g++; team4g--;
                        }
                        else if (!richTextBox2.Lines[29].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[29];
                            changeLine(richTextBox2, 29, goldplayerToReplace);
                            changeLine(richTextBox2, 19, whiteplayerToReplace);
                            team6g++; team4g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team7"))
                    {
                        if (!richTextBox2.Lines[33].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[33];
                            changeLine(richTextBox2, 33, goldplayerToReplace);
                            changeLine(richTextBox2, 19, whiteplayerToReplace);
                            team7g++; team4g--;
                        }
                        else if (!richTextBox2.Lines[34].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[34];
                            changeLine(richTextBox2, 34, goldplayerToReplace);
                            changeLine(richTextBox2, 19, whiteplayerToReplace);
                            team7g++; team4g--;
                        }
                    }
                }
            }
            else if (maxGoldTeamA.Contains("team5"))
            {                
                if (richTextBox2.Lines[23].Contains("*"))
                {
                    goldplayerToReplace = richTextBox2.Lines[23];       //2nd Player of the Team with max Golds
                    
                    if (minGoldTeamA.Contains("team1"))
                    {
                        if (!richTextBox2.Lines[3].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[3];
                            changeLine(richTextBox2, 3, goldplayerToReplace);
                            changeLine(richTextBox2, 23, whiteplayerToReplace);
                            team1g++; team5g--;
                        }
                        else if (!richTextBox2.Lines[4].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[4];
                            changeLine(richTextBox2, 4, goldplayerToReplace);
                            changeLine(richTextBox2, 23, whiteplayerToReplace);
                            team1g++; team5g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team2"))
                    {
                        if (!richTextBox2.Lines[8].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[8];
                            changeLine(richTextBox2, 8, goldplayerToReplace);
                            changeLine(richTextBox2, 23, whiteplayerToReplace);
                            team2g++; team5g--;
                        }
                        else if (!richTextBox2.Lines[9].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[9];
                            changeLine(richTextBox2, 9, goldplayerToReplace);
                            changeLine(richTextBox2, 23, whiteplayerToReplace);
                            team2g++; team5g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team3"))
                    {
                        if (!richTextBox2.Lines[13].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[13];
                            changeLine(richTextBox2, 13, goldplayerToReplace);
                            changeLine(richTextBox2, 23, whiteplayerToReplace);
                            team3g++; team5g--;
                        }
                        else if (!richTextBox2.Lines[14].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[14];
                            changeLine(richTextBox2, 14, goldplayerToReplace);
                            changeLine(richTextBox2, 23, whiteplayerToReplace);
                            team3g++; team5g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team4"))
                    {
                        if (!richTextBox2.Lines[18].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[18];
                            changeLine(richTextBox2, 18, goldplayerToReplace);
                            changeLine(richTextBox2, 23, whiteplayerToReplace);
                            team4g++; team5g--;
                        }
                        else if (!richTextBox2.Lines[19].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[19];
                            changeLine(richTextBox2, 19, goldplayerToReplace);
                            changeLine(richTextBox2, 23, whiteplayerToReplace);
                            team4g++; team5g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team6"))
                    {
                        if (!richTextBox2.Lines[28].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[28];
                            changeLine(richTextBox2, 28, goldplayerToReplace);
                            changeLine(richTextBox2, 23, whiteplayerToReplace);
                            team6g++; team5g--;
                        }
                        else if (!richTextBox2.Lines[29].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[29];
                            changeLine(richTextBox2, 29, goldplayerToReplace);
                            changeLine(richTextBox2, 23, whiteplayerToReplace);
                            team6g++; team5g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team7"))
                    {
                        if (!richTextBox2.Lines[33].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[33];
                            changeLine(richTextBox2, 33, goldplayerToReplace);
                            changeLine(richTextBox2, 23, whiteplayerToReplace);
                            team7g++; team5g--;
                        }
                        else if (!richTextBox2.Lines[34].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[34];
                            changeLine(richTextBox2, 34, goldplayerToReplace);
                            changeLine(richTextBox2, 23, whiteplayerToReplace);
                            team7g++; team5g--;
                        }
                    }
                }
                else if (richTextBox2.Lines[24].Contains("*"))
                {
                    goldplayerToReplace = richTextBox2.Lines[24];       //3rd Player of MaxGoldTeam
                    if (minGoldTeamA.Contains("team1"))
                    {
                        if (!richTextBox2.Lines[3].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[3];
                            changeLine(richTextBox2, 3, goldplayerToReplace);
                            changeLine(richTextBox2, 24, whiteplayerToReplace);
                            team1g++; team5g--;
                        }
                        else if (!richTextBox2.Lines[4].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[4];
                            changeLine(richTextBox2, 4, goldplayerToReplace);
                            changeLine(richTextBox2, 24, whiteplayerToReplace);
                            team1g++; team5g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team2"))
                    {
                        if (!richTextBox2.Lines[8].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[8];
                            changeLine(richTextBox2, 8, goldplayerToReplace);
                            changeLine(richTextBox2, 24, whiteplayerToReplace);
                            team2g++; team5g--;
                        }
                        else if (!richTextBox2.Lines[9].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[9];
                            changeLine(richTextBox2, 9, goldplayerToReplace);
                            changeLine(richTextBox2, 24, whiteplayerToReplace);
                            team2g++; team5g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team3"))
                    {
                        if (!richTextBox2.Lines[13].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[13];
                            changeLine(richTextBox2, 13, goldplayerToReplace);
                            changeLine(richTextBox2, 24, whiteplayerToReplace);
                            team3g++; team5g--;
                        }
                        else if (!richTextBox2.Lines[14].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[14];
                            changeLine(richTextBox2, 14, goldplayerToReplace);
                            changeLine(richTextBox2, 24, whiteplayerToReplace);
                            team3g++; team5g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team4"))
                    {
                        if (!richTextBox2.Lines[18].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[18];
                            changeLine(richTextBox2, 18, goldplayerToReplace);
                            changeLine(richTextBox2, 24, whiteplayerToReplace);
                            team4g++; team5g--;
                        }
                        else if (!richTextBox2.Lines[19].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[19];
                            changeLine(richTextBox2, 19, goldplayerToReplace);
                            changeLine(richTextBox2, 24, whiteplayerToReplace);
                            team4g++; team5g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team6"))
                    {
                        if (!richTextBox2.Lines[28].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[28];
                            changeLine(richTextBox2, 28, goldplayerToReplace);
                            changeLine(richTextBox2, 24, whiteplayerToReplace);
                            team6g++; team5g--;
                        }
                        else if (!richTextBox2.Lines[29].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[29];
                            changeLine(richTextBox2, 29, goldplayerToReplace);
                            changeLine(richTextBox2, 24, whiteplayerToReplace);
                            team6g++; team5g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team7"))
                    {
                        if (!richTextBox2.Lines[33].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[33];
                            changeLine(richTextBox2, 33, goldplayerToReplace);
                            changeLine(richTextBox2, 24, whiteplayerToReplace);
                            team7g++; team5g--;
                        }
                        else if (!richTextBox2.Lines[34].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[34];
                            changeLine(richTextBox2, 34, goldplayerToReplace);
                            changeLine(richTextBox2, 24, whiteplayerToReplace);
                            team7g++; team5g--;
                        }
                    }

                }
            }
            else if (maxGoldTeamA.Contains("team6"))
            {
                if (richTextBox2.Lines[28].Contains("*"))
                {
                    goldplayerToReplace = richTextBox2.Lines[28];       //2nd Player of the Team with max Golds

                    if (minGoldTeamA.Contains("team1"))
                    {
                        if (!richTextBox2.Lines[3].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[3];
                            changeLine(richTextBox2, 3, goldplayerToReplace);
                            changeLine(richTextBox2, 28, whiteplayerToReplace);
                            team1g++; team6g--;
                        }
                        else if (!richTextBox2.Lines[4].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[4];
                            changeLine(richTextBox2, 4, goldplayerToReplace);
                            changeLine(richTextBox2, 28, whiteplayerToReplace);
                            team1g++; team6g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team2"))
                    {
                        if (!richTextBox2.Lines[8].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[8];
                            changeLine(richTextBox2, 8, goldplayerToReplace);
                            changeLine(richTextBox2, 28, whiteplayerToReplace);
                            team2g++; team6g--;
                        }
                        else if (!richTextBox2.Lines[9].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[9];
                            changeLine(richTextBox2, 9, goldplayerToReplace);
                            changeLine(richTextBox2, 28, whiteplayerToReplace);
                            team2g++; team6g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team3"))
                    {
                        if (!richTextBox2.Lines[13].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[13];
                            changeLine(richTextBox2, 13, goldplayerToReplace);
                            changeLine(richTextBox2, 23, whiteplayerToReplace);
                            team3g++; team6g--;
                        }
                        else if (!richTextBox2.Lines[14].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[14];
                            changeLine(richTextBox2, 14, goldplayerToReplace);
                            changeLine(richTextBox2, 28, whiteplayerToReplace);
                            team3g++; team6g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team4"))
                    {
                        if (!richTextBox2.Lines[18].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[18];
                            changeLine(richTextBox2, 18, goldplayerToReplace);
                            changeLine(richTextBox2, 23, whiteplayerToReplace);
                            team4g++; team6g--;
                        }
                        else if (!richTextBox2.Lines[19].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[19];
                            changeLine(richTextBox2, 19, goldplayerToReplace);
                            changeLine(richTextBox2, 28, whiteplayerToReplace);
                            team4g++; team6g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team5"))
                    {
                        if (!richTextBox2.Lines[23].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[23];
                            changeLine(richTextBox2, 23, goldplayerToReplace);
                            changeLine(richTextBox2, 28, whiteplayerToReplace);
                            team5g++; team6g--;
                        }
                        else if (!richTextBox2.Lines[24].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[24];
                            changeLine(richTextBox2, 24, goldplayerToReplace);
                            changeLine(richTextBox2, 28, whiteplayerToReplace);
                            team5g++; team6g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team7"))
                    {
                        if (!richTextBox2.Lines[33].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[33];
                            changeLine(richTextBox2, 33, goldplayerToReplace);
                            changeLine(richTextBox2, 28, whiteplayerToReplace);
                            team7g++; team6g--;
                        }
                        else if (!richTextBox2.Lines[34].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[34];
                            changeLine(richTextBox2, 34, goldplayerToReplace);
                            changeLine(richTextBox2, 28, whiteplayerToReplace);
                            team7g++; team6g--;
                        }
                    }
                }
                else if (richTextBox2.Lines[29].Contains("*"))
                {
                    goldplayerToReplace = richTextBox2.Lines[29];       //3rd Player of the Team with max Golds
                    if (minGoldTeamA.Contains("team1"))
                    {
                        if (!richTextBox2.Lines[3].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[3];
                            changeLine(richTextBox2, 3, goldplayerToReplace);
                            changeLine(richTextBox2, 29, whiteplayerToReplace);
                            team1g++; team6g--;
                        }
                        else if (!richTextBox2.Lines[4].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[4];
                            changeLine(richTextBox2, 4, goldplayerToReplace);
                            changeLine(richTextBox2, 29, whiteplayerToReplace);
                            team1g++; team6g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team2"))
                    {
                        if (!richTextBox2.Lines[8].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[8];
                            changeLine(richTextBox2, 8, goldplayerToReplace);
                            changeLine(richTextBox2, 29, whiteplayerToReplace);
                            team2g++; team6g--;
                        }
                        else if (!richTextBox2.Lines[9].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[9];
                            changeLine(richTextBox2, 9, goldplayerToReplace);
                            changeLine(richTextBox2, 29, whiteplayerToReplace);
                            team2g++; team6g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team3"))
                    {
                        if (!richTextBox2.Lines[13].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[13];
                            changeLine(richTextBox2, 13, goldplayerToReplace);
                            changeLine(richTextBox2, 23, whiteplayerToReplace);
                            team3g++; team6g--;
                        }
                        else if (!richTextBox2.Lines[14].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[14];
                            changeLine(richTextBox2, 14, goldplayerToReplace);
                            changeLine(richTextBox2, 29, whiteplayerToReplace);
                            team3g++; team6g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team4"))
                    {
                        if (!richTextBox2.Lines[18].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[18];
                            changeLine(richTextBox2, 18, goldplayerToReplace);
                            changeLine(richTextBox2, 23, whiteplayerToReplace);
                            team4g++; team6g--;
                        }
                        else if (!richTextBox2.Lines[19].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[19];
                            changeLine(richTextBox2, 19, goldplayerToReplace);
                            changeLine(richTextBox2, 29, whiteplayerToReplace);
                            team4g++; team6g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team5"))
                    {
                        if (!richTextBox2.Lines[23].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[23];
                            changeLine(richTextBox2, 23, goldplayerToReplace);
                            changeLine(richTextBox2, 29, whiteplayerToReplace);
                            team5g++; team6g--;
                        }
                        else if (!richTextBox2.Lines[24].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[24];
                            changeLine(richTextBox2, 24, goldplayerToReplace);
                            changeLine(richTextBox2, 29, whiteplayerToReplace);
                            team5g++; team6g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team7"))
                    {
                        if (!richTextBox2.Lines[33].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[33];
                            changeLine(richTextBox2, 33, goldplayerToReplace);
                            changeLine(richTextBox2, 29, whiteplayerToReplace);
                            team7g++; team6g--;
                        }
                        else if (!richTextBox2.Lines[34].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[34];
                            changeLine(richTextBox2, 34, goldplayerToReplace);
                            changeLine(richTextBox2, 29, whiteplayerToReplace);
                            team7g++; team6g--;
                        }
                    }
                }
            }
            else if (maxGoldTeamA.Contains("team7"))
            {
                //2nd Player of the Team with max Golds
                if (richTextBox2.Lines[33].Contains("*"))
                {
                    goldplayerToReplace = richTextBox2.Lines[33];
                    /*
                    */
                    if (minGoldTeamA.Contains("team1"))
                    {
                        if (!richTextBox2.Lines[3].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[3];
                            changeLine(richTextBox2, 3, goldplayerToReplace);
                            changeLine(richTextBox2, 33, whiteplayerToReplace);
                            team1g++; team7g--;
                        }
                        else if (!richTextBox2.Lines[4].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[4];
                            changeLine(richTextBox2, 4, goldplayerToReplace);
                            changeLine(richTextBox2, 33, whiteplayerToReplace);
                            team1g++; team7g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team2"))
                    {
                        if (!richTextBox2.Lines[8].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[8];
                            changeLine(richTextBox2, 8, goldplayerToReplace);
                            changeLine(richTextBox2, 33, whiteplayerToReplace);
                            team2g++; team7g--;
                        }
                        else if (!richTextBox2.Lines[9].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[9];
                            changeLine(richTextBox2, 9, goldplayerToReplace);
                            changeLine(richTextBox2, 33, whiteplayerToReplace);
                            team2g++; team7g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team3"))
                    {
                        if (!richTextBox2.Lines[13].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[13];
                            changeLine(richTextBox2, 13, goldplayerToReplace);
                            changeLine(richTextBox2, 33, whiteplayerToReplace);
                            team3g++; team7g--;
                        }
                        else if (!richTextBox2.Lines[14].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[14];
                            changeLine(richTextBox2, 14, goldplayerToReplace);
                            changeLine(richTextBox2, 33, whiteplayerToReplace);
                            team3g++; team7g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team4"))
                    {
                        if (!richTextBox2.Lines[18].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[18];
                            changeLine(richTextBox2, 18, goldplayerToReplace);
                            changeLine(richTextBox2, 33, whiteplayerToReplace);
                            team4g++; team7g--;
                        }
                        else if (!richTextBox2.Lines[19].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[19];
                            changeLine(richTextBox2, 19, goldplayerToReplace);
                            changeLine(richTextBox2, 33, whiteplayerToReplace);
                            team4g++; team7g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team5"))
                    {
                        if (!richTextBox2.Lines[23].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[23];
                            changeLine(richTextBox2, 23, goldplayerToReplace);
                            changeLine(richTextBox2, 33, whiteplayerToReplace);
                            team5g++; team7g--;
                        }
                        else if (!richTextBox2.Lines[24].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[24];
                            changeLine(richTextBox2, 24, goldplayerToReplace);
                            changeLine(richTextBox2, 33, whiteplayerToReplace);
                            team5g++; team7g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team6"))
                    {
                        if (!richTextBox2.Lines[28].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[28];
                            changeLine(richTextBox2, 28, goldplayerToReplace);
                            changeLine(richTextBox2, 33, whiteplayerToReplace);
                            team6g++; team7g--;
                        }
                        else if (!richTextBox2.Lines[29].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[29];
                            changeLine(richTextBox2, 29, goldplayerToReplace);
                            changeLine(richTextBox2, 33, whiteplayerToReplace);
                            team6g++; team7g--;
                        }
                    }
                }
                else if (richTextBox2.Lines[34].Contains("*"))
                {
                    goldplayerToReplace = richTextBox2.Lines[34];
                    if (minGoldTeamA.Contains("team1"))
                    {
                        if (!richTextBox2.Lines[3].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[3];
                            changeLine(richTextBox2, 3, goldplayerToReplace);
                            changeLine(richTextBox2, 33, whiteplayerToReplace);
                            team1g++; team7g--;
                        }
                        else if (!richTextBox2.Lines[4].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[4];
                            changeLine(richTextBox2, 4, goldplayerToReplace);
                            changeLine(richTextBox2, 34, whiteplayerToReplace);
                            team1g++; team7g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team2"))
                    {
                        if (!richTextBox2.Lines[8].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[8];
                            changeLine(richTextBox2, 8, goldplayerToReplace);
                            changeLine(richTextBox2, 34, whiteplayerToReplace);
                            team2g++; team7g--;
                        }
                        else if (!richTextBox2.Lines[9].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[9];
                            changeLine(richTextBox2, 9, goldplayerToReplace);
                            changeLine(richTextBox2, 34, whiteplayerToReplace);
                            team2g++; team7g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team3"))
                    {
                        if (!richTextBox2.Lines[13].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[13];
                            changeLine(richTextBox2, 13, goldplayerToReplace);
                            changeLine(richTextBox2, 34, whiteplayerToReplace);
                            team3g++; team7g--;
                        }
                        else if (!richTextBox2.Lines[14].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[14];
                            changeLine(richTextBox2, 14, goldplayerToReplace);
                            changeLine(richTextBox2, 34, whiteplayerToReplace);
                            team3g++; team7g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team4"))
                    {
                        if (!richTextBox2.Lines[18].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[18];
                            changeLine(richTextBox2, 18, goldplayerToReplace);
                            changeLine(richTextBox2, 34, whiteplayerToReplace);
                            team4g++; team7g--;
                        }
                        else if (!richTextBox2.Lines[19].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[19];
                            changeLine(richTextBox2, 19, goldplayerToReplace);
                            changeLine(richTextBox2, 34, whiteplayerToReplace);
                            team4g++; team7g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team5"))
                    {
                        if (!richTextBox2.Lines[23].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[23];
                            changeLine(richTextBox2, 23, goldplayerToReplace);
                            changeLine(richTextBox2, 34, whiteplayerToReplace);
                            team5g++; team7g--;
                        }
                        else if (!richTextBox2.Lines[24].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[24];
                            changeLine(richTextBox2, 24, goldplayerToReplace);
                            changeLine(richTextBox2, 34, whiteplayerToReplace);
                            team5g++; team7g--;
                        }
                    }
                    else if (minGoldTeamA.Contains("team6"))
                    {
                        if (!richTextBox2.Lines[28].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[28];
                            changeLine(richTextBox2, 28, goldplayerToReplace);
                            changeLine(richTextBox2, 34, whiteplayerToReplace);
                            team6g++; team7g--;
                        }
                        else if (!richTextBox2.Lines[29].Contains("*") && goldplayerToReplace != null)
                        {
                            whiteplayerToReplace = richTextBox2.Lines[29];
                            changeLine(richTextBox2, 29, goldplayerToReplace);
                            changeLine(richTextBox2, 34, whiteplayerToReplace);
                            team6g++; team7g--;
                        }
                    }
                }
            }
            // Refresh the gold golfers Array with updated values now that the players have been swapped once
            goldGolfers[0] = team1g; goldGolfers[1] = team2g; goldGolfers[2] = team3g;
            goldGolfers[3] = team4g; goldGolfers[4] = team5g; goldGolfers[5] = team6g;
            goldGolfers[6] = team7g;
        }

        private Int32 calcTeamHcp (string rtbLine)
        {
            int loc = rtbLine.IndexOf(" ");
            string hcp = rtbLine.Substring(0, loc);

            string revSignOfValue = hcp.Replace("+", "-");

            Int32 plusHCP = Convert.ToInt32(revSignOfValue);

            return plusHCP;            
        }
        private void determineTeamsToSwap(int cntOfTeam)
        {
            // Re-Initial the min/max GoldTeams
            minGoldTeamA = null; minGoldTeamB = null; minGoldTeamC = null; maxGoldTeamA = null; maxGoldTeamB = null; maxGoldTeamC = null;

            // Look at the 4-man Teams
            for (int i = 0; i < cntOfTeam; i++)
            {
                // MAX MAX MAX MAX MAX MAX MAX MAX MAX MAX MAX MAX MAX MAX MAX MAX MAX MAX
                
                //By checking for null, the 1st Team that has all 4 players playing from the Gold Tees is set to maxGoldA
                if (goldGolfers[i] == 4)
                {
                    if (maxGoldTeamA == null)
                        maxGoldTeamA = "team" + (1 + i).ToString() + "= Four";
                    else
                        maxGoldTeamB = maxGoldTeamA;
                }
                //By checking for null, the 1st Team that has 3 players playing from the Gold Tees is set to maxGoldB
                else if (goldGolfers[i] == 3)
                {
                    if (maxGoldTeamB == null)
                        maxGoldTeamB = "team" + (1 + i).ToString() + "= Three";
                    else
                        maxGoldTeamC = maxGoldTeamB;
                }
                //By checking for null, the 1st Team that has 2 players playing from the Gold Tees is set to maxGoldC
                else if (goldGolfers[i] == 2)
                {
                    if (maxGoldTeamC == null) 
                        maxGoldTeamC = "team" + (1 + i).ToString() + "= Two";
                }                

                // MIN MIN MIN MIN MIN MIN MIN MIN MIN MIN MIN MIN MIN MIN MIN MIN MIN MIN 
                //Find the 1st Team that has 0 player playing from the Gold Tees
                else if (goldGolfers[i] == 0)
                {
                    if (minGoldTeamA == null)
                        minGoldTeamA = "team" + (1 + i).ToString() + "= Zero";
                    else
                        minGoldTeamB = minGoldTeamA;
                }

                //Find the 1st Team that has 1 player playing from the Gold Tees
                else if (goldGolfers[i] == 1)
                {
                    if (minGoldTeamB == null)
                        minGoldTeamB = "team" + (1 + i).ToString() + "= One";
                    else
                        minGoldTeamC = minGoldTeamB;
                }                
            }
            // If there isn't a team with 4 Golds pick promote the other teams with 3 or 2
            if (maxGoldTeamA == null && maxGoldTeamB != null)
            {
                maxGoldTeamA = maxGoldTeamB;
                maxGoldTeamB = maxGoldTeamC;
            }
            if (maxGoldTeamA == null && maxGoldTeamB == null && maxGoldTeamC != null)
            {
                maxGoldTeamA = maxGoldTeamC;
            }

            // Check if there is a team with zero golds
            if (minGoldTeamA == null)
            {
                // If there isn't a team with zero Golds use a team with just one Gold player
                minGoldTeamA = minGoldTeamB;
            }
            
        }
        
    }
    public static class ListControlExtensions
    {
        public static object GetItemValue(this ListControl list, object item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            if (string.IsNullOrEmpty(list.ValueMember))
                return item;

            var property = TypeDescriptor.GetProperties(item)[list.ValueMember];
            if (property == null)
                throw new ArgumentException(
                    string.Format("item doesn't contain '{0}' property or column.",
                    list.ValueMember));
            return property.GetValue(item);
        }
    }
    static class Utility
    {
        public static void HighlightText(this RichTextBox myRtb, string word, Color color)
        {

            if (word == string.Empty)
                return;

            int s_start = myRtb.SelectionStart, startIndex = 0, index;

            while ((index = myRtb.Text.IndexOf(word, startIndex)) != -1)
            {
                myRtb.Select(index, word.Length);
                myRtb.SelectionColor = color;

                startIndex = index + word.Length;
            }

            myRtb.SelectionStart = s_start;
            myRtb.SelectionLength = 0;
            myRtb.SelectionColor = Color.Black;
        }
    }
}
/*
/// <summary>
/// I experimented with coloring the first person that starts with A, then B, etc. But the Draw mode is too slow in updating the listbox as the user scolls thru the list of names
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>
private void ListBox1_DrawItem(object sender, System.Windows.Forms.DrawItemEventArgs e)
{
    // Draw the background of the ListBox control for each item.
    e.DrawBackground();
    // Define the default color of the brush as black.
    Brush myBrush = Brushes.Black;

    // Determine the color of the brush to draw each item based 
    // on the index of the item to draw.
    switch (e.Index)
    {
        case 0:
            myBrush = Brushes.Red;
            break;
        case 1:
            myBrush = Brushes.Orange;
            break;
        case 2:
            myBrush = Brushes.Purple;
            break;
    }

    // Draw the current item text based on the current Font 
    // and the custom brush settings.
    e.Graphics.DrawString(listBox1.Items[e.Index].ToString(),
        e.Font, myBrush, e.Bounds, StringFormat.GenericDefault);
    // If the ListBox has focus, draw a focus rectangle around the selected item.
    e.DrawFocusRectangle();
}
*/
