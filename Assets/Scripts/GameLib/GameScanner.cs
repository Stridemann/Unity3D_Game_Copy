using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScanner
{
	private GC_GameObject SceneRoot = new GC_GameObject();
	private Dictionary<int, GC_GameObject> AllGameObjects = new Dictionary<int, GC_GameObject>();
	private List<Transform> ProcessQueue;

	public GameScanner()
	{
		NetworkEvent<ExistTextureInfo>.Subscribe(ExistTex);
	}

	private void ExistTex(ExistTextureInfo obj)
	{
		var texs = obj.TexNames.Split(',');
		Debug.LogError("Received Exist tex");
		MeshRendererInfo.CTex = new HashSet<string>(texs);
	}

	public void SendGO()
	{
		var transforms = Object.FindObjectsOfType<Transform>();
		ProcessQueue = new List<Transform>(transforms);
		CreateHierarchy();
		SendGo(SceneRoot);
	}

	public IEnumerator SendComps()
	{
		yield return SendComponents(SceneRoot);
	}

	private void CreateHierarchy()
	{
		if (ProcessQueue.Count == 0)
			return;

		var transform = ProcessQueue[0];
		ProcessQueue.RemoveAt(0);

		var id = transform.GetInstanceID();

		GC_GameObject gcObj;

		if (!AllGameObjects.TryGetValue(id, out gcObj))
		{
			gcObj = new GC_GameObject(transform, id);
			AllGameObjects.Add(id, gcObj);
		}

		if (gcObj.Parent == null)
		{
			var parentTransform = transform.parent;

			if (parentTransform != null)
			{
				var gcParId = parentTransform.GetInstanceID();

				GC_GameObject parentGcObj;

				if (!AllGameObjects.TryGetValue(gcParId, out parentGcObj))
				{
					parentGcObj = new GC_GameObject(parentTransform, gcParId);
					AllGameObjects.Add(gcParId, parentGcObj);
				}

				parentGcObj.Childs.Add(gcObj);
				gcObj.Parent = parentGcObj;
			}
			else
				SceneRoot.Childs.Add(gcObj);
		}

		CreateHierarchy();
	}

	private static void SendGo(GC_GameObject obj)
	{
		foreach (var child in obj.Childs)
		{
			PackageEventSystem.Send(child.GetGOInfo());
		}

		foreach (var child in obj.Childs)
		{
			SendGo(child);
		}
	}

	private IEnumerator SendComponents(GC_GameObject obj)
	{
		foreach (var child in obj.Childs)
		{
			ComponentExporter.ExportComponents(child.Transform.GetComponents<Component>());
			yield return new WaitForEndOfFrame();
		}

		foreach (var child in obj.Childs)
		{
			yield return SendComponents(child);
		}
	}
}

public class GC_GameObject
{
	public GC_GameObject Parent;
	public Transform Transform;
	public int Id;
	public List<GC_GameObject> Childs = new List<GC_GameObject>();

	public GC_GameObject() { }

	public GC_GameObject(Transform transform, int id)
	{
		Transform = transform;
		Id = id;
	}

	public GameObjectInfo GetGOInfo()
	{
		var info = new GameObjectInfo(Transform);
		return info;
	}
}