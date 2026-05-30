using AudioMate.Core.Music;
using Microsoft.Win32;
using System.Diagnostics;
using System.Security;

namespace AudioMate.App.Settings;

internal static class SystemApplicationScanner
{
    private const int MaxFolderDepth = 4;

    public static IReadOnlyList<string> ScanProcessNames()
    {
        var processNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        AddRunningProcesses(processNames);
        AddRegistryAppPaths(processNames);
        AddCommonInstallFolders(processNames);

        return ProcessNameNormalizer.NormalizeDistinct(processNames)
            .Where(static processName => processName is not "system" and not "idle")
            .ToArray();
    }

    private static void AddRunningProcesses(HashSet<string> processNames)
    {
        foreach (var process in Process.GetProcesses())
        {
            using (process)
            {
                AddProcessName(processNames, process.ProcessName);
            }
        }
    }

    private static void AddRegistryAppPaths(HashSet<string> processNames)
    {
        AddAppPathsFromRoot(Registry.CurrentUser);
        AddAppPathsFromRoot(Registry.LocalMachine);

        void AddAppPathsFromRoot(RegistryKey root)
        {
            try
            {
                using var appPaths = root.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\App Paths");
                if (appPaths is null)
                {
                    return;
                }

                foreach (var subKeyName in appPaths.GetSubKeyNames())
                {
                    AddProcessName(processNames, subKeyName);
                    using var subKey = appPaths.OpenSubKey(subKeyName);
                    AddExecutablePath(processNames, subKey?.GetValue(null) as string);
                }
            }
            catch (SecurityException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }

    private static void AddCommonInstallFolders(HashSet<string> processNames)
    {
        foreach (var folder in GetCommonInstallFolders())
        {
            AddExecutablesFromFolder(processNames, folder, MaxFolderDepth);
        }
    }

    private static IEnumerable<string> GetCommonInstallFolders()
    {
        var folders = new[]
        {
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Programs"),
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Spotify"),
        };

        return folders
            .Where(static folder => !string.IsNullOrWhiteSpace(folder))
            .Distinct(StringComparer.OrdinalIgnoreCase);
    }

    private static void AddExecutablesFromFolder(HashSet<string> processNames, string folder, int maxDepth)
    {
        if (!Directory.Exists(folder))
        {
            return;
        }

        var pending = new Stack<(string Folder, int Depth)>();
        pending.Push((folder, 0));

        while (pending.Count > 0)
        {
            var (currentFolder, depth) = pending.Pop();

            foreach (var filePath in SafeEnumerateFiles(currentFolder, "*.exe"))
            {
                AddExecutablePath(processNames, filePath);
            }

            if (depth >= maxDepth)
            {
                continue;
            }

            foreach (var childFolder in SafeEnumerateDirectories(currentFolder))
            {
                pending.Push((childFolder, depth + 1));
            }
        }
    }

    private static IEnumerable<string> SafeEnumerateFiles(string folder, string searchPattern)
    {
        try
        {
            return Directory.EnumerateFiles(folder, searchPattern).ToArray();
        }
        catch (UnauthorizedAccessException)
        {
            return [];
        }
        catch (IOException)
        {
            return [];
        }
        catch (SecurityException)
        {
            return [];
        }
    }

    private static IEnumerable<string> SafeEnumerateDirectories(string folder)
    {
        try
        {
            return Directory.EnumerateDirectories(folder).ToArray();
        }
        catch (UnauthorizedAccessException)
        {
            return [];
        }
        catch (IOException)
        {
            return [];
        }
        catch (SecurityException)
        {
            return [];
        }
    }

    private static void AddExecutablePath(HashSet<string> processNames, string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        var trimmed = path.Trim().Trim('"');
        var executableIndex = trimmed.IndexOf(".exe", StringComparison.OrdinalIgnoreCase);
        if (executableIndex >= 0)
        {
            trimmed = trimmed[..(executableIndex + ".exe".Length)];
        }

        AddProcessName(processNames, trimmed);
    }

    private static void AddProcessName(HashSet<string> processNames, string? processName)
    {
        if (string.IsNullOrWhiteSpace(processName))
        {
            return;
        }

        processNames.Add(ProcessNameNormalizer.Normalize(processName));
    }
}
