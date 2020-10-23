using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace AvigilonRestTriggers.Utils
{
    public static class Logger
    {
        public static List<Action<String>> LogActions = new List<Action<string>>();
        public static bool LogToConsole { get; set; } = true;
        public static bool LogToFile { get; set; } = true;

        public static String LogFilePath { get; set; } = "logs/{YYYY}{MM}/log_{YYYY}{MM}{DD}.log";

        public static void Log(this string msg,string logType="info",string payload = null)
        {
            DateTime TS = DateTime.UtcNow;
            string logMsg = TS.ToString("yyyy-MM-ddTHH:mm:ss.fff" , null)+"|"+logType+"|"+msg;

            if (LogToConsole) Console.WriteLine(logMsg);
            if (LogToFile) FileLog(logMsg+"\r\n", logType);

        }

        public static void LogError(this string msg,string payload=null)
        {
            Log(msg, "error", payload);
        }

        public static void LogWarning(this string msg,string payload = null)
        {
            Log(msg, "warning", payload);
        }

        private static void FileLog(string msg,string logType)
        {
            try
            {
                var outputpath = GetLogFilePath("", logType);
                FileInfo fi = new FileInfo(outputpath);
                if (!fi.Directory.Exists)
                {
                    fi.Directory.Create();
                }

                File.AppendAllText(outputpath, msg);
            }
            catch (System.Exception err)
            {
                Console.WriteLine(err.ToString());
            }
        }

        private static string GetLogFilePath(string _logFilePath = "",string _logType="info", DateTime? _ts = null)
        {
            if (String.IsNullOrEmpty(_logFilePath))
            {
                _logFilePath = LogFilePath;
            }
            var curDate = _ts.HasValue ? _ts.Value : DateTime.UtcNow;
            string ret = _logFilePath;
            ret = ret.Replace("{YYYY}", curDate.ToString("yyyy"));
            ret = ret.Replace("{MM}", curDate.ToString("MM"));
            ret = ret.Replace("{DD}", curDate.ToString("dd"));
            ret = ret.Replace("{HH}", curDate.ToString("HH"));
            ret = ret.Replace("{mm}", curDate.ToString("mm"));
            ret = ret.Replace("{ss}", curDate.ToString("ss"));
            ret = ret.Replace("{{type}}", _logType);

            return ret;
        }
    }
}