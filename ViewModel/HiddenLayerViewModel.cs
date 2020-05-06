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
                var viewModel = NavigationViewModel.Instance.CurrentPage.DataContext;
                switch (viewModel)
                {
                    case CreateNerualNetworkViewModel model:
                        model.IsValidNeuronCount = true;
                        break;
                    case NerualNetworkProcessViewModel model:
                        model.IsValidNeuronCount = true;
                        break;
                }

            }
        }
    }
}