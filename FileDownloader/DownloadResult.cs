using System;
using System.Collections.Generic;

namespace FileDownloader
{
    public class DownloadResult
    {
        public DownloadResult()
        {
            NumberOfDownloadedFiles = 0;
            FailedToDownload = new Dictionary<string, Exception>();
        }

        public int NumberOfDownloadedFiles { get; set; }
        public Dictionary<string, Exception> FailedToDownload { get; set; }
    }
}
