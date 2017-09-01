using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;

namespace DigiClass.Methods.Perceptron
{
    public class PerceptronModel
    {
        /// <summary>
        ///     Inicjalizuje sieć neuronową.
        /// </summary>
        /// <param name="layerSizes">Wektor informujący ile perceptronów ma posiadać każda z warstw sieci.</param>
        public PerceptronModel(params int[] layerSizes)
        {
            LayerSizes = layerSizes.ToList();

            var dist = new Normal();

            // Dla każdej z warstw sieci (oprócz pierwszej) buduje wektor kolumnowy
            // zawierający wartość progową pobudzenia kolejnych perceptronów.
            // Wartości progowe są inicjowane losowymi wartościami z rozkładu normalnego.
            Biases = LayerSizes.Skip(1).Select(
                y => Matrix<double>.Build.Random(y, 1, dist)
            ).ToList();

            // Dla każdej z warstw sieci buduje macierz zawierającą wagi każdego z kolejnych perceptronów.
            // Wagi sygnałów wejściowych są wypełniane pionowo (od góry do dołu). Każda kolumna macierzy
            // reprezentuje kolejny perceptron danej warstwy.
            // Wagi są inicjowane losowymi wartościami z rozkładu normalnego.
            Weights = LayerSizes.Take(LayerSizes.Count - 1).Zip(LayerSizes.Skip(1),
                (x, y) => Matrix<double>.Build.Random(y, x, dist) / Math.Sqrt(x)
            ).ToList();
        }

        /// <summary>
        ///     Inicjalizuje sieć neuronową danymi wartościami.
        /// </summary>
        public PerceptronModel(
            IEnumerable<int> layerSizes,
            IEnumerable<Matrix<double>> biases,
            IEnumerable<Matrix<double>> weights)
        {
            LayerSizes = layerSizes.ToList();
            Biases = biases.ToList();
            Weights = weights.ToList();
        }

        /// <summary>
        ///     Zawiera listę określającą rozmiary kolejnych warstw sieci neuronowej.
        ///     Pierwsza i ostatnia warstwa to odpowiednio warstwa wejściowa i wyjściowa.
        /// </summary>
        public List<int> LayerSizes { get; }

        /// <summary>
        ///     Lista wektorów kolumnowych zawierający progi pobudzenia dla każdego neuronu w danej warstwie.
        /// </summary>
        public List<Matrix<double>> Biases { get; }

        /// <summary>
        ///     Macierze wag sygnałów wejściowych perceptronów dla każdej warstwy sieci.
        /// </summary>
        public List<Matrix<double>> Weights { get; }
    }
}