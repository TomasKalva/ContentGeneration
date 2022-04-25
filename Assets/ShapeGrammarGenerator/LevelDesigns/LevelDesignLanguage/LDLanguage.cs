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
        LevelLanguage<LDLanguage>, 
        BrothersLanguage<LDLanguage>,
        FarmersLanguage<LDLanguage>
    {
        protected ShapeGrammarState GrammarState { get; }
        protected LevelDevelopmentKit Ldk { get; }
        protected Libraries Lib { get; }

        ShapeGrammarState ILDLanguage<LDLanguage>.GrammarState => GrammarState;
        LevelDevelopmentKit ILDLanguage<LDLanguage>.Ldk => Ldk;
        Libraries ILDLanguage<LDLanguage>.Lib => Lib;

        public LDLanguage(ShapeGrammarState grammarState, LevelDevelopmentKit ldk, Libraries lib)
        {
            GrammarState = grammarState;
            Ldk = ldk;
            Lib = lib;
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

    interface ILDLanguage<LanguageUserT>
    {
        protected ShapeGrammarState GrammarState { get; }
        protected LevelDevelopmentKit Ldk { get; }
        protected Libraries Lib { get; }
        protected LanguageUserT This => (LanguageUserT)this;
    }

    #region Language tools

    interface IEnvironmentCreator<LanguageUserT> : ILDLanguage<LanguageUserT>
    {
        public LanguageUserT AddLine(ProductionList productions, int count)
        {
            var linearGrammar = new CustomGrammar(productions, count, null, state => state.LastCreated);
            linearGrammar.Evaluate(GrammarState);
            return This;
        }

        public LanguageUserT AddOne(ProductionList productions, out Area one)
        {
            var grammar = new RandomGrammar(productions, 1);
            grammar.Evaluate(GrammarState);
            one = null;
            return This;
        }

        public LanguageUserT AddRandom(ProductionList productions, int count)
        {
            var grammar = new RandomGrammar(productions, count);
            grammar.Evaluate(GrammarState);
            return This;
        }
    }

    interface IProductions<LanguageUserT> : ILDLanguage<LanguageUserT>
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

    interface IInteractiveObjects<LanguageUserT> : ILDLanguage<LanguageUserT>
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

    interface LevelLanguage<LanguageUserT> 
        : ILDLanguage<LanguageUserT>,
            IEnvironmentCreator<LanguageUserT>,
            IProductions<LanguageUserT>
    {
        public LanguageUserT LevelStart(out Area area)
        {
            AddOne(CreateNewHouse(), out area);
            return This;
        }

        public LanguageUserT LevelPathSegment()
        {

            return This;
        }

        public LanguageUserT LevelEnd()
        {

            return This;
        }
    }

    interface BrothersLanguage<LanguageUserT> : ILDLanguage<LanguageUserT>
    {
        public LanguageUserT ThymeTea()
        {

            return This;
        }

        public LanguageUserT GiftOfHope()
        {

            return This;
        }
    }

    interface FarmersLanguage<LanguageUserT> 
        : IEnvironmentCreator<LanguageUserT>,
          IProductions<LanguageUserT>,
          IInteractiveObjects<LanguageUserT>
    {
        public LanguageUserT FarmerBranch(int progress)
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

            return This;
        }
    }

    #endregion

}
