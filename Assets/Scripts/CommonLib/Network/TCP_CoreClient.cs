using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class TCP_CoreClient
{
	public static TCP_CoreClient Instance;
	public Action OnDisconected = delegate { };
	protected Socket _clientSocket;
	protected readonly byte[] _recieveBuffer = new byte[100000];
	private byte[] ReceiveCacheBuffer = new byte[0];
	public bool Waiting;
	public bool Connected
	{
		get { return _clientSocket != null && _clientSocket.Connected; }
	}

	public TCP_CoreClient()
	{
		Instance = this;
	}

	public virtual void Initialize(IPAddress ipAddress = null, int port = 11000)
	{
		if (ipAddress == null)
			ipAddress = IPAddress.Parse("127.0.0.1");

		_clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		_clientSocket.Connect(new IPEndPoint(ipAddress, port));
		_clientSocket.BeginReceive(_recieveBuffer, 0, _recieveBuffer.Length, SocketFlags.None, ReceiveCallback, null);
	}

	public static ulong TotalReceived;
	protected void ReceiveCallback(IAsyncResult AR)
	{
		try
		{
			var recieved = _clientSocket.EndReceive(AR);
			TotalReceived += (uint)recieved;

			if (recieved <= 0)
			{
				Debug.Log("Disconnected?");
				OnDisconected();
			}
			else
			{
				Waiting = false;
				if (recieved == _recieveBuffer.Length)
				{
					if (ReceiveCacheBuffer.Length == 0)
					{
						ReceiveCacheBuffer = new byte[_recieveBuffer.Length];
						Buffer.BlockCopy(_recieveBuffer, 0, ReceiveCacheBuffer, 0, ReceiveCacheBuffer.Length);
						Waiting = true;
					}
					else
					{
						var newBuffer = new byte[ReceiveCacheBuffer.Length + _recieveBuffer.Length];
						Buffer.BlockCopy(ReceiveCacheBuffer, 0, newBuffer, 0, ReceiveCacheBuffer.Length);
						Buffer.BlockCopy(_recieveBuffer, 0, newBuffer, ReceiveCacheBuffer.Length, _recieveBuffer.Length);
						ReceiveCacheBuffer = newBuffer;
						Waiting = true;
					}
				}
				else
				{
					if (ReceiveCacheBuffer.Length == 0)
					{
						var recData = new byte[recieved];
						Buffer.BlockCopy(_recieveBuffer, 0, recData, 0, recieved);

						DecodeCacked(recData);
					}
					else
					{
						var newBuffer = new byte[ReceiveCacheBuffer.Length + recieved];
						Buffer.BlockCopy(ReceiveCacheBuffer, 0, newBuffer, 0, ReceiveCacheBuffer.Length);
						Buffer.BlockCopy(_recieveBuffer, 0, newBuffer, ReceiveCacheBuffer.Length, recieved);
						ReceiveCacheBuffer = new byte[0];
						DecodeCacked(newBuffer);
					}
				}
			}

			_clientSocket.BeginReceive(_recieveBuffer, 0, _recieveBuffer.Length, SocketFlags.None, ReceiveCallback, null);
		}
		catch (Exception e)
		{
			Debug.LogError(e);
		}
	}

	private void DecodeCacked(byte[] recData)
	{
		var pos = Decode(recData);
		if (pos >= 0)
		{
			var left = recData.Length - pos;
			if (left > 0)
			{
				byte[] finalBuffer;
				if (pos == 0)
				{
					//Debug.Log("Backup all: " + left);
					finalBuffer = recData;
				}
				else
				{
					//Debug.Log("Backup: " + left);
					finalBuffer = new byte[left];
					Buffer.BlockCopy(recData, pos, finalBuffer, 0, left);
				}

				if (ReceiveCacheBuffer.Length == 0)
				{
					ReceiveCacheBuffer = finalBuffer;
					Waiting = true;
				}
				else
				{
					var newBuffer = new byte[ReceiveCacheBuffer.Length + finalBuffer.Length];
					Buffer.BlockCopy(ReceiveCacheBuffer, 0, newBuffer, 0, ReceiveCacheBuffer.Length);
					Buffer.BlockCopy(finalBuffer, 0, newBuffer, ReceiveCacheBuffer.Length, finalBuffer.Length);
					ReceiveCacheBuffer = newBuffer;
					Waiting = true;
				}
			}
		}
	}

	private const int HASH1Index = 0;
	private const int PackSizeIndex = 1;
	private const int PackTypeIndex = 5;
	private const int DataIndex = 6;

	public static int Decode(byte[] bytes, int pos = 0)
	{
		if (bytes.Length == 0)
			return -1;
		try
		{
#if _2ByteLengthMax
			var packLength = BitConverter.ToUInt16(bytes, pos);
			var packageId = bytes[pos + 2];
#else
			if (bytes.Length < 6)
			{
				Debug.LogError("Too Small package to read it's length");
				return pos;
			}

			var hash = bytes[pos + HASH1Index];
			if (hash != 255)
			{
				Debug.LogError("Package hash 1 is not correct");
			}
			var packLength = BitConverter.ToInt32(bytes, pos + 1);
			if (packLength + 7 > bytes.Length)
			{
				//Debug.Log("Expecting: " + (packLength + 7) + " bytes. Waiting new package, Pos: " + pos);
				return pos;
			}

			if (bytes[pos + DataIndex + packLength] != 255)
			{
				Debug.LogError("Package hash 2 is not correct");
				return -1;
			}
			if (pos + PackTypeIndex >= bytes.Length)
			{
				Debug.LogError("Small package: ");
				return pos;
			}

			var packageId = bytes[pos + PackTypeIndex];
#endif

			var realPkgBytes = new byte[packLength];

			try
			{
				Buffer.BlockCopy(bytes, pos + DataIndex, realPkgBytes, 0, packLength);
			}
			catch (Exception e)
			{
				Debug.LogError(e.ToString());
			}
		
			var decodeFunc = PackageEventSystem.Subscribers[packageId];
			if (decodeFunc != null)
			{
				try
				{
					decodeFunc(realPkgBytes);
				}
				catch (Exception e)
				{
					Debug.LogError("Error decode: " + e.ToString());
					return -1;
				}
			}
			else
				Debug.LogError("Package " + packageId + " is not registered (nobody listening).");

			var lastPos = pos + packLength + 7;

			if (bytes.Length > lastPos)
			{
				return Decode(bytes, lastPos);
			}
			else
			{
				return -1;
			}
		
		}
		catch (Exception e)
		{
			Debug.LogError(e.ToString());
		}

		return -1;
	}

	public void SendData(byte[] pkgBytes, byte PackageId)
	{
		var resultBytes = new byte[pkgBytes.Length + 7];
		Buffer.BlockCopy(pkgBytes, 0, resultBytes, 6, pkgBytes.Length);

#if _2ByteLengthMax
//Ushort package length
		ushort packageUshortLength;
		try
		{	
			checked
			{
				packageUshortLength = (ushort) pkgBytes.Length;
			}
		}
		catch (OverflowException)
		{
			Debug.LogError("Length of package is bigger that expected. UShort package length is enabled (#_2ByteLengthMax is enabled). " +
			               "Increase allowed package size by commenting out this preprocessor directive.");
			throw;
		}

		var pkgLenBytes = BitConverter.GetBytes(packageUshortLength);
		resultBytes[0] = pkgLenBytes[0];
		resultBytes[1] = pkgLenBytes[1];
		resultBytes[2] = PackageId;
#else
		//Int package length
		var pkgLenBytes = BitConverter.GetBytes(pkgBytes.Length);
		resultBytes[0] = 255;
		resultBytes[1] = pkgLenBytes[0];
		resultBytes[2] = pkgLenBytes[1];
		resultBytes[3] = pkgLenBytes[2];
		resultBytes[4] = pkgLenBytes[3];
		resultBytes[5] = PackageId;
		resultBytes[resultBytes.Length - 1] = 255;
#endif


		var socketAsyncData = new SocketAsyncEventArgs();
		socketAsyncData.SetBuffer(resultBytes, 0, resultBytes.Length);
		_clientSocket.SendAsync(socketAsyncData);
	}
}