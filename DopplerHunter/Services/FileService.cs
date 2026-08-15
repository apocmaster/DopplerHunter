using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.IO.Hashing;

namespace DopplerHunter.Services
{
    public class FileService : IFileService
    {
        public async Task<string> OpenFileAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return ("File path is empty.");

            if (IsFileNotExists(filePath))
                return ($"File does not exist: {filePath}");

            try
            {
                await Task.Run(() =>
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = filePath,
                        UseShellExecute = true
                    });
                });

                return ($"Opened file: {filePath}");
            }
            catch (Exception ex)
            {
                return ($"Error opening file: {ex.Message}");
            }
        }

        public bool IsFileNotExists(string filePath)
        {
            return !IsFileExists(filePath);
        }

        public bool IsFileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        public async Task<string> ComputeMD5(string filePath)
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(filePath);
            var hash = md5.ComputeHash(stream);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        public async Task<string> ComputeXXHash(string filePath)
        {
            using var stream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 81920,
                useAsync: true);

            var xxHash = new XxHash64();

            // Procesa el archivo en bloques
            var buffer = new byte[81920];
            int bytesRead;
            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                xxHash.Append(buffer.AsSpan(0, bytesRead));
            }

            // Finaliza el cálculo
            var hash = xxHash.GetHashAndReset();

            return Convert.ToHexString(hash).ToLowerInvariant();
        }
    }
}
