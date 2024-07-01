using Dynamsoft.CaptureVisionRouter.Maui;
using Dynamsoft.BarcodeReader.Maui;
using Microsoft.Maui.Graphics.Platform;

namespace BarcodeQrScanner;

public class ImageWithOverlayDrawable : IDrawable
{
    private readonly DecodedBarcodesResult? _barcodeResults;
    private readonly float _originalWidth;
    private readonly float _originalHeight;

    public ImageWithOverlayDrawable(DecodedBarcodesResult? barcodeResults, float originalWidth, float originalHeight)
    {
        _barcodeResults = barcodeResults;
        _originalWidth = originalWidth;
        _originalHeight = originalHeight;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        // Calculate scaling factors
        float scaleX = dirtyRect.Width / _originalWidth;
        float scaleY = dirtyRect.Height / _originalHeight;

        // Set scaling to maintain aspect ratio
        float scale = Math.Min(scaleX, scaleY);

        canvas.StrokeColor = Colors.Red;
        canvas.StrokeSize = 2;
        canvas.FontColor = Colors.Red;

        if (_barcodeResults != null) {
            var items = _barcodeResults.Items;
            foreach (var item in items) {
                Microsoft.Maui.Graphics.Point[] points = item.Location.Points;

                // Draw the bounding box
                canvas.DrawLine((float)points[0].X * scale, (float)points[0].Y * scale, (float)points[1].X * scale, (float)points[1].Y * scale);
                canvas.DrawLine((float)points[1].X * scale, (float)points[1].Y * scale, (float)points[2].X * scale, (float)points[2].Y * scale);
                canvas.DrawLine((float)points[2].X * scale, (float)points[2].Y * scale, (float)points[3].X * scale, (float)points[3].Y * scale);
                canvas.DrawLine((float)points[3].X * scale, (float)points[3].Y * scale, (float)points[0].X * scale, (float)points[0].Y * scale);

                canvas.DrawString(item.Text, (float)points[0].X * scale, (float)points[0].Y * scale, HorizontalAlignment.Left);
            }
        }
    }
}

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

        float originalWidth = 2736;
        float originalHeight = 3648;

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
            var drawable = new ImageWithOverlayDrawable(barcodeResults, originalWidth, originalHeight);

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