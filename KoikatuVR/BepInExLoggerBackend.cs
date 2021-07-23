using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRGIN.Core;
using BepInEx.Logging;
using System.Diagnostics;

namespace KoikatuVR
{
    /// <summary>
    /// A logger backend that forwards messages to BepInEx.
    /// </summary>
    class BepInExLoggerBackend : ILoggerBackend
    {
        private ManualLogSource _logger;

        public BepInExLoggerBackend(ManualLogSource logger)
        {
            _logger = logger;
        }

        public void Log(string message, object[] args, VRLog.LogMode severity)
        {
            string str = Format(String.Format(message, args));
            switch (severity)
            {
                case VRLog.LogMode.Debug:
                    _logger.LogDebug(str);
                    break;
                case VRLog.LogMode.Info:
                    _logger.LogInfo(str);
                    break;
                case VRLog.LogMode.Warning:
                    _logger.LogWarning(str);
                    break;
                case VRLog.LogMode.Error:
                    _logger.LogError(str);
                    break;
                default:
                    _logger.LogError(str);
                    break;
            }
        }

        private static String Format(string text)
        {
            var trace = new StackTrace(4);
            var caller = trace.GetFrame(0);
            return String.Format("{1}.{2}: {0}", text, caller.GetMethod().DeclaringType.Name, caller.GetMethod().Name);
        }
    }
}
