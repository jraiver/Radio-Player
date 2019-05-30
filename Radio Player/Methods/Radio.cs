namespace radio
{
	internal class Radio
	{
		public class RadioToSave
		{
			private string title { get; set; }
			private string url { get; set; }
			private string picture { get; set; }

			public string Name
			{
				get => title;
				set => title = value;
			}

			public string Url
			{
				get => url;
				set => url = value;
			}

			public string Picture
			{
				get => picture;
				set => picture = value;
			}
		}

		public class RadioFull : RadioToSave
		{
			private string[] stream { get; set; }
			private string[] quality { get; set; }
			private string janre { get; set; }
			private string whoPlay { get; set; }
			private string site { get; set; }

			public string Site
			{
				get => site;
				set => site = value;
			}

			public string Janre
			{
				get => janre;
				set => janre = value;
			}

			public string WhoPlay
			{
				get => whoPlay;
				set => whoPlay = value;
			}

			public string[] Stream
			{
				get => stream;
				set => stream = value;
			}

			public string[] QualityStream
			{
				get => quality;
				set => quality = value;
			}
		}
	}
}