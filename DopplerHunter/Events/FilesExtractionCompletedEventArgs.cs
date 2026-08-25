using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DopplerHunter.Events
{
    public class FilesExtractionCompletedEventArgs(List<FileInfo> files) : EventArgs
    {
        public int FilesExtracted = files.Count;
    }
}
