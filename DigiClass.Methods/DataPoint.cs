using MathNet.Numerics.LinearAlgebra;

namespace DigiClass.Methods
{
    /// <summary>
    ///     Reprezentuje pojedynczy punkt zestawu danych uczących lub testujących.
    /// </summary>
    public interface IDataPoint
    {
        /// <summary>
        ///     Wektor reprezentujący zestaw wartości wejściowych.
        /// </summary>
        Vector<float> Input { get; }

        /// <summary>
        ///     Wektor  reprezentujący zestaw wartości wyjściowych.
        /// </summary>
        Vector<float> Output { get; }
    }

    public class DataPoint : IDataPoint
    {
        public DataPoint(Vector<float> input, Vector<float> output)
        {
            Input = input;
            Output = output;
        }

        public Vector<float> Input { get; }
        public Vector<float> Output { get; }

        /// <summary>
        ///     Implementuje konwersję do ciągu znaków (x_0, x_1, ..., x_k) → (y_0, y_1, ..., y_m).
        /// </summary>
        public override string ToString()
        {
            return $"({string.Join(", ", Input.Enumerate())}) → ({string.Join(", ", Output.Enumerate())})";
        }
    }
}