using System;
using System.IO;
using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;

namespace ModTools
{

    class GamePanelExtender : MonoBehaviour
    {

        private bool initializedBuildingsPanel = false;
        private ZonedBuildingWorldInfoPanel zonedBuildingInfoPanel;
        private UILabel zonedBuildingAssetNameLabel;
        private UIButton zonedBuildingShowExplorerButton;
        private UIButton zonedBuildingDumpMeshTextureButton;

        private UIView uiView;

        private SceneExplorer sceneExplorer;

        private ReferenceChain buildingsBufferRefChain;

        void Awake()
        {
            uiView = FindObjectOfType<UIView>();
        }

        void OnDestroy()
        {
            Destroy(zonedBuildingAssetNameLabel.gameObject);
            Destroy(zonedBuildingShowExplorerButton.gameObject);
            Destroy(zonedBuildingDumpMeshTextureButton.gameObject);

            zonedBuildingInfoPanel.component.Find<UILabel>("AllGood").isVisible = true;
            zonedBuildingInfoPanel.component.Find<UIPanel>("ProblemsPanel").isVisible = true;
        }

        UIButton CreateButton(string text, int width, int height, UIComponent parentComponent, Vector3 offset, UIAlignAnchor anchor, MouseEventHandler handler)
        {
            var button = uiView.AddUIComponent(typeof(UIButton)) as UIButton;
            button.name = "ModTools Button";
            button.text = text;
            button.textScale = 0.8f;
            button.width = width;
            button.height = height;
            button.normalBgSprite = "ButtonMenu";
            button.disabledBgSprite = "ButtonMenuDisabled";
            button.hoveredBgSprite = "ButtonMenuHovered";
            button.focusedBgSprite = "ButtonMenu";
            button.pressedBgSprite = "ButtonMenuPressed";
            button.textColor = new Color32(255, 255, 255, 255);
            button.disabledTextColor = new Color32(7, 7, 7, 255);
            button.hoveredTextColor = new Color32(255, 255, 255, 255);
            button.focusedTextColor = new Color32(255, 255, 255, 255);
            button.pressedTextColor = new Color32(30, 30, 44, 255);
            button.eventClick += handler;
            button.AlignTo(parentComponent, anchor);
            button.relativePosition += offset;
            return button;
        }

        UILabel CreateLabel(string text, int width, int height, UIComponent parentComponent, Vector3 offset,
            UIAlignAnchor anchor)
        {
            var label = uiView.AddUIComponent(typeof (UILabel)) as UILabel;
            label.text = text;
            label.name = "ModTools Label";
            label.width = width;
            label.height = height;
            label.textColor = new Color32(255, 255, 255, 255);
            label.AlignTo(parentComponent, anchor);
            label.relativePosition += offset;
            return label;
        }

        void AddZonedBuildingPanelControls()
        {
            zonedBuildingInfoPanel.component.Find<UILabel>("AllGood").isVisible = false;
            zonedBuildingInfoPanel.component.Find<UIPanel>("ProblemsPanel").isVisible = false;

            zonedBuildingAssetNameLabel = CreateLabel
            (
                "AssetName: <>", 160, 24,
                zonedBuildingInfoPanel.component,
                new Vector3(8.0f, 48.0f, 0.0f),
                UIAlignAnchor.TopLeft
            );

            zonedBuildingShowExplorerButton = CreateButton
            (
                "Find in SceneExplorer", 160, 24,
                zonedBuildingInfoPanel.component,
                new Vector3(-8.0f, 100.0f, 0.0f), 
                UIAlignAnchor.TopRight, 
                (component, param) =>
                {
                    InstanceID instance = Util.ReadPrivate<ZonedBuildingWorldInfoPanel, InstanceID>(zonedBuildingInfoPanel, "m_InstanceID");
                    sceneExplorer.ExpandFromRefChain(buildingsBufferRefChain.Add(instance.Building));
                    sceneExplorer.visible = true;
                }
            );

            zonedBuildingDumpMeshTextureButton = CreateButton
            (
                "Dump mesh+texture", 160, 24,
                zonedBuildingInfoPanel.component,
                new Vector3(-8.0f, 132.0f, 0.0f), 
                UIAlignAnchor.TopRight, 
                (component, param) =>
                {
                    InstanceID instance = Util.ReadPrivate<ZonedBuildingWorldInfoPanel, InstanceID>(zonedBuildingInfoPanel, "m_InstanceID");
                    var building = BuildingManager.instance.m_buildings.m_buffer[instance.Building];
                    var material = building.Info.m_material;
                    var mesh = building.Info.m_mesh;

                    var assetName = building.Info.name;

                    Log.Warning(String.Format("Dumping asset \"{0}\"", assetName));
                    Util.DumpMeshOBJ(mesh, String.Format("{0}.obj", assetName));
                    Util.DumpTextureToPNG(material.GetTexture("_MainTex"), String.Format("{0}_MainTex.png", assetName));
                    Util.DumpTextureToPNG(material.GetTexture("_XYSMap"), String.Format("{0}_xyz.png", assetName));
                    Util.DumpTextureToPNG(material.GetTexture("_ACIMap"), String.Format("{0}_aci.png", assetName));
                    Log.Warning("Done!");
                }
            );
        }

        void Update()
        {
            if (!initializedBuildingsPanel)
            {
                sceneExplorer = FindObjectOfType<SceneExplorer>();

                buildingsBufferRefChain = new ReferenceChain();
                buildingsBufferRefChain = buildingsBufferRefChain.Add(BuildingManager.instance.gameObject);
                buildingsBufferRefChain = buildingsBufferRefChain.Add(BuildingManager.instance);
                buildingsBufferRefChain = buildingsBufferRefChain.Add(typeof(BuildingManager).GetField("m_buildings"));
                buildingsBufferRefChain = buildingsBufferRefChain.Add(typeof(Array16<Building>).GetField("m_buffer"));

                zonedBuildingInfoPanel = GameObject.Find("(Library) ZonedBuildingWorldInfoPanel").GetComponent<ZonedBuildingWorldInfoPanel>();
                if (zonedBuildingInfoPanel != null)
                {
                    AddZonedBuildingPanelControls();
                    initializedBuildingsPanel = true;
                }
            }

            if (zonedBuildingInfoPanel.component.isVisible)
            {
                InstanceID instance = Util.ReadPrivate<ZonedBuildingWorldInfoPanel, InstanceID>(zonedBuildingInfoPanel, "m_InstanceID");
                var building = BuildingManager.instance.m_buildings.m_buffer[instance.Building];
                zonedBuildingAssetNameLabel.text = "AssetName: " + building.Info.name;
            }
        }

    }

}
