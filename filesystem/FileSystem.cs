using System;
using System.Collections.Generic;
using System.IO;
using Sys = Cosmos.System;

namespace OSaft.filesystem {
    public static class FileSystem
    {
        public static readonly Sys.FileSystem.CosmosVFS Instance = new();
        private static string currentPath = "0:\\";
        private static string ValidatePath(string inputPath)
        {
            if (string.IsNullOrWhiteSpace(inputPath)) { return "0:\\"; }
            
            inputPath = inputPath.Trim().Replace('/', '\\');

            if (!inputPath.Contains(":\\"))
            {
                inputPath = inputPath.TrimStart('\\');
                inputPath = currentPath + inputPath;
            }

            string normalized = Path.GetFullPath(inputPath);

            return normalized;
        }

        public static FileStream CreateFile(string path) {
            path = ValidatePath(currentPath + path ?? "");
            try
            {
                if (!File.Exists(path) && path.Contains('.'))
                {
                    return File.Create(path);
                } else
                {
                    Console.WriteLine("CreateFile: " + path + " already exists or is not a file");
                }
                
            } catch(Exception e)
            {
                Console.WriteLine("CreateFile: Exception thrown: " + e.Message);
            }
            return null;
        }


        public static DirectoryInfo CreateDirectory(string path) {
            path = ValidatePath(currentPath + (path ?? ""));
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

        public static void DeleteDirectory(string path)
        {
            path = ValidatePath(currentPath + (path ?? ""));
            try
            {
                Directory.Delete(path);
            }
            catch (Exception e)
            {
                Console.WriteLine("DeleteFileOrDirectory: Exception thrown: " + e.Message);
            }
        }

        public static void DeleteFile(string path)
        {
            path = ValidatePath(currentPath + (path ?? ""));
            try
            {
                File.Delete(path);
            }
            catch (Exception e)
            {
                Console.WriteLine("DeleteFileOrDirectory: Exception thrown: " + e.Message);
            }
        }

        public static void WriteFile(string path, string content) {
            path = ValidatePath(currentPath + (path ?? ""));
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
            path = ValidatePath(currentPath + (path ?? ""));
            foreach (string s in content)
            {
                WriteFile(path, s);
            }
        }

        public static string ReadFile(string path) {
            path = ValidatePath(currentPath + (path ?? ""));
            try
            {
                return File.ReadAllText(path);
            } catch (Exception e)
            {
                Console.WriteLine("ReadFile: Exception thrown: " + e.Message);
                return null;
            }
        }

        public static string[] FindFile(string filename, string path)
        {
            path = ValidatePath(currentPath + (path ?? ""));
            List<string> paths = new List<string>();

            try
            {
                foreach (string file in Directory.GetFiles(path))
                {
                    if (filename == file)
                    {
                        paths.Add(path + "\\" + file);
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

    }
}
