#nullable enable

using System.Threading;
using System.Threading.Tasks;

namespace TankRacerViewer.Core
{
    public interface IPlatformStorage
    {
        public Task WriteAsync(string key, byte[] bytes,
            CancellationToken cancellationToken = default);

        public Task<byte[]?> ReadAsync(string key,
            CancellationToken cancellationToken = default);

        public Task<bool> ExistsAsync(string key,
            CancellationToken cancellationToken = default);
    }
}
