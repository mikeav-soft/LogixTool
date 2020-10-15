using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace LogixTool
{
    public partial class FormAbout : Form
    {
        public FormAbout()
        {
            InitializeComponent();
            string content = "";
            
            try
            {
                content = Properties.Resources.txt_AppRegistratorRelease;
            }
            catch
            {
                content = "ERROR OF READING DATA.";
            }

            this.richTextBox.Text = content;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_Close_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
