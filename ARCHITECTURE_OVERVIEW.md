# Unity Multiplayer Platformer - Technical Architecture Overview

## System Architecture

### High-Level Architecture Diagram
```
┌─────────────────────────────────────────────────────────────┐
│                    Unity Game Engine                        │
├─────────────────────────────────────────────────────────────┤
│                  Photon PUN 2 Layer                        │
├─────────────────────────────────────────────────────────────┤
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────┐│
│  │   Game Systems  │  │   UI Systems    │  │ Audio System ││
│  │                 │  │                 │  │              ││
│  │ • GameManager   │  │ • UIManager     │  │ • SFX        ││
│  │ • StateMachine  │  │ • HUD           │  │ • Music      ││
│  │ • InputReader   │  │ • Menus         │  │ • VO         ││
│  └─────────────────┘  └─────────────────┘  └──────────────┘│
├─────────────────────────────────────────────────────────────┤
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────┐│
│  │ Character Layer │  │ Environment     │  │ Effects      ││
│  │                 │  │                 │  │              ││
│  │ • PlayerSystem  │  │ • Goals         │  │ • Particles  ││
│  │ • EnemyAI       │  │ • Collectibles  │  │ • Lighting   ││
│  │ • Combat        │  │ • Terrain       │  │ • Camera     ││
│  └─────────────────┘  └─────────────────┘  └──────────────┘│
├─────────────────────────────────────────────────────────────┤
│                    Unity Core Systems                       │
│  • Physics • Input • Rendering • Audio • Networking        │
└─────────────────────────────────────────────────────────────┘
```

## Component Architecture

### Player System Architecture
The player system follows a component-based design with clear separation of responsibilities:

```
PlayerController (Main Orchestrator)
├── PlayerMovement (Physics & Movement)
├── PlayerCombat (Attack & Targeting)
├── PlayerScoring (Objectives & Collection)
├── CharacterStats (Health, XP, Stats)
├── StateMachine (Behavior States)
└── PhotonView (Networking)
```

#### Core Components:

**1. PlayerController** (`/Assets/_Project/Scripts/Characters/PlayerController.cs`)
- **Purpose:** Central coordinator for all player functionality
- **Dependencies:** StateMachine, PlayerMovement, PlayerCombat, PlayerScoring
- **Key Features:**
  - Input event handling and distribution
  - State machine orchestration
  - Network synchronization
  - Visual feedback coordination

**2. PlayerMovement** (`/Assets/_Project/Scripts/Characters/PlayerMovement.cs`)
- **Purpose:** Handles all movement mechanics
- **Dependencies:** CharacterController, CharacterStats
- **Key Features:**
  - Physics-based movement with gravity
  - Jump mechanics (single and double jump)
  - Ground detection and slope handling
  - Rotation and directional facing

**3. PlayerCombat** (`/Assets/_Project/Scripts/Characters/PlayerCombat.cs`)
- **Purpose:** Manages attack systems and combat interactions
- **Dependencies:** CharacterStats, TargetingSystem, PhotonView
- **Key Features:**
  - Melee and ranged attack execution
  - Empowered attack system
  - Target acquisition and projectile spawning
  - Attack cooldown management

**4. PlayerScoring** (`/Assets/_Project/Scripts/Characters/PlayerScoring.cs`)
- **Purpose:** Handles objective completion and scoring
- **Dependencies:** CharacterStats, PlayerController
- **Key Features:**
  - Coin collection and management
  - Goal zone interaction
  - Score calculation and XP rewards
  - Banking mechanics

### State Machine Framework

The state machine system provides flexible behavior management for both players and AI:

```
StateMachine (Core Engine)
├── IState (State Interface)
├── ITransition (Transition Interface)
├── IPredicate (Condition Interface)
└── StateNode (Internal Management)
```

#### Player State Hierarchy:
```
PlayerBaseState (Abstract Base)
├── PlayerIdleState
├── PlayerGroundedState
├── PlayerAirborneState
├── PlayerAttackState
└── PlayerScoringState
```

**State Transition Logic:**
- Priority-based transition evaluation
- "Any" state transitions for emergency conditions
- Frame-rate independent state updates
- Clear entry/exit lifecycle management

### Enemy AI Architecture

```
EnemyAIController (Main AI Brain)
├── CharacterStats (Health & Stats)
├── NavMeshAgent (Pathfinding)
├── StateMachine (Behavior States)
└── PhotonView (Networking)
```

#### AI State System:
```
EnemyBaseState (Abstract Base)
├── EnemyIdleState (Passive Behavior)
├── EnemyCombatState (Active Fighting)
└── EnemyReturnState (Returning to Spawn)
```

**AI Decision Making:**
- Aggro system with player detection
- Leash mechanics for bounded AI
- Master Client authority for deterministic behavior
- Stealth interaction and reveal mechanics

### Networking Architecture

#### Photon PUN 2 Integration:
```
ConnectionManager
├── Connection Handling
├── Room Management
├── Player Spawning
└── Offline Mode Support
```

**Network Authority Model:**
- **Master Client:** Controls AI, authoritative actions, object spawning
- **Individual Clients:** Control their own player, send input
- **RPC Communication:** Damage dealing, visual effects, state changes

**Key Network Components:**
1. **Damage System:** RPC-based with PhotonView ID tracking
2. **Object Spawning:** PhotonNetwork.Instantiate for shared objects
3. **State Synchronization:** Custom RPCs for critical state changes
4. **Hybrid Mode:** Seamless offline/online operation

### Data Management

#### Core Data Structures:
```
StatBlock (Character Stats)
├── HP, Attack, Defense
├── Special Attack/Defense
├── Speed, Crit Rate
└── Attack Speed
```

#### Team System:
```
enum Team {
    Home,    // Player team 1
    Away,    // Player team 2  
    Neutral  // Environmental/AI
}
```

### Input System Architecture

```
InputReader (ScriptableObject)
├── Unity Input System Integration
├── Event-Based Input Distribution
├── Multi-Device Support (Keyboard, Gamepad)
└── Action Mapping Configuration
```

**Input Flow:**
1. Unity Input System → InputReader
2. InputReader → Event Broadcasting
3. Components → Event Subscription
4. Components → Action Execution

## Performance & Optimization

### Memory Management:
- **Object Pooling:** Ready for projectiles and effects
- **Component Caching:** Cached references to prevent GetComponent calls
- **Event Cleanup:** Proper event subscription/unsubscription

### Network Optimization:
- **Master Client Authority:** Reduces network traffic
- **Selective Synchronization:** Only critical state changes sent
- **Bandwidth Efficient:** RPC parameters optimized for size

### Rendering Optimization:
- **URP Pipeline:** Universal Render Pipeline for mobile compatibility
- **Level-of-Detail:** Ready for implementation
- **Efficient Materials:** Shared materials for similar objects

## Extensibility & Modularity

### Adding New Features:
1. **New Player States:** Implement IState interface, add to state machine
2. **New Components:** Follow component pattern, integrate with PlayerController
3. **New Abilities:** Extend combat system, add to input mapping
4. **New AI Behaviors:** Create EnemyBaseState derivatives

### Configuration System:
- **ScriptableObjects:** Input configuration, settings management  
- **Inspector Serialization:** Easy designer tweaking
- **Runtime Modification:** Stats and behaviors can be modified at runtime

### Platform Compatibility:
- **Mobile Ready:** Touch input system prepared
- **Console Ready:** Gamepad support implemented
- **PC Optimized:** Keyboard/mouse primary target

## Quality Assurance

### Code Quality Measures:
- **Null Safety:** Comprehensive null checking throughout
- **Exception Handling:** Graceful degradation on errors
- **Debug Logging:** Extensive debug information available
- **Code Documentation:** XML documentation and inline comments

### Testing Considerations:
- **Unit Testing:** State machine logic testable
- **Integration Testing:** Component interaction validation
- **Network Testing:** Offline/online mode compatibility
- **Performance Testing:** Ready for profiling and optimization

### Error Recovery:
- **Network Disconnection:** Graceful offline mode fallback
- **Component Missing:** Safe degradation with warning logs
- **State Conflicts:** Priority-based resolution
- **Invalid Actions:** Validation and rejection with feedback

---

*This architecture supports a scalable, maintainable multiplayer platformer with clear separation of concerns and extensible design patterns.*