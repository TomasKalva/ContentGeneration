using ContentGeneration.Assets.UI;
using System;
using UnityEngine;

namespace OurFramework.LevelDesignLanguage.CustomModules
{
    class DeathModule : LDLanguage
    {
        public DeathModule(LanguageParams parameters) : base(parameters)
        {
        }

        /// <summary>
        /// Restart the level.
        /// </summary>
        public void DieClasically()
        {
            var playerState = State.World.PlayerState;
            playerState
                .AddOnDeath(() =>
                {
                    GameObject.Destroy(playerState.Agent.gameObject);
                    State.GC.ResetLevel(5.0f);
                });
        }

        /// <summary>
        /// Pay Vibrant Memory to stay alive. The item is payed automatically on death.
        /// This effect can be negated by other death events such as RunEndingBloodstain.
        /// </summary>
        public void DieIfNotProtected()
        {
            var playerState = State.World.PlayerState;
            playerState
                .AddOnDeath(() =>
                {
                    playerState.Inventory.TryPay(
                        "Vibrant Memory", 1,
                        () => 
                        {
                            playerState.Reset();
                            Msg.Show("Life endures");
                        },
                        () => 
                        {
                            GameObject.Destroy(playerState.Agent.gameObject);
                            State.GC.ResetLevel(5.0f);
                        });
                });
        }

        /// <summary>
        /// Bloodstain will get created after level reset. The bloodstain will stop spawning after player dies.
        /// </summary>
        void DropBloodstain(Action onRetrieval, Action onNonRetrieval = null)
        {
            onNonRetrieval = onNonRetrieval ?? (() => { });

            var playerState = State.World.PlayerState;
            var deathPosition = playerState.Agent.transform.position;

            Action spawnBloodstain = null;
            Action bloodstainRemoval = null;

            // Create bloodstain
            var bloodstain = Lib.InteractiveObjects.Bloodstain(
                () =>
                {
                    onRetrieval();
                    State.World.OnLevelRestart -= spawnBloodstain;
                    playerState.OnDeath -= bloodstainRemoval;
                });

            // Spawn bloodstain
            spawnBloodstain = () =>
            {
                var bloodstainBody = bloodstain.MakeGeometry();
                bloodstainBody.transform.position = deathPosition;
                State.World.AddInteractiveObject(bloodstain);
            };
            State.World.OnLevelRestart += spawnBloodstain;

            // Remove old bloodstain after death
            bloodstainRemoval = () =>
            {
                onNonRetrieval();
                State.World.OnLevelRestart -= spawnBloodstain;
                playerState.OnDeath -= bloodstainRemoval;
            };
            playerState.AddOnDeath(bloodstainRemoval);

            // Spawn th bloodstain now in case the player survives
            spawnBloodstain();
        }

        /// <summary>
        /// Player drops spirit into bloodstain after death.
        /// </summary>
        public void DropSpiritBloodstainOnDeath()
        {
            var playerState = State.World.PlayerState;
            playerState.AddOnDeath(() =>
            {
                var lostSpirit = playerState.Spirit;
                playerState.Spirit = 0;
                DropBloodstain(() =>
                {
                    playerState.Spirit += lostSpirit;
                    Msg.Show("Spirit retrieved");
                });
            });
        }

        /// <summary>
        /// The game ends if player doesn't retrieve bloodstain before end of the level.
        /// </summary>
        public void DropRunEndingBloodstainOnDeath()
        {
            var playerState = State.World.PlayerState;
            playerState.AddOnDeath(() =>
            {
                DropBloodstain(() => { Msg.Show("Ending alleviated"); }, State.GC.EndRun);
            });
        }

        /// <summary>
        /// The run will end after max(1, deathCount) player's deaths.
        /// </summary>
        public void EndRunAfterDeaths(int deathCount)
        {
            var playerState = State.World.PlayerState;
            playerState
                .AddOnDeath(() =>
                {
                    if(playerState.DeathCount == deathCount)
                    {
                        State.GC.EndRun();
                    }
                });
        }

        /// <summary>
        /// Ends run when no smile remains in the inventory.
        /// </summary>
        public void EndRunIfOutOfSmile()
        {
            var playerState = State.World.PlayerState;
            playerState
                .AddOnDeath(() =>
                {
                    playerState.Inventory.TryPay(
                        "Smile", 1,
                        () => { },
                        () =>
                        {
                            State.GC.EndRun();
                        });
                });
        }
    }
}
