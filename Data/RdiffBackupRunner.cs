﻿using System;
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
			var output = await runCommandAsync("rdiff-backup", $"--list-increment-sizes --no-acls {backupDirectoryPath}");
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
