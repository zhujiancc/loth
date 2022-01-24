using CefSharp;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace loth.cef
{
    public partial class Form1 : Form
    {
        private readonly ChromiumWebBrowser browser;

        public Form1()
        {
            InitializeComponent();

            WindowState = FormWindowState.Maximized;

            browser = new ChromiumWebBrowser("https://www.cnblogs.com/");

            this.Controls.Add(browser);
        }
    }
}
