using Logger;
using SteamShortcut.Service;

namespace SteamShortcut.Controller;

public class ShortcutController(Service.SteamShortcut steamShortcut, SteamProcess steamProcess, ILogger logger)
    : IController
{
    public void Invoke(params object[]? args)
    {
        if (args?.FirstOrDefault() is not string targetPath)
        {
            return;
        }

        if (!Path.Exists(targetPath))
        {
            logger.Error($"Cannot find executable path: {targetPath}");
            return;
        }

        string? exePath = targetPath;
        if (Path.GetExtension(targetPath).Equals(".lnk", StringComparison.OrdinalIgnoreCase))
        {
            exePath = ShortcutLinkResolver.ResolveTarget(targetPath);
            if (string.IsNullOrEmpty(exePath) || !Path.Exists(exePath))
            {
                logger.Error($"Could not resolve shortcut target: {targetPath}");
                return;
            }
        }

        if (!steamShortcut.InitialisePaths())
        {
            return;
        }

        if (!steamShortcut.Add(exePath))
        {
            return;
        }

        if (!steamProcess.IsRunning)
        {
            return;
        }

        DialogResult result = MessageBox.Show(
            Localization.SteamShortcut_RestartSteamQuestion,
            Localization.SteamShortcut_RestartSteamCaption,
            MessageBoxButtons.OKCancel,
            MessageBoxIcon.Information
        );

        if (result == DialogResult.Cancel)
        {
            return;
        }

        if (!steamProcess.Restart())
        {
            logger.Error($"Failed to restart Steam process: {exePath}");
            MessageBox.Show(Localization.ShortcutController_Invoke_Failed_to_restart_Steam_process,
                Localization.SteamShortcut_RestartSteamCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
