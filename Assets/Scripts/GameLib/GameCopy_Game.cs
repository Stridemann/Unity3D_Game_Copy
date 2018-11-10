using UnityEngine;

public class GameCopy_Game : MonoBehaviour
{
	private TCP_CoreClient Client;
	private static GameCopy_Game Instance;
	public static void _LoadModule()
	{
		if (Instance != null)
			return;
		Instance = new GameObject("GameCopy_Game").AddComponent<GameCopy_Game>();
	}
	private GameScanner gs;
	private void Start()
	{
		gs = new GameScanner();
		Client = new TCP_CoreClient();		
		PackagesApplier.Launch();
	}

	private void OnGUI()
	{
		GUILayout.Space(500);
		GUILayout.Label("Connected: " + Client.Connected);

		if (GUILayout.Button("Connect to server")) Client.Initialize();

		if (Client.Connected)
		{
			if (GUILayout.Button("Send World"))
			{
				gs = new GameScanner();
				gs.SendGO();
			}

			if (GUILayout.Button("Send Comps"))
			{
				StartCoroutine(gs.SendComps());
			}
		}
	}
}