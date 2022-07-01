using System.IO;

namespace Assistant.Model.ServiceManager;

// ReSharper disable once InconsistentNaming
public partial class NginxServiceModel : ServiceBaseModel
{
    public NginxServiceModel()
    {
        var baseDir = Path.Combine(Global.CurrentDir, "Services", "Nginx");
        DisplayName = "Nginx";
        Version     = "1.21.5";
        ServiceName = "Neuz.Nginx";
        BinPath     = Path.Combine(baseDir, "nginx_service.exe");
        ServiceDescription = "Neuz.Nginx 服务";
        ServiceDirectory   = baseDir;
        ConfigFilePath     = Path.Combine(baseDir, "conf", "nginx.conf");
        LogDirectory       = Path.Combine(baseDir, "logs");
        TempDirectory      = Path.Combine(baseDir, "temp");
        Installed          = false;
        RunningStatus      = RunningStatus.UnKnown;
        NginxConfig        = new NginxConfigModel();
    }
}

public partial class NginxServiceModel : ServiceBaseModel
{
    public string? TempDirectory { get; set; }

    public NginxConfigModel NginxConfig { get; set; } = new();

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