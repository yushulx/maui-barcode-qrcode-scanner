using Com.Dynamsoft.Dbr;
using SkiaSharp;

namespace BarcodeQrScanner.Services
{
    public class DBRLicenseVerificationListener : Java.Lang.Object, IDBRLicenseVerificationListener
    {
        public void DBRLicenseVerificationCallback(bool isSuccess, Java.Lang.Exception error)
        {
            if (!isSuccess)
            {
                System.Console.WriteLine(error.Message);
            }
        }
    }

    public partial class BarcodeQRCodeService
    {
        BarcodeReader reader;

        public partial void InitSDK(string license)
        {
            BarcodeReader.InitLicense(license, new DBRLicenseVerificationListener());
            reader = new BarcodeReader();
        }

        public partial BarcodeQrData[] DecodeFile(string filePath)
        {
            BarcodeQrData[] output = null;
            try
            {
                PublicRuntimeSettings settings = reader.RuntimeSettings;
                settings.ExpectedBarcodesCount = 512;
                reader.UpdateRuntimeSettings(settings);
                TextResult[] results = reader.DecodeFile(filePath);
                if (results != null && results.Length > 0)
                {
                    output = new BarcodeQrData[results.Length];
                    int index = 0;
                    foreach (TextResult result in results)
                    {
                        BarcodeQrData data = new BarcodeQrData();
                        data.text = result.BarcodeText;
                        data.format = result.BarcodeFormatString;
                        LocalizationResult localizationResult = result.LocalizationResult;
                        data.points = new SKPoint[localizationResult.ResultPoints.Count];
                        int pointsIndex = 0;
                        foreach (Com.Dynamsoft.Dbr.Point point in localizationResult.ResultPoints)
                        {
                            SKPoint p = new SKPoint();
                            p.X = point.X;
                            p.Y = point.Y;
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
