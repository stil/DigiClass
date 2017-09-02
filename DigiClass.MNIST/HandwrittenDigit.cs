using System.Drawing;
using System.Text;
using DigiClass.Methods;
using MathNet.Numerics.LinearAlgebra;

namespace DigiClass.MNIST
{
    public class HandwrittenDigit
    {
        public const int ImageSize = 28;

        public HandwrittenDigit(byte[] pixels, byte label)
        {
            Pixels = pixels;
            Label = label;
        }

        public byte Label { get; }
        public byte[] Pixels { get; }

        public IDataPoint ToDataPoint()
        {
            var pixels = Pixels;
            var inputArray = new float[pixels.Length];
            for (var i = 0; i < pixels.Length; i++)
            {
                var pixel = pixels[i];
                inputArray[i] = pixel > 0 ? 1.0f * pixel / byte.MaxValue : 0;
            }
            var inputVector = Vector<float>.Build.Dense(inputArray);

            var outputVector = Vector<float>.Build.Dense(10, 0);
            outputVector[Label] = 1;

            return new DataPoint(
                inputVector,
                outputVector
            );
        }

        public override string ToString()
        {
            var s = new StringBuilder();

            var offset = 0;
            for (var i = 0; i < ImageSize; ++i)
            {
                for (var j = 0; j < ImageSize; ++j)
                {
                    switch (Pixels[offset++])
                    {
                        case 0:
                            s.Append(" "); // white
                            break;
                        case 255:
                            s.Append("O"); // black
                            break;
                        default:
                            s.Append("."); // gray
                            break;
                    }
                }
                s.Append("\n");
            }

            s.Append(Label.ToString());

            return s.ToString();
        }

        public Bitmap ToBitmap()
        {
            var bitmap = new Bitmap(ImageSize, ImageSize);

            var offset = 0;
            for (var j = 0; j < bitmap.Height; ++j)
            {
                for (var i = 0; i < bitmap.Width; ++i)
                {
                    var darkness = 255 - Pixels[offset++];
                    bitmap.SetPixel(i, j, Color.FromArgb(darkness, darkness, darkness));
                }
            }

            return bitmap;
        }
    }
}