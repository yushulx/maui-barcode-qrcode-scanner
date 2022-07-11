using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeQrScanner
{
    public class ResultReadyEventArgs : EventArgs
    {
        public ResultReadyEventArgs(object result, int previewWidth, int previewHeight)
        {
            Result = result;
            PreviewWidth = previewWidth;
            PreviewHeight = previewHeight;
        }

        public object Result { get; private set; }
        public int PreviewWidth { get; private set; }
        public int PreviewHeight { get; private set; }

    }

    public class CameraPreview : View
    {
        public static readonly BindableProperty CameraProperty = BindableProperty.Create(
            propertyName: "Camera",
            returnType: typeof(CameraOptions),
            declaringType: typeof(CameraPreview),
            defaultValue: CameraOptions.Rear);

        public CameraOptions Camera
        {
            get { return (CameraOptions)GetValue(CameraProperty); }
            set { SetValue(CameraProperty, value); }
        }

        public static readonly BindableProperty ScanProperty = BindableProperty.Create(
            propertyName: "ScanMode",
            returnType: typeof(ScanOptions),
            declaringType: typeof(CameraPreview),
            defaultValue: ScanOptions.Single);

        public ScanOptions ScanMode
        {
            get { return (ScanOptions)GetValue(ScanProperty); }
            set { SetValue(ScanProperty, value); }
        }

        public event EventHandler<ResultReadyEventArgs> ResultReady;

        public void NotifyResultReady(object result, int previewWidth, int previewHeight)
        {
            if (ResultReady != null)
            {
                ResultReady(this, new ResultReadyEventArgs(result, previewWidth, previewHeight));
            }
        }
    }
}
