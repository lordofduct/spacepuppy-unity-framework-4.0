# spacepuppy-unity-framwork-4.0

A modular framework of tools for use with the Unity game engine version 2020.3

This repository primarily exists as a way for me to just share the tools I personally use when making games. This project is constantly in flux and should seldom be considered "stable". Use at your own risk.

## com.spacepuppy

Core library, all other libraries in spacepuppy depend on this library. Core functionality includes:

SPComponent - an extension of MonoBehaviour with support for mixins as well as inheriting contract interfaces like IGameObjectsource, and IComponent.
GameLoop - A global hook into the Update/FixedUpdate loops of the game.
SPEntity - a foundation class placed on the root GameObject of an entity that facilitates treating its entire hierarchy as a single "entity".
SPTime - object-identity for time suppliers as well as ability to create custom timesuppliers and reference them through the inspector
MultiTag - ability to add multiple tags to a GameObject
SPEvent - similar to UnityEvent but predates it with some added features like "Trigger All On Target" and IObserverableTrigger and hijacking.
Collections - various collection implementations from a BinaryHeap to temporary/reusable lists to reduce GC on collections.
Dynamic - generaly reflection tools
VariantReference - ability reference a value of varying type through the inspector/serialization.

## com.spacepuppy.AI

A basic AI boilerplate for writing AI in Unity. This has mostly been replaced by our mecanim based AI, see com.spacepuppy.Mecanim

Dependencies:
com.spacepuppy, 
com.spacepuppy.radicalcoroutine, 
com.spacepuppy.geom, 
com.spacepuppy.pathfinding, 
com.spacepuppy.sensors, 
com.spacepuppy.statemachine

## com.spacepuppy.Anim

An extension of the legacy animation system in Unity. Mostly unnecessary today as we've moved to Mecanim, exists for historical purposes.

Dependencies:
com.spacepuppy, 
com.spacepuppy.radicalcoroutine

## com.spacepuppy.Audio

Incomplete at this time.

Dependencies:
com.spacepuppy, 
com.spacepuppy.radicalcoroutine

## com.spacepuppy.Cameras

Manager from cameras and a contract interface ICamera for complex cameras.

Dependencies:
com.spacepuppy

## com.spacepuppy.Geom

Various geometry/math helpers.

Dependencies:
com.spacepuppy

## com.spacepuppy.Input

Our custom input system to supplement the built in Unity input system. This predates the new input system from Unity (which we actually haven't used since this is our primary input system).

Dependencies:
com.spacepuppy, 
com.spacepuppy.radicalcoroutine

## com.spacepuppy.Mecanim

Various tools to help working with Mecanim. Things from "override layers" which facilitate adding/removing override animations as layers by id/token. As well as a BehaviourStateMachine to Component bridge for scripting against the statemachine as well as facilitating AI logic.

Dependencies:
com.spacepuppy, 
com.spacepuppy.radicalcoroutine

## com.spacepuppy.Motor

Generalized interface IMotor for moving entities around regardless of if they use Rigidbody or CharacterController. As well as movement style controller.

Dependencies:
com.spacepuppy, 
com.spacepuppy.radicalcoroutine, 
com.spacepuppy.Geom

## com.spacepuppy.Pathfinding

Implementations of Djikstra/A* as well as IPathSeeker/IPath contract interfaces to abstract which pathfinding system you use from your movement logic (may you use Unity, AronGranberg, your own, whatever.

See SpacepuppyAGExtensions for integrating with AronGranberg A* project. Note you MUST supply your own version of AG A* and some features may require the pro license of AG A*.

Dependencies:
com.spacepuppy, 
com.spacepuppy.radicalcoroutine

## com.spacepuppy.PathfindingMotor

Some classes that bridge com.spacepuppy.Motor and com.spacepuppy.Pathfinding

Dependencies:
com.spacepuppy, 
com.spacepuppy.radicalcoroutine, 
com.spacepuppy.Motor, 
com.spacepuppy.Pathfinding

## com.spacepuppy.RadicalCoroutine

Advanced coroutine support. A lot of the features such as custom yield instructions and object identity for coroutines predate these features being added to Unity. Most of the features are no longer necessary in current versions of Unity.

Dependencies:
com.spacepuppy

## com.spacepuppy.Scenes

A SceneManager for managing scenes in Unity

Dependencies:
com.spacepuppy, 
com.spacepuppy.radicalcoroutine

## com.spacepuppy.Sensors

Can be used in tandem with AI to create vision sensors that facilitate an entities ability to sense other entities around them via "aspects" attached to them as components.

Dependencies:
com.spacepuppy, 
com.spacepuppy.radicalcoroutine, 
com.spacepuppy.Triggers

## com.spacepuppy.Serialization

Various serialization tools for save game states.

Dependencies:
com.spacepuppy

## com.spacepuppy.Spawn

A Spawn manager for pooling spawnable objects.

Dependencies:
com.spacepuppy, 
com.spacepuppy.radicalcoroutine

## com.spacepuppy.StateMachine

Various statemachine implementations.

Dependencies:
com.spacepuppy

## com.spacepuppy.Triggers

Various components that utilize the SPEvent system in com.spacepuppy to allow editor based scripting through what we call the "T&I System". Essentially T's are triggers that trigger/call I's on certain events. For example T_OnEnterTrigger will occur when a trigger collider is entered which can then call a I_PlaySoundEffect which... plays a sound effect.

Dependencies:
com.spacepuppy, 
com.spacepuppy.radicalcoroutine, 
com.spacepuppy.tween

## com.spacepuppy.Tween

A tween library.

Dependencies:
com.spacepuppy, 
com.spacepuppy.radicalcoroutine

## com.spacepuppy.Waypoints

A waypoint system with in scene editor.

Dependencies:
com.spacepuppy, 
com.spacepuppy.radicalcoroutine, 
com.spacepuppy.Tween

## com.spacepuppy.Extensions

A random assortment of tools that tap into different parts of Spacepuppy.

Dependencies:
com.spacepuppy, 
com.spacepuppy.radicalcoroutine, 
com.spacepuppy.Camera, 
com.spacepuppy.Geom, 
com.spacepuppy.Motor, 
com.spacepuppy.Pathfinding, 
com.spacepuppy.Tween, 
TextMesh Pro

## com.spacepuppy.AGAstarExtensions

A bridge between the Aron Granber A* Project and Spacepuppy Pathfinding/Motor libraries.

Dependencies:
com.spacepuppy, 
com.spacepuppy.radicalcoroutine, 
com.spacepuppy.Geom, 
com.spacepuppy.Pathfinding, 
com.spacepuppy.Motor, 
com.spacepuppy.PathfindingMotor, 
AstarPathfindingProject