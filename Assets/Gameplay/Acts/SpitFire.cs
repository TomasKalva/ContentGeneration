using OurFramework.Gameplay.Data;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OurFramework.Gameplay.RealWorld
{
    /// <summary>
    /// Spit multiple fireballs with a spell effect.
    /// </summary>
    public class SpitFire : MultiEventAct
    {
        public ByTransform<Effect> SpitFromPositionDirectionEffect { set; private get; }

        [SerializeField]
        Transform spirtStart;

        [SerializeField]
        float startT;

        [SerializeField]
        float endT;

        [SerializeField]
        int shotsCount;

        public override void OnStart(Agent agent)
        {
            base.OnStart(agent);

            timedActions = new List<TimeAction>();
            timedActions.AddRange(Enumerable.Range(0, shotsCount).Select(i => new TimeAction(startT + i / (float)shotsCount * (endT - startT), () => DoShot(agent))));
        }

        void DoShot(Agent agent)
        {
            if (SpitFromPositionDirectionEffect != null)
            {
                SpitFromPositionDirectionEffect(spirtStart.position, spirtStart.up)(agent.CharacterState);
            }
        }
    }
}
