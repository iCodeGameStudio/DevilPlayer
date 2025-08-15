using DevilPlayer;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{

    public partial class MainFrm : Form
    {
        int x, y;
        const int WM_SYSCOMMAND = 0x0112;
        const int SC_MOVE = 0xF010;
        private NAudio.Wave.BlockAlignReductionStream stream = null;
        private AudioFileReader audioFileReader;
        private NAudio.Wave.DirectSoundOut output = null;
        private Action<float> setVolumeDelegate;
        String getfilepath;
        public List<String> list_music_path = new List<String>();
        List<String> supportedAudioTypes = new List<String>() { ".mp3", ".mp2", ".mp1", ".wav", ".ogg", ".wma", ".m4a", ".flac", ".mid" };
        Int32 index_rightclick;
        String mode = "Shuffle";
        String status = "NoPlayed";
        Int32 listbox_index;
        void menuStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                String lanuage = ReadValue("Settings", "Language");

                if (lanuage == "English")
                {
                    switch (e.ClickedItem.Text)
                    {
                        case "Play":
                            if (index_rightclick != -1)
                            {
                                listBox1.SelectedIndex = index_rightclick;
                                status = "Played";
                                timer2.Enabled = false;
                            }
                            else
                            {
                                listBox1.SelectedIndex = 0;
                            }

                            break;
                        case "Pause":
                            if (output.PlaybackState == PlaybackState.Playing)
                            {
                                output.Pause();
                                status = "Paused";
                            }
                            contextMenuStrip1.Items[0].Text = "Resume";
                            timer2.Enabled = true;
                            timer2.Start();
                            break;
                        case "Resume":
                            if (output.PlaybackState == PlaybackState.Paused)
                            {
                                output.Play();
                                status = "Played";
                                timer1.Enabled = true;
                            }
                            contextMenuStrip1.Items[0].Text = "Pause";
                            timer2.Enabled = false;
                            break;

                        case "Previous":
                            if (listBox1.SelectedIndex > 0)
                            {
                                listBox1.SelectedIndex -= 1;
                            }
                            else if (listBox1.SelectedIndex == -1)
                            {
                                listBox1.SelectedIndex = 0;
                            }
                            timer2.Enabled = false;
                            break;
                        case "Next":
                            if (listBox1.SelectedIndex < listBox1.Items.Count)
                            {
                                listBox1.SelectedIndex += 1;
                            }
                            else if (listBox1.SelectedIndex == -1)
                            {
                                listBox1.SelectedIndex = 0;
                            }
                            timer2.Enabled = false;
                            break;
                    }
                }
                else if (lanuage == "Persian")
                {
                    switch (e.ClickedItem.Text)
                    {
                        case "پخش":
                            if (index_rightclick != -1)
                            {
                                listBox1.SelectedIndex = index_rightclick;
                                status = "Played";
                                timer2.Enabled = false;
                            }
                            else
                            {
                                listBox1.SelectedIndex = 0;
                            }

                            break;
                        case "مکث":
                            if (output.PlaybackState == PlaybackState.Playing)
                            {
                                output.Pause();
                                status = "Paused";
                            }
                            contextMenuStrip1.Items[0].Text = "ادامه پخش";
                            timer2.Enabled = true;
                            timer2.Start();
                            break;
                        case "ادامه پخش":
                            if (output.PlaybackState == PlaybackState.Paused)
                            {
                                output.Play();
                                status = "Played";
                                timer1.Enabled = true;
                            }
                            contextMenuStrip1.Items[0].Text = "مکث";
                            timer2.Enabled = false;
                            break;

                        case "قبلی":
                            if (listBox1.SelectedIndex > 0)
                            {
                                listBox1.SelectedIndex -= 1;
                            }
                            else if (listBox1.SelectedIndex == -1)
                            {
                                listBox1.SelectedIndex = 0;
                            }
                            timer2.Enabled = false;
                            break;
                        case "بعدی":
                            if (listBox1.SelectedIndex < listBox1.Items.Count)
                            {
                                listBox1.SelectedIndex += 1;
                            }
                            else if (listBox1.SelectedIndex == -1)
                            {
                                listBox1.SelectedIndex = 0;
                            }
                            timer2.Enabled = false;
                            break;
                    }
                }

            }
            catch
            {

            }

        }

        public MainFrm()
        {
            InitializeComponent();
            string lanuage = ReadValue("Settings", "Language");
            string ontop = ReadValue("Settings", "OnTop");
            string move = ReadValue("Settings", "Move");
            if (move == "Off")
            {
                onToolStripMenuItem1.Checked = false;
                offToolStripMenuItem1.Checked = true;
            }
            else
            {
                onToolStripMenuItem1.Checked = true;
                offToolStripMenuItem1.Checked = false;
            }
            if (ontop == "On")
            {
                onToolStripMenuItem.Checked = true;
                offToolStripMenuItem.Checked = false;
                this.TopMost = true;
            }
            else
            {
                onToolStripMenuItem.Checked = false;
                offToolStripMenuItem.Checked = true;
                this.TopMost = false;
            }
            if (lanuage == null)
            {
                lanuage = "English";
                WriteValue("Settings", "Language", "English");
            }
            if (lanuage == "English")
            {
                changeLanguage("English");
                englishToolStripMenuItem.Checked = true;
                persianToolStripMenuItem.Checked = false;
            }
            else if (lanuage == "Persian")
            {
                changeLanguage("Persian");
                englishToolStripMenuItem.Checked = false;
                persianToolStripMenuItem.Checked = true;
            }
            FileAssociations.EnsureAssociationsSet(".DPP", "Devil_Playlist", "DPP File");
            FileAssociations.EnsureAssociationsSet(".MP3", "MP3_File", "MP3 File");
            FileAssociations.EnsureAssociationsSet(".WAV", "WAV_File", "WAV File");
            FileAssociations.EnsureAssociationsSet(".MP2", "MP2_File", "MP3 File");
            FileAssociations.EnsureAssociationsSet(".MP1", "MP1_File", "MP1 File");
            FileAssociations.EnsureAssociationsSet(".OGG", "OGG_File", "OGG File");
            FileAssociations.EnsureAssociationsSet(".M4A", "M4A_File", "M4A File");
            FileAssociations.EnsureAssociationsSet(".WMA", "WMA_File", "WMA File");
            FileAssociations.EnsureAssociationsSet(".FLAC", "FLAC_File", "FLAC File");
            FileAssociations.EnsureAssociationsSet(".MID", "MID_File", "MID File");
            try
            {

                contextMenuStrip1.ItemClicked += menuStrip_ItemClicked;
                listBox1.ContextMenuStrip = contextMenuStrip1;

                foreach (string file in Program.filesToOpen)
                {
                    if (Path.GetExtension(file).ToLower() == ".dpp")
                    {
                        string line;
                        var myfile = new System.IO.StreamReader(file);

                        while ((line = myfile.ReadLine()) != null)
                        {
                            if (supportedAudioTypes.Contains(Path.GetExtension(line).ToLower()))
                            {
                                if (!listBox1.Items.Contains(Path.GetFileName(line)))
                                {
                                    list_music_path.Add(line);
                                    listBox1.Items.Add(Path.GetFileName(line));
                                    contextMenuStrip1.Items[0].Enabled = true;
                                    contextMenuStrip1.Items[1].Enabled = true;
                                    contextMenuStrip1.Items[2].Enabled = true;
                                    contextMenuStrip1.Items[3].Enabled = true;
                                }
                            }
                        }
                        WriteValue("Playlist", "LastPlaylist", file);
                        FilePlaylistToolStripMenuItem1.Text = file;
                        FilePlaylistToolStripMenuItem1.Enabled = true;
                        listBox1.SelectedIndex = 0;
                    }
                    else
                    {
                        String dir = Path.GetDirectoryName(file);
                        string[] files = Directory.GetFiles(dir);
                        foreach (string myfile in files)
                        {
                            if (supportedAudioTypes.Contains(Path.GetExtension(myfile).ToLower()))
                            {
                                if (!listBox1.Items.Contains(Path.GetFileName(myfile)))
                                {
                                    listBox1.Items.Add(Path.GetFileName(myfile));
                                    list_music_path.Add(myfile);
                                }
                            }
                        }
                        for (int i = 0; i < list_music_path.Count; i++)
                        {
                            if (file == list_music_path[i])
                            {
                                listBox1.SelectedIndex = i;
                            }
                        }
                    }
                }
                if (listBox1.Items.Count == 0)
                    getHistoryList();
                getSettingMode();
                String lastplaylist = ReadValue("Playlist", "LastPlaylist");
                if (lastplaylist != "")
                {
                    FilePlaylistToolStripMenuItem1.Text = ReadValue("Playlist", "LastPlaylist");
                    FilePlaylistToolStripMenuItem1.Enabled = true;
                }
                else
                {
                    if (lanuage == "English")
                    {
                        FilePlaylistToolStripMenuItem1.Text = "Empty";
                    }
                    else if (lanuage == "Persian")
                    {
                        FilePlaylistToolStripMenuItem1.Text = "خالی";
                    }

                    FilePlaylistToolStripMenuItem1.Enabled = false;
                }
                getSettingOpacity();

            }
            catch
            {

            }

        }

        private void DisposeWave()
        {
            try
            {
                if (output != null)
                {
                    if (output.PlaybackState == NAudio.Wave.PlaybackState.Playing) output.Stop();
                    output.Dispose();
                    output = null;
                }
                if (stream != null)
                {
                    stream.Dispose();
                    stream = null;
                }
            }
            catch
            {

            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                x = this.Left;
                y = this.Top;
                notifyIcon1.Text = "Devil Player";
                notifyIcon1.Visible = true;
                this.Left = Int32.Parse(ReadValue("Settings", "Left"));
                this.Top = Int32.Parse(ReadValue("Settings", "Top"));
                listBox1.DrawMode = DrawMode.OwnerDrawFixed;

                if (listBox1.Items.Count == 0)
                {
                    contextMenuStrip1.Items[0].Enabled = false;
                    contextMenuStrip1.Items[1].Enabled = false;
                    contextMenuStrip1.Items[2].Enabled = false;
                    contextMenuStrip1.Items[3].Enabled = false;
                }

            }
            catch
            {

            }
        }

        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            try
            {
                Font font = new Font(e.Font, (e.State & DrawItemState.Selected) == DrawItemState.Selected ? FontStyle.Bold : FontStyle.Regular);

                if (e.Index < 0) return;

                if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                    e = new DrawItemEventArgs(e.Graphics,
                                              font,
                                              e.Bounds,
                                              e.Index,
                                              e.State ^ DrawItemState.Selected,
                                              e.ForeColor,
                                              Color.DarkBlue);


                e.DrawBackground();
                e.Graphics.DrawString(listBox1.Items[e.Index].ToString(), font, Brushes.Lime, e.Bounds, StringFormat.GenericTypographic);
                e.DrawFocusRectangle();
            }
            catch
            {

            }

        }


        private void playMusic()
        {
            try
            {
                string lanuage = ReadValue("Settings", "Language");
                DisposeWave();
                audioFileReader = new AudioFileReader(getfilepath);
                var sampleChannel = new SampleChannel(audioFileReader, true);
                this.setVolumeDelegate = (vol) => sampleChannel.Volume = vol;
                var postVolumeMeter = new MeteringSampleProvider(sampleChannel);
                postVolumeMeter.StreamVolume += OnPostVolumeMeter;
                output = new NAudio.Wave.DirectSoundOut();
                output.Init(postVolumeMeter);
                setVolumeDelegate(volumeSlider1.Volume);
                output.Play();
                status = "Played";
                if (status == "Played")
                {
                    if (lanuage == "English")
                    {
                        contextMenuStrip1.Items[0].Text = "Pause";
                    }
                    else if (lanuage == "Persian")
                    {
                        contextMenuStrip1.Items[0].Text = "مکث";
                    }

                }
                timer2.Enabled = false;
                timer1.Enabled = true;
            }
            catch
            {

            }

        }

        private void nextPlayList()
        {
            try
            {

                TimeSpan currentTime = (output.PlaybackState == PlaybackState.Stopped) ? TimeSpan.Zero : audioFileReader.CurrentTime;
                if (output != null)
                {
                    if (String.Format("{0:00}:{1:00}", (int)currentTime.TotalMinutes, currentTime.Seconds) == String.Format("{0:00}:{1:00}", (int)audioFileReader.TotalTime.TotalMinutes, audioFileReader.TotalTime.Seconds))
                    {
                        if (mode == "Shuffle")
                        {
                            Random generator = new Random();
                            listBox1.SelectedIndex = generator.Next(listBox1.Items.Count);
                        }
                        if (mode == "Repeat")
                        {
                            playMusic();
                        }
                        if (mode == "NoRepeat")
                        {
                            if (listBox1.SelectedIndex < listBox1.Items.Count - 1)
                            {
                                listBox1.SelectedIndex += 1;
                            }
                        }
                        if (mode == "RepeatAll")
                        {
                            if (listBox1.SelectedIndex < listBox1.Items.Count - 1)
                            {
                                listBox1.SelectedIndex += 1;
                            }
                            else
                            {
                                listBox1.SelectedIndex = 0;
                            }
                        }

                    }
                }
            }
            catch
            {

            }

        }

        void OnPostVolumeMeter(object sender, StreamVolumeEventArgs e)
        {
            try
            {
                volumeMeter1.Amplitude = e.MaxSampleValues[0];
                volumeMeter2.Amplitude = e.MaxSampleValues[1];
            }
            catch
            {

            }

        }
        private void listBox1_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string myfile in files)
                {
                    if (supportedAudioTypes.Contains(Path.GetExtension(myfile).ToLower()))
                    {
                        if (!listBox1.Items.Contains(Path.GetFileName(myfile)))
                        {
                            listBox1.Items.Add(Path.GetFileName(myfile));
                            list_music_path.Add(myfile);
                            if (listBox1.Items.Count > 0)
                            {
                                //listBox1.SelectedIndex = 0;
                                contextMenuStrip1.Items[0].Enabled = true;
                                contextMenuStrip1.Items[1].Enabled = true;
                                contextMenuStrip1.Items[2].Enabled = true;
                                contextMenuStrip1.Items[3].Enabled = true;
                            }
                        }
                    }
                }
            }
            catch
            {

            }
        }

        private void listBox1_DragEnter(object sender, DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
            }
            catch
            {

            }
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string myfile in files)
                {
                    if (supportedAudioTypes.Contains(Path.GetExtension(myfile).ToLower()))
                    {
                        if (!listBox1.Items.Contains(Path.GetFileName(myfile)))
                        {
                            listBox1.Items.Add(Path.GetFileName(myfile));
                            list_music_path.Add(myfile);
                        }
                    }
                    else
                    {
                        MessageBox.Show("No support format file");
                    }
                }
            }
            catch
            {

            }

        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
            }
            catch
            {

            }
        }

        private void OnVolumeSliderChanged(object sender, EventArgs e)
        {
            try
            {
                if (setVolumeDelegate != null)
                {
                    setVolumeDelegate(volumeSlider1.Volume);
                }
            }
            catch
            {

            }

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (output != null)
                {
                    TimeSpan currentTime = (output.PlaybackState == PlaybackState.Stopped) ? TimeSpan.Zero : audioFileReader.CurrentTime;
                    progressBar1.Value = Math.Min(progressBar1.Maximum, (int)(100 * currentTime.TotalSeconds / audioFileReader.TotalTime.TotalSeconds));
                    this.Text = "Devil Player - " + String.Format("{0:00}:{1:00}", (int)currentTime.TotalMinutes, currentTime.Seconds) + " / " + String.Format("{0:00}:{1:00}", (int)audioFileReader.TotalTime.TotalMinutes, audioFileReader.TotalTime.Seconds);
                    nextPlayList();
                }
                else
                {
                    progressBar1.Value = 0;
                }
            }
            catch
            {

            }

        }

        private void progressBar1_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (output != null)
                    {
                        float absoluteMouse = (PointToClient(MousePosition).X - progressBar1.Bounds.X);
                        float calcFactor = progressBar1.Width / (int)progressBar1.Maximum;
                        float relativeMouse = absoluteMouse / calcFactor;
                        progressBar1.Value = Convert.ToInt32(relativeMouse);
                        audioFileReader.CurrentTime = TimeSpan.FromSeconds(audioFileReader.TotalTime.TotalSeconds * progressBar1.Value / 100);

                    }
                }

            }
            catch
            {

            }
        }

        private void shuffleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                repeatToolStripMenuItem.Checked = false;
                repeatAllToolStripMenuItem.Checked = false;
                noRepeatToolStripMenuItem.Checked = false;
                mode = "Shuffle";
                WriteValue("Settings", "Mode", mode);
            }
            catch
            {

            }

        }

        private void repeatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                shuffleToolStripMenuItem.Checked = false;
                repeatAllToolStripMenuItem.Checked = false;
                noRepeatToolStripMenuItem.Checked = false;
                mode = "Repeat";
                WriteValue("Settings", "Mode", mode);
            }
            catch
            {

            }
        }

        private void noRepeatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                shuffleToolStripMenuItem.Checked = false;
                repeatAllToolStripMenuItem.Checked = false;
                repeatToolStripMenuItem.Checked = false;
                mode = "NoRepeat";
                WriteValue("Settings", "Mode", mode);
            }
            catch
            {

            }

        }

        private void repeatAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                shuffleToolStripMenuItem.Checked = false;
                noRepeatToolStripMenuItem.Checked = false;
                repeatToolStripMenuItem.Checked = false;
                mode = "RepeatAll";
                WriteValue("Settings", "Mode", mode);
            }
            catch
            {

            }

        }

        private void listBox1_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                String language = ReadValue("Settings", "Language");
                if (e.Button == MouseButtons.Right)
                {
                    index_rightclick = listBox1.IndexFromPoint(e.X, e.Y);
                }
                if (language == "English")
                {
                    if (listbox_index == index_rightclick)
                    {
                        if (status == "NoPlayed")
                        {
                            contextMenuStrip1.Items[0].Text = "Play";
                        }
                        else if (status == "Paused")
                        {
                            contextMenuStrip1.Items[0].Text = "Resume";
                        }
                        else if (status == "Played")
                        {
                            contextMenuStrip1.Items[0].Text = "Pause";
                        }
                        else if (status == "Stoped")
                        {
                            contextMenuStrip1.Items[0].Text = "Play";
                        }
                    }
                    else
                    {
                        contextMenuStrip1.Items[0].Text = "Play";
                    }
                }
                else if (language == "Persian")
                {
                    if (listbox_index == index_rightclick)
                    {
                        if (status == "NoPlayed")
                        {
                            contextMenuStrip1.Items[0].Text = "پخش";
                        }
                        else if (status == "Paused")
                        {
                            contextMenuStrip1.Items[0].Text = "ادامه پخش";
                        }
                        else if (status == "Played")
                        {
                            contextMenuStrip1.Items[0].Text = "مکث";
                        }
                        else if (status == "Stoped")
                        {
                            contextMenuStrip1.Items[0].Text = "پخش";
                        }
                    }
                    else
                    {
                        contextMenuStrip1.Items[0].Text = "پخش";
                    }
                }

            }
            catch
            {

            }

        }

        private void addDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult result = folderBrowserDialog1.ShowDialog();
                if (result == DialogResult.OK)
                {
                    string[] files = Directory.GetFiles(folderBrowserDialog1.SelectedPath);
                    foreach (string file in files)
                    {
                        if (supportedAudioTypes.Contains(Path.GetExtension(file).ToLower()))
                        {
                            if (!listBox1.Items.Contains(Path.GetFileName(file)))
                            {
                                list_music_path.Add(file);
                                listBox1.Items.Add(Path.GetFileName(file));
                                contextMenuStrip1.Items[0].Enabled = true;
                                contextMenuStrip1.Items[1].Enabled = true;
                                contextMenuStrip1.Items[2].Enabled = true;
                                contextMenuStrip1.Items[3].Enabled = true;
                            }
                        }
                    }
                }
            }
            catch
            {

            }

        }

        private void addMediaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                openFileDialog1.Filter = "All Support Format|*.MP3;*.MP2;*.mp1;*.wav;*.ogg;*.WMA;*.M4A;*.FLAC;*.MID";
                openFileDialog1.Title = "Devil Player Open File";
                DialogResult result = openFileDialog1.ShowDialog();
                if (result == DialogResult.OK)
                {
                    string file = openFileDialog1.FileName;
                    if (supportedAudioTypes.Contains(Path.GetExtension(file).ToLower()))
                    {
                        if (!listBox1.Items.Contains(Path.GetFileName(file)))
                        {
                            list_music_path.Add(file);
                            listBox1.Items.Add(Path.GetFileName(file));
                            contextMenuStrip1.Items[0].Enabled = true;
                            contextMenuStrip1.Items[1].Enabled = true;
                            contextMenuStrip1.Items[2].Enabled = true;
                            contextMenuStrip1.Items[3].Enabled = true;
                        }
                    }
                }
            }
            catch
            {

            }

        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (index_rightclick != listBox1.SelectedIndex)
                {
                    list_music_path.RemoveAt(index_rightclick);
                    listBox1.Items.RemoveAt(index_rightclick);
                }
                else
                {
                    output.Stop();
                    list_music_path.RemoveAt(index_rightclick);
                    listBox1.Items.RemoveAt(index_rightclick);
                    timer2.Enabled = true;
                    timer2.Start();
                    timer1.Enabled = false;
                    this.Text = "Devil Player";
                    progressBar1.Value = 0;
                }
            }
            catch
            {

            }

        }

        private void newPlaylistToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (output != null)
                {
                    if (output.PlaybackState == PlaybackState.Paused || output.PlaybackState == PlaybackState.Playing)
                    {
                        output.Stop();
                        timer2.Enabled = true;
                        timer2.Start();
                        timer1.Enabled = false;
                        this.Text = "Devil Player";
                        progressBar1.Value = 0;
                        list_music_path.Clear();
                        listBox1.Items.Clear();
                    }
                    else
                    {
                        list_music_path.Clear();
                        listBox1.Items.Clear();
                    }
                }
                else
                {
                    list_music_path.Clear();
                    listBox1.Items.Clear();
                }
            }
            catch
            {

            }

        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            try
            {
                float a = float.Parse("0.007");
                if (volumeMeter1.Amplitude > 0 || volumeMeter2.Amplitude > 0)
                {
                    volumeMeter1.Amplitude -= a;
                    volumeMeter2.Amplitude -= a;
                }
                else
                {
                    timer2.Stop();
                }

            }
            catch
            {

            }

        }

        private void savePlaylistToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                saveFileDialog1.Filter = "Devil Playlist(*.DPP) | *.DPP";
                saveFileDialog1.Title = "Save Playlist";
                DialogResult result = saveFileDialog1.ShowDialog();
                if (result == DialogResult.OK)
                {
                    System.IO.StreamWriter SaveFile = new System.IO.StreamWriter(saveFileDialog1.FileName);
                    foreach (var item in list_music_path)
                    {
                        SaveFile.WriteLine(item);
                    }

                    SaveFile.Close();

                    MessageBox.Show("Playlist is Saved!", "Devil Player", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch
            {

            }

        }

        private void loadPlaylistToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                openFileDialog1.Filter = "Devil Playlist (*.DPP)|*.DPP";
                openFileDialog1.Title = "Load Playlist";
                DialogResult result = openFileDialog1.ShowDialog();
                if (result == DialogResult.OK)
                {
                    string line;
                    var file = new System.IO.StreamReader(openFileDialog1.FileName);

                    while ((line = file.ReadLine()) != null)
                    {
                        if (supportedAudioTypes.Contains(Path.GetExtension(line).ToLower()))
                        {
                            if (!listBox1.Items.Contains(Path.GetFileName(line)))
                            {
                                list_music_path.Add(line);
                                listBox1.Items.Add(Path.GetFileName(line));
                                contextMenuStrip1.Items[0].Enabled = true;
                                contextMenuStrip1.Items[1].Enabled = true;
                                contextMenuStrip1.Items[2].Enabled = true;
                                contextMenuStrip1.Items[3].Enabled = true;
                            }
                        }
                    }
                    WriteValue("Playlist", "LastPlaylist", openFileDialog1.FileName);
                    FilePlaylistToolStripMenuItem1.Text = openFileDialog1.FileName;
                    FilePlaylistToolStripMenuItem1.Enabled = true;
                }
            }
            catch
            {

            }
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            this.Opacity = 1.0;
            toolStripMenuItem4.Checked = true;
            toolStripMenuItem5.Checked = false;
            toolStripMenuItem6.Checked = false;
            toolStripMenuItem7.Checked = false;
            toolStripMenuItem8.Checked = false;
            toolStripMenuItem9.Checked = false;
            toolStripMenuItem10.Checked = false;
            toolStripMenuItem11.Checked = false;
            toolStripMenuItem12.Checked = false;
            toolStripMenuItem13.Checked = false;
            WriteValue("Settings", "Opacity", "1.0");
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            this.Opacity = 0.9;
            toolStripMenuItem4.Checked = false;
            toolStripMenuItem5.Checked = true;
            toolStripMenuItem6.Checked = false;
            toolStripMenuItem7.Checked = false;
            toolStripMenuItem8.Checked = false;
            toolStripMenuItem9.Checked = false;
            toolStripMenuItem10.Checked = false;
            toolStripMenuItem11.Checked = false;
            toolStripMenuItem12.Checked = false;
            toolStripMenuItem13.Checked = false;
            WriteValue("Settings", "Opacity", "0.9");
        }

        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
            this.Opacity = 0.8;
            toolStripMenuItem4.Checked = false;
            toolStripMenuItem5.Checked = false;
            toolStripMenuItem6.Checked = true;
            toolStripMenuItem7.Checked = false;
            toolStripMenuItem8.Checked = false;
            toolStripMenuItem9.Checked = false;
            toolStripMenuItem10.Checked = false;
            toolStripMenuItem11.Checked = false;
            toolStripMenuItem12.Checked = false;
            toolStripMenuItem13.Checked = false;
            WriteValue("Settings", "Opacity", "0.8");
        }

        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        {
            this.Opacity = 0.7;
            toolStripMenuItem4.Checked = false;
            toolStripMenuItem5.Checked = false;
            toolStripMenuItem6.Checked = false;
            toolStripMenuItem7.Checked = true;
            toolStripMenuItem8.Checked = false;
            toolStripMenuItem9.Checked = false;
            toolStripMenuItem10.Checked = false;
            toolStripMenuItem11.Checked = false;
            toolStripMenuItem12.Checked = false;
            toolStripMenuItem13.Checked = false;
            WriteValue("Settings", "Opacity", "0.7");
        }

        private void toolStripMenuItem8_Click(object sender, EventArgs e)
        {
            this.Opacity = 0.6;
            toolStripMenuItem4.Checked = false;
            toolStripMenuItem5.Checked = false;
            toolStripMenuItem6.Checked = false;
            toolStripMenuItem7.Checked = false;
            toolStripMenuItem8.Checked = true;
            toolStripMenuItem9.Checked = false;
            toolStripMenuItem10.Checked = false;
            toolStripMenuItem11.Checked = false;
            toolStripMenuItem12.Checked = false;
            toolStripMenuItem13.Checked = false;
            WriteValue("Settings", "Opacity", "0.6");
        }

        private void toolStripMenuItem9_Click(object sender, EventArgs e)
        {
            this.Opacity = 0.5;
            toolStripMenuItem4.Checked = false;
            toolStripMenuItem5.Checked = false;
            toolStripMenuItem6.Checked = false;
            toolStripMenuItem7.Checked = false;
            toolStripMenuItem8.Checked = false;
            toolStripMenuItem9.Checked = true;
            toolStripMenuItem10.Checked = false;
            toolStripMenuItem11.Checked = false;
            toolStripMenuItem12.Checked = false;
            toolStripMenuItem13.Checked = false;
            WriteValue("Settings", "Opacity", "0.5");
        }

        private void toolStripMenuItem10_Click(object sender, EventArgs e)
        {
            this.Opacity = 0.4;
            toolStripMenuItem4.Checked = false;
            toolStripMenuItem5.Checked = false;
            toolStripMenuItem6.Checked = false;
            toolStripMenuItem7.Checked = false;
            toolStripMenuItem8.Checked = false;
            toolStripMenuItem9.Checked = false;
            toolStripMenuItem10.Checked = true;
            toolStripMenuItem11.Checked = false;
            toolStripMenuItem12.Checked = false;
            toolStripMenuItem13.Checked = false;
            WriteValue("Settings", "Opacity", "0.4");
        }

        private void toolStripMenuItem11_Click(object sender, EventArgs e)
        {
            this.Opacity = 0.3;
            toolStripMenuItem4.Checked = false;
            toolStripMenuItem5.Checked = false;
            toolStripMenuItem6.Checked = false;
            toolStripMenuItem7.Checked = false;
            toolStripMenuItem8.Checked = false;
            toolStripMenuItem9.Checked = false;
            toolStripMenuItem10.Checked = false;
            toolStripMenuItem11.Checked = true;
            toolStripMenuItem12.Checked = false;
            toolStripMenuItem13.Checked = false;
            WriteValue("Settings", "Opacity", "0.3");
        }

        private void toolStripMenuItem12_Click(object sender, EventArgs e)
        {
            this.Opacity = 0.2;
            toolStripMenuItem4.Checked = false;
            toolStripMenuItem5.Checked = false;
            toolStripMenuItem6.Checked = false;
            toolStripMenuItem7.Checked = false;
            toolStripMenuItem8.Checked = false;
            toolStripMenuItem9.Checked = false;
            toolStripMenuItem10.Checked = false;
            toolStripMenuItem11.Checked = false;
            toolStripMenuItem12.Checked = true;
            toolStripMenuItem13.Checked = false;
            WriteValue("Settings", "Opacity", "0.2");
        }

        private void toolStripMenuItem13_Click(object sender, EventArgs e)
        {
            this.Opacity = 0.1;
            toolStripMenuItem4.Checked = false;
            toolStripMenuItem5.Checked = false;
            toolStripMenuItem6.Checked = false;
            toolStripMenuItem7.Checked = false;
            toolStripMenuItem8.Checked = false;
            toolStripMenuItem9.Checked = false;
            toolStripMenuItem10.Checked = false;
            toolStripMenuItem11.Checked = false;
            toolStripMenuItem12.Checked = false;
            toolStripMenuItem13.Checked = true;
            WriteValue("Settings", "Opacity", "0.1");
        }



        //===================================ini setting save==========================================
        private String ReadSection()
        {

            string[] values = DevilPlayer.IniFileHelper.ReadSections(System.IO.Path.GetFullPath(Application.StartupPath + @"\" + "settings.ini"));
            if (values != null)
            {
                string value = string.Join(Environment.NewLine, values);
                return value;
            }
            else
            {
                return "Reading sections failed.";
            }
        }

        private String ReadKeys(String section)
        {
            string[] values = DevilPlayer.IniFileHelper.ReadKeys(section, System.IO.Path.GetFullPath(Application.StartupPath + @"\" + "settings.ini"));
            if (values != null)
            {
                string value = string.Join(Environment.NewLine, values);
                return value;
            }
            else
            {
                return "Reading keys failed.";
            }
        }

        private String ReadValue(String section, String key)
        {
            string value = DevilPlayer.IniFileHelper.ReadValue(section, key, System.IO.Path.GetFullPath(Application.StartupPath + @"\" + "settings.ini"));
            return value;
        }

        private void WriteValue(String section, String key, String value)
        {
            bool result = DevilPlayer.IniFileHelper.WriteValue(section, key, value, System.IO.Path.GetFullPath(Application.StartupPath + @"\" + "settings.ini"));
        }

        private void DeleteKey(String section, String key)
        {
            bool result = DevilPlayer.IniFileHelper.DeleteKey(section, key, System.IO.Path.GetFullPath(Application.StartupPath + @"\" + "settings.ini"));
        }

        //========================================================================================
        private void getSettingHistoryPlayList(String playlist)
        {
            try
            {
                string line;
                if (File.Exists(playlist))
                {
                    var file = new System.IO.StreamReader(playlist);

                    while ((line = file.ReadLine()) != null)
                    {
                        if (supportedAudioTypes.Contains(Path.GetExtension(line).ToLower()))
                        {
                            if (!listBox1.Items.Contains(Path.GetFileName(line)))
                            {
                                list_music_path.Add(line);
                                listBox1.Items.Add(Path.GetFileName(line));
                                contextMenuStrip1.Items[0].Enabled = true;
                                contextMenuStrip1.Items[1].Enabled = true;
                                contextMenuStrip1.Items[2].Enabled = true;
                                contextMenuStrip1.Items[3].Enabled = true;
                            }
                        }
                    }
                }
            }
            catch
            {

            }

        }
        private void getSettingMode()
        {
            try
            {
                mode = ReadValue("Settings", "Mode");
                switch (mode)
                {
                    case "Shuffle":
                        shuffleToolStripMenuItem.Checked = true;
                        repeatToolStripMenuItem.Checked = false;
                        repeatAllToolStripMenuItem.Checked = false;
                        noRepeatToolStripMenuItem.Checked = false;
                        break;
                    case "Repeat":
                        shuffleToolStripMenuItem.Checked = false;
                        repeatToolStripMenuItem.Checked = true;
                        repeatAllToolStripMenuItem.Checked = false;
                        noRepeatToolStripMenuItem.Checked = false;
                        break;
                    case "NoRepeat":
                        shuffleToolStripMenuItem.Checked = false;
                        repeatToolStripMenuItem.Checked = false;
                        repeatAllToolStripMenuItem.Checked = false;
                        noRepeatToolStripMenuItem.Checked = true;
                        break;
                    case "RepeatAll":
                        shuffleToolStripMenuItem.Checked = false;
                        repeatToolStripMenuItem.Checked = false;
                        repeatAllToolStripMenuItem.Checked = true;
                        noRepeatToolStripMenuItem.Checked = false;
                        break;
                }
            }
            catch
            {

            }

        }

        private void getSettingOpacity()
        {
            try
            {
                this.Opacity = Double.Parse(ReadValue("Settings", "Opacity"));
                switch (Double.Parse(ReadValue("Settings", "Opacity")))
                {
                    case 1.0:
                        toolStripMenuItem4.Checked = true;
                        toolStripMenuItem5.Checked = false;
                        toolStripMenuItem6.Checked = false;
                        toolStripMenuItem7.Checked = false;
                        toolStripMenuItem8.Checked = false;
                        toolStripMenuItem9.Checked = false;
                        toolStripMenuItem10.Checked = false;
                        toolStripMenuItem11.Checked = false;
                        toolStripMenuItem12.Checked = false;
                        toolStripMenuItem13.Checked = false;
                        break;
                    case 0.9:
                        toolStripMenuItem4.Checked = false;
                        toolStripMenuItem5.Checked = true;
                        toolStripMenuItem6.Checked = false;
                        toolStripMenuItem7.Checked = false;
                        toolStripMenuItem8.Checked = false;
                        toolStripMenuItem9.Checked = false;
                        toolStripMenuItem10.Checked = false;
                        toolStripMenuItem11.Checked = false;
                        toolStripMenuItem12.Checked = false;
                        toolStripMenuItem13.Checked = false;
                        break;
                    case 0.8:
                        toolStripMenuItem4.Checked = false;
                        toolStripMenuItem5.Checked = false;
                        toolStripMenuItem6.Checked = true;
                        toolStripMenuItem7.Checked = false;
                        toolStripMenuItem8.Checked = false;
                        toolStripMenuItem9.Checked = false;
                        toolStripMenuItem10.Checked = false;
                        toolStripMenuItem11.Checked = false;
                        toolStripMenuItem12.Checked = false;
                        toolStripMenuItem13.Checked = false;
                        break;
                    case 0.7:
                        toolStripMenuItem4.Checked = false;
                        toolStripMenuItem5.Checked = false;
                        toolStripMenuItem6.Checked = false;
                        toolStripMenuItem7.Checked = true;
                        toolStripMenuItem8.Checked = false;
                        toolStripMenuItem9.Checked = false;
                        toolStripMenuItem10.Checked = false;
                        toolStripMenuItem11.Checked = false;
                        toolStripMenuItem12.Checked = false;
                        toolStripMenuItem13.Checked = false;
                        break;
                    case 0.6:
                        toolStripMenuItem4.Checked = false;
                        toolStripMenuItem5.Checked = false;
                        toolStripMenuItem6.Checked = false;
                        toolStripMenuItem7.Checked = false;
                        toolStripMenuItem8.Checked = true;
                        toolStripMenuItem9.Checked = false;
                        toolStripMenuItem10.Checked = false;
                        toolStripMenuItem11.Checked = false;
                        toolStripMenuItem12.Checked = false;
                        toolStripMenuItem13.Checked = false;
                        break;
                    case 0.5:
                        toolStripMenuItem4.Checked = false;
                        toolStripMenuItem5.Checked = false;
                        toolStripMenuItem6.Checked = false;
                        toolStripMenuItem7.Checked = false;
                        toolStripMenuItem8.Checked = false;
                        toolStripMenuItem9.Checked = true;
                        toolStripMenuItem10.Checked = false;
                        toolStripMenuItem11.Checked = false;
                        toolStripMenuItem12.Checked = false;
                        toolStripMenuItem13.Checked = false;
                        break;
                    case 0.4:
                        toolStripMenuItem4.Checked = false;
                        toolStripMenuItem5.Checked = false;
                        toolStripMenuItem6.Checked = false;
                        toolStripMenuItem7.Checked = false;
                        toolStripMenuItem8.Checked = false;
                        toolStripMenuItem9.Checked = false;
                        toolStripMenuItem10.Checked = true;
                        toolStripMenuItem11.Checked = false;
                        toolStripMenuItem12.Checked = false;
                        toolStripMenuItem13.Checked = false;
                        break;
                    case 0.3:
                        toolStripMenuItem4.Checked = false;
                        toolStripMenuItem5.Checked = false;
                        toolStripMenuItem6.Checked = false;
                        toolStripMenuItem7.Checked = false;
                        toolStripMenuItem8.Checked = false;
                        toolStripMenuItem9.Checked = false;
                        toolStripMenuItem10.Checked = false;
                        toolStripMenuItem11.Checked = true;
                        toolStripMenuItem12.Checked = false;
                        toolStripMenuItem13.Checked = false;
                        break;
                    case 0.2:
                        toolStripMenuItem4.Checked = false;
                        toolStripMenuItem5.Checked = false;
                        toolStripMenuItem6.Checked = false;
                        toolStripMenuItem7.Checked = false;
                        toolStripMenuItem8.Checked = false;
                        toolStripMenuItem9.Checked = false;
                        toolStripMenuItem10.Checked = false;
                        toolStripMenuItem11.Checked = false;
                        toolStripMenuItem12.Checked = true;
                        toolStripMenuItem13.Checked = false;
                        break;
                    case 0.1:
                        toolStripMenuItem4.Checked = false;
                        toolStripMenuItem5.Checked = false;
                        toolStripMenuItem6.Checked = false;
                        toolStripMenuItem7.Checked = false;
                        toolStripMenuItem8.Checked = false;
                        toolStripMenuItem9.Checked = false;
                        toolStripMenuItem10.Checked = false;
                        toolStripMenuItem11.Checked = false;
                        toolStripMenuItem12.Checked = false;
                        toolStripMenuItem13.Checked = true;
                        break;
                }
            }
            catch
            {

            }


        }

        private void getHistoryList()
        {
            try
            {
                string line;

                if (File.Exists(Application.StartupPath + @"\History.HIS"))
                {
                    var file = new System.IO.StreamReader(Application.StartupPath + @"\History.HIS");

                    while ((line = file.ReadLine()) != null)
                    {
                        if (supportedAudioTypes.Contains(Path.GetExtension(line).ToLower()))
                        {
                            if (!listBox1.Items.Contains(Path.GetFileName(line)))
                            {
                                list_music_path.Add(line);
                                listBox1.Items.Add(Path.GetFileName(line));
                                contextMenuStrip1.Items[0].Enabled = true;
                                contextMenuStrip1.Items[1].Enabled = true;
                                contextMenuStrip1.Items[2].Enabled = true;
                                contextMenuStrip1.Items[3].Enabled = true;
                            }
                        }
                    }
                    if (listBox1.Items.Count > 0)
                    {
                        listBox1.SelectedIndex = Int32.Parse(ReadValue("List", "Index"));
                    }
                }
            }
            catch
            {

            }


        }

        private void FilePlaylistToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                getSettingHistoryPlayList(FilePlaylistToolStripMenuItem1.Text);

            }
            catch
            {

            }
        }

        private void clearHistoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string lanuage = ReadValue("Settings", "Language");
                DeleteKey("Playlist", "LastPlaylist");
                if (lanuage == "English")
                {
                    FilePlaylistToolStripMenuItem1.Text = "Empty";
                }
                else if (lanuage == "Persian")
                {
                    FilePlaylistToolStripMenuItem1.Text = "خالی";
                }
                FilePlaylistToolStripMenuItem1.Enabled = false;
            }
            catch
            {

            }

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (output != null)
                {
                    output.Stop();
                    output = null;
                    audioFileReader = null;
                    getfilepath = null;
                    status = "NoPlayed";
                }
                if (listBox1.Items.Count > 0)
                {
                    listbox_index = listBox1.SelectedIndex;
                    getfilepath = list_music_path[listBox1.SelectedIndex];
                    playMusic();
                }
            }
            catch
            {

            }
        }

        private void MainFrm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    WriteValue("Settings", "Left", x.ToString());
                    WriteValue("Settings", "Top", y.ToString());
                    if (listBox1.Items.Count > 0)
                    {
                        WriteValue("List", "Index", listBox1.SelectedIndex.ToString());
                    }
                    if (File.Exists(Application.StartupPath + @"\History.HIS"))
                    {
                        File.Delete(Application.StartupPath + @"\History.HIS");
                        System.IO.StreamWriter SaveFile = new System.IO.StreamWriter(Application.StartupPath + @"\History.HIS");
                        foreach (var item in list_music_path)
                        {
                            SaveFile.WriteLine(item);
                        }

                        SaveFile.Close();
                    }
                    else
                    {
                        System.IO.StreamWriter SaveFile = new System.IO.StreamWriter(Application.StartupPath + @"\History.HIS");
                        foreach (var item in list_music_path)
                        {
                            SaveFile.WriteLine(item);
                        }

                        SaveFile.Close();
                    }
                }

            }
            catch
            {

            }

        }

        private void searchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string language = ReadValue("Settings", "Language");
                FindFrm frmFind = new FindFrm();
                if (language == "Persian")
                {
                    frmFind.Text = "جستجو";
                }
                else
                {
                    frmFind.Text = "Search";
                }
                foreach (String myfile in list_music_path)
                {
                    frmFind.list_music_path2.Add(myfile);
                }
                if (this.TopMost == true)
                {
                    frmFind.TopMost = true;
                }
                frmFind.Owner = this;
                frmFind.ShowDialog();
            }
            catch
            {

            }

        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                output.Stop();
                status = "NoPlayed";
                listBox1.SelectedIndex = -1;
                contextMenuStrip1.Items[0].Text = "Play";
                timer1.Enabled = false;
                this.Text = "Devil Player";
                progressBar1.Value = 0;
                timer2.Enabled = true;
                timer2.Start();
            }
            catch
            {

            }

        }

        private void MainFrm_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                String language = ReadValue("Settings", "Language");
                if (e.KeyCode == Keys.Space)
                {
                    if (output.PlaybackState == PlaybackState.Playing)
                    {
                        output.Pause();
                        status = "Paused";
                        if (language == "English")
                        {
                            contextMenuStrip1.Items[0].Text = "Resume";
                        }
                        else if (language == "Persian")
                        {
                            contextMenuStrip1.Items[0].Text = "ادامه پخش";
                        }

                        timer2.Enabled = true;
                        timer2.Start();
                    }

                    else if (output.PlaybackState == PlaybackState.Paused)
                    {
                        output.Play();
                        status = "Played";
                        timer1.Enabled = true;
                        if (language == "English")
                        {
                            contextMenuStrip1.Items[0].Text = "Pause";
                        }
                        else if (language == "Persian")
                        {
                            contextMenuStrip1.Items[0].Text = "مکث";
                        }

                        timer2.Enabled = false;
                    }

                }
                else if (e.KeyCode == Keys.X)
                {
                    if (index_rightclick != -1)
                    {
                        listBox1.SelectedIndex = index_rightclick;
                        status = "Played";
                        timer2.Enabled = false;
                    }
                    else
                    {
                        listBox1.SelectedIndex = 0;
                    }
                }
                else if (e.KeyCode == Keys.V)
                {
                    if (output.PlaybackState == PlaybackState.Paused || output.PlaybackState == PlaybackState.Playing)
                    {
                        output.Stop();
                        status = "NoPlayed";
                        listBox1.SelectedIndex = -1;
                        if (language == "English")
                        {
                            contextMenuStrip1.Items[0].Text = "Play";
                        }
                        else if (language == "Persian")
                        {
                            contextMenuStrip1.Items[0].Text = "پخش";
                        }

                        timer1.Enabled = false;
                        this.Text = "Devil Player";
                        progressBar1.Value = 0;
                        timer2.Enabled = true;
                        timer2.Start();
                    }
                }
                else if (e.KeyCode == Keys.B)
                {
                    if (listBox1.SelectedIndex < listBox1.Items.Count)
                    {
                        listBox1.SelectedIndex += 1;
                    }
                    else if (listBox1.SelectedIndex == -1)
                    {
                        listBox1.SelectedIndex = 0;
                    }
                    timer2.Enabled = false;
                }
                else if (e.KeyCode == Keys.Z)
                {
                    if (listBox1.SelectedIndex > 0)
                    {
                        listBox1.SelectedIndex -= 1;
                    }
                    else if (listBox1.SelectedIndex == -1)
                    {
                        listBox1.SelectedIndex = 0;
                    }
                    timer2.Enabled = false;
                }
                else if (e.KeyCode == Keys.N)
                {
                    openFileDialog1.Filter = "All Support Format|*.MP3;*.MP2;*.mp1;*.wav;*.ogg;*.WMA;*.M4A;*.FLAC;*.MID";
                    openFileDialog1.Title = "Devil Player Open File";
                    DialogResult result = openFileDialog1.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        string file = openFileDialog1.FileName;
                        if (supportedAudioTypes.Contains(Path.GetExtension(file).ToLower()))
                        {
                            if (!listBox1.Items.Contains(Path.GetFileName(file)))
                            {
                                list_music_path.Add(file);
                                listBox1.Items.Add(Path.GetFileName(file));
                                contextMenuStrip1.Items[0].Enabled = true;
                                contextMenuStrip1.Items[1].Enabled = true;
                                contextMenuStrip1.Items[2].Enabled = true;
                                contextMenuStrip1.Items[3].Enabled = true;
                            }
                        }
                    }
                }
                else if (e.KeyCode == Keys.D)
                {
                    DialogResult result = folderBrowserDialog1.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        string[] files = Directory.GetFiles(folderBrowserDialog1.SelectedPath);
                        foreach (string file in files)
                        {
                            if (supportedAudioTypes.Contains(Path.GetExtension(file).ToLower()))
                            {
                                if (!listBox1.Items.Contains(Path.GetFileName(file)))
                                {
                                    list_music_path.Add(file);
                                    listBox1.Items.Add(Path.GetFileName(file));
                                    contextMenuStrip1.Items[0].Enabled = true;
                                    contextMenuStrip1.Items[1].Enabled = true;
                                    contextMenuStrip1.Items[2].Enabled = true;
                                    contextMenuStrip1.Items[3].Enabled = true;
                                }
                            }
                        }
                    }
                }
                else if (e.KeyCode == Keys.C)
                {
                    if (output != null)
                    {
                        if (output.PlaybackState == PlaybackState.Paused || output.PlaybackState == PlaybackState.Playing)
                        {
                            output.Stop();
                            timer2.Enabled = true;
                            timer2.Start();
                            timer1.Enabled = false;
                            this.Text = "Devil Player";
                            progressBar1.Value = 0;
                            list_music_path.Clear();
                            listBox1.Items.Clear();
                        }
                        else
                        {
                            list_music_path.Clear();
                            listBox1.Items.Clear();
                        }
                    }
                    else
                    {
                        list_music_path.Clear();
                        listBox1.Items.Clear();
                    }
                }
                else if (e.KeyCode == Keys.L)
                {
                    openFileDialog1.Filter = "Devil Playlist (*.DPP)|*.DPP";
                    openFileDialog1.Title = "Load Playlist";
                    DialogResult result = openFileDialog1.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        string line;
                        var file = new System.IO.StreamReader(openFileDialog1.FileName);

                        while ((line = file.ReadLine()) != null)
                        {
                            if (supportedAudioTypes.Contains(Path.GetExtension(line).ToLower()))
                            {
                                if (!listBox1.Items.Contains(Path.GetFileName(line)))
                                {
                                    list_music_path.Add(line);
                                    listBox1.Items.Add(Path.GetFileName(line));
                                    contextMenuStrip1.Items[0].Enabled = true;
                                    contextMenuStrip1.Items[1].Enabled = true;
                                    contextMenuStrip1.Items[2].Enabled = true;
                                    contextMenuStrip1.Items[3].Enabled = true;
                                }
                            }
                        }
                        WriteValue("Playlist", "LastPlaylist", openFileDialog1.FileName);
                        FilePlaylistToolStripMenuItem1.Text = openFileDialog1.FileName;
                        FilePlaylistToolStripMenuItem1.Enabled = true;
                    }
                }
                else if (e.KeyCode == Keys.S)
                {
                    saveFileDialog1.Filter = "Devil Playlist(*.DPP) | *.DPP";
                    saveFileDialog1.Title = "Save Playlist";
                    DialogResult result = saveFileDialog1.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        System.IO.StreamWriter SaveFile = new System.IO.StreamWriter(saveFileDialog1.FileName);
                        foreach (var item in list_music_path)
                        {
                            SaveFile.WriteLine(item);
                        }

                        SaveFile.Close();
                        if (language == "English")
                        {
                            MessageBox.Show("Playlist is Saved!", "Devil Player", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        }
                        else if (language == "Persian")
                        {
                            MessageBox.Show("لیست پخش ذخیره شد", "Devil Player", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        }
                    }
                }
                else if (e.KeyCode == Keys.R)
                {
                    if (listBox1.SelectedIndex != -1)
                    {
                        if (index_rightclick != listBox1.SelectedIndex)
                        {
                            list_music_path.RemoveAt(index_rightclick);
                            listBox1.Items.RemoveAt(index_rightclick);
                        }
                        else
                        {
                            output.Stop();
                            list_music_path.RemoveAt(index_rightclick);
                            listBox1.Items.RemoveAt(index_rightclick);
                            timer2.Enabled = true;
                            timer2.Start();
                            timer1.Enabled = false;
                            this.Text = "Devil Player";
                            progressBar1.Value = 0;
                        }
                    }
                }
                else if (e.KeyCode == Keys.H)
                {
                    if (language == "English")
                    {
                        if (FilePlaylistToolStripMenuItem1.Text != "Empty")
                            getSettingHistoryPlayList(FilePlaylistToolStripMenuItem1.Text);
                    }
                    else if (language == "Persian")
                    {
                        if (FilePlaylistToolStripMenuItem1.Text != "خالی")
                            getSettingHistoryPlayList(FilePlaylistToolStripMenuItem1.Text);
                    }
                }
            }
            catch
            {

            }

        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                        WriteValue("Settings", "Left", x.ToString());
                        WriteValue("Settings", "Top", y.ToString());
                        if (listBox1.Items.Count > 0)
                        {
                            WriteValue("List", "Index", listBox1.SelectedIndex.ToString());
                        }
                        if (File.Exists(Application.StartupPath + @"\History.HIS"))
                        {
                            File.Delete(Application.StartupPath + @"\History.HIS");
                            System.IO.StreamWriter SaveFile = new System.IO.StreamWriter(Application.StartupPath + @"\History.HIS");
                            foreach (var item in list_music_path)
                            {
                                SaveFile.WriteLine(item);
                            }

                            SaveFile.Close();
                        }
                        else
                        {
                            System.IO.StreamWriter SaveFile = new System.IO.StreamWriter(Application.StartupPath + @"\History.HIS");
                            foreach (var item in list_music_path)
                            {
                                SaveFile.WriteLine(item);
                            }

                            SaveFile.Close();
                        }                   

                Application.Exit();
            }
            catch
            {

            }

        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                String language = ReadValue("Settings", "Language");
                if (language == "Persian")
                {
                    MessageBox.Show("برنامه نویس: علیرضا محمدی \n\n ایمیل: cracker.crypt@gmail.com \n\n نوشته شده در مرداد 98", "Devil Player", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Programming by Alireza Mohammadi \n\n Email: cracker.crypt@gmail.com \n\n Copyright © August 2019", "Devil Player", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch
            {

            }

        }

        private void notifyIcon1_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.Button == MouseButtons.Left)
                    this.WindowState = FormWindowState.Normal;
            }
            catch
            {

            }


        }

        protected override void WndProc(ref Message message)
        {
            try
            {
                String move = ReadValue("Settings", "Move");
                if (move == "Off")
                {
                    switch (message.Msg)
                    {
                        case WM_SYSCOMMAND:
                            int command = message.WParam.ToInt32() & 0xfff0;
                            if (command == SC_MOVE)
                                return;
                            break;
                    }
                }
                base.WndProc(ref message);
            }
            catch
            {

            }


        }
        private void MainFrm_Move(object sender, EventArgs e)
        {
            try
            {
                if (this.WindowState == FormWindowState.Normal)
                {
                    x = this.Left;
                    y = this.Top;
                }
            }
            catch
            {

            }

        }

        private void englishToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                englishToolStripMenuItem.Checked = true;
                persianToolStripMenuItem.Checked = false;
                changeLanguage("English");
                WriteValue("Settings", "Language", "English");
            }
            catch
            {

            }

        }

        private void persianToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                persianToolStripMenuItem.Checked = true;
                englishToolStripMenuItem.Checked = false;
                changeLanguage("Persian");
                WriteValue("Settings", "Language", "Persian");
            }
            catch
            {

            }

        }

        private void onToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                onToolStripMenuItem.Checked = true;
                offToolStripMenuItem.Checked = false;
                WriteValue("Settings", "OnTop", "On");
                this.TopMost = true;
            }
            catch
            {

            }

        }

        private void offToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                onToolStripMenuItem.Checked = false;
                offToolStripMenuItem.Checked = true;
                WriteValue("Settings", "OnTop", "Off");
                this.TopMost = false;
            }
            catch
            {

            }
        }

        private void onToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                onToolStripMenuItem1.Checked = true;
                offToolStripMenuItem1.Checked = false;
                WriteValue("Settings", "Move", "On");
            }
            catch
            {

            }

        }

        private void offToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                onToolStripMenuItem1.Checked = false;
                offToolStripMenuItem1.Checked = true;
                WriteValue("Settings", "Move", "Off");
            }
            catch
            {

            }

        }

        private void changeLanguage(String language)
        {
            try
            {
                if (language == "English")
                {
                    playlistToolStripMenuItem.Text = "Play";
                    stopToolStripMenuItem.Text = "Stop";
                    previousToolStripMenuItem.Text = "Previous";
                    nextToolStripMenuItem.Text = "Next";
                    modeToolStripMenuItem.Text = "Mode";
                    shuffleToolStripMenuItem.Text = "Shuffle";
                    repeatAllToolStripMenuItem.Text = "Repeat All";
                    noRepeatToolStripMenuItem.Text = "No Repeat";
                    repeatToolStripMenuItem.Text = "Repeat";
                    addToolStripMenuItem.Text = "Add";
                    addMediaToolStripMenuItem.Text = "Add Media";
                    addDirectoryToolStripMenuItem.Text = "Add Directory";
                    playlistToolStripMenuItem.Text = "Playlist";
                    newPlaylistToolStripMenuItem.Text = "New Playlist";
                    loadPlaylistToolStripMenuItem.Text = "Load Playlist";
                    savePlaylistToolStripMenuItem.Text = "Save Playlist";
                    removeToolStripMenuItem.Text = "Remove";
                    recentToolStripMenuItem.Text = "History";
                    FilePlaylistToolStripMenuItem1.Text = "Empty";
                    clearHistoryToolStripMenuItem.Text = "Clear History";
                    searchToolStripMenuItem.Text = "Search";
                    languageToolStripMenuItem.Text = "Language";
                    englishToolStripMenuItem.Text = "English";
                    persianToolStripMenuItem.Text = "Persian";
                    opacityToolStripMenuItem.Text = "Opacity";
                    OnTopMenuItem3.Text = "On Top";
                    onToolStripMenuItem.Text = "On";
                    offToolStripMenuItem.Text = "Off";
                    movableToolStripMenuItem.Text = "Moveable";
                    onToolStripMenuItem1.Text = "On";
                    offToolStripMenuItem1.Text = "Off";
                    aboutToolStripMenuItem.Text = "About";
                    exitToolStripMenuItem.Text = "Exit";
                }
                else if (language == "Persian")
                {
                    playlistToolStripMenuItem.Text = "پخش";
                    stopToolStripMenuItem.Text = "توقف";
                    previousToolStripMenuItem.Text = "قبلی";
                    nextToolStripMenuItem.Text = "بعدی";
                    modeToolStripMenuItem.Text = "نوع پخش";
                    shuffleToolStripMenuItem.Text = "تصادفی";
                    repeatAllToolStripMenuItem.Text = "تکرار همه";
                    noRepeatToolStripMenuItem.Text = "بدون تکرار";
                    repeatToolStripMenuItem.Text = "تکرار";
                    addToolStripMenuItem.Text = "اضافه کردن";
                    addMediaToolStripMenuItem.Text = "اضافه کردن موسیقی";
                    addDirectoryToolStripMenuItem.Text = "اضافه کردن پوشه";
                    playlistToolStripMenuItem.Text = "لیست پخش";
                    newPlaylistToolStripMenuItem.Text = "لیست پخش جدید";
                    loadPlaylistToolStripMenuItem.Text = "باز کردن لیست پخش";
                    savePlaylistToolStripMenuItem.Text = "ذخیره لیست پخش";
                    removeToolStripMenuItem.Text = "حذف";
                    recentToolStripMenuItem.Text = "لیست پخش قبلی";
                    FilePlaylistToolStripMenuItem1.Text = "خالی";
                    clearHistoryToolStripMenuItem.Text = "پاک کردن لیست قبلی";
                    searchToolStripMenuItem.Text = "جستجو";
                    languageToolStripMenuItem.Text = "زبان";
                    englishToolStripMenuItem.Text = "انگلیسی";
                    persianToolStripMenuItem.Text = "پارسی";
                    opacityToolStripMenuItem.Text = "شفافیت";
                    OnTopMenuItem3.Text = "بالاترین اولویت";
                    onToolStripMenuItem.Text = "فعال";
                    offToolStripMenuItem.Text = "غیرفعال";
                    movableToolStripMenuItem.Text = "قابلیت جابجایی";
                    onToolStripMenuItem1.Text = "فعال";
                    offToolStripMenuItem1.Text = "غیر فعال";
                    aboutToolStripMenuItem.Text = "درباره";
                    exitToolStripMenuItem.Text = "خروج";
                }
            }
            catch
            {

            }

        }
    }

}
