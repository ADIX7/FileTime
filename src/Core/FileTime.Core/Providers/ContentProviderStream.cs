using System.Threading.Tasks;
namespace FileTime.Core.Providers
{
    public class ContentProviderStream : Stream
    {

        private readonly IContentReader? _contentReader;
        private readonly IContentWriter? _contentWriter;
        public override bool CanRead => _contentReader == null;

        public override bool CanSeek => false;

        public override bool CanWrite => _contentWriter == null;

        public override long Length => throw new NotImplementedException();

        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ContentProviderStream(IContentReader contentReader)
        {
            _contentReader = contentReader;
        }

        public ContentProviderStream(IContentWriter contentWriter)
        {
            _contentWriter = contentWriter;
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_contentReader == null) throw new IOException("This stream is not readable");
            var dataTask = Task.Run(async () =>  await _contentReader.ReadBytesAsync(count, offset));
            dataTask.Wait();
            var data = dataTask.Result;

            if (data.Length > count) throw new Exception("More bytes has been read than requested");
            Array.Copy(data, buffer, data.Length);
            return data.Length;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}