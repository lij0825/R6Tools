using CommunityToolkit.Mvvm.ComponentModel;

namespace R6ServerChanger.Models
{
    public partial class DataCenterInfo : ObservableObject
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        
        // 핑 측정을 위한 대표 IP (AWS/Azure 등)
        public string TestIp { get; set; } = string.Empty;
        
        [ObservableProperty]
        private string pingText = "-";
    }
}
