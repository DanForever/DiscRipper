using OGTddb = global::TheDiscDb;

namespace DiscRipper.TheDiscDb.ImportBuddy
{
    public static class ExportUtilities
    {
        public static OGTddb.InputModels.Track CreateTddbExportType(this DiscRipper.Types.VideoTrack track)
        {
            return new OGTddb.InputModels.Track()
            {
                Index = track.Index,
                Type = track.Type,
                Name = track.Name,

                Resolution = track.Resolution,
                AspectRatio = track.AspectRatio
            };
        }

        public static OGTddb.InputModels.Track CreateTddbExportType(this DiscRipper.Types.AudioTrack track)
        {
            return new OGTddb.InputModels.Track()
            {
                Index = track.Index,
                Type = track.Type,
                Name = track.Name,

                AudioType = track.AudioType,
                LanguageCode = track.LanguageCode,
                Language = track.Language
            };
        }

        public static OGTddb.InputModels.Track CreateTddbExportType(this DiscRipper.Types.SubtitleTrack track)
        {
            return new OGTddb.InputModels.Track()
            {
                Index = track.Index,
                Type = track.Type,
                Name = track.Name,

                LanguageCode = track.LanguageCode,
                Language = track.Language
            };
        }
    }
}
