## SteamShortcut
 
[![GitHub release (latest SemVer)](https://img.shields.io/github/v/release/mops1k/SteamShortcut?label=stable&style=flat-square)](https://github.com/mops1k/SteamShortcut/releases/latest)
![GitHub all releases](https://img.shields.io/github/downloads/mops1k/SteamShortcut/total?style=flat-square)

It's a simple Windows application, which adds context menu item on executable apps to add it to Steam Library

This is a fork of [mops1k/SteamShortcut](https://github.com/mops1k/SteamShortcut) with the following changes:

- Added `.lnk` shortcut support — the context menu now also appears on shortcut files, not just `.exe` files, and the shortcut's target executable is resolved automatically.
- `shortcuts.vdf` is now created automatically if it doesn't exist yet, instead of failing with an error and requiring you to add a non-Steam game through Steam's own UI first.
- Fixed a bug where added games were silently written with the wrong field name casing (`appName`/`exe` instead of `AppName`/`Exe`) and never actually showed up in the Steam library, even though the app reported success.

## Requirements

- [Steam Client for Windows](https://store.steampowered.com/about/)
- [.NET Desktop Runtime 9](https://dotnet.microsoft.com/ru-ru/download/dotnet/thank-you/runtime-desktop-9.0.3-windows-x64-installer)

## Usage

- First application run will ask you to add windows context menu item to executable apps. (Right mouse click menu on executable application)
- With second run will ask you to remove context menu item

1. Start application
2. Click "OK"
3. Choose shortcut or exe file you want to add to Steam library
4. Make mouse right click on it
5. Choose "Add to Steam"
6. If steam running it will ask you to restart it (you can restart it later manually just pressing "Cancel")
7. If steam not running, and you have more than one steam user it asks you to which user library select to add shortcut. In next steam running you will see application in library.

## Authors
- Aleksandr "mops1k" Kvintilyanov, 2025

SteamShortcut is not affiliated with Valve, Steam, or any of their partners.

## Support
If you like the project, you can support it thru:
- [Boosty](https://boosty.to/mops1k/donate)
