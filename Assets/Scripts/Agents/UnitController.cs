using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public enum UnitState
{
    Idle,    
    Moving,
    Flock,
    Follow,
    Attack
}

public class UnitController : MonoBehaviour
{
    [SerializeField] string unitName;

    [SerializeField] int steelCost = 10;
    [SerializeField] int timeToTrain = 1;

    #region variable declartion
    [Header("Game Objects and transforms")]
    public GameObject selectionHighlight;
    public Transform turret;
    public Transform firingPoint;
    public Transform body;
    public Sprite guiIcon;

    [Header("Particle System")]
    public ParticleSystem fireEffect;
    public ParticleSystem hitEffect;
    public ParticleSystem destroyEffect;

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
    [SerializeField][Range(1, 100)]
    float attackRange = 20.0f;
    [SerializeField][Range(1, 100)]
    float turretRotationSpeed = 1.0f;

    [Header("Enemy Detection")]
    [Range(1, 100)]
    [SerializeField] float detectionRadius = 30.0f;
    //[SerializeField] LayerMask detectionLayer;
    [SerializeField] LayerMask enemyUnitLayer;
    [SerializeField] LayerMask enemyBuildingLayer;
    [SerializeField] LayerMask environmentLayer;

    [Header("Sound Effects")]
    public AudioClip unitSelectSound;
    public AudioClip[] moveSounds;
    public AudioClip[] turretSounds;
    public AudioClip[] fireSounds;
    public AudioClip[] hitSounds;
    public AudioClip[] destroySounds;

    [Header("Configurations")]
    public bool autoAttack = true;
    public bool seeThroughWalls = false;

    [Header("Gizmos")]
    [SerializeField] bool showSolidDisks = true;
    [SerializeField] bool showDetectionRadius = true;
    [SerializeField] [Range(0, 0.5f)]
    float detectionOpacity = 0.02f;
    [SerializeField] bool showAttackRange = true;
    [SerializeField] [Range(0, 0.5f)]
    float attackRangeOpacity = 0.04f;

    // private variables
    private float health;
    //private Vector3[] waypoints;

    // state classes
    private IdleState idleState;
    private SeekState moveState;
    private AgentMoveState agentMoveState;
    private FlockState flockState;
    private CombatState attackState;
    private FollowEnemyState followState;
    private AttackState agentAttackState;

    // other variables
    private Vector3 healthBarOffset;
    private AudioSource audio;
    private GameManager gameManager;
    #endregion

    # region properties
    public UnitState State { get; private set; }
    public Transform AttackTarget { get; set; }
    public float DetectionRadius { get => detectionRadius; }
    //public LayerMask DetectionLayer { get => detectionLayer; }
    public LayerMask EnemyUnitLayer { get => enemyUnitLayer; }
    public LayerMask EnemyBuildingLayer { get => enemyBuildingLayer; }

    public LayerMask EnvironmentLayer { get => environmentLayer; }
    public Sprite GuiIcon { get => guiIcon; }
    //public float HaltTime { get => haltTime; }
    public bool UnitHalt { get; set; } = false;

    // unit stats
    public string Name { get => unitName; }
    public int Cost { get => steelCost; }
    public float TimeToTrain { get => timeToTrain; }
    public float MaxHealth { get => maxHealth; }
    public float CurrentHealth {  get=> health; }
    //public float Heal { set=> health = Mathf.Clamp(health + value, 0.0f, maxHealth); }
    public float Speed { get => movementSpeed; }  
    public float TurretRotationSpeed { get => turretRotationSpeed; }
    public float DamagePerHit { get => damagePerHit; }
    public float AttackRate { get => attackRate; }
    public float AttackRange {  get => attackRange; }
    #endregion

    /*
    public void SetPath(Vector3[] waypoints)
    {
        this.waypoints = waypoints;
    }*/

    // Start is called before the first frame update
    void Start()
    {
    	health = maxHealth;

        if(healthBar)
            healthBarOffset = healthBar.transform.parent.localPosition;

        audio = GetComponent<AudioSource>();
        gameManager = GameObject.FindObjectOfType<GameManager>();

        ChangeState(UnitState.Idle);
    }

    // Update is called once per frame
    void Update()
    {
        if (!healthBar)
            return;

        healthBar.progress = health / maxHealth;

        if (health <= 0)
        {
            GameObject.Destroy(this.gameObject);
            //GameObject.Destroy(this.gameObject.transform.parent.gameObject);

            // play destruction sound
            if (destroySounds.Length > 0)            
                gameManager.PlaySound(destroySounds[0], 1);            

            if(destroyEffect != null)
                gameManager.InstantiateParticles(destroyEffect, body.position);
        }

        healthBar.transform.position = body.position + healthBarOffset;
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

        PlayParticles(hitEffect);

        if (hitSounds.Length > 0)
        {
            audio.PlayOneShot(hitSounds[0], 0.25f);
        }
    }

    /// <summary>
    /// Increease the units health by a specified amount
    /// </summary>
    /// <param name="amount">the amount of HP to increase the health by</param>
    public void Heal(float amount)
    {
        health += amount;

        // prevent health from going above maximum
        Mathf.Clamp(health, 0.0f, maxHealth);
    }

    /// <summary>
    /// Plays a particle system attached to the UnitControlelr script
    /// </summary>
    /// <param name="particles">The particle system to start playing</param>
    public void PlayParticles(ParticleSystem particles)
    {
        if (particles == null)
            return;

        var childParticles = particles.gameObject.GetComponentsInChildren<ParticleSystem>();

        particles.Play();

        foreach (ParticleSystem child in childParticles)
            child.Play();
    }

    /// <summary>
    /// 
    /// </summary>
    public void PlayFireSound()
    {
        audio.PlayOneShot(fireSounds[0], 0.1f);
    }

    /// <summary>
    /// Changes the Unit's state in the finite state machine to another one
    /// </summary>
    /// <param name="newState">The state being switched to</param>
    /// <param name="target">optional position parameter</param>
    public void ChangeState(UnitState newState, Vector3 target = new Vector3())
    {
        // Destroy script corrensponding to current state
        switch (State)
        {
            case UnitState.Idle: Destroy(idleState); break;
            case UnitState.Moving:
                // prevents destruction during redirect
                if (newState != UnitState.Moving)
                {
                    if (tag == "PlayerUnit")
                        Destroy(moveState);
                    else if (tag == "NavMesh Agent")
                        Destroy(agentMoveState);
                }
            break;

            case UnitState.Follow:

                if (tag == "NavMesh Agent")
                    Destroy(followState);
                else if (tag == "PlayerUnit")
                    Destroy(attackState);

            break;

            case UnitState.Flock: Destroy(flockState); break;
            case UnitState.Attack:                
                if(tag == "PlayerUnit")
                    Destroy(attackState);
                else if (tag == "NavMesh Agent")
                    Destroy(agentAttackState);                
            break;            
        }

        State = newState;

        switch (newState)
        {
            case UnitState.Idle:
                if (GetComponent<IdleState>() == null)
                {
                    idleState = gameObject.AddComponent<IdleState>();
                }

                break;

            case UnitState.Moving:

                if (this.tag == "PlayerUnit")
                {
                    if (GetComponent<SeekState>() == null)
                    {                       
                        moveState = gameObject.AddComponent<SeekState>();
                    }               

                    //moveState.Target = target;
                    moveState.MoveTo(target);
                }
                else if (this.tag == "NavMesh Agent")
                {
                    if(agentMoveState == null)
                        agentMoveState = gameObject.AddComponent<AgentMoveState>();
                    else                      
                        body.GetComponent<NavMeshAgent>().isStopped = true;

                    agentMoveState.MoveTo(target);
                }
            break;

            case UnitState.Flock:
                if (GetComponent<FlockState>() == null)
                {
                    flockState = gameObject.AddComponent<FlockState>();
                }

                flockState.Target = target;
                //flockState.FormationTarget = formationTarget;
            break;

            case UnitState.Follow:

                if (this.tag == "NavMesh Agent")
                {
                    followState = gameObject.AddComponent<FollowEnemyState>();
                }
                else if (this.tag == "PlayerUnit")
                {
                    if (GetComponent<CombatState>() == null)
                    {
                        attackState = gameObject.AddComponent<CombatState>();
                    }

                }
            break;

            case UnitState.Attack:

                if (this.tag == "PlayerUnit")
                {
                    if (GetComponent<CombatState>() == null)
                    {
                        attackState = gameObject.AddComponent<CombatState>();
                    }

                }
                else if (this.tag == "NavMesh Agent")
                {
                    agentAttackState = gameObject.AddComponent<AttackState>();                    
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
#if UNITY_EDITOR
        if (showDetectionRadius)
        {
            if (showSolidDisks)
            {
                UnityEditor.Handles.color = new Color(Color.yellow.r, Color.yellow.g, Color.yellow.b, detectionOpacity);
                UnityEditor.Handles.DrawSolidDisc(body.position, Vector3.up, detectionRadius);
            }
            else
            {
                UnityEditor.Handles.color = Color.yellow;
                UnityEditor.Handles.DrawWireDisc(body.position, Vector3.up, detectionRadius);
            }
        }

        if (showAttackRange)
        {
            if (showSolidDisks)
            {
                UnityEditor.Handles.color = new Color(Color.red.r, Color.red.g, Color.red.b, attackRangeOpacity);
                UnityEditor.Handles.DrawSolidDisc(body.position + Vector3.up, Vector3.up, attackRange);
            }
            else
            {
                UnityEditor.Handles.color = Color.red;
                UnityEditor.Handles.DrawWireDisc(body.position, Vector3.up, attackRange);
            }

        }

        /*
        var stateString = "";

        switch(State)
        {
            case UnitState.Idle: stateString = "Idle";
            case State.Attack: stateString = "Combat"; break;

        }

        UnityEditor.Handles.Label(body.position + Vector3.up * 5, stateString);*/
#endif

    }



}
