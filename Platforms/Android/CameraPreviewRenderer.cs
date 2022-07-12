using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Hardware;
using Android.OS;
using Android.Views;
using Android.Widget;
using BarcodeQrScanner.Platforms.Android;
using BarcodeQrScanner.Services;
using Com.Dynamsoft.Dbr;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Controls.Compatibility.Platform.Android.FastRenderers;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Platform;
using SkiaSharp;
using System.ComponentModel;
using Camera = Android.Hardware.Camera;
using View = Android.Views.View;

[assembly: ExportRenderer(typeof(BarcodeQrScanner.CameraPreview), typeof(CameraPreviewRenderer))]
namespace BarcodeQrScanner.Platforms.Android
{
    public class CameraPreviewRenderer : FrameLayout, IVisualElementRenderer, IViewRenderer, TextureView.ISurfaceTextureListener, Camera.IPreviewCallback, Handler.ICallback
    {
        global::Android.Hardware.Camera camera;
        global::Android.Views.View view;

        Activity activity;
        CameraFacing cameraType;
        AutoFitTextureView textureView;
        SurfaceTexture surfaceTexture;
        CameraPreview element;
        VisualElementTracker visualElementTracker;
        VisualElementRenderer visualElementRenderer;
        int? defaultLabelFor;

        private HandlerThread handlerThread;
        private Handler backgroundHandler;

        public event EventHandler<VisualElementChangedEventArgs> ElementChanged;
        public event EventHandler<PropertyChangedEventArgs> ElementPropertyChanged;

        BarcodeReader barcodeReader = new BarcodeReader();
        private int previewWidth;
        private int previewHeight;
        private int[] stride;
        private bool isReady = true;

        CameraPreview Element
        {
            get => element;
            set
            {
                if (element == value)
                {
                    return;
                }

                var oldElement = element;
                element = value;
                OnElementChanged(new ElementChangedEventArgs<CameraPreview>(oldElement, element));
            }
        }

        public CameraPreviewRenderer(Context context) : base(context)
        {
            visualElementRenderer = new VisualElementRenderer(this);
        }

        void OnElementChanged(ElementChangedEventArgs<CameraPreview> e)
        {
            if (e.OldElement != null)
            {
                e.OldElement.PropertyChanged -= OnElementPropertyChanged;
            }
            if (e.NewElement != null)
            {
                this.EnsureId();

                e.NewElement.PropertyChanged += OnElementPropertyChanged;

                ElevationHelper.SetElevation(this, e.NewElement);
            }

            ElementChanged?.Invoke(this, new VisualElementChangedEventArgs(e.OldElement, e.NewElement));

            try
            {
                SetupUserInterface();
                AddView(view);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(@"			ERROR: ", ex.Message);
            }
        }

        void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ElementPropertyChanged?.Invoke(this, e);
        }

        public void OnPreviewFrame(byte[] data, Camera camera)
        {
            try
            {
                YuvImage yuvImage = new YuvImage(data, ImageFormatType.Nv21,
                        previewWidth, previewHeight, null);
                stride = yuvImage.GetStrides();
                try
                {
                    if (isReady)
                    {
                        if (backgroundHandler != null)
                        {
                            isReady = false;
                            Message msg = new Message();
                            msg.What = 100;
                            msg.Obj = yuvImage;
                            backgroundHandler.SendMessage(msg);
                        }
                    }
                }
                catch (BarcodeReaderException e)
                {
                    e.PrintStackTrace();
                }
            }
            catch (System.IO.IOException)
            {
            }
        }

        void SetupUserInterface()
        {
            activity = this.Context as Activity;
            view = activity.LayoutInflater.Inflate(Resource.Layout.CameraLayout, this, false);
            cameraType = (Element.Camera == CameraOptions.Front) ? CameraFacing.Front : CameraFacing.Back;

            textureView = view.FindViewById<AutoFitTextureView>(Resource.Id.textureView);
            textureView.SurfaceTextureListener = this;

            PublicRuntimeSettings settings = barcodeReader.RuntimeSettings;
            settings.ExpectedBarcodesCount = (Element.ScanMode == ScanOptions.Single) ? 1 : 512;
            barcodeReader.UpdateRuntimeSettings(settings);
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            base.OnLayout(changed, l, t, r, b);

            var msw = MeasureSpec.MakeMeasureSpec(r - l, MeasureSpecMode.Exactly);
            var msh = MeasureSpec.MakeMeasureSpec(b - t, MeasureSpecMode.Exactly);

            view.Measure(msw, msh);
            view.Layout(0, 0, r - l, b - t);
        }

        public void OnSurfaceTextureUpdated(SurfaceTexture surface)
        {

        }

        public void OnSurfaceTextureAvailable(SurfaceTexture surface, int width, int height)
        {
            handlerThread = new HandlerThread("background");
            handlerThread.Start();
            backgroundHandler = new Handler(handlerThread.Looper, this);

            camera = global::Android.Hardware.Camera.Open((int)cameraType);
            textureView.LayoutParameters = new FrameLayout.LayoutParams(width, height);
            surfaceTexture = surface;

            camera.SetPreviewTexture(surface);
            PrepareAndStartCamera();
        }

        public bool OnSurfaceTextureDestroyed(SurfaceTexture surface)
        {
            if (handlerThread != null)
            {
                handlerThread.QuitSafely();
                handlerThread.Join();
                handlerThread = null;
            }

            if (backgroundHandler != null)
            {
                backgroundHandler.RemoveMessages(100);
                backgroundHandler = null;
            }

            camera.SetPreviewCallback(null);
            camera.StopPreview();
            camera.Release();

            return true;
        }

        public void OnSurfaceTextureSizeChanged(SurfaceTexture surface, int width, int height)
        {
            PrepareAndStartCamera();
        }

        void PrepareAndStartCamera()
        {
            camera.SetPreviewCallback(null);
            camera.StopPreview();

            var display = activity.WindowManager.DefaultDisplay;
            if (display.Rotation == SurfaceOrientation.Rotation0)
            {
                camera.SetDisplayOrientation(90);
            }

            if (display.Rotation == SurfaceOrientation.Rotation270)
            {
                camera.SetDisplayOrientation(180);
            }

            Camera.Parameters parameters = camera.GetParameters();
            previewWidth = parameters.PreviewSize.Width;
            previewHeight = parameters.PreviewSize.Height;
            if (parameters.SupportedFocusModes.Contains(Camera.Parameters.FocusModeContinuousVideo))
            {
                parameters.FocusMode = Camera.Parameters.FocusModeContinuousVideo;
            }
            camera.SetParameters(parameters);
            camera.SetPreviewCallback(this);
            camera.StartPreview();
        }

        public bool HandleMessage(Message msg)
        {
            if (msg.What == 100)
            {
                Message uiMsg = new Message();
                uiMsg.What = 200;
                uiMsg.Obj = "";
                BarcodeQrData[] output = null;
                try
                {
                    YuvImage image = (YuvImage)msg.Obj;
                    if (image != null)
                    {
                        int[] stridelist = image.GetStrides();
                        TextResult[] results = barcodeReader.DecodeBuffer(image.GetYuvData(), previewWidth, previewHeight, stridelist[0], EnumImagePixelFormat.IpfNv21);
                        if (results != null && results.Length > 0)
                        {
                            output = new BarcodeQrData[results.Length];
                            int index = 0;
                            foreach (TextResult result in results)
                            {
                                BarcodeQrData data = new BarcodeQrData();
                                data.text = result.BarcodeText;
                                data.format = result.BarcodeFormatString;
                                LocalizationResult localizationResult = result.LocalizationResult;
                                data.points = new SKPoint[localizationResult.ResultPoints.Count];
                                int pointsIndex = 0;
                                foreach (Com.Dynamsoft.Dbr.Point point in localizationResult.ResultPoints)
                                {
                                    SKPoint p = new SKPoint();
                                    p.X = point.X;
                                    p.Y = point.Y;
                                    data.points[pointsIndex++] = p;
                                }
                                output[index++] = data;
                            }
                        }
                    }
                }
                catch (BarcodeReaderException e)
                {
                    e.PrintStackTrace();
                }

                Element.NotifyResultReady(output, previewWidth, previewHeight);
                isReady = true;
            }

            return true;
        }

        #region IViewRenderer

        void IViewRenderer.MeasureExactly() => MeasureExactly(this, Element, Context);

        static void MeasureExactly(View control, VisualElement element, Context context)
        {
            if (control == null || element == null)
            {
                return;
            }

            double width = element.Width;
            double height = element.Height;

            if (width <= 0 || height <= 0)
            {
                return;
            }

            int realWidth = (int)context.ToPixels(width);
            int realHeight = (int)context.ToPixels(height);

            int widthMeasureSpec = MeasureSpecFactory.MakeMeasureSpec(realWidth, MeasureSpecMode.Exactly);
            int heightMeasureSpec = MeasureSpecFactory.MakeMeasureSpec(realHeight, MeasureSpecMode.Exactly);

            control.Measure(widthMeasureSpec, heightMeasureSpec);
        }

        #endregion

        #region IVisualElementRenderer

        VisualElement IVisualElementRenderer.Element => Element;

        VisualElementTracker IVisualElementRenderer.Tracker => visualElementTracker;

        View IVisualElementRenderer.View => this;

        SizeRequest IVisualElementRenderer.GetDesiredSize(int widthConstraint, int heightConstraint)
        {
            Measure(widthConstraint, heightConstraint);
            SizeRequest result = new SizeRequest(new Size(MeasuredWidth, MeasuredHeight), new Size(Context.ToPixels(20), Context.ToPixels(20)));
            return result;
        }

        void IVisualElementRenderer.SetElement(VisualElement element)
        {
            if (!(element is CameraPreview camera))
            {
                throw new ArgumentException($"{nameof(element)} must be of type {nameof(CameraPreview)}");
            }

            if (visualElementTracker == null)
            {
                visualElementTracker = new VisualElementTracker(this);
            }
            Element = camera;
        }

        void IVisualElementRenderer.SetLabelFor(int? id)
        {
            if (defaultLabelFor == null)
            {
                defaultLabelFor = LabelFor;
            }
            LabelFor = (int)(id ?? defaultLabelFor);
        }

        void IVisualElementRenderer.UpdateLayout() => visualElementTracker?.UpdateLayout();

        #endregion

        static class MeasureSpecFactory
        {
            public static int GetSize(int measureSpec)
            {
                const int modeMask = 0x3 << 30;
                return measureSpec & ~modeMask;
            }

            public static int MakeMeasureSpec(int size, MeasureSpecMode mode) => size + (int)mode;
        }
    }
}
