using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    static class Program
    {
        public static List<string> filesToOpen = new List<string>();

        [STAThread]
        static void Main(String [] args)
        {           
            foreach (string s in args)
            {
                if (Directory.Exists(Path.GetDirectoryName(s)))
                {
                    filesToOpen.Add(s);
                }

            }
            bool ok;
            Mutex m = new Mutex(true, "Devil Player", out ok);
            if (!ok)
            {
                 // MessageBox.Show("برنامه در حاله اجرا است!!");
                return;
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainFrm());
        }
    }
}
