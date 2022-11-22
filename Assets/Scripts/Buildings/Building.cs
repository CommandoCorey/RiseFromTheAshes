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

	public GameObject selectionHighlight;

	[Header("Sound Effects")]
    [SerializeField] SoundEffect[] hitSounds;
	[SerializeField] SoundEffect[] destroySounds;

	[Header("Visual Effects")]
	[SerializeField] ParticleSystem[] hitVFX;
	[SerializeField] ParticleSystem[] destroyEffects;
	[SerializeField] Transform[] damagedVFX;

    new AudioSource audio;

	[HideInInspector] public Ghost ghost;

	MeshRenderer[] childMeshRenderers;
	MeshRenderer myMeshRenderer;

	List<Material> materials;

	// Added by Paul
	private bool damageEffectOn = false;
	private GameObject damagedEffect;
	[HideInInspector]
	public Transform ghostTransform;

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

		if (!startAtMaxHP)
		{
			HP = 0;
			isBuilding = true;
		}
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
				Outpost o;
				if (TryGetComponent(out o))
				{
					o.OnBuilt();
				}

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

		if (gameObject.layer != 8) // Aded by Paul
			return;

		vehicleBay.Interact();
	}

	public void OnDie()
	{
		TriggerBuilding trigger;
		if (TryGetComponent(out trigger))
		{
			trigger.OnDie();
		}

		if (ghost)
		{
			ghost.gameObject.SetActive(true);
			ghost.child = null;
		}

		if (ghostTransform)
		{
			ghostTransform.gameObject.SetActive(true);
		}

        // if the destroyed building is on the AIBuilding layer
		// and it is nto the headquarters rebuild it
        if (gameObject.layer == 9 && 
		    gameObject.tag != "Headquarters")
		{
			AiTaskScheduler aiTasks = AiTaskScheduler.Instance;

			if(aiTasks != null)
				aiTasks.AddRebuildTask(gameObject.tag, ghostTransform);
		}

		if(destroySounds.Length > 0)
        {
			int random = Random.Range(0, destroySounds.Length);

			GameManager.Instance.PlaySound(destroySounds[random].clip, 
									      destroySounds[random].volumeScale);
        }

		if (destroyEffects.Length > 0)
		{
			int random = Random.Range(0, destroyEffects.Length - 1);
			var fx = Instantiate(destroyEffects[random], transform.position, Quaternion.identity);
			Destroy(fx, 10.0f);
		}

		Destroy(gameObject);
	}

	public void Interact()
	{
		// turn on selection highlight if player building
		if (gameObject.layer == 8)
		{
			SelectionManager.Instance.SetSelectedBuilding(this);
		}

        TryVehicleBayInteract();
	}

	public void TakeDamage(Vector3 hitPoint, float amount) {
		HP -= amount;

        if (hitSounds.Length > 0) {
            audio.PlayOneShot(hitSounds[Random.Range(0, hitSounds.Length - 1)].clip,
				hitSounds[Random.Range(0, hitSounds.Length - 1)].volumeScale);
        }

		if (hitVFX.Length > 0)
		{
			var go = Instantiate(hitVFX[Random.Range(0, hitVFX.Length - 1)], hitPoint, Quaternion.identity);
			Destroy(go.gameObject, 3.0f);
		}

		// turn of fire if healh is below 50%
		if(!damageEffectOn && HP <= maxHP/2 && damagedVFX.Length > 0)
        {
			int randomPick = Random.Range(0, damagedVFX.Length - 1);

			damagedEffect = damagedVFX[randomPick].gameObject;
			damagedEffect.SetActive(true);

			damageEffectOn = true;
		}
		else if(damageEffectOn && damagedEffect.activeInHierarchy && HP > maxHP/2)
        {
			damagedEffect.SetActive(false);
			damageEffectOn = false;

			damagedEffect = null;
        }


        if (HP <= 0.0f) {
			OnDie();
        }
	}

}
