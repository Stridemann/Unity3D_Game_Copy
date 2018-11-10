using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class PackageWriter : BasePackageRW
{
	public PackageWriter(byte[] bytes) : base(bytes) { }

	public void WriteInt(int value)
	{
		Marshal.WriteInt32(BytesPtr, value);
		BytesPtr = BytesPtr.Offset(4);
	}

	public void WriteArray(int[] array)
	{
		WriteInt(array.Length);
		if (array.Length == 0)
			return;
		var arrByteSize = array.Length * sizeof(int);
		Marshal.Copy(array, 0, BytesPtr, array.Length);
		BytesPtr = BytesPtr.Offset(arrByteSize);
	}

	public void WriteArray(byte[] array)
	{
		WriteInt(array.Length);
		if (array.Length == 0)
			return;
		var arrByteSize = array.Length;
		Marshal.Copy(array, 0, BytesPtr, array.Length);
		BytesPtr = BytesPtr.Offset(arrByteSize);
	}

	public void WriteString(string s)
	{
		var elementsCount = s.Length;
		WriteInt(elementsCount);
		if (s.Length == 0)
			return;
		Marshal.Copy(s.ToCharArray(), 0, BytesPtr, elementsCount);
		BytesPtr = BytesPtr.Offset(elementsCount * sizeof(char));
	}



	public void WriteArray(Vector3[] array)
	{
		WriteInt(array.Length);
		if (array.Length == 0)
			return;
		var arrByteSize = array.Length * 3 * sizeof(float);
		var handleVs = GCHandle.Alloc(array, GCHandleType.Pinned);
		var copyLenBytes = array.Length * sizeof(float) * 3;

		try
		{
			CopyMemory(BytesPtr, handleVs.AddrOfPinnedObject(), copyLenBytes);
		}
		finally 
		{
			handleVs.Free();
			BytesPtr = BytesPtr.Offset(arrByteSize);
		}
	}

	public void WriteArray(Vector2[] array)
	{
		WriteInt(array.Length);
		if (array.Length == 0)
			return;
		var arrByteSize = array.Length * 2 * sizeof(float);
		var handleVs = GCHandle.Alloc(array, GCHandleType.Pinned);

		try
		{
			CopyMemory(BytesPtr, handleVs.AddrOfPinnedObject(), arrByteSize);
		}
		finally 
		{
			handleVs.Free();
			BytesPtr = BytesPtr.Offset(arrByteSize);
		}
	}


	public void WriteArray(Color32[] array)
	{
		WriteInt(array.Length);
		if (array.Length == 0)
			return;
		var arrByteSize = array.Length * 4 * sizeof(byte);
		var handleVs = GCHandle.Alloc(array, GCHandleType.Pinned);

		try
		{
			CopyMemory(BytesPtr, handleVs.AddrOfPinnedObject(), arrByteSize);
		}
		finally 
		{
			handleVs.Free();
			BytesPtr = BytesPtr.Offset(arrByteSize);
		}
	}

	public void WriteStruct<T>(T str)
	{
		var strSize = Marshal.SizeOf(typeof(T));
		Marshal.StructureToPtr(str, BytesPtr, true);
		BytesPtr = BytesPtr.Offset(strSize);
	}

	[DllImport("msvcrt.dll", EntryPoint = "memcpy")]
	public static extern void CopyMemory(IntPtr pDest, IntPtr pSrc, int length);
}