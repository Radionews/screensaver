/*
 * ScreenSaverForm.cs
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
using System.Runtime.InteropServices;
using System.IO;
using Microsoft.Win32;

namespace ScreenSaver
{
    public partial class ScreenSaverForm : Form
    {
        #region Win32 API functions

        [DllImport("user32.dll")]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern bool GetClientRect(IntPtr hWnd, out Rectangle lpRect);

        #endregion


        private Point mouseLocation;
        private bool previewMode = false;
        private Random rand = new Random();
        private int shift = 20;
        private string sTime;
        private bool flag = false;
        Color wordColor = Color.FromArgb(0x15,0,0,0xf0);
        System.Drawing.Text.PrivateFontCollection myfont = new System.Drawing.Text.PrivateFontCollection();
        public ScreenSaverForm()
        {
            InitializeComponent();
        }

        public ScreenSaverForm(Rectangle Bounds)
        {
            InitializeComponent();
            this.Bounds = Bounds;
        }

        public ScreenSaverForm(IntPtr PreviewWndHandle)
        {
            InitializeComponent();

            // Set the preview window as the parent of this window
            SetParent(this.Handle, PreviewWndHandle);

            // Make this a child window so it will close when the parent dialog closes
            SetWindowLong(this.Handle, -16, new IntPtr(GetWindowLong(this.Handle, -16) | 0x40000000));

            // Place our window inside the parent
            Rectangle ParentRect;
            GetClientRect(PreviewWndHandle, out ParentRect);
            Size = ParentRect.Size;
            Location = new Point(0, 0);

            // Make text smaller
            textLabel.Font = new System.Drawing.Font("Arial", 6);

            previewMode = true;
        }

        private void ScreenSaverForm_Load(object sender, EventArgs e)
        {
            using (MemoryStream fontStream = new MemoryStream(Properties.Resources.a_lcdnova))
            {
                // create an unsafe memory block for the font data
                System.IntPtr data = Marshal.AllocCoTaskMem((int)fontStream.Length);
                // create a buffer to read in to
                byte[] fontdata = new byte[fontStream.Length];
                // read the font data from the resource
                fontStream.Read(fontdata, 0, (int)fontStream.Length);
                // copy the bytes to the unsafe memory block
                Marshal.Copy(fontdata, 0, data, (int)fontStream.Length);
                // pass the font to the font collection
                myfont.AddMemoryFont(data, (int)fontStream.Length);
                // close the resource stream
                fontStream.Close();
                // free the unsafe memory
                Marshal.FreeCoTaskMem(data);

            }
            LoadSettings();
            //myfont.AddFontFile("a_lcdnova.ttf");
            Cursor.Hide();            
            TopMost = true;

            moveTimer.Interval = 1;
            moveTimer.Tick += new EventHandler(moveTimer_Tick);
            moveTimer.Start();
        }

        private void moveTimer_Tick(object sender, System.EventArgs e)
        {
            System.Drawing.Graphics formGraphics = this.CreateGraphics();
            System.Drawing.StringFormat drawFormat = new System.Drawing.StringFormat();
            sTime = DateTime.Now.Hour.ToString().PadLeft(2, '0') + " : " + DateTime.Now.Minute.ToString().PadLeft(2, '0');
            if (sTime == "00 : 00")
            {
                Font drawTime = new Font(myfont.Families[0], 100);
                SolidBrush drawBTime = new SolidBrush(wordColor);
                Size len = TextRenderer.MeasureText("SHOW MUST GO ON", drawTime);
                formGraphics.DrawString("SHOW MUST GO ON", drawTime, drawBTime, (SystemInformation.PrimaryMonitorSize.Width / 2) - len.Width / 2, (SystemInformation.PrimaryMonitorSize.Height / 2) - len.Height / 2, drawFormat);

            }
            else
            {
                System.Drawing.Font drawTime = new System.Drawing.Font(myfont.Families[0], 300);
                SolidBrush drawBTime = new SolidBrush(wordColor);
                Size len = TextRenderer.MeasureText(sTime, drawTime);
                formGraphics.DrawString(sTime, drawTime, drawBTime, (SystemInformation.PrimaryMonitorSize.Width / 2) - len.Width / 2, (SystemInformation.PrimaryMonitorSize.Height / 2) - len.Height / 2, drawFormat);
            }

            moveTimer.Enabled = false;
            for (int i = 0; i < 500; i++)
            {
                int x = rand.Next(Math.Max(0, SystemInformation.PrimaryMonitorSize.Width));
                int y = rand.Next(Math.Max(0, SystemInformation.PrimaryMonitorSize.Height));
                
                System.Drawing.Font drawFont = new System.Drawing.Font("Arial", 14);


                SolidBrush drawBrush = new SolidBrush(Color.FromArgb((rand.Next(Math.Max(32, 126)) << 24) | (wordColor.ToArgb()&0x00FFFFFF)));


                SolidBrush blueBrush = new SolidBrush(Color.Black);
                Rectangle rect = new Rectangle(x, y, shift-1, shift-1);
                formGraphics.FillRectangle(blueBrush, rect);
                formGraphics.DrawString(((char)rand.Next(Math.Max(32, 126))).ToString(), drawFont, drawBrush, x, y, drawFormat);
                
                blueBrush.Dispose();
                drawFont.Dispose();
                drawBrush.Dispose();

                
                
            }
            formGraphics.Dispose();
            moveTimer.Enabled = true;
            
        }

        private void LoadSettings()
        {
            // Use the string from the Registry if it exists
            RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\ScreenSaver_RADIONEWS");
            if (key == null)
                wordColor = Color.FromArgb(0x15, 0, 0, 0xf0);
            else
                wordColor = Color.FromArgb((int)key.GetValue("color"));
        }

        private void ScreenSaverForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (!previewMode)
            {
                if (!mouseLocation.IsEmpty)
                {
                    // Terminate if mouse is moved a significant distance
                    if (Math.Abs(mouseLocation.X - e.X) > 5 ||
                        Math.Abs(mouseLocation.Y - e.Y) > 5)
                        Application.Exit();
                }

                // Update current mouse location
                mouseLocation = e.Location;
            }
        }

        private void ScreenSaverForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!previewMode)
                Application.Exit();
        }

        private void ScreenSaverForm_MouseClick(object sender, MouseEventArgs e)
        {
            if (!previewMode)
                Application.Exit();
        }
    }
}
