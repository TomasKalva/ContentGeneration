using ContentGeneration.Assets.UI;
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

        public void DropBloodstain()
        {
            var playerState = State.World.PlayerState;
            playerState.AddOnDeath(() =>
            {
                    // Lose spirit at the position
                    var deathPosition = playerState.Agent.transform.position;
                    var lostSpirit = playerState.Spirit;
                    playerState.Spirit = 0;

                    Action spawnBloodstain = null;
                    Action bloodstainRemoval = null;

                    // Create bloodstain
                    var bloodstain = Lib.InteractiveObjects.Bloodstain(
                        () =>
                        {
                            playerState.Spirit += lostSpirit;
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

                    //
                    
                }
            );
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
