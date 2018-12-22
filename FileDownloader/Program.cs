using System;
using System.Linq;
using FileDownloader.Interfaces;

namespace FileDownloader
{
    class Program
    {
        static void Main(string[] args)
        {
            IWebClientFactory wcf = new WebClientFactory();
            BankGovUaFileDownloader fd = new BankGovUaFileDownloader(wcf);

            var awaiter = fd.Start(@"https://bank.gov.ua/control/uk/publish/article?art_id=6738234&cat_id=51342",
                @"D:\\Test\")
                .ConfigureAwait(false)
                .GetAwaiter();

            Console.WriteLine("Downloading...");

            var res = awaiter.GetResult();

            Console.WriteLine("Number of successfully downloaded files: {0}", res.NumberOfDownloadedFiles);
            Console.WriteLine("Failed to download: {0}", res.FailedToDownload.Count());

            foreach (var file in res.FailedToDownload)
                Console.WriteLine(file.Key);
        }
    }
}
