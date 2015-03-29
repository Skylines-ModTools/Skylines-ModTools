using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ColossalFramework;
using ColossalFramework.IO;
using ColossalFramework.Plugins;
using ColossalFramework.Steamworks;
using ColossalFramework.UI;
using ICities;
using UnityEngine;

namespace ModTools
{

    public class ImprovedWorkshopIntegration 
    {

        private static bool bootstrapped = false;

        private static WorkshopModUploadPanel workshopModUploadPanel;

        private static FieldInfo m_StagingPath;
        private static FieldInfo m_PreviewPath;
        private static FieldInfo m_ContentPath;
        private static FieldInfo m_CurrentHandle;
        private static FieldInfo m_ShareButton;
        private static FieldInfo m_TargetFolder;
        private static FieldInfo m_Title;
        private static FieldInfo m_Desc;
        private static FieldInfo m_ChangeNote;
        private static FieldInfo m_DefaultModPreviewTexture;

        private static MethodInfo reloadPreviewImage;
        private static MethodInfo startWatchingPath;

        private static RedirectCallsState revertState;
        private static RedirectCallsState revertState2;
        private static RedirectCallsState revertState3;

        private static bool improvedModsPanelExists = false;
        private static bool modsPanelBootstrapped = false;

        public static void Bootstrap()
        {
            if (bootstrapped)
            {
                return;
            }

            improvedModsPanelExists = CheckForImprovedModsPanel();

            var go = GameObject.Find("(Library) WorkshopModUploadPanel");
            if (go == null)
            {
                return;
            }

            workshopModUploadPanel = go.GetComponent<WorkshopModUploadPanel>();

            if (workshopModUploadPanel == null)
            {
                return;
            }

            var modsList = GameObject.Find("ModsList");
            if (modsList == null)
            {
                return;
            }

            m_StagingPath = Util.FindField(workshopModUploadPanel, "m_StagingPath");
            m_PreviewPath = Util.FindField(workshopModUploadPanel, "m_PreviewPath");
            m_ContentPath = Util.FindField(workshopModUploadPanel, "m_ContentPath");
            m_CurrentHandle = Util.FindField(workshopModUploadPanel, "m_CurrentHandle");
            m_ShareButton = Util.FindField(workshopModUploadPanel, "m_ShareButton");
            m_TargetFolder = Util.FindField(workshopModUploadPanel, "m_TargetFolder");
            m_ShareButton = Util.FindField(workshopModUploadPanel, "m_ShareButton");
            m_Title = Util.FindField(workshopModUploadPanel, "m_Title");
            m_Desc = Util.FindField(workshopModUploadPanel, "m_Desc");
            m_ChangeNote = Util.FindField(workshopModUploadPanel, "m_ChangeNote");
            m_DefaultModPreviewTexture = Util.FindField(workshopModUploadPanel, "m_DefaultModPreviewTexture");

            reloadPreviewImage = typeof(WorkshopModUploadPanel).GetMethod("ReloadPreviewImage",
                BindingFlags.Instance | BindingFlags.NonPublic);

            startWatchingPath = typeof(WorkshopModUploadPanel).GetMethod("StartWatchingPath",
                BindingFlags.Instance | BindingFlags.NonPublic);

            revertState = RedirectionHelper.RedirectCalls
            (
                typeof(WorkshopModUploadPanel).GetMethod("SetAssetInternal",
                    BindingFlags.Instance | BindingFlags.NonPublic),
                typeof(ImprovedWorkshopIntegration).GetMethod("SetAssetInternal",
                    BindingFlags.Instance | BindingFlags.NonPublic)
            );

            if (!improvedModsPanelExists)
            {
                modsPanelBootstrapped = true;
                revertState2 = RedirectionHelper.RedirectCalls
                (
                    typeof(PackageEntry).GetMethod("FormatPackageName",
                        BindingFlags.Static | BindingFlags.NonPublic),
                    typeof(ImprovedWorkshopIntegration).GetMethod("FormatPackageName",
                        BindingFlags.Static | BindingFlags.NonPublic)
                );

                revertState3 = RedirectionHelper.RedirectCalls
                (
                    typeof(CustomContentPanel).GetMethod("RefreshPlugins",
                        BindingFlags.Instance | BindingFlags.NonPublic),
                    typeof(ImprovedWorkshopIntegration).GetMethod("RefreshPlugins",
                        BindingFlags.Static | BindingFlags.Public)
                );
            }
            else
            {
                modsPanelBootstrapped = false;
            }
            
            bootstrapped = true;
        }

        public static bool CheckForImprovedModsPanel()
        {
            var plugins = PluginManager.instance.GetPluginsInfo();

            foreach (var current in plugins)
            {
                IUserMod[] instances = current.GetInstances<IUserMod>();
                if (instances[0].Name == "ImprovedModsPanel")
                {
                    return true;
                }
            }

            return false;
        }

        public static void Revert()
        {
            if (!bootstrapped)
            {
                return;
            }

            RedirectionHelper.RevertRedirect(typeof(WorkshopModUploadPanel).GetMethod("SetAssetInternal",
                    BindingFlags.Instance | BindingFlags.NonPublic), revertState);

            if (modsPanelBootstrapped)
            {
                RedirectionHelper.RevertRedirect(typeof(PackageEntry).GetMethod("FormatPackageName",
                  BindingFlags.Static | BindingFlags.NonPublic), revertState2);

                RedirectionHelper.RevertRedirect(typeof(CustomContentPanel).GetMethod("RefreshPlugins",
                    BindingFlags.Instance | BindingFlags.NonPublic), revertState3);
            }
          
            bootstrapped = false;
        }

        private static string FormatPackageName(string entryName, string authorName, bool isWorkshopItem)
        {
            if (!isWorkshopItem)
            {
                return String.Format("{0} (by {1})", entryName, authorName);
            }
            else
            {
                return entryName;
            }
        }

        private static Color32 blackColor = new Color32(0, 0, 0, 255);
        private static Color32 whiteColor = new Color32(200, 200, 200, 255);

        public static void RefreshPlugins()
        {
            if (improvedModsPanelExists)
            {
                return;
            }

            var modsList = GameObject.Find("ModsList");
            if (modsList == null)
            {
                return;
            }

            var plugins = PluginManager.instance.GetPluginsInfo();
            
            Dictionary<PluginManager.PluginInfo, string> pluginNames = new Dictionary<PluginManager.PluginInfo, string>();
            Dictionary<PluginManager.PluginInfo, string> pluginDescriptions = new Dictionary<PluginManager.PluginInfo, string>();

            foreach (var current in plugins)
            {
                IUserMod[] instances = current.GetInstances<IUserMod>();
                pluginNames.Add(current, instances[0].Name);
                pluginDescriptions.Add(current, instances[0].Description);
            }

            UIComponent uIComponent = modsList.GetComponent<UIComponent>();
            UITemplateManager.ClearInstances("ModEntryTemplate");

            var pluginsSorted = PluginManager.instance.GetPluginsInfo().ToArray();
            Array.Sort(pluginsSorted, (a, b) => pluginNames[a].CompareTo(pluginNames[b]));

            int count = 0;
            foreach (var current in pluginsSorted)
            {
                PackageEntry packageEntry = UITemplateManager.Get<PackageEntry>("ModEntryTemplate");
                uIComponent.AttachUIComponent(packageEntry.gameObject);
                packageEntry.entryName = pluginNames[current];
                packageEntry.entryActive = current.isEnabled;
                packageEntry.pluginInfo = current;
                packageEntry.publishedFileId = current.publishedFileID;
                packageEntry.RequestDetails();

                var panel = packageEntry.gameObject.GetComponent<UIPanel>();
                panel.size = new Vector2(panel.size.x, 24.0f);
                panel.color = count % 2 == 0 ? panel.color : new Color32
                    ((byte)(panel.color.r * 0.60f), (byte)(panel.color.g * 0.60f), (byte)(panel.color.b * 0.60f), panel.color.a);

                var name = (UILabel)panel.Find("Name");
                name.textScale = 0.85f;
                name.tooltip = pluginDescriptions[current];
                name.textColor = count % 2 == 0 ? blackColor : whiteColor;
                name.textScaleMode = UITextScaleMode.ControlSize;
                name.position = new Vector3(30.0f, 2.0f, name.position.z);

                var view = (UIButton) panel.Find("View");
                view.size = new Vector2(84.0f, 20.0f);
                view.textScale = 0.7f;
                view.text = "WORKSHOP";
                view.position = new Vector3(1011.0f, -2.0f, view.position.z);

                var share = (UIButton)panel.Find("Share");
                share.size = new Vector2(84.0f, 20.0f);
                share.textScale = 0.7f;
                share.position = new Vector3(1103.0f, -2.0f, share.position.z);

                var delete = (UIButton) panel.Find("Delete");
                delete.size = new Vector2(24.0f, 24.0f);
                delete.position = new Vector3(1195.0f, delete.position.y, delete.position.z);

                var active = (UICheckBox) panel.Find("Active");
                active.position = new Vector3(4.0f, active.position.y, active.position.z);

                var onOff = (UILabel) active.Find("OnOff");
                onOff.enabled = false;
                count++;
            }
        }

        private void SetAssetInternal(string folder)
        {
            m_StagingPath.SetValue(workshopModUploadPanel, null);
            m_PreviewPath.SetValue(workshopModUploadPanel, null);
            m_ContentPath.SetValue(workshopModUploadPanel, null);
            m_CurrentHandle.SetValue(workshopModUploadPanel, UGCHandle.invalid);
            Util.GetFieldValue<UIButton>(m_ShareButton, workshopModUploadPanel).isEnabled = false;
            m_TargetFolder.SetValue(workshopModUploadPanel, folder);
            Util.GetFieldValue<UIButton>(m_ShareButton, workshopModUploadPanel).isEnabled = true;

            bool isUpdate = Util.GetFieldValue<UIButton>(m_ShareButton, workshopModUploadPanel).localeID == "WORKSHOP_UPDATE";

            if (isUpdate)
            {
                Util.GetFieldValue<UITextField>(m_Title, workshopModUploadPanel).text = string.Empty;
                Util.GetFieldValue<UITextField>(m_Desc, workshopModUploadPanel).text = string.Empty;
                Util.GetFieldValue<UITextField>(m_Title, workshopModUploadPanel).readOnly = true;
                Util.GetFieldValue<UITextField>(m_Desc, workshopModUploadPanel).readOnly = true;
            }
            else
            {
                Util.GetFieldValue<UITextField>(m_Title, workshopModUploadPanel).text = "<YOUR MOD NAME>";
                Util.GetFieldValue<UITextField>(m_Desc, workshopModUploadPanel).text = "<YOUR MOD DESCRIPTION>";
                Util.GetFieldValue<UITextField>(m_Title, workshopModUploadPanel).readOnly = false;
                Util.GetFieldValue<UITextField>(m_Desc, workshopModUploadPanel).readOnly = false;
            }

            Util.GetFieldValue<UITextField>(m_ChangeNote, workshopModUploadPanel).text = string.Empty;

            PrepareStagingArea(folder);
        }

        private static readonly string[] previewImageFilenames = new string[] { "PreviewImage.png", "preview.png", "Preview.png" };

        private void PrepareStagingArea(string folder)
        {
            string path = Guid.NewGuid().ToString();

            var stagingPath = Path.Combine(Path.Combine(DataLocation.localApplicationData, "WorkshopStagingArea"), path);

            m_StagingPath.SetValue(workshopModUploadPanel, stagingPath);

            Directory.CreateDirectory(stagingPath);

            var previewPath = Path.Combine(stagingPath, "PreviewImage.png");

            foreach (var previewImageFilename in previewImageFilenames)
            {
                if (File.Exists(Path.Combine(folder, previewImageFilename)))
                {
                    File.Copy(Path.Combine(folder, previewImageFilename), previewPath);
                    break;
                }
            }

            if (!File.Exists(previewPath))
            {
                var defaultTexture = m_DefaultModPreviewTexture.GetValue(workshopModUploadPanel);
                File.WriteAllBytes(previewPath, (UnityEngine.Object.Instantiate((Texture)defaultTexture) as Texture2D).EncodeToPNG());
            }

            m_PreviewPath.SetValue(workshopModUploadPanel, previewPath);
            reloadPreviewImage.Invoke(workshopModUploadPanel, null);

            var contentPath = Path.Combine(stagingPath, "Content" + Path.DirectorySeparatorChar);
            m_ContentPath.SetValue(workshopModUploadPanel, contentPath);

            Directory.CreateDirectory(contentPath);
            WorkshopHelper.DirectoryCopy(folder, contentPath, true);

            startWatchingPath.Invoke(workshopModUploadPanel, null);
        }

    }

}
