﻿using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FileDownloader.Interfaces;
using System.Threading;
using System.Net;

namespace FileDownloader
{
    public class BankGovUaFileDownloader
    {
        private static readonly string _subPageUri = @"files/Shareholders/([\d]+)/index.html";
        private static readonly string _fileUri = @".*\.(pdf|PDF)";
        private static readonly string _tagAndSpace = @"(<[^>]*>)|(\t|\n|\r|\s)";
        private IWebClientFactory _webClientFactory;
        private DownloadResult _result;
        private int _filesCount;

        public BankGovUaFileDownloader(IWebClientFactory webClientFactory)
        {
            _webClientFactory = webClientFactory;
            _result = new DownloadResult();
        }

        public async Task<DownloadResult> Start(string uri, string localPath)
        {
            _filesCount = 0;

            await DownloadAllFilesAsync(uri, localPath);

            _result.NumberOfDownloadedFiles = _filesCount;
            return _result;
        }

        private async Task DownloadAllFilesAsync(string uri, string localPath)
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
                    var name = Regex.Replace(link.InnerHtml, _tagAndSpace, "");
                    pageUris[hrefValue] = name;
                }

                if (fileRx.IsMatch(hrefValue))
                {
                    var name = Regex.Replace(link.InnerHtml, _tagAndSpace, "");
                    fileUris[hrefValue] = name;
                }                
            }
                           
            foreach (var pageUri in pageUris)
            {
                string fullUri = @"https://bank.gov.ua/" + pageUri.Key;
                await DownloadAllFilesAsync(fullUri, localPath + pageUri.Value);
            }

            foreach (var fileUri in fileUris)
            {
                int i = uri.LastIndexOf('/');

                if (i > 0)
                {
                    string fullUri = uri.Substring(0, i + 1) + fileUri.Key;

                    try
                    {
                        await DownloadFileAsync(fullUri, localPath, fileUri.Value);
                        Interlocked.Increment(ref _filesCount);
                    }
                    catch (Exception e)
                    {
                        _result.FailedToDownload[fullUri] = e;
                    }
                }
            }
        }

        public async Task DownloadFileAsync(string fileUri, string localPath, string fileName)
        {
            bool exist = Directory.Exists(localPath);

            if (!exist)
            {
                Directory.CreateDirectory(localPath);
            }

            IWebClient wc = _webClientFactory.Create();

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    await wc.DownloadFileTaskAsync(fileUri, localPath + '/' + fileName + ".pdf");
                    break;
                }
                catch (WebException)
                {
                    if (i == 2)
                    {
                        throw;
                    }

                    await Task.Delay(1000);
                }
                catch (Exception)
                {
                    throw;
                }               
            }
        }
    }
}
