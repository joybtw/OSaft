using System;
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


    }
}
