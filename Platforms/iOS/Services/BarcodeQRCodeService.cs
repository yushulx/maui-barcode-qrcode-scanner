using Foundation;
using SkiaSharp;
using DBRiOS;

namespace BarcodeQrScanner.Services
{
    public class Listener : DBRLicenseVerificationListener
    {
        public void DBRLicenseVerificationCallback(bool isSuccess, NSError error)
        {
            if (error != null)
            {
                System.Console.WriteLine(error.UserInfo);
            }
        }
    }

    public partial class BarcodeQRCodeService
    {
        DynamsoftBarcodeReader reader;

        public partial void InitSDK(string license)
        {
            DynamsoftBarcodeReader.InitLicense(license, new Listener());
            reader = new DynamsoftBarcodeReader();
        }

        public partial BarcodeQrData[] DecodeFile(string filePath)
        {
            BarcodeQrData[] output = null;
            try
            {
                NSError error;

                iPublicRuntimeSettings settings = reader.GetRuntimeSettings(out error);
                settings.ExpectedBarcodesCount = 512;
                reader.UpdateRuntimeSettings(settings, out error);

                iTextResult[] results = reader.DecodeFileWithName(filePath, out error);
                if (results != null && results.Length > 0)
                {
                    output = new BarcodeQrData[results.Length];
                    int index = 0;
                    foreach (iTextResult result in results)
                    {
                        BarcodeQrData data = new BarcodeQrData();
                        data.text = result.BarcodeText;
                        data.format = result.BarcodeFormatString;
                        iLocalizationResult localizationResult = result.LocalizationResult;
                        data.points = new SKPoint[localizationResult.ResultPoints.Length];
                        int pointsIndex = 0;
                        foreach (NSObject point in localizationResult.ResultPoints)
                        {
                            SKPoint p = new SKPoint();
                            p.X = (float)((NSValue)point).CGPointValue.X;
                            p.Y = (float)((NSValue)point).CGPointValue.Y;
                            data.points[pointsIndex++] = p;
                        }
                        output[index++] = data;
                    }
                }
            }
            catch (Exception e)
            {
            }

            return output;
        }

    }
}
