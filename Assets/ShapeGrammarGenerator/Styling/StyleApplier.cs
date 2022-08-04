using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeGrammar
{
    public class StyleApplier
    {
        public void Apply(LevelElement levelElement)
        {
            levelElement.Leafs().ForEach(le => le.ApplyStyle());
        }
    }
}
