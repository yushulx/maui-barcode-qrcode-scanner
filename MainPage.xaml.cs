using BarcodeQrScanner.Services;

namespace BarcodeQrScanner;

public partial class MainPage : ContentPage
{
    string PhotoPath = "";
    BarcodeQRCodeService _barcodeQRCodeService;

    public MainPage()
	{
		InitializeComponent();
        InitService();
    }

    async void OnTakePhotoButtonClicked(object sender, EventArgs e)
    {
        try
        {
            var photo = await FilePicker.PickAsync(PickOptions.Images);
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

        await Navigation.PushAsync(new PicturePage(PhotoPath, _barcodeQRCodeService));
    }

    private async void InitService()
    {
        await Task.Run(() =>
        {
            _barcodeQRCodeService = new BarcodeQRCodeService();
            try
            {
                _barcodeQRCodeService.InitSDK("DLS2eyJoYW5kc2hha2VDb2RlIjoiMjAwMDAxLTE2NDk4Mjk3OTI2MzUiLCJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSIsInNlc3Npb25QYXNzd29yZCI6IndTcGR6Vm05WDJrcEQ5YUoifQ==");
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.Message, "OK");
            }

            return Task.CompletedTask;
        });
    }

    async void OnTakeVideoButtonClicked(object sender, EventArgs e)
    {
        var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (status == PermissionStatus.Granted)
        {
            await Navigation.PushAsync(new CameraPage());
        }
        else
        {
            status = await Permissions.RequestAsync<Permissions.Camera>();
            if (status == PermissionStatus.Granted)
            {
                await Navigation.PushAsync(new CameraPage());
            }
            else
            {
                await DisplayAlert("Permission needed", "I will need Camera permission for this action", "Ok");
            }
        }
    }
}

