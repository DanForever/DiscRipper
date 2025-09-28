using Fantastic.FileSystem;

namespace DiscRipper.TheDiscDb
{
    public struct DiscName
    {
        public string Name;
        public int Index;
    }

    /// <summary>
    /// These are functions that kinda want exposing in a more generic way in the ImportBuddy assemblies
    /// </summary>
    public class CopiedImportBuddyFunctions
    {
        public required IFileSystem FileSystem { get; init; }

        public async Task<DiscName> GetDiscName(string path)
        {
            var files = await FileSystem.Directory.GetFiles(path, "*disc*");
            var name = new DiscName
            {
                Name = "disc01",
                Index = 1
            };

            for (int i = 1; i < 100; i++)
            {
                name.Name = string.Format("disc{0:00}", i);
                name.Index = i;

                if (files.Any(f => f.Contains(name.Name)))
                {
                    continue;
                }

                break;
            }

            return name;
        }
    }
}
