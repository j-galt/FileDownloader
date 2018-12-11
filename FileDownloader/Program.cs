using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebFileDownloader
{
    class Program
    {
        static void Main(string[] args)
        {
            FileDownloader fd = new FileDownloader(@"https://bank.gov.ua/control/uk/publish/article?art_id=6738234&cat_id=51342");
            var awaiter = fd.Start().ConfigureAwait(false).GetAwaiter();

            awaiter.GetResult();
        }
    }
}
