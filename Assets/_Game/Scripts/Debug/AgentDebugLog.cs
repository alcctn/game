using System;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;

namespace CleanEnergy.DebugTools
{
    /// <summary>Session debug NDJSON writer (agent debug mode).</summary>
    public static class AgentDebugLog
    {
        private const string SessionId = "b79ea4";
        private const string IngestUrl =
            "http://127.0.0.1:7796/ingest/12ad03ac-4b9c-45e8-83f7-ee6de154908c";

        private static readonly string[] LogPaths =
        {
            @"e:\CURSOL PROJELER\game\debug-b79ea4.log",
            Path.Combine(Application.dataPath, "..", "debug-b79ea4.log"),
            Path.Combine(Application.persistentDataPath, "debug-b79ea4.log")
        };

        public static void Write(string hypothesisId, string location, string message, string dataJson = "{}")
        {
            // #region agent log
            try
            {
                var ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var line =
                    "{\"sessionId\":\"" + SessionId +
                    "\",\"hypothesisId\":\"" + hypothesisId +
                    "\",\"location\":\"" + location +
                    "\",\"message\":\"" + Escape(message) +
                    "\",\"data\":" + (string.IsNullOrEmpty(dataJson) ? "{}" : dataJson) +
                    ",\"timestamp\":" + ts +
                    "}";

                Debug.Log("[AgentDebug] " + line);

                for (var i = 0; i < LogPaths.Length; i++)
                {
                    try
                    {
                        var dir = Path.GetDirectoryName(LogPaths[i]);
                        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }

                        File.AppendAllText(LogPaths[i], line + "\n");
                    }
                    catch
                    {
                        // try next path
                    }
                }

                try
                {
                    using (var client = new WebClient())
                    {
                        client.Headers[HttpRequestHeader.ContentType] = "application/json";
                        client.Headers["X-Debug-Session-Id"] = SessionId;
                        client.UploadString(IngestUrl, "POST", line);
                    }
                }
                catch
                {
                    // ingest optional
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[AgentDebug] write failed: " + ex.Message);
            }
            // #endregion
        }

        private static string Escape(string s)
        {
            return (s ?? string.Empty).Replace("\\", "\\\\").Replace("\"", "\\\"");
        }
    }
}
