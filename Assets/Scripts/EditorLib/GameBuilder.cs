using System.Collections.Generic;
using UnityEngine;

public class GameBuilder : MonoBehaviour
{
	private Dictionary<int, Transform> Gameobjects = new Dictionary<int, Transform>();
	public Material DefaultMat;
	public static Material _DefaultMat;

	void Start()
	{
		_DefaultMat = DefaultMat;
		NetworkEvent<GameObjectInfo>.Subscribe(ReceiveGOInfo);
		NetworkEvent<BoxColliderInfo>.Subscribe(ReceiveComponentInfo);
		NetworkEvent<MeshFilterInfo>.Subscribe(ReceiveMeshFilterComponentInfo);
		NetworkEvent<MeshRendererInfo>.Subscribe(ReceiveMeshRendererComponentInfo);
		NetworkEvent<LightComponentInfo>.Subscribe(SpawnLight);
	}

	private void SpawnLight(LightComponentInfo info)
	{
		Transform objTransf;

		if (!Gameobjects.TryGetValue(info.Owner, out objTransf))
		{
			Debug.LogError("Can't find owner of BoxCollider component on id: " + info.Owner);
			return;
		}

		info.AddComponent(objTransf.gameObject);
	}

	private void ReceiveGOInfo(GameObjectInfo info)
	{
		Transform objTransf;

		if (!Gameobjects.TryGetValue(info.Main.Id, out objTransf))
		{
			Gameobjects.Add(info.Main.Id, objTransf = new GameObject().transform);
		}

		if (info.Main.ParentId != int.MinValue)
		{
			Transform cachedTr;

			if (!Gameobjects.TryGetValue(info.Main.ParentId, out cachedTr))
				Gameobjects.Add(info.Main.ParentId, cachedTr = new GameObject().transform);

			objTransf.parent = cachedTr;
		}

		objTransf.localPosition = info.Main.LocalPos;
		objTransf.localEulerAngles = info.Main.LocalEulerAngles;
		objTransf.localScale = info.Main.LocalScale;
		objTransf.name = info.Name;
	}

	private void ReceiveComponentInfo(BoxColliderInfo info)
	{
		Transform objTransf;

		if (!Gameobjects.TryGetValue(info.Owner, out objTransf))
		{
			Debug.LogError("Can't find owner of BoxCollider component on id: " + info.Owner);
			return;
		}

		info.AddComponent(objTransf.gameObject);
	}
	private void ReceiveMeshFilterComponentInfo(MeshFilterInfo info)
	{
		Transform objTransf;

		if (!Gameobjects.TryGetValue(info.Owner, out objTransf))
		{
			Debug.LogError("Can't find owner of MeshFilter component on id: " + info.Owner);
			return;
		}

		info.AddComponent(objTransf.gameObject);

		var mr = objTransf.GetComponent<MeshRenderer>();
		if (mr == null)
			mr = objTransf.gameObject.AddComponent<MeshRenderer>();

		var mat = new Material(GameBuilder._DefaultMat);
		mr.material = mat;
	}

	private void ReceiveMeshRendererComponentInfo(MeshRendererInfo info)
	{
		Transform objTransf;

		if (!Gameobjects.TryGetValue(info.Owner, out objTransf))
		{
			Debug.LogError("Can't find owner of MeshFilter component on id: " + info.Owner);
			return;
		}

		info.ProcessComponent(objTransf.gameObject);
	}
}