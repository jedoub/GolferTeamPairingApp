using System;
using System.Windows.Forms;

namespace BlueTeeApp
{
    public partial class EnterSubnetForm : Form
    {
        public string guestPlayerID = null;

        public EnterSubnetForm()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            // Assign the Subnet NAME to be seen in the GUI when there isn't a CONFIG FILE then paste it into the CONFIG FILE to be used repeatedly there after
            guestPlayerID = textBox1.Text.ToUpper();

            textBox1.Clear();
            
            this.DialogResult = DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            TypingDelayTimer.Stop();            

            this.DialogResult = DialogResult.Cancel;
        }

        private void TypingDelayTimer_Tick(object sender, EventArgs e)
        {
            //When the Delay timer expires the Timer Tick is "called" by the OS.
            //Stop the Timer from running again, and execute the function below.
            //TypingDelayTimer.Enabled = false;
            TypingDelayTimer.Stop();

            this.ActiveControl = btnOK;

            btnOK.Visible = true;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // Upon Each Character typed the Timer is stopped and started
            //TypingDelayTimer.Enabled = false;
            TypingDelayTimer.Stop();

            if (textBox1.Text != "")
                //TypingDelayTimer.Enabled = true;
                TypingDelayTimer.Start();
        }

        private void EnterSubnetForm_Load(object sender, EventArgs e)
        {
            // Set theActiveControl property of the form to be the textbox1 component.
            this.ActiveControl = textBox1;
            // Initially, the button is not visible.
            btnOK.Visible = false;
            
            label1.Text = "Enter the player's Name:";
        }
    }
}
