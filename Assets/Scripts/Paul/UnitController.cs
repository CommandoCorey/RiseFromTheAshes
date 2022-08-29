using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum UnitState
{
    Idle,
    Moving,
    Flock,
    Attack
}

public class UnitController : MonoBehaviour
{
    public GameObject selectionHighlight;
    public Transform turret;
    public Transform firingPoint;
    public RectTransform healthBar;

    #region variable declartion
    [Header("Unit Stats")]
    [SerializeField]
    float maxHealth = 100;
    [SerializeField]
    float movementSpeed = 1.0f;
    [SerializeField]
    float damagePerHit = 1.0f;
    [SerializeField]
    float attackRate = 1.0f;
    [SerializeField]
    float attackRange = 20.0f;
    [SerializeField] 
    float turretRotationSpeed = 1.0f;

    [Header("Enemy Detection")]
    [SerializeField] float detectionRadius = 30.0f;
    [SerializeField] LayerMask detectionLayer;

    [Header("Gizmos")]
    [SerializeField] bool showDetectionRadius = true;
    [SerializeField] bool showAttackRange = true;

    // private variables
    [SerializeField]
    private float health;
    private Vector3[] waypoints;

    // state classed
    private IdleState idleState;
    private SeekState moveState;
    private FlockState flockState;
    private CombatState attackState;

    private Color drawColor = Color.white;
    private Vector3 formationTarget;

    private float widthPerHealth;
    #endregion

    // properties
    public UnitState State { get; private set; }
    public GameObject AttackTarget { get; set; }
    public float DetectionRadius { get => detectionRadius; }
    public LayerMask DetectionLayer { get => detectionLayer; }
    public float Speed { get => movementSpeed; }  
    public float TurretRotationSpeed { get => turretRotationSpeed; }
    public float DamagePerHit { get => damagePerHit; }
    public float AttackRate { get => attackRate; }
    public float AttackRange {  get => attackRange; }

    public void SetPath(Vector3[] waypoints)
    {
        this.waypoints = waypoints;
    }

    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;

        widthPerHealth = healthBar.rect.width / health;

        ChangeState(UnitState.Idle);
    }

    // Update is called once per frame
    void Update()
    {      
        healthBar.sizeDelta = new Vector2(widthPerHealth * health, healthBar.rect.height);

        if (health <= 0)
        {
            GameObject.Destroy(this.gameObject);
        }
    }    

    /// <summary>
    /// 
    /// </summary>
    /// <param name="selected"></param>
    public void SetSelected(bool selected)
    {
        selectionHighlight.SetActive(selected);
    }

    /// <summary>
    /// Removes health from the unit by a specified amoount
    /// </summary>
    /// <param name="amount">the amount of HP to remove</param>
    public void TakeDamage(float amount)
    {
        health -= amount;
    }

    public void ChangeState(UnitState newState, Vector3 target = new Vector3())
    {
        State = newState;

        switch (newState)
        {
            case UnitState.Idle:
                if (GetComponent<IdleState>() == null)
                {
                    idleState = gameObject.AddComponent<IdleState>();
                }

                Destroy(flockState);
                Destroy(moveState);
                Destroy(attackState);

                drawColor = Color.white;
                break;

            case UnitState.Moving:

                if (GetComponent<SeekState>() == null)
                {
                    moveState = gameObject.AddComponent<SeekState>();
                }

                Destroy(idleState);
                Destroy(flockState);
                Destroy(attackState);

                //moveState.Target = target;
                moveState.MoveTo(target);

                drawColor = Color.red;
            break;

            case UnitState.Flock:
                if (GetComponent<FlockState>() == null)
                {
                    flockState = gameObject.AddComponent<FlockState>();
                }

                Destroy(idleState);
                Destroy(moveState);
                Destroy(attackState);

                flockState.Target = target;
                //flockState.FormationTarget = formationTarget;

                drawColor = Color.blue;
                break;

            case UnitState.Attack:
                if (GetComponent<CombatState>() == null)
                {
                    attackState = gameObject.AddComponent<CombatState>();
                }

                Destroy(idleState);
                Destroy(moveState);
                Destroy(flockState);
            break;
        }
    }

    private void OnDrawGizmos()
    {
        if (showDetectionRadius)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }

        if (showAttackRange)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }

}
