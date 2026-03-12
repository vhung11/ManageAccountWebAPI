namespace ManageAccountWebAPI.Data.Entities
{
    public class AppLog
    {
        public long Id { get; set; }
        public DateTime LoggedAt { get; set; }
        public string LogLevel { get; set; } = string.Empty;
        public string? Logger { get; set; }
        public string? Message { get; set; }
        public string? Exception { get; set; }
        public string? Properties { get; set; }
        public string? MachineName { get; set; }
        public string? AppName { get; set; }
    }
}
