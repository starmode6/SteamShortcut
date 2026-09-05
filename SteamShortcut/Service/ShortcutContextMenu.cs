using Logger;
using System.Linq;

namespace SteamShortcut.Service;

public class ShortcutContextMenu(ILogger? logger = null)
{
    private static readonly string[] TargetFileTypes = ["exefile", "lnkfile"];
    private readonly string _menuName = Localization.ContextMenu_Name;
    private ILogger _logger => logger ?? new Logger.Logger("SteamShortcut", "SteamShortcut");
    private WindowsContextMenu.WindowsContextMenu WinContextMenu => new(_logger);

    private string? ExeFullPath
    {
        get
        {
            string exePath = AppContext.BaseDirectory;

            return Path.Combine(Path.GetDirectoryName(exePath) ?? ".", "SteamShortcut.exe");
        }
    }

    public bool IsExists() => TargetFileTypes.All(fileType => WinContextMenu.IsContextMenuExists(fileType, _menuName));

    public void Add()
    {
        if (ExeFullPath == null)
        {
            throw new FileNotFoundException("Could not find the SteamShortcut executable.");
        }

        if (!File.Exists(ExeFullPath))
        {
            throw new FileNotFoundException($"SteamShortcut executable not found at: {ExeFullPath}");
        }

        foreach (string fileType in TargetFileTypes)
        {
            WinContextMenu.AddContextMenu(fileType, _menuName, $"{ExeFullPath} \"%1\"", $"\"{ExeFullPath}\",0");
        }
    }

    public void Remove()
    {
        foreach (string fileType in TargetFileTypes)
        {
            WinContextMenu.RemoveContextMenu(fileType, _menuName);
        }
    }
}
