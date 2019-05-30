using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using Newtonsoft.Json;

namespace radio
{
	internal class Favorite
	{
		private static readonly string path = $@"{Directory.GetCurrentDirectory()}\\Favorite.data";

		public static Radio.RadioFull[] Read()
		{
			try
			{
				var favorites = new Radio.RadioFull[0];

				if (File.Exists(path))
				{
					var FavoriteData = "";

					FavoriteData = File.ReadAllText(path, Encoding.UTF8);

					favorites = JsonConvert.DeserializeObject<Radio.RadioFull[]>(FavoriteData);
				}
				else
				{
					File.Create(path);
					return new Radio.RadioFull[0];
				}

				return favorites;
			}
			catch (Exception ex)
			{
				return new Radio.RadioFull[0];
			}
		}

		private static void Save(Radio.RadioFull[] Favorites)
		{
			if (!File.Exists(path))
				File.Create(path);

			var FavoriteData = JsonConvert.SerializeObject(Favorites,
				Formatting.None,
				new JsonSerializerSettings
				{
					NullValueHandling = NullValueHandling.Ignore
				});

			File.WriteAllText(path, FavoriteData, Encoding.UTF8);
		}

		public static Radio.RadioFull[] Add(Radio.RadioFull[] radios, Radio.RadioFull station)
		{
			Array.Resize(ref radios, radios.Length + 1);
			radios[radios.Length - 1] = station;
			Save(radios);
			return radios;
		}

		public static bool Coincidence(Radio.RadioFull[] radios, string name)
		{
			for (var i = 0; i < radios.Length; i++)
				if (string.Compare(radios[i].Name, name) == 0)
					return true;
			return false;
		}

		public static Radio.RadioFull[] Remove(Radio.RadioFull[] radios, int id)
		{
			if (radios != null)
			{
				for (var i = id; i < radios.Length - 1; i++)
					radios[i] = radios[i + 1];

				Array.Resize(ref radios, radios.Length - 1);

				Save(radios);
				return radios;
			}

			return null;
		}

		public static Radio.RadioFull GetClickedStation(WrapPanel panel, string name)
		{
			for (var i = 0; i < panel.Children.Count; i++)
				if (string.Compare(((panel.Children[i] as StackPanel).DataContext as Radio.RadioFull).Name, name) == 0)
					return (panel.Children[i] as StackPanel).DataContext as Radio.RadioFull;

			return null;
		}
	}
}