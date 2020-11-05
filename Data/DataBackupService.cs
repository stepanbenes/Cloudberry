using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Diagnostics;

namespace Cloudberry.Data
{
	public abstract record FileSystemEntry(string Name);

	record FileEntry(string Name, DataSize Size) : FileSystemEntry(Name);

	record DirectoryEntry(string Name) : FileSystemEntry(Name);

	public record DataSize(double Value, string Units);

	public record BackupIncrementInfo(DateTime DateTime, DataSize IncrementSize, DataSize CummulativeSize);

	public class DataBackupService
	{
		//public static readonly string DataBaseDirectoryPath = @"/mnt/sidlo_data/data";
		public static readonly string BackupBaseDirectoryPath = @"/mnt/sidlo_backup/data";

		private static readonly HashSet<string> excludedSubdirectories = new() { "rdiff-backup-data" };

		private readonly ILogger<DataBackupService> logger;

		public DataBackupService(ILoggerFactory loggerFactory) => this.logger = loggerFactory.CreateLogger<DataBackupService>();

		public async Task RunBackupNowAsync()
		{
			var output = await runCommandAsync("/home/pi/projects/memorykeeper/backup.sh");

			logger.LogInformation(output);
		}

		public IEnumerable<FileSystemEntry> GetFileSystemEntries(string relativePath)
		{
			string baseDirectoryPath = Path.Combine(BackupBaseDirectoryPath, relativePath);

			foreach (string subdirectoryPath in Directory.EnumerateDirectories(baseDirectoryPath))
			{
				string subdirectoryName = Path.GetFileName(subdirectoryPath);
				if (!excludedSubdirectories.Contains(subdirectoryName))
				{
					yield return new DirectoryEntry(subdirectoryName);
				}
			}

			foreach (string file in Directory.EnumerateFiles(baseDirectoryPath))
			{
				var fileInfo = new FileInfo(file);
				yield return new FileEntry(Path.GetFileName(file), Size: new(fileInfo.Length, "bytes"));
			}
		}

		public async Task<IReadOnlyList<BackupIncrementInfo>> GetBackupIncrementsAsync(string relativePath)
		{
			string backupDirectoryPath = Path.Combine(BackupBaseDirectoryPath, relativePath);

			var output = await runCommandAsync("rdiff-backup", $"--list-increment-sizes --no-acls {backupDirectoryPath}");

			logger.LogInformation(output);

			//string dateTimeFormat = "ddd MMM d HH:mm:ss yyyy";

			using StringReader stringReader = new(output);
			List<BackupIncrementInfo> result = new();
			int lineIndex = 0;
			while (await stringReader.ReadLineAsync() is string line)
			{
				if (lineIndex >= 2) // skip first two lines
				{
					//DateTime dateTime = DateTime.ParseExact(line[..dateTimeFormat.Length], dateTimeFormat, CultureInfo.InvariantCulture);
					string[] tokens = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
					if (tokens.Length < 9)
						throw new InvalidOperationException("backup increment info has unexpected format");

					DateTime? dateTime = tryParseDateTime(tokens.Take(5));

					result.Add(new(dateTime ?? throw new InvalidOperationException("datetime could not be parsed"),
						IncrementSize: new DataSize(parseDoubleNumber(tokens[5]) ?? double.NaN, tokens[6]),
						CummulativeSize: new DataSize(parseDoubleNumber(tokens[7]) ?? double.NaN, tokens[8])
						));
				}
				lineIndex += 1;
			}
			return result;

			static DateTime? tryParseDateTime(IEnumerable<string> parts)
			{
				DateTime result;
				if (DateTime.TryParseExact(string.Join(' ', parts), format: "ddd MMM dd HH:mm:ss yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out result))
					return result;

				if (DateTime.TryParseExact(string.Join(' ', parts), format: "ddd MMM d HH:mm:ss yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out result))
					return result;

				return null;
			}
		}

		public IEnumerable<(string pathSegment, string combinedPath)> DirectorySplit(string? path)
		{
			string[] segments = path?.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();

			StringBuilder combinedPath = new StringBuilder();
			foreach (string segment in segments)
			{
				combinedPath.Append(Path.DirectorySeparatorChar);
				combinedPath.Append(segment);
				yield return (segment, combinedPath.ToString());
			}

			//DirectoryInfo? dir = (path != null) ? new(path) : null;
			//while (dir != null)
			//{
			//	yield return (pathSegment: dir.Name, combinedPath: dir.FullName);
			//	dir = dir.Parent;
			//}
		}

		private static Task<string> runCommandAsync(string command, string args = "")
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

		private static double? parseDoubleNumber(string text) => double.TryParse(text, out var value) ? value : null;

		private static long? parseByteUnitsFactor(string text) => text switch
		{
			"bytes" => 1L,
			"KB" => 1024L,
			"MB" => 1024L * 1024,
			"GB" => 1024L * 1024 * 1024,
			"TB" => 1024L * 1024 * 1024 * 1024,
			_ => null
		};
	}
}
