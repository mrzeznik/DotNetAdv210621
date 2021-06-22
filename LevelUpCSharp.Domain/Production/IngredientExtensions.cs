using System;
using System.Collections.Generic;
using System.Text;

namespace LevelUpCSharp.Production
{
    public static class IngredientExtensions
    {
        public static IEnumerable<string> AsStrings(this List<IIngredient> source)
        {
            foreach (IIngredient ingredient in source)
            {
                yield return ingredient.Name;
            }
        }
    }
}
