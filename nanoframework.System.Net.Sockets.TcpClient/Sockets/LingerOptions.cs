//
// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

namespace System.Net.Sockets
{
    /// <summary>
    /// Contains information for a socket's linger time, the amount of time it will
    /// remain after closing if data remains to be sent.
    /// </summary>
    public class LingerOption
    {
        int _lingerTime;

        /// <summary>
        /// Initializes a new instance of the <see cref='Sockets.LingerOption'/>
        /// </summary>
        /// <param name="enable">Enable or disable option.</param>
        /// <param name="seconds">Number of seconds to linger after close.</param>
        public LingerOption(bool enable, int seconds)
        {
            Enabled = enable;
            LingerTime = seconds;
        }

        /// <summary>
        /// Enables or disables lingering after close.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// The amount of time, in seconds, to remain connected after a close.
        /// </summary>
        public int LingerTime { get => _lingerTime; set => _lingerTime = value; }
    } 
} 