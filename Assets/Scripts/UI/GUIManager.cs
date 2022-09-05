using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GUIManager : MonoBehaviour
{
    public Transform buttonPanel;
    public Transform unitPanal;
    public Transform statsPanel;

    public GameObject unitIconPrefab;

    private List<UnitController> selectedUnits;
    private List<GameObject> unitIcons;

    // Start is called before the first frame update
    void Start()
    {
        unitIcons = new List<GameObject>();
        selectedUnits = new List<UnitController>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateUnitHealth();
    }

    private void UpdateUnitHealth()
    {
        if (selectedUnits != null)
        {
            for (int i = 0; i < selectedUnits.Count; i++)
            {
                if (selectedUnits[i] == null)
                {
                    GameObject.Destroy(unitIcons[i]);

                    selectedUnits.RemoveAt(i);
                    unitIcons.RemoveAt(i);
                    return;
                }

                TextMeshProUGUI[] healthText = unitIcons[i].GetComponentsInChildren<TextMeshProUGUI>();
                healthText[0].text = selectedUnits[i].CurrentHealth.ToString();

                var healthBar = unitIcons[i].GetComponentInChildren<ProgressBar>();
                healthBar.progress = selectedUnits[i].CurrentHealth / selectedUnits[i].MaxHealth;
            }

        }
    }

    /// <summary>
    /// populates the unit panel with unit icons alongs with their health
    /// </summary>
    /// <param name="selection">The list of unit game objects to use when populating the panel</param>
    public void GenerateUnitIcons(List<GameObject> selection)
    {
        ClearUnitSelection();

        // generate new icons
        for (int i=0; i < selection.Count; i++)
        {
            unitIcons.Add(Instantiate(unitIconPrefab, unitPanal));
            selectedUnits.Add(selection[i].GetComponent<UnitController>());

            //var maxHalth = unitIcons[unitIcons.Count - 1].transform.Find("Max Health").GetComponent<TextMeshProUGUI>();
            //var currentHealth = unitIcons[unitIcons.Count - 1].transform.Find("Current Health").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI[] healthText = unitIcons[i].GetComponentsInChildren<TextMeshProUGUI>();

            healthText[2].text = selectedUnits[i].MaxHealth.ToString();
            healthText[0].text = selectedUnits[i].CurrentHealth.ToString();

            unitIcons[i].GetComponentInChildren<Image>().sprite = selectedUnits[i].GuiIcon;

            var healthBar = unitIcons[i].GetComponentInChildren<ProgressBar>();
            healthBar.progress = selectedUnits[i].CurrentHealth / selectedUnits[i].MaxHealth;
        }
    }

    /// <summary>
    /// Removes all unit information from the GUI and clears the lists
    /// </summary>
    public void ClearUnitSelection()
    {
        // Destroy existing icons on GUI
        foreach (var icon in unitIcons)
        {
            GameObject.Destroy(icon);
        }

        // clear the lists
        unitIcons.Clear();
        selectedUnits.Clear();
    }


}
