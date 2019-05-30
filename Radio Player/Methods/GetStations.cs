using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace radio
{
	internal class GetStations
	{
		private static readonly int count = 1000;

		public static Radio.RadioFull[] Get(object url)
		{
			var stations = new Radio.RadioFull[0];

			var hw = new HtmlWeb();
			var htmlDoc = hw.Load(url as string);
			var table = htmlDoc.GetElementbyId("catalog_items");
			htmlDoc.LoadHtml(table.InnerHtml);

			var j = 0;

			foreach (var link in htmlDoc.DocumentNode.SelectNodes("//a[@href]"))
			{
				if (link.Attributes.Count < 1 || link.FirstChild == null || link.FirstChild.Attributes.Count < 1)
					break;

				Array.Resize(ref stations, stations.Length + 1);
				stations[j] = new Radio.RadioFull
				{
					Name = link.InnerText,
					Picture = "https://www.radiobells.com" + link.FirstChild.Attributes[0].Value,
					Url = "https://www.radiobells.com" + link.Attributes[0].Value
				};

				j++;
				if (j == count)
					break;
			}

			return stations;
		}

		public static Radio.RadioFull GetStream(string url, Radio.RadioFull station)
		{
			try
			{
				var client = new WebClient();
				client.Encoding = Encoding.UTF8;
				var parse = client.DownloadString(url);

				station.Site = GetSite(parse);

				parse = parse.Substring(parse.IndexOf("playradio"),
					parse.IndexOf("var colors") - parse.IndexOf("playradio"));

				parse = parse.Replace("\n", "");

				var pattern = "\"(.*?)\"";
				var regex = new Regex(pattern);
				var match = regex.Match(parse);

				station.Stream = new string[2];
				station.QualityStream = new string[2];

				var matchCut = "";

				for (var i = 0; match.Success; i++)
				{
					matchCut = match.Value;
					matchCut = matchCut.Trim('"');

					switch (i)
					{
						case 0:
							station.Stream[0] = matchCut;
							break;

						case 1:
							if (match.Value.IndexOf("http://") == -1)
								station.Stream[1] = null;
							else
								station.Stream[1] = matchCut;
							break;

						case 4:
							station.WhoPlay = matchCut;
							break;

						case 6:
							station.QualityStream[0] = matchCut;
							break;

						case 7:
							if (station.Stream[1] == null)
								station.QualityStream[1] = null;
							else
								station.QualityStream[1] = matchCut;
							break;

						case 8:
							station.Janre = matchCut;
							break;
					}

					match = match.NextMatch();
				}

				return station;
			}
			catch (Exception ex)
			{
				return null;
			}
		}

		private static string GetSite(string parse)
		{
			try
			{
				var point = "<span>Сайт:</span>";
				parse = parse.Substring(parse.IndexOf(point) + point.Length, 300);
				point = parse.Substring(parse.IndexOf("<a href=\"") + 9, parse.IndexOf("\" target") - 9);
				point = point.Trim('"');

				return point;
			}
			catch (Exception ex)
			{
				return null;
			}
		}
	}
}