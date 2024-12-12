namespace NPaperless.OCRLibrary;

public interface IOcrClient : IDisposable
{
    string OcrPdf(Stream pdfStream);
}
