using System.Collections.Generic;

namespace OurFramework.LevelDesignLanguage
{
    class UniqueNameGenerator
    {
        Dictionary<string, int> AlreadyGenerated { get; }

        public UniqueNameGenerator()
        {
            AlreadyGenerated = new Dictionary<string, int>();
        }

        public string GenerateUniqueName(List<string> adjectives, List<string> nouns)
        {
            int c = 100;
            string generated = "";
            while (c-- >= 0)
            {
                generated = $"{adjectives.GetRandom()} {nouns.GetRandom()}";
                if (!AlreadyGenerated.ContainsKey(generated))
                {
                    AlreadyGenerated.Add(generated, 0);
                    return generated;
                }
            }
            var n = ++AlreadyGenerated[generated];
            return $"{generated} {n}";
        }

        public string UniqueName(string str)
        {
            if (AlreadyGenerated.TryGetValue(str, out var value))
            {
                AlreadyGenerated[str]++;
                return $"{str} {value}";
            }
            else
            {
                AlreadyGenerated.Add(str, 1);
                return str;
            }
        }
    }
}
