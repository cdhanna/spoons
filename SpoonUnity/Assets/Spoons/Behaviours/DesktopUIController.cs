using Beamable.Common;
using DefaultNamespace.Spoons.Services;
using UnityEngine;
using UnityEngine.UI;

namespace Spoons.Behaviours
{
    public class DesktopUIController : StandardBehaviour
    {
        public Button SpoonsButton;

        protected override Promise OnStart()
        {
            SpoonsButton.onClick.AddListener(HandleSpoonClick);
            return Promise.Success;
        }

        void HandleSpoonClick()
        {
            ctx.GameStateService().GoToMenu();
        }
    }
}