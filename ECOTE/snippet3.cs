namespace snippet2
{
	class Person
	{
		private double Inebriation;
		public string Country;
		public string Name;
		public int Age;
		public int? Passport;
		public int? License;

		public bool IsAdult()
		{
			if (Country == "USA")
				return Age >= 21;
			else
				return Age >= 18;
		}

		public bool CanDrink()
		{
			return Passport.HasValue && IsAdult() && Inebriation < 1.0;
		}

		public bool CanDrive()
		{
			return Passport.HasValue && License.HasValue && IsAdult();
		}

		public void ImportantDecisionmaking()
		{
			if (CanDrink()) Drink();
			else if (CanDrive()) Drive();
			else if (Universe.NextDouble() > 0.5) Sleep();
			else Study();
		}

		public void Drink()
		{
			System.Console.WriteLine("Cheers");
			Inebriation += 0.1;
		}

		public void Drive()
		{
			System.Console.WriteLine("Let's go...");

			if (!License.HasValue)
			{
				System.Console.WriteLine("Arrested");
			}

			if (!(Universe.NextDouble() > Inebriation))
			{
				System.Console.WriteLine("Woops");
				License = null;
			}
		}

		public void Study()
		{
			System.Console.WriteLine("Oh, man!");
		}

		public void Sleep()
		{
			Inebriation = 0;
		}

		static System.Random Universe = new System.Random();
	}
}