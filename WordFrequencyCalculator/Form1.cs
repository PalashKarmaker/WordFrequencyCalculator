using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using WordProcessor;

namespace WordFrequencyCalculator
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
        private void button1_Click(object sender, EventArgs e) => ButtonClick(1);

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
            if (e.ProgressPercentage == 50)
                richTextBox1.Text = e.UserState.ToString();
            else if (e.ProgressPercentage == 75)
                BindList(e.UserState as IEnumerable<PhraseFrequency>);
            else if (e.ProgressPercentage == 100)
                label2.Text = $"Time taken: {e.UserState} seconds";
        }

        private void BackgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            var startTime = DateTime.Now;
            var fc = new FrequencyCalculator();
            if (!byte.TryParse(textBox2.Text, out byte depth))
                depth = 4;
            var content = new Utility(depth).GetTextFromUrlAsync(link).Result;
            //var content = Utility.GetTextFromUrl(link);
            backgroundWorker1.ReportProgress(50, content);
            IEnumerable<PhraseFrequency> res;
            if (e.Argument.ToString() == "1")
                res = fc.CalculateWordFrequence(content, 10);
            else
                res = fc.CalculateWordPairFrequency(content, 10);
            backgroundWorker1.ReportProgress(75, res);
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

        private void button2_Click(object sender, EventArgs e) => ButtonClick(2);
    }
}