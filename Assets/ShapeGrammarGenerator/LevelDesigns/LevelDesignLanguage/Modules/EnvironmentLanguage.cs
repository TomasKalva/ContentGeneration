﻿using Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage.Factions;
using Assets.Util;
using ContentGeneration.Assets.UI.Model;
using ContentGeneration.Assets.UI.Util;
using ShapeGrammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage.Factions.SelectorLibrary;
using static InteractiveObject;

namespace Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage
{

    class EnvironmentLanguage : LDLanguage
    {
        struct SkyParameters
        {
            float SkyVariability { get; }
            float SkyBrightness { get; }

            public SkyParameters(float skyVariability, float skyBrightness)
            {
                SkyVariability = skyVariability;
                SkyBrightness = skyBrightness;
            }

            public void Set(Environment environment)
            {
                environment.SetSkyVariability(SkyVariability);
                environment.SetSkyBrightness(SkyBrightness);
            }
        }

        SkyParameters[] SkyParams { get; }
        public EnvironmentLanguage(LanguageParams tools) : base(tools) 
        {
            SkyParams = new SkyParameters[8]
            {
                new SkyParameters(1.3f, 1.0f),
                new SkyParameters(1.8f, 0.9f),
                new SkyParameters(2.3f, 0.8f),
                new SkyParameters(0.6f, 0.7f),
                new SkyParameters(0.3f, 0.6f),
                new SkyParameters(0.0f, 0.5f),
                new SkyParameters(-0.5f, 0.4f),
                new SkyParameters(-1.5f, 0.3f),
            };
        }

        SkyParameters GetSkyParameter(int level)
            => SkyParams[Mathf.Clamp(0, SkyParams.Length - 1, level)];


        public void CreateSky(int level)
        {
            var env = Lib.Objects.Environment();
            var parameters = GetSkyParameter(level);
            parameters.Set(env);
            RenderSettings.sun = env.Sun;
        }

        public void TestSky()
        {
            var env = Lib.Objects.Environment();
            Env.One(Gr.PrL.Town(), NodesQueries.All, out var area);

            GetSkyParameter(5).Set(env);

            Func<ItemState>[] skyChangingItems = SkyParams
                .Select<SkyParameters, Func<ItemState>>((skyParams, i) => () => Lib.Items
                 .NewItem($"Set sky {i}", "Sets sky to some texture")
                     .OnUse(user => skyParams.Set(env))).ToArray();

            area.Get.AddInteractiveObject(
                Lib.InteractiveObjects.InteractiveObject("Sky distributor", Lib.InteractiveObjects.Geometry<InteractiveObject>(Lib.Objects.farmer))
                    .SetInteraction(
                        new InteractionSequence<InteractiveObject>()
                            .Act("Take all skies", (ios, player) => skyChangingItems.ForEach(itemF => player.AddItem(itemF()))
                            )
                        )
                    );
        }
    }
}