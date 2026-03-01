using System.Globalization;

namespace DiscRipper.Types
{
	public class Locales
	{
		public static IEnumerable<CultureInfo> AllRegions { get; private set; }

		static Locales()
		{
			AllRegions = CultureInfo.GetCultures(CultureTypes.SpecificCultures).OrderBy(c => c.DisplayName);
		}
	}
}
