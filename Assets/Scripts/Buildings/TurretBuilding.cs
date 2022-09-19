using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretBuilding : Building
{
    enum TurretState
    {
        Searching, Aiming, Firing
    }

    [Header("Transforms")]
    public Transform turret;
    public Transform gun;
    public Transform firingPoint;
    public ParticleSystem fireParticles;

    [Header("Stats")]
    [SerializeField] 
    float detectionRadius = 10;
    [SerializeField] 
    float aimSpeed = 0.5f;
    [SerializeField]
    float damagePerShot = 10;
    [SerializeField]
    float fireRate = 1;

    [Header("Layers")]
    [SerializeField] LayerMask enemyLayer;

    [Header("Gizmos")]
    [SerializeField] bool showDetectionRadius;

    private TurretState state = TurretState.Searching;
    private Transform target = null;

    private Vector3 faceDirection;
    private Quaternion lookRotation;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        switch(state)
        {
            case TurretState.Searching: SearchForUnits(); break;
            case TurretState.Aiming: Aim(); break;
            case TurretState.Firing:
                if (!Physics.Raycast(firingPoint.position, turret.forward, out RaycastHit hit, 30) && hit.transform == target)
                {
                    Debug.Log("Searching");
                    SearchForUnits();
                    state = TurretState.Searching;
                }
            break;
        }
    }

    private void SearchForUnits()
    {
        // check for other units in vision radius
        var unitsInRange = Physics.OverlapSphere(transform.position, detectionRadius, enemyLayer);

        if (unitsInRange.Length > 0)
        {
            target = GetClosestEnemy(unitsInRange);
            //Debug.Log("Aiming");
            state = TurretState.Aiming;
        }
    }

    private void Aim()
    {
        faceDirection = (target.position - turret.position).normalized;
        lookRotation = Quaternion.LookRotation(faceDirection);

        turret.rotation = Quaternion.RotateTowards(turret.rotation, lookRotation, Time.deltaTime * aimSpeed);
        
        if(turret.rotation == lookRotation)
        {
            //Debug.Log("Facing target");
            state = TurretState.Firing;
            Invoke("Fire", fireRate);
        }
    }

    private void Fire()
    {
        if (Physics.Raycast(firingPoint.position, turret.forward, out RaycastHit hit, 30) && hit.transform == target)
        {
            target.GetComponentInParent<UnitController>().TakeDamage(damagePerShot);
            Invoke("Fire", fireRate);
        }
        else
        {
            state = TurretState.Searching;
            //Debug.Log("Searching");
        }
    }

    // Searches through all detected enemies in the overlap sphere and returns the transform
    // of the one that is the closest
    private Transform GetClosestEnemy(Collider[] enemies, Transform current = null)
    {
        Transform closest;
        float shortestDistance;

        if (current != null)
        {
            closest = current;
            shortestDistance = Vector3.Distance(transform.position, current.position);
        }
        else
        {
            closest = enemies[0].transform;
            shortestDistance = Vector3.Distance(transform.position, enemies[0].transform.position);
        }

        foreach (Collider enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                closest = enemy.transform;
            }
        }

        return closest;
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        Gizmos.color = Color.red;
        Gizmos.DrawLine(firingPoint.position, firingPoint.position + turret.forward * 30);

        if (showDetectionRadius)
        {
            UnityEditor.Handles.color = new Color(Color.yellow.r, Color.yellow.g, Color.yellow.b, 0.01f);
            UnityEditor.Handles.DrawSolidDisc(transform.position, Vector3.up, detectionRadius);
        }
#endif
    }

}