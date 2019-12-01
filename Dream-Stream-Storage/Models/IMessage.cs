using Dream_Stream_Storage.Models.Requests;
using Dream_Stream_Storage.Models.Responses;
using MessagePack;

namespace Dream_Stream_Storage.Models
{
    [Union(2, typeof(OffsetRequest))]
    [Union(5, typeof(MessageRequest))]
    [Union(7, typeof(MessageReceived))]
    [Union(8, typeof(OffsetResponse))]
    [Union(9, typeof(StoreRequest))]
    [Union(10, typeof(ReadResponse))]
    [Union(11, typeof(StoreOffsetRequest))]
    public interface IMessage
    {
    }
}
