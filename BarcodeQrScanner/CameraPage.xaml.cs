using Dynamsoft.Core.Maui;
using Dynamsoft.CaptureVisionRouter.Maui;
using Dynamsoft.BarcodeReader.Maui;
using Dynamsoft.CameraEnhancer.Maui;
using System.Diagnostics;

namespace BarcodeQrScanner;

public partial class CameraPage : ContentPage, ICapturedResultReceiver, ICompletionListener
{
    public CameraEnhancer? enhancer = null;
    CaptureVisionRouter router;

    public CameraPage()
    {
        InitializeComponent();
        enhancer = new CameraEnhancer();
        router = new CaptureVisionRouter();
        router.SetInput(enhancer);
        router.AddResultReceiver(this);
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        if (this.Handler != null && enhancer != null)
        {
            enhancer.SetCameraView(camera);
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

    public void OnDecodedBarcodesReceived(DecodedBarcodesResult result)
    {
        if (result != null && result.Items != null && result.Items.Count > 0)
        {
            // MainThread.BeginInvokeOnMainThread(() =>
            // {
            //     router?.StopCapturing();
            //     enhancer?.ClearBuffer();
            // });
            var message = "";
            foreach (var item in result.Items)
            {
                message += "\nFormat: " + item.FormatString + "\nText: " + item.Text + "\n";
            }
            // MainThread.BeginInvokeOnMainThread(async () =>
            // {
            //     await DisplayAlert("Results", message, "OK");
            //     router?.StartCapturing(EnumPresetTemplate.PT_READ_BARCODES, this);
            // });
        }
    }

    public void OnSuccess()
    {
        Debug.WriteLine("success");
    }

    public void OnFailure(int errorCode, string errorMessage)
    {
        Debug.WriteLine(errorMessage);
    }
}
