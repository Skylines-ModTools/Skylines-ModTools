using UnityEngine;

namespace ModTools
{
    public static class ColorUtil
    {

        public struct HSV
        {
            public float h;
            public float s;
            public float v;

            public HSV(Color color)
            {
                double min, max, delta, temp;
                min = Mathf.Min(color.r, Mathf.Min(color.g, color.b));
                max = Mathf.Max(color.r, Mathf.Max(color.g, color.b));
                delta = max - min;
                v = (int)max;

                if (delta == 0.0f)
                {
                    h = s = 0;
                }
                else
                {
                    temp = delta / max;
                    s = (int)(temp * 255);

                    if (color.r == (int)max)
                    {
                        temp = (double)(color.g - color.b) / delta;
                    }
                    else if (color.g == (int)max)
                    {
                        temp = 2.0 + ((double)(color.b - color.r) / delta);
                    }
                    else
                    {
                        temp = 4.0 + ((double)(color.r - color.g) / delta);
                    }

                    temp *= 60;

                    if (temp < 0)
                    {
                        temp += 360;
                    }

                    if (temp == 360)
                    {
                        temp = 0;
                    }

                    h = (int)temp;
                }

                s /= 255.0f;
                v /= 255.0f;
            }

            public Color ToColor()
            {
                if (h < 0.0f) { h += 360.0f; }

                if (h > 360.0f) { h -= 360.0f; }

                s *= 255.0f;
                v *= 255.0f;
                float r, g, b;

                if (h == 0.0f && s == 0.0f)
                {
                    r = g = b = v;
                }

                double min, max, delta, hue;
                max = v;
                delta = (max * s) / 255.0;
                min = max - delta;
                hue = h;

                if (h > 300 || h <= 60)
                {
                    r = (int)max;

                    if (h > 300)
                    {
                        g = (int)min;
                        hue = (hue - 360.0) / 60.0;
                        b = (int)((hue * delta - min) * -1);
                    }
                    else
                    {
                        b = (int)min;
                        hue = hue / 60.0;
                        g = (int)(hue * delta + min);
                    }
                }
                else if (h > 60 && h < 180)
                {
                    g = (int)max;

                    if (h < 120)
                    {
                        b = (int)min;
                        hue = (hue / 60.0 - 2.0) * delta;
                        r = (int)(min - hue);
                    }
                    else
                    {
                        r = (int)min;
                        hue = (hue / 60 - 2.0) * delta;
                        b = (int)(min + hue);
                    }
                }
                else
                {
                    b = (int)max;

                    if (h < 240)
                    {
                        r = (int)min;
                        hue = (hue / 60.0 - 4.0) * delta;
                        g = (int)(min - hue);
                    }
                    else
                    {
                        g = (int)min;
                        hue = (hue / 60 - 4.0) * delta;
                        r = (int)(min + hue);
                    }
                }

                return new Color(r/255.0f, g/255.0f, b/255.0f, 1.0f);
            }
        }

    }
}
