# SquintLite

A lightweight, always-on-top contrast checker for Windows and macOS. Built for graphic designers and artists who need quick WCAG contrast compliance checks without opening a browser or while offline.

## Features

- Live contrast ratio calculation per WCAG 2.1
- Pass/fail results for all three WCAG criteria
  - Normal Text (AA and AAA)
  - Large Text (AA and AAA)
  - Graphical Objects and UI Components (AA)
- Foreground alpha support with correct luminance blending
- Colour picker with spectrum and component views
- Screen eyedropper for sampling colours from anywhere on screen (Windows)
- Always-on-top floating widget that stays visible over your work

## Download

Head to the [latest release](https://github.com/Interactive-63/squint-lite/releases/latest) to download SquintLite for Windows or macOS.

| Platform | Download |
|---|---|
| Windows | `SquintLite-1.0.0-Windows-x64.exe` |
| macOS (Universal) | `SquintLite-1.0.0-macOS-Universal.dmg` |

## macOS Installation

SquintLite is not notarised with Apple. macOS may show a security warning on first launch. This only needs to be done once.

**"SquintLite is damaged and can't be opened"**

Open Terminal (Command + Space, type Terminal) and run:

```bash
xattr -cr /Applications/SquintLite.app
```

Then open the app again.

**"SquintLite cannot be opened because the developer cannot be verified"**

Click Cancel on the dialog, then open System Settings and go to Privacy and Security. Scroll down to find the SquintLite blocked message and click Open Anyway. Confirm when prompted.

## Built With

- [Avalonia UI](https://avaloniaui.net) - cross-platform .NET UI framework
- [.NET 10](https://dotnet.microsoft.com) - runtime
- [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) - MVVM source generators
- [FluentIcons.Avalonia](https://github.com/davidxuang/FluentIcons) - Fluent icon set


---

Made by [Interactive 63](https://www.interactive63.com)
