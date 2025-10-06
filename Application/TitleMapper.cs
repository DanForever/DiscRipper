using System.Diagnostics;

using DiscRipper.Types;

namespace DiscRipper
{
    namespace Mapped
    {
        [DebuggerDisplay("{QueriedTitle}")]
        class Title
        {
            public Types.Title mmkvTitle { get; set; }

            public TheDiscDb2.GraphQL.Title tddbTitle { get; set; }
        }

        class Disc
        {
            public TheDiscDb2.GraphQL.Node tddbNode { get; set; }

            public TheDiscDb2.GraphQL.Release tddbRelease { get; set; }

            public TheDiscDb2.GraphQL.Disc tddbDisc { get; set; }

            public List<TheDiscDb2.GraphQL.Title> tddbTitlesSorted { get; set; }

            public List<Types.Title> mmkvTitlesSorted { get; set; }

            public List<Title> MatchedTitles { get; set; } = [];
        }
    }

    internal class TitleMapper
    {
        #region Public methods

        public static List<Mapped.Disc> Map(IEnumerable<Title> titles, List<TheDiscDb2.GraphQL.Node> nodes)
        {
            List<Title> titlesSorted = [.. titles];
            titlesSorted.Sort((a, b) => b.DurationInSeconds.CompareTo(a.DurationInSeconds));

            List<Mapped.Disc> mappedDiscs = [];

            foreach (var node in nodes)
            {
                foreach (var release in node.Releases)
                {
                    foreach (var disc in release.Discs)
                    {
                        Mapped.Disc mappedDisc = MapDisc(disc, titlesSorted);
                        mappedDisc.tddbNode = node;
                        mappedDisc.tddbRelease = release;

                        mappedDiscs.Add(mappedDisc);
                    }
                }
            }

            mappedDiscs.Sort((a, b) => b.MatchedTitles.Count.CompareTo(a.MatchedTitles.Count));

            return mappedDiscs;
        }

        #endregion Public methods

        #region Private methods

        private static Mapped.Disc MapDisc(TheDiscDb2.GraphQL.Disc tddbDisc, List<Title> mmkvTitlesSorted)
        {
            List<TheDiscDb2.GraphQL.Title> tddbTitlesSorted = [.. tddbDisc.Titles];
            tddbTitlesSorted.Sort((a, b) => b.DurationInSeconds.CompareTo(a.DurationInSeconds));

            Mapped.Disc mappedDisc = new()
            {
                tddbDisc = tddbDisc,
                tddbTitlesSorted = [.. tddbTitlesSorted],
                mmkvTitlesSorted = mmkvTitlesSorted
            };

            foreach(var mmkvTitle in mmkvTitlesSorted)
            {
                foreach(var tddbTitle in tddbTitlesSorted)
                {
                    if(tddbTitle.DurationInSeconds == mmkvTitle.DurationInSeconds)
                    {
                        if(tddbTitle.Item != null)
                        {
                            mappedDisc.MatchedTitles.Add(new Mapped.Title() { mmkvTitle = mmkvTitle, tddbTitle = tddbTitle });
                        }

                        tddbTitlesSorted.Remove(tddbTitle);
                        break;
                    }
                }
            }

            return mappedDisc;
        }

        #endregion Private methods
    }
}
