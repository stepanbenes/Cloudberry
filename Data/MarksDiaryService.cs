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
		public Day(int day, int? weight, IReadOnlyList<string> images) => (DayNumber, Weight, Images) = (day, weight, images);

		public int DayNumber { get; } // 0-based
		public int? Weight { get; } // g
		public string? Note { get; set; }

		public IReadOnlyList<string> Images { get; }

		public DateTime Date => new DateTime(2020, 8, 27, 10, 33, 0).AddDays(DayNumber);

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
				string directoryName = Path.GetFileName(directorypath);
				string[] tokens = directoryName.Split('_');
				if (tokens.Length <= 1)
					continue;
				string dayString = tokens[1];
				if (int.TryParse(dayString, out int day))
				{
					string? text = null;
					int? weight = null;
					var images = new List<string>();

					foreach (var filepath in Directory.EnumerateFiles(directorypath))
					{
						string extension = Path.GetExtension(filepath);
						switch (extension.ToLowerInvariant())
						{
							case ".txt":
								{
									text = await File.ReadAllTextAsync(filepath);
									if (int.TryParse(text[..text.IndexOf(' ')], out int value))
									{
										weight = value;
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
						}
					}

					yield return new Day(day, weight, images) { Note = text };
				}
			}
		}
	}
}
