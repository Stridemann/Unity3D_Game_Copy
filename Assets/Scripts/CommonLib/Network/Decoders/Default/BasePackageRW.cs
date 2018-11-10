using System;
using System.Runtime.InteropServices;

public class BasePackageRW : IDisposable
{
	protected byte[] Bytes;
	protected IntPtr BytesPtr;
	protected GCHandle BytesHandle;

	public BasePackageRW(byte[] bytes)
	{
		Bytes = bytes;
		BytesHandle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
		BytesPtr = BytesHandle.AddrOfPinnedObject();
	}

	public void Dispose()
	{
		BytesHandle.Free();
	}
}