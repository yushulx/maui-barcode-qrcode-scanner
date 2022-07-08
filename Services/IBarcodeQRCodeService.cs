using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace BarcodeQrScanner.Services
{
    public class BarcodeQrData
    {
        public string text;
        public string format;
        public SKPoint[] points;
    }

    public interface IBarcodeQRCodeService
    {
        Task<int> InitSDK(string license);
        Task<BarcodeQrData[]> DecodeFile(string filePath);
    }
}
