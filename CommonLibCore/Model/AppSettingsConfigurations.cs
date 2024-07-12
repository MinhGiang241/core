using System.Collections.Generic;

namespace CommonLibCore.Model
{
    public class AppSettingsConfiguration
    {
        public RedisOptions Redis { get; set; }
        public Cache Cache { get; set; }
    }

    public class RedisOptions
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Passwword { get; set; }
        public int Db { get; set; }
    }

    public class Cache
    {
        public int PermissionTTL { get; set; }
    }

    public class SentinelOptions
    {
        public bool AllowAdmin { get; set; }
        public string ServiceName { get; set; }
        public int KeepAlive { get; set; }
        public bool AbortOnConnectFail { get; set; }
        public int ConnectRetry { get; set; }
        public int ConnectTimeout { get; set; }
        public int SyncTimeout { get; set; }
        public string[] EndPoints { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int DataBase { get; set; }
    }
}
