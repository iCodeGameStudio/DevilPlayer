using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevilPlayer;
using System.IO;
using WindowsFormsApp1;

namespace DevilPlayer
{
    public partial class FindFrm : Form
    {
        public List<String> list_music_path2 = new List<String>();
        public FindFrm()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                listBox1.Items.Clear();
                listBox2.Items.Clear();
                for (int i = 0; i < list_music_path2.Count; i++)
                {
                    if (Path.GetFileName(list_music_path2[i]).ToLower().Contains(textBox1.Text.ToLower()))
                    {
                        listBox1.Items.Add(Path.GetFileName(list_music_path2[i]));
                        listBox2.Items.Add(list_music_path2[i]);
                    }
                }
            }
            catch
            {

            }

        }

        private void FindFrm_Load(object sender, EventArgs e)
        {
            try
            {
                foreach (string myfile in list_music_path2)
                {
                    listBox1.Items.Add(Path.GetFileName(myfile));
                    listBox2.Items.Add(myfile);
                }
            }
            catch
            {

            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                listBox2.SelectedIndex = listBox1.SelectedIndex;
            }
            catch
            {

            }
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                MainFrm f1 = (MainFrm)Owner;
                for (int i = 0; i <= list_music_path2.Count; i++)
                {
                    if (f1.list_music_path[i] == listBox2.Text)
                    {
                        f1.list_music_path.ElementAt(i);
                        f1.listBox1.SelectedIndex = i;
                        this.Close();
                    }
                }
                
            }
            catch
            {

            }
        }
    }
}
