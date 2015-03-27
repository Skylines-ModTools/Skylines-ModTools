using ICities;
using UnityEngine;

namespace ModTools
{

    public class Mod : IUserMod
    {

        public string Name
        {
            get { return "ModTools"; }
        }

        public string Description
        {
            get { return "Debugging toolkit for modders"; }
        }

    }
    public class ModLoad : LoadingExtensionBase
    {

        private GameObject modToolsGameObject;
        private ModTools modTools;

        public override void OnLevelLoaded(LoadMode mode)
        {
            modToolsGameObject = new GameObject("ModTools");
            modTools = modToolsGameObject.AddComponent<ModTools>();

            if (mode == LoadMode.LoadAsset || mode == LoadMode.NewAsset)
            {
                ModTools.assetEditor = true;
                ModTools.mapEditor = false;
            }
            else if (mode == LoadMode.LoadMap || mode == LoadMode.NewMap)
            {
                ModTools.mapEditor = true;
                ModTools.assetEditor = false;
            }
            else
            {
                ModTools.mapEditor = false;
                ModTools.assetEditor = false;
            }
        }

        public override void OnLevelUnloading()
        {
            GameObject.Destroy(modTools);
            GameObject.Destroy(modToolsGameObject);
        }
    }

}
