using Assistant.Utils;
using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;

namespace Assistant.Model.ServiceManager;

// ReSharper disable once InconsistentNaming
public partial class NginxService : ServiceBase
{
    public override async Task Install(Action<string>? infoAction = null)
    {
        Guard.IsNotNullOrEmpty(BinPath);
        Guard.IsNotNullOrEmpty(ServiceName);
        Guard.IsNotNullOrEmpty(ConfigFilePath);
        Guard.IsNotNullOrEmpty(ServiceDirectory);
        Guard.IsNotNullOrEmpty(TempDirectory);
        Guard.IsNotNullOrEmpty(LogDirectory);

        if (!Directory.Exists(TempDirectory))
        {
            Directory.CreateDirectory(TempDirectory);
            infoAction?.Invoke($"创建目录 [{TempDirectory}]");
        }

        if (!Directory.Exists(LogDirectory))
        {
            Directory.CreateDirectory(LogDirectory);
            infoAction?.Invoke($"创建目录 [{LogDirectory}]");
        }

        // 备份配置文件
        if (File.Exists(ConfigFilePath))
        {
            var rs = await FileUtils.BackupFile(ConfigFilePath);
            infoAction?.Invoke($"备份配置文件 [{rs}]");
        }

        // 更新配置文件
        var contents = NginxConfig.ToString();
        await FileUtils.WriteToFile(contents, ConfigFilePath);
        infoAction?.Invoke($"更新配置文件 [{ConfigFilePath}]");

        // 写入ins.conf
        var insConfPath = Path.Combine(ServiceDirectory, Global.InstallConfFileName);
        await FileUtils.WriteToFile(this, insConfPath);
        infoAction?.Invoke($"写入ins.conf: [{insConfPath}]");

        // 创建Windows服务
        await WinServiceUtils.CreateService(BinPath, ServiceName, ServiceDescription ?? string.Empty, ServiceDescription ?? string.Empty);
        infoAction?.Invoke($"windows 服务创建成功: [{ServiceName}]");
    }

    public override async Task UnInstall(Action<string>? infoAction = null)
    {
        Guard.IsNotNullOrEmpty(ServiceName);

        if (Directory.Exists(TempDirectory))
        {
            Directory.Delete(TempDirectory, true);
            infoAction?.Invoke($"清理临时目录 [{TempDirectory}]");
        }

        if (File.Exists(InsConfFilePath))
        {
            File.Delete(InsConfFilePath);
            infoAction?.Invoke($"删除ins.conf [{InsConfFilePath}]");
        }

        infoAction?.Invoke($"删除Windows服务 [{ServiceName}]");
        await WinServiceUtils.DeleteService(ServiceName);
    }
}

public partial class NginxService
{
    public NginxService()
    {
        var baseDir = Path.Combine(Global.CurrentDir, "Services", "Nginx");

        DisplayName        = "Nginx";
        Version            = "1.21.5";
        ServiceName        = "Neuz.Nginx";
        BinPath            = Path.Combine(baseDir, "nginx_service.exe");
        ServiceDescription = "Neuz.Nginx 服务";
        ServiceDirectory   = baseDir;
        ConfigFilePath     = Path.Combine(baseDir, "conf", "nginx.conf");
        LogDirectory       = Path.Combine(baseDir, "logs");
        TempDirectory      = Path.Combine(baseDir, "temp");
        Installed          = false;
        RunningStatus      = RunningStatus.UnKnown;
        NginxConfig        = new NginxConfigModel();
    }

    public string? TempDirectory { get; set; }

    public NginxConfigModel NginxConfig { get; set; }

    public class NginxConfigModel
    {
        public int WebPort { get; set; } = 10003;
        public int PdaPort { get; set; } = 10004;

        public override string ToString()
        {
            return string.Format(_configTemplate, WebPort, PdaPort);
        }

        private string _configTemplate = @"# user nobody nogroup;
worker_processes 1;          

events {{
  worker_connections 512;      
}}

http {{
    include       mime.types;
    default_type  application/octet-stream;
    server {{
        listen *:{0};                
        server_name """";             
        # include       /etc/nginx/mime.types;	
        root ../../App/web; 

	    location / {{
          try_files $uri /index.html;
        }}

	    location /api/ {{
    	    rewrite ^/api/(.*)$ /$1 break;
            proxy_pass http://127.0.0.1:5000;
        }}

	    location /hubs/ {{
            proxy_pass http://127.0.0.1:5000;
            proxy_http_version 1.1;
            proxy_set_header Upgrade $http_upgrade;
            proxy_set_header Connection ""upgrade"";
            proxy_read_timeout 3600s; 
        }}
    }}

    server {{
        listen *:{1};                
        server_name """";             
        # include       /etc/nginx/mime.types;	
        root ../../App/pda; 

        location / {{
            try_files $uri /index.html;
        }}

        location /api/ {{
            rewrite ^/api/(.*)$ /$1 break;
            proxy_pass http://127.0.0.1:5000;
        }}

        location /hubs/ {{
            proxy_pass http://127.0.0.1:5000;
            proxy_http_version 1.1;
            proxy_set_header Upgrade $http_upgrade;
            proxy_set_header Connection ""upgrade"";
            proxy_read_timeout 3600s; 
        }}
    }}
}}";
    }
}