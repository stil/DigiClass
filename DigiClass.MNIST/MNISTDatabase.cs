using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DigiClass.MNIST.Helpers;

namespace DigiClass.MNIST
{
    public enum MNISTFileType
    {
        TrainImages,
        TrainLabels,
        TestImages,
        TestLabels
    }

    public class MNISTFile
    {
        public MNISTFile(MNISTFileType type, string url, string sha1)
        {
            Type = type;
            Url = url;
            Sha1 = sha1;
            FileName = Path.GetFileName(Url);
            ;
        }

        public MNISTFileType Type { get; }
        public string Url { get; }
        public string Sha1 { get; }
        public string FileName { get; }

        public bool VerifyIntegrity(out string status)
        {
            var fi = new FileInfo(FileName);
            var ok = false;
            if (!fi.Exists)
            {
                status = "not found";
            }
            else
            {
                try
                {
                    if (Sha1File.ValidateChecksum(Sha1, fi.FullName))
                    {
                        status = "OK";
                        ok = true;
                    }
                    else
                    {
                        status = "invalid checksum";
                    }
                }
                catch (IOException)
                {
                    status = "cant't read";
                }
            }

            return ok;
        }
    }

    public class MNISTDatabase
    {
        private readonly List<MNISTFile> _files = new List<MNISTFile>
        {
            new MNISTFile(
                MNISTFileType.TrainImages,
                "http://yann.lecun.com/exdb/mnist/train-images-idx3-ubyte.gz",
                "6c95f4b05d2bf285e1bfb0e7960c31bd3b3f8a7d"
            ),
            new MNISTFile(
                MNISTFileType.TrainLabels,
                "http://yann.lecun.com/exdb/mnist/train-labels-idx1-ubyte.gz",
                "2a80914081dc54586dbdf242f9805a6b8d2a15fc"
            ),
            new MNISTFile(
                MNISTFileType.TestImages,
                "http://yann.lecun.com/exdb/mnist/t10k-images-idx3-ubyte.gz",
                "c3a25af1f52dad7f726cce8cacb138654b760d48"
            ),
            new MNISTFile(
                MNISTFileType.TestLabels,
                "http://yann.lecun.com/exdb/mnist/t10k-labels-idx1-ubyte.gz",
                "763e7fa3757d93b0cdec073cef058b2004252c17"
            )
        };

        public bool PauseWatchingForIntegrity { get; set; }

        public MNISTFile FindByType(MNISTFileType type)
        {
            return _files.First(file => file.Type == type);
        }

        public event Action<MNISTFileType, string> StateChanged;
        public event Action<bool> IntegrityStatusUpdate;

        public void VerifyIntegrity()
        {
            var ok = 0;
            foreach (var file in _files)
            {
                if (file.VerifyIntegrity(out var msg))
                {
                    ok++;
                }

                StateChanged?.Invoke(file.Type, msg);
            }

            IntegrityStatusUpdate?.Invoke(ok == _files.Count);
        }

        public async Task DownloadMissingFiles()
        {
            var clients = new List<HttpClientDownloadWithProgress>();

            foreach (var file in _files)
            {
                if (file.VerifyIntegrity(out _))
                {
                    continue;
                }

                var fileName = Path.GetFileName(file.Url);
                var destinationFilePath = Path.GetFullPath(fileName);

                var client = new HttpClientDownloadWithProgress(file.Url, destinationFilePath);
                client.ProgressChanged += (totalFileSize, totalBytesDownloaded, progressPercentage) =>
                {
                    var status = progressPercentage.HasValue
                        ? $"[{Math.Round(progressPercentage.Value)}%]"
                        : "downloading...";

                    StateChanged?.Invoke(file.Type, status);
                };

                clients.Add(client);
            }

            var tasks = clients.Select(progress => progress.StartDownload()).ToList();
            await Task.WhenAll(tasks);

            foreach (var client in clients)
            {
                client.Dispose();
            }
        }

        public void WatchForIntegrity()
        {
            var watcher = new FileSystemWatcher
            {
                Path = Path.GetFullPath("."),
                Filter = "*.*"
            };
            FileSystemEventHandler callback = (sender, args) =>
            {
                if (PauseWatchingForIntegrity)
                {
                    return;
                }

                VerifyIntegrity();
            };
            watcher.Created += callback;
            watcher.Changed += callback;
            watcher.Deleted += callback;
            watcher.EnableRaisingEvents = true;
        }
    }
}