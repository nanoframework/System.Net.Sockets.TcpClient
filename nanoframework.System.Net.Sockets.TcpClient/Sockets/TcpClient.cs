//
// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

namespace System.Net.Sockets
{
    public class TcpClient : IDisposable
    {
        private Socket _client;
        private NetworkStream _stream;
        private bool disposedValue;
        private AddressFamily _family = AddressFamily.InterNetwork;
        bool _active;

        /// <summary>
        /// Initializes a new instance of the TcpClient class.
        /// </summary>
        public TcpClient() : this(AddressFamily.InterNetwork)
        {
        }

        /// <summary>
        /// Initializes a new instance of the TcpClient class with specified end point
        /// </summary>
        /// <param name="localEP"></param>
        public TcpClient(IPEndPoint localEP)
        {
            _family = localEP.AddressFamily;
            initialize();
            _client.Bind(localEP);
        }

        /// <summary>
        /// Initializes a new instance of the TcpClient class with address family.
        /// </summary>
        /// <param name="family"></param>
        public TcpClient(AddressFamily family)
        {
            _family = family;
            initialize();
        }

        /// <summary>
        /// Initializes a new instance of the TcpClient class, Bind and connect to
        /// host name and port.
        /// </summary>
        /// <param name="hostname">Target Host name</param>
        /// <param name="port">Target port</param>
        public TcpClient(string hostname, int port)
        {
            try
            {
                Connect(hostname, port);
            }
            catch (Exception ex)
            {
                _client?.Close();
                throw ex;
            }
        }

        /// <summary>
        /// Used by TcpListener.Accept()
        /// </summary>
        /// <param name="acceptedSocket"></param>
        internal TcpClient(Socket acceptedSocket)
        {
            _client = acceptedSocket;
            _active = true;
        }

        /// <summary>
        /// Gets or sets the underlying Socket.
        /// </summary>
        public Socket Client
        {
            get => _client;
            set => _client = value;
        }

        /// <summary>
        /// Returns the NetworkStream used to send and receive data to remote host.
        /// </summary>
        /// <returns>The underlying NetworkStream.</returns>
        public NetworkStream GetStream()
        {
            //if (!_client.Connected)
            //         {
            //	throw new InvalidOperationException("Not connected");
            //         }

            if (_stream == null)
            {
                _stream = new NetworkStream(Client, true);
            }
            return _stream;
        }

        /// <summary>
        /// Gets the amount of data that has been received from the network 
        /// and is available to be read.
        /// </summary>
        public int Available { get => _client.Available; }


        /// <summary>
        /// Return connection status of underlying socket.
        /// </summary>
        public bool Connected
        { 
            get 
            {
                // We should be returning the _client.Connected state but that's not available
                // so for the moment just return active state
                return _active;
            } 
        }

        /// <summary>
        /// Gets or sets the receive time out value of the connection in seconds.
        /// </summary>
        public int ReceiveTimeout
        {
            get
            {
                return (int)Client.GetSocketOption(SocketOptionLevel.Socket,
                                    SocketOptionName.ReceiveTimeout);
            }
            set
            {
                Client.SetSocketOption(SocketOptionLevel.Socket,
                                  SocketOptionName.ReceiveTimeout, value);
            }
        }

        /// <summary>
        ///  Gets or sets the send time out value of the connection in seconds.
        /// </summary>
        public int SendTimeout
        {
            get
            {
                return (int)Client.GetSocketOption(SocketOptionLevel.Socket,
                                    SocketOptionName.SendTimeout);
            }

            set
            {
                Client.SetSocketOption(SocketOptionLevel.Socket,
                        SocketOptionName.SendTimeout, value);
            }
        }

        /// <summary>
        /// Gets or sets the value of the connection's linger option
        /// </summary>
        public LingerOption LingerState
        {
            get
            {
                int optionValue = (int)Client.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger);
                return (optionValue < 0)? new LingerOption(false, 0) : new LingerOption(true, optionValue);
            }
            set
            {
                int optionValue = value.Enabled ? value.LingerTime : -1;
                Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, optionValue);
            }
        }

        /// <summary>
        /// Enables or disables delay when send or receive buffers are full.
        /// </summary>
        public bool NoDelay
        {
            get
            {
                return (int)Client.GetSocketOption(SocketOptionLevel.Tcp,
                                        SocketOptionName.NoDelay) != 0 ? true : false;
            }
            set
            {
                Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, value ? 1 : 0);
            }
        }

        /// <summary>
        /// Connects the client to a remote TCP host using the specified 
        /// remote network endpoint.
        /// </summary>
        /// <param name="remoteEP">The IPEndPoint to which you intend to connect.</param>
        public void Connect(IPEndPoint remoteEP)
        {
            _client.Connect(remoteEP);
            _active = true;
        }

        /// <summary>
        /// Connects the client to a remote TCP host using the specified IP address 
        /// and port number.
        /// </summary>
        /// <param name="address">The IPAddress of the host to which you intend to connect.</param>
        /// <param name="port">The port number to which you intend to connect.</param>
        public void Connect(IPAddress address, int port)
        {
            Connect(new IPEndPoint(address, port));
        }

        /// <summary>
        /// Connects the client to a remote TCP host using the specified IP addresses and port number.
        /// </summary>
        /// <param name="address">The IPAddress array of the host to which you intend to connect.</param>
        /// <param name="port">The port number to which you intend to connect.</param>
        public void Connect(IPAddress[] address, int port)
        {
            Connect(new IPEndPoint(address[0], port));
        }

        /// <summary>
        /// Connects the Client to the specified port on the specified host.
        /// </summary>
        /// <param name="hostname">Remote host</param>
        /// <param name="port">Remote port</param>
        public void Connect(string hostname, int port)
        {
            if (_active)
            {
                throw new SocketException(SocketError.IsConnected);
            }

            IPHostEntry hostEntry = Dns.GetHostEntry(hostname);
            Exception lastex = null;
            Socket ipv4Socket = null;
            Socket ipv6Socket = null;

            try
            {
                // Via host name, port constructor ?
                if (_client == null)
                {
                    ipv4Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    ipv6Socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                }

                foreach (IPAddress address in hostEntry.AddressList)
                {
                    try
                    {
                        if (_client == null)
                        {
                            if (address.AddressFamily == AddressFamily.InterNetwork && ipv4Socket != null)
                            {
                                ipv4Socket.Connect(new IPEndPoint(address, port));
                                _client = ipv4Socket;
                                if (ipv6Socket != null)
                                {
                                    ipv6Socket.Close();
                                }
                            }
                            else if (ipv6Socket != null)
                            {
                                ipv6Socket.Connect(new IPEndPoint(address, port));
                                _client = ipv4Socket;
                                if (ipv4Socket != null)
                                {
                                    ipv4Socket.Close();
                                }
                            }

                            _active = true;
                            break;
                        }
                        else
                        {
                            Connect(new IPEndPoint(address, port));
                            _active = true;
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex is OutOfMemoryException)
                        {
                            throw;
                        }
                        lastex = ex;
                    }
                } // for each address
            }
            catch (Exception ex)
            {
                if (ex is OutOfMemoryException)
                {
                    throw;
                }
                lastex = ex;
            }
            finally
            {
                // Clean up if it failed
                if (!_active)
                {
                    if (ipv6Socket != null)
                    {
                        ipv6Socket.Close();
                    }

                    if (ipv4Socket != null)
                    {
                        ipv4Socket.Close();
                    }
                }

                // Throw exception if connect failed
                if (lastex != null)
                {
                    throw lastex;
                }
                else
                {
                    throw new SocketException(SocketError.NotConnected);
                }
            }
        }

        /// <summary>
        /// Disposes this TcpClient instance and 
        /// requests that the underlying TCP connection be closed.
        /// </summary>
        public void Close()
        {
            this.Dispose();
        }

        private void initialize()
        {
            _client = new Socket(_family, SocketType.Stream, ProtocolType.Tcp);
            _active = false;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _stream?.Dispose();
                    _client?.Close();
                    _client = null;
                }

                disposedValue = true;
            }
        }

        /// <summary>
        /// Dispose TcpClient
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}