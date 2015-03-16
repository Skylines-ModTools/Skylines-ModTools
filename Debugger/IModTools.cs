using ModTools;
using UnityEngine;

namespace YourModNamespaceHere
{

    public static class ModToolsInterface
    {

        public static void AddButton(string name, string text, ModTools.ButtonClicked onClick)
        {
            if (Interface != null)
            {
                Interface.AddButton(name, text, onClick);
            }
        }

        public static void RemoveButton(string name)
        {
            if (Interface != null)
            {
                Interface.RemoveButton(name);
            }
        }

        private static ModTools.IModTools Interface
        {
            get
            {
                if (iface == null)
                {
                    var cameraController = GameObject.FindObjectOfType<CameraController>();
                    iface = (ModTools.IModTools)cameraController.gameObject.GetComponent("ModTools");
                }

                return iface;
            }
        }

        private static IModTools iface = null;

    }

}

namespace ModTools
{
    public delegate void ButtonClicked();

    public interface IModTools
    {
        void AddButton(string name, string text, ButtonClicked onClick);
        void RemoveButton(string name);
    }
}