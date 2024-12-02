using System.Text;
using ImageMagick;
using Microsoft.Extensions.Options;
using Tesseract;

namespace NPaperless.OCRLibrary;

public class OcrClient : IOcrClient
{
    private readonly string tessDataPath;
    private readonly string language;
    public OcrClient(OcrOptions options)
    {
        this.tessDataPath = options.TessDataPath;
        this.language = options.Language;
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

            Console.WriteLine("Converting PDF to image(s)...");
            try
            {
                magickImages.Read(pdfStream, magickSettings);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not convert PDF to image(s)!");
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine("Done converting PDF to image(s)!");
            foreach (var magickImage in magickImages)
            {
                magickImage.Quality = 100;
                magickImage.Format = MagickFormat.Png;
                // Set the resolution and format of the image (adjust as needed)
                //magickImage.Density = new Density(1000, 1000);
                //magickImage.Format = MagickFormat.Png;

                // Perform OCR on the image
                Console.WriteLine("Starting tesseract engine...");
                using (var tesseractEngine = new TesseractEngine(tessDataPath, language, EngineMode.Default))
                {
                    Console.WriteLine("Performing image processing...");
                    using (var page = tesseractEngine.Process(Pix.LoadFromMemory(magickImage.ToByteArray())))//byte[] image
                    {
                        var extractedText = page.GetText();
                        stringBuilder.Append(extractedText);
                    }

                    Console.WriteLine("Done!");
                }
            }
        }

        return stringBuilder.ToString();
    }
}
