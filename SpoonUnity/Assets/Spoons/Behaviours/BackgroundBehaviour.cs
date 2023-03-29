using System;
using UnityEngine;

namespace Spoons.Behaviours
{
    [ExecuteAlways]
    public class BackgroundBehaviour : MonoBehaviour
    {
        public Material backgroundMaterial;
        public Transform tracker;

        private void Update()
        {
            if (backgroundMaterial == null) return;
            if (tracker == null) return;
            
            backgroundMaterial.SetFloat("_X", tracker.position.x);
            // _backgroundBlock.SetVector("_Sizes", new Vector4(backgroundSprite.bounds.size.x, backgroundSprite.bounds.size.y, 0, 0));
            
        }
    }
}