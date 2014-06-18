using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace USIConnect
{
    /// <summary>
    /// INFO通知のパラメータ
    /// </summary>
    public class USIRecievedEventArgs : EventArgs
    {
        public string Response;

        public USIRecievedEventArgs(string res)
        {
            this.Response = res;
        }
    }

    public delegate void USIReceivedEventHandler(object sender, USIRecievedEventArgs e);

    public class UsiEngine : IDisposable
    {
        private const int ProcessExitWaitMs = 10000; // Exitするときの最大待ち 10秒値は暫定
        private Process engineProcess = null;
        private bool connected = false;
        private SynchronizationContext syncContext; // 同期用

        public event USIReceivedEventHandler RecievedEvent;

        public UsiEngine()
        {
            this.syncContext = SynchronizationContext.Current;

            if (this.syncContext == null)
            {
                this.syncContext = new SynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(this.syncContext);
            }
        }

        // 接続
        public void Connect(string path)
        {
            if (!System.IO.Path.IsPathRooted(path))
            {
                // 相対パスなら絶対パスに直す
                path = Path.GetDirectoryName(Application.ExecutablePath) + Path.DirectorySeparatorChar + path;
            }

            this.engineProcess = new Process();

            this.engineProcess.StartInfo.FileName = path;
#if DEBUG
            this.engineProcess.StartInfo.CreateNoWindow = false;
#else
            this.engineProcess.StartInfo.CreateNoWindow = true;
#endif
            this.engineProcess.StartInfo.UseShellExecute = false;  // リダイレクトする場合はfalse
            this.engineProcess.StartInfo.RedirectStandardInput = true;
            this.engineProcess.StartInfo.RedirectStandardOutput = true;
            this.engineProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(path);

            this.engineProcess.OutputDataReceived += this.process_DataRecieved;

            this.engineProcess.EnableRaisingEvents = true;  // プロセス終了を受け取るためにtrueにする
            this.engineProcess.Exited += this.process_Exited;    // 終了イベント

            this.engineProcess.Start();
            this.engineProcess.BeginOutputReadLine();

            this.connected = true;
        }

        /// <summary>
        /// 切断
        /// </summary>
        public void Disconnect()
        {
            if (this.connected)
            {
                this.Send("quit");
                this.connected = false;

                if (!this.engineProcess.WaitForExit(ProcessExitWaitMs))
                {
                    try
                    {
                        this.engineProcess.Kill(); // 強制終了させる
                    }
                    catch
                    {
                    }
                }

                this.engineProcess.Exited -= this.process_Exited;    // イベント削除 必要？

                this.engineProcess.Dispose();
                this.engineProcess = null;             
            }
        }

        // 送信
        public void Send(string str)
        {
            if (this.connected)
            {
                this.engineProcess.StandardInput.WriteLine(str);
                this.engineProcess.StandardInput.Flush();
            }
        }
       
        public void Dispose()
        {
            try
            {
                if (this.engineProcess != null)
                {
                    this.engineProcess.Dispose();
                    this.engineProcess = null;
                }
            }
            catch
            {
            }
        }

        private void OnRecieved(USIRecievedEventArgs e)
        {
            if (this.RecievedEvent != null)
            {
                this.RecievedEvent(this, e);
            }
        }

        /// <summary>
        /// データの非同期受信
        /// </summary>
        /// <param name="sendingProcess"></param>
        /// <param name="outLine"></param>
        private void process_DataRecieved(object sender, DataReceivedEventArgs outLine)
        {
            if (!string.IsNullOrEmpty(outLine.Data))
            {
                // コールバック呼び出し
                // Console.WriteLine(outLine.Data);
                this.syncContext.Post(
                    state =>
                    {
                        this.OnRecieved(new USIRecievedEventArgs(outLine.Data));
                    },
                    null);
            }
        }

        /// <summary>
        /// プロセス終了時のイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void process_Exited(object sender, EventArgs e)
        {
        }
    }
}
