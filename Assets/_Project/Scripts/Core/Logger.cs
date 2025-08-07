// File: Assets/_Project/Scripts/Core/Logger.cs
// Fix: Restored all the convenience methods (Debug, Info, Error, etc.) that were accidentally removed.
// Fix: Added [RuntimeInitializeOnLoadMethod] to reset log level on play.

using UnityEngine;
using System;
using System.Text;
using Photon.Pun;

namespace Platformer.Core
{
    public static class Logger
    {
        public enum LogLevel { Verbose, Debug, Info, Warning, Error, Critical, Off }
        public enum LogCategory { General, Movement, Combat, Networking, UI, Performance, StateManagement, Scoring, AI }
        
        private static LogLevel _currentLogLevel;
        private static bool _includeStackTrace;
        private static bool _includeTimestamp;
        private static bool _includeNetworkInfo;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void InitializeOnLoad()
        {
            _currentLogLevel = LogLevel.Info;
            _includeStackTrace = true;
            _includeTimestamp = true;
            _includeNetworkInfo = true;
        }
        
        public static void SetLogLevel(LogLevel level)
        {
            _currentLogLevel = level;
        }
        
        public static void Log(LogLevel level, LogCategory category, string message, UnityEngine.Object context = null)
        {
            if (level < _currentLogLevel) return;
            
            string formattedMessage = FormatMessage(level, category, message);
            
            switch (level)
            {
                case LogLevel.Verbose:
                case LogLevel.Debug:
                case LogLevel.Info:
                    UnityEngine.Debug.Log(formattedMessage, context);
                    break;
                case LogLevel.Warning:
                    UnityEngine.Debug.LogWarning(formattedMessage, context);
                    break;
                case LogLevel.Error:
                case LogLevel.Critical:
                    UnityEngine.Debug.LogError(formattedMessage, context);
                    break;
            }
        }
        
        private static string FormatMessage(LogLevel level, LogCategory category, string message)
        {
            var sb = new StringBuilder();
            if (_includeTimestamp) sb.Append($"[{DateTime.Now:HH:mm:ss.fff}] ");
            sb.Append($"[{level}] [{category}] ");
            if (_includeNetworkInfo && PhotonNetwork.InRoom) sb.Append($"[{(PhotonNetwork.IsMasterClient ? "M" : "C")}] ");
            sb.Append(message);
            return sb.ToString();
        }

        // --- CONVENIENCE METHODS RESTORED ---

        public static void Verbose(LogCategory category, string message, UnityEngine.Object context = null) => Log(LogLevel.Verbose, category, message, context);
        public static void Debug(LogCategory category, string message, UnityEngine.Object context = null) => Log(LogLevel.Debug, category, message, context);
        public static void Info(LogCategory category, string message, UnityEngine.Object context = null) => Log(LogLevel.Info, category, message, context);
        public static void Warning(LogCategory category, string message, UnityEngine.Object context = null) => Log(LogLevel.Warning, category, message, context);
        public static void Error(LogCategory category, string message, UnityEngine.Object context = null) => Log(LogLevel.Error, category, message, context);
        public static void Critical(LogCategory category, string message, UnityEngine.Object context = null) => Log(LogLevel.Critical, category, message, context);
        
        // Category-specific helpers
        public static void Combat(LogLevel level, string message, UnityEngine.Object context = null) => Log(level, LogCategory.Combat, message, context);
        public static void Performance(LogLevel level, string message, UnityEngine.Object context = null) => Log(level, LogCategory.Performance, message, context);

        public static void Exception(LogCategory category, Exception exception, string additionalMessage = "", UnityEngine.Object context = null)
        {
            string message = $"{additionalMessage} - Exception: {exception.Message}";
            if (_includeStackTrace)
            {
                message += $"\n{exception.StackTrace}";
            }
            Log(LogLevel.Critical, category, message, context);
        }
    }
}