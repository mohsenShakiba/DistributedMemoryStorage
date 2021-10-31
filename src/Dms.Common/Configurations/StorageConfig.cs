namespace Dms.Common.Configurations
{
    public class StorageConfig
    {
        public string DbFilePath { get; init; }
        public int VacuumPeriodInMinutes { get; init; }
        public float VacuumThreshold { get; init; }
    }
}