//
// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

using nanoFramework.TestFramework;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace NFUnitTests
{
    [TestClass]
    public class TestTcpListener
    {
        private const int TESTPORT = 1234;

        [Setup]
        public void Initialize()
        {
            // Remove this line to run hardware tests
            Assert.SkipTest("No Network");

            Debug.WriteLine("TcpListener Tests initialized.");
        }

        [TestMethod]
        public void CreateAndStart()
        {
            Debug.WriteLine("CreateAndStart.");

            TcpListener listener = CreateListener();

            listener.Start(1);

            // Get underlying socket
            Socket listenSocket = listener.Server;
            Assert.IsNotNull(listenSocket, "Listen socket null");

            listener.Stop();

            listener = null;
        }

        [TestMethod]
        public void StateExceptionChecks()
        {
            TcpListener listener = CreateListener();

            Assert.ThrowsException(typeof(InvalidOperationException), () => { listener.Stop(); }, "No exception when stopping and not started");

            listener.Start(1);

            Assert.ThrowsException(typeof(InvalidOperationException), () => { listener.Start(1); }, "No exception when starting and already started");

            listener.Stop();
        }

        public TcpListener CreateListener()
        {
            TcpListener listener = new TcpListener(IPAddress.Loopback, TESTPORT);
            Assert.IsNotNull(listener);

            Debug.WriteLine("TcpListener created.");

            Assert.AreEqual(((IPEndPoint)listener.LocalEndpoint).Port, TESTPORT, "Wrong port on Listener");
            Assert.AreEqual((int)((IPEndPoint)listener.LocalEndpoint).AddressFamily, (int)AddressFamily.InterNetwork, "Wrong address family");
            Assert.IsTrue(((IPEndPoint)listener.LocalEndpoint).Address == IPAddress.Loopback, "Wrong IP address");

            return listener;
        }
    }
}
