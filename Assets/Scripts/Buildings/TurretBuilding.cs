using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretBuilding : MonoBehaviour
{
    enum TurretState
    {
        Searching, Aiming, Firing
    }

    [Header("Transforms")]
    public Transform turretGun;
    public Transform firingPoint;
    public Transform fireEffect;

    [Header("Stats")]
    [SerializeField][Range(1, 100)]
    float detectionRadius = 10;
    [SerializeField][Range(1, 100)]
    float aimSpeed = 30;
    [SerializeField][Range(1, 100)]
    float damagePerShot = 10;
    [SerializeField][Range(0.1f, 10)]
    float fireRate = 1;
    [SerializeField][Range(-180, 180)]
    float defaultAimAngle = 90;

    //[SerializeField]
    Vector3 defaultAimDirection;

    [Header("Layers")]
    [SerializeField] LayerMask enemyLayer;

    [Header("Gizmos")]
    [SerializeField] bool showDetectionRadius;

    private TurretState state = TurretState.Searching;
    private Transform target = null;

    private Vector3 faceDirection;
    private Quaternion lookRotation;

    private Building building;

    // Start is called before the first frame update
    void Start()
    {
        building = GetComponent<Building>();

        defaultAimDirection.x = Mathf.Cos(defaultAimAngle * Mathf.Deg2Rad);
        defaultAimDirection.z = Mathf.Sin(defaultAimAngle * Mathf.Deg2Rad);
    }

    // Update is called once per frame
    void Update()
    {
        if (!building.IsBuilt)
            return;

        switch(state)
        {
            case TurretState.Searching: SearchForUnits(); break;
            case TurretState.Aiming: Aim(); break;
            case TurretState.Firing:
                if (!Physics.Raycast(firingPoint.position, turretGun.forward, out RaycastHit hit, 30) && hit.transform == target)
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
            return;
        }

        lookRotation = Quaternion.LookRotation(defaultAimDirection);

        if (turretGun.rotation != lookRotation)
        {
            turretGun.rotation = Quaternion.RotateTowards(turretGun.rotation, lookRotation, aimSpeed * Time.deltaTime);
        }
    }

    private void Aim()
    {
        if (target == null)
        {
            state = TurretState.Searching;
            return;
        }

        faceDirection = (target.position - turretGun.position).normalized;
        lookRotation = Quaternion.LookRotation(faceDirection);

        turretGun.rotation = Quaternion.RotateTowards(turretGun.rotation, lookRotation, Time.deltaTime * aimSpeed);
        
        if(turretGun.rotation == lookRotation)
        {
            //Debug.Log("Facing target");
            state = TurretState.Firing;
            Invoke("Fire", fireRate);
        }

        if(Vector3.Distance(transform.position, target.position) > detectionRadius)
        {
            state = TurretState.Searching;
        }
    }

    private void Fire()
    {
        if (Physics.Raycast(firingPoint.position, turretGun.forward, out RaycastHit hit, 30))
        {
            if (hit.transform == target)
            {
                Instantiate(fireEffect, firingPoint.position, firingPoint.rotation, transform);

                int layer = target.gameObject.layer;

                if (layer == 6 || layer == 7)
                    target.GetComponentInParent<UnitController>().TakeDamage(damagePerShot, hit.point);
                else if (layer == 8 || layer == 9)
                    target.GetComponent<Building>().TakeDamage(hit.point, damagePerShot);

                Invoke("Fire", fireRate);

                return;
            }
        }

        state = TurretState.Searching;
        //Debug.Log("Searching");
        
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
        Gizmos.DrawLine(firingPoint.position, firingPoint.position + turretGun.forward * 30);

        if (showDetectionRadius)
        {
            UnityEditor.Handles.color = Color.red;
            //UnityEditor.Handles.color = new Color(Color.yellow.r, Color.yellow.g, Color.yellow.b, 0.01f);
            //UnityEditor.Handles.DrawSolidDisc(transform.position, Vector3.up, detectionRadius);
            UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, detectionRadius);
        }
#endif
    }

}