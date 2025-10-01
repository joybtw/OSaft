using System;
using IO = System.IO;
using Sys = Cosmos.System;

namespace OSaft.filesystem {

    public class FileSystem
    {
        public static readonly Sys.FileSystem.CosmosVFS Instance = new();

        public static IO.FileStream CreateFile(string filename, string where) {
            try
            {
                return IO.File.Create(@"0:\" + where + @"\" + filename);
            } catch(Exception e)
            {
                Console.WriteLine("CreateFile: Exception thrown: " + e.Message);
                return null;
            }
        }


        public static IO.DirectoryInfo CreateDirectory(string filename, string where = "") {
            try
            {
                return IO.Directory.CreateDirectory(@"0:\" + where + filename);
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
                    IO.File.Delete(path);
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

    }
}
