using System;
using UnityEngine;

// Taken from the MechJeb2 (https://github.com/MuMech/MechJeb2) source, see MECHJEB-LICENSE for license info

namespace ModTools
{

    public class GUIComboBox
    {

        // Easy to use combobox class
        // ***** For users *****
        // Call the Box method with the latest selected item, list of text entries
        // and an object identifying who is making the request.
        // The result is the newly selected item.
        // There is currently no way of knowing when a choice has been made

        // Position of the popuprect
        private static Rect rect;
        // Identifier of the caller of the popup, null if nobody is waiting for a value
        private static object popupOwner = null;
        private static string[] entries;
        private static bool popupActive;
        // Result to be returned to the owner
        private static int selectedItem;
        // Unity identifier of the window, just needs to be unique
        private static int id = GUIUtility.GetControlID(FocusType.Passive);
        // ComboBox GUI Style
        private static GUIStyle style;

        static GUIComboBox()
        {
            Texture2D background = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            background.wrapMode = TextureWrapMode.Clamp;

            for (int x = 0; x < background.width; x++)
                for (int y = 0; y < background.height; y++)
                {
                    if (x == 0 || x == background.width - 1 || y == 0 || y == background.height - 1)
                        background.SetPixel(x, y, new Color(0, 0, 0, 1));
                    else
                        background.SetPixel(x, y, new Color(0.05f, 0.05f, 0.05f, 0.95f));
                }

            background.Apply();

            style = new GUIStyle(GUI.skin.window);
            style.normal.background = background;
            style.onNormal.background = background;
            style.border.top = style.border.bottom;
            style.padding.top = style.padding.bottom;
        }

        static GUIStyle _yellowOnHover;
        public static GUIStyle yellowOnHover
        {
            get
            {
                if (_yellowOnHover == null)
                {
                    _yellowOnHover = new GUIStyle(GUI.skin.label);
                    _yellowOnHover.hover.textColor = Color.yellow;
                    Texture2D t = new Texture2D(1, 1);
                    t.SetPixel(0, 0, new Color(0, 0, 0, 0));
                    t.Apply();
                    _yellowOnHover.hover.background = t;
                }
                return _yellowOnHover;
            }
        }

        public static void DrawGUI()
        {
            if (popupOwner == null || rect.height == 0 || !popupActive)
                return;

            // Make sure the rectangle is fully on screen
          //  rect.x = Math.Max(0, Math.Min(rect.x, rect.width));
          //  rect.y = Math.Max(0, Math.Min(rect.y, rect.height));

            rect = GUILayout.Window(id, rect, identifier =>
            {
                selectedItem = GUILayout.SelectionGrid(-1, entries, 1, yellowOnHover);
                if (GUI.changed)
                    popupActive = false;
            }, "", style);

            //Cancel the popup if we click outside
            if (Event.current.type == EventType.MouseDown && !rect.Contains(Event.current.mousePosition))
                popupOwner = null;
        }

        public static int Box(int selectedItem, string[] entries, object caller)
        {
            // Trivial cases (0-1 items)
            if (entries.Length == 0)
                return 0;
            if (entries.Length == 1)
            {
                GUILayout.Label(entries[0]);
                return 0;
            }

            // A choice has been made, update the return value
            if (popupOwner == caller && !GUIComboBox.popupActive)
            {
                popupOwner = null;
                selectedItem = GUIComboBox.selectedItem;
                GUI.changed = true;
            }

            bool guiChanged = GUI.changed;

            float width = 0;
            foreach (var entry in entries)
            {
                var len = GUI.skin.button.CalcSize(new GUIContent(entry)).x;
                if (len > width)
                {
                    width = len;
                }
            }

            if (GUILayout.Button("↓ " + entries[selectedItem] + " ↓", GUILayout.Width(width+8)))
            {
                // We will set the changed status when we return from the menu instead
                GUI.changed = guiChanged;
                // Update the global state with the new items
                popupOwner = caller;
                popupActive = true;
                GUIComboBox.entries = entries;
                // Magic value to force position update during repaint event
                rect = new Rect(0, 0, 0, 0);
            }
            // The GetLastRect method only works during repaint event, but the Button will return false during repaint
            if (Event.current.type == EventType.Repaint && popupOwner == caller && rect.height == 0)
            {
                rect = GUILayoutUtility.GetLastRect();
                // But even worse, I can't find a clean way to convert from relative to absolute coordinates
                Vector2 mousePos = Input.mousePosition;
                mousePos.y = Screen.height - mousePos.y;
                Vector2 clippedMousePos = Event.current.mousePosition;
                rect.x = (rect.x + mousePos.x) - clippedMousePos.x;
                rect.y = (rect.y + mousePos.y) - clippedMousePos.y;
                rect.height = rect.height*entries.Length;
            }

            return selectedItem;
        }

    }
}
