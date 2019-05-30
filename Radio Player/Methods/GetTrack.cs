using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace radio
{
	internal class GetTrack
	{
		public static string Get(string url)
		{
			try
			{
				var client = new WebClient();
				client.Encoding = Encoding.UTF8;
				var track = client.DownloadString($"https://www.radiobells.com/whoplay/{url}.json");
				track = Regex.Replace(track, @"\\u([\da-f]{4})",
					m => ((char) Convert.ToInt32(m.Groups[1].Value, 16)).ToString());

				var pattern = "\"(.*?)\"";
				var regex = new Regex(pattern);
				var match = regex.Match(track);

				var matchCut = "";

				for (var i = 0; match.Success; i++)
				{
					matchCut = match.Value;
					matchCut = matchCut.Trim('"');

					switch (i)
					{
						case 1:
							track = matchCut;
							break;

						case 3:
							track = $"{track} - {matchCut}";
							break;
					}

					match = match.NextMatch();
				}

				if (track.IndexOf("<html>") > -1)
					throw new Exception("error 1");
				return track;
			}
			catch (Exception ex)
			{
				if (ex is WebException) return (string) Application.Current.TryFindResource("CantTrack");

				if (ex.Message.IndexOf("error 1") == 1)
					return (string) Application.Current.TryFindResource("CantTrack");
				return null;
			}
		}
	}
}