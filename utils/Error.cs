using Cosmos.System.FileSystem;
using IL2CPU.API.Attribs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSaft.filesystem
{
    
    public class Error
    {
    public enum criticality
        {
            Info,
            Warning,
            Critical,
            VeryCritical
        }

        public string ProgName { get; }
        public string Message { get; }
        public criticality Crit { get; }
        public bool Print { get; }

        public Error(string progname, string message, criticality crit, bool print)      // error-nachricht, festlegung der kritikalitätsstufe, soll auf konsole ausgegeben werden?
        {
            this.ProgName = progname;
            this.Message = message;
            this.Crit = crit;
            this.Print = print;
            //CreateLogFileIfNotExists();
        }

        internal void CreateLogFileIfNotExists()
        {
            string path = FileSystem.CurrentPath + "SysLog.log";
            var file_stream = FileSystem.CreateFile(path);

        }

        public void CreateNewLog()
        {
            Console.WriteLine("[" + Crit.ToString() + "] " + this.ProgName + ": " + this.Message);
        }


    }
}
