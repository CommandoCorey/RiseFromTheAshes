using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class FormationManager : MonoBehaviour
{
    [SerializeField]
    [Range(1, 10)]
    float spaceBetweenUnits = 1.5f;
    [SerializeField]
    [Range(1, 20)]
    int maxUnitsPerRow = 5;
    [SerializeField]
    int maxRows = 1000;

    List<Vector3> formationPositions = new List<Vector3>();
    private List<Vector3> playerRallyFormation = new List<Vector3>();
    private List<Vector3> aiRallyFormation = new List<Vector3>();
    private List<Vector3> searchedPositions = new List<Vector3>();

    [Header("Gizmos")]
    public bool showFormationPositions = false;
    public bool showAiRallyPositions = false;
    public bool showSearchedPositions = false;

    public static FormationManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    /// <summary>
    /// Create a formations for all the selected units based on the position the player clicked
    /// </summary>
    /// <param name="point">The target position on the map to created a formation around</param>
    /// <returns>A list of coordinates for all positions in the formation</returns>
    public List<Vector3> GetFormationPositions(Vector3 point, List<GameObject> units)
    {
        Vector3 unitCenter = new Vector3();
        RaycastHit rayHit;
        NavMeshHit navHit;        

        searchedPositions.Clear();
        formationPositions.Clear();

        foreach (GameObject unit in units)
        {
            unitCenter += unit.transform.position;
        }
        unitCenter /= units.Count;

        Vector3 moveDirection = (point - unitCenter).normalized;
        Vector3 offsetDirection = GetRightAngle(moveDirection);
        Vector3 position;
        int unitsOnLeft = 0;
        int unitsOnRight = 0;
        int unitsPlaced = 0;

        // make sure an object with the ground tag exists
        /*if (!GameObject.FindWithTag("Ground"))
        {
            Debug.LogError("The ground object has not been tagged");
            return formationPositions;
        }*/

        for (int row = 0; row < maxRows; row++)
        {
            // create the formation positions
            for (int column = 0; column < maxUnitsPerRow; column++)
            {
                if (column <= maxUnitsPerRow / 2)
                {
                    position = point + (offsetDirection * unitsOnRight * spaceBetweenUnits);
                    unitsOnRight++;
                }
                else
                {
                    unitsOnLeft++;
                    position = point - (offsetDirection * unitsOnLeft * spaceBetweenUnits);
                }

                //Debug.Log("Old position: " + position);
                position -= moveDirection * spaceBetweenUnits * row;
                //Debug.Log("New position: " + position);                

                searchedPositions.Add(position);

                // check that the position is not out of bounds
                if (Physics.Raycast(position + Vector3.up, Vector3.down, out rayHit))
                {
                    // check if the position is on the ground
                    if (rayHit.transform.gameObject.layer == 3 || rayHit.transform.tag == "Ground")
                    {
                        // Make sure point is on navmesh
                        // Note: maxDistance must be above agent radius else program will get stuck in loop forever
                        if (NavMesh.SamplePosition(rayHit.point, out navHit, 1.0f, NavMesh.AllAreas))
                        {
                            formationPositions.Add(navHit.position);
                            unitsPlaced++;
                        }
                    }

                }

                // exit loop if all units are placed
                if (unitsPlaced == units.Count)
                    return formationPositions;
            }

            unitsOnLeft = 0;
            unitsOnRight = 0;
        }

        return formationPositions;
    }

    /// <summary>
    /// Cretes formation positions for a group of units moving towards a specified point
    /// </summary>
    /// <param name="destination">The position on the map to move all of the units to</param>
    /// <param name="unitList">The list of transforms being moved</param>
    /// <returns></returns>
    public List<Vector3> GetFormationPositions(Vector3 destination, List<Transform> unitList)
    {
        Vector3 unitCenter = new Vector3();
        RaycastHit rayHit;
        NavMeshHit navHit;
        List<Vector3> formationPositions = new List<Vector3>();

        searchedPositions.Clear();

        foreach (Transform unit in unitList)
        {
            unitCenter += unit.position;
        }
        unitCenter /= unitList.Count;

        Vector3 moveDirection = (destination - unitCenter).normalized;
        Vector3 offsetDirection = GetRightAngle(moveDirection);
        Vector3 position;
        int unitsOnLeft = 0;
        int unitsOnRight = 0;
        int unitsPlaced = 0;

        for (int row = 0; row < maxRows; row++)
        {
            // create the formation positions
            for (int column = 0; column < maxUnitsPerRow; column++)
            {
                if (column <= maxUnitsPerRow / 2)
                {
                    position = destination + (offsetDirection * unitsOnRight * spaceBetweenUnits);
                    unitsOnRight++;
                }
                else
                {
                    unitsOnLeft++;
                    position = destination - (offsetDirection * unitsOnLeft * spaceBetweenUnits);
                }

                //Debug.Log("Old position: " + position);
                position -= moveDirection * spaceBetweenUnits * row;
                //Debug.Log("New position: " + position);               
                searchedPositions.Add(position);

                // check that the position is not out of bounds
                if (Physics.Raycast(position + Vector3.up, Vector3.down, out rayHit))
                {
                    // check if the position is on the ground
                    if (rayHit.transform.gameObject.layer == 3 || rayHit.transform.tag == "Ground")
                    {
                        // Make sure point is on navmesh
                        // Note: maxDistance must be above agent radius else program will get stuck in loop forever
                        if (NavMesh.SamplePosition(rayHit.point, out navHit, 1.0f, NavMesh.AllAreas))
                        {
                            formationPositions.Add(navHit.position);
                            unitsPlaced++;
                        }
                    }

                }
                else
                {
                    //Debug.Log("Raycast did not hit anything at position " + position);
                    break;
                }

                // exit loop if all units are placed
                if (unitsPlaced == unitList.Count)
                    return formationPositions;
            }

            unitsOnLeft = 0;
            unitsOnRight = 0;
        }

        if (unitsPlaced < unitList.Count)
        {
            Debug.LogError("Max Rows in formation exceeded");
        }

        return formationPositions;
    }

    public void AddRallyFormationPoint(Vector3 point, int player = 0)
    {
        if (player == 0)
            playerRallyFormation.Add(point);
        else if (player == 1)
            aiRallyFormation.Add(point);
    }

    public List<Vector3> GetRallyFormation(int player = 0)
    {
        if (player == 0)
            return playerRallyFormation;
        else
            return aiRallyFormation;
    }

    public void ClearRallyFormation(int player = 0)
    {
        if (player == 0)
            playerRallyFormation.Clear();
        else
            aiRallyFormation.Clear();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="rallyPoint"></param>
    /// <param name="player"></param>
    /// <returns></returns>
    public Vector3 GetRallyPosition(Vector3 rallyPoint, int player)
    {
        if (player == 0) // Human player
            return GetNextFormationPoint(playerRallyFormation, rallyPoint);

        else if (player == 1) // Ai player
            return GetNextFormationPoint(aiRallyFormation, rallyPoint);

        else
            return rallyPoint;
    }

    // used for agent priorities
    public int GetCurrentRallySize(int player)
    {
        if (player == 0)
            return playerRallyFormation.Count;

        else if (player == 1) // Ai player
            return aiRallyFormation.Count;

        return 0;
    }

    #region private functions
    private Vector3 GetNextFormationPoint(List<Vector3> formation, Vector3 centerPoint)
    {
        Vector3 lastPos;
        Vector3 newPos;

        if (formation.Count == 0)
        {
            formation.Add(centerPoint);
            return formation[0];
        }

        lastPos = formation.Last();
        newPos = lastPos;

        // check end of row
        if (formation.Count % maxUnitsPerRow == 0)
        {
            newPos.z = lastPos.z - spaceBetweenUnits;
            newPos.x = centerPoint.x;
        }
        // check if odd or even
        else if (formation.Count % 2 == 0)
        {
            int unitsOnRight = formation.Count % maxUnitsPerRow;
            newPos.x = lastPos.x + (unitsOnRight * spaceBetweenUnits);
        }
        else
        {
            int unitsOnLeft = formation.Count % maxUnitsPerRow;
            newPos.x = lastPos.x - (unitsOnLeft * spaceBetweenUnits);
        }

        formation.Add(newPos);
        return newPos;
    }

    private bool IsOutOfBounds(Vector3 position)
    {
        if (Physics.Raycast(position + Vector3.up * 10, Vector3.down, 10))
            return false;

        return true;
    }

    private Vector3 GetRightAngle(Vector3 current)
    {
        Vector3 newVector;
        newVector.x = -current.z;
        newVector.y = 0;
        newVector.z = current.x;

        return newVector;
    }
    #endregion

    private void OnDrawGizmos()
    {
        // draws the fromation positions that each unit will finish at
        if (formationPositions != null && showFormationPositions)
        {
            foreach (Vector3 position in formationPositions)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(position, 1);
            }
        }

        // draws all of the positions being searched when the formations are being created
        if (searchedPositions != null && showSearchedPositions)
        {
            foreach (Vector3 position in searchedPositions)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(position, 1);
            }
        }

        // shows the rally point position of each unit spawned from an a.i. vehicle bay
        if (aiRallyFormation != null && showAiRallyPositions)
        {
            foreach (Vector3 position in aiRallyFormation)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(position + Vector3.up * 0.5f, 1);
            }
        }

        /*
        if (searchedPositions != null)
        {
            Gizmos.color = Color.green;
            foreach (Vector3 position in searchedPositions)
            {                   
                if(position == searchedPositions.Last())
                    Gizmos.color = Color.blue;

                Gizmos.DrawWireCube(position, Vector3.one);
            }               
        }*/
    }

}