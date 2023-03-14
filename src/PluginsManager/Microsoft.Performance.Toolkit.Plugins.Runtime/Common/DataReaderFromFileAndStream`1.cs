using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.Toolkit.Plugins.Core.Serialization;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Common
{
    public class DataReaderFromFileAndStream<TEntity>
        : IDataReaderFromFileAndStream<TEntity>
    {
        private readonly ISerializer serializer;

        public DataReaderFromFileAndStream(ISerializer serializer)
        {
            this.serializer = serializer;
        }

        public bool CanReadData(Stream source)
        {
            return source.CanRead;
        }

        public bool CanReadData(FileInfo source)
        {
            return File.Exists(source.FullName);
        }

        public TEntity ReadData(Stream source)
        {
            return this.serializer.Deserialize<TEntity>(source);
        }
        
        public TEntity ReadData(FileInfo source)
        {
            using (var stream = new FileStream(source.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return ReadData(stream);
            }
        }

        public Task<TEntity> ReadDataAsync(Stream source, CancellationToken cancellationToken)
        {
            return this.serializer.DeserializeAsync<TEntity>(source, cancellationToken);
        }

        public Task<TEntity> ReadDataAsync(FileInfo source, CancellationToken cancellationToken)
        {
            using (var stream = new FileStream(source.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return ReadDataAsync(stream, cancellationToken);
            }
        }
    }
}
