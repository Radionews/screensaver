/*
 * SettingsForm.cs
 * By Frank McCown
 * Summer 2010
 * 
 * Feel free to modify this code.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Security.Permissions;

namespace ScreenSaver
{
    public partial class SettingsForm : Form
    {
        private Color wordColor;
        public SettingsForm()
        {
            InitializeComponent();
            LoadSettings();
        }

        /// <summary>
        /// Load display text from the Registry
        /// </summary>
        private void LoadSettings()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\ScreenSaver_RADIONEWS");
            if (key == null)
            {
                wordColor = Color.FromArgb(0x15, 0, 0, 0xF0);
                pictureBox1.BackColor = Color.FromArgb(0xff, wordColor.R, wordColor.G, wordColor.B);
            }
            else
            {
                wordColor = Color.FromArgb((int)key.GetValue("color"));
                pictureBox1.BackColor = Color.FromArgb(0xff, wordColor.R, wordColor.G, wordColor.B);
            }

        }

        /// <summary>
        /// Save text into the Registry.
        /// </summary>
        private void SaveSettings()
        {
            // Create or get existing subkey
            RegistryKey key = Registry.CurrentUser.CreateSubKey("SOFTWARE\\ScreenSaver_RADIONEWS");

            key.SetValue("color", (int)wordColor.ToArgb());
        }

        private void okButton_Click(object sender, EventArgs e)
        {

            SaveSettings();
            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                wordColor = Color.FromArgb((colorDialog1.Color.ToArgb() & 0x00FFFFFF) | 0x15000000);
                pictureBox1.BackColor = colorDialog1.Color;
            }
        }
    }
}
