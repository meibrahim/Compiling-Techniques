namespace snippet2
{
	class Shape
	{
		public int PosX = 0;
		public int PosY = 0;
	}

	class Box : Shape
	{
		public int Width = 1;
		public int Height;

		public bool Inside(int x, int y)
		{
			return x >= PosX
				&& y >= PosY
				&& x < PosX + Width 
				&& y < PosY + Height;
		}
	}

	class Circle : Shape
	{
		public double Radius;

		public bool Inside(int x, int y)
		{
			return System.Math.Sqrt((PosX - x) * (PosX - x) + (PosY - y) * (PosY - y)) <= Radius;
		}
	}
}