using Assistant.Utils;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;

namespace Assistant.Model.ServiceManager;

// ReSharper disable once InconsistentNaming
public partial class RedisService : ServiceBase
{
    public override async Task Install(Action<string>? infoAction = null)
    {
        Guard.IsNotNullOrEmpty(BinPath);
        Guard.IsNotNullOrEmpty(ServiceName);
        Guard.IsNotNullOrEmpty(ConfigFilePath);
        Guard.IsNotNullOrEmpty(ServiceDirectory);
        Guard.IsNotNullOrEmpty(LogDirectory);

        // 备份配置文件
        if (File.Exists(ConfigFilePath))
        {
            var rs = await FileUtils.BackupFile(ConfigFilePath);
            infoAction?.Invoke($"备份配置文件 [{rs}]");
        }

        // 更新配置文件
        var text = GetConfigText();
        await File.WriteAllTextAsync(ConfigFilePath, text, new UTF8Encoding(false));
        infoAction?.Invoke($"更新配置文件 [{ConfigFilePath}]");

        // 写入ins.conf
        var insConfPath = Path.Combine(ServiceDirectory, Global.InstallConfFileName);
        await FileUtils.WriteToFile(this, insConfPath);
        infoAction?.Invoke($"写入ins.conf: [{insConfPath}]");

        // 创建Windows服务
        var binPath = @$"""{BinPath}"" --service-run ""{ConfigFilePath}""";
        await WinServiceUtils.CreateService(binPath, ServiceName, ServiceDescription ?? string.Empty, ServiceDescription ?? string.Empty);
        infoAction?.Invoke($"windows 服务创建成功: [{ServiceName}]");
    }

    public override async Task UnInstall(Action<string>? infoAction = null)
    {
        Guard.IsNotNullOrEmpty(ServiceName);

        infoAction?.Invoke($"删除Windows服务 [{ServiceName}]");
        await WinServiceUtils.DeleteService(ServiceName);
    }
}

public partial class RedisService
{
    public RedisService()
    {
        var baseDir = Path.Combine(Global.CurrentDir, "Services", "Redis");
        DisplayName        = "Redis";
        Version            = "5.0.14.1";
        ServiceName        = "Neuz.Redis";
        BinPath            = Path.Combine(baseDir, "redis-server.exe");
        ServiceDescription = "Neuz.Redis 服务";
        ServiceDirectory   = baseDir;
        LogDirectory       = baseDir;
        ConfigFilePath     = Path.Combine(baseDir, "redis.conf");
        Installed          = false;
        RunningStatus      = RunningStatus.UnKnown;
        RedisConfig        = new RedisConfigModel();
    }

    public RedisConfigModel RedisConfig { get; set; }

    public string GetConfigText()
    {
        return string.Format(RedisConfig.ConfigText, RedisConfig.Port);
    }

    public class RedisConfigModel
    {
        public int Port { get; set; } = 10002;

        internal string ConfigText = @"bind 127.0.0.1
protected-mode yes
port 10002
tcp-backlog 511
timeout 0
tcp-keepalive 300
loglevel notice
logfile ""redis_server.log""
syslog-enabled yes
syslog-ident redis
databases 16
always-show-logo yes
save 900 1
save 300 10
save 60 10000
stop-writes-on-bgsave-error yes
rdbcompression yes
rdbchecksum yes
dbfilename dump.rdb
dir ./
replica-serve-stale-data yes
replica-read-only yes
repl-diskless-sync no
repl-diskless-sync-delay 5
repl-disable-tcp-nodelay no
replica-priority 100
lazyfree-lazy-eviction no
lazyfree-lazy-expire no
lazyfree-lazy-server-del no
replica-lazy-flush no
appendonly no
appendfilename ""appendonly.aof""
appendfsync everysec
no-appendfsync-on-rewrite no
auto-aof-rewrite-percentage 100
auto-aof-rewrite-min-size 64mb
aof-load-truncated yes
aof-use-rdb-preamble yes
lua-time-limit 5000
slowlog-log-slower-than 10000
slowlog-max-len 128
latency-monitor-threshold 0
notify-keyspace-events """"
hash-max-ziplist-entries 512
hash-max-ziplist-value 64
list-max-ziplist-size -2
list-compress-depth 0
set-max-intset-entries 512
zset-max-ziplist-entries 128
zset-max-ziplist-value 64
hll-sparse-max-bytes 3000
stream-node-max-bytes 4096
stream-node-max-entries 100
activerehashing yes
client-output-buffer-limit normal 0 0 0
client-output-buffer-limit replica 256mb 64mb 60
client-output-buffer-limit pubsub 32mb 8mb 60
hz 10
dynamic-hz yes
aof-rewrite-incremental-fsync yes
rdb-save-incremental-fsync yes";
    }
}