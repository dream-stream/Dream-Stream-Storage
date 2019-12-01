using MessagePack;

namespace Dream_Stream_Storage.Models.Responses
{
    [MessagePackObject]
    public class MessageReceived : IMessage
    {
        [Key(0)]
        public long Offset { get; set; }
    }
}
