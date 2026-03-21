# FrontLine Testing Checklist

## Core Match Flow
- Start match and confirm Player1 turn begins correctly
- Select a friendly unit with AP
- Move a unit and confirm AP is consumed correctly
- Shoot an enemy in range and confirm the command resolves correctly
- End turn and confirm Player2 turn starts correctly
- Eliminate all units of one side and confirm game over triggers correctly

## Input / UX
- Tap empty tile while idle
- Tap empty tile while a unit is selected
- Toggle move on and off
- Toggle shoot on and off
- Cancel from each selection state
- Confirm popup execute and cancel behavior
- Verify camera drag works on mobile input path

## Rule Consistency
- Highlighted move tiles match actual legal move commands
- Highlighted shoot targets match actual legal shot commands
- Hit chance shown in the popup matches combat calculation
- Units with no AP cannot be selected for actions

## Visual Sync
- Unit world position matches model position after movement
- HP bars update after damage
- Dead units are removed from both state and scene
- Exhausted visuals update correctly when AP reaches zero

## Future Additions

### Crates
- Moving onto a crate triggers pickup correctly
- Crate disappears from state and scene after pickup
- Unit does not receive duplicate grenade ammo beyond the alpha rule

### Grenade
- Grenade action appears only when available
- Grenade targeting and confirmation work correctly
- Grenade result matches UI feedback and state changes

### AI
- AI takes legal turns only
- AI can finish its turn without stalling
- AI can use move/shoot flow correctly
