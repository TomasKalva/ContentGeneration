using OurFramework.Util;
using System;
using UnityEngine;

namespace OurFramework.Gameplay.RealWorld
{
    public enum AgentState
    {
        PREPARE,
        DAMAGE,
        RESTORE,
        NORMAL
    }

    public class Attack : AnimatedAct
    {
        [SerializeField, Curve(0f, 0f, 1f, 15f, true)]
        AnimationCurve speedF;

        Vector3 direction;
        public Vector3 Direction
        {
            get => direction;
            set => direction = value.normalized;
        }

        [SerializeField]
        WeaponSlot[] weaponSlots;

        /// <summary>
        /// Normalized to [0,1].
        /// </summary>
        [SerializeField]
        float damageStartT;

        /// <summary>
        /// Normalized to [0,1].
        /// </summary>
        [SerializeField]
        float damageEndT;

        Action StopLockOn { get; set; }
        MovementConstraint lockOnTarget;


        public override void OnStart(Agent agent)
        {

            PlayAnimation(agent);

            bool shouldLockOn = true;
            Vector3 lastDirection = agent.movement.AgentForward;
            Direction3F directionF = () =>
            {
                if (shouldLockOn)
                {
                    lastDirection = TargetPosition == null ? Direction : TargetPosition.DirectionFrom(agent.transform.position);
                }
                return lastDirection;
                //return TargetPosition == null ? Direction : TargetPosition.DirectionFrom(agent.transform.position);
            };

            agent.movement.VelocityUpdater = new CurveVelocityUpdater(speedF, duration, directionF);

            lockOnTarget = new TurnToDirection(() => directionF().XZ().normalized);

            StopLockOn = () =>
            {
                shouldLockOn = false;
                lockOnTarget.Finished = true;
            };

            SetupMovementConstraints(agent,
                new VelocityInDirection(directionF),
                lockOnTarget);

            agent.State = AgentState.PREPARE;
        }

        float DamageDuration() => (damageEndT - damageStartT) * Duration;

        void SetSlotsActive(bool active, Agent agent)
        {
            foreach (var weaponSlot in weaponSlots)
            {
                var weapon = weaponSlot.Equipment;
                if (weapon != null)
                {
                    // When the damage starts
                    if (!weaponSlot.Equipment.Active && active)
                    {
                        weapon.DealDamage(agent, DamageDuration());
                    }
                    weaponSlot.Equipment.Active = active;
                }
            }
        }

        public override void OnUpdate(Agent agent)
        {
            var normalizedElapsed = timeElapsed / duration;
            if(agent.State == AgentState.PREPARE && normalizedElapsed >= damageStartT)
            {
                StopLockOn();
                agent.State = AgentState.DAMAGE;
                SetSlotsActive(true, agent);
            }

            if (agent.State == AgentState.DAMAGE && normalizedElapsed >= damageEndT)
            {
                agent.State = AgentState.RESTORE;
                SetSlotsActive(false, agent);
            }
        }

        public override void EndAct(Agent agent)
        {
            SetSlotsActive(false, agent);
            agent.State = AgentState.NORMAL;
            MovementContraints.ForEach(con => con.Finished = true);
        }
    }

    public class TargetPosition
    {
        Transform target;

        Vector3 defaultPosition;

        public Vector3 Position => target != null ? target.position : defaultPosition;
        public Vector3 DirectionFrom(Vector3 from) => (Position - from).normalized;

        public TargetPosition(Transform target, Vector3 defaultPosition)
        {
            this.target = target;
            this.defaultPosition = defaultPosition;
        }
    }
}