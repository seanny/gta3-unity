# Coding Standards

This document defines the coding standards for the GTA3Unity project. All new code should follow these conventions unless an existing subsystem requires a different style for consistency.

AI-ASSISTED: OpenAI ChatGPT - July 11, 2026

## Namespace Conventions

All project scripts must be contained within the `GTA3Unity` root namespace.

Namespaces should follow the folder hierarchy beneath the project's main `Scripts` directory.

Examples:

| Script location                  | Namespace                         |
| -------------------------------- | --------------------------------- |
| `Scripts/`                       | `GTA3Unity`                       |
| `Scripts/Example/`               | `GTA3Unity.Example`               |
| `Scripts/Example/NestedExample/` | `GTA3Unity.Example.NestedExample` |

```csharp
namespace GTA3Unity.Example.NestedExample
{
    public sealed class ExampleComponent
    {
    }
}
```

Do not use folder names containing dots to represent nested namespaces.

Incorrect:

```text
Scripts/Example.NestedExample/
```

Correct:

```text
Scripts/Example/NestedExample/
```

Third-party code, generated code, and external packages do not need to use the `GTA3Unity` namespace.

## File Naming

Each source file should normally contain one primary type.

The file name must match the name of the primary type declared inside it.

Examples:

```text
PlayerController.cs
VehicleManager.cs
EGameState.cs
IInteractable.cs
```

Avoid vague file names such as:

```text
Manager.cs
Helper.cs
Utilities.cs
Data.cs
```

Use a name that describes the responsibility of the type.

## Type Naming

Classes, structs, records, delegates, and interfaces must use `PascalCase`.

```csharp
public sealed class PlayerController
{
}

public readonly struct VehicleHandle
{
}

public delegate void MissionCompletedHandler(Mission mission);
```

Interfaces must begin with `I`.

```csharp
public interface IInteractable
{
    void Interact();
}
```

Enum names must begin with `E`.

```csharp
public enum EVersion
{
    GTA3
}
```

Enum members should use `PascalCase`.

```csharp
public enum EGameState
{
    None,
    MainMenu,
    Loading,
    Playing,
    Paused
}
```

Use a `None`, `Unknown`, or `Invalid` member with a value of `0` when an enum requires an uninitialised or invalid state.

```csharp
public enum EVehicleType
{
    None = 0,
    Automobile,
    Boat,
    Train
}
```

Enums intended to be used as bit flags must use the `Flags` attribute and explicitly assigned power-of-two values.

```csharp
[Flags]
public enum EEntityFlags
{
    None = 0,
    Visible = 1 << 0,
    Collidable = 1 << 1,
    Persistent = 1 << 2
}
```

## Member Naming

Public members must use `PascalCase`.

```csharp
public int CurrentHealth { get; private set; }

public void ApplyDamage(int damage)
{
}
```

Private and protected instance fields must use `m_` followed by `PascalCase`.

```csharp
private int m_CurrentHealth;
private Transform m_TargetTransform;
```

Private static fields must use `s_` followed by `PascalCase`.

```csharp
private static int s_ActiveVehicleCount;
```

Constants must use `PascalCase`.

```csharp
private const float MaximumInteractionDistance = 3.0f;
```

Method parameters and local variables must use `camelCase`.

```csharp
public void ApplyDamage(int damageAmount)
{
    int remainingHealth = m_CurrentHealth - damageAmount;
}
```

Boolean names should describe a true or false condition clearly.

Preferred:

```csharp
bool isVisible;
bool hasFinishedLoading;
bool canEnterVehicle;
```

Avoid:

```csharp
bool visible;
bool finished;
bool enterVehicle;
```

## Properties

Use properties when exposing state outside a type.

```csharp
public int CurrentHealth { get; private set; }
```

Do not expose mutable public fields unless required by a Unity-specific API or data format.

Avoid properties that perform expensive calculations, allocate memory, modify state, or produce unexpected side effects.

Incorrect:

```csharp
public Vehicle ClosestVehicle
{
    get
    {
        return FindClosestVehicleInWorld();
    }
}
```

Prefer an explicit method:

```csharp
public Vehicle FindClosestVehicle()
{
    return FindClosestVehicleInWorld();
}
```

## Methods

Method names must begin with a verb and clearly describe the operation being performed.

Preferred:

```csharp
LoadGame()
CreateVehicle()
FindNearestPedestrian()
ApplyDamage()
```

Avoid:

```csharp
Game()
Vehicle()
NearestPedestrian()
Damage()
```

Methods should perform one clearly defined task.

Where practical, prefer early returns over deeply nested conditions.

Preferred:

```csharp
public void EnterVehicle(Vehicle vehicle)
{
    if (vehicle == null)
    {
        return;
    }

    if (!vehicle.CanBeEntered)
    {
        return;
    }

    BeginEnteringVehicle(vehicle);
}
```

Avoid:

```csharp
public void EnterVehicle(Vehicle vehicle)
{
    if (vehicle != null)
    {
        if (vehicle.CanBeEntered)
        {
            BeginEnteringVehicle(vehicle);
        }
    }
}
```

## Formatting

Use four spaces for indentation. Do not use tabs.

Opening braces must be placed on a new line.

```csharp
public void StartMission()
{
    if (m_CurrentMission == null)
    {
        return;
    }

    m_CurrentMission.Start();
}
```

Always use braces for conditional statements and loops, including single-line bodies.

Preferred:

```csharp
if (vehicle == null)
{
    return;
}
```

Avoid:

```csharp
if (vehicle == null)
    return;
```

Place one blank line between logical sections of a method.

Avoid unnecessary blank lines inside small blocks.

Lines should generally remain below 120 characters where practical.

## Access Modifiers

Always specify an access modifier for types and members.

Preferred:

```csharp
private void UpdateVehicle()
{
}
```

Avoid relying on implicit access:

```csharp
void UpdateVehicle()
{
}
```

Use the most restrictive access level possible.

Prefer `private` unless a member genuinely needs to be accessed by another type.

## Type Inference

Use `var` when the type is immediately obvious from the right-hand side.

```csharp
var vehicle = new Vehicle();
var playerPosition = transform.position;
```

Use the explicit type when it improves readability or the resulting type is not obvious.

```csharp
IReadOnlyList<Vehicle> nearbyVehicles = FindNearbyVehicles();
```

Do not use `var` merely to shorten complex type names when doing so hides useful information.

## Null Handling

Validate required arguments at public API boundaries.

```csharp
public void RegisterVehicle(Vehicle vehicle)
{
    ArgumentNullException.ThrowIfNull(vehicle);

    m_Vehicles.Add(vehicle);
}
```

For Unity objects, remember that destroyed `UnityEngine.Object` instances use Unity's custom null behaviour.

```csharp
if (m_Target == null)
{
    return;
}
```

Do not use null-forgiving operators to hide unresolved nullability problems without justification.

Avoid silently ignoring invalid state when it represents a programming error.

Use exceptions for invalid API usage and validation failures in non-frame-critical code.

Use warnings or guarded returns for recoverable runtime conditions.

## Assertions

Use assertions for conditions that should always be true during development.

```csharp
Debug.Assert(m_Player != null);
```

Assertions must not replace runtime handling when failure can occur because of user input, missing content, corrupted data, or normal gameplay conditions.

For required Unity inspector references, validate both during development and at runtime where appropriate.

```csharp
private void Awake()
{
    Debug.Assert(m_PlayerPrefab != null);

    if (m_PlayerPrefab == null)
    {
        Debug.LogError($"{nameof(PlayerSpawner)} requires a player prefab.", this);
        enabled = false;
    }
}
```

## Unity Serialized Fields

Avoid public fields solely to expose values in the Unity Inspector.

Use private serialized fields.

```csharp
[SerializeField]
private float m_MoveSpeed = 5.0f;
```

Group related inspector fields using headers where it improves clarity.

```csharp
[Header("Movement")]
[SerializeField]
private float m_MoveSpeed = 5.0f;

[SerializeField]
private float m_RotationSpeed = 180.0f;
```

Use `[Tooltip]` where the purpose or expected unit is not immediately obvious.

```csharp
[SerializeField]
[Tooltip("Maximum interaction distance in metres.")]
private float m_InteractionDistance = 3.0f;
```

Do not rename serialized fields casually. Renaming them can break existing prefabs and scenes.

When a serialized field must be renamed, use `FormerlySerializedAs`.

```csharp
[SerializeField]
[FormerlySerializedAs("m_Speed")]
private float m_MoveSpeed;
```

## Unity Component Conventions

Unity event methods must use their official names and signatures.

```csharp
private void Awake()
{
}

private void Start()
{
}

private void Update()
{
}

private void OnDestroy()
{
}
```

Keep `Update`, `FixedUpdate`, and `LateUpdate` small.

Move substantial logic into clearly named methods.

```csharp
private void Update()
{
    UpdateInput();
    UpdateMovement();
    UpdateAnimation();
}
```

Use `FixedUpdate` for physics operations involving `Rigidbody`.

Use `Update` for input and regular frame-based logic.

Avoid repeated calls to component lookup methods in frame loops.

Avoid:

```csharp
private void Update()
{
    GetComponent<Rigidbody>().AddForce(Vector3.forward);
}
```

Prefer caching the component:

```csharp
private Rigidbody m_Rigidbody;

private void Awake()
{
    m_Rigidbody = GetComponent<Rigidbody>();
}
```

Avoid using `Find`, `FindObjectOfType`, `GameObject.Find`, or similar scene-wide searches during normal gameplay.

Dependencies should normally be assigned through serialized fields, constructors in non-Unity code, factories, or explicit initialisation methods.

## MonoBehaviour Responsibilities

A `MonoBehaviour` should primarily act as a bridge between Unity and the project's gameplay or application logic.

Where practical, place reusable logic in ordinary C# classes that do not inherit from `MonoBehaviour`.

Preferred structure:

```csharp
public sealed class VehicleFuelSystem
{
    public float Fuel { get; private set; }

    public void Consume(float amount)
    {
        Fuel = Math.Max(0.0f, Fuel - amount);
    }
}
```

```csharp
public sealed class VehicleFuelComponent : MonoBehaviour
{
    private readonly VehicleFuelSystem m_FuelSystem = new();

    private void Update()
    {
        m_FuelSystem.Consume(Time.deltaTime);
    }
}
```

This makes systems easier to test outside Unity.

## ScriptableObjects

Use `ScriptableObject` assets for shared configuration or data definitions that should be authored in the Unity Editor.

Do not store mutable runtime state directly in shared `ScriptableObject` assets unless the asset is specifically designed for that purpose.

Use the `Data`, `Definition`, `Config`, or `Settings` suffix when appropriate.

```csharp
[CreateAssetMenu(
    fileName = "VehicleDefinition",
    menuName = "GTA3Unity/Vehicles/Vehicle Definition")]
public sealed class VehicleDefinition : ScriptableObject
{
}
```

## Events

Events should describe something that has already happened.

Preferred:

```csharp
public event Action<Vehicle> VehicleSpawned;
public event Action MissionCompleted;
```

Avoid imperative event names:

```csharp
public event Action SpawnVehicle;
public event Action CompleteMission;
```

Methods that raise events should begin with `On`.

```csharp
private void OnMissionCompleted()
{
    MissionCompleted?.Invoke();
}
```

Do not expose delegates as public mutable fields.

## Collections

Expose collections using read-only interfaces unless callers must modify them directly.

```csharp
private readonly List<Vehicle> m_Vehicles = new();

public IReadOnlyList<Vehicle> Vehicles => m_Vehicles;
```

Do not return a newly allocated collection from frequently called properties.

Avoid modifying a collection while iterating over it.

Use collection names that indicate they contain multiple objects.

```csharp
m_Vehicles
m_ActiveMissions
m_LoadedModels
```

## Logging

Use the appropriate Unity logging method.

```csharp
Debug.Log("Game loaded successfully.");
Debug.LogWarning("Vehicle definition is missing an audio profile.");
Debug.LogError("Failed to load the requested save file.");
```

Include sufficient context in warnings and errors.

Preferred:

```csharp
Debug.LogError(
    $"Unable to spawn vehicle '{vehicleId}' because its definition was not found.",
    this);
```

Avoid logging every frame or inside high-frequency loops.

Temporary debug logs must be removed before merging unless they provide lasting diagnostic value.

## Comments

Comments should explain why code exists, not merely repeat what the code already says.

Avoid:

```csharp
// Increase speed
speed++;
```

Prefer:

```csharp
// The original game applies the acceleration before clamping the final speed.
speed++;
```

Use XML documentation for public APIs where the behaviour, parameters, return value, or limitations are not immediately obvious.

```csharp
/// <summary>
/// Attempts to place the player inside the specified vehicle.
/// </summary>
/// <param name="vehicle">The vehicle the player should enter.</param>
/// <returns>
/// <see langword="true"/> when the enter sequence was started; otherwise,
/// <see langword="false"/>.
/// </returns>
public bool TryEnterVehicle(Vehicle vehicle)
{
    return false;
}
```

Do not leave commented-out code in source files. Version control already preserves previous implementations.

Use `TODO` comments only when they describe a concrete and actionable task.

```csharp
// TODO: Replace the temporary linear search with the world spatial index.
```

## Control Flow

Use `switch` expressions where they improve clarity.

```csharp
string displayName = gameVersion switch
{
    EVersion.GTA3 => "Grand Theft Auto III",
    _ => throw new ArgumentOutOfRangeException(nameof(gameVersion))
};
```

All switch statements or expressions should handle unexpected values.

Avoid using exceptions for normal gameplay control flow.

## Numeric Values

Use the appropriate suffix for numeric literals where required.

```csharp
float moveSpeed = 5.0f;
double duration = 5.0;
long fileSize = 1024L;
```

Do not use unexplained numeric literals.

Avoid:

```csharp
if (distance > 3.5f)
{
}
```

Prefer:

```csharp
private const float MaximumEnterVehicleDistance = 3.5f;
```

Use Unity units consistently:

* Distance: metres
* Rotation: degrees
* Time: seconds
* Velocity: metres per second

State the unit in the variable name or tooltip when it may be ambiguous.

```csharp
private float m_RespawnDelaySeconds;
```

## Async Code

Asynchronous methods must use the `Async` suffix.

```csharp
public async Task LoadGameAsync(CancellationToken cancellationToken)
{
}
```

Pass a `CancellationToken` to operations that may run for a meaningful length of time.

Avoid `async void` except for event handlers or Unity APIs that explicitly require it.

Do not access Unity objects from background threads unless the relevant Unity API explicitly supports it.

## Resource Management

Types that own disposable resources must implement `IDisposable`.

```csharp
public sealed class ArchiveReader : IDisposable
{
    public void Dispose()
    {
    }
}
```

Use `using` statements or declarations for disposable resources.

```csharp
using FileStream stream = File.OpenRead(filePath);
```

Unsubscribe from events when the subscriber's lifetime ends.

```csharp
private void OnEnable()
{
    m_MissionManager.MissionCompleted += HandleMissionCompleted;
}

private void OnDisable()
{
    m_MissionManager.MissionCompleted -= HandleMissionCompleted;
}
```

## Error Handling

Exceptions should provide useful context.

```csharp
throw new InvalidOperationException(
    $"Cannot start mission '{missionId}' because another mission is active.");
```

Do not catch an exception unless the code can recover, add meaningful context, or perform required cleanup.

Avoid:

```csharp
try
{
    LoadFile();
}
catch
{
}
```

When wrapping an exception, preserve the original exception.

```csharp
catch (IOException exception)
{
    throw new SaveLoadException(
        $"Failed to load save file '{filePath}'.",
        exception);
}
```

## Performance

Avoid unnecessary allocations in methods called every frame.

In performance-sensitive code:

* Cache component references.
* Reuse collections where practical.
* Avoid LINQ in frame loops.
* Avoid string interpolation in repeated logging paths.
* Avoid repeatedly creating delegates or closures.
* Prefer squared distance comparisons when the exact distance is unnecessary.

```csharp
float maximumDistanceSquared = maximumDistance * maximumDistance;

if ((targetPosition - currentPosition).sqrMagnitude <= maximumDistanceSquared)
{
}
```

Optimisation should be guided by profiling rather than guesswork.

Code clarity should not be sacrificed without evidence that the optimisation is needed.

## Architecture

Avoid global mutable state.

Static classes may be used for:

* Pure utility functions.
* Constants.
* Stateless conversion helpers.
* Explicit application-wide services where their lifetime is controlled.

Do not create a static singleton merely to make a system easier to access.

Dependencies should be explicit wherever practical.

Keep Unity presentation code separate from:

* Game rules.
* Save data.
* File parsing.
* Asset import logic.
* Original GTA data structures.
* Rendering abstractions.

Systems intended to run outside Unity must not reference `UnityEngine`.

## Original GTA Compatibility

Code that reproduces behaviour from the original GTA III implementation should include a comment when the behaviour appears unusual but is intentional.

```csharp
// GTA III checks the current animation state before validating the vehicle seat.
// Preserve this order for behavioural compatibility.
```

Do not silently "fix" original behaviour when the project requires compatibility.

Compatibility changes should document whether they are:

* Original behaviour.
* Intentional bug-for-bug compatibility.
* A GTA3Unity-specific correction.
* A modernised replacement.

Types that directly represent original game structures should retain recognisable names where doing so helps comparison with the original source.

New project-facing APIs should follow modern C# naming conventions.

## Testing

Non-Unity logic should be developed so that it can be tested using regular .NET unit tests where practical.

Test names should describe the condition and expected result.

```csharp
public void ApplyDamage_WhenDamageExceedsHealth_SetsHealthToZero()
{
}
```

Use the following naming format:

```text
MethodName_WhenCondition_ExpectedResult
```

Tests must not depend on execution order.

Each test should arrange its own state and clean up any resources it creates.

## Code Organisation

Organise scripts by feature or subsystem rather than placing every type into broad folders such as `Managers`, `Helpers`, or `Misc`.

Preferred:

```text
Scripts/
├── Audio/
├── Characters/
├── Core/
├── Missions/
├── Rendering/
├── SaveSystem/
├── Vehicles/
└── World/
```

A subsystem may contain its own runtime, editor, data, and test code.

```text
Vehicles/
├── Runtime/
├── Data/
├── Editor/
└── Tests/
```

Editor-only scripts must be placed inside an `Editor` directory or an editor-only assembly definition.

Runtime assemblies must not reference editor assemblies.

## Compiler Warnings

New code must compile without warnings.

Do not disable warnings globally to hide problems.

A warning suppression must be narrowly scoped and accompanied by a comment explaining why it is safe.

```csharp
#pragma warning disable CS0618 // Required while importing the legacy GTA data format.
LegacyImporter.Import();
#pragma warning restore CS0618
```

## General Principles

Code should prioritise:

1. Correctness.
2. Readability.
3. Maintainability.
4. Testability.
5. Performance.

Prefer straightforward code over clever abstractions.

Do not add an abstraction until it solves an existing problem or clearly supports an imminent feature.

Follow the established conventions of the surrounding subsystem when modifying existing code, unless the purpose of the change is to standardise that subsystem.