using System;

public class WeatherEntity
{
	public class WeatherEntity : TableEntity
	{
		public string Grad { get; set; }
		public string Tid { get; set; }
		public string Nederbörd { get; set; }
		public string Vindstyrka { get; set; }
	}
}
