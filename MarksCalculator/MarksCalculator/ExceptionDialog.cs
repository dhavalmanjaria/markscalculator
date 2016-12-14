using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MarksCalculator
{
    public partial class ExceptionDialog : Form
    {
        private Boolean heightClick = true;
        public ExceptionDialog()
        {
            InitializeComponent();
        }

        public ExceptionDialog(string ErrorName, string ErrorMessage = "An unknown error occured.")
        {
            InitializeComponent();
            this.Text = "Critical Error";
            rtbErrorHeading.Text = ErrorName;
            rtbErrorText.Text = ErrorMessage;
        }

        private void ExceptionDialog_Load(object sender, EventArgs e)
        {
            
            System.Drawing.Icon icon = System.Drawing.SystemIcons.Error;
            pictureBox1.Image = icon.ToBitmap();
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
        }

        private void btnDetails_Click(object sender, EventArgs e)
        {
            if (heightClick)
                this.Height += 150;
            else
                this.Height -= 150;

            heightClick = !heightClick;
        }
    }
}
