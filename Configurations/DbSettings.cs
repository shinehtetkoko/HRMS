namespace HRMS.Configurations
{
    public class DbSettings
    {
        public string? Host { get; set; }
        public int? Port { get; set; }
        public string? TargetDb { get; set; }
        public string? Username { get; set; }
        public string?   Password { get; set; }

        public string ConnectionString => $"Host={Host};Port={Port};Database={TargetDb};Username={Username};Password={Password};";
    }
}
