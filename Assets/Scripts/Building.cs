using UnityEngine;
using System.Collections.Generic;

public class Building : MonoBehaviour
{
	[SerializeField] float timeToBuild = 1.0f;
	[SerializeField] float maxHP = 100.0f;

	bool isBuilding;

	float buildTimer;
	float HP;

	MeshRenderer[] childMeshRenderers;
	MeshRenderer myMeshRenderer;

	List<Material> materials;

	public bool IsBuilt {
		get {
			return buildTimer >= timeToBuild;
		}
	}

	public float BuiltPerc {
		get {
			return buildTimer / timeToBuild;
		}
	}

	private void OnEnable()
	{
		childMeshRenderers = GetComponentsInChildren<MeshRenderer>();
		myMeshRenderer = GetComponent<MeshRenderer>();

		PopulateMaterialRefs();
	}

	void PopulateMaterialRefs()
	{
		materials = new List<Material>();

		if (myMeshRenderer != null)
		{
			foreach (Material material in myMeshRenderer.materials)
			{
				materials.Add(material);
			}
		}

		if (childMeshRenderers != null)
		{
			foreach (MeshRenderer childRenderer in childMeshRenderers)
			{
				foreach (Material childMaterial in childRenderer.materials)
				{
					materials.Add(childMaterial);
				}
			}
		}
	}

	void EnableRendering(bool enable)
	{
		if (childMeshRenderers != null)
		{
			foreach (var m in childMeshRenderers)
			{
				m.enabled = enable;
			}
		}

		if (myMeshRenderer != null)
		{
			myMeshRenderer.enabled = enable;
		}
	}

	void Update()
	{
		if (isBuilding) {
			buildTimer += Time.deltaTime;

			/* NOTE: This won't work if the building is attacked
			 * while it is in the process of being built.
			 * 
			 * TODO (George): Fix this. */
			HP = Mathf.Lerp(0.0f, maxHP, BuiltPerc);

			foreach (Material material in materials)
			{
				material.SetFloat("BuiltPercentage", BuiltPerc);
			}

			if (IsBuilt) {
				isBuilding = false;
			}
		}
	}

	public void Build()
	{
		isBuilding = true;
		buildTimer = 0.0f;
		HP = 0.0f;

		EnableRendering(true);
	}
}
