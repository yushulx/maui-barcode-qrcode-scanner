using AVFoundation;
using BarcodeQrScanner.Services;
using CoreFoundation;
using CoreMedia;
using CoreVideo;
using Foundation;
using SkiaSharp;
using DBRiOS;

namespace BarcodeQrScanner.Platforms.iOS
{
    class CaptureOutput : AVCaptureVideoDataOutputSampleBufferDelegate
    {
        public DynamsoftBarcodeReader reader;
        public Action update;
        private bool ready = true;
        private DispatchQueue queue = new DispatchQueue("ReadTask", true);
        private NSError errorr;
        private nint bpr;
        public nint width;
        public nint height;
        private NSData buffer;
        public string result = "";
        private iTextResult[] results;
        CameraPreview cameraPreview;
        public BarcodeQrData[] output = null;

        public CaptureOutput(CameraPreview preview)
        {
            cameraPreview = preview;
        }

        public override void DidOutputSampleBuffer(AVCaptureOutput captureOutput, CMSampleBuffer sampleBuffer, AVCaptureConnection connection)
        {
            if (ready)
            {
                ready = false;
                CVPixelBuffer cVPixelBuffer = (CVPixelBuffer)sampleBuffer.GetImageBuffer();

                cVPixelBuffer.Lock(CVPixelBufferLock.ReadOnly);
                nint dataSize = cVPixelBuffer.DataSize;
                width = cVPixelBuffer.Width;
                height = cVPixelBuffer.Height;
                IntPtr baseAddress = cVPixelBuffer.BaseAddress;
                bpr = cVPixelBuffer.BytesPerRow;
                cVPixelBuffer.Unlock(CVPixelBufferLock.ReadOnly);
                buffer = NSData.FromBytes(baseAddress, (nuint)dataSize);
                cVPixelBuffer.Dispose();
                queue.DispatchAsync(ReadTask);
            }
            sampleBuffer.Dispose();
        }

        private void ReadTask()
        {
            output = null;
            if (reader != null)
            {
                results = reader.DecodeBuffer(buffer,
                                            width,
                                            height,
                                            bpr,
                                            EnumImagePixelFormat.Argb8888, out errorr);

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
                else
                {
                    result = "";
                }
            }

            DispatchQueue.MainQueue.DispatchAsync(update);
            ready = true;
        }
    }
}
