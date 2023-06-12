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
    public partial class FormTeeSelc : Form
    {
        static public int playersTeeBox = 0;    //WHT = 1, GOLD = 2, RED = 3, GRN = 4
        public FormTeeSelc()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            playersTeeBox = 1;
            this.DialogResult = DialogResult.OK;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            playersTeeBox = 2;
            this.DialogResult = DialogResult.OK;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            playersTeeBox = 3;
            this.DialogResult = DialogResult.OK;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            playersTeeBox = 4;
            this.DialogResult = DialogResult.OK;
        }

        private void FormTeeSelc_Load(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
    }
}
