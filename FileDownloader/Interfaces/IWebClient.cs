using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDownloader.Interfaces
{
    public interface IWebClient : IDisposable
    {
        Task DownloadFileTaskAsync(string address, string fileName);
    }
}