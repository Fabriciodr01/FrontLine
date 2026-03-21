# FrontLine Decision Log

This file records short architecture and scope decisions as the project evolves. Keep entries brief and practical.

## 2026-03-21

### Alpha scope reduction
- The alpha will focus on a stable tactical battle loop, one designed map, one collectible type, one grenade action, stub AI, and polish.
- Networking, DI migration, and multi-map scope are intentionally deferred.

Reason:
- One month is enough for a strong vertical slice, not a broad feature set.

### Documentation during development
- Architecture, roadmap, decisions, and testing notes will be maintained in the repo while features are built.

Reason:
- The thesis will be easier to write if technical decisions are captured at implementation time instead of reconstructed later.

### Anti-overengineering rule
- New systems should be extensible but lightweight.
- We will prefer small clean abstractions over speculative frameworks.

Reason:
- The project needs professional structure, but the TCC alpha still has strict time constraints.
