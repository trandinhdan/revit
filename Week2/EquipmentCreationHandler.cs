using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Week2.Models;
using System;
using System.Collections.Generic;

namespace Week2
{
    public class EquipmentCreationHandler : IExternalEventHandler
    {
        private static EquipmentCreationHandler _instance;
        private static ExternalEvent _externalEvent;
        private static UIApplication _uiApp;
        private EquipmentRequest _pendingRequest;

        public static EquipmentCreationHandler Instance => _instance;
        public static event EventHandler<EquipmentResult> EquipmentCreated;

        public static void Initialize(UIApplication uiApp)
        {
            if (_instance == null)
            {
                _uiApp = uiApp;
                _instance = new EquipmentCreationHandler();
                _externalEvent = ExternalEvent.Create(_instance);
            }
        }

        public void RequestEquipmentCreation(EquipmentRequest request)
        {
            _pendingRequest = request;
            _externalEvent?.Raise();
        }

        public void Execute(UIApplication app)
        {
            if (_pendingRequest == null) return;

            try
            {
                var result = CreateEquipment(app.ActiveUIDocument.Document, _pendingRequest);
                EquipmentCreated?.Invoke(this, result);
            }
            catch (Exception ex)
            {
                var errorResult = new EquipmentResult
                {
                    Success = false,
                    Message = ex.Message
                };
                EquipmentCreated?.Invoke(this, errorResult);
            }
            finally
            {
                _pendingRequest = null;
            }
        }

        private EquipmentResult CreateEquipment(Document doc, EquipmentRequest request)
        {
            using (Transaction trans = new Transaction(doc, "Create Equipment"))
            {
                trans.Start();

                TaskDialog.Show("Log", $"Đang tạo thiết bị: {request.Name}");


                try
                {
                    // Tạo DirectShape làm placeholder
                    DirectShape equipment = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_ElectricalEquipment));

                    // Tạo geometry (hộp đơn giản)
                    var solid = CreateEquipmentGeometry(request.Type);
                    equipment.SetShape(new GeometryObject[] { solid });

                    // Đặt tên
                    equipment.Name = $"{request.Name}";

                    // THÊM ELECTRICAL PARAMETERS
                    AddElectricalParameters(doc, equipment, request.Type);

                    // Vị trí ngẫu nhiên gần origin
                    Random rand = new Random();
                    double offsetX = (rand.NextDouble() - 0.5) * 10; // ±5 feet
                    double offsetY = (rand.NextDouble() - 0.5) * 10;

                    XYZ translation = new XYZ(offsetX, offsetY, 0);
                    ElementTransformUtils.MoveElement(doc, equipment.Id, translation);

                    trans.Commit();

                    return new EquipmentResult
                    {
                        Success = true,
                        Message = $"Đã tạo {request.Name} thành công với thông tin điện",
                        ElementId = (int)equipment.Id.Value
                    };
                }
                catch (Exception ex)
                {
                    trans.RollBack();
                    throw new Exception($"Không thể tạo thiết bị: {ex.Message}");
                }
            }
        }

        private void AddElectricalParameters(Document doc, DirectShape equipment, EquipmentType type)
        {
            try
            {
                // Lấy electrical properties based on equipment type
                var electricalProps = GetElectricalProperties(type);

                TaskDialog.Show("Log", $"Đang tạo thiết bị: {electricalProps}");


                // Thêm shared parameters hoặc project parameters
                AddParameter(doc, equipment, "Electrical_Voltage", electricalProps.Voltage, "Volts");
                AddParameter(doc, equipment, "Electrical_Current", electricalProps.Current, "Amperes");
                AddParameter(doc, equipment, "Electrical_Power", electricalProps.Power, "Watts");
                AddParameter(doc, equipment, "Electrical_Phases", electricalProps.Phases, "");
                AddParameter(doc, equipment, "Equipment_Rating", electricalProps.Rating, "kVA");
            }
            catch (Exception ex)
            {
                // Log error but don't fail the creation
                TaskDialog.Show("Warning", $"Cannot add electrical parameters: {ex.Message}");
            }
        }

        private ElectricalProperties GetElectricalProperties(EquipmentType type)
        {
            return type switch
            {
                EquipmentType.ElectricalPanel => new ElectricalProperties
                {
                    Voltage = 480,
                    Current = 400,
                    Power = 150000,
                    Phases = 3,
                    Rating = "200kVA"
                },
                EquipmentType.Transformer => new ElectricalProperties
                {
                    Voltage = 13800,
                    Current = 100,
                    Power = 1000000,
                    Phases = 3,
                    Rating = "1000kVA"
                },
                EquipmentType.Generator => new ElectricalProperties
                {
                    Voltage = 480,
                    Current = 600,
                    Power = 250000,
                    Phases = 3,
                    Rating = "300kVA"
                },
                EquipmentType.UPS => new ElectricalProperties
                {
                    Voltage = 480,
                    Current = 200,
                    Power = 75000,
                    Phases = 3,
                    Rating = "100kVA"
                },
                EquipmentType.SwitchGear => new ElectricalProperties
                {
                    Voltage = 480,
                    Current = 800,
                    Power = 300000,
                    Phases = 3,
                    Rating = "400kVA"
                },
                EquipmentType.MotorControlCenter => new ElectricalProperties
                {
                    Voltage = 480,
                    Current = 1200,
                    Power = 500000,
                    Phases = 3,
                    Rating = "600kVA"
                },
                _ => new ElectricalProperties { Voltage = 120, Current = 20, Power = 2000, Phases = 1, Rating = "5kVA" }
            };
        }

        private void AddParameter(Document doc, Element element, string paramName, object value, string unit)
        {
            // Tìm parameter existing hoặc tạo mới
            Parameter param = element.LookupParameter(paramName);

            if (param != null && !param.IsReadOnly)
            {
                if (value is double doubleVal)
                    param.Set(doubleVal);
                else if (value is int intVal)
                    param.Set(intVal);
                else if (value is string stringVal)
                    param.Set(stringVal);
            }
            else
            {
                // Có thể thêm vào Project Information hoặc tạo shared parameter
                // (Cần implementation phức tạp hơn)
            }
        }

        // Helper class cho electrical properties
        private class ElectricalProperties
        {
            public double Voltage { get; set; }
            public double Current { get; set; }
            public double Power { get; set; }
            public int Phases { get; set; }
            public string Rating { get; set; }
        }

        private Solid CreateEquipmentGeometry(EquipmentType type)
        {
            // Kích thước khác nhau cho từng loại thiết bị
            double width, height, depth;

            switch (type)
            {
                case EquipmentType.ElectricalPanel:
                    width = 2.0; height = 4.0; depth = 0.5;  // Tủ điện - mỏng, cao
                    break;
                case EquipmentType.Transformer:
                    width = 4.0; height = 5.0; depth = 3.0;  // Máy biến áp - lớn, vuông
                    break;
                case EquipmentType.Generator:
                    width = 6.0; height = 3.0; depth = 2.5;  // Máy phát - dài, thấp
                    break;
                case EquipmentType.UPS:
                    width = 2.5; height = 3.0; depth = 1.5;  // UPS - trung bình
                    break;
                case EquipmentType.SwitchGear:
                    width = 3.0; height = 6.0; depth = 1.0;  // Tủ phân phối - cao, mỏng
                    break;
                case EquipmentType.MotorControlCenter:
                    width = 5.0; height = 7.0; depth = 2.0;  // MCC - rất cao
                    break;
                default:
                    width = height = depth = 1.0;
                    break;
            }

            // Tạo CurveLoop cho hình chữ nhật thủ công
            List<Curve> curves = new List<Curve>();

            double halfWidth = width / 2;
            double halfDepth = depth / 2;

            // Tạo 4 cạnh của hình chữ nhật
            XYZ p1 = new XYZ(-halfWidth, -halfDepth, 0);
            XYZ p2 = new XYZ(halfWidth, -halfDepth, 0);
            XYZ p3 = new XYZ(halfWidth, halfDepth, 0);
            XYZ p4 = new XYZ(-halfWidth, halfDepth, 0);

            curves.Add(Line.CreateBound(p1, p2));
            curves.Add(Line.CreateBound(p2, p3));
            curves.Add(Line.CreateBound(p3, p4));
            curves.Add(Line.CreateBound(p4, p1));

            CurveLoop curveLoop = CurveLoop.Create(curves);

            // Tạo solid bằng extrusion
            return GeometryCreationUtilities.CreateExtrusionGeometry(
                new CurveLoop[] { curveLoop },
                XYZ.BasisZ,
                height);
        }

        public string GetName()
        {
            return "Equipment Creation Handler";
        }
    }
}