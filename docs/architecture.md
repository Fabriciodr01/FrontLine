# Architecture.md

Current implementation state of FrontLine. This document focuses on **what isn't obvious from reading the code** — the why behind decisions, constraints that shaped the design, and gotchas a newcomer would miss.

## Layering

| Layer | Location | Rule |
|---|---|---|
| Models | `Assets/_Game/Models/` | Pure C#, zero UnityEngine imports |
| Controllers/Services | `Assets/_Game/Controllers/`, `Assets/_Game/Services/` | Rule logic and orchestration |
| Commands | `Assets/_Game/Commands/` | Self-validating `ICommand` objects |
| Views | `Assets/_Game/Views/`, `Assets/_Game/UI/` | MonoBehaviours that only react to events — no gameplay logic |
| Input | `Assets/_Game/Input/` | Raw input → intents → SelectionManager |

**Non-obvious:**
1. Views never own or mutate game state. This is strict — even "harmless" convenience writes from Views break the command audit trail and make the thesis analysis unreliable.

2. "TODO-POST-ALPHA" is the comment left in the code to point out what will remain bad/untouched until we have more time to build a better system it falls into the
'good enough' category for a alpha.

## Service Locator Bootstrap

`GameBootstrapper.Awake()` registers services in explicit order. **Order matters** — services resolved during registration of later services must already be registered. If you add a new service, think about where it falls in the dependency chain.

## Command Execution Flow

```
InputManager (fires intent)
  → SelectionManager (interprets based on current selection state)
    → Command constructed
      → CommandProcessor.Process(command)
        → command.Execute() validates against GameState
          → if valid: mutates UnitData/TileData, returns CommandResult.Ok()
            → CommandProcessor fires OnCommandExecuted
              → Views/HUD react via event subscriptions
```

**Non-obvious:** Validation lives inside `command.Execute()`, not in SelectionManager or Views. SelectionManager only decides *which* command to build — it doesn't gatekeep whether the action is legal. This means a command can be constructed and immediately fail validation, which is intentional.

## Turn & Phase Flow

```
GamePhase: WaitingForPlayers → Setup → Player1Turn ↔ Player2Turn → GameOver
```

- `TurnController.StartGame()` transitions Setup → Player1Turn and resets AP.
- Each unit: 2 AP/turn. Move and Shoot each cost 1 AP.
- `TurnController.EndTurn()` checks win condition (all opposing units dead?), then switches phase.

**Non-obvious:** Win condition is checked **only at end-of-turn**, not mid-action. A unit dying mid-turn doesn't immediately end the game.

## Grid & Combat

- 16×14 grid. Distance uses **Chebyshev** (diagonal = 1), computed in `GridMath`.
- Unit defaults: 3 HP, 2 AP/turn, move range 3, attack range 2, damage 1.
- Hit chance: base 75%, −10% per tile beyond range 1, +5% per elevation advantage, floor 15%. Logic in `CombatResolver`.

**Non-obvious:** Chebyshev was chosen over Manhattan because diagonal movement feeling "free" matches the tactical mobility expected in XCOM-likes. This is a deliberate design + thesis decision, not a default.

## Pathfinding

Two algorithms in `GridMath`, each for a different query type:

- **`GetReachableTiles(GameState, fromX, fromY, range)`** — BFS flood-fill. Returns `HashSet<(int,int)>` of all walkable tiles reachable within `range` steps. Used by: `MoveCommand` (validation), `SelectionManager` (highlight), `MoveTowardNearestEnemyNode` (AI candidate selection).
- **`GetPath(GameState, fromX, fromY, toX, toY)`** — A* with Chebyshev heuristic. Returns the tile-by-tile path. Used by: `MoveCommand.Execute()` to populate `MoveCommand.Path`, which `UnitSpawner` passes to `UnitView` for waypoint animation.

**Non-obvious:**
- The path is computed inside `MoveCommand.Execute()` **before** the unit position is mutated. After mutation the old position is gone — any refactor must preserve this order.
- A* uses a cross-product tie-breaker: when f-scores are equal (common with Chebyshev), the node least deviated from the straight start→goal line wins. Without this, paths zigzag visually among equally-cheap routes.
- The open list is a sorted `List<T>`, not a heap. Acceptable for a 224-tile grid; a binary heap would be needed at larger scale.

## State Machines

- `GameStateMachine` — game phase transitions (see above).
- `SelectionManager` — player input states: `Idle → UnitSelected → MovePending / ShootPending`.

**Non-obvious:** These are two separate state machines with different responsibilities. `GameStateMachine` governs whose turn it is; `SelectionManager` governs what the current player's tap means. They communicate through events, not direct references.

## Key Entry Points

| Class | Role |
|---|---|
| `GameBootstrapper` | `Awake()` — initializes and wires all services |
| `GameState` | Central data container (grid, units dict, phase) |
| `CommandProcessor` | `Process(ICommand)` — authoritative path for all state changes |
| `SelectionManager` | `HandleTap()` — player input state machine |
| `TurnController` | `StartGame()`, `EndTurn()` — turn order and win conditions |

## Design Patterns In Use

Documented here only when their usage has a **non-obvious constraint or implication**:

- **Command** — Every gameplay action is an `ICommand`. New actions must follow this. Commands self-validate. This is thesis-critical: the command log is a primary data source for pattern analysis.
- **Service Locator** — `ServiceLocator.Instance.Get<T>()`. Chosen over DI containers because Unity's MonoBehaviour lifecycle makes constructor injection impractical. Trade-off: implicit dependencies, but acceptable for a solo-dev project with explicit boot order.
- **Observer** — C# events (`OnPhaseChanged`, `OnCommandExecuted`, `OnTurnStarted`, etc.) for all cross-system communication. No Unity Events — pure C# events for testability and to keep Models layer Unity-free.
- **Strategy** — `IInputHandler` abstraction so local / AI / network input can be swapped. Currently only local input is implemented.
- **State Machine** — Two separate machines (see State Machines section above).

## Bug Tracking Convention

When a bug is found during implementation that is **unrelated to the current task**, do not fix it inline. Log it in `KnownIssues.md` with severity, file, root cause, and a fix hint — then continue the current task uninterrupted. Fix it in its own dedicated commit so the git history stays readable and the thesis change log stays accurate.

---

## Alpha Scope — NOT Implemented

- Networking (NGO v2.x — deferred)
- Grenade action (`ThrowGrenadeCommand`)
- Collectible crates
- AI opponent beyond stub
- Cover / overwatch / reload mechanics
- Multiple maps (only one map for alpha)
