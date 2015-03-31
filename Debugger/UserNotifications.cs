using System.Collections.Generic;

namespace ModTools
{

    public static class UserNotifications
    {
        private static Configuration config
        {
            get { return ModTools.Instance.config; }
        }

        private static List<KeyValuePair<int, string>> notifications = new List<KeyValuePair<int, string>>();
        private static int notificationsCount = 0;

        static UserNotifications()
        {
            Add(LoggingChangeNotification);    
            Add(UnityLoggingHookNotification);
        }

        private static void Add(string notification)
        {
            notifications.Add(new KeyValuePair<int, string>(notificationsCount++, notification));
        }

        public static List<KeyValuePair<int, string>> GetNotifications()
        {
            List<KeyValuePair<int, string>> result = new List<KeyValuePair<int, string>>();

            foreach (var item in notifications)
            {
                if ((config.hiddenNotifications & (1 << item.Key)) == 0)
                {
                    result.Add(item);   
                }
            }

            return result;
        }

        public static void HideNotification(int index)
        {
            config.hiddenNotifications |= 1 << index;
            ModTools.Instance.SaveConfig();
        }

        private static string LoggingChangeNotification = @"You are using the new ModTools console.
It offers an improved experience over the old one but requires a change to your logging code.
You should no longer use DebugOutputPanel for logging as messages sent to it won't get displayed by ModTools.
Instead you should use the built-in Unity Debug API (http://docs.unity3d.com/ScriptReference/Debug.html). Example: Debug.Log(""Hello world!"");";

        private static string UnityLoggingHookNotification = @"Your version of ModTools has a new feature which allows it to hook Unity's Debug logging so you can safely log from the simulation thread (or any other thread).
This feature is currently marked as experimental and is off by default. You can find it in the main menu (Ctrl+Q) as ""Hook Unity's logging (experimental)"". After enabling it you should see a warning in the console saying so.
It is recommended that you enable this (as it will probably become the default mode in the future) and report any issues.";

    }

}
