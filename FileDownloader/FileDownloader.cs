using HtmlAgilityPack;
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
        private readonly string _subPageUri = @"files/Shareholders/([\d]+)/index.html";
        private readonly string _fileUri = @".*\.(pdf|PDF)";

        public FileDownloader(string uri)
        {
            _uri = uri;
        }

        public async Task Start()
        {
            WebClient wc = new WebClient();
            await DownloadAllFiles(_uri, @"D:\Test\", wc);
        }

        private async Task DownloadAllFiles(string uri, string localPath, WebClient wc)
        {
            HtmlWeb hw = new HtmlWeb();
            HtmlDocument hd = new HtmlDocument();

            try
            {
                hd = hw.Load(uri);
            }
            catch (Exception)
            {
                throw;
            }

            Regex pageRx = new Regex(_subPageUri, RegexOptions.IgnoreCase);
            Regex fileRx = new Regex(_fileUri, RegexOptions.IgnoreCase);
            Dictionary<string, string> pageUris = new Dictionary<string, string>();
            Dictionary<string, string> fileUris = new Dictionary<string, string>();

            foreach (var link in hd.DocumentNode.SelectNodes("//a[@href]"))
            {
                string hrefValue = link.GetAttributeValue("href", string.Empty);

                if (pageRx.IsMatch(hrefValue))
                {
                    var name = Regex.Replace(link.InnerHtml, @"(<[^>]*>)|(\t|\n|\r)", "");
                    pageUris[hrefValue] = name;
                }

                if (fileRx.IsMatch(hrefValue))
                {
                    var name = Regex.Replace(link.InnerHtml, @"(<[^>]*>)|(\t|\n|\r)", "").Trim();
                    fileUris[hrefValue] = name;
                }                
            }
                           
            foreach (var pageUri in pageUris)
            {
                string fullUri = @"https://bank.gov.ua/" + pageUri.Key;
                await DownloadAllFiles(fullUri, localPath + pageUri.Value, wc);
            }

            foreach (var fileUri in fileUris)
            {
                int i = uri.LastIndexOf('/');

                if (i > 0)
                {
                    string fullUri = uri.Substring(0, i + 1) + fileUri.Key;
                    await DownloadFileAsync(fullUri, localPath, fileUri.Value);
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

            try
            {
                await wc.DownloadFileTaskAsync(fileUri, localPath + '/' + fileName + ".pdf");
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
