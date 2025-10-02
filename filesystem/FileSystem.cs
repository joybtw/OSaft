using System;
using System.Runtime.Versioning;
using IO = System.IO;
using Sys = Cosmos.System;

namespace OSaft.filesystem {

    public class FileSystem
    {
        public static readonly Sys.FileSystem.CosmosVFS Instance = new();

        public static IO.FileStream CreateFile(string path) {
            try
            {
                return IO.File.Create(@"0:\" + path);
            } catch(Exception e)
            {
                Console.WriteLine("CreateFile: Exception thrown: " + e.Message);
                return null;
            }
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

        public static void DeleteFileOrDirectory(string path)
        {
            try
            {
                if (path.Contains('.'))
                {
                    IO.File.Delete(@":\" + path);
                } else
                {
                    IO.Directory.Delete(path);
                }
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

    }
}
