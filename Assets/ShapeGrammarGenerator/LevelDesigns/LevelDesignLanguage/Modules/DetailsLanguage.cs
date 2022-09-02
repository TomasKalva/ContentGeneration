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
using static InteractiveObject;

namespace Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage
{
    class DetailsLanguage : LDLanguage
    {
        public DetailsLanguage(LanguageParams tools) : base(tools) 
        {
            string itemTooltip = "The volatile presence guides.";
            Items = new List<Func<ItemState>>()
            {
                () => Lib.Items.NewItem("Presence of small Dog", $"Dog, o'Dog, why are you so small? Your little presence is the most welcome one. {itemTooltip}").OnUse(user => user.Spirit += 100),
                () => Lib.Items.NewItem("Presence of lovely Dog", $"The heartwarming doggie, rests in its nest. Her loveliness shall not go unnoticed. {itemTooltip}").OnUse(user => user.Spirit += 200),
                () => Lib.Items.NewItem("Presence of angry Dog", $"A heap of barks is turning into a mountain. Will your barking know any limits? {itemTooltip}").OnUse(user => user.Spirit += 500),
                () => Lib.Items.NewItem("Presence of impatient Dog", $"Dog knows where. Dog knows how. Dog, however lacks the time. {itemTooltip}").OnUse(user => user.Spirit += 1000),
                () => Lib.Items.NewItem("Presence of pure Dog", $"Are you even real? A dog so clean, a dog so proud. If only petting was allowed. {itemTooltip}").OnUse(user => user.Spirit += 2000),
                () => Lib.Items.NewItem("Absence of Dog", $"The dog is gone? How could it. Will it return? No one knows. Let us rejoice in her past presence for a few last times.").OnUse(user => user.Spirit += 3000),
            };
        }

        List<Func<ItemState>> Items { get; }

        int ItemLevel(int level)
        {
            var desiredLevel = new UniformDistr(level - 1, level + 1).Sample();
            return Math.Clamp(desiredLevel, 0, Items.Count - 1);
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
