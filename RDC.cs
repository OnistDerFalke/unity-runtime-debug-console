/*
 *      MIT License
 *      # Copyright (c) 2025 OnistDerFalke
 *      # This software is provided "as is", without any warranty. You can use, modify, and share it freely.
 */

using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.Profiling;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

/// <summary>
/// Main class of RDC (Runtime Debug Console).
/// </summary>
public class RDC : MonoBehaviour
{
    [SerializeField] private GameObject rdcObject;

    private void Awake()
    {
        TelemetryAwake();
        ConsoleAwake();
        FpsGraphAwake();

        rdcObject.SetActive(false);
    }

    private void Start()
    {
        TelemetryStart();
        ConsoleStart();
        FPSGraphStart();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
            rdcObject.SetActive(!rdcObject.activeSelf);

        if (!rdcObject.activeSelf)
            return;

        TelemetryUpdate();
        ConsoleUpdate();
        FpsGraphUpdate();
    }

    private void OnDestroy()
    {
        TelemetryOnDestroy();
        ConsoleOnDestroy();
        FpsGraphOnDestroy();
    }


    #region Telemetry

    [SerializeField] private TextMeshProUGUI telemetryFps;  //Text for FPS telemetry value
    [SerializeField] private TextMeshProUGUI telemetryRam;  //Text for RAM used by mono + system
    [SerializeField] private TextMeshProUGUI telemetryGc;   //Text for garbage collector and time since last gc
    [SerializeField] private TextMeshProUGUI telemetryGms;  //Text for number of gameobjects currently on scene
    [SerializeField] private TextMeshProUGUI telemetryScn;  //Text for information about scene
    [SerializeField] private TextMeshProUGUI telemetryTrn;  //Text for number of triangles
    [SerializeField] private TextMeshProUGUI telemetryDc;   //Text for draw calls / batches
    [SerializeField] private TextMeshProUGUI telemetryAc;   //Text for audio channels
    [SerializeField] private TextMeshProUGUI telemetrySys;  //Text for system info

    private float fpsUpdateInterval = 0.5f;
    private float fpsTimer = 0f;
    private int frameCount = 0;
    private float lastGCCheckTime = 0f;
    private long lastGCTotalMemory = 0;

    private void TelemetryAwake()
    {

    }

    private void TelemetryStart()
    {
        lastGCTotalMemory = GC.GetTotalMemory(false);
        lastGCCheckTime = Time.realtimeSinceStartup;
    }

    private void TelemetryUpdate()
    {
        frameCount++;
        fpsTimer += Time.unscaledDeltaTime;

        if (fpsTimer >= fpsUpdateInterval)
        {
            UpdateTelemetry();
            frameCount = 0;
            fpsTimer = 0f;
        }
    }

    private void TelemetryOnDestroy()
    {

    }

    private void UpdateTelemetry()
    {
        // FPS
        float fps = frameCount / fpsTimer;
        telemetryFps.text = $"FPS: {fps:F1}";

        // RAM
        float monoUsedMb = Profiler.GetMonoUsedSizeLong() / (1024f * 1024f);
        float systemUsedMb = Profiler.GetTotalAllocatedMemoryLong() / (1024f * 1024f);
        telemetryRam.text = $"RAM: Mono {monoUsedMb:F1} MB / Total {systemUsedMb:F1} MB";

        // GC
        long gcMemory = GC.GetTotalMemory(false);
        float gcDelta = (gcMemory - lastGCTotalMemory) / (1024f * 1024f);
        float timeSinceLastCheck = Time.realtimeSinceStartup - lastGCCheckTime;
        telemetryGc.text = $"GC: {gcMemory / (1024f * 1024f):F1} MB (delta: {gcDelta:F2} MB, {timeSinceLastCheck:F1}s)";

        lastGCTotalMemory = gcMemory;
        lastGCCheckTime = Time.realtimeSinceStartup;

        // GameObjects
        telemetryGms.text = $"Objects: {FindObjectsOfType<GameObject>().Length}";

        // Scene info
        string sceneName = SceneManager.GetActiveScene().name;
        telemetryScn.text = $"Scene: {sceneName} | Time: {Time.time:F1}s | TS: {Time.timeScale:F1}";

        //Draw calls
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        telemetryTrn.text = "Triangles: " + UnityEditor.UnityStats.triangles.ToString();
        telemetryDc.text = "Draw calls: " + UnityEditor.UnityStats.drawCalls.ToString();
#else
        telemetryTrn.text = "Triangles: " +"N/A (Only for Unity Editor or development build)";
        telemetryDc.text = "Draw calls: " + "N/A (Only for Unity Editor or development build)";
#endif

        //Audio Channels
        telemetryAc.text = "Audio channels playing sounds: " + FindObjectsOfType<AudioSource>().Count(a => a.isPlaying).ToString();

        //System info
        telemetrySys.text = $"System info:\n" +
                     $"- CPU: {SystemInfo.processorType}\n" +
                     $"- Cores: {SystemInfo.processorCount}\n" +
                     $"- RAM: {SystemInfo.systemMemorySize} MB\n" +
                     $"- GPU: {SystemInfo.graphicsDeviceName}\n" +
                     $"- OS: {SystemInfo.operatingSystem}";
    }
    #endregion

    #region Console

    [SerializeField] private TextMeshProUGUI outputText;  //Text for console output
    [SerializeField] private ScrollRect outputScrollRect; //Scroll rect for output text
    [SerializeField] private TMP_InputField inputField;   //Input field for console

    private Queue<string> logBuffer = new Queue<string>();
    private const int maxLogLines = 50;
    private DebugCommandHandler commandHandler;

    private void ConsoleAwake()
    {
        commandHandler = new DebugCommandHandler();
        inputField.onSubmit.AddListener(HandleInputSubmit);
    }

    private void ConsoleStart()
    {
        Application.logMessageReceived += HandleLog;
    }

    private void ConsoleUpdate()
    {

    }

    private void ConsoleOnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        string prefix = type switch
        {
            LogType.Warning => "<color=yellow>[Warning]</color> ",
            LogType.Error => "<color=red>[Error]</color> ",
            LogType.Exception => "<color=red>[Exception]</color> ",
            _ => "<color=grey></color>",
        };

        logBuffer.Enqueue(prefix + logString);

        while (logBuffer.Count > maxLogLines)
            logBuffer.Dequeue();

        outputText.gameObject.GetComponent<TMPAutoSizeHeight>().SetText(string.Join("\n", logBuffer));
        Canvas.ForceUpdateCanvases();
        outputScrollRect.verticalNormalizedPosition = 0f;
    }

    private void HandleInputSubmit(string input)
    {
        commandHandler.ExecuteCommand(input);
        inputField.text = "";
        inputField.ActivateInputField();
    }

    #endregion

    #region FPS Graph

    [SerializeField] private RawImage graphImage;
    [SerializeField] private int width = 1000;
    [SerializeField] private int height = 500;

    private Texture2D texture;
    private Queue<float> fpsSamples = new Queue<float>();

    private Color backgroundColor = Color.black;
    private Color lineColor = Color.green;

    void FpsGraphAwake()
    {

    }

    void FPSGraphStart()
    {
        texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        ClearTexture();
        graphImage.texture = texture;
    }

    void FpsGraphUpdate()
    {
        float currentFps = 1f / Time.unscaledDeltaTime;
        if (fpsSamples.Count >= width)
            fpsSamples.Dequeue();
        fpsSamples.Enqueue(currentFps);

        DrawGraph();
    }

    void FpsGraphOnDestroy()
    {

    }

    void ClearTexture()
    {
        Color[] resetColors = new Color[width * height];
        for (int i = 0; i < resetColors.Length; i++)
            resetColors[i] = backgroundColor;
        texture.SetPixels(resetColors);
        texture.Apply();
    }

    void DrawGraph()
    {
        ClearTexture();

        float[] samples = fpsSamples.ToArray();
        float maxFps = samples.Length > 0 ? Mathf.Max(samples) : 120f;

        for (int x = 0; x < samples.Length - 1; x++)
        {
            int y1 = Mathf.Clamp((int)(samples[x] / maxFps * height), 0, height - 1);
            int y2 = Mathf.Clamp((int)(samples[x + 1] / maxFps * height), 0, height - 1);
            DrawLine(x, y1, x + 1, y2, lineColor);
        }

        texture.Apply();
    }

    //Bresenham's line algorithm for drawing between two points
    void DrawLine(int x0, int y0, int x1, int y1, Color col)
    {
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            texture.SetPixel(x0, y0, col);
            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx) { err += dx; y0 += sy; }
        }
    }

    #endregion
}


/// <summary>
/// Class defining attribute for console command method.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class DebugCommandAttribute : Attribute
{
    public string Name;
    public string Description;

    public DebugCommandAttribute(string name, string description = "")
    {
        Name = name.ToLowerInvariant();
        Description = description;
    }
}

/// <summary>
/// Handler for commnads in debug console.
/// </summary>
public class DebugCommandHandler
{
    private class CommandInfo
    {
        public MethodInfo Method;
        public object Target;
        public DebugCommandAttribute Attribute;
    }

    private readonly Dictionary<string, CommandInfo> commands = new();

    public DebugCommandHandler()
    {
        RegisterAllCommands();
    }

    private void RegisterAllCommands()
    {
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
        {
            foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var attr = method.GetCustomAttribute<DebugCommandAttribute>();
                if (attr != null)
                {
                    string key = attr.Name.ToLowerInvariant();
                    if (commands.ContainsKey(key))
                    {
                        Debug.LogWarning($"Command {key} already registered.");
                        continue;
                    }

                    object target = null;
                    if (!method.IsStatic)
                    {
                        target = UnityEngine.Object.FindObjectOfType(type);
                        if (target == null)
                        {
                            Debug.LogWarning($"Instance of {type.Name} not found for command '{key}'.");
                            continue;
                        }
                    }

                    commands[key] = new CommandInfo
                    {
                        Method = method,
                        Target = target,
                        Attribute = attr
                    };
                }
            }
        }
    }

    public void ExecuteCommand(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return;

        string[] tokens = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string cmd = tokens[0].ToLowerInvariant();
        string[] args = tokens.Length > 1 ? tokens[1..] : Array.Empty<string>();

        if (cmd == "help")
        {
            string helpText = args.Length == 0 ? GetHelp() : GetHelp(args[0]);
            Debug.Log(helpText);
            return;
        }

        if (!commands.TryGetValue(cmd, out var commandInfo))
        {
            Debug.LogWarning($"Unknown command: {cmd}");
            return;
        }

        var parameters = commandInfo.Method.GetParameters();
        if (parameters.Length != args.Length)
        {
            Debug.LogWarning($"Expected {parameters.Length} args, got {args.Length}");
            return;
        }

        object[] parsedArgs = new object[args.Length];
        for (int i = 0; i < args.Length; i++)
        {
            try
            {
                parsedArgs[i] = Convert.ChangeType(args[i], parameters[i].ParameterType);
            }
            catch
            {
                Debug.LogWarning($"Invalid argument #{i + 1}: '{args[i]}'");
                return;
            }
        }

        try
        {
            commandInfo.Method.Invoke(commandInfo.Target, parsedArgs);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Command execution error: {ex.InnerException?.Message ?? ex.Message}");
        }
    }

    public string GetHelp(string command = null)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            var list = commands.Select(c =>
            {
                var p = c.Value.Method.GetParameters();
                string args = string.Join(" ", p.Select(a => $"<{a.Name}>"));
                return $"<b>{c.Key}</b> {args} — {c.Value.Attribute.Description}";
            });
            return string.Join("\n", list);
        }
        else
        {
            if (!commands.TryGetValue(command.ToLowerInvariant(), out var info))
                return $"Command '{command}' not found.";

            var p = info.Method.GetParameters();
            string args = string.Join("\n", p.Select(a => $"- <b>{a.Name}</b>: {a.ParameterType.Name}"));
            return $"<b>{info.Attribute.Name}</b>\n{info.Attribute.Description}\n\nArguments:\n{args}";
        }
    }
}