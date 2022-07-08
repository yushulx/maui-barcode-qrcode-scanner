using SkiaSharp;

namespace BarcodeQrScanner.Services
{
    public class BarcodeQrData
    {
        public string text;
        public string format;
        public SKPoint[] points;
    }

    public partial class BarcodeQRCodeService
    {
        public partial void InitSDK(string license);
        public partial BarcodeQrData[] DecodeFile(string filePath);
    }
}
