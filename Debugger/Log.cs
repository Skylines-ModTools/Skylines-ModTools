using UnityEngine;

namespace ModTools
{
    public static class Log
    {
        public static void Message(string s)
        {
            if (ModTools.Instance.console != null)
            {
                ModTools.Instance.console.AddMessage(s, LogType.Log, false, true);
            }
            else
            {
                DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, s);
            }
        }

        public static void Error(string s)
        {
            if (ModTools.Instance.console != null)
            {
                ModTools.Instance.console.AddMessage(s, LogType.Error, false, true);
            }
            else
            {
                DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Error, s);
            }
        }

        public static void Warning(string s)
        {
            if (ModTools.Instance.console != null)
            {
                ModTools.Instance.console.AddMessage(s, LogType.Warning, false, true);
            }
            else
            {
                DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Warning, s);
            }
        }
    }
}
