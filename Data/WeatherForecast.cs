using System;

namespace Cloudberry.Data
{
	// public record WeatherForecast
	// {
	// 	public DateTime Date { get; init; }
	// 	public int TemperatureC { get; init; }
	// 	public string Summary { get; init; }
	// 	public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
	// }

	public class WeatherForecast
	{
		public DateTime Date { get; set; }

		public int TemperatureC { get; set; }

		public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

		public string Summary { get; set; }
	}
}
