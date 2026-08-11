using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DopplerHunter.Services
{
    public class DriveService : IDriveService
    {
        public DriveInfo[] GetDrives()
        {
            return DriveInfo.GetDrives();
        }
    }
}
