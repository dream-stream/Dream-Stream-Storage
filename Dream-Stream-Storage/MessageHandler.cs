using System;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Dream_Stream_Storage.Models;
using Dream_Stream_Storage.Models.Requests;
using Dream_Stream_Storage.Models.Responses;
using MessagePack;
using Microsoft.AspNetCore.Http;

namespace Dream_Stream_Storage
{
    public class MessageHandler
    {
        private static readonly StorageService Storage = new StorageService();

        public async Task Handle(HttpContext context, WebSocket webSocket)
        {
            var buffer = new byte[1024 * 6];
            WebSocketReceiveResult result = null;

            try
            {
                do
                {
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.CloseStatus.HasValue) break;

                    var buf = buffer.Take(result.Count).ToArray();

                    var message = LZ4MessagePackSerializer.Deserialize<IMessage>(buf);

                    switch (message)
                    {
                        case StoreRequest request:
                            var storedAtOffset = await Storage.Store(request.Topic, request.Partition, request.Message);
                            await SendResponse(new MessageReceived() {Offset = storedAtOffset }, webSocket);
                            break;
                        case MessageRequest request:
                            await Storage.StoreOffset(request.ConsumerGroup, request.Topic, request.Partition,
                                request.OffSet);
                            var messages = await Storage.Read(request.ConsumerGroup, request.Topic, request.Partition, request.OffSet,
                                request.ReadSize);
                            await SendResponse(new ReadResponse {Message = messages}, webSocket);
                            break;
                        case OffsetRequest request:
                            var offset = await Storage.ReadOffset(request.ConsumerGroup, request.Topic, request.Partition);
                            await SendResponse(new OffsetResponse
                            {
                                Offset = offset
                            }, webSocket);
                            break;
                    }

                } while (!result.CloseStatus.HasValue);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Connection closed");
            }
            finally
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, result?.CloseStatusDescription ?? "Failed hard", CancellationToken.None);
            }
        }

        private static async Task SendResponse(IMessage message, WebSocket webSocket)
        {
            await webSocket.SendAsync(new ArraySegment<byte>(LZ4MessagePackSerializer.Serialize(message)), WebSocketMessageType.Binary, false,
                CancellationToken.None);
        }
    }
}
