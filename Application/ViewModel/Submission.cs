using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace DiscRipper.ViewModel;

internal class Submission : ViewModel
{
	private static string TmdbUrlPattern = """https:\/\/www.themoviedb.org\/(movie|tv)\/(\d+)[\w\d-]*""";
	private static Regex TmdbRegex = new Regex(TmdbUrlPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

	public SolidColorBrush? TmdbInputBackgroundBrush { get; set; }

	public required TheDiscDb.Submission Model { get; init; }

	public SolidColorBrush TmdbBackgroundBrush { get; set; }

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

				string mediaType = match.Groups[1].Value;
				switch(mediaType)
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
		set => ChangeProperty(Model, value);
	}

	public string? ReleaseSlug
	{
		get => Model.ReleaseSlug;
		set => ChangeProperty(Model, value);
	}

	public string? UPC
	{
		get => Model.UPC;
		set => ChangeProperty(Model, value);
	}

	public string? ASIN
	{
		get => Model.ASIN;
		set => ChangeProperty(Model, value);
	}

	public DateTime? PublicationDate
	{
		get => Model.PublicationDate;
		set => ChangeProperty(Model, value);
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
		set => ChangeProperty(Model, value);
	}

	public TheDiscDb.RegionCode RegionCode
	{
		get => Model.RegionCode;
		set => ChangeProperty(Model, value);
	}

	public string? Locale
	{
		get => Model.Locale;
		set => ChangeProperty(Model, value);
	}

	public required IEnumerable<SubmissionTitle> Titles { get; init; }
}
