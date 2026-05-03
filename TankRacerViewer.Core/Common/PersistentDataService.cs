using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace TankRacerViewer.Core
{
    public sealed class PersistentDataService
    {
        private readonly IPlatformStorage _platformStorage;

        private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = [];

        public PersistentDataService(IPlatformStorage platformStorage)
        {
            _platformStorage = platformStorage;
        }

        public async Task SaveAsync<T>(string key, T data,
            JsonSerializerOptions jsonSerializerOptions,
            CancellationToken cancellationToken = default)
        {
            var serializedBytes = JsonSerializer.SerializeToUtf8Bytes(data, jsonSerializerOptions);

            var semaphoreSlim = GetSemaphoreSlim(key);
            await semaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                await _platformStorage.WriteAsync(key, serializedBytes, cancellationToken)
                    .ConfigureAwait(false);
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public async Task<T> LoadAsync<T>(string key,
            JsonSerializerOptions jsonSerializerOptions,
            CancellationToken cancellationToken = default)
        {
            var semaphoreSlim = GetSemaphoreSlim(key);
            await semaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                if (!await _platformStorage.ExistsAsync(key, cancellationToken).ConfigureAwait(false))
                    return default;

                var result = await _platformStorage.ReadAsync(key, cancellationToken)
                    .ConfigureAwait(false);

                var data = JsonSerializer.Deserialize<T>(result, jsonSerializerOptions);
                if (data is null)
                    return default;

                return data;
            }
            catch (JsonException exception)
            {
                Debug.WriteLine(exception.Message);
                return default;
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        private SemaphoreSlim GetSemaphoreSlim(string key)
            => _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
    }
}
