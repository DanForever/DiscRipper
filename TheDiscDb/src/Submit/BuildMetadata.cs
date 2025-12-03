namespace DiscRipper.TheDiscDb.Submit;

using TDDB = global::TheDiscDb;

public class BuildMetadata : IStep
{
    public Task Run(SubmissionContext context)
    {
        if(context.ImportItem == null)
        {
            throw new ArgumentException("ImportItem must be set in context before building metadata.");
        }

        if(context.Release?.MediaType == null)
        {
            throw new ArgumentException("Submission MediaType must be set in context before building metadata.");
        }

        context.Year = context.ImportItem.TryGetYear();
        context.Metadata = BuildMetadataInternal(context.ImportItem.ImdbTitle, context.ImportItem.GetTmdbItemToSerialize() as Fantastic.TheMovieDb.Models.Movie, context.ImportItem.GetTmdbItemToSerialize() as Fantastic.TheMovieDb.Models.Series, context.Year, context.Release.MediaType);

        return Task.CompletedTask;
    }

    private static string? GetSortTitle(string? title)
    {
        if (string.IsNullOrEmpty(title))
        {
            return title;
        }

        if (title.StartsWith("the", StringComparison.OrdinalIgnoreCase))
        {
            return title.Substring(4).Trim() + ", The";
        }
        else
        {
            return title;
        }
    }

    private static string CreateSlug(string name, int year)
    {
        if (year != default)
        {
            return string.Format("{0}-{1}", TDDB.StringExtensions.Slugify(name), year);
        }

        return TDDB.StringExtensions.Slugify(name);
    }

    private TDDB.ImportModels.MetadataFile BuildMetadataInternal(TDDB.Imdb.TitleData? imdbTitle, Fantastic.TheMovieDb.Models.Movie? movie, Fantastic.TheMovieDb.Models.Series? series, int year, string importItemType)
    {
        var metadata = new TDDB.ImportModels.MetadataFile
        {
            Year = year,
            Type = importItemType,
            DateAdded = DateTimeOffset.UtcNow.Date
        };

        if (imdbTitle != null && string.IsNullOrEmpty(imdbTitle.ErrorMessage))
        {
            metadata.Title = imdbTitle.Title;
            metadata.FullTitle = imdbTitle.FullTitle;
            metadata.ExternalIds.Imdb = imdbTitle.Id;
            if (imdbTitle?.Title != null)
            {
                metadata.Slug = CreateSlug(imdbTitle.Title, year);
            }
        }
        else if (movie != null)
        {
            if (movie.ReleaseDate.HasValue)
            {
                metadata.ReleaseDate = movie.ReleaseDate.Value;
            }

            metadata.Title = movie.Title;
            metadata.FullTitle = movie.OriginalTitle;

            if (movie?.Title != null)
            {
                metadata.Slug = CreateSlug(movie.Title, year);
            }

            if (string.IsNullOrEmpty(metadata.ExternalIds.Imdb))
            {
                metadata.ExternalIds.Imdb = movie!.ImdbId;
                if (string.IsNullOrEmpty(metadata.ExternalIds.Imdb))
                {
                    metadata.ExternalIds.Imdb = movie!.ExternalIds?.ImdbId;
                }
            }
        }
        else if (series != null)
        {
            if (series.FirstAirDate.HasValue)
            {
                metadata.ReleaseDate = series.FirstAirDate.Value;
            }

            metadata.Title = series.Name;
            metadata.SortTitle = GetSortTitle(series.Name);
            metadata.SortTitle = GetSortTitle(metadata.Title);
            metadata.FullTitle = series.OriginalName;

            if (series?.Name != null)
            {
                metadata.Slug = CreateSlug(series.Name, year);
            }

            if (string.IsNullOrEmpty(metadata.ExternalIds.Imdb))
            {
                metadata.ExternalIds.Imdb = series!.ExternalIds?.ImdbId;
            }
        }

        if (imdbTitle != null && string.IsNullOrEmpty(imdbTitle.ErrorMessage))
        {
            metadata.Plot = imdbTitle.Plot;
            metadata.Directors = imdbTitle.Directors;
            metadata.Stars = imdbTitle.Stars;
            metadata.Writers = imdbTitle.Writers;
            metadata.Genres = imdbTitle.Genres;
            metadata.Runtime = imdbTitle.RuntimeStr;
            metadata.ContentRating = imdbTitle.ContentRating;
            metadata.Tagline = imdbTitle.Tagline;
            if (metadata.ReleaseDate == default(DateTimeOffset) && !string.IsNullOrEmpty(imdbTitle.ReleaseDate))
            {
                metadata.ReleaseDate = DateTimeOffset.Parse(imdbTitle.ReleaseDate + "T00:00:00+00:00");
            }

            if (Int32.TryParse(imdbTitle.RuntimeMins, out int minutes))
            {
                metadata.RuntimeMinutes = minutes;
            }
        }

        if (movie != null)
        {
            metadata.ExternalIds.Tmdb = movie.Id.ToString();

            if (string.IsNullOrEmpty(metadata.Plot))
            {
                metadata.Plot = movie.Overview;
            }

            if (string.IsNullOrEmpty(metadata.Tagline))
            {
                metadata.Tagline = movie.Tagline;
            }
        }
        else if (series != null)
        {
            metadata.ExternalIds.Tmdb = series.Id.ToString();

            if (string.IsNullOrEmpty(metadata.Plot))
            {
                metadata.Plot = series.Overview;
            }
        }

        if (metadata.Title != null && metadata.Title.StartsWith("the", StringComparison.OrdinalIgnoreCase))
        {
            metadata.SortTitle = metadata.Title.Substring(4, metadata.Title.Length - 4).Trim() + ", The";
        }
        else
        {
            metadata.SortTitle = metadata.Title;
        }

        return metadata;
    }
}
