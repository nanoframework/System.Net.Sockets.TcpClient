//
// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//
//

using nanoFramework.TestFramework;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace NFUnitTests
{
    [TestClass]
    public class ConnectionTests
    {
        const int TESTPORT = 1234;
        static byte[] _testData = new byte[256];

        [Setup]
        public void Initialize()
        {
            // Remove this line to run hardware tests
            Assert.SkipTest("No Network");

            Debug.WriteLine("TcpListener.TcpClient connection Tests initialized.");

            // Init test data
            for (int i = 0; i < _testData.Length; i++)
                _testData[i] = (byte)i;
        }

        [TestMethod]
        public void ListenAndConnect()
        {
            const int MaxConnections = 4;

            bool testRunning = true;
            int workerID = 1;
            int connectionAccepted = 0;

            Thread[] workers = new Thread[MaxConnections];

            // Create listener on Loopback addres
            TcpListener listener = CreateListener();

            listener.Start(4);

            // Start some Threads to connect to Listener and Send/Receive data
            for (int sndCount = 0; sndCount < MaxConnections; sndCount++)
            {
                Thread Sender1 = new Thread(() => SenderThread(100 + workerID++));
                Sender1.Start();
            }

            while (testRunning)
            {
                // All connections accepted, break accept loop
                if (connectionAccepted >= MaxConnections)
                    break;

                try
                {
                    TcpClient client = listener.AcceptTcpClient();

                    workers[connectionAccepted] = new Thread(() => WorkerThread(client, workerID++));
                    workers[connectionAccepted].Start();

                    connectionAccepted++;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Exception:-{ex.Message}");
                    Thread.Sleep(500);
                }
            }

            listener.Stop();
            Debug.WriteLine($"listener stopped");

            Debug.WriteLine($"Waiting for Workers to end");

            bool WorkersRunning = true;
            while (WorkersRunning)
            {
                // Wait for all Worker threads to close
                WorkersRunning = false;
                foreach (Thread thd in workers)
                {
                    if (thd.IsAlive)
                    {
                        WorkersRunning = true;
                        break;
                    }
                }
                Thread.Sleep(10);
            }
            Debug.WriteLine($"All workers ended");
        }

        public static void SenderThread(int workerID)
        {
            // IPV4 address
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, TESTPORT);

            TcpClient sender = new TcpClient();

            sender.Connect(endPoint);

            NetworkStream stream = sender.GetStream();

            for (int count = 1; count < 20; count++)
            {
                int length = count * 2;
                ///Debug.WriteLine($"{workerID}: Sender count:{count} write len:{length} + 3 hdr");
                
                // Write header
                stream.WriteByte(0xfc);
                stream.WriteByte((byte)(length & 0xff)); // Low length
                stream.WriteByte((byte)((length >> 8) & 0xff)); // High length

                byte[] buffer = new byte[length];

                // fill buffer with test data
                Array.Copy(_testData, count, buffer, 0, length);

                stream.Write(buffer, 0, length);

                Array.Clear(buffer, 0, length);

                int sync = stream.ReadByte();
                Assert.True(sync == 0xfc, $"{workerID} count={count} read sync != 0xfc => {sync}");
                int lenL = stream.ReadByte();
                int lenH = stream.ReadByte();
                int dataLength = (lenH << 8) + lenL;
                Assert.True(dataLength == length, $"{workerID} Sender rx invalid length {dataLength} should be {length}");

                int readBytes = stream.Read(buffer, 0, length);
                Assert.True(readBytes == length, $"{workerID} Read bytes:{readBytes} <> requested length:{length}");
                //Debug.WriteLine($"{workerID}: Read len:{readBytes}");

                // Validate buffer
                for (int i = 0; i < length; i++)
                {
                    Assert.False(buffer[i] != _testData[i + count], $"Received data not same as send, position:{i} {buffer[i]}!={_testData[i + count]}");
                }
                
                Debug.WriteLine($"{workerID}: Send/Receive count:{count} complete");
            }

            stream.Close();
            sender.Dispose();
        }


        // Thread to echo back data received
        // Data in format 0xFC, length:uint16, data bytes ....... 

        public static void WorkerThread(TcpClient client, int workerID)
        {
            Debug.WriteLine($" {workerID}:Client connected to {client.Client.RemoteEndPoint.ToString()}");

            NetworkStream stream = client.GetStream();

            // Set RX time outs on stream
            stream.ReadTimeout = 10000;

            //Debug.WriteLine($"{workerID}: Log time outs, Read time out:{stream.ReadTimeout} Write time out:{stream.WriteTimeout}");

            // Netstream.Read will return straight away if there is no data to read 
            while (true)
            {
                try
                {
                    // Wait for first byte
                    // This will sit on Read until 1 byte of data available giving time to other threads
                    int syncbyte = stream.ReadByte();
                    if (syncbyte == -1)
                    {
                        Debug.WriteLine($"{workerID}:Connection closed");
                        break;
                    }
                    //Debug.WriteLine($"{workerID}:Read sync");
                    Assert.True(syncbyte == 0xfc, $"{workerID}:Sync byte != FC => {syncbyte}");

                    int lenL = stream.ReadByte();
                    //Debug.WriteLine($"{workerID}:Read lenL {lenL}");
                    int lenH = stream.ReadByte();
                    //Debug.WriteLine($"{workerID}:Read lenH {lenH}");
                    int dataLength = (lenH << 8) + lenL;
                    Assert.True(dataLength <= 512, $"{workerID}:Invalid length {dataLength}");
                    //Debug.WriteLine($"{workerID}:Data length {dataLength}");

                    byte[] buffer = new byte[dataLength];

                    // Then read the rest of data in loop
                    int bufferPos = 0;
                    int bytesToRead = 0;

                    while (bytesToRead < dataLength)
                    {
                        int dataAv = client.Available;
                        if (dataAv > 0)
                        {
                            //Debug.WriteLine($"{workerID}:Read data len {dataAv} pos {bufferPos}");
                            int bytesRead = stream.Read(buffer, bufferPos, dataAv);
                            bufferPos += bytesRead;
                            bytesToRead += bytesRead;
                            //Debug.WriteLine($"{workerID}:Data Read bytes {bytesRead}");
                        }
                        else
                            Thread.Sleep(0);
                    }

                    Assert.True(bytesToRead == dataLength, $"{workerID}:Data read != availableInvalid length {dataLength}");

                    //Debug.WriteLine($"{workerID}:Data {bytesToRead} bytes RX, echo back");

                    stream.WriteByte(0xfc);
                    stream.WriteByte((byte)(dataLength & 0xff)); // Low length
                    stream.WriteByte((byte)((dataLength >> 8) & 0xff)); // High length

                    // Echo data back
                    stream.Write(buffer, 0, dataLength);
                    //Debug.WriteLine($"{workerID}:Data sent");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{workerID}:Worker exception {ex.Message}");
                    break;
                }

            } // while

            client.Close();
            Debug.WriteLine($"{workerID}:Worker closed");
        }



        public TcpListener CreateListener()
        {
            TcpListener listener = new TcpListener(IPAddress.Loopback, TESTPORT);
            Assert.NotNull(listener);

            Debug.WriteLine("TcpListener created.");

            Assert.Equal(((IPEndPoint)listener.LocalEndpoint).Port, TESTPORT, "Wrong port on Listener");
            Assert.Equal((int)((IPEndPoint)listener.LocalEndpoint).AddressFamily, (int)AddressFamily.InterNetwork, "Wrong address family");
            Assert.True(((IPEndPoint)listener.LocalEndpoint).Address == IPAddress.Loopback, "Wrong IP address");

            return listener;
        }
    }
}
