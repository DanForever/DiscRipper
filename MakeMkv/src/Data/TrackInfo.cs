namespace DiscRipper.MakeMkv;

public class TrackInfo
{
    public int TitleId { get; set; }
    public int Id { get; set; }
    public int Code { get; set; }
    public int Unknown { get; set; }
    public required string Value { get; set; }

    public TrackInfoType Type => Enum.IsDefined(typeof(TrackInfoType), Code)? (TrackInfoType) Code : TrackInfoType.Unknown;
}
