using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Spoons.Behaviours
{
    [CreateAssetMenu]
    public class BackgroundSkyProfileObject : ScriptableObject
    {
        public const int MAX_COLORS_PER_ROW = 32;
        
        public List<BackgroundSkyColorDataPoint> topColors;
        public List<BackgroundSkyColorDataPoint> lowColors;

        [NonSerialized]
        private Texture2D _topTex;
        [NonSerialized]
        private Texture2D _lowTex;
        
        public Texture2D CreateTopTexture()
        {
            if (_topTex != null) return _topTex;
            var tex = new Texture2D(MAX_COLORS_PER_ROW, 1, TextureFormat.RGB24, false);
            tex.filterMode = FilterMode.Bilinear;
            _topTex = tex;

            ApplyToTexture(_topTex, ref topColors);
            return tex;
        }
        
        public Texture2D CreateLowTexture()
        {
            if (_lowTex != null) return _lowTex;
            var tex = new Texture2D(MAX_COLORS_PER_ROW, 1, TextureFormat.RGB24, false);
            tex.filterMode = FilterMode.Bilinear;
            _lowTex = tex;

            ApplyToTexture(_lowTex, ref lowColors);
            return tex;
        }


        
        private void OnValidate()
        {
            ApplyToTexture(CreateTopTexture(), ref topColors);
            ApplyToTexture(CreateLowTexture(), ref lowColors);
        }

        public void ApplyToTexture(Texture2D tex, ref List<BackgroundSkyColorDataPoint> points)
        {
            var colors = new Color[MAX_COLORS_PER_ROW];
            for (var i = 0; i < colors.Length; i++)
            {
                colors[i] = Color.clear;
            }

            var copy = points.ToList();
            copy.Sort((a, b) => a.time.CompareTo(b.time));

            var curr = 0;
            
            for (var i = 0; i < MAX_COLORS_PER_ROW; i++)
            {
                var n = ((float)i) / MAX_COLORS_PER_ROW;

                var safety = 9999;
                while (safety-- > 0 && curr < copy.Count && n > copy[curr].time)
                {
                    curr++;
                }

                if (curr == copy.Count)
                {
                    colors[i] = copy[copy.Count - 1].color;
                    continue;
                }

                var end = copy[curr];
                if (curr > 0)
                {
                    var start = copy[curr - 1];

                    var x = (n - start.time) / (end.time - start.time);

                    colors[i] = Color.Lerp(start.color, end.color, x);
                }
                else
                {
                    colors[i] = copy[0].color;
                }
            }
            tex.SetPixels(colors);
            tex.Apply();
        }
    }

    [Serializable]
    public class BackgroundSkyColorDataPoint
    {
        [Range(0, 1)]
        public float time;
        public Color color;
    }
}