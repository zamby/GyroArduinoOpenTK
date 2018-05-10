using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace GyroArduinoOpenTK
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

            ISerialData serialdata;
            if (args.Length == 2)
                serialdata = new SerialData(args[0], Convert.ToInt32(args[1]));
            else
                serialdata = new EmuSerialData();

            Application.Run(new Form1(serialdata));
        }
    }
}
