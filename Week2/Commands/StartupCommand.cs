using Autodesk.Revit.Attributes;
using Nice3point.Revit.Toolkit.External;
using Week2.ViewModels;
using Week2.Views;

namespace Week2.Commands
{
    /// <summary>
    ///     External command entry point
    /// </summary>
    [UsedImplicitly]
    [Transaction(TransactionMode.Manual)]
    public class StartupCommand : ExternalCommand
    {
        public override void Execute()
        {
            var viewModel = new Week2ViewModel();
            var view = new Week2View(viewModel);
            view.ShowDialog();
        }
    }
}