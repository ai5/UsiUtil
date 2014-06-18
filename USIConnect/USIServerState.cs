using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace USIConnect
{
    public class USIServerState
    {
        private Socket handler;
        private ManualResetEvent mreset;
        private bool stop = false;

        public event USIServerEventHandler ClosedEvent;

        public USIServerState(Socket handler)
        {
            this.handler = handler;
            this.mreset = new ManualResetEvent(true);
        }

        public string Info
        {
            get { return (string)((IPEndPoint)this.handler.RemoteEndPoint).Address.ToString(); }
        }

        /// <summary>
        /// 開始
        /// </summary>
        public void Start()
        {
            this.stop = false;
            this.mreset.Reset();
            ThreadPool.QueueUserWorkItem(this.DoWork, this);
        }

        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            // 排他必要？           
            this.stop = true;
        }

        public void WaitStop()
        {
            this.mreset.WaitOne(10 * 1000); // 時間は暫定
        }

        private void OnClosed(USIServerEventArgs e)
        {
            if (this.ClosedEvent != null)
            {
                this.ClosedEvent(this, e);
            }
        }

        /// <summary>
        /// 処理
        /// </summary>
        /// <param name="state"></param>
        private void DoWork(object state)
        {
            UsiEngine engine = null;  
            bool connect = false;

            AppLog.Info(this.Info + ": start");

            using (Socket socket = this.handler)
            {
                using (NetworkStream ns = new NetworkStream(socket))
                {
                    ns.ReadTimeout = 1000; // 1秒

                    using (StreamReader sr = new StreamReader(ns, Encoding.UTF8))
                    {                       
                        try
                        {
                            while (true)
                            {
                                string cmd = null;

                                try
                                {
                                    cmd = sr.ReadLine();
                                }
                                catch (IOException)
                                {
                                    // タイムアウト 停止のうまい案がないのでタイムアウトで停止を見る
                                    if (this.stop)
                                    {
                                        cmd = null;
                                    }
                                    else
                                    {
                                        cmd = string.Empty;
                                    }
                                }
  
                                if (cmd == null)
                                {
                                    break;
                                }
                                else if (string.IsNullOrEmpty(cmd))
                                {
                                }
                                else
                                {
                                    if (connect == false)
                                    {
                                        AppLog.Info(this.Info + ": " + cmd);

                                        if (cmd.StartsWith("connect"))
                                        {
                                            Tokenizer tok = new Tokenizer(cmd);

                                            tok.Token();
                                            string type = tok.Token();

                                            if (type == "engine")
                                            {
                                                string path = tok.Token();

                                                try
                                                {
                                                    engine = new UsiEngine();
                                                    engine.RecievedEvent += this.UsiRecievedEvent;
                                                    engine.Connect(path);
                                                    connect = true;
                                                }
                                                catch (Exception e)
                                                {
                                                    if (engine != null)
                                                    {
                                                        engine.Dispose();
                                                        engine = null;
                                                    }

                                                    AppLog.Error(e.Message);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        engine.Send(cmd);
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            AppLog.Error(e.Message);
                        }
                        finally
                        {
                            if (engine != null)
                            {
                                engine.Disconnect();
                                engine.Dispose();
                                engine.RecievedEvent -= this.UsiRecievedEvent;
                            }
                        }
                    }

                    AppLog.Info(this.Info + ": end");
                }
            }

            this.mreset.Set();

            this.OnClosed(new USIServerEventArgs(this));
        }

        /// <summary>
        /// 受信処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UsiRecievedEvent(object sender, USIRecievedEventArgs e)
        {
            byte[] response = Encoding.UTF8.GetBytes(e.Response + "\n");

            this.handler.Send(response);
        }
    }
}
