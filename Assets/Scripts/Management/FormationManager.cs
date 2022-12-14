using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class FormationManager : MonoBehaviour
{
    [SerializeField]
    [Range(1, 10)]
    float spaceBetweenUnits = 4.5f;
    [SerializeField][Range(1, 10)]
    float spaceBetweenRows = 3.0f;
    [SerializeField]
    [Range(1, 20)]
    int maxUnitsPerRow = 5;
    [SerializeField]
    int maxRows = 1000;

    [SerializeField]
    Vector3 boxSize = Vector3.one;

    [Header("Casting")]
    [SerializeField] float castHeight = 10;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] LayerMask environmentLayer;

    [Header("Gizmos")]
    public bool showFormationPositions = false;
    public bool showPlayerRallyPositions = false;
    public bool showAiRallyPositions = false;
    public bool showSearchedPositions = false;    

    List<Vector3> formationPositions = new List<Vector3>();
    private Dictionary<int, Vector3> playerRallyFormation = new Dictionary<int, Vector3>();
    private Dictionary<int, Vector3> aiRallyFormation = new Dictionary<int, Vector3>();
    private List<Vector3> searchedPositions = new List<Vector3>();

    private Vector3 playerRallyPosition = new Vector3();
    private Vector3 aiRallyPosition = new Vector3();

    private int playerId = 0;

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
                if (Physics.Raycast(position + Vector3.up, Vector3.down, out rayHit, groundLayer))
                {
                    // Make sure point is on navmesh
                    // Note: maxDistance must be above agent radius else program will get stuck in loop forever
                    if (NavMesh.SamplePosition(rayHit.point, out navHit, 1.0f, NavMesh.AllAreas))
                    {
                        formationPositions.Add(navHit.position);
                        unitsPlaced++;
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
                if (Physics.Raycast(position + Vector3.up, Vector3.down, out rayHit, groundLayer))
                {
                    // Make sure point is on navmesh
                    // Note: maxDistance must be above agent radius else program will get stuck in loop forever
                    if (NavMesh.SamplePosition(rayHit.point, out navHit, 1.0f, NavMesh.AllAreas))
                    {
                        formationPositions.Add(navHit.position);
                        unitsPlaced++;
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public Dictionary<int, Vector3> GetRallyFormation(int player = 0)
    {
        if (player == 0)
            return playerRallyFormation;
        else
            return aiRallyFormation;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="player"></param>
    public void ClearRallyFormation(int player = 0)
    {
        if (player == 0)
        {
            playerRallyFormation.Clear();
            playerId = 0;
        }
        else
        {
            aiRallyFormation.Clear();
        }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="rallyPoint"></param>
    /// <param name="origin"></param>
    /// <param name="player"></param>
    /// <param name="rallyNumber"></param>
    /// <returns></returns>
    public Vector3 GetRallyPosition(Vector3 rallyPoint, Vector3 origin, bool aiPlayer, ref int rallyNumber)
    {
        Vector3 moveDirection = (rallyPoint - origin).normalized;

        Debug.DrawLine(origin, rallyPoint, Color.yellow, 3.0f);

        if (aiPlayer) // Ai player
            //return GetNextAiFormationPoint(rallyPoint, moveDirection, ref rallyNumber);
            return GetNextFormationPoint(rallyPoint, moveDirection, ref rallyNumber, aiRallyFormation, 
                   out aiRallyPosition);
        else
            return GetNextFormationPoint(rallyPoint, moveDirection, ref rallyNumber, playerRallyFormation,
                   out playerRallyPosition);
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="position"></param>
    /// <param name="aiPlayer"></param>
    public void RemovePositionFromRally(int positionNum, bool aiPlayer)
    {
        try
        {
            if (aiPlayer && aiRallyFormation.Count > 0)
            {
                //Debug.Log("Ai unit has moved from position: " + aiRallyFormation[positionNum]);
                aiRallyFormation.Remove(positionNum);
            }
            else if (!aiPlayer && playerRallyFormation.Count > 0)
            {
                //Debug.Log("Player unit has moved from position: " + playerRallyFormation[positionNum]);
                playerRallyFormation.Remove(positionNum);

                if (aiRallyFormation.Count == 0)
                    playerId = 0;
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    #region private functions
    private Vector3 GetNextFormationPoint(Vector3 centerPoint, Vector3 direction, ref int rallyNumber,
            Dictionary<int, Vector3> rallyFormations, out Vector3 rallyPosition)
    {
        int unitsOnLeft = 0;
        int unitsOnRight = 0;

        bool hitEnvironment = false;
        bool oddFailed = false;
        bool evenFailed = false;

        rallyPosition = centerPoint;

        Vector3 offsetDirection = GetRightAngle(direction);
        Vector3 offset;

        Debug.DrawLine(centerPoint, centerPoint + (offsetDirection * 10), Color.red, 3.0f);

        searchedPositions.Clear();

        for (int row = 0; row < maxRows; row++)
        {
            //Debug.Log("Row " + row);

            for (int col = 0; col < maxUnitsPerRow; col++)
            {
                //Debug.Log("Column " + col);

                if (Physics.BoxCast(rallyPosition + Vector3.up * castHeight, boxSize/2, 
                    Vector3.down, out RaycastHit hitInfo))               

                //if (Physics.Raycast(rallyPosition + Vector3.up * 10, Vector3.down,
                    //out RaycastHit hitInfo, Mathf.Infinity))
                {
                    //Debug.DrawLine(rallyPosition + Vector3.up * castHeight, hitInfo.point, Color.red, 2.0f);

                    int layerMask = 1 << hitInfo.transform.gameObject.layer;

                    if (layerMask == groundLayer && !rallyFormations.ContainsValue(hitInfo.point))
                    {
                        rallyNumber = playerId;
                        rallyFormations.Add(playerId++, hitInfo.point);

                        //Debug.Log("Row: " + row + ", Column: " + col);
                        //Debug.Log("Spot Found");                           
                        return hitInfo.point;
                    }
                    else if(layerMask == environmentLayer)
                    {
                        hitEnvironment = true;
                    }

                    //Debug.Log("Hit" + hitInfo.transform.gameObject.name);

                    searchedPositions.Add(hitInfo.point);
                }                

                if (col % 2 == 0) // check odd or even
                {
                    unitsOnRight++;                   
                    offset = -offsetDirection * unitsOnRight * spaceBetweenUnits;

                    if(hitEnvironment)
                        evenFailed = true;
                }
                else
                {
                    unitsOnLeft++;
                    offset = offsetDirection * unitsOnLeft * spaceBetweenUnits;

                    if(hitEnvironment)
                        oddFailed = true;
                }

                if (hitEnvironment && oddFailed && evenFailed)
                {
                    break; // move to next row
                }

                rallyPosition = centerPoint - (direction * row);
                rallyPosition += offset;
            }            

            // move position to center of next row
            rallyPosition = centerPoint;
            rallyPosition -= direction * (spaceBetweenRows * (row + 1));

            //Debug.Log("Next row at " + rallyPosition);

            unitsOnLeft = 0;
            unitsOnRight = 0;

            hitEnvironment = false;
            oddFailed = false;
            evenFailed = false;
        }

        Debug.LogError("Max rows exceeded. Could not find a valid rally formation position.");

        rallyNumber = playerId;
        rallyFormations.Add(playerId++, centerPoint);

        return centerPoint;
    }

    // Checks if the player clicked outside of the map by
    // seeing if a raycase hit anything
    private bool IsOutOfBounds(Vector3 position)
    {
        if (Physics.Raycast(position + Vector3.up * 10, Vector3.down, 10))
            return false;

        return true;
    }

    // Take a vector and returns another vector that is a right-angle to it
    private Vector3 GetRightAngle(Vector3 current)
    {
        Vector3 newVector;
        newVector.x = -current.z;
        newVector.y = 0;
        newVector.z = current.x;

        return newVector;
    }

    /*
    private void CheckUnitsMoved()
    {
        foreach(Vector3 position in playerRallyFormation)
        {
            if (Physics.Raycast(playerRallyPosition + Vector3.up * 2, Vector3.down,
                out RaycastHit hitInfo))
            {
                if(hitInfo.transform.gameObject.layer == 3)
            }
        }
    }*/

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
                //Gizmos.DrawWireSphere(position, 1);
                Gizmos.DrawWireCube(position, Vector3.one);
            }
        }

        // shows the rally point position of each unit spawned from a player's vehicle bay
        /*if (playerRallyFormation != null && showPlayerRallyPositions)
        {
            foreach (Vector3 position in playerRallyFormation)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(position + Vector3.up * 0.5f, 1);
            }
        }*/

        if (showPlayerRallyPositions)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(playerRallyPosition + Vector3.up * 0.5f, 1);
        }

        // shows the rally point position of each unit spawned from an a.i. vehicle bay
        if (aiRallyFormation != null && showAiRallyPositions)
        {
            foreach (Vector3 position in aiRallyFormation.Values)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(position + Vector3.up * 0.5f, 1);
            }
        }

    }

}