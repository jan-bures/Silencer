using System.Text.RegularExpressions;
using BepInEx.Logging;
using HarmonyLib;
using Silencer.Data;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Silencer;

[HarmonyPatch]
internal static class Patches
{
    private static StorageManager Storage => SilencerPlugin.Instance.Storage;

    [HarmonyPatch(
        typeof(DebugLogHandler),
        nameof(DebugLogHandler.LogFormat),
        typeof(LogType),
        typeof(Object),
        typeof(string),
        typeof(object[])
    )]
    [HarmonyPrefix]
    public static bool LogFormat(LogType logType, Object context, string format, params object[] args)
    {
        return IsMessageAllowed(string.Format(format, args));
    }

    [HarmonyPatch(
        typeof(DebugLogHandler),
        nameof(DebugLogHandler.LogFormat),
        typeof(LogType),
        typeof(LogOption),
        typeof(Object),
        typeof(string),
        typeof(object[])
    )]
    [HarmonyPrefix]
    public static bool LogFormat(
        LogType logType,
        LogOption logOptions,
        Object context,
        string format,
        params object[] args
    )
    {
        return IsMessageAllowed(string.Format(format, args));
    }

    [HarmonyPatch(typeof(DebugLogHandler), nameof(DebugLogHandler.LogException))]
    [HarmonyPrefix]
    public static bool LogException(Exception exception, Object context)
    {
        return exception == null || IsMessageAllowed(exception.ToString());
    }

    [HarmonyPatch(typeof(ManualLogSource), nameof(ManualLogSource.Log))]
    [HarmonyPrefix]
    private static bool ManualLogPrefix(LogLevel level, object data)
    {
        return IsMessageAllowed(data.ToString());
    }

    private static bool IsMessageAllowed(string message)
    {
        foreach (var item in Storage.Data)
        {
            switch (item.Type)
            {
                case SavedItemType.PlainString when message.ToLower().Contains(item.Value.ToLower()):
                case SavedItemType.Regex when Regex.IsMatch(message, item.Value, RegexOptions.IgnoreCase):
                    return false;
            }
        }

        return true;
    }
}