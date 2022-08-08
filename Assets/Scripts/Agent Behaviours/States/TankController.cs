using UnityEngine;

public class TankController : MonoBehaviour
{
    public Transform turret;
    public Transform firingPoint;

    [SerializeField] float rotateSpeed = 1.0f;
    [SerializeField] float detectionRadius = 1.0f;
    [SerializeField] float damagePerHit = 0.1f;

    public LayerMask detectionLayer;

    private Transform target;
    private Quaternion lookRotation;
    private Vector3 direction;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var unitsInRange = Physics.OverlapSphere(transform.position, detectionRadius, detectionLayer);

        //rotate us over time according to speed until we are in the required rotation
        turret.rotation = Quaternion.Slerp(turret.rotation, lookRotation, Time.deltaTime * rotateSpeed);

        if (unitsInRange.Length > 0)
        {
            //Debug.Log("Enemy target detected");
            target = unitsInRange[0].transform;
            RaycastHit hit;

            // check if turret is pointing at target
            if (Physics.Raycast(firingPoint.position, turret.forward, out hit) && hit.transform == target)            
            {
                Debug.DrawLine(turret.position, target.position, Color.yellow);
                target.GetComponent<Agent>().SubtractHealth(damagePerHit);
            }
            else
            {
                //find the vector pointing from our position to the target
                direction = (target.position - turret.position).normalized;                
            }
        }
        else
        {
            target = null;
            direction = transform.forward;
        }

        //create the rotation we need to be in to look at the target
        lookRotation = Quaternion.LookRotation(direction);

    }

}
