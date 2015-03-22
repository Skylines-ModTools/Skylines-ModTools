using ColossalFramework;
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

        private ModTools modTools;

        public override void OnLevelLoaded(LoadMode mode)
        {
            var controller = GameObject.FindObjectOfType<CameraController>();
            modTools = controller.gameObject.AddComponent<ModTools>();
        }

        public override void OnLevelUnloading()
        {
            GameObject.Destroy(modTools);
        }
    }

}
