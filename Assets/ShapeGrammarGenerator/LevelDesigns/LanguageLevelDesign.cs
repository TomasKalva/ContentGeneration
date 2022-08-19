﻿using ContentGeneration.Assets.UI;
using ContentGeneration.Assets.UI.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShapeGrammar
{
    /*
    public class LanguageLevelDesign
    {
        LevelDevelopmentKit ldk;
        Libraries lib;
        World world;

        public LanguageLevelDesign(LevelDevelopmentKit ldk, Libraries lib, World world)
        {
            this.ldk = ldk;
            this.lib = lib;
            this.world = world;
        }

        public LevelElement CreateLevel()
        {
            // Declaration
            MyLanguage language;
            ShapeGrammarState grammarState;
            {
                grammarState = new ShapeGrammarState(ldk);
                var levelConstructor = new LevelConstructor();
                var languageState = new LanguageState(levelConstructor, ldk);
                languageState.Restart(world);
                var gr = new Grammars(ldk);
                var sym = new Symbols();
                ProductionProgram.pr = new Productions(ldk, sym);
                ProductionProgram.ldk = ldk;

                language = new MyLanguage(new LanguageParams(lib, gr, languageState));

                language.MyWorldStart();
            }


            language.State.LC.Construct();

            language.Instantiate();

            // enable disabling enemies in distance
            var spacePartitioning = new SpacePartitioning(language.State.TraversabilityGraph);
            var playerState = GameViewModel.ViewModel.PlayerState;
            playerState.OnUpdate = () =>
            {
                var playerGridPosition = Vector3Int.RoundToInt(world.WorldGeometry.WorldToGrid(GameViewModel.ViewModel.PlayerState.Agent.transform.position));
                var playerNode = language.State.GrammarState.GetNode(playerGridPosition);
                spacePartitioning.Update(playerNode);
            };

            grammarState.Print(new PrintingState()).Show();
            grammarState.Stats.Print();

            var level = grammarState.WorldState.Added;
            return level;
        }
    }*/
}
