using ContentGeneration.Assets.UI;
using ContentGeneration.Assets.UI.Model;
using ShapeGrammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage.Modules
{
    class DeathLanguage : LDLanguage
    {
        public DeathLanguage(LanguageParams tools) : base(tools)
        {
        }

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
        /// Pay item to stay alive. The item is payed automatically on death.
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
        void DropBloodstain(Action onRetrieval)
        {
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
                State.World.OnLevelRestart -= spawnBloodstain;
                playerState.OnDeath -= bloodstainRemoval;
            };
            playerState.AddOnDeath(bloodstainRemoval);
        }

        public void DropSpiritBloodstainOnDeath()
        {
            var playerState = State.World.PlayerState;
            playerState.AddOnDeath(() =>
            {
                var lostSpirit = playerState.Spirit;
                playerState.Spirit = 0;
                DropBloodstain(() => playerState.Spirit += lostSpirit);
            });
        }

        int TotalPlayersDeaths { get; set; } = 0;

        /// <summary>
        /// The run will end after max(1, deathCount) player's deaths.
        /// </summary>
        public void EndRunAfterDeaths(int deathCount)
        {
            var playerState = State.World.PlayerState;
            playerState
                .AddOnDeath(() =>
                {
                    if(++TotalPlayersDeaths == deathCount)
                    {
                        State.GC.EndRun();
                    }
                });
        }
    }
}
