using Dynamsoft.Core.Maui;
using Dynamsoft.CaptureVisionRouter.Maui;
using Dynamsoft.BarcodeReader.Maui;
using Dynamsoft.CameraEnhancer.Maui;
using System.Diagnostics;
using CommunityToolkit.Mvvm.Messaging;

namespace BarcodeQrScanner;

public partial class CameraPage : ContentPage, ICapturedResultReceiver, ICompletionListener
{
    private CameraEnhancer? enhancer = null;
    private CaptureVisionRouter router;
    private float previewWidth = 0;
    private float previewHeight = 0;

    public CameraPage()
    {
        InitializeComponent();
        enhancer = new CameraEnhancer();
        router = new CaptureVisionRouter();
        router.SetInput(enhancer);
        router.AddResultReceiver(this);

        WeakReferenceMessenger.Default.Register<LifecycleEventMessage>(this, (r, message) =>
        {
            if (message.EventName == "Resume")
            {
                if (this.Handler != null && enhancer != null)
                {
                    enhancer.Open();
                }
            }
            else if (message.EventName == "Stop")
            {
                enhancer?.Close();
            }
        });
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        if (this.Handler != null && enhancer != null)
        {
            enhancer.SetCameraView(CameraPreview);
            enhancer.Open();
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Permissions.RequestAsync<Permissions.Camera>();
        router?.StartCapturing(EnumPresetTemplate.PT_READ_BARCODES, this);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        enhancer?.Close();
        router?.StopCapturing();
    }

    public void OnCapturedResultReceived(CapturedResult result)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var drawable = new ImageWithOverlayDrawable(null, previewWidth, previewHeight, false);

            // Set drawable to GraphicsView
            OverlayGraphicsView.Drawable = drawable;
            OverlayGraphicsView.Invalidate();
        });
    }

    public void OnDecodedBarcodesReceived(DecodedBarcodesResult result)
    {
        if (previewWidth == 0 && previewHeight == 0)
        {
            IntermediateResultManager manager = router.GetIntermediateResultManager();
            ImageData data = manager.GetOriginalImage(result.OriginalImageHashId);
            
            // Create a drawable with the barcode results
            previewWidth = (float)data.Width;
            previewHeight = (float)data.Height;
        }

        MainThread.BeginInvokeOnMainThread(() =>
        {
            var drawable = new ImageWithOverlayDrawable(result, previewWidth, previewHeight, false);

            // Set drawable to GraphicsView
            OverlayGraphicsView.Drawable = drawable;
            OverlayGraphicsView.Invalidate();
        });
    }

    public void OnSuccess()
    {
        Debug.WriteLine("success");
    }

    public void OnFailure(int errorCode, string errorMessage)
    {
        Debug.WriteLine(errorMessage);
    }

    private void OnImageSizeChanged(object sender, EventArgs e)
    {
        // Adjust the GraphicsView size to match the Image size
        OverlayGraphicsView.WidthRequest = CameraPreview.Width;
        OverlayGraphicsView.HeightRequest = CameraPreview.Height;
    }
}
