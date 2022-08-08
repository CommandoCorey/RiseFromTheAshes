using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedDictionary : MonoBehaviour
{
    // contains all of the selected units
    public Dictionary<int, GameObject> selectedTable = new Dictionary<int, GameObject> ();

    /// <summary>
    /// Gets an instant ID from game object and addes it to the unit selection
    /// if it is not already in there
    /// </summary>
    /// <param name="go">The unit to add to the selection</param>
    public void AddSelected(GameObject go)
    {
        int id = go.GetInstanceID();

        // check if the game object is already in the dictionary
        if(!(selectedTable.ContainsKey(id)))
        {
            selectedTable.Add(id, go);

            if (go.GetComponent<SelectionComponent>() != null)
            {
                go.GetComponent<SelectionComponent>().SetHighlight(true);
                //Debug.Log("Added " + id + " to selected dict");
            }
        }
    }

    /// <summary>
    /// Removes a specific unit from the selection specified by an id
    /// </summary>
    /// <param name="id">The instance id of the game object to be deselected</param>
    public void Deselect(int id)
    {
        //Destroy(selectedTable[id].GetComponent<SelectionComponent>());
        selectedTable[id].GetComponent<SelectionComponent>().SetHighlight(false);
        selectedTable.Remove(id);
    }

    /// <summary>
    /// Remove all units from the selection
    /// </summary>
    public void DeselectAll()
    {
        // looks trhough every value in the dictionary and if it is
        // not null, destroys it
        foreach(KeyValuePair<int, GameObject> pair in selectedTable)
        {
            if(pair.Value != null)
            {
                //Destroy(selectedTable[pair.Key].GetComponent<SelectedDictionary>());
                selectedTable[pair.Key].GetComponent<SelectionComponent>().SetHighlight(false);                
            }
        }
        selectedTable.Clear(); // clears the whole dictionary
    }
}
