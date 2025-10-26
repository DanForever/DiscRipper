using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace DiscRipper.ViewModel;

internal class Submission : ViewModel
{
	private static string TmdbUrlPattern = """(?:https:\/\/)?(?:www\.)?themoviedb\.org\/(movie|tv)\/(\d+)[\w\d-]*""";
	private static Regex TmdbRegex = new Regex(TmdbUrlPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

	private static string AsinUrlPattern = """(?:https:\/\/)?(?:www\.)?amazon\.[\w\.]+\/[\w\d-]+\/dp\/([A-Z\d]{10})""";
	private static Regex AsinRegex = new Regex(AsinUrlPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

	private bool _generateReleaseSlug = true;
	private bool _generateDiscSlug = true;

	private void GenerateReleaseSlug()
	{
		if (!_generateReleaseSlug)
			return;

		int year = PublicationDate?.Year ?? 0;
		string discFormat = DiscFormat ?? "unknown";
		string locale = Locale ?? "unknown";

		string generatedReleaseSlug = $"{year:D4}-{discFormat.ToLower()}-{locale.ToLower()}";
		ChangeProperty(Model, generatedReleaseSlug, nameof(ReleaseSlug));
	}

	private void GenerateDiscSlug()
	{
		if (!_generateDiscSlug)
			return;

		string discFormat = DiscFormat ?? "unknown";

		string generatedDiscSlug = $"{discFormat.ToLower()}";
		ChangeProperty(Model, generatedDiscSlug, nameof(DiscSlug));
	}

	public required TheDiscDb.Submission Model { get; init; }

	public string? TMDB
	{
		get => Model.TMDB;
		set
		{
			Match match = TmdbRegex.Match(value);
			if (match.Success)
			{
				string tmdbId = match.Groups[2].Value;
				ChangeProperty(Model, tmdbId);

				// We can also deduce the media type from the TMDB URL
				string mediaType = match.Groups[1].Value;
				switch (mediaType)
				{
				case "movie":
					MediaType = "Movie";
					break;
				case "tv":
					MediaType = "Series";
					break;
				}
			}
			else
			{
				ChangeProperty(Model, value);
			}
		}
	}

	public string? MediaType
	{
		get => Model.MediaType;
		set => ChangeProperty(Model, value);
	}

	public string? DiscFormat
	{
		get => Model.DiscFormat;
		set
		{
			ChangeProperty(Model, value);

			GenerateReleaseSlug();
			GenerateDiscSlug();
		}
	}

	public string? ReleaseSlug
	{
		get => Model.ReleaseSlug;
		set
		{
			if (string.IsNullOrEmpty(value))
				_generateReleaseSlug = true;
			else
				_generateReleaseSlug = false;

			ChangeProperty(Model, value);
		}
	}

	public string? UPC
	{
		get => Model.UPC;
		set => ChangeProperty(Model, value);
	}

	public string? ASIN
	{
		get => Model.ASIN;
		set
		{
			Match match = AsinRegex.Match(value);
			if (match.Success)
			{
				string asin = match.Groups[1].Value;
				ChangeProperty(Model, asin);
			}
			else
			{
				ChangeProperty(Model, value);
			}
		}
	}

	public DateTime? PublicationDate
	{
		get => Model.PublicationDate;
		set
		{
			ChangeProperty(Model, value);

			GenerateReleaseSlug();
		}
	}

	public string? FrontCoverUrl
	{
		get => Model.FrontCoverUrl;
		set => ChangeProperty(Model, value);
	}

	public string? BackCoverUrl
	{
		get => Model.BackCoverUrl;
		set => ChangeProperty(Model, value);
	}

	public string? EditionName
	{
		get => Model.EditionName;
		set => ChangeProperty(Model, value);
	}

	public string? DiscTitle
	{
		get => Model.DiscTitle;
		set => ChangeProperty(Model, value);
	}

	public string? DiscSlug
	{
		get => Model.DiscSlug;
		set
		{
			if (string.IsNullOrEmpty(value))
				_generateDiscSlug = true;
			else
				_generateDiscSlug = false;

			ChangeProperty(Model, value);
		}
	}

	public TheDiscDb.RegionCode RegionCode
	{
		get => Model.RegionCode;
		set => ChangeProperty(Model, value);
	}

	public string? Locale
	{
		get => Model.Locale;
		set
		{
			ChangeProperty(Model, value);

			GenerateReleaseSlug();
		}
	}

	public required IEnumerable<SubmissionTitle> Titles { get; init; }
}
