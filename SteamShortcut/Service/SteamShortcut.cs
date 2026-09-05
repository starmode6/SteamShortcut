using System.Text;
using Logger;
using SteamShortcut.Form;
using SteamShortcut.Model;
using VDFMapper.ShortcutConfig;
using VDFMapper.ShortcutMap;
using VDFMapper.VDF;

namespace SteamShortcut.Service;

public class SteamShortcut(ILogger logger, SteamUserDialog userDialog)
{
    private VDFMap? _root;
    private string _vdfPath = "";
    private ILogger Logger => logger;
    private ShortcutRoot? ShortcutRoot { get; set; }

    public bool InitialisePaths()
    {
        string? path = SteamShortcutPath.GetUserDataPath();
        if (string.IsNullOrEmpty(path))
        {
            return false;
        }

        int? userId = SteamShortcutPath.GetUserId();
        if (SteamShortcutPath.GetUserId() == null)
        {
            List<SteamUser>? users = GetUsers(path);
            if (users is { Count: 1 })
            {
                userId = users.First().Id;
            }

            if (userId == null)
            {
                SteamUser? steamUser = userDialog.Show(GetUsers(path));
                if (steamUser == null)
                {
                    return false;
                }

                userId = steamUser.Id;
            }
        }

        if (userId == null)
        {
            Error(Localization.SteamShortcut_PathsNotInitialized);

            return false;
        }

        _vdfPath = SteamShortcutPath.GetShortcutsPath((int)userId);

        EnsureShortcutsFileExists();

        return true;
    }

    private void EnsureShortcutsFileExists()
    {
        if (File.Exists(_vdfPath))
        {
            return;
        }

        string? directory = Path.GetDirectoryName(_vdfPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using BinaryWriter writer = new(new FileStream(_vdfPath, FileMode.Create));
        writer.Write((byte)VDFType.MapStart);
        writer.Write(Encoding.UTF8.GetBytes("shortcuts"));
        writer.Write((byte)0);
        writer.Write((byte)VDFType.MapEnd);
        writer.Write((byte)VDFType.MapEnd);
    }

    public bool Add(string path)
    {
        if (!Read())
        {
            Error(Localization.VDF_ReadError);

            return false;
        }

        if (!AddExecutable(path, Path.GetFileNameWithoutExtension(path)))
        {
            Error(Localization.VDF_AddExecutableError);

            return false;
        }

        if (Write())
        {
            return true;
        }

        Error(Localization.VDF_WriteError);

        return false;
    }

    private bool Read()
    {
        if (!File.Exists(_vdfPath))
        {
            return false;
        }

        VDFStream stream = new(_vdfPath);
        _root = new VDFMap(stream);
        ShortcutRoot = new ShortcutRoot(_root);
        stream.Close();

        return true;
    }

    private bool Write()
    {
        try
        {
            File.WriteAllText(_vdfPath, "");
            BinaryWriter writer = new(new FileStream(_vdfPath, FileMode.OpenOrCreate));
            _root!.Write(writer, null);
            writer.Close();

            return true;
        }
        catch (Exception? e)
        {
            Logger.Fatal("Write shortcut to Steam library error!", e);

            return false;
        }
    }

    private bool AddExecutable(string? path, string? name)
    {
        if (!File.Exists(path))
        {
            return false;
        }

        ShortcutEntry? entry = null;

        if (ShortcutRoot != null)
        {
            for (int i = 0; i < ShortcutRoot.GetSize(); i++)
            {
                ShortcutEntry? existingEntry = ShortcutRoot!.GetEntry(i);
                if (existingEntry?.AppName != name)
                {
                    continue;
                }

                entry = existingEntry;

                break;
            }
        }

        if (entry != null)
        {
            Logger.Info($"Updating entry {name}");
        }
        else
        {
            Logger.Info($"Adding {name}");
            entry = ShortcutRoot!.AddEntry();
        }

        if (entry == null)
        {
            return false;
        }

        entry.AppName = name;
        entry.StartDir = Path.GetDirectoryName(path);
        entry.Exe = path;

        if (entry.Exe.Contains(' '))
        {
            entry.Exe = $"\"{entry.Exe}\"";
        }

        if (entry.StartDir != null && entry.StartDir.Contains(' '))
        {
            entry.StartDir = $"\"{entry.StartDir}\"";
        }

        entry.AppId = ShortcutEntry.GenerateSteamGridAppId(entry.AppName, entry.Exe);

        return true;
    }

    private static List<SteamUser>? GetUsers(string path)
    {
        List<DirectoryInfo> directories = new DirectoryInfo(path).GetDirectories().ToList();
        if (directories.Count == 0)
        {
            return null;
        }

        List<SteamUser> users = new();
        foreach (DirectoryInfo dir in directories)
        {
            string localConfigPath = Path.Combine(dir.FullName, "config", "localconfig.vdf");
            if (File.Exists(localConfigPath))
            {
                string? username = GetUsernameFromLocalConfig(localConfigPath);
                users.Add(new SteamUser(int.Parse(dir.Name), username));

                return users;
            }

            users.Add(new SteamUser(int.Parse(dir.Name), Localization.SteamUserDialog_UnknownUser));
        }

        return users;
    }

    private static string? GetUsernameFromLocalConfig(string localConfigPath)
    {
        try
        {
            string[] lines = File.ReadAllLines(localConfigPath);
            foreach (string line in lines)
            {
                if (!line.Contains("PersonaName"))
                {
                    continue;
                }

                string?[] parts = line.Split(['\t', '"'], StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    return parts[1];
                }
            }
        }
        catch
        {
            return null;
        }

        return null;
    }

    private void Error(string message)
    {
        Logger.Error(message);
        MessageBox.Show(message, Localization.SteamShortcut_Add_Steam_Shortcut_Error, MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }
}
