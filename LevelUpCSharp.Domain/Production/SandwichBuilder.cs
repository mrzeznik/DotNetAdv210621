using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LevelUpCSharp.Products;

namespace LevelUpCSharp.Production
{
    public class SandwichBuilder : IStarter, IAdditionable, IToppingable, ISandwichBuilder
    {
        private SandwichKind _type = SandwichKind.Cheese;

        private readonly List<IIngredient> _ingredients = new List<IIngredient>(4);
        private bool _hasButter;
        private int _extras;
        private readonly int _maxExtras;

        public SandwichBuilder(int maxExtras = 5)
        {
            _maxExtras = maxExtras;
        }

        public Sandwich Wrap()
        {
            return new Sandwich(_type, DateTimeOffset.Now.AddHours(3), _ingredients.AsStrings().ToArray());
        }

        public ISandwichBuilder Add(ITopping topping)
        {
            _ingredients.Add(topping);
            return this;
        }

        public IAdditionable AddExtra(IAddition ingredient)
        {
            if (_extras > _maxExtras)
            {
            }

            _extras ++;
            _ingredients.Add(ingredient);
            return this;
        }

        public IAdditionable Add(IKeyIngredient ingredient)
        {
            _type = ingredient.Kind;
            _ingredients.Add(ingredient);
            return this;
        }

        public IAdditionable AddButter()
        {
            if (_hasButter)
            {
                return this;
            }

            _hasButter = true;
            _ingredients.Add(new Butter());

            return this;
        }

        public string Name { get; }

        public DateTimeOffset ExpirationDate { get; }
    }

    public interface IKeyIngredient : IIngredient
    {
        SandwichKind Kind { get; }
    }

    #region key ingredients
    public class GrilledChicken : IKeyIngredient
    {
        public SandwichKind Kind => SandwichKind.Chicken;
        public string Name => "grilled chicken";
        public DateTimeOffset ExpirationDate => DateTimeOffset.Now.AddDays(10);
    }

    public class Ham : IKeyIngredient
    {
        public SandwichKind Kind => SandwichKind.Pork;
        public string Name => "ham";
        public DateTimeOffset ExpirationDate => DateTimeOffset.Now.AddDays(10);
    }

    public class Kebap : IKeyIngredient
    {
        public SandwichKind Kind => SandwichKind.Pork;
        public string Name => "kebap";
        public DateTimeOffset ExpirationDate => DateTimeOffset.Now.AddDays(10);
    }

    public class KebapChicken : IKeyIngredient
    {
        public SandwichKind Kind => SandwichKind.Chicken;
        public string Name => "chicken kebap";
        public DateTimeOffset ExpirationDate => DateTimeOffset.Now.AddDays(10);
    }

    public class PulledBeef : IKeyIngredient
    {
        public SandwichKind Kind => SandwichKind.Beef;
        public string Name => "pullled beef";
        public DateTimeOffset ExpirationDate => DateTimeOffset.Now.AddDays(10);
    }

    public class Cheese : IKeyIngredient, IAddition
    {
        public SandwichKind Kind => SandwichKind.Cheese;
        public string Name => "cheese";
        public DateTimeOffset ExpirationDate => DateTimeOffset.Now.AddDays(10);
    }

    public class Fish : IKeyIngredient
    {
        public string Name { get; }
        public DateTimeOffset ExpirationDate { get; }
        public SandwichKind Kind { get; }
    }
    #endregion

    public interface IAddition : IIngredient
    {
    }

    #region additions 
    public class Lettuce : IAddition
    {
        public string Name => "lettuce";
        public DateTimeOffset ExpirationDate => DateTimeOffset.Now.AddMonths(1);
    }

    public class Olives : IAddition
    {
        public string Name => "olives";
        public DateTimeOffset ExpirationDate => DateTimeOffset.Now.AddMonths(1);
    }

    public class Onion : IAddition
    {
        public string Name => "onion";
        public DateTimeOffset ExpirationDate => DateTimeOffset.Now.AddMonths(1);
    }

    public class Tomato : IAddition
    {
        public string Name => "tomato";
        public DateTimeOffset ExpirationDate => DateTimeOffset.Now.AddMonths(1);
    }
    #endregion

    public interface ITopping : IIngredient
    {
    }

    #region toppings
    public class Ketchup : ITopping
    {
        public string Name => "Ketchup";
        public DateTimeOffset ExpirationDate => DateTimeOffset.Now.AddDays(3);
    }

    public class Mayo : ITopping
    {
        public string Name => "Mayo";
        public DateTimeOffset ExpirationDate => DateTimeOffset.Now.AddDays(3);
    }

    public class Mustard : ITopping
    {
        public string Name => "mustard";
        public DateTimeOffset ExpirationDate => DateTimeOffset.Now.AddDays(3);
    }
    #endregion

    public interface IIngredient
    {
        string Name { get; }

        DateTimeOffset ExpirationDate { get; }
    }

    #region  ingredients

    public class Butter : IIngredient
    {
        public string Name => "butter";

        public DateTimeOffset ExpirationDate { get; }
    }

    #endregion

    public interface ISandwichBuilder
    {
        Sandwich Wrap();
    }

    public interface IToppingable : ISandwichBuilder
    {
        ISandwichBuilder Add(ITopping topping);
    }

    public interface IAdditionable : IToppingable, ISandwichBuilder
    {
        IAdditionable AddExtra(IAddition addition);
    }

    public interface IStarter : IAdditionable, IToppingable, ISandwichBuilder
    {
        IAdditionable Add(IKeyIngredient ingredient);
    }
}
