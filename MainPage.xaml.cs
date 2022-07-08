using BarcodeQrScanner.Services;

namespace BarcodeQrScanner;

public partial class MainPage : ContentPage
{
    string PhotoPath = "";
    IBarcodeQRCodeService _barcodeQRCodeService;

    public MainPage()
	{
		InitializeComponent();
	}

    async void OnTakePhotoButtonClicked(object sender, EventArgs e)
    {
        try
        {
            var photo = await MediaPicker.CapturePhotoAsync();
            await LoadPhotoAsync(photo);
            Console.WriteLine($"CapturePhotoAsync COMPLETED: {PhotoPath}");
        }
        catch (FeatureNotSupportedException fnsEx)
        {
            // Feature is not supported on the device
        }
        catch (PermissionException pEx)
        {
            // Permissions not granted
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CapturePhotoAsync THREW: {ex.Message}");
        }
    }

    async Task LoadPhotoAsync(FileResult photo)
    {
        // canceled
        if (photo == null)
        {
            PhotoPath = null;
            return;
        }
        // save the file into local storage
        var newFile = Path.Combine(FileSystem.CacheDirectory, photo.FileName);
        using (var stream = await photo.OpenReadAsync())
        using (var newStream = File.OpenWrite(newFile))
            await stream.CopyToAsync(newStream);

        PhotoPath = newFile;

        await Navigation.PushAsync(new PicturePage());
    }

    async void OnTakeVideoButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CameraPage());
    }
}

