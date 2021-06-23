using System.Collections.Generic;

namespace LevelUpCSharp.Production
{
    public static class IngredientsSetExtensions
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