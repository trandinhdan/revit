using Week2.ViewModels;

namespace Week2.Views
{
    public sealed partial class Week2View
    {
        public Week2View(Week2ViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}