using Microsoft.Maui.Controls.Compatibility.Hosting;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace BarcodeQrScanner;

// https://docs.microsoft.com/en-us/dotnet/api/skiasharp.views.maui.controls.apphostbuilderextensions?view=skiasharp-views-maui-2.88
public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder.UseSkiaSharp()
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});


        return builder.Build();
	}
}
