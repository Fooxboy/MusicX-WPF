using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;

namespace MusicX.Helpers;

public class EmbeddedFileStreamReference(string resourceName, string contentType) : IRandomAccessStreamReference
{
    public IAsyncOperation<IRandomAccessStreamWithContentType> OpenReadAsync() =>
        OpenReadAsyncInternal().AsAsyncOperation();

    private async Task<IRandomAccessStreamWithContentType> OpenReadAsyncInternal()
    {
        var assembly = typeof(EmbeddedFileStreamReference).Assembly;
        await using var manifestResourceStream = assembly.GetManifestResourceStream(resourceName);
        
        if (manifestResourceStream == null)
            throw new FileNotFoundException("Resource not found", resourceName);
        
        var stream = new InMemoryRandomAccessStream();

        await using var writeStream = stream.AsStreamForWrite();
        await manifestResourceStream.CopyToAsync(writeStream);
        
        return new InMemoryRandomAccessStreamWithContentType(stream, contentType);
    }

}

public sealed class InMemoryRandomAccessStreamWithContentType(
    InMemoryRandomAccessStream randomAccessStream,
    string contentType) : IRandomAccessStreamWithContentType
{
    public void Dispose()
    {
        randomAccessStream.Dispose();
    }

    public IAsyncOperationWithProgress<IBuffer, uint> ReadAsync(IBuffer buffer, uint count, InputStreamOptions options) => 
        randomAccessStream.ReadAsync(buffer, count, options);

    public IAsyncOperationWithProgress<uint, uint> WriteAsync(IBuffer buffer) =>
        randomAccessStream.WriteAsync(buffer);

    public IAsyncOperation<bool> FlushAsync() => randomAccessStream.FlushAsync(); 

    public IInputStream GetInputStreamAt(ulong position) => randomAccessStream.GetInputStreamAt(position);

    public IOutputStream GetOutputStreamAt(ulong position) => randomAccessStream.GetOutputStreamAt(position);

    public void Seek(ulong position) => randomAccessStream.Seek(position);

    public IRandomAccessStream CloneStream() => randomAccessStream.CloneStream();

    public bool CanRead => randomAccessStream.CanRead;
    public bool CanWrite => randomAccessStream.CanWrite;
    public ulong Position => randomAccessStream.Position;
    public ulong Size
    {
        get => randomAccessStream.Size;
        set => randomAccessStream.Size = value;
    }
    public string ContentType => contentType;
}