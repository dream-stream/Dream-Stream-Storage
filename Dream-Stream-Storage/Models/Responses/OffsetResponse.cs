using MessagePack;

namespace Dream_Stream_Storage.Models.Responses
{
    [MessagePackObject]
    public class OffsetResponse : IMessage
    {
        [Key(0)]
        public long Offset { get; set; }
    }
}
