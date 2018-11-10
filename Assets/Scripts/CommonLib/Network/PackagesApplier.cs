using UnityEngine;

public class PackagesApplier : MonoBehaviour
{
	private static PackagesApplier Instance;

	public static void Launch()
	{
		if (Instance != null)
			return;

		Instance = new GameObject("PackagesApplier").AddComponent<PackagesApplier>();
	}

	private void Update()
	{
		PackageEventSystem.ApplyPackages();
	}
}