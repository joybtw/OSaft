using Sys = Cosmos.System;

namespace OSaft.filesystem {

    public class FileSystem
    {
        public static readonly Sys.FileSystem.CosmosVFS Instance = new();
    }
}
