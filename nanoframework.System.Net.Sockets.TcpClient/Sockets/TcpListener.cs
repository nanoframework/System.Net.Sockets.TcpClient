//
// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

namespace System.Net.Sockets
{
    /// <summary>
    /// Listens for connections from TCP network clients.
    /// </summary>
    public class TcpListener
    {
        /// <summary>
        /// Initializes a new instance of the TcpListener class that listens for incoming connection attempts on the specified local IP address and port number.
        /// </summary>
        /// <param name="localaddr">An IPAddress that represents the local IP address.</param>
        /// <param name="port">The port on which to listen for incoming connection attempts.</param>
        public TcpListener(IPAddress localaddr, int port) : this(new IPEndPoint(localaddr, port))
        {
        }

        /// <summary>
        /// Initializes a new instance of the TcpListener class that listens for incoming connection 
        /// attempts with the specified endpoint.
        /// </summary>
        /// <param name="localEP">An IPEndPoint that represents the local endpoint to which to bind the listener Socket.</param>
        public TcpListener(IPEndPoint localEP)
        {
            LocalEndpoint = localEP;
            Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IPv4);
        }

        /// <summary>
        /// Gets the underlying EndPoint of the current TcpListener.
        /// </summary>
        public EndPoint LocalEndpoint { get; }

        /// <summary>
        /// Gets the underlying network Socket.
        /// </summary>
        public Socket Server { get; private set; }

        /// <summary>
        /// Gets a value that indicates whether TcpListener is actively listening 
        /// for client connections.
        /// </summary>
        protected bool Active { get; private set; }

        /// <summary>
        /// Starts listening for incoming connection requests with a maximum number of pending connection.
        /// </summary>
        /// <param name="backlog">The maximum length of the pending connections queue.</param>
        public void Start(int backlog)
        {
            if (Active)
            {
                throw new InvalidOperationException();
            }

            if (Server == null)
            {
                Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IPv4);
            }

            Server.Bind(LocalEndpoint);

            Server.Listen(backlog);

            Active = true;
        }

        /// <summary>
        /// Closes the listener.
        /// </summary>
        public void Stop()
        {
            if (Active)
            {
                throw new InvalidOperationException();
            }

            Server.Close();
            Server = null;
            Active = false;
        }

        /// <summary>
        /// Accepts a pending connection request.
        /// </summary>
        /// <returns>A TcpClient used to send and receive data.</returns>
        public TcpClient AcceptTcpClient()
        {
            TcpClient client = new TcpClient(Server.Accept());

            return client;
        }

        /// <summary>
        /// Accepts a pending connection request.
        /// </summary>
        /// <returns>A Socket used to send and receive data.</returns>
        public Socket AcceptSocket()
        {
            return Server.Accept();
        }
    }
}
