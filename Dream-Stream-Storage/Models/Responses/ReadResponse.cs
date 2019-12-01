using MessagePack;

namespace Dream_Stream_Storage.Models.Responses
{
    [MessagePackObject]
    public class ReadResponse : IMessage
    {
        [Key(0)]
        public byte[] Message { get; set; }
    }
}
