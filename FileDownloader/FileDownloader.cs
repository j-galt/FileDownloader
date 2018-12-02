using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebFileDownloader
{
    public class FileDownloader
    {
        private string _uri;
        private readonly string _subPageUri = @"(files/Shareholders/([\d]+)/index.html)";
        private readonly string _fileUri = @"""(.*\.(pdf|PDF))"".*\>[\s]*([\d]+\.[\d]+\.[\d]+)";

        public FileDownloader(string uri)
        {
            _uri = uri;
        }

        public async Task Start()
        {
            WebClient wc = new WebClient();
            await DownloadAllFiles(_uri, @"D:\Test1", wc);
        }

        private async Task DownloadAllFiles(string uri, string localPath, WebClient wc)
        {
            string str;

            try
            {
                var data = await wc.OpenReadTaskAsync(new Uri(uri));

                using (StreamReader sr = new StreamReader(data))
                {
                    str = await sr.ReadToEndAsync();
                }
            }
            catch (Exception e)
            {
                throw;
            }

            Regex rx = new Regex(_subPageUri, RegexOptions.IgnoreCase);
            MatchCollection subPageUris = rx.Matches(str);

            foreach (var subPageUri in subPageUris)
            {
                string strUri = @"https://bank.gov.ua/" + ((Match)subPageUri).Groups[1].Value;
                await DownloadAllFiles(strUri, localPath + '/' + ((Match)subPageUri).Groups[2].Value, wc);
            }

            rx = new Regex(_fileUri, RegexOptions.IgnoreCase);
            MatchCollection fileUris = rx.Matches(str);

            foreach (var fileUri in fileUris)
            {
                string file = ((Match)fileUri).Groups[1].Value;
                string fileName = ((Match)fileUri).Groups[2].Value;
                int i = uri.LastIndexOf('/');

                if (i > 0)
                {
                    string strUri = uri.Substring(0, i + 1) + file;
                    await DownloadFileAsync(strUri, localPath, fileName);
                }
            }
        }

        private async Task DownloadFileAsync(string fileUri, string localPath, string fileName) 
        {
            bool exist = Directory.Exists(localPath);

            if (!exist)
            {
                Directory.CreateDirectory(localPath);
            }
            
            WebClient wc = new WebClient();
            await wc.DownloadFileTaskAsync(fileUri, localPath + '/' + fileName + ".pdf");        
        }
    }
}
