using OurFramework.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Util;
using OurFramework.Gameplay.RealWorld;
using OurFramework.Gameplay.Data;

namespace OurFramework.Gameplay.Libraries
{
    public class SelectorLibrary
    {
        Libraries lib;

        public SelectorLibrary(Libraries lib)
        {
            this.lib = lib;
        }

        public Selector ConstSelector(CharacterState target, float duration, IDistribution<float> timeBetweenHits)
        {
            var countdown = new CountdownTimer(duration);
            return new Selector(
                timeBetweenHits,
                () =>
                {
                    return target.ToEnumerable();
                },
                dt => countdown.Finished(dt)
            );
        }

        public ByUser<Selector> SelfSelector() =>
            ch => new Selector(
                new ConstDistr(1f),
                () =>
                {
                    return ch.ToEnumerable();
                },
                dt => true
            );

        public SelectorInitializator Initializator() => new SelectorInitializator();

        public delegate void SelectorInitializationOperation(CharacterState caster, Rigidbody selector);
        public class SelectorInitializator
        {
            /// <summary>
            /// These operations that will initialize the selector.
            /// </summary>
            List<SelectorInitializationOperation> initializationOperations;

            public SelectorInitializator()
            {
                this.initializationOperations = new List<SelectorInitializationOperation>();
            }

            public SelectorInitializator ConstPosition(Vector3 pos)
            {
                initializationOperations.Add((ch, s) => s.transform.position = pos);
                return this;
            }

            public SelectorInitializator FrontOfCharacter(float frontDist)
            {
                initializationOperations.Add((ch, s) => s.transform.position = ch.Agent.GetGroundPosition() + ch.Agent.movement.AgentForward * frontDist);
                return this;
            }

            public SelectorInitializator RightHandOfCharacter(float frontDist)
            {
                initializationOperations.Add((ch, s) => s.transform.position = ch.Agent.GetRightHandPosition() + ch.Agent.movement.AgentForward * frontDist + 0.2f * Vector3.down);
                return this;
            }

            public SelectorInitializator LeftHandOfCharacter(float frontDist)
            {
                initializationOperations.Add((ch, s) => s.transform.position = ch.Agent.GetLeftHandPosition() + ch.Agent.movement.AgentForward * frontDist + 0.2f * Vector3.down);
                return this;
            }

            public SelectorInitializator HandOfCharacter(float frontDist, bool right = true)
            {
                if (right)
                {
                    RightHandOfCharacter(frontDist);
                }
                else
                {
                    LeftHandOfCharacter(frontDist);
                }
                return this;
            }

            public SelectorInitializator SetVelocity(Func<CharacterState, Vector3> directionF, float speed)
            {
                initializationOperations.Add((ch, s) => s.velocity = speed * directionF(ch));
                return this;
            }

            public SelectorInitializator RotatePitch(float angles)
            {
                initializationOperations.Add((ch, s) => s.transform.Rotate(new Vector3(angles, 0f, 0f)));
                return this;
            }

            public SelectorInitializator SetDirection(Vector3 direction)
            {
                initializationOperations.Add((ch, s) => s.transform.forward = direction);
                return this;
            }

            public SelectorInitializator Move(Vector3 direction)
            {
                initializationOperations.Add((ch, s) => s.transform.position = s.transform.position + direction);
                return this;
            }

            public SelectorInitializator Scale(float scale)
            {
                initializationOperations.Add((ch, s) => s.transform.localScale = new Vector3(scale, scale, scale));
                return this;
            }

            public void Initialize(CharacterState caster, Rigidbody selector)
            {
                caster.World.PutToCache(selector.transform);
                initializationOperations.ForEach(op => op(caster, selector));
            }
        }

        public SelectorByArgsByUser GeometricSelector(Func<VFX> vfxF, float duration, SelectorInitializator selectorInitialization, bool selfImmune = true)
        {
            return args => ch =>
            {
                VFX vfx = vfxF();
                vfx.SetColor(args.Color);
                vfx.SetTexture(args.FlipbookTexture);

                var rb = vfx.gameObject.AddComponent<Rigidbody>();
                rb.useGravity = false;
                rb.transform.forward = ch.Agent.transform.forward;

                selectorInitialization.Initialize(ch, rb);


                ColliderDetector collider = vfx.ColliderDetector;
                var countdown = new CountdownTimer(duration);
                var wallHitCountdown = new EventsCountdown(10);
                var ts = new GeometricTargetSelector(
                        vfx,
                        collider,
                        dt => countdown.Finished(dt) || wallHitCountdown.Finished(collider.Hits.SelectNN(hit => hit.gameObject).Where(go => go.layer == LayerMask.NameToLayer("StaticEnvironment")).Any())
                    );

                //vfx.transform.position = ch.Agent.transform.position;

                /*
                var movingBall = Libraries.GeometricSelectors.Ball()
                    .PutTo(ch.Agent.rightWeaponSlot)
                    .MoveDir(ch.Agent.movement.AgentForward)
                    .Speed()
                    .DestroyAtWallTouch()*/

                var selector = new Selector(
                    new ConstDistr(1f),
                    ts.SelectTargets,
                    dt => ts.Update(dt)
                );

                if (selfImmune)
                {
                    selector.AddImmuneCharacter(ch);
                }
                return selector;
            };
        }

        public ByTime<SelectorByUser> WeaponSelector(ColliderDetector colliderDetector)
        {
            return duration => user =>
            {
                var countdown = new CountdownTimer(duration);
                var selector = new Selector(
                    new ConstDistr(10f),
                    () => colliderDetector.Hits.SelectNN(c => c?.GetComponentInParent<Agent>()?.CharacterState),
                    dt => countdown.Finished(dt)
                );
                selector.AddImmuneCharacter(user);
                return selector;
            };
        }
    }
}
