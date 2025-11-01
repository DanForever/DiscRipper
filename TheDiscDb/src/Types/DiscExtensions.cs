
using OGTddb = global::TheDiscDb;

namespace DiscRipper.TheDiscDb.Types;

public static class DiscExtensions
{
	public static OGTddb.Core.DiscHash.DiscHashInfo ToImportBuddyType(this DiscRipper.Types.Hash.Disc disc)
	{
		var files = from file in disc.Files select new OGTddb.Core.DiscHash.FileHashInfo
		{
			Index = file.Index,
			Name = file.Name,
			CreationTime = file.CreationTime,
			Size = file.Size
		};

		OGTddb.Core.DiscHash.DiscHashInfo outData = new()
		{
			Hash = disc.Hash,
			Files = [.. files]
		};

		return outData;
	}

	public static DiscRipper.Types.Hash.Disc ToDiscRipperType(this OGTddb.Core.DiscHash.DiscHashInfo disc)
	{
		var files = from file in disc.Files select new DiscRipper.Types.Hash.File
		{
			Index = file.Index,
			Name = file.Name,
			CreationTime = file.CreationTime,
			Size = file.Size
		};

		DiscRipper.Types.Hash.Disc outData = new()
		{
			Hash = disc.Hash ?? "unset",
			Files = [.. files]
		};

		return outData;
	}
}
