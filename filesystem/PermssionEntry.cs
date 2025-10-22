using System.Collections.Generic;

namespace OSaft.usersystem
{
    public class PermissionEntry
    {
        public string Path { get; set; }            // Pfad zur Datei
        public uint OwnerUid { get; set; }          // Eigentümer
        public List<uint> ReadUids { get; set; }    // wer darf lesen
        public List<uint> WriteUids { get; set; }   // wer darf schreiben

        public PermissionEntry(string path, uint owner)
        {
            Path = path;
            OwnerUid = owner;
            ReadUids = new List<uint> { owner };
            WriteUids = new List<uint> { owner };
        }

        public PermissionEntry(string path, uint owner, List<uint> read, List<uint> write)
        {
            Path = path;
            OwnerUid = owner;
            ReadUids = read;
            WriteUids = write;
        }

        public bool CanRead(uint uid) => ReadUids.Contains(uid) || OwnerUid == uid;
        public bool CanWrite(uint uid) => WriteUids.Contains(uid) || OwnerUid == uid;
    }
}
