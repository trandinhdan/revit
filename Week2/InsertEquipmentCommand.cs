using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Week2
{
    [Transaction(TransactionMode.Manual)]
    public class InsertEquipmentCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                // Khởi tạo ExternalEvent nếu chưa có
                if (EquipmentCreationHandler.Instance == null)
                {
                    EquipmentCreationHandler.Initialize(commandData.Application);
                }

                // Mở WPF Window
                var window = new EquipmentWindow();
                window.Show();

                return Result.Succeeded;
            }
            catch (System.Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}