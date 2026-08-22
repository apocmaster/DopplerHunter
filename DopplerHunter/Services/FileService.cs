using DopplerHunter.Events;
using DopplerHunter.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Hashing;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;

namespace DopplerHunter.Services
{
    public class FileService : IFileService
    {

        public event EventHandler<HashesCalculatedEventArgs>? HashesCalculated;


        protected virtual void OnHashesCalculated(int hashesCalculated) => HashesCalculated?.Invoke(this, new HashesCalculatedEventArgs(hashesCalculated));

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

        private async Task<string> ComputeSampleHash(string filePath, int sampleSize)
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var buffer = new byte[sampleSize * 2];

            // Primeros bytes
            int readStart = await stream.ReadAsync(buffer.AsMemory(0, sampleSize));

            // Últimos bytes
            stream.Seek(-sampleSize, SeekOrigin.End);
            int readEnd = await stream.ReadAsync(buffer.AsMemory(sampleSize, sampleSize));
            var hash = MD5.HashData(buffer.AsSpan(0, readStart + readEnd));

            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        public async Task CalculatePossibleDuplicates(ObservableCollection<FileMetadata> files)
        {
            var possibleDuplicates = files
                        .GroupBy(f => f.FileSize)
                        .Where(g => g.Count() > 1)
                        .SelectMany(g => g);

            int counter = 0;
            foreach (var file in possibleDuplicates)
            {
                var hash = await ComputeHashAsync(file);
                file.FileHash = hash;
                file.IsHashCalculated = true;

                counter++;
                if (counter % 10 == 0)
                {
                    OnHashesCalculated(counter);
                }
            }

            OnHashesCalculated(counter);
        }

        public async Task MarkFilesDuplicates(ObservableCollection<FileMetadata> files)
        {
            var duplicates = files
                    .Where(f => f.IsHashCalculated && !string.IsNullOrEmpty(f.FileHash))
                    .GroupBy(f => f.FileHash)
                    .Where(g => g.Count() > 1)
                    .SelectMany(g => g)
                    ;

            foreach (var file in duplicates)
            {
                file.IsFileDuplicated = true;                
            }

            OnHashesCalculated(duplicates.Count());
        }

        public async Task GroupFilesDuplicated(ObservableCollection<FileMetadata> files)
        {
            var grouped = files
                .Where(f => f.IsFileDuplicated)
                .GroupBy(f => f.FileHash)
                .Select(g => g.OrderBy(f => f.FolderPath))
                ;

            foreach (var group in grouped)
            {
                int index = 1;
                foreach (var file in group)
                {
                    file.DuplicateIndex = index++;
                }
            }
        }

        private Task<string> ComputeHashAsync(FileMetadata file)
        {
            const long FiftyMB = 50 * 1024 * 1024;
            const long FiveHundredMB = 500 * 1024 * 1024;

            return file.FileSize switch
            {
                < FiftyMB => ComputeMD5(file.FullPath),       // Archivos pequeños
                < FiveHundredMB => ComputeXXHash(file.FullPath),    // Archivos medianos
                _ => ComputeSampleHash(file.FullPath, 1024)        // Archivos muy grandes
            };
        }
    }
}
