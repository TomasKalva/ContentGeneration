using OurFramework.Gameplay.State;
using OurFramework.Gameplay.RealWorld;
using OurFramework.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OurFramework.Libraries
{
    /// <summary>
    /// Defines spell effects.
    /// </summary>
    public class Spells
    {
        EffectLibrary eff;
        SelectorLibrary sel;
        VFXs vfxs;

        public Spells(EffectLibrary eff, SelectorLibrary sel, VFXs vfxs)
        {
            this.eff = eff;
            this.sel = sel;
            this.vfxs = vfxs;
        }

        /// <summary>
        /// Periodically use effect for the given duration.
        /// passed to the effect.
        /// </summary>
        public Effect Periodically(Effect effect, float duration, float tickLength)
        {
            return ch => ch.World.CreateOccurence(
                    sel.ConstSelector(ch, duration, new ConstDistrFloat(tickLength)),
                    effect
                    );
        }

        struct Point
        {
            public Vector3 Position;
            public Vector3 Normal;

            public Point(Vector3 position, Vector3 normal)
            {
                Position = position;
                Normal = normal;
            }
        }

        /// <summary>
        /// The center of the arc is on startDirection.
        /// </summary>
        IEnumerable<Point> EvenlySampleCircleBorder(Vector3 center, float radius, int samplesCount, float halfArcSizeDeg, Vector2 startDirection)
        {
            var startAngle = Mathf.Atan2(startDirection.y, startDirection.x);
            var arcSizeRad = halfArcSizeDeg * Mathf.Deg2Rad;
            return Enumerable.Range(0, samplesCount)
                .Select(i => (Mathf.PI * 2f * (i / (float)samplesCount) + 2 * Mathf.PI) % (2 * Mathf.PI))
                .Where(angle => angle < arcSizeRad || angle >= 2 * Mathf.PI - arcSizeRad)
                .Select(angle =>
                {
                    angle = angle + startAngle;
                    var cos = Mathf.Cos(angle);
                    var sin = Mathf.Sin(angle);
                    return new Point(center + radius * new Vector3(cos, 0f, sin), -new Vector3(cos, 0f, sin));
                });
        }

        IEnumerable<Vector3> EvenlySampleCircleArea(Vector3 center, float radius, int samplesCount)
        {
            return Enumerable.Range(0, samplesCount)
                .Select(_ => new Vector2(radius * Mathf.Sqrt(MyRandom.Range(0f, 1f)), 2 * Mathf.PI * MyRandom.Range(0f, 1f)))
                .Select(rTheta =>
                {
                    float r = rTheta.x;
                    float theta = rTheta.y;

                    var x = r * Mathf.Cos(theta);
                    var z = r * Mathf.Sin(theta);
                    return center + new Vector3(x, 0f, z);
                });
        }

        /// <summary>
        /// Shoots a bolt forward from character's hand. It deals damage upon impact.
        /// </summary>
        public Effect Bolt(Func<VFX> vfxF, Color color, FlipbookTexture texture, float scale, float speed, DamageDealt damageDealt, bool rightHand = true)
        {
            return user => user.World.CreateOccurence(
                sel.GeometricSelector(vfxF, 4f, sel.Initializator()
                    .HandOfCharacter(0f, rightHand)
                    .SetVelocity(user => user.Agent.movement.AgentForward, speed)
                    .RotatePitch(-90f)
                    .Scale(scale)
                    )(new SelectorArgs(color, texture))(user),
                eff.Damage(damageDealt)
                );
        }

        /// <summary>
        /// Spawns the vfx in the given circle arc.
        /// </summary>
        public Effect CircleBorder(Func<VFX> vfxF, Color color, FlipbookTexture texture, Vector3 center, float radius, int sampleCount, float duration, float halfArcSize, Vector2 startDirection, DamageDealt damageDealt)
        {
            return user => EvenlySampleCircleBorder(center, radius, sampleCount, halfArcSize, startDirection)
                .ForEach(point => user.World.CreateOccurence(
                    sel.GeometricSelector(vfxF, duration, sel.Initializator()
                        .ConstPosition(point.Position)
                        .SetDirection(point.Normal) // face out of the circle center
                        )(new SelectorArgs(color, texture))(user),
                    eff.Damage(damageDealt)
                    )
                );
        }

        public Effect CircleBorder(Func<VFX> vfxF, Color color, FlipbookTexture texture, float radius, int sampleCount, float duration, float halfArcSize, Vector2 startDirection, DamageDealt damageDealt)
        {
            return user => CircleBorder(vfxF, color, texture, user.Agent.transform.position, radius, sampleCount, duration, halfArcSize, startDirection, damageDealt)(user);
        }

        public Effect Cloud(Func<VFX> vfxF, Color color, FlipbookTexture texture, float scale, float speed, float pushForce, DamageDealt damageDealt)
        {
            return user =>
            {
                var pushDirection = user.Agent.transform.forward;
                user.World.CreateOccurence(
                    sel.GeometricSelector(vfxF, 4f, sel.Initializator()
                        .FrontOfCharacter(1.3f)
                        .SetVelocity(user => user.Agent.movement.AgentForward, speed)
                        .Scale(scale)
                        )(new SelectorArgs(color, texture))(user),
                    eff.Damage(damageDealt),
                    eff.Push(pushDirection, pushForce)
                    );
            };
        }

        public Effect Firefall(Func<VFX> vfxF, Color color, FlipbookTexture texture, DamageDealt damageDealt)
        {
            return user =>
            {
                user.World.CreateOccurence(
                    sel.GeometricSelector(vfxF, 6f, sel.Initializator()
                        .FrontOfCharacter(1.3f)
                        .RotatePitch(-90)
                        .Move(1.5f * Vector3.up)
                        )(new SelectorArgs(color, texture))(user),
                    eff.Damage(damageDealt)
                    );
            };
        }

        /// <summary>
        /// Places vfx evenly in a circle.
        /// </summary>
        public Effect CircleArea(Func<VFX> vfxF, Color color, FlipbookTexture texture, Vector3 center, float radius, int samplesCount, DamageDealt damageDealt)
        {
            return user => EvenlySampleCircleArea(center, radius, samplesCount)
                .ForEach(pos =>
                user.World.CreateOccurence(
                    sel.GeometricSelector(vfxF, 6f, sel.Initializator()
                        .ConstPosition(pos)
                        )(new SelectorArgs(color, texture))(user),
                    eff.Damage(damageDealt)
                    )
            );
        }

        public Effect FlameOfHeaven(Func<VFX> vfxF, Color color, FlipbookTexture texture, float forward, float scale, float radius, int samplesCount, DamageDealt damageDealt)
        {
            return user => EvenlySampleCircleArea(Vector3.zero, radius, samplesCount)
                .ForEach(relativePos =>
                    user.World.CreateOccurence(
                        sel.GeometricSelector(vfxF, 6f, sel.Initializator()
                            .FrontOfCharacter(forward)
                            .Move(relativePos)
                            .Scale(scale)
                            )(new SelectorArgs(color, texture))(user),
                        eff.Damage(damageDealt)
                        )
                );
        }

        /// <summary>
        /// Spawns the vfx in the given circle.
        /// </summary>
        public Effect TangentCircleBorder(Func<VFX> vfxF, Color color, FlipbookTexture texture, float radius, int sampleCount, float duration, DamageDealt damageDealt)
        {
            return user => EvenlySampleCircleBorder(user.Agent.transform.position, radius, sampleCount, 180f, user.Agent.transform.forward)
                .ForEach(point => user.World.CreateOccurence(
                    sel.GeometricSelector(vfxF, duration, sel.Initializator()
                        .ConstPosition(point.Position)
                        .SetDirection(Quaternion.Euler(0f, 90f, 0f) * point.Normal/* new Vector3(-point.Normal.x, 0f, point.Normal.z)*/) // face along tangent
                        )(new SelectorArgs(color, texture))(user),
                    eff.Damage(damageDealt)
                    )
                );
        }

        public Effect Refreshment(Func<VFX> vfxF, Color color, FlipbookTexture texture, float healing)
        {
            return user =>
                    user.World.CreateOccurence(
                        sel.GeometricSelector(vfxF, 4f, sel.Initializator()
                            .FrontOfCharacter(0f)
                            .Scale(2f)
                            , selfImmune: false)(new SelectorArgs(color, texture))(user),
                        eff.Heal(healing)
                        );
        }

        public Effect Replenishment(float healing)
        {
            return user => eff.Heal(healing)(user);
        }
    }
}

