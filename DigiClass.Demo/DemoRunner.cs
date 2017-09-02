using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Threading;
using DigiClass.Methods;
using DigiClass.Methods.Extensions;
using DigiClass.Methods.Perceptron;
using DigiClass.MNIST;
using MathNet.Numerics.LinearAlgebra;

namespace DigiClass.Demo
{
    public class DemoRunner
    {
        private PerceptronClassifier _classifier;
        private PerceptronModel _model;

        public DemoRunner()
        {
            MnistFiles = new MNISTDatabase();

            var defaultFile = new FileInfo("network.xml");
            if (defaultFile.Exists)
            {
                var xml = defaultFile.OpenRead();
                using (var textReader = new StreamReader(xml))
                {
                    var serializer = new PerceptronModelSerializer();
                    _model = serializer.Deserialize(textReader);
                }
            }
            else
            {
                _model = new PerceptronModel(30);
            }

            _classifier = new PerceptronClassifier(_model, 2, 50);
        }

        public MNISTDatabase MnistFiles { get; }

        public event Action<string> Log;
        public event Action<int> EpochComplete;

        public void Run(int firstHiddenLayerSize, int secondHiddenLayerSize, int iterations, float learningRate, int batchSize)
        {
            var digitReader = new HandwrittenDigitReader();

            var trainDigits = digitReader.Iterate(
                MnistFiles.FindByType(MNISTFileType.TrainImages).FileName,
                MnistFiles.FindByType(MNISTFileType.TrainLabels).FileName);

            var testDigits = digitReader.Iterate(
                MnistFiles.FindByType(MNISTFileType.TestImages).FileName,
                MnistFiles.FindByType(MNISTFileType.TestLabels).FileName);

            var trainingData = trainDigits.Select(im => im.ToDataPoint());
            var testData = testDigits.Select(im => im.ToDataPoint());

            var cancellationToken = new CancellationTokenSource();

            _model = new PerceptronModel(
                HandwrittenDigit.ImageSize * HandwrittenDigit.ImageSize,
                firstHiddenLayerSize,
                secondHiddenLayerSize,
                10);

            _classifier = new PerceptronClassifier(
                _model,
                learningRate,
                batchSize);


            var watch = Stopwatch.StartNew();

            _classifier.IterationComplete += j =>
            {
                //Console.WriteLine($"Epoch {j}: {MNISTEvaulate(net, testData)} / {testData.Count}");
                OnLog(
                    $"Iteration #{j} has completed: {EvaluateClassifier(_classifier, testData)} / {testData.Count()} [{watch.ElapsedMilliseconds} ms]");
                watch = Stopwatch.StartNew();

                if (j == iterations)
                {
                    cancellationToken.Cancel();
                }

                EpochComplete?.Invoke(j);
            };

            _classifier.Train(trainingData, cancellationToken.Token);
        }

        private static int EvaluateClassifier(IClassifier net, IEnumerable<IDataPoint> testData)
        {
            var testResults = testData.Where(dataPoint =>
            {
                var networkOutput = net.Classify(dataPoint.Input).Enumerate().ToList();
                return networkOutput.MaxIndex() == dataPoint.Output.Enumerate().MaxIndex();
            });

            return testResults.Count();
        }

        protected virtual void OnLog(string obj)
        {
            Log?.Invoke(obj);
        }

        public int RecognizeDigit(Bitmap bitmap, Color backgroundColor)
        {
            var small = new Bitmap(HandwrittenDigit.ImageSize, HandwrittenDigit.ImageSize);
            using (var g = Graphics.FromImage(small))
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.DrawImage(bitmap, 0, 0, HandwrittenDigit.ImageSize, HandwrittenDigit.ImageSize);
            }
            small.Save("canvas.png");

            var vector = Vector<float>.Build.Dense(HandwrittenDigit.ImageSize * HandwrittenDigit.ImageSize, i =>
            {
                var actualColor = small.GetPixel(i % HandwrittenDigit.ImageSize, i / HandwrittenDigit.ImageSize);
                return actualColor.R == backgroundColor.R &&
                       actualColor.G == backgroundColor.G &&
                       actualColor.B == backgroundColor.B
                    ? 0
                    : 1;
            });

            var networkOutput = _classifier.Classify(vector).Enumerate().ToList();
            return networkOutput.MaxIndex();
        }

        public void SaveModel()
        {
            var serializer = new PerceptronModelSerializer();
            var destination = "network.xml";
            if (File.Exists(destination))
            {
                File.Delete(destination);
            }

            using (var fileStream = File.OpenWrite(destination))
            using (var writer = new StreamWriter(fileStream))
            {
                serializer.Serialize(writer, _model);
            }
        }
    }
}