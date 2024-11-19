namespace NPaperless.OCRLibrary;

public interface IOcrClient
{
    string OcrPdf(Stream pdfStream);
}
