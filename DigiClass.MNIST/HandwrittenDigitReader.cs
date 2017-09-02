using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using DigiClass.MNIST.Helpers;

namespace DigiClass.MNIST
{
    public class HandwrittenDigitReader
    {
        public IEnumerable<HandwrittenDigit> Iterate(string imagePath, string labelPath)
        {
            using (var ifsImages = File.OpenRead(imagePath))
            using (var ifsLabels = File.OpenRead(labelPath))
            using (var igzsImages = new GZipStream(ifsImages, CompressionMode.Decompress))
            using (var igzsLabels = new GZipStream(ifsLabels, CompressionMode.Decompress))
            using (var brImages = new BinaryReader(igzsImages))
            using (var brLabels = new BinaryReader(igzsLabels))
            {
                if (brImages.ReadInt32LittleEndian() != 2051)
                {
                    throw new Exception("Invalid magic number.");
                }

                if (brLabels.ReadInt32LittleEndian() != 2049)
                {
                    throw new Exception("Invalid magic number.");
                }

                var numImages = brImages.ReadInt32LittleEndian();
                var numRows = brImages.ReadInt32LittleEndian();
                var numCols = brImages.ReadInt32LittleEndian();
                var numLabels = brLabels.ReadInt32LittleEndian();

                if (numImages != numLabels)
                {
                    throw new Exception("Number of images is different from number of labels.");
                }

                // Item buffering for better performance.
                const int bufferSize = 30000;
                var buffer = new Queue<HandwrittenDigit>();

                for (var i = 0; i < numImages; i++)
                {
                    var pixels = brImages.ReadBytes(numRows * numCols);
                    var label = brLabels.ReadByte();
                    var image = new HandwrittenDigit(pixels, label);
                    //Console.WriteLine(image.ToString());

                    if (buffer.Count < bufferSize)
                    {
                        buffer.Enqueue(image);
                    }
                    else
                    {
                        Debug.WriteLine($"Buffered {buffer.Count} images.");

                        while (buffer.Count > 0)
                        {
                            yield return buffer.Dequeue();
                        }
                    }
                }

                while (buffer.Count > 0)
                {
                    yield return buffer.Dequeue();
                }
            }
        }
    }
}