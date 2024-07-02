using Dynamsoft.CaptureVisionRouter.Maui;
using Dynamsoft.BarcodeReader.Maui;
using Microsoft.Maui.Graphics.Platform;

namespace BarcodeQrScanner;

public partial class PicturePage : ContentPage
{
    private CaptureVisionRouter? router;

    public PicturePage(FileResult result)
    {
        InitializeComponent();
        LoadImageWithOverlay(result);
    }

    async private void LoadImageWithOverlay(FileResult result)
    {
        // Get the file path
        var filePath = result.FullPath;
        var stream = await result.OpenReadAsync();

        float originalWidth = 0;
        float originalHeight = 0;

        try
        {
            var image = PlatformImage.FromStream(stream);
            originalWidth = image.Width;
            originalHeight = image.Height;

            // Reset the stream position to the beginning
            stream.Position = 0;
            ImageSource imageSource = ImageSource.FromStream(() => stream);
            PickedImage.Source = imageSource;
            

            // Decode barcode
            router = new CaptureVisionRouter();
            CapturedResult capturedResult = router.Capture(filePath, EnumPresetTemplate.PT_READ_BARCODES);
            DecodedBarcodesResult? barcodeResults = null;

            if (capturedResult != null) {
                // Get the barcode results
                barcodeResults = capturedResult.DecodedBarcodesResult;
            }

            // Create a drawable with the barcode results
            var drawable = new ImageWithOverlayDrawable(barcodeResults, originalWidth, originalHeight, true);

            // Set drawable to GraphicsView
            OverlayGraphicsView.Drawable = drawable;
            OverlayGraphicsView.Invalidate(); // Redraw the view
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    private void OnImageSizeChanged(object sender, EventArgs e)
    {
        // Adjust the GraphicsView size to match the Image size
        OverlayGraphicsView.WidthRequest = PickedImage.Width;
        OverlayGraphicsView.HeightRequest = PickedImage.Height;
    }

    async void OnImageLoad(string imagepath)
    {
        await Navigation.PopAsync();
    }
}