using Dynamsoft;
using SkiaSharp;

namespace BarcodeQrScanner.Services
{
    public partial class BarcodeQRCodeService
    {
        private BarcodeQRCodeReader reader;

        public partial void InitSDK(string license)
        {
            
            try
            {   
                Console.WriteLine("GetVersionInfo(): " + BarcodeQRCodeReader.GetVersionInfo());
                BarcodeQRCodeReader.InitLicense(license);
                reader = BarcodeQRCodeReader.Create();
                // Refer to https://www.dynamsoft.com/barcode-reader/parameters/structure-and-interfaces-of-parameters.html?ver=latest
                reader.SetParameters("{\"Version\":\"3.0\", \"ImageParameter\":{\"Name\":\"IP1\", \"BarcodeFormatIds\":[\"BF_QR_CODE\", \"BF_ONED\"], \"ExpectedBarcodesCount\":20}}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public partial BarcodeQrData[] DecodeFile(string filePath)
        {
            if (reader == null)
                return null;

            BarcodeQrData[] output = null;
            try
            {
                BarcodeQRCodeReader.Result[] results = reader.DecodeFile(filePath);
                if (results != null && results.Length > 0)
                {
                    output = new BarcodeQrData[results.Length];
                    int index = 0;
                    foreach (BarcodeQRCodeReader.Result result in results)
                    {
                        BarcodeQrData data = new BarcodeQrData();
                        data.text = result.Text;
                        data.format = result.Format1;
                        data.points = new SKPoint[4];
                        int pointsIndex = 0;
                        for (int i = 0; i < result.Points.Length; i+= 2)
                        {
                            SKPoint p = new SKPoint();
                            p.X = result.Points[i];
                            p.Y = result.Points[i + 1];
                            data.points[pointsIndex++] = p;
                        }
                        output[index++] = data;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return output;
        }
    }
}
