#define __DONT_USE_TASK_RUN__
using System;
using System.Linq;
using FileDownloader.Interfaces;

namespace FileDownloader
{
    class Program
    {
        private static readonly string DEFAULT_START_URL = @"https://bank.gov.ua/control/uk/publish/article?art_id=6738234&cat_id=51342";
        private static readonly string DEFAULT_SAVE2DIR = @"D:\Test\";
        static void Main(string[] args)
        {
            #region parse cmd line args
            string save2Dir = args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]) ? args[0] : DEFAULT_SAVE2DIR;
            string startUrl = args.Length > 1 && !string.IsNullOrWhiteSpace(args[1]) ? args[1] : DEFAULT_START_URL;
            #endregion

            IWebClientFactory wcf = new WebClientFactory();
            BankGovUaFileDownloader fd = new BankGovUaFileDownloader(wcf);
#if __DONT_USE_TASK_RUN__
            Console.Read();
            var awaiter = fd.Start(startUrl,
                save2Dir)
                .ConfigureAwait(false)
                .GetAwaiter();

            Console.WriteLine("Downloading...");

            var res = awaiter.GetResult();
#else
            Console.WriteLine("Downloading...");
            var res = System.Threading.Tasks.Task.Run(() => fd.Start(DEFAULT_START_URL,
               DEFAULT_SAVE2DIR)).Result;
#endif

            Console.WriteLine("Number of successfully downloaded files: {0}", res.NumberOfDownloadedFiles);
            Console.WriteLine("Failed to download: {0}", res.FailedToDownload.Count());

            foreach (var file in res.FailedToDownload)
                Console.WriteLine(file.Key);
        }
    }
}
