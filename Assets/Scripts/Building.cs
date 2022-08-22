using UnityEngine;

public class Building : MonoBehaviour
{
	public Material[] OriginalMaterials {  get; private set; }

	[SerializeField] bool built = false;

	public void Start()
	{
		OriginalMaterials = (Material[])GetComponent<MeshRenderer>().materials.Clone();

		if (!built)
        {
			GetComponent<MeshRenderer>().materials = new Material[] { BuildingManager.Instance.ghostMaterial };
		}
	}

	public void Build()
    {
		if (built) { return; }

		GetComponent<MeshRenderer>().materials = (Material[])OriginalMaterials.Clone();
		built = true;
    }
}
