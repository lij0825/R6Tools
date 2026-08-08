namespace R6ServerChanger.Models
{
    public class UserAccount
    {
        public string Uuid { get; set; } = string.Empty;
        public string IniFilePath { get; set; } = string.Empty;
        
        // UI에 바인딩할 표시용 이름
        public string DisplayName => $"계정: {Uuid}";
    }
}
