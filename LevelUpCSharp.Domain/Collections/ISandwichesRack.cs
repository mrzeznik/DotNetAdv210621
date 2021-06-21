using System.Collections.Generic;
using System.Text;

namespace LevelUpCSharp.Collections
{
    internal interface ISandwichesRack<in TKind, TElement> : IEnumerable<TElement>
    {
        void Add(TElement sandwich);

        TElement Get(TKind kind);

        bool Contains(TKind kind);
    }
}
