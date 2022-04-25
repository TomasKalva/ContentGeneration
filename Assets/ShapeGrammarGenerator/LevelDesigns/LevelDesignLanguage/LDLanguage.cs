using ContentGeneration.Assets.UI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShapeGrammar
{
    class LDLanguage :
        ILDLanguageImpl
    {
        protected ShapeGrammarState GrammarState { get; }
        protected LevelDevelopmentKit Ldk { get; }
        protected Libraries Lib { get; }

        ShapeGrammarState ILDLanguage.GrammarState => GrammarState;
        LevelDevelopmentKit ILDLanguage.Ldk => Ldk;
        Libraries ILDLanguage.Lib => Lib;

        public LDLanguage(ShapeGrammarState grammarState, LevelDevelopmentKit ldk, Libraries lib)
        {
            GrammarState = grammarState;
            Ldk = ldk;
            Lib = lib;
        }
    }

    interface ILDLanguageImpl : 
        LevelLanguage, 
        BrothersLanguage,
        FarmersLanguage
    {
        public void Level()
        {
            LevelStart(out var area);
            FarmerBranch(0);
        }
    }

    #region Primitives

    class Area
    {
        List<InteractiveObjectState> InteractiveObjectStates { get; }

        public void AddInteractiveObject(InteractiveObjectState interactiveObject)
        {
            InteractiveObjectStates.Add(interactiveObject);
        }

        public void InstantiateAll()
        {
            foreach(var ios in InteractiveObjectStates)
            {
                ios.MakeGeometry();
            }
        }
    }

    class AreasConnection
    {

    }

    class Enemy
    {

    }

    class Item
    {

    }

    class GeometryMaker
    {
        Transform geometry;
        Func<Transform> GeometryF { get; }

        public GeometryMaker(Func<Transform> geometryF)
        {
            GeometryF = geometryF;
        }

        public Transform CreateGeometry()
        {
            geometry = GeometryF();
            return geometry;
        }
    }

    #endregion

    interface ILDLanguage
    {
        protected ShapeGrammarState GrammarState { get; }
        protected LevelDevelopmentKit Ldk { get; }
        protected Libraries Lib { get; }
    }

    #region Language tools

    interface IEnvironmentCreator : ILDLanguage
    {
        public void AddLine(ProductionList productions, int count)
        {
            var linearGrammar = new CustomGrammar(productions, count, null, state => state.LastCreated);
            linearGrammar.Evaluate(GrammarState);
        }

        public void AddOne(ProductionList productions, out Area one)
        {
            var grammar = new RandomGrammar(productions, 1);
            grammar.Evaluate(GrammarState);
            one = null;
        }

        public void AddRandom(ProductionList productions, int count)
        {
            var grammar = new RandomGrammar(productions, count);
            grammar.Evaluate(GrammarState);
        }
    }

    interface IProductions : ILDLanguage
    {
        protected ProductionLists PrL => new ProductionLists(Ldk);
        protected Productions Pr => new Productions(Ldk);

        public ProductionList CreateNewHouse() => PrL.CreateNewHouse(Pr);
        public ProductionList Garden() => PrL.Garden(Pr);
        public ProductionList GuidedGarden(PathGuide guide) => PrL.GuidedGarden(Pr, guide);
        public ProductionList Graveyard() => PrL.Graveyard(Pr);
        public ProductionList GraveyardPostprocess() => PrL.GraveyardPostprocess(Pr);
        public ProductionList ConnectBack() => PrL.ConnectBack(Pr);
        public ProductionList Roofs() => PrL.Roofs(Pr);
    }

    interface IInteractiveObjects : ILDLanguage
    {
        public GeometryMaker Geometry(Transform prefab)
        {
            return new GeometryMaker(() => GameObject.Instantiate(prefab));
        }

        public InteractiveObjectState NewInteractiveObject(string name, GeometryMaker geometryMaker)
        {
            var newInteractiveObject = new InteractiveObjectState()
            {
                Name = name,
            };
            return newInteractiveObject;
        }
    }

    #endregion

    #region Module languages
    /// <summary>
    /// Conatains declaration of all data members of LDLanguage, so that the
    /// module sub-languages can use them.
    /// </summary>
    interface LevelLanguage 
        : ILDLanguage,
            IEnvironmentCreator,
            IProductions
    {
        public void LevelStart(out Area area)
        {
            AddOne(CreateNewHouse(), out area);
        }

        public void LevelPathSegment()
        {

        }

        public void LevelEnd()
        {

        }
    }

    interface BrothersLanguage : ILDLanguage
    {
        public void ThymeTea()
        {

        }

        public void GiftOfHope()
        {

        }
    }

    interface FarmersLanguage 
        : IEnvironmentCreator,
          IProductions,
          IInteractiveObjects
    {
        public void FarmerBranch(int progress)
        {
            AddLine(Garden(), 2);
            AddOne(Garden(), out var farmer_area);
            //farmer_area.AddInteractiveObject(
            //    NewInteractiveObject("Farmer", Geometry(Lib.Objects.farmer))
                    //.Show("Bring me apples")
                    /*.SetInteract("Give apples",
                        () =>
                        {
                            Debug.Log("Interacting with farmer");
                            //Levels().Next().AddPossibleBranch(FarmerBranch(progress + 1);
                            //Player.AddSpirit(10 * progress);
                        })*/
            //    );
            AddRandom(Garden(), 5);

        }
    }

    #endregion

}
