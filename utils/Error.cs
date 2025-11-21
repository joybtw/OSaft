using Cosmos.System.FileSystem;
using IL2CPU.API.Attribs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSaft.utils;
using OSaft.applications;
using OSaft.usersystem;


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

        public Error(string progname, string message, criticality crit, bool print)      // error-nachricht, festlegung der kritikalit�tsstufe, soll auf konsole ausgegeben werden?
        {
            this.ProgName = progname;
            this.Message = message;
            this.Crit = crit;
            this.Print = print;

            CreateLogFileIfNotExists();
        }

        private void CreateLogFileIfNotExists()
        {
            string dirPath = @"0:\logs\";
            //validierung, ob log-directory existiert
            if (!Directory.Exists(dirPath))
            {
                FileSystem.CreateDirectory(dirPath);
            }
            string filePath = dirPath + this.ProgName + ".log";

            if (!File.Exists(filePath))
            {
                FileSystem.CreateFile(filePath);

                List<User> users = UserDatabase.LoadAllUsers();
                foreach (User user in users)
                {
                    PermissionManager.AddReading(filePath, user.Uid);
                }

            }
        }

        public void CreateNewLog()
        {
            string dirPath = @"0:\logs\";
            string filePath = dirPath + this.ProgName + ".log";
            string enumstring = GetCriticalityString(Crit);
            string message = Clock.ShowTime() + " [" + enumstring + "] " + this.ProgName + ": " + this.Message;

            if (Print == true)
            {
                Console.WriteLine(message);
            }

            if (File.Exists(filePath))
            {
                string oldfilecontent = File.ReadAllText(filePath);
                string newfilecontent = string.Concat(oldfilecontent, "\n", message);
                FileSystem.WriteFile((filePath), newfilecontent);
            }

            if (Crit == criticality.VeryCritical)
            {
                string[] logfilecontent = File.ReadAllLines(filePath);
                //string logfilecontent = File.ReadAllText(@"0:\" + filePath);
                ErrorScreen.init(this.ProgName, logfilecontent);
            }
        }

        private static string GetCriticalityString(criticality c)
        {
            return c switch
            {
                criticality.Info => "Info",
                criticality.Warning => "Warning",
                criticality.Critical => "Critical",
                criticality.VeryCritical => "VeryCritical",
                _ => "Unknown"
            };
        }
    }
}
