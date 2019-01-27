using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;
using FileDownloader.Interfaces;
using System.Net;

namespace BankGovUaFileDownloader.Tests
{
    [TestClass]
    public class BankGovUaFileDownloaderTests
    {
        private Mock<IWebClientFactory> _mockWcf;
        private Mock<IWebClient> _mockWebClient;

        [TestMethod]
        public async Task DownloadFileAsync_Calls_DownloadFileTaskAsync_Three_Times()
        {
            // Arrange
            _mockWcf = new Mock<IWebClientFactory>();
            _mockWebClient = new Mock<IWebClient>();

            _mockWebClient.SetupSequence(wc => wc.DownloadFileTaskAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new WebException("mine"))
                .Throws(new WebException("mine"))
                .Returns(Task.CompletedTask);
            _mockWcf.Setup(wcf => wcf.Create()).Returns(_mockWebClient.Object);

            FileDownloader.BankGovUaFileDownloader fd = new FileDownloader.BankGovUaFileDownloader(_mockWcf.Object);

            // Act
            await fd.DownloadFileAsync(@"https://bank.gov.ua/files/Shareholders/304706/index.html", 
                    "D://Test1.pdf");

            // Assert
            _mockWebClient.Verify(wc => wc.DownloadFileTaskAsync(It.IsAny<string>(), It.IsAny<string>()), 
                Times.Exactly(3));
        }

        [TestMethod]
        public async Task Start_Returns_DownloadResult_With_Dictionary_Of_Failed()
        {
            // Arrange
            _mockWcf = new Mock<IWebClientFactory>();
            _mockWebClient = new Mock<IWebClient>();

            _mockWebClient.Setup(wc => wc.DownloadFileTaskAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new WebException());
            _mockWcf.Setup(wcf => wcf.Create()).Returns(_mockWebClient.Object);

            FileDownloader.BankGovUaFileDownloader fd = new FileDownloader.BankGovUaFileDownloader(_mockWcf.Object);

            // Act
            var res = await fd.Start(@"https://bank.gov.ua/files/Shareholders/304706/index.html", "D://Test1/");

            // Assert
            Assert.IsNotNull(res.FailedToDownload);
        }
    }
}
