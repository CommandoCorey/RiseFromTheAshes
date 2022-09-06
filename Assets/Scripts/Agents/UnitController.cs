using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public enum UnitState
{
    Idle,
    Halt,
    Moving,
    Flock,
    Attack
}

public class UnitController : MonoBehaviour
{
    [SerializeField] string unitName;

    #region variable declartion
    [Header("Game Objects and transforms")]
    public GameObject selectionHighlight;
    public Transform turret;
    public Transform castingPoint;
    public Transform firingPoint;    
    public Sprite guiIcon;

    [Header("External Scripts")]
    public ProgressBar healthBar;
    
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
    [SerializeField] LayerMask environmentLayer;
    [SerializeField] float haltTime = 5.0f;

    [Header("Gizmos")]
    [SerializeField] bool showDetectionRadius = true;
    [SerializeField] bool showAttackRange = true;

    // private variables
    [SerializeField]
    private float health;
    private Vector3[] waypoints;

    // state classed
    private IdleState idleState;
    private HaltState haltState;
    private SeekState moveState;
    private FlockState flockState;
    private CombatState attackState;

    private Color drawColor = Color.white;
    private Vector3 formationTarget;
    #endregion

    # region properties
    public UnitState State { get; private set; }
    public Transform AttackTarget { get; set; }
    public float DetectionRadius { get => detectionRadius; }
    public LayerMask DetectionLayer { get => detectionLayer; }
    public LayerMask EnvironmentLayer { get => environmentLayer; }
    public Sprite GuiIcon { get => guiIcon; }
    public float HaltTime { get => haltTime; }

    // unit stats
    public string Name { get => unitName; }
    public float MaxHealth { get => maxHealth; }
    public float CurrentHealth {  get=> health; }
    public float Speed { get => movementSpeed; }  
    public float TurretRotationSpeed { get => turretRotationSpeed; }
    public float DamagePerHit { get => damagePerHit; }
    public float AttackRate { get => attackRate; }
    public float AttackRange {  get => attackRange; }
    #endregion

    public void SetPath(Vector3[] waypoints)
    {
        this.waypoints = waypoints;
    }

    // Start is called before the first frame update
    void Start()
    {
    	health = maxHealth;

        ChangeState(UnitState.Idle);
    }

    // Update is called once per frame
    void Update()
    {      
        healthBar.progress = health / maxHealth;

        if (health <= 0)
        {
            GameObject.Destroy(this.gameObject);
        }
    }    

    /// <summary>
    /// Toggles the visibility of the unit selection highlight and health bar
    /// </summary>
    /// <param name="selected">true or false value</param>
    public void SetSelected(bool selected)
    {
        selectionHighlight.SetActive(selected);
        healthBar.gameObject.SetActive(selected);
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
        // Destroy script corrensponding to current state
        switch (State)
        {
            case UnitState.Idle: Destroy(idleState); break;
            case UnitState.Halt: Destroy(haltState); break;
            case UnitState.Moving: Destroy(moveState); break;
            case UnitState.Flock: Destroy(flockState); break;
            case UnitState.Attack: Destroy(attackState); break;            
        }

        State = newState;

        switch (newState)
        {
            case UnitState.Idle:
                if (GetComponent<IdleState>() == null)
                {
                    idleState = gameObject.AddComponent<IdleState>();
                }

                drawColor = Color.white;
                break;

            case UnitState.Halt:
                if(GetComponent<HaltState>() == null)
                {
                    haltState = gameObject.AddComponent<HaltState>();
                }
            break;

            case UnitState.Moving:

                if (GetComponent<SeekState>() == null)
                {
                    moveState = gameObject.AddComponent<SeekState>();
                }

                //moveState.Target = target;
                moveState.MoveTo(target);

                drawColor = Color.red;
            break;

            case UnitState.Flock:
                if (GetComponent<FlockState>() == null)
                {
                    flockState = gameObject.AddComponent<FlockState>();
                }

                flockState.Target = target;
                //flockState.FormationTarget = formationTarget;

                drawColor = Color.blue;
                break;

            case UnitState.Attack:
                if (GetComponent<CombatState>() == null)
                {
                    attackState = gameObject.AddComponent<CombatState>();
                }
            break;
        }
    }

    // Removes unit from lists in unit manager and GUI once it is destroyed
    private void OnDestroy()
    {
        var unitGui = GameObject.FindObjectOfType<UnitGui>();
        var unitManager = GameObject.FindObjectOfType<UnitManager>();

        if (unitManager != null)
            unitManager.RemoveFromSelection(this.gameObject);

        if (unitGui != null)        
            unitGui.RemoveUnitFromSelection(this);
        
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
