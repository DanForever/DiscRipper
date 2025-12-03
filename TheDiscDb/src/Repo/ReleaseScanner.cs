namespace DiscRipper.TheDiscDb.Repo;

public class ReleaseScanner
{
	public List<DiscRipper.Types.Media> Scan(string repositoryFolder)
	{
		List<DiscRipper.Types.Media> scanResult = new();

		string[] mediaTypes =
		[
			"movie",
			"series",
		];

		string dataPath = Path.Join(repositoryFolder, "data");
		if (!Directory.Exists(dataPath))
			return [];

		foreach (string mediaType in mediaTypes)
		{
			string path = Path.Join(dataPath, mediaType);
			if (!Directory.Exists(path))
				return [];

			IEnumerable<string> items = Directory.EnumerateDirectories(path);

			foreach (string item in items)
			{
				IEnumerable<string> releases = Directory.EnumerateDirectories(item);

				DiscRipper.Types.Media media = new()
				{
					Title = Path.GetFileName(item),
					Path = item,
					Releases =
						from slug in releases
						select new DiscRipper.Types.Release
						{
							Slug = Path.GetFileName(slug),
							Path = Path.Combine(item, slug),
						}
				};

				scanResult.Add(media);
			}
		}

		return scanResult;
	}
}
