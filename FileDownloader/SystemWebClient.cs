using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FileDownloader.Interfaces;

namespace FileDownloader
{
    public class SystemWebClient : WebClient, IWebClient
    {
        public SystemWebClient()
        {
        }
    }
}
