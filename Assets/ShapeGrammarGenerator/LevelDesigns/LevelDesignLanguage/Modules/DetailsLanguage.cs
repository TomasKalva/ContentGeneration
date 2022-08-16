using Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage.Factions;
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
    class DetailsLanguage : LDLanguage
    {
        public DetailsLanguage(LanguageParams tools) : base(tools) 
        {
            Items = new List<Func<ItemState>>()
            {
                () => Lib.Items.NewItem("Will of a small Dog 1", "Why dog?").OnUse(user => user.Spirit += 100),
                () => Lib.Items.NewItem("Will of a small Dog 2", "Why dog?").OnUse(user => user.Spirit += 200),
                () => Lib.Items.NewItem("Will of a small Dog 3", "Why dog?").OnUse(user => user.Spirit += 500),
                () => Lib.Items.NewItem("Will of a small Dog 4", "Why dog?").OnUse(user => user.Spirit += 1000),
                () => Lib.Items.NewItem("Will of a small Dog 5", "Why dog?").OnUse(user => user.Spirit += 2000),
                () => Lib.Items.NewItem("Will of a small Dog 6", "Why dog?").OnUse(user => user.Spirit += 3000),
            };
        }

        List<Func<ItemState>> Items { get; }

        int ItemLevel(int level)
        {
            var desiredLevel = new UniformDistr(level, level + 2).Sample();
            return Math.Min(Items.Count - 1, desiredLevel);
        }

        public void AddDetails(int level)
        {
            var allDetails = new ProductionList(
                Gr.PrL.CastleDetails().Get
                    .Concat(Gr.PrL.ChapelsDetails().Get)
                    .Concat(Gr.PrL.TownDetails().Get)
                    .ToArray());

            Env.BranchRandomly(allDetails, 6, out var disconnectedDetails);

            var itemPlacer = PlO.EvenPlacer(
                Enumerable.Range(0, disconnectedDetails.AreasList.Count)
                .Select(_ => Items[ItemLevel(level)]().SetStackable(1))
                .Select(itemState => Lib.InteractiveObjects.Item(itemState)));
            itemPlacer.Place(disconnectedDetails);
        }
    }
}
