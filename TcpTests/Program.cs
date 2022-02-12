#define HAS_WIFI

using System;
using System.Diagnostics;
using System.Threading;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using nanoFramework.Networking;

namespace TcpTests
{
    public class Program
    {
        public static void Main()
        {
           // string MySsid = "MySsid";
           // string MyPassword = "MyPassword";
            string MySsid = "Home";
            string MyPassword = "Daniel457#Kaikop";
            int clientID = 0;
            string ipaddress;

            Debug.WriteLine("Hello from nanoFramework TcpClient test!");

            bool success;
            CancellationTokenSource cs = new(60000);
#if HAS_WIFI
            success = WiFiNetworkHelper.ConnectDhcp(MySsid, MyPassword, requiresDateTime: true, token: cs.Token);
            ipaddress = NetworkInterface.GetAllNetworkInterfaces()[0].IPv4Address;
#else
            success = NetworkHelper.SetupAndConnectNetwork(cs.Token, true);
            ipaddress = NetworkInterface.GetAllNetworkInterfaces()[2].IPv4Address;
#endif
            if (!success)
            {
                Debug.WriteLine($"{DateTime.UtcNow} Can't get a proper IP address and DateTime, error: {NetworkHelper.Status}.");
                if (NetworkHelper.HelperException != null)
                {
                    Debug.WriteLine($"ex: {NetworkHelper.HelperException}");
                }

                return;
            }

            Debug.WriteLine($"{DateTime.UtcNow} Network connected as {ipaddress}");

            TcpListener listener = new TcpListener(IPAddress.Any, 1234);

            listener.Start(4);

            while (true)
            {
                try
                {
                    TcpClient client = listener.AcceptTcpClient();

                    Thread Worker = new Thread(() => WorkerThread(client, clientID++) );
                    Worker.Start();
                }
                catch(Exception ex)
                {
                    Debug.WriteLine($"Exception:-{ex.Message}");
                    Thread.Sleep(500);
                }
            }
        }

        private static void WorkerThread(TcpClient client, int clientID)
        {
            Debug.WriteLine($" {clientID}:Client connected to {client.Client.RemoteEndPoint.ToString()}");
            try
            {
                byte[] buffer = new byte[256];
                NetworkStream stream = client.GetStream();

                // test time outs
                client.ReceiveTimeout = 10000;
                client.SendTimeout = 60000;

                Debug.WriteLine($"Rx timeout:{client.ReceiveTimeout} Tx timeout:{client.SendTimeout}");
                
                // Reset to defaults ( no timeout)
                //stream.ReadTimeout = 0;
                //stream.WriteTimeout = 0;

                // Netstream.Read will return straight away if there is no data to read 
                while (true)
                {
                    // Wait for first byte
                    // This will sit on Read until 1 byte of data available giving time to other threads
                    int bytesRead = stream.Read(buffer, 0, 1);

                    // Then read the rest of data
                    int dataAv = client.Available;
                    
                    // Restrict to buffer size
                    if (dataAv > buffer.Length)
                        dataAv = buffer.Length;

                    if (dataAv> 0)
                        bytesRead += stream.Read(buffer, 1, dataAv);

                    if (bytesRead>0)
                    {
                        Debug.WriteLine($"{clientID}:Client data {bytesRead} bytes, available {dataAv}");

                        // Echo back
                        stream.Write(buffer, 0, bytesRead);
                    }
                }
            }
            catch (Exception) { }

            client.Close();

            Debug.WriteLine($"{clientID}:Client closed");
        }
    }
}
