using System;

public interface IPackageSerialization
{
	int PackSize();
	void Decode(byte[] bytes);
	byte[] Encode();
}