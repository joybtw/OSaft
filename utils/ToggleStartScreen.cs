using OSaft;
using OSaft.filesystem;
using System;
using System.IO;
using System.Threading;

namespace OSaftUtils
{
    public static class ToggleStartScreen
    {
        private static string path = "/system/startscreen.cfg";
        public static void Toggle()
        {
            if (FileSystem.FindFile("startscreen.cfg", "/system").Length > 0)
            {
                FileSystem.DeleteFile(path);
                Console.WriteLine("Startup screen disabled.");
            }
            else
            {
                FileSystem.CreateFile(path);
                OSaftColortheme.chcollor();
                FileSystem.WriteFile(path, "startupscreen=enabled");
                Console.WriteLine("Startup screen enabled.");
            }
        }
    }
}