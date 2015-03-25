using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace ModTools
{

    public class Configuration
    {

        public Rect mainWindowRect = new Rect(128, 128, 356, 260);
        public bool mainWindowVisible = false;

        public Rect sceneExplorerRect = new Rect(128, 440, 800, 500);
        public bool sceneExplorerVisible = false;

        public Rect watchesRect = new Rect(504, 128, 800, 300);
        public bool watchesVisible = false;

        public Rect textureViewerRect = new Rect(512, 128, 512, 512);
        public bool textureViewerVisible = false;

        public Rect meshViewerRect = new Rect(512, 128, 512, 512);
        public bool meshViewerVisible = false;

        public bool logExceptionsToConsole = true;
        public bool evaluatePropertiesAutomatically = true;
        public bool extendGamePanels = true;

        public void OnPreSerialize()
        {

        }

        public void OnPostDeserialize()
        {

        }

        public static void Serialize(string filename, Configuration config)
        {
            var serializer = new XmlSerializer(typeof(Configuration));

            using (var writer = new StreamWriter(filename))
            {
                config.OnPreSerialize();
                serializer.Serialize(writer, config);
            }
        }

        public static Configuration Deserialize(string filename)
        {
            var serializer = new XmlSerializer(typeof(Configuration));

            try
            {
                using (var reader = new StreamReader(filename))
                {
                    var config = (Configuration)serializer.Deserialize(reader);
                    config.OnPostDeserialize();
                    return config;
                }
            }
            catch { }

            return null;
        }
    }

}
