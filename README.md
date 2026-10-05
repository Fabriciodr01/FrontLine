# FrontLine
Developed as an academic project focused on software engineering and game development.

**FrontLine** is a tactical, turn-based strategy game developed in Unity as part of an academic study on the application of **software design patterns in game development**.

The project was created to explore how established software engineering patterns and architectural techniques can be applied to common game-development problems such as player actions, turn management, game states, AI decision-making, combat, and communication between systems.

## Overview

FrontLine is a tactical combat game where two players command units on a grid-based battlefield.

Players can:

* Move units across the battlefield
* Attack enemy units
* Manage action points
* Throw grenades
* Use smoke to affect combat
* Collect grenade supplies
* Take advantage of positioning and line of sight
* Play through alternating turns until one player wins

The game was designed not only as a playable prototype, but also as a practical case study for evaluating the use of software design patterns in game development.

## Design Patterns & Architecture

A major goal of FrontLine was to apply software design patterns to real gameplay systems rather than implementing them as isolated examples.

### Command Pattern

Player actions are represented as commands through the `ICommand` abstraction.

Examples include:

* `MoveCommand`
* `ShootCommand`
* `ThrowGrenadeCommand`
* `EndTurnCommand`

Commands encapsulate gameplay actions and their execution, allowing the game to process different actions through a common interface.

The `CommandProcessor` is responsible for executing commands and coordinating their resulting side effects.

### State Pattern

The game's progression is controlled through explicit game phases using `GameStateMachine`.

The game can transition between states such as:

* Waiting for Players
* Setup
* Player 1 Turn
* Player 2 Turn
* Game Over

Transitions are explicitly validated, preventing invalid changes in the game flow.

### Behavior Tree

Enemy AI uses a behavior-tree structure to represent decision-making.

The implementation includes:

* `IBehaviorNode`
* `SelectorNode`
* `SequenceNode`
* Action nodes

This allows AI behavior to be composed from smaller decision-making components instead of placing all AI logic inside a single class.

### Service Locator

Shared gameplay services are registered and retrieved through a `ServiceLocator`.

Services include systems such as:

* Combat resolution
* Line-of-sight calculations
* Map loading
* Grid-related logic

This provides a centralized way for gameplay systems to access shared services.

### Event-Driven Communication

Gameplay systems communicate through C# events in several places.

For example, the command-processing layer exposes events for:

* Command execution
* Unit damage
* Unit deaths
* Turn completion
* Game over
* Grenade collection
* Game-state changes

This reduces direct coupling between gameplay logic and presentation systems.

## Architecture

The project separates gameplay responsibilities into several areas:

```text
_Game/
├── AI/
│   └── BehaviorTree/
├── Commands/
├── Controllers/
├── Input/
├── Models/
├── Services/
├── UI/
└── Views/
```

### Models

Contains the game's core data and state, including:

* Units
* Tiles
* Game state
* Combat information
* Grenades
* Health
* Map data

### Commands

Contains player actions represented as command objects.

### Controllers

Coordinates higher-level gameplay systems such as:

* Turn management
* Command processing
* Game-state transitions
* Camera behavior

### Services

Contains reusable gameplay logic such as:

* Combat resolution
* Grid calculations
* Line-of-sight
* Map loading

### Views

Responsible for representing gameplay objects and state visually in Unity.

### AI

Contains the tactical AI and behavior-tree implementation.

## Technology

* **Unity 6**
* **C#**
* Unity Input System
* Unity Netcode for GameObjects
* Unity AI Navigation
* Universal Render Pipeline (URP)
* Unity Test Framework

The project currently targets **Unity 6000.3.9f1**.

## Academic Context

FrontLine was developed as the practical component of a paper/study on:

> **Software Design Patterns Applied to Game Development**

The purpose of the project was to demonstrate how software design patterns can be adapted to the particular constraints and requirements of game development.

Rather than implementing patterns as purely theoretical examples, the project uses them as part of actual gameplay systems, making it possible to evaluate their benefits, trade-offs, and applicability in a real game architecture.

## Project Goals

The main goals of the project were:

1. Develop a functional tactical game prototype.
2. Identify common architectural problems encountered during game development.
3. Apply appropriate software design patterns to those problems.
4. Evaluate how the patterns affect code organization, maintainability, and extensibility.
5. Demonstrate practical applications of software engineering concepts within game development.
