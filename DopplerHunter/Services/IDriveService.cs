using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DopplerHunter.Services
{
    public interface IDriveService
    {
        DriveInfo[] GetDrives();
    }
}
