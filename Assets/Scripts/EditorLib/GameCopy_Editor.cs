using System;
using System.IO;
using System.Linq;
using UnityEngine;

public class GameCopy_Editor : MonoBehaviour
{
	private static GameCopy_Editor Instance;
	private TCP_CoreServer Server;
	public bool SaveTex;

	public static bool IsSaveTex
	{
		get { return Instance == null ? false : Instance.SaveTex; }
	}
	private void Start()
	{
		Server = new TCP_CoreServer();
		Server.Initialize();
		Instance = this;
		PackagesApplier.Launch();
	}

	private void OnGUI()
	{
		GUILayout.Space(400);
		GUILayout.Label("Total received: " + SizeSuffix(TCP_CoreClient.TotalReceived));
		if (GUILayout.Button("Send existent tex"))
		{
			SendExistTex();
		}
	}

	static readonly string[] SizeSuffixes = 
			{ "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
	static string SizeSuffix(ulong value, int decimalPlaces = 1)
	{
		if (value == 0)
			return "None";
		if (decimalPlaces < 0) { throw new ArgumentOutOfRangeException("decimalPlaces"); }

		// mag is 0 for bytes, 1 for KB, 2, for MB, etc.
		int mag = (int)Math.Log(value, 1024);

		// 1L << (mag * 10) == 2 ^ (10 * mag) 
		// [i.e. the number of bytes in the unit corresponding to mag]
		decimal adjustedSize = (decimal)value / (1L << (mag * 10));

		// make adjustment when the value is large enough that
		// it would round up to 1000 or more
		if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
		{
			mag += 1;
			adjustedSize /= 1024;
		}

		return string.Format("{0:n" + decimalPlaces + "} {1}", 
				adjustedSize, 
				SizeSuffixes[mag]);
	}

	private void SendExistTex()
	{
		var dInfo = new DirectoryInfo(Path.Combine(Application.dataPath, "Resources"));
		if(!dInfo.Exists)
			Debug.LogError("Doesn't exist: " + dInfo.FullName);

		var files = dInfo.GetFiles("*.jpg");

		var texsName = string.Join(",", files.Select(x => Path.GetFileNameWithoutExtension(x.Name)).ToArray());

		var texInfo = new ExistTextureInfo(texsName);
		PackageEventSystem.Send(texInfo);
	}
}