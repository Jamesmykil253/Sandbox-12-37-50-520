# Unity Multiplayer Platformer - Implementation Guide

## Getting Started

### Prerequisites
- Unity 6000.1.4f1 or later
- Basic understanding of Unity development
- Familiarity with C# programming
- Photon PUN 2 account (for multiplayer testing)

### Project Setup
1. **Clone/Open Project:** Load the Unity project
2. **Import Dependencies:** Ensure all packages are imported
3. **Scene Setup:** Load the main game scene (`/Assets/_Project/Scenes/Prototype/Sandbox.unity`)
4. **Photon Setup:** Configure Photon settings if testing multiplayer

## Core System Implementation Details

### 1. Player Controller System

#### Overview
The PlayerController acts as the central hub for all player-related functionality, coordinating between movement, combat, scoring, and state management systems.

#### Implementation Steps:

**Step 1: Component Setup**
```csharp
// Required components are enforced through attributes
[RequireComponent(typeof(StateMachine))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerCombat))]
[RequireComponent(typeof(PlayerScoring))]
public class PlayerController : MonoBehaviour
```

**Step 2: Component References**
```csharp
private void Awake()
{
    StateMachine = GetComponent<StateMachine>();
    Movement = GetComponent<PlayerMovement>();
    Combat = GetComponent<PlayerCombat>();
    Scoring = GetComponent<PlayerScoring>();
    MyStats = GetComponent<CharacterStats>();
    PhotonView = GetComponent<PhotonView>();
}
```

**Step 3: State Machine Configuration**
```csharp
private void SetupStateMachine()
{
    var idleState = new PlayerIdleState(this, StateMachine);
    var groundedState = new PlayerGroundedState(this, StateMachine);
    var airborneState = new PlayerAirborneState(this, StateMachine);
    
    // Configure transitions between states
    StateMachine.AddAnyTransition(airborneState, 
        new FunkPredicate(() => !Movement.IsGrounded));
    StateMachine.AddTransition(airborneState, groundedState, 
        new FunkPredicate(() => Movement.IsGrounded && MoveInput != Vector2.zero));
}
```

### 2. State Machine Implementation

#### Creating Custom States

**Step 1: Implement IState Interface**
```csharp
public class PlayerCustomState : State
{
    public PlayerCustomState(PlayerController player, StateMachine stateMachine) 
        : base(player, stateMachine) {}

    public override void OnEnter()
    {
        // Initialize state
    }

    public override void Update()
    {
        // Per-frame logic
        // Check transition conditions
    }

    public override void FixedUpdate()
    {
        // Physics-related updates
    }

    public override void OnExit()
    {
        // Cleanup
    }
}
```

**Step 2: Add State to State Machine**
```csharp
private void SetupStateMachine()
{
    var customState = new PlayerCustomState(this, StateMachine);
    
    // Add transitions
    StateMachine.AddTransition(fromState, customState, 
        new FunkPredicate(() => /* condition */));
}
```

### 3. Movement System Implementation

#### Physics-Based Movement
The movement system uses Unity's CharacterController for reliable physics-based movement with custom gravity and jump mechanics.

**Key Implementation Details:**

**Ground Detection:**
```csharp
private void CheckGroundedStatus()
{
    Vector3 pos = transform.position + Controller.center;
    pos.y -= (Controller.height / 2f) - Controller.radius + 0.01f;
    
    if (Physics.SphereCast(pos, Controller.radius, Vector3.down, 
        out RaycastHit hit, 0.1f, groundLayer, QueryTriggerInteraction.Ignore))
    {
        if (Vector3.Angle(Vector3.up, hit.normal) < Controller.slopeLimit)
        {
            IsGrounded = true;
            return;
        }
    }
    IsGrounded = false;
}
```

**Jump Mechanics:**
```csharp
// In PlayerIdleState or PlayerGroundedState Update()
if (player.ConsumeJumpBuffer())
{
    var v = player.Movement.Velocity;
    v.y = Mathf.Sqrt(player.Movement.initialJumpHeight * -2f * player.Movement.gravity);
    player.Movement.Velocity = v;
}
```

### 4. Combat System Implementation

#### Damage Calculation System
The combat system uses a Pokémon Unite-inspired damage formula for consistent and scalable damage calculations.

**Implementation:**
```csharp
public static int CalculateDamage(CharacterStats attacker, CharacterStats defender, bool isEmpowered)
{
    // Base damage calculation
    float rawDamage = (attacker.baseStats.Attack * 1.2f) + (attacker.level * 20);
    
    // Defense mitigation
    float defenseMitigation = 1 - (defender.baseStats.Defense / (defender.baseStats.Defense + 600f));
    float damageAfterDefense = rawDamage * defenseMitigation;
    
    // Apply multipliers
    if (isEmpowered) damageAfterDefense *= EMPOWERED_MULTIPLIER;
    if (Random.value < attacker.baseStats.CritRate) damageAfterDefense *= CRITICAL_MULTIPLIER;
    
    return Mathf.Max(1, Mathf.RoundToInt(damageAfterDefense * Random.Range(0.95f, 1.05f)));
}
```

#### Empowered Attack System
Every third attack becomes empowered with increased damage and special effects.

**Implementation:**
```csharp
public bool IsNextAttackEmpowered()
{
    return _basicAttackCounter >= ATTACKS_UNTIL_EMPOWERED - 1;
}

public void PerformBasicAttack()
{
    _basicAttackCounter++;
    if (_basicAttackCounter >= ATTACKS_UNTIL_EMPOWERED)
    {
        _basicAttackCounter = 0;
    }
}
```

### 5. Enemy AI Implementation

#### State-Based AI System
Enemy AI uses the same state machine framework as players but with different states tailored for AI behavior.

**AI State Implementation:**
```csharp
public class EnemyCombatState : EnemyBaseState
{
    public override void Update()
    {
        if (enemy.PlayerTarget != null && enemy.Agent.isOnNavMesh)
        {
            enemy.Agent.SetDestination(enemy.PlayerTarget.position);
        }

        _attackTimer -= Time.deltaTime;
        if (_attackTimer <= 0 && enemy.IsPlayerInRadius(enemy.attackRadius))
        {
            Attack();
            _attackTimer = _attackCooldown;
        }
    }
}
```

#### Aggro and Targeting System
```csharp
public void AggroOnDamage(Transform attacker)
{
    if (canFollowPlayer)
    {
        IsAggroed = true;
        PlayerTarget = attacker;
    }
}

private bool ShouldChasePlayer()
{
    if (PlayerTarget == null || !canFollowPlayer) return false;
    
    CharacterStats playerStats = PlayerTarget.GetComponent<CharacterStats>();
    
    // Stealth mechanics
    if (playerStats.isInGrass && !playerStats.isRevealed)
    {
        if (this.MyStats.team != Team.Neutral)
        {
            if (IsAggroed) LoseAggro();
            return false;
        }
    }
    
    return IsAggroed && IsPlayerWithinLeash();
}
```

### 6. Networking Implementation

#### RPC-Based Damage System
All damage dealing uses RPCs to ensure synchronization across all clients.

**Implementation:**
```csharp
[PunRPC]
public void Rpc_TakeDamage(int damage, int attackerViewID)
{
    if (_isDead) return;
    
    currentHealth -= damage;
    currentHealth = Mathf.Clamp(currentHealth, 0, baseStats.HP);
    
    PhotonView attackerView = PhotonView.Find(attackerViewID);
    if (attackerView != null)
    {
        CharacterStats damageDealer = attackerView.GetComponent<CharacterStats>();
        if (currentHealth <= 0)
        {
            damageDealer.AddXp(100);
            Die();
        }
    }
}
```

#### Master Client Authority
Critical game logic runs only on the Master Client to prevent conflicts.

**Implementation:**
```csharp
private void Update() 
{ 
    // Only the master client should control the AI's logic
    if(!PhotonNetwork.IsMasterClient && PhotonNetwork.InRoom) return;
    
    if (StateMachine.enabled) StateMachine.Tick(); 
}
```

### 7. Input System Integration

#### ScriptableObject-Based Input Reader
The input system uses Unity's new Input System with a ScriptableObject wrapper for clean event handling.

**Setup:**
```csharp
[CreateAssetMenu(fileName = "InputReader", menuName = "Platformer/Input Reader")]
public class InputReader : ScriptableObject, InputSystem_Actions.IPlayerActions
{
    private InputSystem_Actions _inputActions;
    
    public event Action<Vector2> MoveEvent;
    public event Action JumpEvent;
    public event Action AttackEvent;
    
    private void OnEnable()
    {
        if (_inputActions == null)
        {
            _inputActions = new InputSystem_Actions();
            _inputActions.Player.SetCallbacks(this);
        }
        _inputActions.Player.Enable();
    }
}
```

**Component Integration:**
```csharp
private void OnEnable()
{
    if (inputReader == null) return;
    inputReader.MoveEvent += OnMove;
    inputReader.JumpEvent += OnJump;
    inputReader.AttackEvent += OnAttack;
}

private void OnMove(Vector2 move) => MoveInput = move;
```

### 8. Scoring and Objectives System

#### Coin Collection
The scoring system handles collectible coins and banking mechanics.

**Collection Logic:**
```csharp
public void CollectCoin(Coin coin)
{
    CoinCount += coin.coinValue;
    Debug.Log($"Coin collected! New total: {CoinCount}");
    coin.Collect();
}
```

#### Goal Zone Interaction
Goal zones provide team-based objectives and healing functionality.

**Goal Zone Implementation:**
```csharp
private void OnTriggerEnter(Collider other)
{
    if (other.TryGetComponent<PlayerController>(out var player))
    {
        if (player.Scoring != null)
        {
            player.Scoring.OnEnterGoalZone(this);
        }
    }
}

public int ScorePoints(int pointsToScore)
{
    int pointsActuallyScored = Mathf.Min(pointsToScore, scoreCapacity);
    scoreCapacity -= pointsActuallyScored;
    if (scoreCapacity <= 0) BreakGoal();
    return pointsActuallyScored;
}
```

### 9. Camera System Implementation

#### Multi-Position Camera Controller
The camera system provides multiple viewing angles that players can cycle through.

**Implementation:**
```csharp
private void LateUpdate()
{
    if (target == null || offsets.Count == 0) return;

    Vector3 desiredPosition = target.position + offsets[_currentOffsetIndex];
    transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, 
        ref _currentPositionVelocity, positionSmoothTime);
    transform.LookAt(target.position + Vector3.up * 1.5f);
}

private void HandleCycleCamera()
{
    _currentOffsetIndex++;
    if (_currentOffsetIndex >= offsets.Count)
    {
        _currentOffsetIndex = 0;
    }
}
```

## Best Practices & Guidelines

### Code Organization
1. **Namespace Usage:** All game code in `Platformer` namespace
2. **File Structure:** Organized by system/functionality
3. **Component Separation:** Each component has single responsibility
4. **Interface Usage:** IState, IPredicate for extensibility

### Performance Optimization
1. **Component Caching:** Cache frequently used components
2. **Object Pooling:** Ready for high-frequency objects
3. **Update Optimization:** Use appropriate Update vs FixedUpdate
4. **Network Efficiency:** Minimize RPC calls and data transfer

### Error Handling
1. **Null Checking:** Comprehensive null validation
2. **Safe Degradation:** System continues if components missing
3. **Debug Logging:** Extensive logging for debugging
4. **Exception Handling:** Graceful error recovery

### Extensibility
1. **State Machine:** Easy to add new player/enemy states
2. **Component System:** Modular and extensible
3. **Event System:** Loose coupling between systems
4. **Configuration:** ScriptableObjects for designer control

## Testing and Debugging

### Debug Features
- Visual state indicators (material color changes)
- Debug logging throughout systems
- Inspector debugging with runtime state display
- Network status information

### Testing Approaches
1. **Single Player:** Test core mechanics offline
2. **Local Multiplayer:** Multiple clients on same machine
3. **Network Testing:** Real network conditions
4. **Performance Testing:** Profile with Unity Profiler

---

*This implementation guide provides the foundation for understanding and extending the Unity Multiplayer Platformer project.*