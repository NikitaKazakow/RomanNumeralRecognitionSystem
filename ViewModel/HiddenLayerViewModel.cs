namespace RomanNumeralRecognitionSystem.ViewModel
{
    public class HiddenLayerViewModel
    {
        private int _neuronCount;
        public int Number { get; set; }

        public int NeuronCount
        {
            get => _neuronCount;
            set
            {
                if (value <= 0)
                    return;
                _neuronCount = value;
                ((CreateNerualNetworkViewModel)
                    NavigationViewModel.Instance.CurrentPage.DataContext).IsValidNeuronCount = true;
            }
        }
    }
}