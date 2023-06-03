using System;
using System.Diagnostics;
using System.IO;
using BepInEx.Logging;
using HarmonyLib;
using VRGIN.Core;

namespace KKS_VR.Fixes
{
    public static class VRLogRedirectPatch
    {
        private static ManualLogSource _logger;

        public static void Patch()
        {
            _logger = BepInEx.Logging.Logger.CreateLogSource("VRLog");

            new Harmony("VRLogRedirectPatch").PatchAll(typeof(VRLogRedirectPatch));

            // Get rid of the VR log file since it's unnecessary
            var writer = new Traverse(typeof(VRLog)).Field<StreamWriter>("S_Handle").Value;
            var fs = (FileStream)writer.BaseStream;
            var streamFilename = fs.Name;
            fs.Close();
            File.Delete(streamFilename);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(VRLog), nameof(VRLog.Log))]
        private static bool LogRedirect(string text, object[] args, VRLog.LogMode severity)
        {
            if (severity >= VRLog.Level)
            {
                try
                {
                    var bepinLevel = ConvertToBepinLogLevel(severity);
                    _logger.Log(bepinLevel, string.Format(Format(text), args));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed at logging: " + ex);
                }
            }
            return false;
        }

        private static string Format(string text)
        {
            var frame = new StackTrace(4).GetFrame(0);
            return string.Format("[{0:0000}] [{2}.{3}] {1}", UnityEngine.Time.realtimeSinceStartup, text, frame.GetMethod().DeclaringType?.Name ?? "???", frame.GetMethod().Name);
        }

        private static LogLevel ConvertToBepinLogLevel(VRLog.LogMode severity)
        {
            return severity switch
            {
                VRLog.LogMode.Debug => LogLevel.Debug,
                VRLog.LogMode.Info => LogLevel.Info,
                VRLog.LogMode.Warning => LogLevel.Warning,
                VRLog.LogMode.Error => LogLevel.Error,
                _ => LogLevel.Message
            };
        }
    }
}
