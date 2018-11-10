using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class PackageReader : BasePackageRW
{
	public PackageReader(byte[] bytes) : base(bytes) { }

	public int ReadInt32()
	{
		var result = Marshal.ReadInt32(BytesPtr);
		BytesPtr = BytesPtr.Offset(4);
		return result;
	}

	public int[] ReadInt32Array()
	{
		var elementsCount = ReadInt32();
		if (elementsCount == 0)
			return new int[0];
		var result = new int[elementsCount * sizeof(int)];
		Marshal.Copy(BytesPtr, result, 0, elementsCount);
		BytesPtr = BytesPtr.Offset(elementsCount * sizeof(int));
		return result;
	}

	public byte[] ReadByteArray()
	{
		var elementsCount = ReadInt32();
		if(elementsCount == 0)
			return new byte[0];
		var result = new byte[elementsCount];
		Marshal.Copy(BytesPtr, result, 0, elementsCount);
		BytesPtr = BytesPtr.Offset(elementsCount);
		return result;
	}

	public string ReadString()
	{
		var elementsCount = ReadInt32();
		if (elementsCount == 0)
			return string.Empty;
		var result = new char[elementsCount];
		Marshal.Copy(BytesPtr, result, 0, elementsCount);
		BytesPtr = BytesPtr.Offset(elementsCount * sizeof(char));
		return new string(result);
	}

	[DllImport("msvcrt.dll", EntryPoint = "memcpy")]
	public static extern unsafe void CopyMemory(IntPtr pDest, IntPtr pSrc, int length);

	public Vector3[] ReadVector3Array()
	{
		var elementsCount = ReadInt32();
		if (elementsCount == 0)
			return new Vector3[0];
		var result = new Vector3[elementsCount];
		var arrByteSize = elementsCount * 3 * sizeof(float);
		var handleVs = GCHandle.Alloc(result, GCHandleType.Pinned);

		try
		{
			CopyMemory(handleVs.AddrOfPinnedObject(), BytesPtr, arrByteSize);
		}
		finally
		{
			handleVs.Free();
			BytesPtr = BytesPtr.Offset(arrByteSize);
		}
		
		return result;
	}

	public Vector2[] ReadVector2Array()
	{
		var elementsCount = ReadInt32();
		if (elementsCount == 0)
			return new Vector2[0];
		var result = new Vector2[elementsCount];
		var arrByteSize = elementsCount * 2 * sizeof(float);
		var handleVs = GCHandle.Alloc(result, GCHandleType.Pinned);

		try
		{
			CopyMemory(handleVs.AddrOfPinnedObject(), BytesPtr, arrByteSize);
		}
		finally
		{
			handleVs.Free();
			BytesPtr = BytesPtr.Offset(arrByteSize);
		}
		
		return result;
	}

	public Color32[] ReadColor32Array()
	{
		var elementsCount = ReadInt32();
		if (elementsCount == 0)
			return new Color32[0];
		var result = new Color32[elementsCount];
		var arrByteSize = elementsCount * 4 * sizeof(byte);
		var handleVs = GCHandle.Alloc(result, GCHandleType.Pinned);

		try
		{
			CopyMemory(handleVs.AddrOfPinnedObject(), BytesPtr, arrByteSize);
		}
		finally
		{
			handleVs.Free();
			BytesPtr = BytesPtr.Offset(arrByteSize);
		}
		
		BytesPtr = BytesPtr.Offset(arrByteSize);
		return result;
	}

	public T ReadStruct<T>() where T : struct
	{
		var strSize = Marshal.SizeOf(typeof(T));
		var result = (T) Marshal.PtrToStructure(BytesPtr, typeof(T));
		BytesPtr = BytesPtr.Offset(strSize);
		return result;
	}
}