using System;
using Devdog.General2;
using UnityEngine;

namespace Devdog.Rucksack
{
    public class UnityLogger : ILogger
    {
        private readonly string _prefix;

        private DevdogLogger.LogType logType
        {
            get { return GeneralSettingsManager.instance.settings.minimalLogType; }
        }

        public UnityLogger(string prefix = "")
        {
            _prefix = prefix;
        }
        
        public void LogVerbose(string msg, object instance = null)
        {
            if (logType <= DevdogLogger.LogType.LogVerbose)
            {
                Debug.Log(_prefix + msg, instance as UnityEngine.Object);
            }
        }

        public void Log(string msg, object instance = null)
        {
            if (logType <= DevdogLogger.LogType.Log)
            {
                Debug.Log(_prefix + msg, instance as UnityEngine.Object);
            }
        }

        public void Warning(string msg, object instance = null)
        {
            if (logType <= DevdogLogger.LogType.Warning)
            {
                Debug.LogWarning(_prefix + msg, instance as UnityEngine.Object);
            }
        }

        public void Error(string msg, object instance = null)
        {
            if (logType <= DevdogLogger.LogType.Error)
            {
                Debug.LogError(_prefix + msg, instance as UnityEngine.Object);
            }
        }

        public void Error(string msg, Error error, object instance = null)
        {
            if (error != null && logType <= DevdogLogger.LogType.Error)
            {
                Debug.LogError(_prefix + msg + "\n" + error, instance as UnityEngine.Object);
            }
        }
        
        public void Error(Exception exception, object instance = null)
        {
            if (exception != null && logType <= DevdogLogger.LogType.Error)
            {
                Debug.LogError(_prefix + exception.Message, instance as UnityEngine.Object);
            }
        }
    }
}