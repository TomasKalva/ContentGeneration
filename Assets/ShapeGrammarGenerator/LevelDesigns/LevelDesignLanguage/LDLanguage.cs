using ContentGeneration.Assets.UI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShapeGrammar
{
    abstract class LDLanguage
    {
        public ShapeGrammarState GrammarState { get; }


        public LevelDevelopmentKit Ldk { get; }
        public Libraries Lib { get; }
        public Grammars Gr { get; }
        public Languages L { get; }
        public Environments Env { get; }

        public LDLanguage(LanguageParams languageParams)
        {
            Ldk = languageParams.Ldk;
            Lib = languageParams.Lib;
            Gr = languageParams.Gr;

            GrammarState = languageParams.GrammarState;
            Env = languageParams.Env;
            L = Languages.Get(languageParams);
        }
    }

    class LanguageParams
    {
        public LevelDevelopmentKit Ldk { get; }
        public Libraries Lib { get; }
        public Grammars Gr { get; }
        public ShapeGrammarState GrammarState { get; }
        public Environments Env { get; }

        public LanguageParams(LevelDevelopmentKit ldk, Libraries lib, Grammars gr, ShapeGrammarState grammarState, Environments env)
        {
            Ldk = ldk;
            Lib = lib;
            Gr = gr;
            GrammarState = grammarState;
            Env = env;
        }
    }

    class MyLanguage : LDLanguage
    {
        public MyLanguage(LanguageParams tools) : base(tools) { }

        public void MyLevel()
        {
            L.LevelLanguage.LevelStart(out var startArea);
            L.FarmersLanguage.FarmerBranch(0);
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

    class Languages
    {
        /// <summary>
        /// Singleton to prevent cycles when creating languages.
        /// </summary>
        static Languages _get;
        public static Languages Get(LanguageParams tools)
        {
            if (_get == null)
            {
                _get = new Languages();
                _get.Init(tools);
            }
            return _get;
        }

        public LevelLanguage LevelLanguage { get; private set; }
        public BrothersLanguage BrothersLanguage { get; private set; }
        public FarmersLanguage FarmersLanguage { get; private set; }

        Languages()
        {
        }

        void Init(LanguageParams tools)
        {
            LevelLanguage = new LevelLanguage(tools);
            BrothersLanguage = new BrothersLanguage(tools);
            FarmersLanguage = new FarmersLanguage(tools);
        }
    }

    #region Language tools

    class Environments
    {
        ShapeGrammarState GrammarState { get; }

        public Environments(ShapeGrammarState grammarState)
        {
            GrammarState = grammarState;
        }

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

    class Grammars
    {
        public ProductionLists PrL { get; }
        public Productions Pr { get; }
        public Symbols Sym { get; }

        public Grammars(LevelDevelopmentKit ldk)
        {
            Sym = new Symbols();
            Pr = new Productions(ldk, Sym);
            PrL = new ProductionLists(ldk, Pr);
        }
    }
    /*
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
    }*/

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

    class FarmersLanguage 
        : LDLanguage
    {
        public FarmersLanguage(LanguageParams tools) : base(tools) {}

        public void FarmerBranch(int progress)
        {
            Env.AddLine(Gr.PrL.Garden(), 2);
            Env.AddOne(Gr.PrL.Garden(), out var farmer_area);
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
            Env.AddRandom(Gr.PrL.Garden(), 5);

        }
    }

    #endregion

}
