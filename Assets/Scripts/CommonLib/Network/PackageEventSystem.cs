//#define _2ByteLengthMax

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public static class NetworkEvent<T> where T : struct
{
	public static Action<T> Received = delegate { };
	private static byte PackageId;

	static NetworkEvent()
	{
		var attrs = typeof(T).GetCustomAttributes(false);

		foreach (var attr in attrs)
		{
			var typeAttr = attr as PackageTypeAttribute;

			if (typeAttr == null)
				continue;

			PackageId = typeAttr.PackageType;
			PackageEventSystem.Subscribers[typeAttr.PackageType] = Decode;
			return;
		}

		throw new InvalidOperationException("Package " + typeof(T) + "is not marked with PackageTypeAttribute attribute");
	}

	public static void Subscribe(Action<T> func)
	{
		Received += func;
	}

	internal static void Decode(byte[] bytes)
	{
		var newT = new T();
		var serInterf = newT as IPackageSerialization;

		if (serInterf != null)
		{
			serInterf.Decode(bytes);
			newT = (T) serInterf;
		}
		else
		{
			newT = IntptrToStruct<T>(bytes);
		}

		Action deleg = () => { Received(newT); };
		if(deleg == null)
			Debug.LogError("Null delol, lol, what?");

		PackageEventSystem.Packages.Enqueue(deleg);
	}

	internal static void Send(T package)
	{
		var serInterf = package as IPackageSerialization;
		byte[] pkgBytes;

		if (serInterf != null)
		{
			pkgBytes = serInterf.Encode();
		}
		else
		{
			pkgBytes = WriteStruct(package);
		}

		TCP_CoreClient.Instance.SendData(pkgBytes, PackageId);
	}

	public static TK IntptrToStruct<TK>(byte[] data) where TK : struct
	{
		var gch = GCHandle.Alloc(data, GCHandleType.Pinned);

		try
		{
			return (TK) Marshal.PtrToStructure(gch.AddrOfPinnedObject(), typeof(TK));
		}
		finally
		{
			gch.Free();
		}
	}

	public static byte[] WriteStruct<K>(K data) where K : struct
	{
		var buffer = new byte[Marshal.SizeOf(typeof(K))];
		var gcHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

		try
		{
			Marshal.StructureToPtr(data, gcHandle.AddrOfPinnedObject(), false);
		}
		finally
		{
			gcHandle.Free();
		}

		return buffer;
	}
}

public static class PackageEventSystem
{
	internal static Action<byte[]>[] Subscribers = new Action<byte[]>[byte.MaxValue];
	internal static Queue<Action> Packages = new Queue<Action>();

	public static void ApplyPackages()
	{
		while (Packages.Count > 0)
		{
			var func = Packages.Dequeue();

			if (func == null)
				Debug.LogError("Null func");
			else
			{
				func.Invoke();
			}
		}
	}

	public static void Send<T>(T package) where T : struct
	{
		NetworkEvent<T>.Send(package);
	}
}