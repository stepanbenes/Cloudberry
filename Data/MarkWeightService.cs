using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace Cloudberry.Data
{
    public class MarkWeightService
	{
		public async IAsyncEnumerable<(int day, int weight)> GetMarkWeightPointsAsync()
		{
			foreach (var directorypath in Directory.EnumerateDirectories(@"/mnt/sidlo_data/data/marek/denik"))
			{
				string? textFilePath = Directory.EnumerateFiles(directorypath, "*.txt").SingleOrDefault();
				if (textFilePath is object)
				{
					string dayString = Path.GetFileNameWithoutExtension(directorypath).Split('_')[1];
					if (int.TryParse(dayString, out int day))
					{
						string fileContent = await File.ReadAllTextAsync(textFilePath);

						if (int.TryParse(fileContent[..fileContent.IndexOf(' ')], out int weight))
						{
							yield return (day, weight);
						}
					}
				}
			}
		}
	}
}
