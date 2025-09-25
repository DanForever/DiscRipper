using System.Text.RegularExpressions;

namespace DiscRipper.MakeMkv;

public class DriveParser : Parser<Drive>
{
    // DRV:0,2,999,12,"DriveName","ALMOST_FAMOUS_P2","D:"
    public override string Pattern => """
        DRV:(\d+),(\d+),(\d+),(\d+),"([^"]*)","([^"]*)","([^"]*)"
        """;

    public override Drive? CreateValueFromMatch(Match match)
    {
        DriveStatus driveStatus = (DriveStatus)int.Parse(match.Groups[2].Value);

        if(driveStatus == DriveStatus.NotAttached)
            return null;

        //     1 2 3   4   5           6                  7
        // DRV:0,2,999,12,"DriveName","ALMOST_FAMOUS_P2","D:"
        Drive drive = new()
        {
            DriveStatus = driveStatus,
            Index = int.Parse(match.Groups[1].Value),
            DiscType = (DiscType)int.Parse(match.Groups[4].Value),
            DriveName = match.Groups[5].Value,
            MediaTitle = match.Groups[6].Value,
            DrivePath = match.Groups[7].Value
        };

        return drive;
    }
}
