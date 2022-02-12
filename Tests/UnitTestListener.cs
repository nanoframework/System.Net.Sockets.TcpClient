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
            Assert.NotNull(listenSocket, "Listen socket null");

            listener.Stop();

            listener = null;
        }

        [TestMethod]
        public void StateExceptionChecks()
        {
            TcpListener listener = CreateListener();

            Assert.Throws(typeof(InvalidOperationException), () => { listener.Stop(); }, "No exception when stopping and not started");

            listener.Start(1);

            Assert.Throws(typeof(InvalidOperationException), () => { listener.Start(1); }, "No exception when starting and already started");

            listener.Stop();
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
