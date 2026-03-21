# FrontLine Architecture

## Objective
Use a small set of design patterns in a practical way to support a scalable tactical prototype without turning the project into an overengineered framework.

## Core Patterns

### Command
- All gameplay actions should be represented as self-validating commands.
- Commands are the authoritative path for changing gameplay state.
- `CommandProcessor` is the central execution entry point.

Current gameplay actions:
- Move
- Shoot
- EndTurn

Planned alpha actions:
- ThrowGrenade

### State Machine
- `GameStateMachine` controls macro match phases.
- `SelectionManager` controls player interaction state for local unit/action selection.

The design intent is:
- game phase state decides whose turn and whether the battle is active
- selection state decides what the current local input means

### Service Locator
- `GameBootstrapper` performs explicit runtime composition.
- Scene systems resolve dependencies through `ServiceLocator`.

Constraint:
- service registration order must stay explicit and deterministic
- Service Locator is acceptable here because the project is small and the TCC benefits from visible composition flow

### Observer
- Systems communicate through C# events.
- UI and views should react to events instead of polling or owning gameplay logic.

### Strategy
- Input origin should be swappable between local player and AI.
- This matters directly for the planned Player2 stub AI.

## Layering Rules

### Models
- Pure C# only
- No `UnityEngine` references
- Own gameplay data, not scene presentation

Current examples:
- `GameState`
- `TileData`
- `UnitData`

### Controllers / Services
- Own rules, orchestration, validation, and state transitions
- Should remain testable in isolation where practical

Current examples:
- `TurnController`
- `CommandProcessor`
- `CombatResolver`
- `GameStateMachine`

### Views / MonoBehaviours
- Translate gameplay state into scene representation
- Forward player input as intents
- Must not become the hidden owner of gameplay rules

Current examples:
- `GridManager`
- `UnitView`
- `CameraController`
- `HUDController`

## Alpha Architecture Priorities
- Remove duplicated or unsynchronized state ownership
- Keep rule calculations consistent across selection/UI/command layers
- Make initialization order match the intended composition model
- Add new features by extending the existing architecture, not bypassing it

## Documentation Rule
When a meaningful architectural decision is made, record it briefly in `docs/decision-log.md`.
