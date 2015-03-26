using UnityEngine;

namespace ModTools
{
    public class GUIArea
    {
        public Vector2 absolutePosition = Vector2.zero;
        public Vector2 absoluteSize = Vector2.zero;
        public Vector2 margin = new Vector2(8.0f, 8.0f);
        public Vector2 relativePosition = Vector2.zero;
        public Vector2 relativeSize = Vector2.zero;
        public GUIWindow window;

        public GUIArea(GUIWindow _window)
        {
            window = _window;
        }

        public Vector2 Position
        {
            get
            {
                return absolutePosition +
                       new Vector2(relativePosition.x*window.rect.width, relativePosition.y*window.rect.height) +
                       margin;
            }
        }

        public Vector2 Size
        {
            get
            {
                return absoluteSize + new Vector2(relativeSize.x*window.rect.width, relativeSize.y*window.rect.height) -
                       margin * 2.0f;
            }
        }

        public void Begin()
        {
            var position = Position;
            var size = Size;
            GUILayout.BeginArea(new Rect(position.x, position.y, size.x, size.y));
        }

        public void End()
        {
            GUILayout.EndArea();
        }
    }
}