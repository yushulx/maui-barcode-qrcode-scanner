using BarcodeQrScanner.Services;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using System.Timers;

namespace BarcodeQrScanner;

public partial class DesktopCameraPage : ContentPage
{
    private System.Timers.Timer timer;
    private int count = 0;

    public DesktopCameraPage()
	{
		InitializeComponent();

        timer = new System.Timers.Timer(30);
        timer.Elapsed += OnTimedEvent;
        timer.AutoReset = true;
        timer.Enabled = true;
    }

    private void OnTimedEvent(Object source, ElapsedEventArgs e)
    {
        canvasView.InvalidateSurface();
    }

    void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
    {
        

        count += 1;
        ResultLabel.Text = "" + count;
    }
}