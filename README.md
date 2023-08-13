# Hibzz.Singletons
![LICENSE](https://img.shields.io/badge/LICENSE-CC--BY--4.0-ee5b32?style=for-the-badge) [![Twitter Follow](https://img.shields.io/badge/follow-%40hibzzgames-1DA1f2?logo=twitter&style=for-the-badge)](https://twitter.com/hibzzgames) [![Discord](https://img.shields.io/discord/695898694083412048?color=788bd9&label=DIscord&style=for-the-badge)](https://discord.gg/YXdJ8cZngB) ![Unity](https://img.shields.io/badge/unity-%23000000.svg?style=for-the-badge&logo=unity&logoColor=white) ![C#](https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=c-sharp&logoColor=white)

***A library of singletons for Unity***

## Installation
**Via Github**
This package can be installed in the Unity Package Manager using the following git URL.
```
https://github.com/hibzzgames/Hibzz.Singletons.git
```

Alternatively, you can download the latest release from the [releases page](https://github.com/hibzzgames/Hibzz.Singletons/releases) and manually import the package into your project.

<br>

## Basic Usage
The singleton classes are located inside the namespace `Hibzz.Singletons` and any of the Unity MonoBehaviors can be converted into a singleton using the following syntax.

```csharp
using Hibzz.Singletons;

public class AIManager : Singleton<AIManager>
{
    public int LiveNPCCount;
}
```

Now this Singleton can be accessed using the Instance field.

```csharp
int _currentNPCCount = AIManager.Instance.LiveNPCCount;
Debug.Log($"Number of Live NPC's in scene: {_currentNPCCount}");
```

<br>

### Scriptable Singletons
This package includes support for `ScriptableSingleton`s. These are Singletons that exists as a ScriptableObject asset in the Resources folder. This allows a Scriptable Singleton to be accessed from anywhere in the project without having to be in the scene.

Moreover, these objects can be automatically created when the class is tagged with the `[CreateScriptableSingletonAsset]` attribute and the "Create Scriptable Singleton Asset" menu item is selected from the "Hibzz/Singletons" menu.

<br>

### [StaticAccess] Attribute
The Singletons package comes with a powerful tool for accessing instanced members of a Singleton class without the `Instance` field. This is done by tagging the member with a `[StaticAccess]` attribute.

```csharp
// defining the singleton
public partial class AIManager : Singleton<AIManager>
{
    [StaticAccess] int _liveNPCCount;
}

// Accessing the member
var npcCount = AIManager.LiveNPCCount;
```

<br>

Learn more about this package in the [documentation](https://docs.hibzz.games/singletons/getting-started/).

## Have a question or want to contribute?
If you have any questions or want to contribute, feel free to join the [Discord server](https://discord.gg/YXdJ8cZngB) or [Twitter](https://twitter.com/hibzzgames). I'm always looking for feedback and ways to improve this tool. Thanks!

Additionally, you can support the development of these open-source projects via [GitHub Sponsors](https://github.com/sponsors/sliptrixx) and gain early access to the projects.
