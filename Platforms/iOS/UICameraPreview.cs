using AVFoundation;
using CoreFoundation;
using CoreVideo;
using Foundation;
using UIKit;
using DBRiOS;

namespace BarcodeQrScanner.Platforms.iOS
{
    public class UICameraPreview : UIView
    {
        AVCaptureVideoPreviewLayer previewLayer;
        CameraOptions cameraOptions;
        private CaptureOutput captureOutput;
        DynamsoftBarcodeReader reader = new DynamsoftBarcodeReader();

        public event EventHandler<EventArgs> Tapped;
        CameraPreview cameraPreview;
        public AVCaptureSession CaptureSession { get; private set; }

        public bool IsPreviewing { get; set; }

        public UICameraPreview(CameraPreview preview)
        {
            cameraPreview = preview;
            captureOutput = new CaptureOutput(cameraPreview);
            cameraOptions = cameraPreview.Camera;
            IsPreviewing = false;
            Initialize();
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            if (previewLayer != null)
                previewLayer.Frame = Bounds;
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);
            OnTapped();
        }

        protected virtual void OnTapped()
        {
            var eventHandler = Tapped;
            if (eventHandler != null)
            {
                eventHandler(this, new EventArgs());
            }
        }

        void Initialize()
        {
            CaptureSession = new AVCaptureSession();
            previewLayer = new AVCaptureVideoPreviewLayer(CaptureSession)
            {
                Frame = Bounds,
                VideoGravity = AVLayerVideoGravity.ResizeAspectFill
            };
            AVCaptureDevice[] videoDevices = AVCaptureDevice.Devices;
            //var videoDevices = AVCaptureDevice.DevicesWithMediaType(AVMediaTypes.Video.ToString()); // return null
            var cameraPosition = (cameraOptions == CameraOptions.Front) ? AVCaptureDevicePosition.Front : AVCaptureDevicePosition.Back;
            var device = videoDevices.FirstOrDefault(d => d.Position == cameraPosition);

            if (device == null)
            {
                return;
            }

            NSError error;

            iPublicRuntimeSettings settings = reader.GetRuntimeSettings(out error);
            settings.ExpectedBarcodesCount = (cameraPreview.ScanMode == ScanOptions.Single) ? 1 : 512;
            reader.UpdateRuntimeSettings(settings, out error);

            var input = new AVCaptureDeviceInput(device, out error);
            CaptureSession.AddInput(input);
            var videoDataOutput = new AVCaptureVideoDataOutput()
            {
                AlwaysDiscardsLateVideoFrames = true
            };
            if (CaptureSession.CanAddOutput(videoDataOutput))
            {
                CaptureSession.AddOutput(videoDataOutput);
                captureOutput.reader = reader;
                captureOutput.update = UpdateResults;

                DispatchQueue queue = new DispatchQueue("camera");
                videoDataOutput.SetSampleBufferDelegate(captureOutput, queue);
                videoDataOutput.WeakVideoSettings = new NSDictionary<NSString, NSObject>(CVPixelBuffer.PixelFormatTypeKey, NSNumber.FromInt32((int)CVPixelFormatType.CV32BGRA));
            }
            CaptureSession.CommitConfiguration();

            Layer.AddSublayer(previewLayer);
            CaptureSession.StartRunning();
            IsPreviewing = true;
        }

        void UpdateResults()
        {
            cameraPreview.NotifyResultReady(captureOutput.output, (int)captureOutput.width, (int)captureOutput.height);
        }
    }
}
