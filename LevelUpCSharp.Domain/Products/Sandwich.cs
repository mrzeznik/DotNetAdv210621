using System;
using System.Collections.Generic;
using System.Text;

namespace LevelUpCSharp.Products
{
    public class Sandwich
    {
        protected Sandwich()
        {
        }

        public SandwichKind Kind { get; set; }

        public DateTimeOffset ExpirationDate { get; set; }
    }
}
