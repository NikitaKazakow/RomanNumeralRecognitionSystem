using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Threading;
using RomanNumeralRecognitionSystem.Model;
using RomanNumeralRecognitionSystem.Util;

namespace RomanNumeralRecognitionSystem.ViewModel
{
    public abstract class NerualNetworkViewModelBase : ViewModelBase
    {
        private static readonly object Locker = new object();
        public static int InputNodesCount { get; } = 90000;

        private static int _outputNeuronCount;

        private bool _isValid;
        private bool _isValidLayersCount;
        private bool _isValidNeuronCount;
        private bool _isValidOutputNeuronCount;

        private string _waitLabelText;
        private string _saveFolder;
        private string _saveName;

        private Visibility _folderErrorVisibility;
        private Visibility _nameErrorVisibility;
        private Visibility _fileAlreadyExistVisibility;
        private Visibility _waitAnimationVisibility;
        private Visibility _learningResultVisibility;
        
        private IFileService<NerualNetwork> FileService { get; }


        private ObservableCollection<HiddenLayerViewModel> _hiddenLayersCollection;

        public static double MaxHiddenLayersCount { get; } = 3;
        public static double MaxOutputNeuronCount { get; } = 100;

        public ObservableCollection<HiddenLayerViewModel> HiddenLayersCollection
        {
            get
            {
                if (_hiddenLayersCollection != null) return _hiddenLayersCollection;
                _hiddenLayersCollection = new ObservableCollection<HiddenLayerViewModel>
                {
                    new HiddenLayerViewModel
                    {
                        Number = 1,
                        NeuronCount = 1
                    }
                };
                return _hiddenLayersCollection;
            }
            set
            {
                _hiddenLayersCollection = value;
                OnPropertyChanged(nameof(HiddenLayersCollection));
            }
        }

        public bool IsValid
        {
            get
            {
                _isValid = IsValidNeuronCount && IsValidLayersCount && IsValidOutputNeuronCount;
                return _isValid;
            }
            set
            {
                _isValid = value;
                OnPropertyChanged(nameof(IsValid));
            }
        }
        public bool IsValidLayersCount
        {
            private get => _isValidLayersCount;
            set
            {
                _isValidLayersCount = value;
                OnPropertyChanged(nameof(IsValid));
            }
        }
        public bool IsValidNeuronCount
        {
            get => _isValidNeuronCount;
            set
            {
                _isValidNeuronCount = value;
                OnPropertyChanged(nameof(IsValid));
            }
        }
        public bool IsValidOutputNeuronCount
        {
            private get => _isValidOutputNeuronCount;
            set
            {
                _isValidOutputNeuronCount = value;
                OnPropertyChanged(nameof(IsValid));
            }
        }

        public int HiddenLayersCount
        {
            get => HiddenLayersCollection.Count;
            set
            {
                if (value <= 0 || value > MaxHiddenLayersCount)
                    return;
                IsValidLayersCount = true;
                var count = HiddenLayersCollection.Count;
                if (value > count)
                {
                    for (var i = 0; i < value - count; i++)
                        HiddenLayersCollection
                            .Add(new HiddenLayerViewModel
                            {
                                Number = count + (i * 1) + 1,
                                NeuronCount = 1
                            });
                }
                else if (value < count)
                {
                    for (var i = count - 1; i >= value; i--)
                        HiddenLayersCollection
                            .RemoveAt(i);
                }
                OnPropertyChanged(nameof(HiddenLayersCount));
            }
        }
        public int OutputNeuronCont
        {
            get
            {
                if (_outputNeuronCount == 0)
                    _outputNeuronCount = 1;
                return _outputNeuronCount;
            }
            set
            {
                _outputNeuronCount = value;
                IsValidOutputNeuronCount = true;
                OnPropertyChanged(nameof(OutputNeuronCont));
            }
        }

        public string WaitLabelText
        {
            get => _waitLabelText;
            set
            {
                _waitLabelText = value;
                OnPropertyChanged(nameof(WaitLabelText));
            }
        }

        public Visibility WaitAnimationVisibility
        {
            get => _waitAnimationVisibility;
            set
            {
                _waitAnimationVisibility = value;
                OnPropertyChanged(nameof(WaitAnimationVisibility));
            }
        }

        public Visibility LearningResultVisibility
        {
            get => _learningResultVisibility;
            set
            {
                _learningResultVisibility = value;
                OnPropertyChanged(nameof(LearningResultVisibility));
            }
        }

        public Visibility FolderErrorVisibility
        {
            get => _folderErrorVisibility;
            set
            {
                _folderErrorVisibility = value;
                OnPropertyChanged(nameof(FolderErrorVisibility));
            }
        }
        public Visibility NameErrorVisibility
        {
            get => _nameErrorVisibility;
            set
            {
                _nameErrorVisibility = value;
                OnPropertyChanged(nameof(NameErrorVisibility));
            }
        }
        public Visibility FileAlreadyExistVisibility
        {
            get => _fileAlreadyExistVisibility;
            set
            {
                _fileAlreadyExistVisibility = value;
                OnPropertyChanged(nameof(FileAlreadyExistVisibility));
            }
        }

        public string SaveFolder
        {
            get
            {
                if (_saveFolder != null) return _saveFolder;
                FolderErrorVisibility = Visibility.Collapsed;
                IsValidLayersCount = true;
                return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            set
            {
                if (_saveFolder == value)
                    return;
                try
                {
                    if (!Directory.Exists(Path.GetFullPath(value)))
                    {
                        IsValidLayersCount = false;
                        FolderErrorVisibility = Visibility.Visible;
                    }
                    else
                    {
                        IsValidLayersCount = true;
                        FolderErrorVisibility = Visibility.Collapsed;
                    }
                }
                catch (Exception)
                {
                    IsValidLayersCount = false;
                    FolderErrorVisibility = Visibility.Visible;
                }
                _saveFolder = value;
                OnPropertyChanged(nameof(SaveFolder));
            }
        }
        public string SaveName
        {
            get
            {
                if (_saveName != null) return _saveName;
                var i = 1;
                var name = "MyNerualNetwork" + i + ".nrnw";
                while (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    name)))
                {
                    i++;
                    name = "MyNerualNetwork" + i + ".nrnw";
                }
                _saveName = "MyNerualNetwork" + i;
                IsValidLayersCount = true;
                NameErrorVisibility =
                    FileAlreadyExistVisibility = Visibility.Collapsed;
                return _saveName;
            }
            set
            {
                if (_saveName == value)
                    return;
                if (value == "" ||
                    value.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                {
                    IsValidLayersCount = false;
                    NameErrorVisibility = Visibility.Visible;
                }
                else if (File.Exists(Path.Combine(SaveFolder, value + ".nrnw")))
                {
                    IsValidLayersCount = false;
                    FileAlreadyExistVisibility = Visibility.Visible;
                }
                else
                {
                    IsValidLayersCount = true;
                    NameErrorVisibility =
                        FileAlreadyExistVisibility = Visibility.Collapsed;
                }
                _saveName = value;
                OnPropertyChanged(nameof(SaveName));
            }
        }
        public static string SavePath { get; set; }


        protected NerualNetworkViewModelBase()
        {
            IsValidLayersCount = true;
            IsValidNeuronCount = true;
            IsValidOutputNeuronCount = true;

            FileService = new BinaryFileService();

        }

        protected void CreateNerualNetwork()
        {
            var thread = new Thread(() =>
            {
                WaitLabelText = "Создание нейронной сети...";
                WaitAnimationVisibility = Visibility.Visible;
                lock (Locker)
                {
                    NerualNetworkProcessViewModel.Instance.NerualNetwork = new NerualNetwork(InputNodesCount,
                        HiddenLayersCollection.Select(hiddenLayerViewModel => hiddenLayerViewModel.NeuronCount)
                            .ToList(),
                        OutputNeuronCont);
                    SavePath = Path.Combine(SaveFolder, SaveName + ".nrnw");
                }

                WaitAnimationVisibility = Visibility.Collapsed;
            }) {IsBackground = true};
            thread.Start();
        }

        protected void StartLearning(string trainSetName, double learningRate, int epochCount)
        {
            var thread = new Thread(obj =>
            {
                WaitLabelText = "Обучение нейронной сети...";
                WaitAnimationVisibility = Visibility.Visible;

                var trainSet = new List<TrainRecord>();
                using (var reader = new StreamReader(Path.GetFullPath("TrainData//" + trainSetName)))
                {
                    while (!reader.EndOfStream)
                    {
                        var values = reader.ReadLine()?.Split(',');

                        if (values != null && values.Length <= 1) continue;

                        var data = new double[values.Length - 1];

                        for (var i = 0; i < values.Length - 1; i++)
                        {
                            var value = double.Parse(values[i]);
                            if (value > 0)
                                data[i] = 1.0;
                            else
                                data[i] = 0.01;
                        }

                        var marker = int.Parse(values[90000]);

                        var temp = NerualNetworkProcessViewModel.Instance.NerualNetwork.HiddenLayersList.Count - 1;

                        trainSet.Add(new TrainRecord(data,
                            NerualNetworkProcessViewModel.Instance.NerualNetwork.HiddenLayersList[temp].RowCount,
                            marker));
                    }
                }

                var random = new Random();
                for (var i = trainSet.Count - 1; i >= 1; i--)
                {
                    var j = random.Next(i + 1);
                    var temp = trainSet[j];
                    trainSet[j] = trainSet[i];
                    trainSet[i] = temp;
                }

                for (var j = 0; j < 3; j++)
                {
                    switch (j)
                    {
                        case 0:
                            NerualNetworkProcessViewModel.Instance.NerualNetwork = new NerualNetwork(90000, new List<int>
                            {
                                25,
                                25,
                                50
                            }, 5);
                            break;
                        case 1:
                            NerualNetworkProcessViewModel.Instance.NerualNetwork = new NerualNetwork(90000, new List<int>
                            {
                                25,
                                50,
                                25
                            }, 5);
                            break;
                        case 2:
                            NerualNetworkProcessViewModel.Instance.NerualNetwork = new NerualNetwork(90000, new List<int>
                            {
                                50,
                                25,
                                25
                            }, 5);
                            break;
                    }

                    var ds = new Series
                    {
                        ChartType = SeriesChartType.Line,
                        BorderDashStyle = ChartDashStyle.Solid,
                        MarkerStyle = MarkerStyle.Diamond,
                        MarkerSize = 8,
                        BorderWidth = 2
                    };

                    var timer = new Stopwatch();
                    timer.Start();
                    for (var i = 0; i < epochCount; i++)
                    {
                        var errorValues = NerualNetworkProcessViewModel.Instance.NerualNetwork
                            .Train(trainSet, learningRate).Select(points => points.Item2).ToList();
                        ds.Points.AddY(StandardDeviation(errorValues));
                    }
                    timer.Stop();
                    var interval = TimeSpan.FromMilliseconds(timer.ElapsedMilliseconds);
                    var time = interval.Hours + " ч. " + interval.Minutes + " мин. " + interval.Seconds + " с. " + interval.Milliseconds + " мс ";
                    
                    ds.Name = "№" + (j + 1);
                    
                    Console.WriteLine(@"Эксперимент №" + (j + 1) + @" Время обучения " + time);
                    
                    ((Dispatcher)obj).BeginInvoke(new MethodInvoker(() =>
                    {
                        NerualNetworkProcessViewModel.Instance.LearningResultCollection.Add(ds);
                    }));
                }

                WaitAnimationVisibility = Visibility.Collapsed;
                LearningResultVisibility = Visibility.Visible;

            })
            { IsBackground = true };
            thread.Start(Dispatcher.CurrentDispatcher);
        }

        private static double StandardDeviation(IReadOnlyCollection<double> vector)
        {
            var sum = vector.Sum();
            var average = sum / vector.Count;

            var squaresQuery = vector.Select(value => (value - average) * (value - average));
            var sumOfSquares = squaresQuery.Sum();

            return Math.Sqrt(sumOfSquares / (vector.Count - 1));
        }

        protected void SaveNerualNetwork(object obj)
        {
            var thread = new Thread(() =>
                {
                    WaitLabelText = "Сохранение на диск...";
                    WaitAnimationVisibility = Visibility.Visible;
                    lock (Locker)
                    {
                        FileService.Save(SavePath, NerualNetworkProcessViewModel.Instance.NerualNetwork);
                    }
                    WaitAnimationVisibility = Visibility.Collapsed;
                    if (obj != null)
                        NavigationViewModel.Instance.ShowPageRelayCommand.Execute(obj);
                })
                { IsBackground = true };
            thread.Start();
        }
    }
}