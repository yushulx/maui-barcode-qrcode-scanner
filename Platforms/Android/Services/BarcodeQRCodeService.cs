using BarcodeQrScanner.Platforms.Android.Services;
using Com.Dynamsoft.Dbr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BarcodeQrScanner.Services;
using SkiaSharp;

[assembly: Dependency(typeof(BarcodeQRCodeService))]
namespace BarcodeQrScanner.Platforms.Android.Services
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

    public class BarcodeQRCodeService : IBarcodeQRCodeService
    {
        BarcodeReader reader;

        Task<int> IBarcodeQRCodeService.InitSDK(string license)
        {
            BarcodeReader.InitLicense(license, new DBRLicenseVerificationListener());
            reader = new BarcodeReader();
            TaskCompletionSource<int> taskCompletionSource = new TaskCompletionSource<int>();
            taskCompletionSource.SetResult(0);
            return taskCompletionSource.Task;
        }

        Task<BarcodeQrData[]> IBarcodeQRCodeService.DecodeFile(string filePath)
        {
            BarcodeQrData[] output = null;
            try
            {
                PublicRuntimeSettings settings = reader.RuntimeSettings;
                settings.ExpectedBarcodesCount = 0;
                reader.UpdateRuntimeSettings(settings);
                TextResult[] results = reader.DecodeFile(filePath);
                if (results != null)
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
            }

            TaskCompletionSource<BarcodeQrData[]> taskCompletionSource = new TaskCompletionSource<BarcodeQrData[]>();
            taskCompletionSource.SetResult(output);
            return taskCompletionSource.Task;
        }
    }
}
