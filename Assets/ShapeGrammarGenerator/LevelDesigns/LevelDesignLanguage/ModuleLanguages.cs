using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeGrammar
{
    class LevelLanguage : LDLanguage
    {
        public LevelLanguage(LanguageParams tools) : base(tools) { }

        public void LevelStart(out Area area)
        {
            Env.AddOne(Gr.PrL.CreateNewHouse(), out area);
        }

        public void LevelPathSegment()
        {

        }

        public void LevelEnd()
        {

        }
    }

    class BrothersLanguage : LDLanguage
    {
        public BrothersLanguage(LanguageParams tools) : base(tools) { }

        public void ThymeTea()
        {

        }

        public void GiftOfHope()
        {

        }
    }

    class FarmersLanguage : LDLanguage
    {
        public FarmersLanguage(LanguageParams tools) : base(tools) { }

        public void FarmerBranch(int progress)
        {
            Env.AddLine(Gr.PrL.Garden(), 2, out var path_to_farmer);
            Env.AddOne(Gr.PrL.Garden(), out var farmer_area);
            farmer_area.AddInteractiveObject(
                Lib.InteractiveObjects.NewInteractiveObject("Farmer", Lib.InteractiveObjects.Geometry(Lib.Objects.farmer))
            //.Show("Bring me apples")
            /*.SetInteract("Give apples",
                () =>
                {
                    Debug.Log("Interacting with farmer");
                    //Levels().Next().AddPossibleBranch(FarmerBranch(progress + 1);
                    //Player.AddSpirit(10 * progress);
                })*/
                );
            UnityEngine.Debug.Log($"{farmer_area.InteractiveObjectStates.Count}");
            UnityEngine.Debug.Log($"Is farmer area in state: {State.TraversableAreas.Contains(farmer_area)}");
            Env.AddRandom(Gr.PrL.Garden(), 5, out var garden);

        }
    }
}
