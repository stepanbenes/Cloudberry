using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.Xml;

namespace Cloudberry.Data
{
	public class RdiffBackupRunner
	{
		public async Task<IReadOnlyList<DateTime>> ListIncrementSizesAsync(string backupDirectoryPath)
		{
			//var output = await runCommandAsync("rdiff-backup", $"--list-increment-sizes --no-acls {backupDirectoryPath}");

			string output = @"        Time                       Size        Cumulative size
-----------------------------------------------------------------------------
Tue Oct 27 19:05:42 2020         5.55 GB           5.55 GB   (current mirror)
Tue Oct 27 18:45:26 2020        73 bytes           5.55 GB
Tue Oct 27 18:44:17 2020         0 bytes           5.55 GB
Tue Oct 27 18:42:32 2020        73 bytes           5.55 GB
Tue Oct 27 16:34:15 2020        73 bytes           5.55 GB
Tue Oct 27 16:33:33 2020        73 bytes           5.55 GB
Tue Oct 27 16:31:23 2020        70 bytes           5.55 GB
Tue Oct 27 16:28:17 2020         0 bytes           5.55 GB
Tue Oct 27 16:28:00 2020        74 bytes           5.55 GB
Tue Oct 27 16:27:42 2020        71 bytes           5.55 GB
Tue Oct 27 16:27:11 2020         0 bytes           5.55 GB
Tue Oct 27 03:00:01 2020        74 bytes           5.55 GB
Mon Oct 26 03:00:01 2020         0 bytes           5.55 GB
Sun Oct 25 03:00:01 2020        76 bytes           5.55 GB
Sat Oct 24 03:00:01 2020       203 bytes           5.55 GB
Fri Oct 23 03:00:02 2020         0 bytes           5.55 GB
Thu Oct 22 03:00:01 2020         1.03 MB           5.55 GB
Wed Oct 21 03:00:02 2020       148 bytes           5.55 GB
Tue Oct 20 03:00:01 2020         0 bytes           5.55 GB
Mon Oct 19 03:00:02 2020         25.2 MB           5.57 GB
Sun Oct 18 00:31:34 2020         1.36 MB           5.57 GB
Sat Oct 17 03:00:02 2020         0 bytes           5.57 GB
Fri Oct 16 03:00:02 2020         0 bytes           5.57 GB
Thu Oct 15 03:00:02 2020         5.44 KB           5.57 GB
Wed Oct 14 11:52:02 2020         9.36 MB           5.58 GB
Wed Oct 14 03:00:07 2020         0 bytes           5.58 GB
Tue Oct 13 16:14:08 2020       406 bytes           5.58 GB
Tue Oct 13 03:00:02 2020          165 MB           5.74 GB
Mon Oct 12 03:00:01 2020       117 bytes           5.74 GB
Sun Oct 11 14:10:40 2020        76 bytes           5.74 GB
Sun Oct 11 13:53:34 2020        72 bytes           5.74 GB
Sun Oct 11 13:45:47 2020        73 bytes           5.74 GB";

			using StringReader stringReader = new(output);
			List<DateTime> result = new();
			int lineIndex = 0;
			while (await stringReader.ReadLineAsync() is string line)
			{
				if (lineIndex >= 2) // skip first two lines
				{
					string[] tokens = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
					if (tokens.Length == 0)
						throw new InvalidOperationException("Unexpected output format");
					DateTime dateTime = DateTime.Parse(tokens[0]);
					result.Add(dateTime);
				}
				lineIndex += 1;
			}
			return result;
		}

		private Task<string> runCommandAsync(string command, string args)
		{
			var process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = command,
					Arguments = args,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					UseShellExecute = false,
					CreateNoWindow = true,
				},
				EnableRaisingEvents = true
			};

			var tsc = new TaskCompletionSource<string>();

			process.Exited += async (s, e) =>
			{
				if (process.ExitCode == 0)
				{
					tsc.SetResult(await process.StandardOutput.ReadToEndAsync());
				}
				else
				{
					tsc.SetException(new InvalidOperationException(await process.StandardError.ReadToEndAsync()));
				}
			};


			process.Start();

			return tsc.Task;
		}
	}
}
