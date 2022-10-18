using UnityEngine;
using System.Collections.Generic;

public class Building : MonoBehaviour
{
	public string buildingName;
	public int steelCost;
	public float timeToBuild = 1.0f;
	public float maxHP = 100.0f;
	[SerializeField] bool startAtMaxHP = false;
	public string buildingDescription;
	public bool aiBuilding;

	bool isBuilding;

	float buildTimer;
	public float HP;

	[Header("Sound and Visual effects")]
    [SerializeField] AudioClip[] hitSounds;
	[SerializeField] ParticleSystem[] hitVFX;

    new AudioSource audio;

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

	public bool IsBuilding {
		get => isBuilding;
		set => isBuilding = value; 
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

	void Start()
    {
		if (startAtMaxHP)
			HP = maxHP;
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

	public void TakeDamage(Vector3 hitPoint, float amount) {
		HP -= amount;

        if (hitSounds.Length > 0) {
            audio.PlayOneShot(hitSounds[Random.Range(0, hitSounds.Length - 1)], 0.5f);
        }

		if (hitVFX.Length > 0)
		{
			var go = Instantiate(hitVFX[Random.Range(0, hitVFX.Length - 1)], hitPoint, Quaternion.identity);
			Destroy(go.gameObject, 3.0f);
		}

        if (HP <= 0.0f) {
			Destroy(gameObject);
        }
	}
}
