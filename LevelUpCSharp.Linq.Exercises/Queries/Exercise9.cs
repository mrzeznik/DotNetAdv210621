using System.Collections.Generic;
using System.Linq;
using LevelUpCSharp.Linq.Model;
using LevelUpCSharp.Linq.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LevelUpCSharp.Linq.Queries
{
	[TestClass]
	public class Exercise9
	{

		[TestMethod]
		public void Exercises9_Run()
		{
			// arrange
			Company[] allCompanies = new Numbers(10).Select(x => new Company(x.ToString())).ToArray();
			Person[] allPersons = new Numbers(10).Select(x => new Person((x).ToString())).ToArray();

            var match = allPersons.Zip(
                allCompanies, 
                ((person, company) => new {Company = company, Person = person}));

			foreach (var item in match)
			{
				Assert.AreEqual(item.Company.Name, item.Person.Name);
			}

		}

	}
}
