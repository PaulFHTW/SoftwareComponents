using System.Text;
using ImageMagick;
using Microsoft.Extensions.Options;
using Tesseract;
using ILogger = Logging.ILogger;

namespace NPaperless.OCRLibrary;

public class OcrClient : IOcrClient
{
    private readonly string _tessDataPath;
    private readonly string _language;
    
    private readonly ILogger _logger;
    private readonly TesseractEngine _tesseractEngine;
 
    public OcrClient(IConfiguration configuration, ILogger logger)
    {
        _tessDataPath = configuration["OcrOptions:TessDataPath"] ?? "./tessdata";
        _language = configuration["OcrOptions:Language"] ?? "eng";
        
        _logger = logger;
        _logger.Info("Starting tesseract engine...");
        _tesseractEngine = new TesseractEngine(_tessDataPath, _language, EngineMode.Default);
    }
    
    public string OcrPdf(Stream pdfStream)
    {
        var stringBuilder = new StringBuilder();
        
        using (var magickImages = new MagickImageCollection())
        {
            var magickSettings = new MagickReadSettings
            {
                Density = new Density(300),
                Format = MagickFormat.Pdf
            };

            _logger.Info("Converting PDF to image(s)...");
            try
            {
                magickImages.Read(pdfStream, magickSettings);
            }
            catch (Exception ex)
            {
                _logger.Error("Could not convert PDF to image(s)!");
                _logger.Error(ex.Message);
            }

            _logger.Info("Done converting PDF to image(s)!");
            foreach (var magickImage in magickImages)
            {
                magickImage.Quality = 100;
                magickImage.Format = MagickFormat.Png;
                
                // Perform OCR
                _logger.Info("Performing image processing...");
                using (var page = _tesseractEngine.Process(Pix.LoadFromMemory(magickImage.ToByteArray())))//byte[] image
                {
                    var extractedText = page.GetText();
                    stringBuilder.Append(extractedText);
                }

                _logger.Info("Done!");
            }
        }

        return stringBuilder.ToString();
    }

    public void Dispose()
    {
        _tesseractEngine.Dispose();
    }
}
