using System;
using System.Collections.Generic;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using ICities;
using UnityEngine;
using UnityEngineInternal;

namespace ModTools
{

    public class ModToolsBootstrap
    {

        private static GameObject modToolsGameObject;
        private static ModTools modTools;

        public static bool IsModToolsActive()
        {
            var pluginManager = PluginManager.instance;
            var plugins = Util.GetPrivate<Dictionary<string, PluginManager.PluginInfo>>(pluginManager, "m_Plugins");

            foreach (var item in plugins)
            {
                if (item.Value.name != "409520576")
                {
                    continue;
                }

                return item.Value.isEnabled;
            }

            return false;
        }

        public static void Bootstrap()
        {

            try
            {
                InitModTools(SimulationManager.UpdateMode.Undefined);

                var target = typeof(LoadingWrapper).GetMethod("OnLevelLoaded",
                new[] { typeof(SimulationManager.UpdateMode) });

                var replacement = typeof(ModToolsBootstrap).GetMethod("OnLevelLoaded",
                    new[] { typeof(SimulationManager.UpdateMode) });

                RedirectionHelper.RedirectCalls(target, replacement);
            }
            catch (Exception ex)
            {
                DebugOutputPanel.AddMessage(PluginManager.MessageType.Error, ex.Message);
            }
        }

        private static void InitModTools(SimulationManager.UpdateMode mode)
        {
            if (!IsModToolsActive())
            {
                return;
            }

            if (modToolsGameObject != null)
            {
                return;
            }

            modToolsGameObject = new GameObject("ModTools");
            modTools = modToolsGameObject.AddComponent<ModTools>();
            modTools.Initialize(mode);
        }

        public void OnLevelLoaded(SimulationManager.UpdateMode mode)
        {
            InitModTools(mode);

            var loadingManager = LoadingManager.instance;
            var wrapper = loadingManager.m_LoadingWrapper;

            var loadingExtensions = Util.GetPrivate<List<ILoadingExtension>>(wrapper, "m_LoadingExtensions");

            for (int i = 0; i < loadingExtensions.Count; i++)
            {
                loadingExtensions[i].OnLevelLoaded((LoadMode)mode);
            }
        }

    }

    public class Mod : IUserMod
    {

        public string Name
        {
            get { ModToolsBootstrap.Bootstrap(); return "ModTools"; }
        }

        public string Description
        {
            get { return "Debugging toolkit for modders"; }
        }

    }
  
}
