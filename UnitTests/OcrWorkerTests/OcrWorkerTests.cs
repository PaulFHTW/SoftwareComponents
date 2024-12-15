using System.Diagnostics.CodeAnalysis;
using System.Text;
using Moq;
using NUnit.Framework;
using ILogger = Logging.ILogger;
using Moq;

namespace NPaperless.OCRLibrary
{
    [TestFixture]
    class OcrWorkerTests
    {   

        [Setup]
        public void Setup()
        {
           var ocrClient = new OcrClient();
        }

    }
}