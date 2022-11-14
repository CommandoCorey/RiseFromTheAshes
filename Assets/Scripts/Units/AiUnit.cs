using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UnitMode
{
    Offensive, Intermediate, Defensive
}

public class AiUnit : MonoBehaviour
{

    public UnitMode Mode { get; set; } = UnitMode.Intermediate;

    UnitController unit;
    AiPlayer ai;

    // Start is called before the first frame update
    void Start()
    {
        ai = AiPlayer.Instance;
        unit = GetComponent<UnitController>();
    }

    // Update is called once per frame
    void Update()
    {
        // Check for outpost ghosts the unit is A.I. controlled
        if (gameObject.layer == 7)
            SearchForOutposts();
    }

    /// <summary>
    /// 
    /// </summary>
    public void AttackClosestHQEnemy()
    {
        // get the closest enemy
        Transform closest = null;
        float shortestDistance = float.MaxValue;        

        foreach (UnitController enemy in ai.UnitsAttackingHQ)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                closest = enemy.transform;
            }
        }

        if(closest != null)
        {
            unit.AttackTarget = closest;
            unit.AttackOrderGiven = true;
            unit.ChangeState(UnitState.Follow);
        }
    }

    //------------------------------------------------------------------
    // Checks if there is an outpost placeholder in detection range that
    // the AiPlayer has not already found
    // If there is the Aiplayer adds it to the outpost placeholder list
    //------------------------------------------------------------------
    private void SearchForOutposts()
    {
        var aiPlayer = AiPlayer.Instance;
        var ghostBuildings = Physics.OverlapSphere(transform.position, unit.DetectionRadius, 22); // 22 = buildable layer

        foreach (Collider ghost in ghostBuildings)
        {
            // check that the placeholder is an outpost placeholder and that
            // the ai player does not already have it
            if (ghost.gameObject.tag == "Outpost" &&
                !aiPlayer.HasOutpostGhost(ghost.transform))
            {
                aiPlayer.AddOutpost(ghost.transform);
            }
        }
    }

}
