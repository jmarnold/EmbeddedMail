// EDITED BY BLOCHER CONSULTING

using System;
using Serilog;

namespace EmbeddedMail
{
    public class SmtpLog
    {
        public static ILogger Logger { get; set; }

        public static void Error(string message) =>
            Logger.Warning(message);

        public static void Error(string message, Exception e) =>
            Logger.Warning(e, message);

        public static void Debug(string message) =>
            Logger.Verbose(message);

        public static void Debug(string message, Exception e) =>
            Logger.Verbose(e, message);

        public static void Info(string message) =>
            Logger.Debug(message);

  }
}
