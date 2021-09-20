namespace snippet1
{
	interface ITalkative
	{
		void Say(string what);
	}

	class Person : ITalkative
	{
		public string Country = "USA";
		public string Name;
		public int Age = 1;

		public void Scenario1()
		{
			Age = 25;

			if (Age > 10)
			{
			}
		}

		public void Scenario2()
		{
			for (int i = 0; i < 15; ++i)
			{
				if (++Age > i)
                {
					 i = 15;
                }
				if (i < 5) { }
			}
		}

		public void Scenario3()
		{
			int k = 0, j = 0;
			k++;
			if (k != j)
			{
				j = 1;
				if (j != k)
				{
				}
			}
		}

		public bool IsAdult()
		{
			if (Country == "USA" && Age >= 16)
				return Age >= 21;
			else
				return Age >= 18;
		}

		public bool MayDrink()
		{
			return IsAdult();
		}

		public int KoreanAge
		{
			get => Age - 1;
		}

		public void Say(string what)
		{
			System.Console.WriteLine(Name);
			System.Console.WriteLine(" said: ");
			System.Console.WriteLine(what);
		}
	}
}