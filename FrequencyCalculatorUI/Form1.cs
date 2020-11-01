using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WordProcessor;

namespace FrequencyCalculatorUI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            backgroundWorker1.DoWork += BackgroundWorker1_DoWork;
            backgroundWorker1.ProgressChanged += BackgroundWorker1_ProgressChanged;
            backgroundWorker1.RunWorkerCompleted += BackgroundWorker1_RunWorkerCompleted;
        }
        Uri link;
        private void Button1_Click(object sender, EventArgs e) => ButtonClick(1);

        private void ButtonClick(object arg)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text))
                return;
            try
            {
                link = new Uri(textBox1.Text);
                ToggleWorking(true);
                backgroundWorker1.RunWorkerAsync(arg);
            }
            catch
            {
                MessageBox.Show("Invalid url", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                backgroundWorker1.CancelAsync();
            }
        }

        private void BackgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar1.Value = 100;
            ToggleWorking(false);
        }

        private void BackgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
            if (e.ProgressPercentage <= 90)
                richTextBox1.Text = e.UserState?.ToString();
            else if (e.ProgressPercentage == 95)
                BindList(e.UserState as IEnumerable<PhraseFrequency>);
            else if (e.ProgressPercentage == 100)
                label2.Text = $"Time taken: {e.UserState} seconds";
        }

        private void BackgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            var startTime = DateTime.Now;
            var fc = new FrequencyCalculator();
            var depth = Convert.ToByte(numericUpDown1.Value);
            var downloader = new Downloader(depth);
            var t = downloader.GetTextFromUrlAsync(link);
            while(t.Status == TaskStatus.Running || t.Status == TaskStatus.WaitingForActivation)
            {
                var progress = Math.Round((90.0f * downloader.PresentLevel) / depth);
                if (progress >= 90)
                    progress = 89;
                backgroundWorker1.ReportProgress(Convert.ToInt32(progress));
                Thread.Sleep(1000);
            }
            var content = t.Result;
            //var content = new Downloader(depth).GetTextFromUrl(link);
            backgroundWorker1.ReportProgress(90, content);
            IEnumerable<PhraseFrequency> res;
            if (e.Argument.ToString() == "1")
                res = fc.CalculateWordFrequency(content, 10);
            else
                res = fc.CalculateWordPairFrequency(content, 10);
            backgroundWorker1.ReportProgress(95, res);
            backgroundWorker1.ReportProgress(100, DateTime.Now.Subtract(startTime).TotalSeconds);
        }

        private void BindList(IEnumerable<PhraseFrequency> res)
        {
            foreach (var w in res)
                listBox1.Items.Add($"{w.Word} ({w.Frequency})\r\n");
        }

        private void ToggleWorking(bool working)
        {
            textBox1.Enabled = !working;
            button1.Enabled = !working;
            button2.Enabled = !working;
            numericUpDown1.Enabled = !working;
            richTextBox1.Enabled = !working;
            if (working)
            {
                progressBar1.Value = 0;
                label2.Text = string.Empty;
                listBox1.Items.Clear();
                richTextBox1.Text = string.Empty;
            }
            listBox1.Enabled = !working;
        }

        private void Button2_Click(object sender, EventArgs e) => ButtonClick(2);
    }
}