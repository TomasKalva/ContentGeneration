using ContentGeneration.Assets.UI.Model;
using OurFramework.Environment.ShapeGrammar;
using System;
using System.Linq;
using UnityEngine;

namespace OurFramework.LevelDesignLanguage.CustomModules
{

    class EnvironmentModule : LDLanguage
    {
        /// <summary>
        /// Can modify the environment map.
        /// </summary>
        public struct SkyParameters
        {
            float SkyVariability { get; }
            float SkyBrightness { get; }

            public SkyParameters(float skyVariability, float skyBrightness)
            {
                SkyVariability = skyVariability;
                SkyBrightness = skyBrightness;
            }

            public void Set(EnvironmentMap environment)
            {
                environment.SetSkyVariability(SkyVariability);
                environment.SetSkyBrightness(SkyBrightness);
            }
        }

        public SkyParameters[] SkyParams { get; }
        public EnvironmentModule(LanguageParams parameters) : base(parameters) 
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

        /// <summary>
        /// State for EnvironmentMap.
        /// </summary>
        public class EnvironmentState
        {
            GeometryMaker<EnvironmentMap> GeometryMaker { get; }
            EnvironmentMap Environment { get; set; }
            SkyParameters CurrentParameters { get; set; }

            public EnvironmentState(GeometryMaker<EnvironmentMap> makeGeometry)
            {
                GeometryMaker = makeGeometry;
            }

            public void MakeGeometry()
            {
                Environment = GeometryMaker();
                CurrentParameters.Set(Environment);
            }

            public void SetParameters(SkyParameters skyParameters)
            {
                CurrentParameters = skyParameters;
                if (Environment != null)
                {
                    skyParameters.Set(Environment);
                }
            }
        }

        /// <summary>
        /// Returns parameters of the environment map for the given level.
        /// </summary>
        public SkyParameters GetSkyParameter(int level)
            => SkyParams[Mathf.Clamp(level, 0, SkyParams.Length - 1)];

        /// <summary>
        /// Creates sky and sea.
        /// </summary>
        public void CreateSky(int level)
        {
            var envState = new EnvironmentState(
                () =>
                {
                    var env = Lib.Objects.EnvironmentMap();
                    State.World.AddSpecialObject(env.transform);
                    RenderSettings.sun = env.Sun;
                    return env;
                });
            State.World.OnLevelStart += envState.MakeGeometry;

            var parameters = GetSkyParameter(level);
            envState.SetParameters(parameters);
        }
    }
}
