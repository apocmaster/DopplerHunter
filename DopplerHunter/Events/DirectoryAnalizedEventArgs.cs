using System.IO;

namespace DopplerHunter.Events
{
    public class DirectoryAnalizedEventArgs(int directoriesAnalized) : EventArgs
    {
        public int DirectoriesAnalized { get; } = directoriesAnalized;
    }
}
