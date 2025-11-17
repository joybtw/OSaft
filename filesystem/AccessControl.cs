using OSaft.applications;
using OSaft.filesystem;
using OSaft.usersystem;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;



namespace OSaft.filesystem
{
    public class PermissionManager
    {
        private static readonly string UserAccessControl = @"\system\rights.db";

        public static void CreateDefaultPermissions()
        {
            List<PermissionEntry> permissions = new List<PermissionEntry>
            {
                new PermissionEntry("0:\\", 0)
            };
            List <User> users = UserDatabase.LoadAllUsers();

            User root = users.Find(u => u.Uid.Equals(0));
            permissions.Add(new PermissionEntry("0:\\system", 0));
            permissions.Add(new PermissionEntry("0:\\users", 0));
            permissions.Add(new PermissionEntry("0:\\users\\" + root.Name, 0));
            permissions.Add(new PermissionEntry("0:\\system\\startscreen.cfg", 0));
           // permissions.Add(new PermissionEntry("\\system\\users.db", 0));
           // permissions.Add(new PermissionEntry("\\system\\root_init.sys", 0));

            /*
               foreach (User u in users.Skip(1))
               {
                   permissions[0].ReadUids.Add(u.Uid);
                   permissions[0].WriteUids.Add(u.Uid);
                   permissions[1].ReadUids.Add(u.Uid);
                   permissions[1].WriteUids.Add(u.Uid);

               }
            */
            SavePermissions(permissions);
        }
        public static void SavePermissions(List<PermissionEntry> ls)
        {
            if (!File.Exists(@"0:\system\rights.db"))
            {
                
                FileSystem.CreateFile("system\\rights.db");
                
                //  ls.Add(new PermissionEntry(UserAccessControl, 0));

            }
            string sb = "";
            foreach (PermissionEntry r in ls)
            {
                string readList = string.Join(",", r.ReadUids);
                string writeList = string.Join(",", r.WriteUids);

                sb += (r.Path + '|' + r.OwnerUid + '|' + readList + '|' + writeList + '\n');
            }
            FileSystem.WriteFile(@"\system\rights.db", sb);
        }

        public static List<PermissionEntry> LoadACR()
        {
            List<PermissionEntry> list = new();

            string content = FileSystem.ReadFile(UserAccessControl, false);
            if (string.IsNullOrWhiteSpace(content))
            {
                Console.WriteLine("[RightsDB] No content found in rights.db");
                return list;
            }

            string[] lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                string[] parts = line.Split('|');
                if (parts.Length < 4) continue;

                try
                {
                    string path = parts[0];
                    uint owner = uint.Parse(parts[1]);
                    List<uint> read = parts[2].Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(x => uint.Parse(x))
                                .ToList();

                    List<uint> write = parts[3].Split(',', StringSplitOptions.RemoveEmptyEntries)
                                               .Select(x => uint.Parse(x))
                                               .ToList();


                    list.Add(new PermissionEntry(path, owner, read, write));

                }
                catch (Exception e)
                {
                    Console.WriteLine("[RightsDB] Error parsing user line: " + e.Message);
                }
            }
            return list;   
        }
        public static void AddReading(string path, uint user)
        {
            List<PermissionEntry> entries = LoadACR();
            if (entries.Find(e => e.Path == path)==null)
            {
                entries.Add(new PermissionEntry(path, user));
            }
            else
            {
                entries.Find(e => e.Path == path)?.ReadUids.Add(user);
            }
            SavePermissions(entries);
        }

        public static void AddWriting(string path, uint user)
        {
            List<PermissionEntry> entries = LoadACR();
            if (entries.Find(e => e.Path == path) == null)
            {
                entries.Add(new PermissionEntry(path, user));
            }
            else
            {
                entries.Find(e => e.Path == path)?.WriteUids.Add(user);
            }
            SavePermissions(entries);
        }

        public static void AddRights(string path, uint user)
        {
            List<PermissionEntry> entries = LoadACR();
            if (entries.Find(e => e.Path == path) == null)
            {
                entries.Add(new PermissionEntry(path, user));
            } else
            {
                entries.Find(e => e.Path == path)?.ReadUids.Add(user);
                entries.Find(e => e.Path == path)?.WriteUids.Add(user);
            }
            SavePermissions(entries);
        }

        public static void RemoveReading(string path, uint user)
        {
            List<PermissionEntry> entries = LoadACR();
            entries.Find(e => e.Path == path)?.ReadUids.Remove(user);
            SavePermissions(entries);
        }

        public static void RemoveWriting(string path, uint user)
        {
            List<PermissionEntry> entries = LoadACR();
            entries.Find(e => e.Path == path)?.WriteUids.Remove(user);
            SavePermissions(entries);
        }
        public static void RemoveRights(string path, uint user)
        {
            List<PermissionEntry> entries = LoadACR();
            entries.Find(e => e.Path == path)?.ReadUids.Remove(user);
            entries.Find(e => e.Path == path)?.WriteUids.Remove(user);
            SavePermissions(entries);
        }
        public static void RemoveEntry(string path)
        {
            List<PermissionEntry> entries = LoadACR();
            PermissionEntry found = entries.Find(e => e.Path.Equals(path));
            entries.Remove(found);
        }
        public static void NewUser(User user)
        {
            AddReading("0:\\", user.Uid);
            AddReading("0:\\users", user.Uid);
            AddRights("0:\\users\\" + user.Name, user.Uid);

            if (user.IsSuperuser)
            {
                AddWriting("0:\\", user.Uid);
                AddWriting("0:\\users", user.Uid);
                AddRights("0:\\system", user.Uid);
                AddRights("0:\\system\\startscreen.cfg", user.Uid);
            }
        }

        public static bool CanRead(string path, uint uid)
        {
            List<PermissionEntry> entries = LoadACR();
            PermissionEntry entry = entries.Find(e => e.Path == path);
            if (entry == null) return false;
            if (entry.ReadUids.Contains(uid)) return true;
            return false;

        }

        public static bool CanWrite(string path, uint uid)
        {
            List<PermissionEntry> entries = LoadACR();
            PermissionEntry entry = entries.Find(e => e.Path == path);
            if (entry == null) return false;
            if (entry.WriteUids.Contains(uid)) return true;
            return false;

        }

    }
}