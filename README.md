![SmartDebugger — In-game debugging for Unity](docs/images/banner.png)

English · [日本語](README.ja.md)

**Inspect your game where the problem happens.**

Runtime variables, logs, performance graphs, and bug reports — inside your Unity build.

[OpenUPM](https://openupm.com/packages/com.github.kurobon.smart_debugger/) ·
[Quick start](#quick-start) ·
[API example](#runtime-variables) ·
[Changelog](CHANGELOG.md) ·
[MIT license](LICENSE)

SmartDebugger adds an extensible debug menu to Unity games, with a focus on Android and iOS. It also works in standalone builds. Inspect device information, adjust parameters, read Unity logs, and collect reports directly on the device, without an Editor connection.

## At a glance

| Tool | What you can do |
| --- | --- |
| **System** | Inspect build identifiers, OS, device, CPU, GPU/driver, memory, display/safe area, rendering and frame rate settings, system language, uptime, and time scale. |
| **Variables** | Expose typed values, sliders, selections, labels, and actions through a small C# API. |
| **Logs** | Filter by severity or text, inspect stack traces, copy entries, and follow incoming logs. |
| **Profiler** | Watch FPS, frame times, and managed/total allocated and reserved memory. |
| **Bug reports** | Send a description, system information, logs, screenshots, and optional recordings. |

![In-game log viewer with severity filters and stack traces](docs/images/log.png)

## Installation

The core package declares **Unity 2022.1+** support. Screen recording requires **Unity 2022.3+** and the optional InstantReplay integration.

### OpenUPM

1. Open **Edit → Project Settings → Package Manager**.
2. Add this scoped registry:

   | Field | Value |
   | --- | --- |
   | Name | `OpenUPM` |
   | URL | `https://package.openupm.com` |
   | Scope | `com.github.kurobon` |

3. Open **Window → Package Manager**, select **My Registries**, and install **SmartDebugger** (`com.github.kurobon.smart_debugger`).

SimpleScroll, uGUI, Image Conversion, and Screen Capture are declared dependencies and are resolved by the Package Manager. Input System and InstantReplay are optional.

### Package Manager screenshots

![Add the OpenUPM scoped registry](docs/images/install1.png)
![Install SmartDebugger from My Registries](docs/images/install2.png)

## Quick start

1. Create a settings asset with **Assets → Create → SmartDebugger → SD Settings**. New settings copy the package's default configuration in the Editor.
2. Add this asset to **Project Settings → Player → Other Settings → Preloaded Assets**.
3. Leave **Auto Initialize** enabled and enter Play mode or launch your build.
4. Open the menu with **Ctrl + Shift + F1**, or **three-finger triple tap** on a touch device. Press **Escape** to close it on a keyboard.

The default touch gesture allows up to **0.5 seconds** between taps. Shortcuts and gestures are configurable in `SDSettings`.

For manual initialization, disable **Auto Initialize** and call `SmartDebug.Initialize()` before accessing `SmartDebug.Instance`. Explicit initialization works regardless of this setting; accessing `Instance` alone does not initialize the debugger when the setting is disabled. The settings asset must still be registered in Preloaded Assets.

![Register SDSettings in the Player's Preloaded Assets](docs/images/preload.png)

To try the included demo, import **SmartDebugger Sample** from the package's **Samples** section. Open its `Scenes/Demo.unity` scene and register the sample settings in Preloaded Assets.

The sample works on Unity 2022.1+. Its reporter supports screen recording only on Unity 2022.3+, with the optional InstantReplay packages installed. On earlier versions it supports logs and screenshots without recording; do not install InstantReplay there.

## Runtime variables

Implement `IFieldLayout`, register it with `SmartDebug`, and describe your controls in `OnLayout`. Multiple controls can share the same variable.

```csharp
using SmartDebugger;
using UnityEngine;

public class PlayerDebugSettings : MonoBehaviour, IFieldLayout
{
    public string Title => "Player";

    private readonly FloatVariable _speed = new(
        "Move speed",
        defaultValue: 6f,
        minValue: 0f,
        maxValue: 20f,
        serializeKey: "debug.player.move_speed");

    private SmartDebug _debugger;

    // Your gameplay code can read this value.
    public float MoveSpeed => _speed.Value;

    private void Start()
    {
        _debugger = SmartDebug.Instance;
        _debugger.AddFieldLayout(this);
    }

    public void OnLayout(FieldGroups groups)
    {
        groups.AddGroup("Movement", group =>
        {
            group.AddField(_speed);
            group.AddSlider(_speed, width: 300f);
            group.AddButton("Write test log", () => Debug.Log("Player debug action"));
        });
    }

    private void OnDestroy()
    {
        if (_debugger != null)
            _debugger.RemoveFieldLayout(this);
    }
}
```

Read `.Value` from your game code, or subscribe to `OnValueChanged` to apply edits to existing systems. Providing a `serializeKey` persists supported variable values through `PlayerPrefs`; omit it for session-only values.

| Value | Variable | Controls |
| --- | --- | --- |
| Boolean | `BoolVariable` | Toggle |
| Integer | `IntVariable` | Input field, slider |
| Float | `FloatVariable` | Input field, slider |
| Text | `TextVariable` | Input field |
| Enum | `EnumVariable<T>` | Dropdown |
| Selection | `SelectionVariable` | Dropdown |
| Action | `ActionVariable` / `AddButton` | Button |

See the [sample layouts](Assets/Samples~/Scripts/Demo.cs) for more examples. If your code uses assembly definitions, reference `Unity.SmartDebugger`.

![Runtime variables with input fields, sliders, and dropdowns](docs/images/variable.png)

## Logs and performance

Unity logs are captured while SmartDebugger is initialized, including logs received from worker threads. The log viewer supports severity and text filters, auto-scroll, stack traces, and copying entries.

**Max Log Entries** defaults to **1000**. Both retained logs and the pending receive queue have a count limit; older entries are discarded when a queue fills. Severity counters describe retained entries. Capture stops and retained data is released when the debugger is destroyed.

The profiler provides a quick view of FPS, average/max frame time, CPU timing categories, and memory usage. Available profiler counters depend on the platform and build configuration. Use Unity's Profiler for detailed analysis.

![Frame-time and memory graphs in the in-game profiler](docs/images/profiler.png)

### System information preview

![Device and runtime system information](docs/images/system.png)

## Bug reports

Built-in **Discord** and **Slack** reporters are included. Create a reporter through **Assets → Create → SmartDebugger → BugReporter**, configure its destination, and assign it to **SDSettings → Bug Reporter**.

| Reporter | Configuration |
| --- | --- |
| `DiscordBugReporter` | Webhook URL and server ID |
| `SlackBugReporter` | API token and channel ID |

Reports can include a description, logs with system information, a screenshot, and an optional video. System information is included with the **Include Logs** attachment. Testers can choose the attachments in the report tab.

For your own destination, derive from `BugReporter` and implement:

```csharp
public override void SendReport(
    string description,
    string report,
    byte[] screenshot,
    string videoPath,
    Action<ReportResult> onResult)
```

Call `onResult(ReportResult.Success(...))` or `onResult(ReportResult.Failed)` when finished. Returned URLs can be opened from the report tab. Derive from `ScreenRecordBugReporter` if your reporter should support video attachments. See [SampleBugReporter](Assets/Samples~/Scripts/SampleBugReporter.cs).

Input state is restored after failures, cancellation, and timeouts: **10 seconds** for screenshot capture, **30 seconds** for video export, and **60 seconds** for submission. Closing the tab cancels the UI wait. The callback API does not abort a custom reporter's ongoing network request; late results are ignored.

![Bug report form with description and attachment options](docs/images/bugreport.png)

### Optional screen recording

Install both packages from [InstantReplay](https://github.com/CyberAgentGameEntertainment/InstantReplay), using **v1.6.11 or newer**:

- `jp.co.cyberagent.instant-replay`
- `jp.co.cyberagent.instant-replay.dependencies`

Follow InstantReplay's setup instructions and add the **`org.nuget`** scope to your OpenUPM registry for its dependencies. Recording controls appear when the integration is available. Configure recording options on a reporter derived from `ScreenRecordBugReporter`.

The integration lives in `Unity.SmartDebugger.InstantReplay`. Add that assembly reference if your own code uses `ScreenRecorder` directly. Define `EXCLUDE_INSTANTREPLAY` to disable the integration while keeping the rest of SmartDebugger enabled.

## Configuration

| `SDSettings` option | Purpose |
| --- | --- |
| **Auto Initialize** | Initialize automatically after the first scene loads. |
| **Dont Destroy On Load** | Keep the debugger across scene changes. |
| **Auto Generate EventSystem** | Create an EventSystem when none exists. |
| **Show Error Indicator** | Show an error indication while the menu is closed. |
| **Max Log Entries** | Limit retained and pending logs; default `1000`. |
| **Open / Close Shortcut** | Configure keyboard shortcuts. |
| **Open Tap Event** | Configure the touch gesture. |
| **Main Tab Contents** | Choose the menu's tabs. |
| **Canvas Sorting Order** | Set the overlay's canvas order; default `100`. |
| **Bug Reporter** | Choose the report destination. |

### Settings Inspector preview

![SDSettings Inspector](docs/images/setting.png)

### Input backends

Legacy input works without Input System. When Input System is installed and enabled in **Player Settings → Active Input Handling**, `Unity.SmartDebugger.InputSystem` is enabled automatically. With **Both** selected, SmartDebugger uses Input System.

### Release builds

Define `DISABLE_SMART_DEBUG` in **Player Settings → Scripting Define Symbols**, or through your build configuration. This excludes the core and optional integration assemblies, so the debugger's UI and event detectors are not created.

Guard your application's SmartDebugger references with `#if !DISABLE_SMART_DEBUG`, or place the integration code in an assembly with a `!DISABLE_SMART_DEBUG` define constraint. This also applies to the runtime-variable example above.

## Troubleshooting

| Symptom | Check |
| --- | --- |
| `SDSettings is not preloaded` | Add your settings asset to Player → Other Settings → Preloaded Assets. |
| `SmartDebug is not initialized` | With Auto Initialize disabled, call `SmartDebug.Initialize()` before accessing `SmartDebug.Instance`. |
| Touch gesture does not open the menu | Check the configured touch/tap counts and whether an SRP debug UI is intercepting the gesture. |
| Recording controls are hidden | Check both InstantReplay packages, their dependencies, and `EXCLUDE_INSTANTREPLAY`. |
| Build errors after disabling SmartDebugger | Guard your own references to its types or exclude their assembly with the same define constraint. |

If the SRP runtime debug UI conflicts with your gesture, disable it during initialization in projects that use that UI:

```csharp
UnityEngine.Rendering.DebugManager.instance.enableRuntimeUI = false;
```

## Development and verification

The repository is a Unity development project; the distributable UPM package is under `Assets/`. Regression tests live in `Assets/Tests/Editor` and cover log retention/disposal, variable initialization, and report submission/UI recovery.

The cloned development project defaults to Unity 2022.1 without InstantReplay. Package dependencies in `Packages/manifest.json` are not conditional on the Editor version: excluding SmartDebugger's integration assembly does not exclude InstantReplay's own code. To test recording, open the project in Unity 2022.3+ and install the two packages listed under [Optional screen recording](#optional-screen-recording). Remove both packages in Package Manager before reopening the project in Unity 2022.1; Unity will update `packages-lock.json` when dependencies are resolved.

Create a fresh, isolated UPM consumer to verify installation and run tests:

```shell
python Tools/prepare_consumer.py --output .utmp/consumer --unity-version 6000.5.1f1 --variant minimal
unity test .utmp/consumer --mode EditMode --output Logs/consumer-results.xml
```

Use an installed Editor version. The generator refuses to overwrite an existing project. Available variants are `minimal`, `input-system`, `recording`, and `disabled`; `disabled` includes optional dependencies and verifies that all debugger runtime assemblies are excluded.

Run consumer checks locally for each Unity version and configuration you want to support. Automated test CI is not configured in this repository.

## License

[MIT](LICENSE). Third-party dependencies retain their own licenses.
