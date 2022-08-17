# .NET MAUI Barcode QR Code Scanner

The .NET MAUI project is ported from [Xamarin.Forms barcode QR code scanner](https://github.com/yushulx/xamarin-forms-barcode-qrcode-scanner). 

## Usage
1. Import the project to `Visual Studio 2022 Preview`, which supports .NET MAUI.
2. Apply for a [30-day free trial license](https://www.dynamsoft.com/customer/license/trialLicense?product=dbr) and update the following code in [MainPage.xaml.cs](https://github.com/yushulx/xamarin-forms-barcode-qrcode-scanner/blob/main/CustomRenderer/MainPage.xaml.cs).

    ```csharp
    _barcodeQRCodeService.InitSDK("DLS2eyJoYW5kc2hha2VDb2RlIjoiMjAwMDAxLTE2NDk4Mjk3OTI2MzUiLCJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSIsInNlc3Npb25QYXNzd29yZCI6IndTcGR6Vm05WDJrcEQ5YUoifQ==");
    ```
3. Select a framework for Android or iOS.
4. Build and run the project. 
  
    https://user-images.githubusercontent.com/2202306/178880069-4a50cd11-77e9-45b5-8c3a-bd9b63ff85fa.mp4
