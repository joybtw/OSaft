using System;
using System.Collections.Generic;
using System.IO;
using IO = System.IO;
using Sys = Cosmos.System;

namespace OSaft.filesystem {

    public static class FileSystem
    {
        public static readonly Sys.FileSystem.CosmosVFS Instance = new();

        public static IO.FileStream CreateFile(string path) {
            try
            {
                if (!IO.File.Exists(path) && path.Contains('.'))
                {
                    return IO.File.Create(@"0:\" + path);
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


        public static IO.DirectoryInfo CreateDirectory(string path) {
            try
            {
                return IO.Directory.CreateDirectory(@"0:\" + path);
            }
            catch (Exception e)
            {
                Console.WriteLine("CreateDirectory: Exception thrown: " + e.Message);
                return null;
            }
        }

        public static void DeleteDirectory(string path)
        {
            try
            {
                IO.Directory.Delete(@"0:\" + path);
            }
            catch (Exception e)
            {
                Console.WriteLine("DeleteFileOrDirectory: Exception thrown: " + e.Message);
            }
        }

        public static void DeleteFile(string path)
        {
            try
            {
                IO.File.Delete(@"0:\" + path);
            }
            catch (Exception e)
            {
                Console.WriteLine("DeleteFileOrDirectory: Exception thrown: " + e.Message);
            }
        }

        public static void WriteFile(string path, string content) {
            try
            {
               IO.File.WriteAllText(@"0:\" + path, content);
            } catch (Exception e) 
            {
                Console.WriteLine("WriteFile: Exception thrown: " + e.Message);
            }
        }

        public static void WriteFile(string path, string[] content)
        {
            foreach(string s in content)
            {
                WriteFile(path, s);
            }
        }

        public static string ReadFile(string path) {
            try
            {
                return IO.File.ReadAllText(@"0:\" + path);
            } catch (Exception e)
            {
                Console.WriteLine("ReadFile: Exception thrown: " + e.Message);
                return null;
            }
        }

        // @todo implement this correctly
        public static string[] FindFile(string filename, string path)
        {
            if (string.IsNullOrEmpty(path)) { path = "0:\\"; } 
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
