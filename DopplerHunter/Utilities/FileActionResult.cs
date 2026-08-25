using System;
using System.Collections.Generic;
using System.Text;

namespace DopplerHunter.Utilities
{
    public class FileActionResult
    {
        public enum ResponseAction
        { 
            Succeeded,
            Failed,
            Warning, 
            Error
        }
        public ResponseAction Status { get; set; }
        public string Message { get; set; } = string.Empty;

        private FileActionResult Create(ResponseAction status, string message)
        {
            return new FileActionResult { Status = status, Message = message };
        }

        public FileActionResult Success(string message) => Create(ResponseAction.Succeeded, message);
        public FileActionResult Failed(string message) => Create(ResponseAction.Failed, message);
        public FileActionResult Warning(string message) => Create(ResponseAction.Warning, message);
        public FileActionResult Error(string message) => Create(ResponseAction.Error, message);
    }
}
