using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RemoteControlWPFClient.BusinessLayer.Extensions;

public static class StreamExtension
{
    public static async Task CopyToAsync(this Stream source, Stream destination, int bufferSize, IProgress<long> progress = null, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(destination, nameof(destination));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bufferSize, nameof(bufferSize));

        var buffer = new byte[bufferSize];
        int bytesRead;
        long totalRead = 0;
        while ((bytesRead = await source.ReadAsync(buffer, token)) > 0)
        {
            await destination.WriteAsync(buffer.AsMemory(0, bytesRead), token);
            token.ThrowIfCancellationRequested();
            totalRead += bytesRead;
            progress?.Report(totalRead);
        }
    }
}