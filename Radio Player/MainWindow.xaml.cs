using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace radio
{
	public partial class MainWindow : MetroWindow
	{
		private MediaPlayer MusicPlayer = new MediaPlayer();
		private Radio.RadioFull[] FavoriteRadio = new Radio.RadioFull[0];
		private Radio.RadioFull[] AllRadio = new Radio.RadioFull[0];
		private Thread Refresher;

		public MainWindow()
		{
			InitializeComponent();
			Init();
		}

		private void Init()
		{
			var buttonList = new List<MenuItem>();
			App.LanguageChanged += LanguageChanged;
			CultureInfo currLang = App.Language;
			ChangeLang.Items.Clear();
			foreach (var lang in App.Languages)
			{
				MenuItem menuLang = new MenuItem()
				{
					Header = lang.DisplayName,
					Tag = lang,
					IsChecked = lang.Equals(currLang)
				};
				menuLang.Click += ChangeLanguageClick;
				buttonList.Add(menuLang);
			}
			ChangeLang.ItemsSource = buttonList;

			FavoriteRadio = Favorite.Read();
			Refresher = new Thread(RefreshArtist);
		}

		private void LanguageChanged(Object sender, EventArgs e)
		{
			CultureInfo currLang = App.Language;

			foreach (MenuItem i in ChangeLang.Items)
			{
				CultureInfo ci = i.Tag as CultureInfo;
				i.IsChecked = ci != null && ci.Equals(currLang);
			}
		}

		private void ChangeLanguageClick(Object sender, EventArgs e)
		{
			MenuItem mi = sender as MenuItem;
			if (mi != null)
			{
				CultureInfo lang = mi.Tag as CultureInfo;
				if (lang != null)
					App.Language = lang;
			}
		}

		private StackPanel CreateRadioButton(Radio.RadioFull radio, bool shadow = true)
		{
			StackPanel stack = new StackPanel
			{
				Width = 105,
				Height = 125,
				DataContext = radio,
				Background = new SolidColorBrush(Colors.White),
				Margin = new Thickness(3),
				Cursor = Cursors.Hand
			};

			Label NameRadiostation = new Label
			{
				Content = radio.Name,
				Uid = radio.Name,
				Width = 105,
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Bottom,
				HorizontalContentAlignment = HorizontalAlignment.Center,
				FontSize = 14
			};

			Grid LabelGrid = new Grid
			{
				Width = 105
			};

			LabelGrid.Children.Add(NameRadiostation);

			if (Favorite.Coincidence(FavoriteRadio, radio.Name) && shadow)
				LabelGrid.Background = (Brush)this.TryFindResource("BottomGradientRadio");

			Image cover = new Image
			{
				Margin = new Thickness(0, 7, 0, 0),
				Width = 90,
				Height = 90,
				Source = new BitmapImage(new Uri(radio.Picture)),
				Uid = radio.Name
			};

			cover.PreviewMouseLeftButtonDown += Grid_PreviewMouseLeftButtonDown;
			LabelGrid.MouseEnter += LabelGrid_MouseEnter;
			LabelGrid.MouseLeave += LabelGrid_MouseLeave;
			LabelGrid.PreviewMouseLeftButtonDown += LabelGrid_PreviewMouseLeftButtonDown;

			stack.Children.Add(cover);
			stack.Children.Add(LabelGrid);

			stack.MouseLeave += Grid_MouseLeave;
			stack.MouseEnter += Grid_MouseEnter;

			return stack;
		}

		private void LabelGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			WrapPanel panel;
			Radio.RadioFull radio = (GetData.GetParentStack((sender as Grid)) as StackPanel).DataContext as Radio.RadioFull;
			Label label = (sender as Grid).Children[0] as Label;

			if (!SearchBox.IsVisible)
				panel = Search_WrapPanel;
			else
				panel = GetData.GetWrapPanel(MainTab.SelectedItem as TabItem);

			//delete from favorite
			if (String.Compare(label.Content.ToString(), (String)this.TryFindResource("Delete")) == 0)
			{
				if (PlayGrid.DataContext != null && String.Compare(radio.Name, (PlayGrid.DataContext as Radio.RadioFull).Name) == 0)
				{
					FavoriteRadioButton.Uid = "0";
					FavoriteRadioButton.Source = new BitmapImage(new Uri("pack://application:,,/Res/star.png"));
				}
				RemoveFromFavoritePanel(radio.Name);
				label.Content = (String)this.TryFindResource("ToFavorite");
			}
			//add to favorite
			else
			{
				if (PlayGrid.DataContext != null && String.Compare(radio.Name, (PlayGrid.DataContext as Radio.RadioFull).Name) == 0)
				{
					FavoriteRadioButton.Uid = "1";
					FavoriteRadioButton.Source = new BitmapImage(new Uri("pack://application:,,/Res/fav.png"));
				}

				AddToFavoritePanel(radio);
				label.Content = (String)this.TryFindResource("Delete");
			}
		}

		private void LabelGrid_MouseLeave(object sender, MouseEventArgs e)
		{
			Radio.RadioFull radio = (GetData.GetParentStack(sender as Grid) as StackPanel).DataContext as Radio.RadioFull;
			WrapPanel panel = GetData.GetWrapPanel(sender);
			Label label = (sender as Grid).Children[0] as Label;

			if (panel != null && radio != null)
			{
				if (panel.Name.IndexOf("Favorite_Radio_Wrap") == -1 && Favorite.Coincidence(FavoriteRadio, radio.Name))
					(sender as Grid).Background = (Brush)this.TryFindResource("BottomGradientRadio");
				else
					(sender as Grid).Background = null;

				label.Content = radio.Name;
			}
		}

		private void LabelGrid_MouseEnter(object sender, MouseEventArgs e)
		{
			Radio.RadioFull radio = (GetData.GetParentStack(sender as Grid) as StackPanel).DataContext as Radio.RadioFull;
			WrapPanel panel = GetData.GetWrapPanel(sender);
			Label label = (sender as Grid).Children[0] as Label;

			(sender as Grid).Background = null;

			if (Favorite.Coincidence(FavoriteRadio, radio.Name))
				label.Content = (String)this.TryFindResource("Delete");
			else
				label.Content = (String)this.TryFindResource("ToFavorite");
		}

		private void Grid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			PlayRadio(sender as Image);
		}

		private void Grid_MouseLeave(object sender, MouseEventArgs e)
		{
			(sender as StackPanel).Background = new SolidColorBrush(Colors.White);
		}

		private void Grid_MouseEnter(object sender, MouseEventArgs e)
		{
			(sender as StackPanel).Background = (Brush)this.TryFindResource("GeneralColorOpacity");
		}

		private void PlayRadio(Image panel)
		{
			Radio.RadioFull station = (GetData.GetParentStack(panel) as StackPanel).DataContext as Radio.RadioFull;

			station = GetStations.GetStream((panel.DataContext as Radio.RadioFull).Url, station);

			if (station != null)
			{
				StationJanre.Content = $"[{ station.Janre}]";
				StationNameLabel.Content = station.Name;

				StationNameLabel.Uid = station.WhoPlay;
				StationCoverImage.ImageSource = new BitmapImage(new Uri(station.Picture));
				StationWhoPlayLabel.Content = GetTrack.Get(station.WhoPlay);
				StationWhoPlayLabel.Uid = station.WhoPlay;

				FirstQualityButton.IsEnabled = true;
				FirstQualityButton.Background = (Brush)this.TryFindResource("GeneralColor");
				FirstQualityButton.Content = station.QualityStream[0];
				FirstQualityButton.Uid = station.Stream[0];

				PlayGrid.DataContext = station;

				SecondQualityButton.IsEnabled = true;
				if (station.QualityStream[1] != null)
				{
					SecondQualityButton.Content = station.QualityStream[1];
					SecondQualityButton.Uid = station.Stream[1];
				}
				else
				{
					SecondQualityButton.Content = "-";
					SecondQualityButton.IsEnabled = false;
				}

				FirstQualityButton.Background = (Brush)this.TryFindResource("GeneralColor");
				SecondQualityButton.Background = new SolidColorBrush(Colors.WhiteSmoke);

				bool found = Favorite.Coincidence(FavoriteRadio, station.Name);

				if (found)
				{
					FavoriteRadioButton.Source = new BitmapImage(new Uri("pack://application:,,/Resources/Icons/pl_start_fav.png"));
					FavoriteRadioButton.Uid = "1";
				}
				else
				{
					FavoriteRadioButton.Source = new BitmapImage(new Uri("pack://application:,,/Resources/Icons/pl_star.png"));
					FavoriteRadioButton.Uid = "0";
				}

				PlayMusic(station.Stream[0]);
			}
		}

		private void Image_MouseEnter(object sender, MouseEventArgs e)
		{
			(sender as Image).Opacity = 100;
		}

		private void Image_MouseLeave(object sender, MouseEventArgs e)
		{
			(sender as Image).Opacity = 0.6;
		}

		private void TabControl_Loaded(object sender, RoutedEventArgs e)
		{
			(sender as TabControl).SelectionChanged += MainWindow_SelectionChanged;
			ParseStations(All_Radio_Wrap, "https://www.radiobells.com/all/", false, true);
		}

		private void ParseStations(WrapPanel panel, string url, bool refresh = false, bool start = false)
		{
			if (MainTab.SelectedIndex == 0 && Favorite_Radio_Wrap.Children.Count > 0)
				return;
			else if (MainTab.SelectedIndex == 0 && Favorite_Radio_Wrap.Children.Count == 0)
				PrintRadio(Favorite_Radio_Wrap, FavoriteRadio);
			else if (panel.Children.Count == 0)
			{
				Task<Radio.RadioFull[]> task = new Task<Radio.RadioFull[]>(() => GetStations.Get(url));
				task.Start();
				task.Wait();

				if (All_Radio_Wrap.Children.Count == 0 && MainTab.SelectedIndex == 1)
				{
					AllRadio = task.Result;
					PrintRadio(panel, AllRadio);
				}
				else
					PrintRadio(panel, task.Result);
			}
		}

		private void PrintRadio(WrapPanel panel, Radio.RadioFull[] stations)
		{
			bool shadow = true;
			if (panel.Name.IndexOf("Favorite_Radio_Wrap") > -1)
				shadow = false;

			for (int i = 0; i < stations.Length; i++)
				panel.Children.Add(CreateRadioButton(stations[i], shadow));
		}

		private void MainWindow_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			GetRadiostationsJanre((sender as TabControl).SelectedItem as TabItem);
		}

		private void GetRadiostationsJanre(TabItem tab)
		{
			ParseStations(GetData.GetWrapPanel(tab), $"https://www.radiobells.com/{tab.Uid}/");
		}

		private void SecondQualityButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (SecondQualityButton.Background != (Brush)this.TryFindResource("GeneralColor"))
			{
				FirstQualityButton.Background = new SolidColorBrush(Colors.WhiteSmoke);
				SecondQualityButton.Background = (Brush)this.TryFindResource("GeneralColor");
				PlayMusic(SecondQualityButton.Uid);
			}
		}

		private void FirstQualityButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (FirstQualityButton.Background != (Brush)this.TryFindResource("GeneralColor"))
			{
				SecondQualityButton.Background = new SolidColorBrush(Colors.WhiteSmoke);
				FirstQualityButton.Background = (Brush)this.TryFindResource("GeneralColor");
				PlayMusic(FirstQualityButton.Uid);
			}
		}

		private void Slider_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
		{
			if (e.Delta > 0)
				VolumeSlider.Value++;
			else if (e.Delta < 0)
				VolumeSlider.Value--;
		}

		private void ShuffleStation_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			ChoiseRadio(true, true);
		}

		private void NextRadio_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			ChoiseRadio(true);
		}

		private void ChoiseRadio(bool next, bool shuffle = false)
		{
			WrapPanel panel;

			if (SearchBox.IsVisible)
				panel = Search_WrapPanel;
			else
				panel = GetData.GetWrapPanel((MainTab.SelectedItem as TabItem));

			if (panel.Children.Count == 0)
				return;

			int id = 0;

			if (shuffle)
			{
				Random rnd = new Random();
				id = rnd.Next(0, panel.Children.Count - 1);
			}
			else
			{
				id = GetData.GetPositionElement(panel, (PlayGrid.DataContext as Radio.RadioFull).Name);
				if (id == -1)
					id = 0;
				else if (next)
				{
					if (id + 1 == panel.Children.Count)
						id = 0;
					else
						id++;
				}
				else if (!next)
				{
					if (id == 0)
						id = panel.Children.Count - 1;
					else id--;
				}
			}

			PlayRadio(GetData.GetImage(panel.Children[id] as StackPanel));
		}

		private void PlayPause_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			PlayPauseFunc();
		}

		private void PlayPauseFunc()
		{
			string path = "pack://application:,,/Resources/Icons/pl_pause.png";
			BitmapImage bitmap = new BitmapImage(new Uri(path));

			if (MusicPlayer.Source != null)
			{
				if (String.Compare(PlayPause.Source.ToString(), path) != 0)
				{
					PlayPause.Source = bitmap;
					MusicPlayer.Play();
				}
				else
				{
					bitmap = new BitmapImage(new Uri("pack://application:,,/Resources/Icons/pl_play.png"));
					PlayPause.Source = bitmap;
					MusicPlayer.Pause();
				}
			}
		}

		private void PreviousRadio_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			ChoiseRadio(false);
		}

		private void StationWhoPlayLabel_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (String.Compare((sender as Label).Uid, (String)this.TryFindResource("CantTrack")) != 0 &&
				String.Compare((sender as Label).Uid, (String)this.TryFindResource("SelectStation")) != 0)
			{
				System.Diagnostics.Process.Start($"https://vk.com/audio?q={StationWhoPlayLabel.Uid.ToString()}");
			}
		}

		private void CoverStation_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if ((PlayGrid.DataContext as Radio.RadioFull).Site != null)
				System.Diagnostics.Process.Start((PlayGrid.DataContext as Radio.RadioFull).Site);
		}

		private void PlayMusic(string stream)
		{
			try
			{
				stream = stream.Replace("https://", "http://");
				BitmapImage bitmap = new BitmapImage(new Uri("pack://application:,,/Resources/Icons/pl_pause.png"));
				PlayPause.Source = bitmap;

				Refresher.Abort();

				Refresher = new Thread(RefreshArtist);
				Refresher.IsBackground = true;
				Refresher.Start(StationWhoPlayLabel.Uid);

				MusicPlayer.Open(new Uri(stream, UriKind.Absolute));
				MusicPlayer.Play();
				RefreshStream.IsEnabled = true;
				FavoriteRadioButton.IsEnabled = true;
			}
			catch (Exception ex)
			{
			}
		}

		private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (VolumeSlider.Value == 0)
				MusicPlayer.Volume = 0;
			else
				MusicPlayer.Volume = VolumeSlider.Value / 10;
		}

		private void StationWhoPlayLabel_MouseEnter(object sender, MouseEventArgs e)
		{
			StationWhoPlayLabel.Uid = StationWhoPlayLabel.Content.ToString();
			StationWhoPlayLabel.Foreground = (Brush)this.TryFindResource("GeneralColor");
			StationWhoPlayLabel.Content = (String)this.TryFindResource("SearchVK");
		}

		private void StationWhoPlayLabel_MouseLeave(object sender, MouseEventArgs e)
		{
			StationWhoPlayLabel.Content = StationWhoPlayLabel.Uid;
			StationWhoPlayLabel.Foreground = new SolidColorBrush(Colors.Black);
		}

		private void RefreshArtist(object id)
		{
			string track = "";

			while (true)
			{
				track = GetTrack.Get(id as string);

				Dispatcher.Invoke(() => StationWhoPlayLabel.Content = track);

				Thread.Sleep(5000);
			}
		}

		private void RefreshStream_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			MusicPlayer.Close();

			if (FirstQualityButton.Background == (Brush)this.TryFindResource("GeneralColor"))
				PlayMusic(FirstQualityButton.Uid);
			else
				PlayMusic(SecondQualityButton.Uid);
		}

		private void OpenSearchPanel(object sender, MouseButtonEventArgs e)
		{
			if (!SearchBox.IsVisible)
			{
				SearchBox.Visibility = Visibility.Visible;
				(MainTab.Effect as System.Windows.Media.Effects.BlurEffect).Radius = 6;
				MainTab.IsEnabled = false;
				SearchTextBox.Focus();
			}
			else if (SearchBox.IsVisible)
			{
				SearchBox.Visibility = Visibility.Hidden;
				(MainTab.Effect as System.Windows.Media.Effects.BlurEffect).Radius = 0;
				MainTab.IsEnabled = true;
				SearchTextBox.Text = "";
			}
		}

		private void AllRadioGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (SearchBox.IsVisible)
			{
				SearchBox.Visibility = Visibility.Hidden;
				(MainTab.Effect as System.Windows.Media.Effects.BlurEffect).Radius = 0;
				MainTab.IsEnabled = true;
				SearchTextBox.Text = "";
			}
		}

		private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			Search_WrapPanel.Children.Clear();

			bool found = false;

			if (SearchTextBox.Text.Length > 0)
			{
				Radio.RadioFull[] temp = AllRadio;

				for (int i = 0; i < temp.Length; i++)
				{
					found = false;
					if (temp[i].Name.ToLower().IndexOf(SearchTextBox.Text.ToLower()) > -1)
					{
						for (int q = 0; q < Search_WrapPanel.Children.Count; q++)
							if (String.Compare(temp[i].Name.ToLower(), GetData.GetName(Search_WrapPanel.Children[q] as StackPanel).ToLower()) == 0)
								found = true;
						if (!found)
							Search_WrapPanel.Children.Add(CreateRadioButton(temp[i]));
					}
				}
			}
		}

		private void FavoriteRadioButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			Radio.RadioFull data = PlayGrid.DataContext as Radio.RadioFull;

			if (FavoriteRadioButton.Uid.IndexOf("1") > -1)
			{
				RemoveFromFavoritePanel(data.Name);
				FavoriteRadioButton.Uid = "0";
				FavoriteRadioButton.Source = new BitmapImage(new Uri("pack://application:,,/Resources/Icons/pl_star.png"));
			}
			else
			{
				AddToFavoritePanel(data);
				FavoriteRadioButton.Uid = "1";
				FavoriteRadioButton.Source = new BitmapImage(new Uri("pack://application:,,/Resources/Icons/pl_start_fav.png"));
			}
		}

		private void AddToFavoritePanel(Radio.RadioFull radio)
		{
			FavoriteRadio = Favorite.Add(FavoriteRadio, radio);
			Favorite_Radio_Wrap.Children.Add(CreateRadioButton(radio, false));
		}

		private void RemoveFromFavoritePanel(string name)
		{
			int ID = GetData.GetPositionElement(Favorite_Radio_Wrap, name);
			if (ID > -1)
			{
				FavoriteRadio = Favorite.Remove(FavoriteRadio, ID);
				Favorite_Radio_Wrap.Children.RemoveAt(ID);
			}
		}
	}
}