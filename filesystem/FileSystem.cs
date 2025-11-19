using System;
using System.Collections.Generic;
using System.IO;
using Sys = Cosmos.System;

namespace OSaft.filesystem {
    public static class FileSystem
    {
        public static readonly Sys.FileSystem.CosmosVFS Instance = new();
        private static string currentPath = "0:\\";
        public static string CurrentPath
        {
            get => currentPath;
            set => currentPath = ValidatePath(value, currentPath);
        }

        private static bool IsRoot(string path) {
            if (path.Contains(":\\"))
            {
                DirectoryInfo info = new(path);
                if (info.Parent == null) { return true; }
            }
            return false;
        }

        internal static string ValidatePath(string newPath, string currentPath)
        {
            if (string.IsNullOrWhiteSpace(currentPath))
            {
                Console.WriteLine("Current Path cannot be null or empty");
                return "0:\\"; 
            }
            if (string.IsNullOrWhiteSpace(newPath))
            {
                Console.WriteLine("Path cannot be null or empty");
                return currentPath;
            }

            currentPath = currentPath.Replace('/', '\\');
            newPath = newPath.Replace('/', '\\');

            if (newPath.Contains(":\\"))
            {
                if (IsRoot(newPath))
                {
                    return newPath;
                }
                return NormalizeAbsolutePath(newPath);
            }

            List<string> parts = new(currentPath.Split('\\', StringSplitOptions.RemoveEmptyEntries));

            string drivePrefix = "";
            if (parts.Count > 0 && parts[0].Contains(':'))
            {
                drivePrefix = parts[0];
                parts.RemoveAt(0);
            }

            foreach (string segment in newPath.Split('\\', StringSplitOptions.RemoveEmptyEntries))
            {
                if (segment == ".")
                    continue;
                else if (segment == "..")
                {
                    if (parts.Count > 0)
                        parts.RemoveAt(parts.Count - 1);
                }
                else
                {
                    parts.Add(segment);
                }
            }

            string result = drivePrefix + "\\" + string.Join("\\", parts.ToArray());
            if (result.EndsWith(":\\") == false && result.EndsWith("\\"))
                result = result.TrimEnd('\\');

            if (!Directory.Exists(result))
            {
                Console.WriteLine("Directory does not exist! / File already exists. (" + result + ")");
                return currentPath;
            }

            if (!result.EndsWith("\\")) { result += "\\"; }

            return result;
        }

        internal static string ValidatePath(string path)
        {
            if (Path.IsPathRooted(path)) // absolute path
            {
                return ValidatePath(path, currentPath);
            }
            else // relative path
            {
                return ValidatePath(currentPath + (path ?? ""), currentPath);
            }
        }

        private static string NormalizeAbsolutePath(string path)
        {
            List<string> parts = new(path.Split("\\", StringSplitOptions.RemoveEmptyEntries));

            string drivePrefix = parts[0];
            parts.RemoveAt(0);

            List<string> finalParts = new();

            foreach (string part in parts)
            {
                if (part == ".")
                    continue;
                else if (part == "..")
                {
                    if (finalParts.Count > 0)
                        finalParts.RemoveAt(finalParts.Count - 1);
                }
                else
                {
                    finalParts.Add(part);
                }
            }

            return drivePrefix + "\\" + string.Join("\\", finalParts.ToArray());
        }

        public static FileStream CreateFile(string path, bool isPathAbs = false) {

            if (path.Contains('.'))
            {
                string filename = Path.GetFileName(path);
                path = ValidatePath(Path.GetDirectoryName(path));
                try
                {
                    path = path + "\\" + filename;
                    if (!File.Exists(path))
                    {
                        return File.Create(path);
                    }
                    else
                    {
                        Console.WriteLine("CreateFile: " + path + " already exists or is not a file");
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine("CreateFile: Exception thrown: " + e.Message);
                }
            }
            else
            {
                Console.WriteLine("CreateFile: You cannot create file without an extension.");
            }
            return null;
        }


        public static DirectoryInfo CreateDirectory(string path) {
            path = ValidatePath(path);
            try
            {
                return Directory.CreateDirectory(path);
            }
            catch (Exception e)
            {
                Console.WriteLine("CreateDirectory: Exception thrown: " + e.Message);
                return null;
            }
        }

        public static bool DeleteDirectory(string path)
        {
            path = ValidatePath(path);
            if (!Directory.Exists(path))
            {
                Console.WriteLine("DeleteDirectory: Directory \"" + path + "\" not found.");
                return false;
            }
            try
            {
                Directory.Delete(path);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("DeleteDirectory: Exception thrown: " + e.Message);
                return false;
            }
        }

        public static bool DeleteFile(string path)
        {
            path = ValidatePath(path);
            if (!File.Exists(path))
            {
                Console.WriteLine("DeleteFile: File \"" + path + "\" not found");
                return false;
            }

            try
            {
                File.Delete(path);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("DeleteFile: Exception thrown: " + e.Message);
                return false;
            }
        }

        public static void WriteFile(string path, string content) {
            path = ValidatePath(path);
            try
            {
               File.WriteAllText(path, content);
            } catch (Exception e) 
            {
                Console.WriteLine("WriteFile: Exception thrown: " + e.Message);
            }
        }

        public static void WriteFile(string path, string[] content)
        {
            path = ValidatePath(path);
            foreach (string s in content)
            {
                WriteFile(path, s);
            }
        }

        public static string ReadFile(string path) {
            path = ValidatePath(path);
            try
            {
                return File.ReadAllText(path);
            }
            catch (Exception e)
            {
                Console.WriteLine("ReadFile: Exception thrown: " + e.Message);
                return null;
            }
        }

        public static string[] FindFile(string filename, string path)
        {
            if (string.IsNullOrEmpty(filename)) { return Array.Empty<string>(); }
            path ??= currentPath;
            path = ValidatePath(path);
            List<string> paths = new();

            try
            {
                foreach (string file in Directory.GetFiles(path))
                {
                    if (filename == file)
                    {
                        paths.Add(ValidatePath(path + "\\" + file, currentPath));
                    }
                }

                string[] directories = Directory.GetDirectories(path);
                foreach (string dir in directories)
                {
                    paths.AddRange(FindFile(filename, path + "\\" + dir));
                }
            } catch(Exception e)
            {
                Console.WriteLine("Error: Accessing path: " + path + " (" + (string.IsNullOrEmpty(e.Message) ? "IO Error" : e.Message) + ")");
            }

            return paths.ToArray();
        }

        public static List<string> GetFileSuggestions()
        {
            return new List<string>();
        }
    }
}
