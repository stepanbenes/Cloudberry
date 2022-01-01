using Iot.Device.CpuTemperature;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cloudberry.Data
{
	public interface ICpuTemperatureService
	{
		double GetTemperatureInCelsius();
	}

	public class RealCpuTemperatureService : ICpuTemperatureService
	{
		public double GetTemperatureInCelsius()
		{
			var cpuTempProvider = new CpuTemperature();
			var cpuTemp = cpuTempProvider.Temperature;
			return cpuTemp.DegreesCelsius;
		}
	}

	public class FakeCpuTemperatureService : ICpuTemperatureService
	{
		private readonly Random random = new();

		public double GetTemperatureInCelsius()
		{
			const double minValue = 20.0;
			const double maxValue = 100.0;

			return random.NextDouble() * (maxValue - minValue) + minValue;
		}
	}
}
