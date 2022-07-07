using System;
using CliWrap.Buffered;

namespace Assistant.Utils.Extensions;

internal static class CliWrapExtension
{
    public static string ToErrorMessage(this BufferedCommandResult rs)
    {
        var newLine = Environment.NewLine;
        return $"{newLine}" +
               $"ExitCode: {rs.ExitCode}{newLine}" +
               $"StartTime: {rs.StartTime.DateTime:yyyy-MM-dd HH:mm:ss.fff}{newLine}" +
               $"ExitTime: {rs.ExitTime.DateTime:yyyy-MM-dd HH:mm:ss.fff}{newLine}" +
               $"RunTime: {rs.RunTime.TotalMilliseconds} ms{newLine}" +
               $"StandardOutput:{newLine}" +
               $"{rs.StandardOutput}{newLine}" +
               $"StandardError:{newLine}" +
               $"{rs.StandardError}{newLine}";
    }
}