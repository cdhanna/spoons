using System;
using Beamable;
using DefaultNamespace.Spoons.Services;
using UnityEngine;

namespace Spoons.Behaviours
{
    [ExecuteAlways]
    public class BackgroundBehaviour : MonoBehaviour
    {
        public Material backgroundMaterial;
        public Transform tracker;

        
        public BackgroundSkyProfileObject skyProfile;

        private GameStateService _stateService;
        async void Start()
        {
            var ctx = await BeamContext.Default.Instance;
            _stateService = ctx.GameStateService();

        }
        
        private void Update()
        {
            if (backgroundMaterial == null) return;
            if (tracker == null) return;

            if (skyProfile != null)
            {
                var topSky = skyProfile.CreateTopTexture();
                var lowSky = skyProfile.CreateLowTexture();
                backgroundMaterial.SetTexture("_TopSkyTex", topSky);
                backgroundMaterial.SetTexture("_LowSkyTex", lowSky);
            }
            
            backgroundMaterial.SetFloat("_X", tracker.position.x);
            if (_stateService != null)
            {
                backgroundMaterial.SetFloat("_GameTime", _stateService.data.DayRatio);
            }
            
        }
    }
}