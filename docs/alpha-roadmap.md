# FrontLine Alpha Roadmap

## Goal
Deliver a playable mobile alpha that demonstrates solid software design patterns and a clean technical structure suitable for the TCC scope.

## Principles
- Favor simple, complete systems over broad feature scope.
- Preserve command-driven gameplay flow for all player actions.
- Keep Unity-facing code as adapters/views, not rule owners.
- Add only features that strengthen both the game and the thesis narrative.
- Avoid overengineering; solve the current alpha needs with clear extension points.

## Alpha Scope
- Stable 1v1 turn-based battle loop
- One intentional playable map
- Basic win/lose flow
- One collectible type: supply crate
- One throwable type: grenade
- Stub AI for Player2
- Focused bug fixing and playtesting

## Out of Scope
- Networking
- VContainer migration
- Multiple maps
- Advanced inventory systems
- Complex throwable variants
- Cover, overwatch, reload, weapon switching

## Execution Order

### Phase 1: Core Loop Stabilization
- Align gameplay rules between selection, commands, and combat resolution
- Fix initialization order to match the intended architecture
- Validate turn flow, AP consumption, kill flow, and end-game transitions
- Validate mobile input and camera behavior

Acceptance:
- A full match can be played without softlocks or rule mismatches
- UI feedback matches actual command validation
- Opening turn state is consistent every run

### Phase 2: Vertical Slice Map
- Build one designed combat map with blocked tiles and meaningful lanes
- Replace pure sandbox placement with a deliberate encounter layout

Acceptance:
- Map supports tactical movement, flanking pressure, and crate placement
- Scene is presentation-ready for alpha playtests

### Phase 3: Collectible Crates
- Add a simple collectible crate object placed on the map
- Crate is consumed when a unit moves onto it
- Crate grants one grenade if the unit does not already have one

Acceptance:
- Crates are represented in state and view
- Pickup happens through the command flow, not ad hoc scene logic
- Crate visuals and gameplay state remain synchronized

### Phase 4: Grenade Action
- Add one grenade action per unit maximum
- Add targeting/confirmation flow consistent with existing action architecture
- Resolve grenade use through a dedicated command

Acceptance:
- Throw action follows the same command/state/UI flow as move and shoot
- Grenade is simple, readable, and tactically useful
- No special-case logic bypasses the architecture

### Phase 5: Stub AI
- Add a basic AI turn handler for Player2
- Prefer legal and understandable behavior over sophistication

Acceptance:
- AI can complete turns without hanging
- AI uses the same action pipeline as the player
- Matches are fully playable solo

### Phase 6: Alpha Hardening
- Main menu / scene flow polish
- Win/lose UX cleanup
- Bug fixing and balancing
- Documentation pass for architecture and design rationale

Acceptance:
- The alpha is stable enough to demo repeatedly
- Architecture and design choices are documented for thesis use
