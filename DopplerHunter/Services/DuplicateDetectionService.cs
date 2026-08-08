//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Security.Cryptography;
//using System.Threading.Tasks;
//using DopplerHunter.Models;

//namespace DopplerHunter.Services
//{
//    /// <summary>
//    /// Servicio para calcular hashes de archivos y detectar duplicados.
//    /// </summary>
//    public class DuplicateDetectionService
//    {
//        /// <summary>
//        /// Calcula el hash SHA256 de un archivo.
//        /// </summary>
//        public async Task<string> CalculateFileHashAsync(string filePath)
//        {
//            try
//            {
//                using (var sha256 = SHA256.Create())
//                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 8192, useAsync: true))
//                {
//                    var hash = await Task.Run(() => sha256.ComputeHash(fileStream));
//                    return BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
//                }
//            }
//            catch (Exception ex)
//            {
//                System.Diagnostics.Debug.WriteLine($"Error calculando hash para {filePath}: {ex.Message}");
//                return null;
//            }
//        }

//        /// <summary>
//        /// Convierte una lista de rutas de archivos a FileMetadata con tamaño.
//        /// </summary>
//        public async Task<List<FileMetadata>> ConvertToFileMetadataAsync(IEnumerable<string> filePaths)
//        {
//            var metadata = new List<FileMetadata>();

//            var tasks = filePaths.Select(async filePath =>
//            {
//                try
//                {
//                    var fileInfo = new FileInfo(filePath);
//                    if (!fileInfo.Exists)
//                        return null;

//                    return new FileMetadata
//                    {
//                        FullPath = filePath,
//                        FileName = fileInfo.Name,
//                        FileSize = fileInfo.Length,
//                        LastModified = fileInfo.LastWriteTime,
//                        IsHashCalculated = false
//                    };
//                }
//                catch (Exception ex)
//                {
//                    System.Diagnostics.Debug.WriteLine($"Error leyendo metadata de {filePath}: {ex.Message}");
//                    return null;
//                }
//            });

//            var results = await Task.WhenAll(tasks);
//            metadata.AddRange(results.Where(m => m != null));

//            return metadata;
//        }

//        /// <summary>
//        /// Agrupa archivos por tamaño (primer filtro rápido).
//        /// </summary>
//        public Dictionary<long, List<FileMetadata>> GroupByFileSize(IEnumerable<FileMetadata> files)
//        {
//            return files
//                .GroupBy(f => f.FileSize)
//                .Where(g => g.Count() > 1) // Solo grupos con duplicados potenciales
//                .ToDictionary(g => g.Key, g => g.ToList());
//        }

//        /// <summary>
//        /// Calcula hashes para archivos en grupos de tamaño similar.
//        /// </summary>
//        public async Task<List<FileMetadata>> CalculateHashesForGroupsAsync(Dictionary<long, List<FileMetadata>> sizeGroups)
//        {
//            var filesWithHash = new List<FileMetadata>();
//            var tasks = new List<Task>();

//            foreach (var group in sizeGroups.Values)
//            {
//                foreach (var file in group)
//                {
//                    tasks.Add(Task.Run(async () =>
//                    {
//                        var hash = await CalculateFileHashAsync(file.FullPath);
//                        if (hash != null)
//                        {
//                            file.FileHash = hash;
//                            file.IsHashCalculated = true;
//                            lock (filesWithHash)
//                            {
//                                filesWithHash.Add(file);
//                            }
//                        }
//                    }));
//                }
//            }

//            await Task.WhenAll(tasks);
//            return filesWithHash;
//        }

//        /// <summary>
//        /// Detecta duplicados basándose en tamaño y hash.
//        /// </summary>
//        //public List<DuplicateGroup> DetectDuplicates(IEnumerable<FileMetadata> filesWithHash)
//        //{
//        //    var duplicateGroups = filesWithHash
//        //        .GroupBy(f => f.DuplicateKey)
//        //        .Where(g => g.Count() > 1) // Solo grupos con más de 1 archivo
//        //        .Select(g => new DuplicateGroup(g.Key, g))
//        //        .OrderByDescending(g => g.TotalWastedSpace)
//        //        .ToList();

//        //    return duplicateGroups;
//        }
//    }
//}
