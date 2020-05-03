using System.Windows.Controls;
using RomanNumeralRecognitionSystem.ViewModel;

namespace RomanNumeralRecognitionSystem.View.Pages.CreationWizard
{
    /// <summary>
    /// Логика взаимодействия для NerualNetworkParamsPage.xaml
    /// </summary>
    public partial class NerualNetworkParamsPage : Page
    {
        public NerualNetworkParamsPage()
        {
            InitializeComponent();
        }

        private void Validation_OnErrorHiddenLayersCount(object sender, ValidationErrorEventArgs e)
        {
            var viewModel = (CreateNerualNetworkViewModel)DataContext;
            viewModel.IsValidLayersCount = false;
        }

        private void Validation_OnErrorNeuronCount(object sender, ValidationErrorEventArgs e)
        {
            var viewModel = (CreateNerualNetworkViewModel)DataContext;
            viewModel.IsValidNeuronCount = false;
        }
    }
}
