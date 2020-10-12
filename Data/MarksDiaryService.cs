using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics.CodeAnalysis;

namespace Cloudberry.Data
{
	public class Day : IComparable<Day>, IEquatable<Day>
	{
		public Day(int day, int weight) => (DayNumber, Weight) = (day, weight);

		public int DayNumber { get; } // 0-based
		public int Weight { get; } // g
		public string? Note { get; set; }

		public int CompareTo(Day? other) => this.DayNumber.CompareTo(other?.DayNumber ?? -1);

		public override bool Equals(object? obj) => this.Equals(obj as Day);

		public bool Equals(Day? other) => ReferenceEquals(this, other) || (other is object && this.DayNumber == other.DayNumber);

		public override int GetHashCode() => DayNumber.GetHashCode();

		public static bool operator ==(Day left, Day right)
		{
			if (left is null)
			{
				return right is null;
			}

			return left.Equals(right);
		}

		public static bool operator !=(Day left, Day right) => !(left == right);

		public static bool operator <(Day left, Day right) => left is null ? right is object : left.CompareTo(right) < 0;

		public static bool operator <=(Day left, Day right) => left is null || left.CompareTo(right) <= 0;

		public static bool operator >(Day left, Day right) => left is object && left.CompareTo(right) > 0;

		public static bool operator >=(Day left, Day right) => left is null ? right is null : left.CompareTo(right) >= 0;
	}

	public class MarksDiaryService
	{
		public async IAsyncEnumerable<Day> GetMarkDaysAsync()
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
							yield return new Day(day, weight) { Note = fileContent };
						}
					}
				}
			}
		}
	}
}
