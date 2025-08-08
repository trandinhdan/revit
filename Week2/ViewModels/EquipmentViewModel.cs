using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Week2.Models;
using System.Windows; // Add this for MessageBox

namespace Week2.ViewModels
{
    public class EquipmentViewModel : INotifyPropertyChanged
    {
        private EquipmentType _selectedEquipmentType;
        private string _statusMessage = "Sẵn sàng tạo thiết bị";
        private bool _isCreating = false;

        public EquipmentViewModel()
        {
            // Khởi tạo danh sách thiết bị
            AvailableEquipments = new ObservableCollection<EquipmentTypeItem>
            {
                new EquipmentTypeItem { Type = EquipmentType.ElectricalPanel, DisplayName = "Tủ điện" },
                new EquipmentTypeItem { Type = EquipmentType.Transformer, DisplayName = "Máy biến áp" },
                new EquipmentTypeItem { Type = EquipmentType.Generator, DisplayName = "Máy phát điện" },
                new EquipmentTypeItem { Type = EquipmentType.UPS, DisplayName = "Bộ lưu điện UPS" },
                new EquipmentTypeItem { Type = EquipmentType.SwitchGear, DisplayName = "Tủ phân phối" },
                new EquipmentTypeItem { Type = EquipmentType.MotorControlCenter, DisplayName = "Trung tâm điều khiển động cơ" }
            };

            SelectedEquipmentType = EquipmentType.ElectricalPanel;

            // Khởi tạo Command
            CreateEquipmentCommand = new RelayCommand(ExecuteCreateEquipment, CanExecuteCreateEquipment);

            // Đăng ký sự kiện từ Handler
            EquipmentCreationHandler.EquipmentCreated += OnEquipmentCreated;
        }

        public ObservableCollection<EquipmentTypeItem> AvailableEquipments { get; }

        public EquipmentType SelectedEquipmentType
        {
            get => _selectedEquipmentType;
            set
            {
                _selectedEquipmentType = value;
                OnPropertyChanged();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public bool IsCreating
        {
            get => _isCreating;
            set
            {
                _isCreating = value;
                OnPropertyChanged();
                ((RelayCommand)CreateEquipmentCommand).RaiseCanExecuteChanged();
            }
        }

        public ICommand CreateEquipmentCommand { get; }

        private void ExecuteCreateEquipment(object parameter)
        {
            IsCreating = true;
            StatusMessage = "Đang tạo thiết bị...";

            var request = new EquipmentRequest
            {
                Type = SelectedEquipmentType,
                Name = GetEquipmentDisplayName(SelectedEquipmentType)
            };

            // Gọi ExternalEvent (không gọi trực tiếp Revit API)
            EquipmentCreationHandler.Instance?.RequestEquipmentCreation(request);
        }

        private bool CanExecuteCreateEquipment(object parameter)
        {
            return !IsCreating;
        }

        private void OnEquipmentCreated(object sender, EquipmentResult result)
        {
            IsCreating = false;

            if (result.Success)
            {
                StatusMessage = $"✅ {result.Message} (ID: {result.ElementId})";
                MessageBox.Show(StatusMessage, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                StatusMessage = $"❌ Thất bại: {result.Message}";
                MessageBox.Show(StatusMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetEquipmentDisplayName(EquipmentType type)
        {
            return type switch
            {
                EquipmentType.ElectricalPanel => "Tủ điện",
                EquipmentType.Transformer => "Máy biến áp",
                EquipmentType.Generator => "Máy phát điện",
                EquipmentType.UPS => "Bộ lưu điện UPS",
                EquipmentType.SwitchGear => "Tủ phân phối",
                EquipmentType.MotorControlCenter => "Trung tâm điều khiển động cơ",
                _ => "Thiết bị điện"
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class EquipmentTypeItem
    {
        public EquipmentType Type { get; set; }
        public string DisplayName { get; set; }
    }
}