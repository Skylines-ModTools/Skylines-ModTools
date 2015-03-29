using System;
using System.IO;
using System.Reflection;
using ColossalFramework.IO;
using ColossalFramework.Steamworks;
using ColossalFramework.UI;
using ModTools;
using UnityEngine;

namespace ModTools
{

    public class ImprovedWorkshopIntegration : MonoBehaviour
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

        private static MethodInfo prepareStagingArea;
        private static MethodInfo reloadPreviewImage;
        private static MethodInfo startWatchingPath;

        private static RedirectCallsState revertState;

        public static void Bootstrap()
        {
            if (bootstrapped)
            {
                return;
            }

            workshopModUploadPanel =
                GameObject.Find("(Library) WorkshopModUploadPanel").GetComponent<WorkshopModUploadPanel>();
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

            prepareStagingArea = typeof(WorkshopModUploadPanel).GetMethod("PrepareStagingArea",
                BindingFlags.Instance | BindingFlags.NonPublic);

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

            bootstrapped = true;
        }

        public static void Revert()
        {
            if (!bootstrapped)
            {
                return;
            }

            RedirectionHelper.RevertRedirect(typeof(WorkshopModUploadPanel).GetMethod("SetAssetInternal",
                    BindingFlags.Instance | BindingFlags.NonPublic), revertState);

            bootstrapped = false;
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
                File.WriteAllBytes(previewPath, (UnityEngine.Object.Instantiate<Texture>((Texture)defaultTexture) as Texture2D).EncodeToPNG());
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
