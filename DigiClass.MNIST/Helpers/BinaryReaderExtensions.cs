using System;
using System.IO;

namespace DigiClass.MNIST.Helpers
{
    internal static class BinaryReaderExtensions
    {
        public static int ReadInt32LittleEndian(this BinaryReader reader)
        {
            var data = reader.ReadBytes(4);
            Array.Reverse(data);
            return BitConverter.ToInt32(data, 0);
        }
    }
}