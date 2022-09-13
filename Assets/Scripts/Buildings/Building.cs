using UnityEngine;
using System.Collections.Generic;

public class Building : MonoBehaviour
{
	[SerializeField] float timeToBuild = 1.0f;
	public float maxHP = 100.0f;

	bool isBuilding;

	float buildTimer;
	float HP;

    [SerializeField] AudioClip[] hitSounds;

    AudioSource audio;

	MeshRenderer[] childMeshRenderers;
	MeshRenderer myMeshRenderer;

	List<Material> materials;

	public bool IsBuilt {
		get {
			return buildTimer >= 1.0f;
		}
	}

	public float BuiltPerc {
		get {
			return buildTimer / 1.0f;
		}
	}

	public float HPPerc {
		get {
			return HP / maxHP;
		}
	}

	private void OnEnable()
	{
        audio = GetComponent<AudioSource>();

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
			buildTimer += Time.deltaTime / timeToBuild;
			HP += Time.deltaTime * maxHP / timeToBuild;
			HP = Mathf.Clamp(HP, 0.0f, maxHP);

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

	void TryVehicleBayInteract()
	{
		VehicleBay vehicleBay;
		if (!TryGetComponent<VehicleBay>(out vehicleBay)) { return; }

		if (vehicleBay == null) { return; }

		vehicleBay.Interact();
	}

	public void Interact()
	{
		TryVehicleBayInteract();
	}

	public void TakeDamage(float amount) {
		HP -= amount;

        if (hitSounds.Length > 0) {
            audio.PlayOneShot(hitSounds[0], 0.5f);
        }

        if (HP <= 0.0f) {
			Destroy(gameObject);
        }
	}
}
