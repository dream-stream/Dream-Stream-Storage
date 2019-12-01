﻿using MessagePack;

namespace Dream_Stream_Storage.Models.Requests
{
    [MessagePackObject]
    public class StoreRequest : IMessage
    {
        [Key(0)]
        public string Topic { get; set; }
        [Key(1)]
        public int Partition { get; set; }
        [Key(2)]
        public byte[] Message { get; set; }
    }
}
