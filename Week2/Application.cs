using Autodesk.Revit.UI;
using System;
using System.Reflection;

namespace Week2
{
    public class Application : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            string tabName = "Electrical Tools";
            try
            {
                application.CreateRibbonTab(tabName);
            }
            catch (ArgumentException)
            {
                // Tab đã tồn tại, bỏ qua
            }

            RibbonPanel panel = application.CreateRibbonPanel(tabName, "Equipment");

            // Tạo Push Button
            string assemblyPath = Assembly.GetExecutingAssembly().Location;

            PushButtonData buttonData = new PushButtonData(
                "InsertEquipment",
                "Insert\nElement",
                assemblyPath,
                "Week2.InsertEquipmentCommand");

            PushButton button = panel.AddItem(buttonData) as PushButton;
            button.ToolTip = "Insert electrical equipment at origin";
             return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}