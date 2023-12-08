﻿using System.Windows.Forms;
using DTO;
using GUI.Handle;

namespace GUI
{
    public partial class FormPDetails : Form
    {
        public FormPDetails(CateProduct product)
        {
            pd = product;
            InitializeComponent();
        }

        private void FormPDetails_MouseDown(object sender, MouseEventArgs e)
        {
            HandleGUI.ReleaseCapture();
            HandleGUI.SendMessage(Handle, 0x112, 0xf012, 8);
        }
        private void exit_Click(object sender, System.EventArgs e)
        {
            Close();
        }
    }
}
