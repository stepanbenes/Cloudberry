using Iot.Device.CpuTemperature;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cloudberry.Data
{
	public class CpuTemperatureService
	{
		public double GetTemperatureInCelsius()
		{
			var cpuTempProvider = new CpuTemperature();
			var cpuTemp = cpuTempProvider.Temperature;
			return cpuTemp.Celsius;
		}
	}
}
