using EasySave.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasySave.Model
{
    class DistantConsoleServer
    {
        private readonly int inPortNumber = 1337;
        private readonly object _lock = new object();
        private readonly Dictionary<int, TcpClient> list_clients = new Dictionary<int, TcpClient>();
        IMainController controller;

        TcpListener ServerSocket;
        private int count = 1;

        public DistantConsoleServer(IMainController contr)
        {
            controller = contr;

            var host = Dns.GetHostEntry(Dns.GetHostName());

            //listening TCP connexionsd
            ServerSocket = new TcpListener(IPAddress.Any, inPortNumber);
            ServerSocket.Start();
        }

        public void RunServer()
        {
            while (true)
            {
                //accept connexion asking
                TcpClient client = ServerSocket.AcceptTcpClient();
                lock (_lock) list_clients.Add(count, client);

                Thread t = new Thread(get_clients);
                t.Start(count);
                count++;
            }
        }

        public void get_clients(object o)
        {
            int id = (int)o;
            TcpClient client;

            lock (_lock) client = list_clients[id];

            while (true)
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];
                int byte_count = stream.Read(buffer, 0, buffer.Length);
                if (byte_count == 0)
                {
                    break;
                }

                string data = Encoding.ASCII.GetString(buffer, 0, byte_count);
                if (data.Contains("UpdateSaveList"))
                {
                    SendTo(SaveNameList(), client);
                }
                else
                {
                    SendTo(GetValue(data), client);
                }
            }

            lock (_lock) list_clients.Remove(id);
            client.Client.Shutdown(SocketShutdown.Both);
            client.Close();
        }

        public void SendTo(string data, TcpClient client)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(data + Environment.NewLine);

            lock (_lock)
            {
               NetworkStream stream = client.GetStream();
               stream.Write(buffer, 0, buffer.Length);
            }
        }



        private string SaveNameList()
        {
            string ret = "";
            if (controller.backup.Count > 0)
            {
                foreach (IBackup backup in controller.backup)
                {
                    ret += backup.name + ",";
                }
                ret = ret.Substring(0, ret.Length-1);
            }
            return ret;
        }

        private string GetValue(string name)
        {
            string ret = "";
            foreach(IBackup backup in controller.backup)
            {
                if (name.Contains(backup.name)){
                    string progressBar = Utils.JsonReader(backup.target_folder + "/realtime_log_"+backup.name+".json", "backup_progress");

                    ret = backup.name + "," + backup.source_folder + "," + backup.target_folder + "," + progressBar;
                }
            }
            return ret;
        }
    }
}