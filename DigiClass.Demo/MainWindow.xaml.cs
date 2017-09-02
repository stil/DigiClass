using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace DigiClass.Demo
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            MainWindowModel viewModel;
            DataContext = viewModel = new MainWindowModel();
            InitializeComponent();

            InkCanvas1.DefaultDrawingAttributes = new DrawingAttributes
            {
                Color = Colors.SpringGreen,
                Height = 10,
                Width = 10,
                FitToCurve = false
            };

            InkCanvas1.StrokeCollected += (sender, args) =>
            {
                // Capture canvas contents to bitmap.
                var width = InkCanvas1.ActualWidth;
                var height = InkCanvas1.ActualHeight;
                var bmpCopied = new RenderTargetBitmap(
                    (int) Math.Round(width),
                    (int) Math.Round(height),
                    96, 96,
                    PixelFormats.Default);
                var dv = new DrawingVisual();
                using (var dc = dv.RenderOpen())
                {
                    var vb = new VisualBrush(InkCanvas1);
                    dc.DrawRectangle(vb, null, new Rect(new Point(), new Size(width, height)));
                }
                bmpCopied.Render(dv);
                Bitmap bitmap;
                using (var outStream = new MemoryStream())
                {
                    // From System.Media.BitmapImage to System.Drawing.Bitmap 
                    BitmapEncoder enc = new BmpBitmapEncoder();
                    enc.Frames.Add(BitmapFrame.Create(bmpCopied));
                    enc.Save(outStream);
                    bitmap = new Bitmap(outStream);
                }

                viewModel.OnCanvasModified(bitmap);
            };
        }
    }
}