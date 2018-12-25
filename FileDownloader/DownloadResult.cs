using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace FileDownloader
{
    public class DownloadResult
    {
        public DownloadResult()
        {
            FailedToDownload = new ConcurrentDictionary<string, Exception>();
        }

        public int NumberOfDownloadedFiles { get; set; }
        public ConcurrentDictionary<string, Exception> FailedToDownload { get; set; }
    }
}
