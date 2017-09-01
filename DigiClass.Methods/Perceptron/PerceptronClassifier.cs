using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DigiClass.Methods.Extensions;
using MathNet.Numerics.LinearAlgebra;

namespace DigiClass.Methods.Perceptron
{
    /// <summary>
    ///     Reprezentuje sieć neuronową typu MLP.
    /// </summary>
    public class PerceptronClassifier : IClassifier
    {
        /// <summary>
        ///     Initializes perceptron classifier.
        /// </summary>
        /// <param name="model">Perceptron model.</param>
        /// <param name="learningRate">Learning rate.</param>
        /// <param name="batchSize">Samples batch size.</param>
        public PerceptronClassifier(PerceptronModel model, double learningRate, int batchSize)
        {
            Model = model;
            LearningRate = learningRate;
            BatchSize = batchSize;
        }

        public PerceptronModel Model { get; }

        /// <summary>
        ///     Perceptron learning rate.
        /// </summary>
        public double LearningRate { get; }

        /// <summary>
        ///     Batch size.
        /// </summary>
        public int BatchSize { get; }

        /// <summary>
        ///     Zdarzenie wywoływane gdy została zakończona pojedyncza iteracja uczenia sieci.
        ///     W argumencie przekazywany jest indeks właśnie zakończonej iteracji.
        /// </summary>
        public event Action<int> IterationComplete;

        /// <summary>
        ///     Wykonuje propagację wprzód, z wejścia do wyjścia.
        /// </summary>
        /// <param name="input">Wektor wejściowy.</param>
        public Vector<double> Classify(Vector<double> input)
        {
            // Przekształca wektor wejściowy do macierzy kolumnowej.
            var output = input.ToColumnMatrix();

            // Wykonuje propagację dla kolejnych warstw sieci.
            Model.Biases.ZipForEach(Model.Weights, (b, w) => { output = (w * output + b).Sigmoid(); });

            // Przekształca macierz kolumnową z powrotem do wektora.
            return Vector<double>.Build.Dense(output.Enumerate().ToArray());
        }

        /// <summary>
        ///     Uczy sieć neuronową metodą spadku w kierunku gradientu.
        /// </summary>
        /// <param name="trainingData">Dane uczące.</param>
        /// <param name="ct">Token pozwalający przerwać proces uczenia poprzez sygnał z zewnątrz.</param>
        public void Train(
            IList<IDataPoint> trainingData,
            CancellationToken ct)
        {
            for (var i = 1;; i++)
            {
                foreach (var miniBatch in trainingData.Chunk(BatchSize))
                {
                    UpdateMiniBatch(miniBatch, BatchSize, LearningRate);
                    if (ct.IsCancellationRequested)
                    {
                        break;
                    }
                }

                IterationComplete?.Invoke(i);
                if (ct.IsCancellationRequested)
                {
                    break;
                }
            }
        }

        /// <summary>
        ///     Dokonuje korekty wag oraz wartości pobudzenia perceptronów w sieci.
        /// </summary>
        /// <param name="miniBatch"></param>
        /// <param name="miniBatchSize"></param>
        /// <param name="eta">Współczynnik uczenia.</param>
        private void UpdateMiniBatch(
            IEnumerable<IDataPoint> miniBatch,
            int miniBatchSize,
            double eta)
        {
            // Tworzy listy macierzy (początkowo wyzerowanych) przechowujących sumaryczną korektę
            // wartości progowych i wag każdej z warstw sieci.
            var nablaB = Model.Biases.Select(b => b.CreateMatrixOfSameShape()).ToList();
            var nablaW = Model.Weights.Select(w => w.CreateMatrixOfSameShape()).ToList();

            var deltaNablaB = nablaB.Select(b => b.Clone()).ToList();
            var deltaNablaW = nablaW.Select(w => w.Clone()).ToList();

            // Iteruje zestaw próbek treningowych.
            foreach (var dataPoint in miniBatch)
            {
                Backpropagation(dataPoint, deltaNablaB, deltaNablaW);

                nablaB.ZipForEach(deltaNablaB, (nb, dnb) => nb.Add(dnb, nb));
                nablaW.ZipForEach(deltaNablaW, (nw, dnw) => nw.Add(dnw, nw));

                deltaNablaB.ForEach(m => m.Clear());
                deltaNablaW.ForEach(m => m.Clear());
            }

            // Korekta wartości progowych i wag uwzględniając współczynnik uczenia.
            Model.Biases.ZipForEach(nablaB, (b, nabla) => b.Subtract(eta / miniBatchSize * nabla, b));
            Model.Weights.ZipForEach(nablaW, (w, nabla) => w.Subtract(eta / miniBatchSize * nabla, w));
        }

        /// <summary>
        ///     Realizuje propagację wsteczną.
        /// </summary>
        private void Backpropagation(IDataPoint dataPoint,
            IList<Matrix<double>> deltaNablaB,
            IList<Matrix<double>> deltaNablaW)
        {
            // Ustawienie wejść.
            var a = dataPoint.Input.ToColumnMatrix(); // Ustawienie "aktywacji" warstwy wejściowej.
            var aLayers = new List<Matrix<double>> {a}; // Zapis aktywacji warstwy wejściowej.

            // Propagacja wprzód.
            var zLayers = new List<Matrix<double>>(); // Lista pobudzeń każdej z warstw sieci.
            for (var i = 0; i < Model.LayerSizes.Count - 1; i++)
            {
                var z = Model.Weights[i] * a + Model.Biases[i]; // z = w*a + b
                zLayers.Add(z); // Dodanie pobudzeń bieżącej warstwy do listy.
                a = z.Sigmoid(); // a=sigma(z)
                aLayers.Add(a); // Dodanie aktywacji bieżącej warstwy do listy.
            }

            // Obliczenie delty dla ostatniej warstwy.
            var bigL = Model.LayerSizes.Count - 2;
            var delta = (aLayers[bigL + 1] - dataPoint.Output.ToColumnMatrix())
                .PointwiseMultiply(zLayers[bigL].SigmoidPrime());

            deltaNablaB[bigL] = delta;
            deltaNablaW[bigL] = delta.TransposeAndMultiply(aLayers[bigL]);

            // Propagacja wstecz poczynając od przedostatniej warstwy.
            while (bigL-- > 0)
            {
                delta = Model.Weights[bigL + 1].TransposeThisAndMultiply(delta)
                    .PointwiseMultiply(zLayers[bigL].SigmoidPrime());

                deltaNablaB[bigL] = delta;
                deltaNablaW[bigL] = delta.TransposeAndMultiply(aLayers[bigL]);
            }
        }
    }
}