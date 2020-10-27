﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Text;

namespace Cloudberry.Data
{
	public abstract record FileSystemEntry(string Name);

	record FileEntry(string Name, long Size) : FileSystemEntry(Name);

	record DirectoryEntry(string Name) : FileSystemEntry(Name);

	public class DataBackupService
	{
		public static readonly string DataBaseDirectoryPath = @"/mnt/sidlo_data/data";
		public static readonly string BackupBaseDirectoryPath = @"/mnt/sidlo_backup/data";
		
		private readonly RdiffBackupRunner rdiffBackupRunner;

		public DataBackupService(RdiffBackupRunner rdiffBackupRunner) => this.rdiffBackupRunner = rdiffBackupRunner;

		public IEnumerable<FileSystemEntry> GetFileSystemEntries(string relativePath)
		{
			string directoryPath = Path.Combine(DataBaseDirectoryPath, relativePath);

			foreach (string directory in Directory.EnumerateDirectories(directoryPath))
			{
				yield return new DirectoryEntry(Path.GetFileName(directory));
			}

			foreach (string file in Directory.EnumerateFiles(directoryPath))
			{
				var fileInfo = new FileInfo(file);
				yield return new FileEntry(Path.GetFileName(file), Size: fileInfo.Length);
			}
		}

		public Task<IReadOnlyList<DateTime>> GetBackupDateTimes(string relativePath) => rdiffBackupRunner.ListIncrementSizesAsync(Path.Combine(BackupBaseDirectoryPath, relativePath));

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
	}
}