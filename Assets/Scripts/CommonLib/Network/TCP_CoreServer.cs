using System;
using System.Net;
using System.Net.Sockets;

[Serializable]
public class TCP_CoreServer : TCP_CoreClient
{
	public override void Initialize(IPAddress ipAddress = null, int port = 11000)
	{
		var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());

		if (ipAddress == null)
			ipAddress = IPAddress.Parse("127.0.0.1");

		var localEndPoint = new IPEndPoint(ipAddress, port);

		_clientSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

		_clientSocket.Bind(localEndPoint);
		_clientSocket.Listen(100);

		_clientSocket.BeginAccept(AcceptCallback, _clientSocket);
	}

	public void AcceptCallback(IAsyncResult ar)
	{
		// Get the socket that handles the client request.  
		var listener = (Socket) ar.AsyncState;
		var handler = listener.EndAccept(ar);
		_clientSocket = handler;
		_clientSocket.BeginReceive(_recieveBuffer, 0, _recieveBuffer.Length, SocketFlags.None, ReceiveCallback, null);

		_clientSocket.BeginAccept(AcceptCallback, _clientSocket);
	}
}