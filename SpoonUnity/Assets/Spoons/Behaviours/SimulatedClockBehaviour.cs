using System;
using Beamable;
using DefaultNamespace.Spoons.Services;
using TMPro;
using UnityEngine;

namespace Spoons.Behaviours
{
    public class SimulatedClockBehaviour : MonoBehaviour
    {
        [Header("Scene references")]
        public TextMeshProUGUI[] displays;

        public float timeSpeed = 1;
        
        [Header("runtime")]
        public bool clockEnabled;
        private GameStateService _stateService;
        private float startedAt;
        private long startedAtSim;

        private async void Start()
        {
            var ctx = await BeamContext.Default.Instance;
            _stateService = ctx.GameStateService();
            
            startedAtSim = _stateService.data.simulatedUnixSeconds;
            startedAt = Time.realtimeSinceStartup;
            _stateService.OnStateChanged += OnStateChanged;
        }

        private void OnStateChanged(GameState old, GameState next)
        {
            clockEnabled = next == GameState.MENU;
            
        }

        private void Update()
        {
            if (_stateService == null) return;

            var gameTime = Time.realtimeSinceStartup;
            var diff = gameTime - startedAt;
            
            _stateService.data.simulatedUnixSeconds = startedAtSim + (int)(diff * timeSpeed);
            
            var time = DateTimeOffset.FromUnixTimeSeconds(_stateService.data.simulatedUnixSeconds);
            var formattedText = time.ToString("t");
            foreach (var text in displays)
            {
                text.text = formattedText;
            }
        }
    }
}