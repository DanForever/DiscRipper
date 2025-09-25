namespace DiscRipper.MakeMkv
{
    public class Message
    {
        public int Code { get; set; }
        public int Flags { get; set; }
        public required string Text { get; set; }
    }
}
