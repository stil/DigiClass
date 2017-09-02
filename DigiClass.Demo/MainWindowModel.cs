using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Ink;
using System.Windows.Input;
using DigiClass.Demo.Helpers;
using DigiClass.MNIST;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;

namespace DigiClass.Demo
{
    internal class MainWindowModel : INotifyPropertyChanged
    {
        private readonly DemoRunner _demoRunner;
        private bool _downloadInProgress;
        private bool _mnistFilesIntegrity;
        private bool _trainInProgress;

        public MainWindowModel()
        {
            _demoRunner = new DemoRunner();

            _demoRunner.MnistFiles.IntegrityStatusUpdate += status => _mnistFilesIntegrity = status;
            _demoRunner.MnistFiles.StateChanged += (type, s) =>
            {
                switch (type)
                {
                    case MNISTFileType.TrainImages:
                        TrainImagesFileStatus = s;
                        break;
                    case MNISTFileType.TrainLabels:
                        TrainLabelsFileStatus = s;
                        break;
                    case MNISTFileType.TestImages:
                        TestImagesFileStatus = s;
                        break;
                    case MNISTFileType.TestLabels:
                        TestLabelsFileStatus = s;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
            };

            // Check integrity on startup.
            _demoRunner.MnistFiles.VerifyIntegrity();

            // Keep checking for integrity.
            _demoRunner.MnistFiles.WatchForIntegrity();

            TrainingDataDownload = new RelayCommand(
                // Enable when files are not integral and there is no download process running.
                o => !_mnistFilesIntegrity && !_downloadInProgress,
                async o =>
                {
                    _demoRunner.MnistFiles.PauseWatchingForIntegrity = true;

                    _downloadInProgress = true;
                    Cursor = Cursors.Wait;

                    await _demoRunner.MnistFiles.DownloadMissingFiles();
                    _demoRunner.MnistFiles.VerifyIntegrity();

                    Cursor = Cursors.Arrow;
                    _downloadInProgress = false;

                    _demoRunner.MnistFiles.PauseWatchingForIntegrity = false;
                });

            StartTraining = new RelayCommand(o => _mnistFilesIntegrity && !_trainInProgress, o =>
            {
                Cursor = Cursors.Wait;
                RecognitionTabIsEnabled = false;
                _trainInProgress = true;

                _demoRunner.Log += s => { TrainLog += s + "\n"; };

                _demoRunner.EpochComplete += epoch => { _demoRunner.SaveModel(); };

                Task.Run(() =>
                {
                    _demoRunner.Run(FirstHiddenLayerSize, SecondHiddenLayerSize, Iterations, LearningRate, BatchSize);
                    Cursor = Cursors.Arrow;
                    RecognitionTabIsEnabled = true;
                    _trainInProgress = false;
                });
            });

            ResetDrawing = new RelayCommand(o => true, o => { StrokeCollection.Clear(); });
            CanvasModified += RecognizeDigit;
        }

        public Cursor Cursor { get; set; }

        public ICommand TrainingDataDownload { get; }
        public ICommand StartTraining { get; }
        public ICommand ResetDrawing { get; }

        public string TrainLog { get; set; } = "Train log has begun.\n";


        public string DigitGuess { get; set; } = "Use your mouse to draw a digit on canvas above.";

        public bool RecognitionTabIsEnabled { get; set; } = true;

        public string TrainImagesFileStatus { get; set; }
        public string TrainLabelsFileStatus { get; set; }
        public string TestImagesFileStatus { get; set; }
        public string TestLabelsFileStatus { get; set; }

        public int FirstHiddenLayerSize { get; set; } = 60;
        public int SecondHiddenLayerSize { get; set; } = 30;
        public float LearningRate { get; set; } = 3.0f;
        public int BatchSize { get; set; } = 15;
        public int Iterations { get; set; } = 10;

        public StrokeCollection StrokeCollection { get; } = new StrokeCollection();

        public Brush InkCanvasBackground { get; set; } = Brushes.DarkSlateBlue;

        public event PropertyChangedEventHandler PropertyChanged;

        public event Action<Bitmap> CanvasModified;

        private void RecognizeDigit(Bitmap bitmap)
        {
            // Start recognition.
            var digit = _demoRunner.RecognizeDigit(bitmap, Color.DarkSlateBlue);
            DigitGuess = $"Is it {digit}?";
        }

        public virtual void OnCanvasModified(Bitmap obj)
        {
            CanvasModified?.Invoke(obj);
        }
    }
}