using System;
using System.Collections.Generic;
using System.Linq;
using LevelUpCSharp.Linq.Model;
using LevelUpCSharp.Linq.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LevelUpCSharp.Linq.Queries
{
	[TestClass]
	public class Exercise8
	{

		[TestMethod]
		public void Exercises8_Run()
		{
			// arrange
			Company[] allCompanies = new Numbers(10).Select(x => new Company()).ToArray();
			foreach (Company c in allCompanies)
			{
				for (int i = 0; i < 5; i++)
				{
					c.CreateEmployee();
				}
			}

            var result = allCompanies.SelectMany(c => c.Employees)
                .GroupBy(
                    x => x.Salear > 2500,
                    (importand, employees) =>
                        new
                        {
                            IsImportand = importand,
                            Employees = employees,
                            MaxSalar = employees.Max(e => e.Salear),
                            MinSalar = employees.Min(e => e.Salear),
                        })
                .OrderBy(x => x.IsImportand)
                .ToArray();

            // assert
			Assert.IsFalse(result.First().Employees.Any(x => x.Salear <= 2500));
            Assert.IsFalse(result.Last().Employees.Any(x => x.Salear > 2500));

			/*
			Assert.AreEqual(notImportantEmployees.Max(x => x.Salear), maxSalearNotImportant);
			Assert.AreEqual(notImportantEmployees.Min(x => x.Salear), minSalearNotImportant);
			Assert.IsFalse(importantEmployees.Exists(x => x.Salear <= 2500));
			Assert.AreEqual(importantEmployees.Max(x => x.Salear), maxSalearImportant);
			Assert.AreEqual(importantEmployees.Min(x => x.Salear), minSalearImportant);
			*/
		}

	}
}
