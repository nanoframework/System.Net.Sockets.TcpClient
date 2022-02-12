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
        private EndPoint _localEndPoint;
        private Socket _listenSocket;
        private bool _active = false;

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
            _localEndPoint = localEP;
            _listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IPv4);
        }

        /// <summary>
        /// Gets the underlying EndPoint of the current TcpListener.
        /// </summary>
        public EndPoint LocalEndpoint { get => _localEndPoint; }

        /// <summary>
        /// Gets the underlying network Socket.
        /// </summary>
        public Socket Server { get => _listenSocket; }

        /// <summary>
        /// Gets a value that indicates whether TcpListener is actively listening 
        /// for client connections.
        /// </summary>
        protected bool Active { get => _active; }

        /// <summary>
        /// Starts listening for incoming connection requests with a maximum number of pending connection.
        /// </summary>
        /// <param name="backlog">The maximum length of the pending connections queue.</param>
        public void Start(int backlog)
        { 
            if (_active)
            {
                throw new InvalidOperationException();
            }

            if (_listenSocket == null)
            {
                _listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IPv4);
            }

            _listenSocket.Bind(_localEndPoint);

            _listenSocket.Listen(backlog);

            _active = true;
        }

        /// <summary>
        /// Closes the listener.
        /// </summary>
        public void Stop()
        {
            if (!_active)
            {
                throw new InvalidOperationException();
            }

            _listenSocket.Close();
            _listenSocket = null;
            _active = false;
        }

        /// <summary>
        /// Accepts a pending connection request.
        /// </summary>
        /// <returns>A TcpClient used to send and receive data.</returns>
        public TcpClient AcceptTcpClient()
        {
            TcpClient client = new TcpClient(_listenSocket.Accept());

            return client;
        }

        /// <summary>
        /// Accepts a pending connection request.
        /// </summary>
        /// <returns>A Socket used to send and receive data.</returns>
        public Socket AcceptSocket()
        {
            return _listenSocket.Accept();
        }
    }
}
