using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Teechip.View;
using Teechip.Controller;

namespace Teechip
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
		static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.ApplicationExit += new EventHandler(Application_ApplicationExit);

            Application.Run(new frmURLShortener());
        }

        static void Application_ApplicationExit(object sender, EventArgs e)
        {

        }
    }
}
