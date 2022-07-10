using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable ReplaceWithSingleCallToAny

namespace Assistant.Utils;

public class NetWorkUtils
{
    public static bool PortAvailable(int port)
    {
        var properties = IPGlobalProperties.GetIPGlobalProperties();
        return properties.GetActiveTcpListeners()
                              .Concat(properties.GetActiveUdpListeners())
                              .Where(p => p.Port == port)
                              .Any();
    }
}