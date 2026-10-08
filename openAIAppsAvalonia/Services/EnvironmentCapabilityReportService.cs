using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace openAiAppsAvalonia.Services
{
    /// <summary>
    /// Collects a lightweight, read-only snapshot of the host and workspace.
    /// This class is UI-framework independent so it can be reused during the Avalonia migration.
    /// </summary>
    public sealed class EnvironmentCapabilityReportService
    {
        private static readonly string[] ExcludedDirectoryNames =
        {
            ".git", ".vs", ".idea", "bin", "obj", "node_modules", "packages", "target", "venv", ".venv"
        };

        private static readonly string[] CandidateTools =
        {
            "dotnet", "cargo", "rustc", "python", "python3", "pip", "pip3",
            "cmake", "ninja", "make", "gcc", "g++", "clang", "clang++",
            "qmake", "qtpaths", "node", "npm", "git"
        };

        public string CreateReport(string workspaceRoot)
        {
            var report = new StringBuilder();
            report.AppendLine("Environment capabilities report");
            report.AppendLine("Generated: " + DateTimeOffset.Now.ToString("u"));
            report.AppendLine();
            report.AppendLine("Host");
            report.AppendLine("- Operating system: " + RuntimeInformation.OSDescription);
            report.AppendLine("- OS platform: " + GetPlatformName());
            report.AppendLine("- Architecture: " + RuntimeInformation.OSArchitecture);
            report.AppendLine("- Application runtime: .NET " + Environment.Version);
            report.AppendLine();
            report.AppendLine("Detected tools (available on PATH)");

            foreach (string tool in CandidateTools)
            {
                string location = FindTool(tool);
                report.AppendLine("- " + tool + ": " + (location ?? "not found"));
            }

            report.AppendLine();
            report.AppendLine("Workspace project files");

            string root = (workspaceRoot ?? string.Empty).Trim();
            if (root.Length == 0)
            {
                report.AppendLine("- No workspace root is set.");
            }
            else if (!Directory.Exists(root))
            {
                report.AppendLine("- The configured workspace path does not exist or is not accessible.");
            }
            else
            {
                report.AppendLine("- Root: " + Path.GetFullPath(root));
                IReadOnlyList<string> projectFiles = FindProjectFiles(root, 10000);
                if (projectFiles.Count == 0)
                {
                    report.AppendLine("- No recognized project or solution files found (scan limited to 10,000 files).");
                }
                else
                {
                    foreach (string projectFile in projectFiles)
                        report.AppendLine("- " + projectFile);

                    report.AppendLine("- Likely project ecosystems: " + string.Join(", ", IdentifyProjectTypes(projectFiles)));
                }
            }

            report.AppendLine();
            report.AppendLine("This is an informational snapshot. Tool availability is detected from PATH; no commands were run.");
            report.AppendLine("Project type detection is based on filenames and may not identify every project in a workspace.");
            return report.ToString();
        }

        private static string GetPlatformName()
        {
            if (OperatingSystem.IsWindows()) return "Windows";
            if (OperatingSystem.IsLinux()) return "Linux";
            if (OperatingSystem.IsMacOS()) return "macOS";
            return "Other";
        }

        private static string FindTool(string toolName)
        {
            string path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
            string[] directories = path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);
            IEnumerable<string> extensions = GetExecutableExtensions();

            foreach (string directory in directories)
            {
                foreach (string extension in extensions)
                {
                    try
                    {
                        string candidate = Path.Combine(directory.Trim(), toolName + extension);
                        if (File.Exists(candidate))
                            return candidate;
                    }
                    catch (ArgumentException) { }
                    catch (NotSupportedException) { }
                    catch (PathTooLongException) { }
                }
            }

            return null;
        }

        private static IEnumerable<string> GetExecutableExtensions()
        {
            if (!OperatingSystem.IsWindows())
                return new[] { string.Empty };

            string pathExt = Environment.GetEnvironmentVariable("PATHEXT");
            if (string.IsNullOrWhiteSpace(pathExt))
                pathExt = ".COM;.EXE;.BAT;.CMD";

            return new[] { string.Empty }.Concat(pathExt.Split(';').Where(x => !string.IsNullOrWhiteSpace(x)));
        }

        private static IReadOnlyList<string> FindProjectFiles(string root, int maximumFiles)
        {
            var found = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
            var pending = new Stack<(string Directory, int Depth)>();
            pending.Push((root, 0));
            int visitedFiles = 0;

            while (pending.Count > 0 && visitedFiles < maximumFiles)
            {
                var current = pending.Pop();
                try
                {
                    foreach (string file in Directory.EnumerateFiles(current.Directory))
                    {
                        visitedFiles++;
                        string name = Path.GetFileName(file);
                        if (IsProjectFile(name))
                            found.Add(Path.GetRelativePath(root, file));
                        if (visitedFiles >= maximumFiles) break;
                    }

                    if (current.Depth >= 6 || visitedFiles >= maximumFiles)
                        continue;

                    foreach (string directory in Directory.EnumerateDirectories(current.Directory))
                    {
                        string name = Path.GetFileName(directory);
                        if (!ExcludedDirectoryNames.Contains(name, StringComparer.OrdinalIgnoreCase))
                            pending.Push((directory, current.Depth + 1));
                    }
                }
                catch (UnauthorizedAccessException) { }
                catch (IOException) { }
            }

            return found.Take(100).ToArray();
        }

        private static bool IsProjectFile(string name)
        {
            string extension = Path.GetExtension(name);
            return name.Equals("CMakeLists.txt", StringComparison.OrdinalIgnoreCase)
                || name.Equals("Cargo.toml", StringComparison.OrdinalIgnoreCase)
                || name.Equals("pyproject.toml", StringComparison.OrdinalIgnoreCase)
                || name.Equals("requirements.txt", StringComparison.OrdinalIgnoreCase)
                || name.Equals("Pipfile", StringComparison.OrdinalIgnoreCase)
                || name.Equals("package.json", StringComparison.OrdinalIgnoreCase)
                || extension.Equals(".sln", StringComparison.OrdinalIgnoreCase)
                || extension.Equals(".slnx", StringComparison.OrdinalIgnoreCase)
                || extension.Equals(".csproj", StringComparison.OrdinalIgnoreCase)
                || extension.Equals(".fsproj", StringComparison.OrdinalIgnoreCase)
                || extension.Equals(".vbproj", StringComparison.OrdinalIgnoreCase)
                || extension.Equals(".vcxproj", StringComparison.OrdinalIgnoreCase)
                || extension.Equals(".pro", StringComparison.OrdinalIgnoreCase)
                || extension.Equals(".pro.user", StringComparison.OrdinalIgnoreCase);
        }

        private static IReadOnlyList<string> IdentifyProjectTypes(IReadOnlyList<string> files)
        {
            var types = new List<string>();
            if (files.Any(IsDotNetProjectFile)) types.Add(".NET");
            if (files.Any(x => Path.GetFileName(x).Equals("Cargo.toml", StringComparison.OrdinalIgnoreCase))) types.Add("Rust");
            if (files.Any(x => new[] { "pyproject.toml", "requirements.txt", "Pipfile" }.Contains(Path.GetFileName(x), StringComparer.OrdinalIgnoreCase))) types.Add("Python");
            if (files.Any(x => Path.GetFileName(x).Equals("CMakeLists.txt", StringComparison.OrdinalIgnoreCase) || Path.GetExtension(x).Equals(".vcxproj", StringComparison.OrdinalIgnoreCase))) types.Add("C/C++ (CMake or Visual C++)");
            if (files.Any(x => Path.GetExtension(x).Equals(".pro", StringComparison.OrdinalIgnoreCase))) types.Add("Qt qmake (possible)");
            if (files.Any(x => Path.GetFileName(x).Equals("package.json", StringComparison.OrdinalIgnoreCase))) types.Add("Node.js");
            return types.Count == 0 ? new[] { "unknown or unrecognized" } : types;
        }

        private static bool IsDotNetProjectFile(string file)
        {
            string name = Path.GetFileName(file);
            string extension = Path.GetExtension(name);
            return extension.Equals(".sln", StringComparison.OrdinalIgnoreCase)
                || extension.Equals(".slnx", StringComparison.OrdinalIgnoreCase)
                || extension.Equals(".csproj", StringComparison.OrdinalIgnoreCase)
                || extension.Equals(".fsproj", StringComparison.OrdinalIgnoreCase)
                || extension.Equals(".vbproj", StringComparison.OrdinalIgnoreCase);
        }
    }
}
