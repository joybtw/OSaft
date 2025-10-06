using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IO = System.IO;
using Sys = Cosmos.System;

namespace OSaft.filesystem {

    public class FileSystem
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
        public static string[] FindFile(string filename, string startPath)
        {
            //List<string> paths = new();
            //List<string> directories = Directory.GetDirectories(@"0:\" + startPath).ToList();
            //directories.Insert(0, "");
            //foreach (string dir in directories)
            //{
            //    foreach(string f in Directory.GetFiles(@"0:\" + dir))
            //    {
            //        if (f == filename) {
            //            paths.Add(@"0:\" + dir + @"\" + filename);
            //        }
            //    }
            //    directories = directories.Concat(directories, Directory.GetDirectories(@"0:\" + dir);
            //}

            //return paths.ToArray();
            return null;
        }

    }
}
