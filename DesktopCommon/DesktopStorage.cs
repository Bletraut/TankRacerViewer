using System.Diagnostics;

using TankRacerViewer.Core;

namespace DesktopCommon
{
    public class DesktopStorage : IPlatformStorage
    {
        // Class.
        private readonly string _applicationDirectory;

        public DesktopStorage(string applicationName)
        {
            var root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _applicationDirectory = Path.Combine(root, applicationName);
        }

        public async Task WriteAsync(string key, byte[] bytes,
            CancellationToken cancellationToken = default)
        {
            var path = GetPath(key);

            var tempPath = path + ".tmp";
            await File.WriteAllBytesAsync(tempPath, bytes, cancellationToken)
                .ConfigureAwait(false);

            if (File.Exists(path))
            {
                File.Replace(tempPath, path, null);
            }
            else
            {
                File.Move(tempPath, path);
            }
        }

        public async Task<byte[]?> ReadAsync(string key,
            CancellationToken cancellationToken = default)
        {
            var path = GetPath(key);
            if (!File.Exists(path))
                return null;

            var bytes = await File.ReadAllBytesAsync(path, cancellationToken)
                .ConfigureAwait(false);
            return bytes;
        }

        public Task<bool> ExistsAsync(string key,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(File.Exists(GetPath(key)));
        }

        private string GetPath(string key)
        {
            Directory.CreateDirectory(_applicationDirectory);
            return Path.Combine(_applicationDirectory, key);
        }
    }
}
