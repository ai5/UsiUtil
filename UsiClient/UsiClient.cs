using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UsiClient
{
    public class UsiClient
    {
        private Socket client;
        private bool connect;
        private byte[] buffer = new byte[1024]; 

        public UsiClient()
        {
        }

        /// <summary>
        /// 接続
        /// 
        /// ※接続失敗は例外発生
        /// </summary>
        public void Connect(string host, int port, string path)
        {
            this.client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            this.client.Connect(host, port);

            this.connect = true;
            this.Send("connect engine " + path);

            this.ReceiveStart();
        }

        /// <summary>
        /// 切断
        /// </summary>
        public void Disconnect()
        {
            if (this.connect)
            {
                this.connect = false;
                this.client.Shutdown(SocketShutdown.Both);
            }

            if (this.client != null)
            {
                this.client.Close();
                this.client = null;
            }
        }

        public bool IsConnected()
        {
            return this.connect;
        }

        /// <summary>
        /// 送信
        /// </summary>
        /// <param name="str"></param>
        public void Send(string str)
        {
            try
            {
                byte[] send = Encoding.UTF8.GetBytes(str + "\n");

                this.client.Send(send);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());
            }
        }

        /// <summary>
        /// 受信開始
        /// </summary>
        private void ReceiveStart()
        {
            try
            {
                // Begin receiving the data from the remote device.
                this.client.BeginReceive(this.buffer, 0, this.buffer.Length, 0, new AsyncCallback(this.ReceiveCallback), this.client);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());
            }
        }

        /// <summary>
        /// 受信コールバック
        /// </summary>
        /// <param name="ar"></param>
        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket 
                // from the asynchronous state object.
                Socket client = (Socket)ar.AsyncState;

                // Read data from the remote device.
                int size = client.EndReceive(ar);

                if (size > 0)
                {
                    // There might be more data, so store the data received so far.
                    Console.Write(Encoding.UTF8.GetString(this.buffer, 0, size));

                    // Get the rest of the data.
                    client.BeginReceive(this.buffer, 0, this.buffer.Length, 0, new AsyncCallback(this.ReceiveCallback), client);
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());
            }
        }
    }
}
