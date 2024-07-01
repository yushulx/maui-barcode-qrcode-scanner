using Dynamsoft.License.Maui;
using System.Diagnostics;


namespace BarcodeQrScanner;

public partial class MainPage : ContentPage, ILicenseVerificationListener
{
	public MainPage()
	{
		InitializeComponent();
		LicenseManager.InitLicense("DLS2eyJoYW5kc2hha2VDb2RlIjoiMjAwMDAxLTE2NDk4Mjk3OTI2MzUiLCJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSIsInNlc3Npb25QYXNzd29yZCI6IndTcGR6Vm05WDJrcEQ5YUoifQ==", this);
	}

	public void OnLicenseVerified(bool isSuccess, string message)
    {
        if (!isSuccess)
        {
            Debug.WriteLine(message);
        }
    }

	async void OnTakePhotoButtonClicked(object sender, EventArgs e)
    {
		try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                FileTypes = FilePickerFileType.Images,
                PickerTitle = "Please select an image"
            });

            if (result != null)
            {
				await Navigation.PushAsync(new PicturePage(result));
            }
        }
        catch (Exception ex)
        {
            // Handle exceptions if any
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
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
				// await DisplayAlert("Permission Granted", "Granted", "Ok");
            }
            else
            {
                await DisplayAlert("Permission needed", "I will need Camera permission for this action", "Ok");
            }
        }
    }
}

