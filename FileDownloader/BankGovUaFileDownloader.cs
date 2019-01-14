using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FileDownloader.Interfaces;
using System.Net;
using System.Threading;

namespace FileDownloader
{
    public class BankGovUaFileDownloader
    {
        private static readonly string _subPageUri = @"files/Shareholders/([\d]+)/index.html";
        private static readonly string _fileUri = @".*\.(pdf|PDF)";
        private static readonly string _domain = @"https://bank.gov.ua/";
        private IWebClientFactory _webClientFactory;
        private DownloadResult _result;
        private string _localPath;
        private int _filesCount;

        public BankGovUaFileDownloader(IWebClientFactory webClientFactory)
        {
            _webClientFactory = webClientFactory;
            _result = new DownloadResult();
        }

        public async Task<DownloadResult> Start(string uri, string localPath)
        {
            _filesCount = 0;
            _localPath = localPath;

            if (!Directory.Exists(localPath))
            {
                Directory.CreateDirectory(localPath);
            }
            
            await DownloadAllFilesAsync(uri);

            _result.NumberOfDownloadedFiles = _filesCount;
            return _result;
        }

        private async Task DownloadAllFilesAsync(string uri)
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

            if (hw.StatusCode == HttpStatusCode.NotFound)
            {
                _result.FailedToDownload[uri] = new ApplicationException(((int)HttpStatusCode.NotFound).ToString());
                return;
            }

            Regex pageRx = new Regex(_subPageUri, RegexOptions.IgnoreCase);
            Regex fileRx = new Regex(_fileUri, RegexOptions.IgnoreCase);
            List<string> pageUris = new List<string>();
            List<string> fileUris = new List<string>();

            HtmlNodeCollection ankers = hd.DocumentNode.SelectNodes("//a[@href]");
            if (ankers != null && ankers.Count > 0)
            {
                foreach (var link in ankers)
                {
                    string hrefValue = link.GetAttributeValue("href", string.Empty);

                    if (pageRx.IsMatch(hrefValue))
                    {
                        pageUris.Add(hrefValue);
                    }

                    if (fileRx.IsMatch(hrefValue))
                    {
                        fileUris.Add(hrefValue);
                    }
                }

            }

            foreach (var pageUri in pageUris)
            {
                await DownloadAllFilesAsync(_domain + pageUri);
            }

            foreach (var fileUri in fileUris)
            {
                int i = uri.LastIndexOf('/');

                if (i > 0)
                {
                    string absoluteUri = uri.Substring(0, i + 1) + fileUri;
                    string currSave2Path = Path.Combine(_localPath, fileUri);
                    try
                    {
                        if (!File.Exists(currSave2Path))
                            await DownloadFileAsync(absoluteUri, currSave2Path);
                        Interlocked.Increment(ref _filesCount);
                    }
                    catch (Exception e)
                    {
                        _result.FailedToDownload[absoluteUri] = e;
                    }
                }
            }
        }

        public async Task DownloadFileAsync(string absoluteUri, string fileName)
        {
            IWebClient wc = _webClientFactory.Create();

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    await wc.DownloadFileTaskAsync(absoluteUri, fileName);
                    break;
                }
                catch (DirectoryNotFoundException)
                {
                    throw;
                }
                catch (WebException)
                {
                    if (i == 2)
                    {
                        throw;
                    }

                    await Task.Delay(1000);
                }
            }
        }
    }
}
