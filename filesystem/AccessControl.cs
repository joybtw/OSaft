using OSaft.applications;
using OSaft.filesystem;
using OSaft.usersystem;
using OSaft.utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OSaft.filesystem
{
    public class PermissionManager
    {
        protected static readonly string rights_db = @"0:\system\rights.db";
        private static readonly string db_hash = @"0:\system\rights_init.sys";
        private static bool dbcheck = false;
        private static bool dblocked = false;

        public static void CreateDefaultPermissions()
        {

            FileSystem.CreateFile(db_hash);

            List<PermissionEntry> permissions = new()
            {
                new PermissionEntry(@"0:\", 0u),
                new PermissionEntry(@"0:\system\", 0u),
                new PermissionEntry(@"0:\users\", 0u),
                new PermissionEntry(@"0:\system\startscreen.cfg", 0u),
                new PermissionEntry(@"0:\logs\", 0u)
            };

            List<User> users = UserDatabase.LoadAllUsers() ?? new List<User>();

            User root = users.Find(u => u.Uid == 0u);
            if (root != null)
                permissions.Add(new PermissionEntry($@"0:\users\{root.Name}\", root.Uid));

            SavePermissions(permissions);
        }

        public static void SavePermissions(List<PermissionEntry> list)
        {
            if (!File.Exists(rights_db))
                FileSystem.CreateFile(rights_db);


            if (dblocked == true)
            {
                Console.WriteLine("--------------------------------");
                Console.WriteLine("[RightsDB] ERROR: Changes rejected! Changes are currently blocked.");
                Console.WriteLine("--------------------------------");
                dbcheck = false;
                return;
            }

            var lines = list.Select(r =>
                $"{r.Path.Trim()}|{r.OwnerUid}|{string.Join(",", r.ReadUids)}|{string.Join(",", r.WriteUids)}"
            );

            string final = string.Join(Environment.NewLine, lines);

            try
            {
                FileSystem.WriteFile(rights_db, final);
                FileSystem.WriteFile(db_hash, OSaftSecurity.FNV1a(final));

            }
            catch (Exception e)
            {
                Console.WriteLine("[RightsDB] Error saving rights: " + e.Message);
            }
            dbcheck = false;
        }

        public static List<PermissionEntry> LoadACR()
        {
            List<PermissionEntry> list = new();

            string content = File.ReadAllText(rights_db);
            if (string.IsNullOrWhiteSpace(content))
                return list;

            string[] lines = content
                .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);


            if (dbcheck == false)
            {
                string hashCheck = FileSystem.ReadFile(db_hash);
                if (OSaftSecurity.FNV1a(content) != hashCheck)
                {
                    Console.WriteLine("--------------------------------");
                    Console.WriteLine("[RightsDB] Warning: rights.db integrity check failed! Files could be corrupted!");
                    Console.WriteLine("No recovery available. You are on yourself. Good Luck!");
                    Console.WriteLine("--------------------------------");
                    dbcheck = true;
                    dblocked = true;
                }
                else { dblocked = false; }
            }

            foreach (string raw in lines)
            {
                string line = raw.Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] parts = line.Split('|');
                if (parts.Length < 4) continue;

                string path = parts[0].Trim();
                if (!uint.TryParse(parts[1].Trim(), out uint owner))
                    continue;

                List<uint> read = parts[2]
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => uint.Parse(s.Trim()))
                    .ToList();

                List<uint> write = parts[3]
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => uint.Parse(s.Trim()))
                    .ToList();

                list.Add(new PermissionEntry(path, owner, read, write));
            }

            return list;
        }

        private static PermissionEntry GetOrCreate(List<PermissionEntry> entries, string path, uint uid)
        {
            PermissionEntry e = entries.Find(p => p.Path == path);
            if (e != null)
                return e;

            if (uid == 0)
            {
                try
                {
                    uid = UserDatabase.GetUserID(UserHelper.CurrentUser);
                }
                catch { }
            }

            e = new PermissionEntry(path, uid);
            entries.Add(e);
            return e;
        }

        public static void AddReading(string path, uint user)
        {
            List<PermissionEntry> entries = LoadACR();
            PermissionEntry e = GetOrCreate(entries, path, user);

            if (!e.ReadUids.Contains(user))
                e.ReadUids.Add(user);

            SavePermissions(entries);
        }

        public static void AddWriting(string path, uint user)
        {
            List<PermissionEntry> entries = LoadACR();
            PermissionEntry e = GetOrCreate(entries, path, user);

            if (!e.WriteUids.Contains(user))
                e.WriteUids.Add(user);

            SavePermissions(entries);
        }

        public static void AddRights(string path, uint user)
        {
            AddReading(path, user);
            AddWriting(path, user);
        }

        public static void RemoveReading(string path, uint user)
        {
            List<PermissionEntry> entries = LoadACR();
            PermissionEntry e = entries.Find(x => x.Path == path);
            if (e != null) e.ReadUids.Remove(user);

            SavePermissions(entries);
        }

        public static void RemoveWriting(string path, uint user)
        {
            List<PermissionEntry> entries = LoadACR();
            PermissionEntry e = entries.Find(x => x.Path == path);
            if (e != null) e.WriteUids.Remove(user);

            SavePermissions(entries);
        }

        public static void RemoveRights(string path, uint user)
        {
            RemoveReading(path, user);
            RemoveWriting(path, user);
        }

        public static void RemoveEntry(string path)
        {
            List<PermissionEntry> entries = LoadACR();
            PermissionEntry e = entries.Find(x => x.Path.Equals(path));
            if (e != null)
            {
                entries.Remove(e);
                SavePermissions(entries);
                return;
            }
            throw new Exception("internal error");
        }

        public static void NewUser(User user)
        {
            AddReading(@"0:\", user.Uid);
            AddReading(@"0:\users\", user.Uid);
            AddRights($@"0:\users\{user.Name}\", user.Uid);
            AddReading(@"0:\logs\", user.Uid);

            if (user.IsSuperuser)
            {
                AddWriting(@"0:\", user.Uid);
                AddWriting(@"0:\users\", user.Uid);
                AddRights(@"0:\system\", user.Uid);
                AddRights(@"0:\system\startscreen.cfg", user.Uid);
                AddWriting((@"0:\logs\"), user.Uid);
            }
        }

        public static bool CanRead(string path, uint uid)
        {
            List<PermissionEntry> entries = LoadACR();
            PermissionEntry e = entries.Find(x => x.Path == path);
            if (e.ReadUids.Contains(uid)) return true;
            return false;
        }

        public static bool CanWrite(string path, uint uid)
        {
            List<PermissionEntry> entries = LoadACR();
            PermissionEntry e = entries.Find(x => x.Path == path);
            if (e.WriteUids.Contains(uid)) return true;
            return false;
        }

        public static bool TestOwner(string path)
        {
            List<PermissionEntry> entries = LoadACR();

            PermissionEntry e = entries.Find(x => x.Path == path);
            if (e == null)
                return false;
                uint uid = UserDatabase.GetUserID(UserHelper.CurrentUser);
            return e.OwnerUid == uid;
        }

        public static void ChangeOwner(string path, string name)
        {
            if (!TestOwner(path))
            {
                Console.WriteLine("[RightsDB] Error: Not the owner.");
                return;
            }

            List<PermissionEntry> entries = LoadACR();
            PermissionEntry e = entries.Find(x => x.Path == path);
            if (e == null)
            {
                Console.WriteLine("[RightsDB] Error: Entry not found.");
                return;
            }

            uint newUid = UserDatabase.GetUserID(name);
            e.OwnerUid = newUid;

            SavePermissions(entries);
        }

        public static void Display(string path)
        {
            List<PermissionEntry> entries = LoadACR();
            PermissionEntry e = entries.Find(x => x.Path == path);
            if (e == null)
            {
                Console.WriteLine("[RightsDB] Error: Entry not found.");
                return;
            }

            uint current = UserDatabase.GetUserID(UserHelper.CurrentUser);
            string ownerName = UserDatabase.GetUserName(e.OwnerUid);

            bool r = e.ReadUids.Contains(current);
            bool w = e.WriteUids.Contains(current);

            string perms = (r, w) switch
            {
                (false, false) => "none",
                (true, false) => "r",
                (false, true) => "w",
                _ => "r, w"
            };
           
           
            Console.WriteLine($"Path: {e.Path}  Owner: {ownerName}  Your permissions: {perms}");
           
        }

        public static void ShouldRead(string path, string name)
        {
            if (!TestOwner(path))
            {
                Console.WriteLine("[RightsDB] Not the owner.");
                return;
            }

            AddReading(path, UserDatabase.GetUserID(name));
        }

        public static void ShouldWrite(string path, string name)
        {
            Console.WriteLine("test");
            if (!TestOwner(path))
            {
                Console.WriteLine("[RightsDB] Not the owner.");
                return;
            }
            Console.WriteLine(name);
            Console.WriteLine(path);
            AddWriting(path, UserDatabase.GetUserID(name));
        }

        public static void ShouldBoth(string path, string name)
        {
            if (!TestOwner(path))
            {
                Console.WriteLine("[RightsDB] Not the owner.");
                return;
            }

            uint id = UserDatabase.GetUserID(name);
            AddReading(path, id);
            AddWriting(path, id);
        }

        public static void ShouldntRead(string path, string name)
        {
            if (!TestOwner(path))
            {
                Console.WriteLine("[RightsDB] Not the owner.");
                return;
            }
            RemoveReading(path, UserDatabase.GetUserID(name));
        }

        public static void ShouldntWrite(string path, string name)
        {
            if (!TestOwner(path))
            {
                Console.WriteLine("[RightsDB] Not the owner.");
                return;
            }
            RemoveWriting(path, UserDatabase.GetUserID(name));
        }

        public static void ShouldntBoth(string path, string name)
        {
            if (!TestOwner(path))
            {
                Console.WriteLine("[RightsDB] Not the owner.");
                return;
            }

            uint id = UserDatabase.GetUserID(name);
            RemoveReading(path, id);
            RemoveWriting(path, id);
        }
    }
}


