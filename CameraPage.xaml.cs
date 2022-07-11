using BarcodeQrScanner.Services;
using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace BarcodeQrScanner;

public partial class CameraPage : ContentPage
{
    BarcodeQrData[] data = null;
    private int imageWidth;
    private int imageHeight;

    public CameraPage()
	{
		InitializeComponent();
	}

    private void CameraPreview_ResultReady(object sender, ResultReadyEventArgs e)
    {
        if (e.Result != null)
        {
            data = (BarcodeQrData[])e.Result;
        }
        else
        {
            data = null;
        }

        imageWidth = e.PreviewWidth;
        imageHeight = e.PreviewHeight;

        canvasView.InvalidateSurface();
    }

    public static SKPoint rotateCW90(SKPoint point, int width)
    {
        SKPoint rotatedPoint = new SKPoint();
        rotatedPoint.X = width - point.Y;
        rotatedPoint.Y = point.X;
        return rotatedPoint;
    }

    void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
    {
        double width = canvasView.Width;
        double height = canvasView.Height;

        var mainDisplayInfo = DeviceDisplay.MainDisplayInfo;
        var orientation = mainDisplayInfo.Orientation;
        var rotation = mainDisplayInfo.Rotation;
        var density = mainDisplayInfo.Density;

        width *= density;
        height *= density;

        double scale, widthScale, heightScale, scaledWidth, scaledHeight;

        if (orientation == DisplayOrientation.Portrait)
        {
            widthScale = imageHeight / width;
            heightScale = imageWidth / height;
            scale = widthScale < heightScale ? widthScale : heightScale;
            scaledWidth = imageHeight / scale;
            scaledHeight = imageWidth / scale;
        }
        else
        {
            widthScale = imageWidth / width;
            heightScale = imageHeight / height;
            scale = widthScale < heightScale ? widthScale : heightScale;
            scaledWidth = imageWidth / scale;
            scaledHeight = imageHeight / scale;
        }

        SKImageInfo info = args.Info;
        SKSurface surface = args.Surface;
        SKCanvas canvas = surface.Canvas;

        canvas.Clear();

        SKPaint skPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Blue,
            StrokeWidth = 10,
        };

        SKPaint textPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Red,
            TextSize = (float)(18 * density),
            StrokeWidth = 4,
        };

        ResultLabel.Text = "";
        if (data != null)
        {
            foreach (BarcodeQrData barcodeQrData in data)
            {
                //ResultLabel.Text += barcodeQrData.text + "\n";

                for (int i = 0; i < 4; i++)
                {
                    if (orientation == DisplayOrientation.Portrait)
                    {
                        barcodeQrData.points[i] = rotateCW90(barcodeQrData.points[i], imageHeight);
                    }

                    if (widthScale < heightScale)
                    {
                        barcodeQrData.points[i].X = (float)(barcodeQrData.points[i].X / scale);
                        barcodeQrData.points[i].Y = (float)(barcodeQrData.points[i].Y / scale - (scaledHeight - height) / 2);
                    }
                    else
                    {
                        barcodeQrData.points[i].X = (float)(barcodeQrData.points[i].X / scale - (scaledWidth - width) / 2);
                        barcodeQrData.points[i].Y = (float)(barcodeQrData.points[i].Y / scale);
                    }
                }

                canvas.DrawText(barcodeQrData.text, barcodeQrData.points[0], textPaint);
                canvas.DrawLine(barcodeQrData.points[0], barcodeQrData.points[1], skPaint);
                canvas.DrawLine(barcodeQrData.points[1], barcodeQrData.points[2], skPaint);
                canvas.DrawLine(barcodeQrData.points[2], barcodeQrData.points[3], skPaint);
                canvas.DrawLine(barcodeQrData.points[3], barcodeQrData.points[0], skPaint);
            }
        }
        else
        {
            ResultLabel.Text = "No barcode QR code found";
        }

    }
}