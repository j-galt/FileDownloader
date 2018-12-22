using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDownloader.Interfaces
{
    public interface IWebClientFactory
    {
        IWebClient Create();
    }
}