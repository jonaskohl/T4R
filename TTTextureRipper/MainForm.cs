using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TTTextureRipper
{
    public partial class MainForm : Form
    {
        NxgTexturesFile currentFile;

        string currentFileName;
        FileStream currentFs;

        public MainForm()
        {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                OpenFileBF(openFileDialog1.FileName);
            }
        }

        internal void Log(string line)
        {
            textBox1.AppendText(line + Environment.NewLine);
            //textBox1.Update();
            Application.DoEvents();
        }

        private void OpenFileBF(string fileName)
        {
            if (currentFs != null)
            {
                currentFs.Dispose();
                currentFs = null;
            }
            currentFs = File.OpenRead(fileName);
            currentFileName = fileName;
            toolStripStatusLabel1.Text = "Opened file " + fileName;
        }

        private void OpenFile(string fileName)
        {
            if (currentFile != null)
            {
                currentFile.Dispose();
                currentFile = null;
            }
            currentFile = NxgTexturesFile.Open(fileName);
            currentFileName = fileName;
            toolStripStatusLabel1.Text = "Opened file " + fileName;
        }

        private void extractDDSFilesToToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var d = new VistaFolderBrowserDialog();
            if (d.ShowDialog())
            {
                ExtractFilesTo(d.SelectedPath);
            }
        }

        private void ExtractFilesTo(string selectedPath)
        {
            var sw = new Stopwatch();
            sw.Start();
            menuStrip1.Enabled = false;
            var baseName = Path.GetFileNameWithoutExtension(currentFileName);
            foreach (var entry in BruteForceDDSFinder.Find(currentFs))
            {
                var outputPath = Path.Combine(selectedPath, baseName + "_" + entry.Item1.ToString("X16") + ".dds");
                File.WriteAllBytes(outputPath, entry.Item3);
                Application.DoEvents();
            }
            sw.Stop();
            MessageBox.Show("Done!\r\nTook: " + sw.Elapsed.TotalSeconds + " seconds", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            menuStrip1.Enabled = true;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var d = new AboutBox1())
                d.ShowDialog(this);
        }
    }
}
