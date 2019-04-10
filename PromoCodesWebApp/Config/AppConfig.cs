namespace PromoCodesWebApp.Config
{
    public class AppConfig
    {
        public MongoDBConfig MongoDB { get; set; } = new MongoDBConfig();
        public RedisConfig Redis { get; set; } = new RedisConfig();
    }
}
