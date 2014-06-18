using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace USIConnect
{
    public class USIServer
    {
        private bool start = false;
        private Socket listener;
        private object synclock = new object();
        private SynchronizationContext syncContext;
        private List<USIServerState> states;

        // コールバック
        public event USIServerEventHandler AcceptEvent;

        public event USIServerEventHandler ClosedEvent;

        public USIServer()
        {
            this.syncContext = SynchronizationContext.Current;
            if (this.syncContext == null)
            {
                this.syncContext = new SynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(this.syncContext);
            }

            this.states = new List<USIServerState>();
        }

        public bool Started
        {
            get { return this.start; }
        }

        public List<USIServerState> States
        {
            get { return this.states; }
        }

        /// <summary>
        /// 開始
        /// </summary>
        /// <param name="portno"></param>
        public void Start(int port)
        {
#if false
            IPHostEntry hostInfo = Dns.GetHostEntry("localhost");

            IPAddress ipaddress = hostInfo.AddressList[0];

            foreach (IPAddress ipadr in hostInfo.AddressList)
            {
                if (ipadr.AddressFamily == AddressFamily.InterNetwork)
                {
                    ipaddress = ipadr;
                    break;
                }
            }

            IPEndPoint endpoint = new IPEndPoint(ipaddress, port);
#else
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, port);
#endif
            this.listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            this.listener.Bind(endpoint);
            this.listener.Listen(4);

            this.start = true;

            this.listener.BeginAccept(new AsyncCallback(this.AcceptCallback), this.listener);
        }

        /// <summary>
        /// 終了
        /// </summary>
        public void End()
        {
            if (this.start)
            {
                this.start = false;

                // all done
                this.AllDone();

                // wait all done
                this.WaitAllDone();

                // 閉じて大丈夫？
                this.listener.Close();
                this.listener = null;
            }
        }

        private void AllDone()
        {
            foreach (USIServerState state in this.states)
            {
                state.Stop();
            }
        }

        private void WaitAllDone()
        {
            // いい待ち方ない？
            foreach (USIServerState state in this.states)
            {
                state.WaitStop();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        private void OnAccept(USIServerEventArgs e)
        {
            if (this.AcceptEvent != null)
            {
                this.AcceptEvent(this, e);
            }
        }

        /// <summary>
        /// 切断
        /// </summary>
        /// <param name="e"></param>
        private void OnClosed(USIServerEventArgs e)
        {
            if (this.ClosedEvent != null)
            {
                this.ClosedEvent(this, e);
            }
        }

        /// <summary>
        /// Acceptコールバック
        /// </summary>
        /// <param name="ar"></param>
        private void AcceptCallback(IAsyncResult ar)
        {
            Socket listener = (Socket)ar.AsyncState;
            Socket handler;

            try
            {
                handler = listener.EndAccept(ar);
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            USIServerState state = new USIServerState(handler);

            state.ClosedEvent += this.UsiServerState_Closed;

            this.syncContext.Post(
                s =>
                {
                    this.states.Add(state);
                    this.OnAccept(new USIServerEventArgs(state));
                },
                null);

            state.Start();

            if (this.start)
            {
                listener.BeginAccept(new AsyncCallback(this.AcceptCallback), listener);
            }
        }

        private void UsiServerState_Closed(object sender, USIServerEventArgs e)
        {
            this.syncContext.Post(
                s =>
                {
                    this.states.Remove(e.State);
                    this.OnClosed(e);
                },
                null);
        }
    }

    public class USIServerEventArgs : EventArgs
    {
        public USIServerState State;

        public USIServerEventArgs(USIServerState state)
        {
            this.State = state;
        }
    }

    public delegate void USIServerEventHandler(object sender, USIServerEventArgs e);
}
