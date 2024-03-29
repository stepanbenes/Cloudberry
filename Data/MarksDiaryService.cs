using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Cloudberry.Data
{
	public class Day : IComparable<Day>, IEquatable<Day>
	{
		public Day(int day, int? weight, string? text, IReadOnlyList<string> images, IReadOnlyList<string> videos)
			=> (DayNumber, Weight, Text, Images, Videos) = (day, weight, text, images, videos);

		public int DayNumber { get; } // 0-based
		public int? Weight { get; } // g
		public string? Text { get; }

		public IReadOnlyList<string> Images { get; }

		public IReadOnlyList<string> Videos { get; }

		public DateTime Date => BirthDate.AddDays(DayNumber);

		public int CompareTo(Day? other) => this.DayNumber.CompareTo(other?.DayNumber ?? -1);

		public override bool Equals(object? obj) => this.Equals(obj as Day);

		public bool Equals(Day? other) => ReferenceEquals(this, other) || (other is object && this.DayNumber == other.DayNumber);

		public override int GetHashCode() => DayNumber.GetHashCode();

		public static bool operator ==(Day? left, Day? right)
		{
			if (left is null)
			{
				return right is null;
			}

			return left.Equals(right);
		}

		public static bool operator !=(Day? left, Day? right) => !(left == right);

		public static bool operator <(Day? left, Day? right) => left is null ? right is object : left.CompareTo(right) < 0;

		public static bool operator <=(Day? left, Day? right) => left is null || left.CompareTo(right) <= 0;

		public static bool operator >(Day? left, Day? right) => left is object && left.CompareTo(right) > 0;

		public static bool operator >=(Day? left, Day? right) => left is null ? right is null : left.CompareTo(right) >= 0;


		public static DateTime BirthDate => new DateTime(2020, 8, 27, 10, 33, 0);

		public static string CalculateWeek(int dayNumber)
		{
			int baseDay = 30 * 7 + 4;
			int currentDay = baseDay + dayNumber;
			int currentWeek = currentDay / 7;
			int weekDay = currentDay % 7;
			return $"{currentWeek}+{weekDay}";
		}

		public static int WhatDayNumberIsToday() => (DateTime.Now.Date - BirthDate.Date).Days;
	}

	public class MarksDiaryService
	{
		public static readonly string SourceDirectoryPath = @"/mnt/sidlo_data/data/marek/denik";

		public async IAsyncEnumerable<Day> GetMarkDaysAsync()
		{
			foreach (var directorypath in Directory.EnumerateDirectories(SourceDirectoryPath))
			{
				var day = await parseDayAsync(directorypath);
				if (day is object)
				{
					yield return day;
				}
			}
		}

		private static async Task<Day?> parseDayAsync(string directoryPath)
		{
			string directoryName = Path.GetFileName(directoryPath);
			if (!directoryName.StartsWith("den_"))
				return null;
			string[] tokens = directoryName.Split('_');
			if (tokens.Length <= 1)
				return null;
			string dayString = tokens[1];
			if (int.TryParse(dayString, out int day))
			{
				string? text = null;
				int? weight = null;
				var images = new List<string>();
				var videos = new List<string>();

				foreach (var filepath in Directory.EnumerateFiles(directoryPath))
				{
					string extension = Path.GetExtension(filepath);
					switch (extension.ToLowerInvariant())
					{
						case ".txt":
							{
								text = await File.ReadAllTextAsync(filepath);
								Match match = Regex.Match(text, @"^\d+");
								if (match.Success)
								{
    								weight = int.Parse(match.Value);
								}
							}
							break;
						case ".jpg":
						case ".jpeg":
						case ".png":
						case ".gif":
							{
								string imagePath = Path.Combine("denik", directoryName, Path.GetFileName(filepath));
								images.Add(imagePath);
							}
							break;
						case ".mov":
						case ".mp4":
							{
								string videoPath = Path.Combine("denik", directoryName, Path.GetFileName(filepath));
								videos.Add(videoPath);
							}
							break;
					}
				}

				return new Day(day, weight, text, images, videos);
			}
			return null;
		}

		public async Task<Day> UpdateMarkDayAsync(int dayNumber, string text)
		{
			string directoryPath = Path.Combine(SourceDirectoryPath, $"den_{dayNumber}");
			_ = Directory.CreateDirectory(directoryPath);
			string filepath = Path.Combine(directoryPath, $"{Day.CalculateWeek(dayNumber)}.txt");
			await File.WriteAllTextAsync(filepath, text);
			return await parseDayAsync(directoryPath) ?? throw new InvalidOperationException();
		}
	}
}
