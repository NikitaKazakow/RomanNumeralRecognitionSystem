using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using RomanNumeralRecognitionSystem.Annotations;

namespace RomanNumeralRecognitionSystem.Model
{
    /// <summary>
    /// Класс нейронной сети
    /// </summary>
    [Serializable]
    public class NerualNetwork : INotifyPropertyChanged
    {
        /// <summary>
        /// Список скрытых слоев
        /// </summary>
        private List<Matrix<double>> _hiddenLayerList;

        public List<Matrix<double>> HiddenLayersList
        {
            get => _hiddenLayerList;
            set
            {
                _hiddenLayerList = value;
                OnPropertyChanged(nameof(HiddenLayersList));
            }
        }

        /// <summary>
        /// Создает экземпляр класса <see cref="NerualNetwork"/>
        /// </summary>
        /// <param name="inputNodes">Количество нейронов в входном слое</param>
        /// <param name="hiddenNodesCountList">Список количества нейронов в i-ом скрытом слое</param>
        /// <param name="outputNodes">Количество нейронов в выходном слое</param>
        public NerualNetwork(int inputNodes, IReadOnlyList<int> hiddenNodesCountList, int outputNodes)
        {
            Console.WriteLine("Создание нейронной сети...");

            _hiddenLayerList = new List<Matrix<double>>();

            for (var i = 0; i <= hiddenNodesCountList.Count; i++)
            {
                Matrix<double> matrix;
                if (i == 0)
                {
                    matrix = Matrix<double>.Build.Random(
                        hiddenNodesCountList[i],
                        inputNodes,
                        new Normal(0.0, Math.Pow(hiddenNodesCountList[i], -0.5)));
                }
                else if (i == hiddenNodesCountList.Count)
                {
                    matrix = Matrix<double>.Build.Random(
                        outputNodes,
                        hiddenNodesCountList[i - 1],
                        new Normal(0.0, Math.Pow(outputNodes, -0.5)));
                }
                else
                {
                    matrix = Matrix<double>.Build.Random(
                        hiddenNodesCountList[i],
                        hiddenNodesCountList[i - 1],
                        new Normal(0.0, Math.Pow(hiddenNodesCountList[i], -0.5)));
                }
                _hiddenLayerList.Add(matrix);
            }
            Console.WriteLine("Нейронная сеть создана!");
        }

        private static Vector<double> Sigmoid(Vector<double> vector)
        {
            for (var i = 0; i < vector.Count; i++)
                vector[i] = 1.0d / (1.0d + Math.Exp(-vector[i]));
            return vector;
        }

        private static double StandardDeviation(ICollection<double> vector)
        {
            var sum = vector.Sum();
            var average = sum / vector.Count;

            var squaresQuery = vector.Select(value => (value - average) * (value - average));
            var sumOfSquares = squaresQuery.Sum();

            return Math.Sqrt(sumOfSquares / (vector.Count - 1));
        }

        /// <summary>
        /// Функция опроса нейронной сети
        /// </summary>
        /// <param name="inputVector">Вектор входных данных</param>
        /// <returns>Результирующий вектор - ответ нейронной сети</returns>
        public Vector<double> Query(Vector<double> inputVector)
        {
            var result = Sigmoid(_hiddenLayerList[0].Multiply(inputVector));
            if (_hiddenLayerList.Count <= 1) return result;
            for (var i = 1; i < _hiddenLayerList.Count; i++)
                result = Sigmoid(_hiddenLayerList[i].Multiply(result));
            return result;
        }

        /// <summary>
        /// Функция обучения нейронной сети
        /// </summary>
        /// <param name="trainRecords">Список с записями для тренировки <see cref="TrainRecord"/></param>
        /// <param name="learningRate">Коэффициент скорости обучения</param>
        public void Train(List<TrainRecord> trainRecords, double learningRate)
        {
            foreach (var trainRecord in trainRecords)
            {
                var errorList = new List<Vector<double>>();
                var valuesList = new List<Vector<double>> { trainRecord.DataVector };

                var result = Sigmoid(_hiddenLayerList[0].Multiply(trainRecord.DataVector));
                valuesList.Add(result);

                if (_hiddenLayerList.Count > 1)
                {
                    for (var i = 1; i < _hiddenLayerList.Count; i++)
                    {
                        result = Sigmoid(_hiddenLayerList[i].Multiply(result));
                        valuesList.Add(result);
                    }
                }

                errorList.Add(trainRecord.TargetVector.Subtract(result));
                if (_hiddenLayerList.Count <= 1) continue;
                {
                    for (int i = _hiddenLayerList.Count - 1, j = 0; i > 0; i--, j++)
                    {
                        errorList.Add(_hiddenLayerList[i].Transpose().Multiply(errorList[j]));
                    }
                }

                errorList.Reverse();

                for (var i = _hiddenLayerList.Count - 1; i >= 0; i--)
                {
                    _hiddenLayerList[i] += errorList[i].PointwiseMultiply(valuesList[i + 1])
                        .PointwiseMultiply(valuesList[i + 1].SubtractFrom(1.0))
                        .ToColumnMatrix().Multiply(valuesList[i].ToRowMatrix()).Multiply(learningRate);
                }
                Console.WriteLine(trainRecord.DataVector);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}