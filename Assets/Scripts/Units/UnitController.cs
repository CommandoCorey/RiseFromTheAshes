using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;
using UnityEngine.VFX;

public enum UnitState
{
    Idle,    
    Moving,
    Follow,
    Attack,
    Patrol
}

public class UnitController : MonoBehaviour
{
    [SerializeField] string unitName;
    [SerializeField] string description;

    [SerializeField] int steelCost = 10;
    [SerializeField] int timeToTrain = 1;
    [SerializeField] int spaceUsed = 1;
    [SerializeField] bool moveToRallyPoint = true;

    float healTimer = 0.0f;

    List<Material> materials;

    MeshRenderer[] childMeshRenderers;
    MeshRenderer myMeshRenderer;

    #region variable declartion
    [Header("Game Objects and Transforms")]    
    public Transform turret;
    public Transform firingPoint;
    public Transform body;
    public Sprite guiIcon;
    public Image unitIcon;
    public TextMeshProUGUI statusText;

    [Header("Highlights")]
    public GameObject selectionHighlight;
    SelectionSprites selection;

    [Header("Health Display")]
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

    [Header("Turret Rotation")]
    [SerializeField][Range(1, 100)]
    float turretRotationSpeed = 20.0f;
    [SerializeField][Range(0, 10)]
    float minAngleBeforeFiring = 1;

    [Header("Enemy Detection")]
    [Range(1, 100)]
    [SerializeField] float detectionRadius = 30.0f;
    [SerializeField] LayerMask enemyUnitLayer;
    [SerializeField] LayerMask enemyBuildingLayer;
    [SerializeField] LayerMask environmentLayer;

    [Header("Particle Systems")]
    public ParticleSystem[] fireEffects;
    public ParticleSystem[] hitEffects;
    public VisualEffect[] destroyEffects;

    [Header("Sound Effects")]
    public SoundEffect[] moveSounds;
    public SoundEffect[] turretSounds;
    public SoundEffect[] fireSounds;
    public SoundEffect[] hitSounds;
    public SoundEffect[] destroySounds;

    [Header("Configurations")]
    public bool autoAttack = true;
    public bool seeThroughWalls = false;

    [Header("Circle meshes")]
    public bool showRanges;
    public Transform detectionRangeMesh;
    public Transform attackRangeMesh;

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
    private Vector3 healthBarOffset;
    private new AudioSource audio;
    private GameManager gameManager;
    private int rallyId = 0;
    private Vector3 spawnPos;
    private NavMeshAgent agent;

    // added by George
    bool recentlyDamaged;
    float RDTimer;

    // state classes
    private IdleState idleState;
    //private SeekState moveState;
    private AgentMoveState agentMoveState;
    //private FlockState flockState;
    private FollowEnemyState followState;
    private AttackState agentAttackState;
    private FollowPathState patrolState;

    // circle material colours
    Material deteectionRangeMaterial;
    Material attackRangeMaterial;

    UnitManager um;

    #endregion

    #region properties
    public UnitState State { get; private set; }
    public Transform AttackTarget { get; set ; }
    public float DetectionRadius { get => detectionRadius; }
    public LayerMask EnemyUnitLayer { get => enemyUnitLayer; }
    public LayerMask EnemyBuildingLayer { get => enemyBuildingLayer; }
    public LayerMask EnvironmentLayer { get => environmentLayer; }
    public Sprite GuiIcon { get => guiIcon; }
    public bool UnitHalt { get; set; } = false;
    public bool IsBuilt { get; set; } = false;
    public bool MovingToBase { get; set; } = false;
    public bool AttackOrderGiven { get; set; } = false;

    // unit stats
    public string Name { get => unitName; }
    public string Description { get => description; }
    public int Cost { get => steelCost; }
    public int SpaceUsed { get => spaceUsed; }
    public float TimeToTrain { get => timeToTrain; }
    public float MaxHealth { get => maxHealth; }
    public float CurrentHealth {  get=> health; }
    //public float Heal { set=> health = Mathf.Clamp(health + value, 0.0f, maxHealth); }
    public float Speed { get => movementSpeed; }  
    public float TurretRotationSpeed { get => turretRotationSpeed; }
    public float DamagePerHit { get => damagePerHit; }
    public float AttackRate { get => attackRate; }
    public float AttackRange {  get => attackRange; }
    public float MinAngle { get => minAngleBeforeFiring; }
    public float DPS { get => damagePerHit / attackRate; }

    public bool SingleSelected { get; set; } = false;
    public bool RotatingHighlight { get; set; } = false;
   
    public bool IsInCombat { get {
            return State == UnitState.Attack || recentlyDamaged;
        }
    }

    public bool ReachedRallyPoint { get; internal set; } = false;
    public int RallyId { get => rallyId; }
    #endregion

    /*
    public void SetPath(Vector3[] waypoints)
    {
        this.waypoints = waypoints;
    }*/

    private void Awake()
    {
        spawnPos = transform.position;        
    }

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameManager.Instance;
        um = UnitManager.Instance;

        health = maxHealth;        

        if (healthBar)
            healthBarOffset = healthBar.transform.parent.localPosition;

        audio = body.GetComponent<AudioSource>();

        bool isAi = body.gameObject.layer == 7;

        gameManager.IncreaseUnitCount(spaceUsed, isAi);
        
        idleState = GetComponent<IdleState>();

        if(idleState != null)
            State = UnitState.Idle;

        if (ReachedRallyPoint || !moveToRallyPoint)
            ChangeState(UnitState.Idle);

        if (um)
        {
            um.UCRefs.AddLast(this);
        }

        childMeshRenderers = GetComponentsInChildren<MeshRenderer>();
        myMeshRenderer = GetComponent<MeshRenderer>();

        PopulateMaterialRefs();

        agent = body.GetComponent<NavMeshAgent>();
        agent.speed = movementSpeed;
        
        if (detectionRangeMesh)
        {            
            detectionRangeMesh.gameObject.SetActive(false);
        }

        if (attackRangeMesh)
        {            
            attackRangeMesh.gameObject.SetActive(false);
        }

    }

	// Update is called once per frame
	void Update()
    {
        // show/hide gui above unit
        if(unitIcon)
            unitIcon.gameObject.SetActive(gameManager.ShowIcons);
        if(statusText)
            statusText.gameObject.SetActive(gameManager.ShowStatusText);

        if (healthBar)
        {
            healthBar.gameObject.SetActive(gameManager.ShowHealthbars);
            healthBar.progress = health / maxHealth;
            healthBar.SetProgress(health / maxHealth, maxHealth);
        }

        if (health <= 0)
        {
            GameObject.Destroy(this.gameObject);

            // play destruction sound
            if (destroySounds.Length > 0)
            {
                int randomPick = Random.Range(0, destroySounds.Length-1);
                gameManager.PlaySound(destroySounds[randomPick].clip,
                    destroySounds[randomPick].volumeScale);
            }

            if(destroyEffects != null)
                gameManager.InstantiateParticles(destroyEffects[RandomPick(destroyEffects)], body.position);
        }

        //healthBar.transform.position = body.position + healthBarOffset;

        if (UnitManager.Instance)
        {
            RDTimer += Time.deltaTime;

            if (RDTimer >= UnitManager.Instance.unitInCombatTimeout)
            {
                recentlyDamaged = false;
                RDTimer = 0.0f;
            }
        }

        healTimer -= Time.deltaTime;
        
        foreach (var mat in materials)
		{
            mat.SetColor("HealEffectColor", Color.blue);
            mat.SetFloat("HealEffectIntensity", Mathf.Clamp(healTimer, 0.0f, 1.0f));
		}

        // set scale of circle meshes
        if (detectionRangeMesh)
        {            
            detectionRangeMesh.localScale = new Vector3(2*detectionRadius, detectionRangeMesh.localScale.y,
                2*detectionRadius);

            detectionRangeMesh.GetComponent<Renderer>().material.color = um.DetectionRangeColor;
        }


        if (attackRangeMesh)
        {            
            attackRangeMesh.localScale = new Vector3(2*attackRange, attackRangeMesh.localScale.y, 2*attackRange);
            attackRangeMesh.GetComponent<Renderer>().material.color = um.AttackRangeColor;
        }

    }

    private void LateUpdate()
    {
        transform.position = body.position;
        body.localPosition = Vector3.zero;
    }

    /// <summary>
    /// Toggles the visibility of the unit selection highlight and health bar
    /// </summary>
    /// <param name="selected">true or false value</param>
    public void SetSelected(bool selected)
    {
        selectionHighlight.SetActive(selected);
        healthBar.gameObject.SetActive(selected);

        GetComponent<SelectionSprites>().SetSelectedSprite(selected);

        if(selected && SingleSelected && showRanges)
        {
            if(detectionRangeMesh != null)
                detectionRangeMesh.gameObject.SetActive(true);

            if(attackRangeMesh != null)
                attackRangeMesh.gameObject.SetActive(true);
        }
        else if(!selected)
        {
            if (detectionRangeMesh != null)
                detectionRangeMesh.gameObject.SetActive(false);

            if (attackRangeMesh != null)
                attackRangeMesh.gameObject.SetActive(false);

            //SingleSelected = false;
        }

        if(AttackTarget != null)
        {
            var targetSprites = AttackTarget.GetComponent<SelectionSprites>();
            targetSprites.ShowTargetedSprite = selected;
        }
    }

    /// <summary>
    /// Removes health from the unit by a specified amoount
    /// </summary>
    /// <param name="amount">the amount of HP to remove</param>
    public void TakeDamage(float amount, Vector3 hitPosition = new Vector3())
    {
        health -= amount;

        ParticleSystem hitParticles = hitEffects[RandomPick(hitEffects)];

        InstantiateParticles(hitParticles, hitPosition);

        if (hitSounds.Length > 0)
        {
            int randomPick = Random.Range(0, hitSounds.Length-1);
            audio.PlayOneShot(hitSounds[randomPick].clip, hitSounds[randomPick].volumeScale);
        }

        recentlyDamaged = true;
        RDTimer = 0.0f;
    }

    /// <summary>
    /// Increease the units health by a specified amount
    /// </summary>
    /// <param name="amount">the amount of HP to increase the health by</param>
    public void Heal(float amount)
    {
        health += amount;

        healTimer = 1.0f;

        // prevent health from going above maximum
        health = Mathf.Clamp(health, 0.0f, maxHealth);
    }

    /// <summary>
    /// Plays a particle system attached to the UnitController script
    /// </summary>
    /// <param name="particles">The particle system to start playing</param>
    public void PlayParticles(ParticleSystem particles)
    {
        if (particles == null)
            return;

        particles.gameObject.SetActive(true);

        var childParticles = particles.gameObject.GetComponentsInChildren<ParticleSystem>();

        particles.Play();

        foreach (ParticleSystem child in childParticles)
            child.Play();
    }

    public void InstantiateParticles(ParticleSystem particles, Vector3 position)
    {
        if (particles == null)
            return;

        Instantiate(particles, position, Quaternion.identity, transform);

        var childParticles = particles.gameObject.GetComponentsInChildren<ParticleSystem>();

        particles.Play();
        foreach (ParticleSystem child in childParticles)
            child.Play();
    }

    public void InstantiateParticles(VisualEffect particles, Vector3 position)
    {
        if (particles == null)
            return;

        var obj = Instantiate(particles, position, Quaternion.identity, transform);

        var childParticles = particles.gameObject.GetComponentsInChildren<VisualEffect>();

        particles.Play();
        foreach (VisualEffect child in childParticles)
            child.Play();

        Destroy(obj, 3.0f);
    }

    /// <summary>
    /// Playss a random movement sound
    /// </summary>
    public void PlayMoveSound()
    {
        if (audio && moveSounds.Length > 0)
        {
            int randomPick = Random.Range(0, moveSounds.Length - 1);
            audio.PlayOneShot(moveSounds[randomPick].clip, moveSounds[randomPick].volumeScale);
        }
    }

    /// <summary>
    /// Plays a random sound from the fireSounds list
    /// </summary>
    public void PlayFireSound()
    {
        if (fireSounds.Length > 0)
        {
            int randomPick = Random.Range(0, hitSounds.Length - 1);
            audio.PlayOneShot(fireSounds[randomPick].clip, destroySounds[randomPick].volumeScale);
        }
    }

    /// <summary>
    /// Plays a random sound from the turretSounds
    /// </summary>
    public void PlayAimSound()
    {
        if(turretSounds.Length > 0)
        {
            var randomPick = Random.Range(0, turretSounds.Length - 1);
            audio.PlayOneShot(turretSounds[randomPick].clip);
        }
    }

    /// <summary>
    /// Creates a formation position around a rally point and moves the unit there
    /// </summary>
    /// <param name="point">the location of the rally point</param>
    public void MoveToRallyPoint(Vector3 point)
    {
        gameManager = GameManager.Instance;

        FormationManager formations = FormationManager.Instance;
        agent = body.GetComponent<NavMeshAgent>();

        agent.avoidancePriority -= formations.GetCurrentRallySize(1);

        bool isAi = gameObject.layer == 7;

        //Debug.DrawLine(spawnPos, point, Color.yellow, 3.0f);

        Vector3 origin;
        if(isAi)
            origin = gameManager.enemyHQ.transform.position;  
        else
            origin = gameManager.playerHQ.transform.position;

        Vector3 formationPos = formations.GetRallyPosition(point, origin, isAi, ref rallyId);

        ChangeState(UnitState.Moving, formationPos);
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
                    Destroy(agentMoveState);
                }
            break;

            case UnitState.Follow:
                    Destroy(followState);

            break;

            case UnitState.Attack:                
                    Destroy(agentAttackState);                
            break;

            case UnitState.Patrol:
                Destroy(patrolState);
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

                if(agentMoveState == null)
                    agentMoveState = gameObject.AddComponent<AgentMoveState>();
                else                      
                    body.GetComponent<NavMeshAgent>().isStopped = true;

                agentMoveState.MoveTo(target);
                
           break;

           case UnitState.Follow:
                followState = gameObject.AddComponent<FollowEnemyState>();

           break;

           case UnitState.Attack:
               agentAttackState = gameObject.AddComponent<AttackState>();                    
           break;

           case UnitState.Patrol:
                patrolState = gameObject.AddComponent<FollowPathState>();
           break;

        }
    }

    #region private functions
    private int RandomPick(Object[] array)
    {
        return Random.Range(0, array.Length - 1);
    }    

    // Removes unit from lists in unit manager and GUI once it is destroyed
    private void OnDestroy()
    {
        var unitGui = GameObject.FindObjectOfType<UnitGui>();

        if (UnitManager.Instance != null) {
            UnitManager.Instance.RemoveFromSelection(this.gameObject);
            UnitManager.Instance.UCRefs.Remove(this);
        }

        if (unitGui != null)
            unitGui.RemoveUnitFromSelection(this);

        bool aiPlayer = body.gameObject.layer == 7;

        gameManager.DecreaseUnitCount(aiPlayer);
    }

    // Added by George
    void PopulateMaterialRefs()
    {
        materials = new List<Material>();

        if (myMeshRenderer != null)
        {
            foreach (Material material in myMeshRenderer.materials)
            {
                materials.Add(material);
            }
        }

        if (childMeshRenderers != null)
        {
            foreach (MeshRenderer childRenderer in childMeshRenderers)
            {
                foreach (Material childMaterial in childRenderer.materials)
                {
                    materials.Add(childMaterial);
                }
            }
        }
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

    #endregion

}
