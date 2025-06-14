# Unity Runtime Debug Console

Unity Runtime Debug Console is a real-time debugging extension that provides:
- a clear FPS monotonicity graph,
- essential performance telemetry,
- and a powerful in-game debug console.

The console captures all log messages from the standard debug stream and allows you to invoke native debug methods—or your own custom functions—by simply marking them with an attribute in your code. It's a practical tool for efficient runtime diagnostics and testing without stopping the game. It works like an overlay opened/closed by pressing F5 key.

![image](https://github.com/user-attachments/assets/2e1c9743-0e92-4f8a-9824-adf95e0e17b1)


## Installation
1. Import Unity Package or repository content to your Unity Project.
2. Inside the packages there are three scripts and prefab.

   ![image](https://github.com/user-attachments/assets/1225fcb3-1097-4ba3-ad52-ad090d4089b6)

4. Add prefab to the scene, attach it to the root.
5. Run the game and press F5, the console should appear.

## Command line
Runtime Debug console handles two types of methods:
- native - Declared in RDCCommands script, you can add as many native methods as you wish, but this methods should not need a reference to any other object on scene than RDC prefab with it's children. Example native command from RDCCommands class:

   
```/// <summary>
/// Resets timescale.
/// </summary>
[DebugCommand("reset_timescale", "Resets global timescale to default value.")]
private void ResetTimeScale()
{
    Time.timeScale = 1f;
    Debug.Log($"Time scale has been reset.");
}
```


- external - declared somewhere in your code, methods should include arguments that can be easily parsed in c# (floats, ints, strings etc.). All commands you can easily create using attribute:

```
[DebugCommand({string: command}, {string: description})]
private void Method(int arg)
{
    val = arg;
    Debug.Log($"Value has been set to {value}.");
}
```

## Help
There is an additional command that will list both native and external commands to the console output: "help". That is where the descriptions from DebugCommand attributes are shown.

![image](https://github.com/user-attachments/assets/5ed3a7bb-3822-48ad-8766-9152fbe34af7)


