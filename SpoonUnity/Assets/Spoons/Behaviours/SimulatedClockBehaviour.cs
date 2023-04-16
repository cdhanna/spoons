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

            startedAtSim = (long)_stateService.data.startTime;
            _stateService.data.secondsInDay = startedAtSim;

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
            
            _stateService.data.secondsInDay = startedAtSim + (int)(diff * timeSpeed);

            var ts = TimeSpan.FromSeconds(_stateService.data.secondsInDay);
            // TODO: handle 12 hour clock...
            var formattedText = ts.ToString(@"hh\:mm");
            foreach (var text in displays)
            {
                text.text = $"<mspace=1em>{formattedText}";
            }
        }
    }
}