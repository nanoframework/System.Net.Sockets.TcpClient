[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=nanoframework_System.Net.Sockets.TcpClient&metric=alert_status)](https://sonarcloud.io/dashboard?id=nanoframework_System.Net.Sockets.TcpClient) [![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=nanoframework_System.Net.Sockets.TcpClient&metric=reliability_rating)](https://sonarcloud.io/dashboard?id=nanoframework_System.Net.Sockets.TcpClient) [![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE) [![NuGet](https://img.shields.io/nuget/dt/nanoFramework.System.Net.Sockets.TcpClient.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.System.Net.Sockets.TcpClient/) [![#yourfirstpr](https://img.shields.io/badge/first--timers--only-friendly-blue.svg)](https://github.com/nanoframework/Home/blob/main/CONTRIBUTING.md) [![Discord](https://img.shields.io/discord/478725473862549535.svg?logo=discord&logoColor=white&label=Discord&color=7289DA)](https://discord.gg/gCyBu8T)

![nanoFramework logo](https://raw.githubusercontent.com/nanoframework/Home/main/resources/logo/nanoFramework-repo-logo.png)

-----

# Welcome to the .NET **nanoFramework** System.Net.Sockets.TcpClient

This API implements the TcpListener and TcpClient classes with a pattern similar to the official .NET equivalent. [System.NET.Sockets.TcpClient](https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.TcpClient).

These are wrapper classes for the Socket when using TCP connections.
The nanoframework implementation of TcpClient doesn't include the asynchronous methods and the Connected property.

## Build status

| Component | Build Status | NuGet Package |
|:-|---|---|
| nanoFramework.System.Net.Sockets.TcpClient | [![Build Status](https://dev.azure.com/nanoframework/System.Net.Sockets.TcpClient/_apis/build/status/nanoframework.System.Net.Sockets.TcpClient?repoName=nanoframework%2FSystem.Net.Sockets.TcpClient&branchName=main)](https://dev.azure.com/nanoframework/System.Net.Sockets.TcpClient/_build/latest?definitionId=93&repoName=nanoframework%2FSystem.Net.Sockets.TcpClient&branchName=main) | [![NuGet](https://img.shields.io/nuget/v/nanoFramework.System.Net.Sockets.TcpClient.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.System.Net.Sockets.TcpClient/) |
| nanoFramework.System.Net.Sockets.TcpClient (preview) | [![Build Status](https://dev.azure.com/nanoframework/System.Net.Sockets.TcpClient/_apis/build/status/nanoframework.System.Net.Sockets.TcpClient?repoName=nanoframework%2FSystem.Net.Sockets.TcpClient&branchName=develop)](https://dev.azure.com/nanoframework/System.Net.Sockets.TcpClient/_build/latest?definitionId=93&repoName=nanoframework%2FSystem.Net.Sockets.TcpClient&branchName=develop) | [![NuGet](https://img.shields.io/nuget/vpre/nanoFramework.System.Net.Sockets.TcpClient.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.System.Net.Sockets.TcpClient/) |

## Usage

**Important:** Obviously this requires a working network connection. Please check the examples with the Network Helpers on how to connect to a network. For example see the [Networking sample pack](https://github.com/nanoframework/Samples/tree/main/samples/Networking)

The `TcpListener` class provides simple methods for creating a listening socket to accept incoming TCP connections and the `TcpClient` provides methods for connecting and communicating on a TCP connection.

### Samples

Samples for `TcpListener` and `TcpClient` are present in the [nanoFramework Sample repository](https://github.com/nanoframework/Samples).

### Listening for incoming connections

The following codes shows how to set up a Listening socket and to accept connections as a TcpClient on the 1234 port.

```csharp
TcpListener listener = new TcpListener(IPAddress.Any, 1234);

// Start listening for incoming connections
listener.Start();
while (true)
{
    try
    {
        // Wait for incoming connections
        TcpClient client = listener.AcceptTcpClient();

        NetworkStream stream = client.GetStream();

        Byte[] bytes = new Byte[256];        
        int i;

        // Wait for incoming data and echo back
        while((i = stream.Read(bytes, 0, bytes.Length))!=0)
        {
            // Do something with data ?

            stream.Write(bytes, 0, i);
        }

        // Shutdown connection
        client.Close();
    }
    catch(Exception ex)
    {
        Debug.WriteLine($"Exception:-{ex.Message}");
    }
}
```

If you want to handle more then one simultaneous connection then a separate worker thread can be started.

```csharp
TcpListener listener = new TcpListener(IPAddress.Any, 1234);

// Start listening for incoming connections with backlog
listener.Start(2);

while (true)
{
    try
    {
        // Wait for incoming connections
        TcpClient client = listener.AcceptTcpClient();

        // Start thread to handle connection
        Thread worker = new Thread(() => WorkerThread(client));
        worker.Start();
    }
    catch(Exception ex)
    {
        Debug.WriteLine($"Exception:-{ex.Message}");
    }
}
```

Worker Thread for handling the TcpClient connection for TcpListener example.

```csharp
private static void WorkerThread(TcpClient client)
{
    try
    {
        NetworkStream stream = client.GetStream();

        Byte[] bytes = new Byte[256];        
        int i;

        // Loop reading data until connection closed
        while((i = stream.Read(bytes, 0, bytes.Length))!=0)
        {
            // Do something with data ?

            // Write back received data bytes to stream
            stream.Write(bytes, 0, i);
        }
    }
    catch(Exception ex)
    {
        Debug.WriteLine($"Exception:-{ex.Message}");
    }
    finally
    {
        // Shutdown connection
        client.Close();
    } 
}
```

### TcpClient

The TcpClient can also be used to initiate a connection passing in the hostname/port or IPEndPoint. 
Maybe connecting to another nanoFramework device which is listening for connections.  

```csharp
TcpClient client = new TcpClient()

try
{
    client.Connect(hostname, port)

    NetworkStream stream = client.GetStream();

    // Write / Read data on stream

    // for example Write 'ABC' and wait for response
    byte[] writeData = new byte[] { 0x41, 0x42, 0x43 };  
    stream.Write(writeData, 0, writeData.Length);

    // Read reply and close
    byte[] buffer = new byte[1024];
    int bytesRead = stream.Read(buffer, 0, buffer.Length);

    // Process read data ?
}
catch(SocketException sx)
{
    Console.WriteLine($"Socket error:{sx.ErrorCode} exception:{sx.Message}");
}
finally
{
    client.Close();
}
```

For secure connections a `SslStream` can be used.

```csharp
client.Connect(HostName, 443);

// Create SSlStream from underlying SOcket
SslStream stream = new SslStream(client.Client);

// Don't verify Server certificate for this sample code
stream.SslVerification = SslVerification.NoVerification;
stream.AuthenticateAsClient(HostName, SslProtocols.Tls12);

// stream.Write() or stream.Read()
```

## Feedback and documentation

For documentation, providing feedback, issues and finding out how to contribute please refer to the [Home repo](https://github.com/nanoframework/Home).

Join our Discord community [here](https://discord.gg/gCyBu8T).

## Credits

The list of contributors to this project can be found at [CONTRIBUTORS](https://github.com/nanoframework/Home/blob/main/CONTRIBUTORS.md).

## License

The **nanoFramework** Class Libraries are licensed under the [MIT license](LICENSE.md).

## Code of Conduct

This project has adopted the code of conduct defined by the Contributor Covenant to clarify expected behaviour in our community.
For more information see the [.NET Foundation Code of Conduct](https://dotnetfoundation.org/code-of-conduct).

### .NET Foundation

This project is supported by the [.NET Foundation](https://dotnetfoundation.org).
