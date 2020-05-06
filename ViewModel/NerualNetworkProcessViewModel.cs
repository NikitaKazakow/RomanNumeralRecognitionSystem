using InkSharp;
using System.Windows;
using System.Drawing;
using System.Threading;
using InkSharp.Interfaces;
using System.Drawing.Imaging;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Windows.Ink;
using System.Windows.Input;
using MathNet.Numerics.LinearAlgebra;
using RomanNumeralRecognitionSystem.Util;
using RomanNumeralRecognitionSystem.Model;
using RomanNumeralRecognitionSystem.View.Pages.SubMenu;

namespace RomanNumeralRecognitionSystem.ViewModel
{
    public class NerualNetworkProcessViewModel : NerualNetworkViewModelBase
    {
        private static readonly object SLock = new object();
        private static NerualNetworkProcessViewModel _instance;
        private NerualNetworkProcessViewModel()
        {
            WaitAnimationVisibility = Visibility.Collapsed;
        }
        public static NerualNetworkProcessViewModel Instance
        {
            get
            {
                if (_instance != null) return _instance;
                Monitor.Enter(SLock);
                var temp = new NerualNetworkProcessViewModel();
                Interlocked.Exchange(ref _instance, temp);
                Monitor.Exit(SLock);

                return _instance;
            }
        }

        private string _recognitionResult;
        
        private RelayCommand _showLearningPageCommand;
        private RelayCommand _showRecognitionRelayCommand;
        private RelayCommand _showSettingsRelayCommand;
        private RelayCommand _saveNerualNetworkRelayCommand;
        private RelayCommand _recognizeRelayCommand;
        private RelayCommand _clearCanvasRelayCommand;

        private RelayCommand _changeParametersRelayCommand;

        private IDrawing _strokesCollection;

        private Page _currentPage;

        private NerualNetwork _nerualNetwork;

        private static Vector<double> GetInputVector(Bitmap bmp)
        {
            var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            var bmpData =
                bmp.LockBits(rect, ImageLockMode.ReadOnly,
                    bmp.PixelFormat);

            var bytes = bmpData.Stride * bmp.Height;
            var rgbValues = new byte[bytes];

            Marshal.Copy(bmpData.Scan0, rgbValues, 0, bytes);
            bmp.UnlockBits(bmpData);

            var result = new double[rgbValues.Length / 4];

            for (int i = 2, j = 0; i < rgbValues.Length; i += 4, j++)
            {
                if (rgbValues[i] > 0)
                    result[j] = 1.0;
                else
                    result[j] = 0.01;
            }

            return Vector<double>.Build.Dense(result);
        }

        public RelayCommand ShowLearningPageCommand
        {
            get
            {
                return _showLearningPageCommand ?? (_showLearningPageCommand = new RelayCommand(obj =>
                {
                    CurrentPage = new LearningPage();
                }));
            }
        }
        public RelayCommand ShowRecognitionRelayCommand
        {
            get
            {
                return _showRecognitionRelayCommand ?? (_showRecognitionRelayCommand = new RelayCommand(obj =>
                {
                    CurrentPage = new RecognitionPage();
                }));
            }
        }
        public RelayCommand ShowSettingsRelayCommand
        {
            get
            {
                return _showSettingsRelayCommand ?? (_showSettingsRelayCommand = new RelayCommand(obj =>
                {
                    CurrentPage = new SettingsPage();

                    Instance.OutputNeuronCont = NerualNetwork.HiddenLayersList[NerualNetwork.HiddenLayersList.Count - 1].RowCount;
                    Instance.HiddenLayersCount = NerualNetwork.HiddenLayersList.Count - 1;

                    Instance.HiddenLayersCollection = new ObservableCollection<HiddenLayerViewModel>();
                    for (var i = 0; i < NerualNetwork.HiddenLayersList.Count - 1; i++)
                    {
                        Instance.HiddenLayersCollection.Add(new HiddenLayerViewModel
                        {
                            Number = i + 1,
                            NeuronCount = NerualNetwork.HiddenLayersList[i].RowCount
                        });
                    }
                }));
            }
        }
        public RelayCommand ChangeParametersRelayCommand
        {
            get
            {
                return _changeParametersRelayCommand ?? (_changeParametersRelayCommand = new RelayCommand(obj =>
                {
                    CreateNerualNetwork();
                    SaveNerualNetwork(obj);
                }));
            }
        }

        public RelayCommand SaveNerualNetworkRelayCommand => _saveNerualNetworkRelayCommand ?? (_saveNerualNetworkRelayCommand = new RelayCommand(SaveNerualNetwork));

        public Page CurrentPage
        {
            get => _currentPage ?? (_currentPage = new LearningPage());
            set
            {
                _currentPage = value;
                OnPropertyChanged(nameof(CurrentPage));
            }
        }

        public RelayCommand RecognizeRelayCommand
        {
            get
            {
                return _recognizeRelayCommand ?? (_recognizeRelayCommand = new RelayCommand(obj =>
                {
                    var result = NerualNetwork.Query(GetInputVector(StrokesCollection.ToBitmap()));
                    RecognitionResult = result.MaximumIndex().ToString();
                }));
            }
        }

        public RelayCommand ClearCanvasRelayCommand
        {
            get
            {
                return _clearCanvasRelayCommand ?? (_clearCanvasRelayCommand = new RelayCommand(obj =>
                {
                    StrokesCollection = null;
                    RecognitionResult = null;
                }));
            }
        }

        public NerualNetwork NerualNetwork
        {
            get => _nerualNetwork;
            set
            {
                _nerualNetwork = value;
                OnPropertyChanged(nameof(NerualNetwork));
            }
        }

        public IDrawing StrokesCollection
        {
            get
            {
                if (_strokesCollection != null) return _strokesCollection;
                _strokesCollection = new Drawing();
                ((StrokeCollection)_strokesCollection).Add(
                    new Stroke(
                        new StylusPointCollection
                        {
                            new StylusPoint(0,0),
                            new StylusPoint(0,299),
                            new StylusPoint(299,299),
                            new StylusPoint(299,0),
                            new StylusPoint(0,0)
                        }));

                return _strokesCollection;
            }
            set
            {
                _strokesCollection = value;
                OnPropertyChanged(nameof(StrokesCollection));
            }
        }

        public string RecognitionResult
        {
            get => _recognitionResult ?? (_recognitionResult = "?");
            set
            {
                _recognitionResult = value;
                OnPropertyChanged(nameof(RecognitionResult));
            }
        }
    }
}