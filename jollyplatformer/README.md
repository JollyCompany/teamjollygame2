JollyPlatformer
===============

![JollyPlatformer](./jollyplatformerpreview.png?raw=true)

JollyPlatformer is a two player plaformer. Players can move, jump, and shoot stars
at one another.

Controls
--------

Player 1:

 * a/d for movement
 * w to jump
 * s to shoot stars

Player 2:
 * left/right keys for movement
 * up to jump
 * down to shoot stars

# Where to start

Load up the project in the Unity editor and open the Assets/Library/titlescreen.scene and hit play
to test the game.
Load up the Assets/Library/level1.scene to see the objects that make up the game, specifically paying
attention to the Hero objects, their components and their child objects.
Look at the Projectile and Hero prefabs, which act as the template for those objects when they need to
be created.


# Architecture

## Scenes

`titlescreen` and `level1` are the only two scenes, with the former being the way the game starts.
It functions only to load the first level. There is no way to win or lose the level, but falling
off of the platforms makes the robots no longer controllable.

## Organization

The game is laid out in a level which contains static sprites for each cloud. Each group of cloud
sprites that create a platform are grouped as children of a single game object. A
`PolygonCollider2D` is applied to the game object with manually drawn boundaries to give the
players something to stand on.

The players are represented by simple, unanimated sprites with `CircleCollider2D` and `BoxCollider2D`
colliders attached. Each has several control scripts and empty `GameObject` children used for
positioning important features required by those scripts. All this structure is wrapped into the
*Hero.prefab* file and instantiated for each player in the game.

The `Main Camera` is automatically controlled such that it pans and zooms to keep both players in
view at all times.

Other components such as the sun, background and logo are self-contained objects with no effect
on gameplay.

## Major Components

### Hero

The `Hero` component controls the main character's visual representation and movement behavior
through the world. Using commands mapped from a `HeroController`, it applies forces to the `Rigidbody2D`
to allow the character to move horizontally, jump or shoot stars.

Two interesting components of the `Hero` are the `ViewportEdgeDetector` and the `GroundDetector`.
Each references an empty child game object of the game object representing the player.
`ViewportEdgeDetector` determines when the player is about to leave the screen so that `Hero`
can halt horizontal motion. `GroundDetector` performs an intersection test with the `Ground`
layer to learn if the hero is touching the ground, and is therefore allowed to jump.

### HeroController

Each `Hero` references a `HeroController` on that object that maps user input to actions. This
abstraction is useful because:
* It allows the controller to be entirely separated from the mechanics of the hero
* Multiple players are distinguished simply by changing the `PlayerNumber` field
* In the future, AI could extend a `HeroController` without having to rewrite anything in `Player`

### CameraFollow

The `CameraFollow` behavior is attached to the `Main Camera` to allow it to keep all `Hero`
characters on screen. It tracks the center point of the heroes horizontally and vertically, and
will zoom out to accommodate characters that get further separated. The main camera is orthogonal
so instead of moving the camera further back on the Z axis (which will have no effect), `CameraFollow`
adjusts the `orthographicSize` property of the camera.

This class is designed so that more heroes can be tracked simply by adding members to the
`HeroesToFollow` array.

### TitleScreen

The first screen that loads in the game is managed by the `TitleScreen`. This object renders
a simple textured button using Unity's built-in GUI components.

### Clouds

The cloud objects are created using a single texture that is then broken up into separate objects
using Unity's built in sprite manager. You can see how this is done by clicking on the `Sprite Editor`
button on the cloud texture.

## Other Components

For documentation on the common components (`JollyDebug`, `Vector3`, `Vector2`) please see
the `../template/` directory.

## Notes

### Player Selection

If you want to have a variable number of heroes in your game, create an instance for each player
and delete heroes that will be unused at the start of the map. Don't forget to also add a check
to classes that reference heroes, such as the `CameraFollow`, that rebuilds the `HeroesToFollow`
array with only valid object references.

### Levels

To support multiple levels, check out the `LevelManager` class from the JollyTouch sample. If
you want to add collectable items like coins, there is example code in JollyTopdown for you
to use.

### Populating your world

Enemies in the game can work very similarly to players. To create an Enemy, you could:
 * Duplicate everything about a Hero as an Enemy (prefab, `Hero`, `HeroController`)
   and reassign references on the prefab to these new classes
 * Drag the Enemy prefab into the scene
 * Change the internals of the `EnemyController` (or whatever you called your duplicate `HeroController`)
   to return commands to execute based on AI.

Writing the AI is as complicated as you make it. A simple Goomba-like enemy that just moves
back and forth is a good place to start.

### Making it Move

Unity has built-in support for creating animated 2d graphics. There are two primary methods:
keyframe and spritesheet. In the keyframe method, each sprite has many parts (e.g. arms, legs, feet, body, head)
that are separated in the source image file and are to be animated individually in Unity.
A spritesheet, on the other hand, is like a GIF: each frame of the animation is drawn
individually and Unity flips between frames rapidly to display the animation. The spritesheet
method is much more art-intensive, but either are supported. To get started, search for
"Unity 2D Sprite Animation" and check out some of Unity's documentation videos. Their 2d sidescroller
project is also very helpful here.