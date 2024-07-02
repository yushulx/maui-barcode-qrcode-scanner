using Dynamsoft.BarcodeReader.Maui;

namespace BarcodeQrScanner;

public class ImageWithOverlayDrawable : IDrawable
{
    private readonly DecodedBarcodesResult? _barcodeResults;
    private readonly float _originalWidth;
    private readonly float _originalHeight;

    private bool _isFile;

    public ImageWithOverlayDrawable(DecodedBarcodesResult? barcodeResults, float originalWidth, float originalHeight, bool isFile = false)
    {
        _barcodeResults = barcodeResults;
        _originalWidth = originalWidth;
        _originalHeight = originalHeight;
        _isFile = isFile;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        // Calculate scaling factors
        float scaleX = (int)dirtyRect.Width / _originalWidth;
        float scaleY = (int)dirtyRect.Height / _originalHeight;

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
                if (_isFile){
                    canvas.DrawLine((float)points[0].X * scale, (float)points[0].Y * scale, (float)points[1].X * scale, (float)points[1].Y * scale);
                    canvas.DrawLine((float)points[1].X * scale, (float)points[1].Y * scale, (float)points[2].X * scale, (float)points[2].Y * scale);
                    canvas.DrawLine((float)points[2].X * scale, (float)points[2].Y * scale, (float)points[3].X * scale, (float)points[3].Y * scale);
                    canvas.DrawLine((float)points[3].X * scale, (float)points[3].Y * scale, (float)points[0].X * scale, (float)points[0].Y * scale);
                }

                canvas.DrawString(item.Text, (float)points[0].X * scale, (float)points[0].Y * scale - 10, HorizontalAlignment.Left);
            }
        }
    }
}