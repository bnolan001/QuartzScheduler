namespace QuartzScheduler.Jobs.AviationWeather.Models.Enums
{
    public class JobMapKeysEnum
    {
        public static JobMapKeysEnum ICAO = new JobMapKeysEnum("icao", 1);

        public string Name { get; set; }

        public int Id { get; set; }

        public JobMapKeysEnum(string key, int id)
        {
            (Name, Id) = (key, id);
        }

        public List<JobMapKeysEnum> ToList()
        {
            return new List<JobMapKeysEnum> { ICAO };
        }

        public JobMapKeysEnum? GetByName(string name)
        {
            return ToList().FirstOrDefault(x => x.Name == name);
        }
    }
}
