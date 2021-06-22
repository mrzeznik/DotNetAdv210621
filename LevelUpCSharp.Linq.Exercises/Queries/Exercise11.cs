﻿using System;
using System.Collections.Generic;
using System.Linq;
using LevelUpCSharp.Linq.Model;
using LevelUpCSharp.Linq.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LevelUpCSharp.Linq.Queries
{
	[TestClass]
	public class Exercise11
	{
		[TestMethod]
		public void Exercises11_Run()
		{
			// arrange
			Company[] allCompanies = new Numbers(10).Select(x => new Company()).ToArray();
			Random random = new Random();
			foreach (Company c in allCompanies)
			{
				for (int i = 0; i < random.Next(1, 25); i++)
				{
					c.CreateEmployee();
				}
			}

            var results = allCompanies.ToDictionary(
                x => x,
                x => new { Employees = x.Employees, Company = x}); 

            // assert
			foreach (var item in results)
			{
				Assert.AreEqual(item.Key, item.Value.Company);
				Assert.AreEqual(item.Key.Employees.Count(), item.Value.Company.Employees.Count());
			}
		}

		private class EmployeesCollection
		{
			public IList<Employee> Employees { get; private set; }
			public Company Company { get; private set; }

			public EmployeesCollection(IList<Employee> employees, Company company)
			{
				Employees = employees;
				Company = company;
			}
		}
	}

}
