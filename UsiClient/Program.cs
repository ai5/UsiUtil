using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UsiClient
{
    public class Program
    {
        private static int port = 53556;
        private static string hostname = "localhost";
        private static string exepath = "gpsfish/gpsfish.exe";

        private static void Main(string[] args)
        {
            LoadSettings();
            ParseArgs(args);

            UsiClient client;

            client = new UsiClient();

            try
            {
                client.Connect(hostname, port, exepath);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
            }

            string cmd = null;

            while (cmd != "quit")
            {
                cmd = Console.ReadLine();

                if (cmd == null)
                {
                    cmd = "quit";
                }

                if (client.IsConnected())
                {
                    // 接続している場合
                    client.Send(cmd);
                }
                else
                {
                    if (cmd == "usi")
                    {
                        Console.WriteLine("option name HostName type string default {0}", hostname);
                        Console.WriteLine("option name Port type spin default {0} min 0 max 65535", port);
                        Console.WriteLine("option name ExePath type string default {0}", exepath);

                        Console.WriteLine("usiok");
                    }
                    else if (cmd.StartsWith("setoption"))
                    {
                        string[] str_array = cmd.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        if (str_array.Length >= 4)
                        {
                            try
                            {
                                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                                config.AppSettings.Settings[str_array[2]].Value = str_array[4];
                                config.Save(ConfigurationSaveMode.Modified);
                                ConfigurationManager.RefreshSection("appSettings");
                            }
                            catch (Exception e)
                            {
                                Console.Error.WriteLine(e.Message);
                            }
                        }
                    }
                }
            }

            client.Disconnect();
        }

        /// <summary>
        /// 設定読み込み
        /// </summary>
        private static void LoadSettings()
        {
            string str = ConfigurationManager.AppSettings["HostName"];
            if (str != null)
            {
                hostname = str;
            }

            str = ConfigurationManager.AppSettings["Port"];
            if (str != null)
            {
                int num;

                if (int.TryParse(str, out num))
                {
                    // ok
                    port = num;
                }
            }

            str = ConfigurationManager.AppSettings["ExePath"];
            if (str != null)
            {
                exepath = str;
            }
        }

        /// <summary>
        /// 引数パース
        /// </summary>
        /// <param name="args"></param>
        private static void ParseArgs(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                string str = args[i];

                if (str == "-h")
                {
                    if ((i + 1) < args.Length)
                    {
                        hostname = args[i + 1];
                        i++;
                    }
                }
                else if (str == "-p")
                {
                    if ((i + 1) < args.Length)
                    {
                        int num;

                        if (int.TryParse(args[i + 1], out num))
                        {
                            // ok
                            port = num;
                        }

                        i++;
                    }                   
                }
                else if (str[0] == '-')
                {
                }
                else
                {
                    exepath = str;
                }
            }         
        }
    }
}
