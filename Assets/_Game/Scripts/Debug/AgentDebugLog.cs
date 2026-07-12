using System;
using System.IO;
using UnityEngine;

namespace CleanEnergy.DebugTools
{
    /// <summary>Session debug NDJSON writer (file only, no Console spam).</summary>
    public static class AgentDebugLog
    {
        private const string SessionId = "b79ea4";
        private static readonly string LogPath = @"e:\CURSOL PROJELER\game\debug-b79ea4.log";
        private static int _writes;

        public static void Write(string hypothesisId, string location, string message, string dataJson = "{}")
        {
            // #region agent log
            try
            {
                if (_writes > 80)
                {
                    return;
                }

                _writes++;
                var ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var line =
                    "{\"sessionId\":\"" + SessionId +
                    "\",\"hypothesisId\":\"" + hypothesisId +
                    "\",\"location\":\"" + location +
                    "\",\"message\":\"" + (message ?? string.Empty).Replace("\\", "\\\\").Replace("\"", "\\\"") +
                    "\",\"data\":" + (string.IsNullOrEmpty(dataJson) ? "{}" : dataJson) +
                    ",\"timestamp\":" + ts +
                    "}\n";
                File.AppendAllText(LogPath, line);
            }
            catch
            {
                // ignore
            }
            // #endregion
        }
    }
}
