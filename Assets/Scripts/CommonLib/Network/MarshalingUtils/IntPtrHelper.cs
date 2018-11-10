using System;

public static class IntPtrHelper
{
	public static IntPtr Offset(this IntPtr ptr, int offset)
	{
		return new IntPtr(ptr.ToInt64() + offset);
	}
}