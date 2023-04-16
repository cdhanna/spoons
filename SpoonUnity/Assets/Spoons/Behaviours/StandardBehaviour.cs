using System;
using Beamable;
using Beamable.Common;
using UnityEngine;

namespace Spoons.Behaviours
{
    public class StandardBehaviour : MonoBehaviour
    {
        protected BeamContext ctx;

        async void Start()
        {
            ctx = await BeamContext.Default.Instance;
            await OnStart();
        }

        protected virtual Promise OnStart()
        {
            return Promise.Success;
        }
    }
}