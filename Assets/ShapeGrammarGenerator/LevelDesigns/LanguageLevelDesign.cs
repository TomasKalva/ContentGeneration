using ContentGeneration.Assets.UI.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShapeGrammar
{

    public class LanguageLevelDesign : LevelDesign
    {
        Libraries lib;

        public LanguageLevelDesign(LevelDevelopmentKit ldk, Libraries lib) : base(ldk)
        {
            this.lib = lib;
        }

        public override LevelElement CreateLevel()
        {
            var grammarState = new ShapeGrammarState(ldk);
            var languageState = new LanguageState(grammarState);
            var gr = new Grammars(ldk);
            var sym = new Symbols();
            ProductionProgram.pr = new Productions(ldk, sym);
            ProductionProgram.ldk = ldk;
            ProductionProgram.StyleRules = ldk.houseStyleRules;

            MyLanguage language = new MyLanguage(new LanguageParams(ldk, lib, gr, languageState));

            language.MyWorldStart();

            language.State.LC.Construct();

            language.Instantiate();

            // enable disabling enemies in distance
            var spacePartitioning = new SpacePartitioning(language.State.TraversabilityGraph);
            var playerState = GameViewModel.ViewModel.PlayerState;
            playerState.OnUpdate += () =>
            {
                var playerGridPosition = Vector3Int.RoundToInt(language.Ldk.gg.WorldToGrid(GameViewModel.ViewModel.PlayerState.Agent.transform.position));
                var playerNode = language.State.GrammarState.GetNode(playerGridPosition);
                spacePartitioning.Update(playerNode);
            };

            grammarState.Print(new PrintingState()).Show();
            grammarState.Stats.Print();

            var level = grammarState.WorldState.Added;
            return level;
        }
    }
}
