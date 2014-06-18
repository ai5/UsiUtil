using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace USIConnect
{
    public partial class Form1 : Form
    {
        private USIServer server;

        public Form1()
        {
            this.InitializeComponent();
            this.server = new USIServer();

            this.server.AcceptEvent += this.server_AcceptEvent;
            this.server.ClosedEvent += this.server_ClosedEvent;

            AppLog.Initialize();
            AppLog.ErrorEvent += this.AppLog_ErrorEvent;
            AppLog.InfoEvent += this.AppLog_InfoEvent;
        }

        public void UpdateClientInfo()
        {
            listView1.Items.Clear();

            foreach (USIServerState state in this.server.States)
            {
                listView1.Items.Add(state.Info);
            }
        }

        private void 終了XToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void startStopButton_Click(object sender, EventArgs e)
        {
            if (this.server.Started)
            {
                this.server.End();
                this.textBox1.AppendText(DateTime.Now.ToShortTimeString() + ":停止\n");
            }
            else
            {
                this.server.Start((int)portNumericUpDown.Value);
                this.textBox1.AppendText(DateTime.Now.ToShortTimeString() + ":開始\n");
            }

            if (this.server.Started)
            {
                portNumericUpDown.Enabled = false;
                startStopButton.Text = "停止";
            }
            else
            {
                portNumericUpDown.Enabled = true;
                startStopButton.Text = "開始";
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.server.End();
        }

        private void server_ClosedEvent(object sender, USIServerEventArgs e)
        {
            this.UpdateClientInfo();
        }

        private void server_AcceptEvent(object sender, USIServerEventArgs e)
        {
            this.UpdateClientInfo();
        }

        private void AppLog_InfoEvent(object sender, AppLogEventArgs e)
        {
            if (!this.textBox1.IsDisposed)
            {
                this.textBox1.AppendText(DateTime.Now.ToShortTimeString());
                this.textBox1.AppendText(":");
                this.textBox1.AppendText(e.Message);
                this.textBox1.AppendText("\n");
            }
        }

        private void AppLog_ErrorEvent(object sender, AppLogEventArgs e)
        {
            if (!this.textBox1.IsDisposed)
            {
                this.textBox1.AppendText(DateTime.Now.ToShortTimeString());
                this.textBox1.AppendText(":error:");
                this.textBox1.AppendText(e.Message);
                this.textBox1.AppendText("\n");
            }
        }
    }
}
