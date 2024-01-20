using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assistant.Model;
using CliWrap;
using CliWrap.Buffered;
using Dapper;
using MySqlConnector;
using Serilog;

namespace Assistant.Services;

public class StatService
{
    public async Task<IList<Stats>> GetMySQLStatsList(string userName, string password, string host = "localhost", int port = 10001)
    {
        var rs = new List<Stats>();

        await Task.Run(() =>
        {
            try
            {
                var sb = new MySqlConnectionStringBuilder()
                {
                    Server            = host,
                    Port              = Convert.ToUInt32(port),
                    UserID            = userName,
                    Password          = password,
                    ConnectionTimeout = Convert.ToUInt32(10)
                };
                using var conn = new MySqlConnection(sb.ToString());
                conn.Open();


                // 变量详细参考
                // https://dev.mysql.com/doc/refman/5.7/en/server-system-variables.html
                var sql = "SHOW GLOBAL VARIABLES";
                var tmp = conn.Query(sql)
                              .Select(s => new KeyValuePair<string, string>(s.Variable_name, s.Value))
                              .ToList();
                foreach (var t in tmp)
                {
                    if (t.Key.Equals("bind_address", StringComparison.OrdinalIgnoreCase))
                        rs.Add(new Stats {Caption = "绑定地址", Key = t.Key, Value = t.Value, Order = 0});
                    if (t.Key.Equals("port", StringComparison.OrdinalIgnoreCase))
                        rs.Add(new Stats {Caption = "端口", Key = t.Key, Value = t.Value, Order = 0});
                    if (t.Key.Equals("hostname", StringComparison.OrdinalIgnoreCase))
                        rs.Add(new Stats {Caption = "服务器名称", Key = t.Key, Value = t.Value, Order = 0});
                    if (t.Key.Equals("version_compile_os", StringComparison.OrdinalIgnoreCase))
                        rs.Add(new Stats {Caption = "操作系统类型", Key = t.Key, Value = t.Value, Order = 1});
                    if (t.Key.Equals("version_compile_machine", StringComparison.OrdinalIgnoreCase))
                        rs.Add(new Stats {Caption = "服务器类型", Key = t.Key, Value = t.Value, Order = 1});
                    if (t.Key.Equals("version", StringComparison.OrdinalIgnoreCase))
                        rs.Add(new Stats {Caption = "MySQL版本", Key = t.Key, Value = t.Value, Order = 1});
                    if (t.Key.Equals("version_comment", StringComparison.OrdinalIgnoreCase))
                        rs.Add(new Stats {Caption = "版本描述", Key = t.Key, Value = t.Value, Order = 1});
                    if (t.Key.Equals("license", StringComparison.OrdinalIgnoreCase))
                        rs.Add(new Stats {Caption = "许可证", Key = t.Key, Value = t.Value, Order = 1});
                    if (t.Key.Equals("wait_timeout", StringComparison.OrdinalIgnoreCase))
                        rs.Add(new Stats {Caption = "超时等待", Key = t.Key, Value = t.Value, Order = 2});
                    if (t.Key.Equals("basedir", StringComparison.OrdinalIgnoreCase))
                        rs.Add(new Stats {Caption = "安装目录", Key = t.Key, Value = t.Value, Order = 2});
                    if (t.Key.Equals("datadir", StringComparison.OrdinalIgnoreCase))
                        rs.Add(new Stats {Caption = "数据目录", Key = t.Key, Value = t.Value, Order = 2});
                    if (t.Key.Equals("tmpdir", StringComparison.OrdinalIgnoreCase))
                        rs.Add(new Stats {Caption = "临时目录", Key = t.Key, Value = t.Value, Order = 2});
                    if (t.Key.Equals("log_error", StringComparison.OrdinalIgnoreCase))
                        rs.Add(new Stats {Caption = "错误日志路径", Key = t.Key, Value = t.Value, Order = 2});
                    if (t.Key.Equals("max_connections", StringComparison.OrdinalIgnoreCase))
                        rs.Add(new Stats {Caption = "最大连接数", Key = t.Key, Value = t.Value, Order = 3});
                    if (t.Key.Equals("Max_used_connections", StringComparison.OrdinalIgnoreCase))
                        rs.Add(new Stats {Caption = "最大响应连接数", Key = t.Key, Value = t.Value, Order = 4});
                    if (t.Key.Equals("character_set_server", StringComparison.OrdinalIgnoreCase))
                        rs.Add(new Stats {Caption = "服务器字符集", Key = t.Key, Value = t.Value});
                    if (t.Key.Equals("character_set_client", StringComparison.OrdinalIgnoreCase))
                        rs.Add(new Stats {Caption = "客户端字符集", Key = t.Key, Value = t.Value});
                    if (t.Key.Equals("character_sets_dir", StringComparison.OrdinalIgnoreCase))
                        rs.Add(new Stats {Caption = "字符集目录", Key = t.Key, Value = t.Value});
                    if (t.Key.Equals("collation_server", StringComparison.OrdinalIgnoreCase))
                        rs.Add(new Stats {Caption = "服务器默认排序", Key = t.Key, Value = t.Value});
                    if (t.Key.Equals("connect_timeout", StringComparison.OrdinalIgnoreCase))
                        rs.Add(new Stats {Caption = "连接超时(秒)", Key = t.Key, Value = t.Value});
                    if (t.Key.Equals("max_allowed_packet", StringComparison.OrdinalIgnoreCase))
                        rs.Add(new Stats {Caption = "最大数据包", Key = t.Key, Value = t.Value});
                }


                // 参数详细参考
                // https://dev.mysql.com/doc/refman/8.0/en/server-status-variables.html
                sql = "SHOW GLOBAL STATUS";
                tmp = conn.Query(sql)
                          .Select(s => new KeyValuePair<string, string>(s.Variable_name, s.Value))
                          .ToList();
                foreach (var t in tmp)
                {
                    if (t.Key.Equals("Uptime", StringComparison.OrdinalIgnoreCase))
                        rs.Add(new Stats {Caption = "启动时间(秒)", Key = t.Key, Value = t.Value, Order = 0});
                    if (t.Key.Equals("Bytes_received", StringComparison.OrdinalIgnoreCase))
                        rs.Add(new Stats {Caption = "所有接收的字节数", Key = t.Key, Value = t.Value, Order = 3});
                    if (t.Key.Equals("Bytes_sent", StringComparison.OrdinalIgnoreCase))
                        rs.Add(new Stats {Caption = "所有发送的字节数", Key = t.Key, Value = t.Value, Order = 3});
                }
            }
            catch (Exception e)
            {
                Log.Error($"获取MYSQL运行状态失败");
                Log.Error(e.Message);
            }
        });

        return rs;
    }


    public async Task<IList<Stats>> GetRedisStatsList(string cliPath, string host = "localhost", int port = 10002)
    {
        var rs = new List<Stats>();
        var executeBuffered = await Cli.Wrap(cliPath)
                                       .WithArguments(new[] {"-h", host, "-p", $"{port}", "INFO"})
                                       .ExecuteBufferedAsync();
        if (executeBuffered.ExitCode != 0) return rs;

        try
        {
            var source = executeBuffered.StandardOutput.Split(Environment.NewLine)
                                        .Select(l => l.Trim())
                                        .Where(l => l.Contains(":"))
                                        .ToList();

            foreach (var s in source)
            {
                var split = s.Split(":");
                if (split.Length < 2) continue;
                var key   = split[0];
                var value = string.Join(":", split[1..]);

                if (key.Equals("tcp_port", StringComparison.OrdinalIgnoreCase))
                    rs.Add(new Stats {Caption = "TCP端口", Key = key, Value = value, Order = 0});
                if (key.Equals("process_id", StringComparison.OrdinalIgnoreCase))
                    rs.Add(new Stats {Caption = "Process Id", Key = key, Value = value, Order = 1});
                if (key.Equals("redis_version", StringComparison.OrdinalIgnoreCase))
                    rs.Add(new Stats {Caption = "Redis版本", Key = key, Value = value, Order = 2});
                if (key.Equals("gcc_version", StringComparison.OrdinalIgnoreCase))
                    rs.Add(new Stats {Caption = "GCC 版本", Key = key, Value = value, Order = 3});
                if (key.Equals("redis_mode", StringComparison.OrdinalIgnoreCase))
                    rs.Add(new Stats {Caption = "运行模式", Key = key, Value = value, Order = 4});
                if (key.Equals("executable", StringComparison.OrdinalIgnoreCase))
                    rs.Add(new Stats {Caption = "服务执行路径", Key = key, Value = value, Order = 5});
                if (key.Equals("config_file", StringComparison.OrdinalIgnoreCase))
                    rs.Add(new Stats {Caption = "服务配置路径", Key = key, Value = value, Order = 6});
                if (key.Equals("os", StringComparison.OrdinalIgnoreCase))
                    rs.Add(new Stats {Caption = "宿主操作系统", Key = key, Value = value, Order = 8});
                if (key.Equals("arch_bits", StringComparison.OrdinalIgnoreCase))
                    rs.Add(new Stats {Caption = "操作系统类型", Key = key, Value = value, Order = 9});
                if (key.Equals("uptime_in_seconds", StringComparison.OrdinalIgnoreCase))
                    rs.Add(new Stats {Caption = "运行时间(秒)", Key = key, Value = value, Order = 10});
                if (key.Equals("uptime_in_days", StringComparison.OrdinalIgnoreCase))
                    rs.Add(new Stats {Caption = "运行时间(天)", Key = key, Value = value, Order = 11});
                if (key.Equals("connected_clients", StringComparison.OrdinalIgnoreCase))
                    rs.Add(new Stats {Caption = "连接客户端数量", Key = key, Value = value});
                if (key.Equals("blocked_clients", StringComparison.OrdinalIgnoreCase))
                    rs.Add(new Stats {Caption = "阻塞客户端数量", Key = key, Value = value});
                if (key.Equals("used_memory_human", StringComparison.OrdinalIgnoreCase))
                    rs.Add(new Stats {Caption = "当前使用内存", Key = key, Value = value});
                if (key.Equals("used_memory_human", StringComparison.OrdinalIgnoreCase))
                    rs.Add(new Stats {Caption = "内存使用峰值", Key = key, Value = value});
                if (key.Equals("total_system_memory_human", StringComparison.OrdinalIgnoreCase))
                    rs.Add(new Stats {Caption = "系统内存", Key = key, Value = value});
                if (key.Equals("maxmemory_human", StringComparison.OrdinalIgnoreCase))
                    rs.Add(new Stats {Caption = "最大内存配置", Key = key, Value = value});
                if (key.Equals("aof_enabled", StringComparison.OrdinalIgnoreCase))
                    rs.Add(new Stats {Caption = "AOF是否开启", Key = key, Value = value});
                if (key.Equals("total_connections_received", StringComparison.OrdinalIgnoreCase))
                    rs.Add(new Stats {Caption = "自启动起连接总数", Key = key, Value = value});
                if (key.Equals("total_commands_processed", StringComparison.OrdinalIgnoreCase))
                    rs.Add(new Stats {Caption = "自启动起运行命令总数", Key = key, Value = value});
                if (key.Equals("instantaneous_ops_per_sec", StringComparison.OrdinalIgnoreCase))
                    rs.Add(new Stats {Caption = "每秒执行的命令数", Key = key, Value = value});
                if (key.Equals("role", StringComparison.OrdinalIgnoreCase))
                    rs.Add(new Stats {Caption = "当前实例角色", Key = key, Value = value});
                if (key.Equals("connected_slaves", StringComparison.OrdinalIgnoreCase))
                    rs.Add(new Stats {Caption = "slave的数量", Key = key, Value = value});
                if (key.Equals("used_cpu_sys", StringComparison.OrdinalIgnoreCase))
                    rs.Add(new Stats {Caption = "Redis使用系统CPU", Key = key, Value = value});
                if (key.Equals("used_cpu_user", StringComparison.OrdinalIgnoreCase))
                    rs.Add(new Stats {Caption = "Redis使用用户CPU", Key = key, Value = value});
                if (key.Equals("used_cpu_sys_children", StringComparison.OrdinalIgnoreCase))
                    rs.Add(new Stats {Caption = "Redis后台使用系统CPU", Key = key, Value = value});
                if (key.Equals("used_cpu_user_children", StringComparison.OrdinalIgnoreCase))
                    rs.Add(new Stats {Caption = "Redis后台使用用户CPU", Key = key, Value = value});
            }
        }
        catch (Exception e)
        {
            Log.Error($"获取Redis运行状态失败");
            Log.Error(e.Message);
        }

        return rs;
    }
}