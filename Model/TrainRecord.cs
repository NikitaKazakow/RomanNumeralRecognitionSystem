using MathNet.Numerics.LinearAlgebra;

namespace RomanNumeralRecognitionSystem.Model
{
    public class TrainRecord
    {
        public Vector<double> DataVector { get; }

        public Vector<double> TargetVector { get; }

        public TrainRecord(double[] data, int outputCount, int result)
        {
            this.DataVector = Vector<double>.Build.DenseOfArray(data);

            this.TargetVector = Vector<double>.Build.Dense(outputCount, 0.01);
            this.TargetVector[result - 1] = 0.99;
        }
    }
}