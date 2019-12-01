using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MessagePack;

namespace Dream_Stream_Storage
{
    public class StorageService
    {
        private const string BasePath = "/mnt/data";
        private static readonly ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();
        private static readonly ReaderWriterLockSlim OffsetLock = new ReaderWriterLockSlim();
        private static readonly Dictionary<string, (Timer timer, FileStream stream)> PartitionFiles = new Dictionary<string, (Timer timer, FileStream stream)>();

        public async Task<long> Store(string topic, int partition, byte[] message)
        {
            var path = $@"{BasePath}/{topic}/{partition}.txt";
            if (!File.Exists(path))
                CreateFile(path);
            if (!PartitionFiles.ContainsKey(path))
            {
                PartitionFiles.TryAdd(path, (new Timer(x =>
                {
                    if (!PartitionFiles.TryGetValue(path, out var tuple)) return;
                    tuple.stream.Close();
                    tuple.stream.Dispose();
                    PartitionFiles.Remove(path);
                    tuple.timer.Dispose();
                }, null, 10000, Timeout.Infinite),
                    new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite)));
            }

            long offset = 0;
            if (PartitionFiles.TryGetValue(path, out var tuple))
            {
                tuple.timer.Change(10000, Timeout.Infinite);
                
                Lock.EnterWriteLock();
                tuple.stream.Seek(0, SeekOrigin.End);
                offset = tuple.stream.Position;
                await tuple.stream.WriteAsync(message);
                Lock.ExitWriteLock();
            }

            return offset;
        }

        public async Task<byte[]> Read(string consumerGroup, string topic, int partition, long offset, int amount)
        {
            var path = $@"{BasePath}/{topic}/{partition}.txt";

            if (!File.Exists(path))
                CreateFile(path);
            if (!PartitionFiles.ContainsKey(path + consumerGroup))
                PartitionFiles.TryAdd(path + consumerGroup, (new Timer(x =>
                {
                    if (!PartitionFiles.TryGetValue(path + consumerGroup, out var tuple)) return;
                    tuple.stream.Close();
                    tuple.stream.Dispose();
                    PartitionFiles.Remove(path + consumerGroup);
                    tuple.timer.Dispose();
                }, null, 10000, Timeout.Infinite), new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite)));


            var buffer = new byte[amount];
            if (PartitionFiles.TryGetValue(path + consumerGroup, out var stream))
            {
                stream.timer.Change(10000, Timeout.Infinite);
                stream.stream.Seek(offset, SeekOrigin.Begin);
                await stream.stream.ReadAsync(buffer, 0, amount);
            }

            return buffer;
        }

        public async Task StoreOffset(string consumerGroup, string topic, int partition, long offset)
        {
            var path = $@"{BasePath}/offsets/{consumerGroup}/{topic}/{partition}.txt";

            if (!File.Exists(path))
                CreateFile(path);
            if (!PartitionFiles.ContainsKey(path))
            {
                PartitionFiles.TryAdd(path, (new Timer(x =>
                {
                    if (!PartitionFiles.TryGetValue(path, out var tuple)) return;
                    tuple.stream.Close();
                    tuple.stream.Dispose();
                    PartitionFiles.Remove(path);
                    tuple.timer.Dispose();
                }, null, 10000, Timeout.Infinite), new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite)));
            }

            var data = LZ4MessagePackSerializer.Serialize(offset);

            OffsetLock.EnterWriteLock();
            if (PartitionFiles.TryGetValue(path, out var stream))
            {
                stream.timer.Change(10000, Timeout.Infinite);
                stream.stream.Seek(0, SeekOrigin.Begin);
                await stream.stream.WriteAsync(data);
            }
            OffsetLock.ExitWriteLock();
        }

        public async Task<long> ReadOffset(string consumerGroup, string topic, int partition)
        {
            var path = $@"{BasePath}/offsets/{consumerGroup}/{topic}/{partition}.txt";

            if (!File.Exists(path))
                await StoreOffset(consumerGroup, topic, partition, 0);

            var buffer = new byte[8]; //long = 64 bit => 64 bit = 8 bytes
            if (PartitionFiles.TryGetValue(path, out var stream))
            {
                stream.timer.Change(10000, Timeout.Infinite);
                stream.stream.Seek(0, SeekOrigin.Begin);
                await stream.stream.ReadAsync(buffer, 0, 8);
            }

            return LZ4MessagePackSerializer.Deserialize<long>(buffer);
        }

        private static void CreateFile(string path)
        {
            var directories = path.Substring(0, path.LastIndexOf("/", StringComparison.Ordinal));
            Directory.CreateDirectory(directories);
            var stream = File.Create(path);
            stream.Close();
        }
    }
}
