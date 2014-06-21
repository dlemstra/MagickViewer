//=================================================================================================
// Copyright 2013-2014 Dirk Lemstra <https://magickviewer.codeplex.com/>
//
// Licensed under the ImageMagick License (the "License"); you may not use this file except in 
// compliance with the License. You may obtain a copy of the License at
//
//   http://www.imagemagick.org/script/license.php
//
// Unless required by applicable law or agreed to in writing, software distributed under the
// License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either
// express or implied. See the License for the specific language governing permissions and
// limitations under the License.
//=================================================================================================

using System;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using ImageMagick;

namespace MagickViewer
{
	///=============================================================================================
	public partial class MainWindow : Window
	{
		//===========================================================================================
		private bool _CanDrop;
		private ImageManager _ImageManager;
		private String _Title;
		//===========================================================================================
		private static bool CanDropFile(DragEventArgs arguments)
		{
			string[] fileNames = arguments.Data.GetData(DataFormats.FileDrop, true) as string[];
			return fileNames.Length == 1 && ImageManager.IsSupported(fileNames[0]);
		}
		//===========================================================================================
		private bool CheckCanDrop(DragEventArgs arguments)
		{
			if (_CanDrop)
				return true;

			arguments.Effects = DragDropEffects.None;
			arguments.Handled = true;

			return false;
		}
		//===========================================================================================
		private void HideMenu()
		{
			_Menu.BeginAnimation(FrameworkElement.OpacityProperty, null);
			_Menu.BeginAnimation(FrameworkElement.VisibilityProperty, null);
		}
		//===========================================================================================
		private void Initialize()
		{
			SetTitle();
		}
		//===========================================================================================
		private void InitializeImageManager()
		{
			_ImageManager = new ImageManager(Dispatcher.CurrentDispatcher);
			_ImageManager.Loaded += ImageManager_Loaded;
			_ImageManager.Loading += ImageManager_Loading;
		}
		//===========================================================================================
		private void ImageManager_Loaded(object sender, EventArgs arguments)
		{
			_ImageViewer.HideLoadingImage();

			if (_ImageManager.Images.Count == 0)
				SetTitle(null);
			else
			{
				_ImageViewer.ImageSource = _ImageManager.Images[0].ToBitmapSource();
				SetTitle(_ImageManager.FileName);
			}
		}
		//===========================================================================================
		private void ImageManager_Loading(object sender, EventArgs arguments)
		{
			_Logo.Visibility = Visibility.Collapsed;
			_ImageViewer.ShowLoadingImage();
		}
		//===========================================================================================
		private void OnCommandClose(object sender, RoutedEventArgs arguments)
		{
			Close();
		}
		//===========================================================================================
		private void OnCommandOpen(object sender, RoutedEventArgs arguments)
		{
			HideMenu();
			_ImageManager.ShowOpenDialog();
		}
		//===========================================================================================
		private void OnCommandSave(object sender, RoutedEventArgs arguments)
		{
			HideMenu();
			_ImageManager.ShowSaveDialog();
		}
		//===========================================================================================
		private void OnCommandStop(object sender, RoutedEventArgs arguments)
		{
			if (WindowState == WindowState.Maximized)
				WindowState = WindowState.Normal;
			else
				Close();
		}
		//===========================================================================================
		private void OnMenuClick(object sender, RoutedEventArgs arguments)
		{
			string resourceKey = _Menu.Visibility == Visibility.Hidden ? "FadeIn" : "FadeOut";
			_Menu.BeginStoryboard(resourceKey);
		}
		//===========================================================================================
		private void OnDragEnter(object sender, DragEventArgs arguments)
		{
			_CanDrop = false;

			if (arguments.Data.GetDataPresent(DataFormats.FileDrop))
				_CanDrop = CanDropFile(arguments);

			CheckCanDrop(arguments);
		}
		//===========================================================================================
		private void OnDragOver(object sender, DragEventArgs arguments)
		{
			CheckCanDrop(arguments);
		}
		//===========================================================================================
		private void OnDrop(object sender, DragEventArgs arguments)
		{
			if (arguments.Data.GetDataPresent(DataFormats.FileDrop))
				OnDropFile(arguments);
		}
		//===========================================================================================
		private void OnDropFile(DragEventArgs arguments)
		{
			string[] fileNames = arguments.Data.GetData(DataFormats.FileDrop, true) as string[];
			_ImageManager.Load(fileNames[0]);
		}
		//===========================================================================================
		private void SetTitle()
		{
			Assembly assembly = typeof(MagickNET).Assembly;
			AssemblyFileVersionAttribute version = (AssemblyFileVersionAttribute)assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false)[0];
			_Title = "MagickViewer " + version.Version;
			SetTitle(null);
		}
		//===========================================================================================
		private void SetTitle(string fileName)
		{
			string title = !string.IsNullOrEmpty(fileName) ? fileName : _Title;

			Title = title;
			_TopBar.Text = title;
		}
		//===========================================================================================
		private void TopBar_MouseDown(object sender, MouseButtonEventArgs arguments)
		{
			if (arguments.ChangedButton != MouseButton.Left)
				return;

			if (arguments.ClickCount == 2)
				WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
			else
				DragMove();
		}
		//===========================================================================================
		public MainWindow()
		{
			InitializeComponent();
			InitializeImageManager();
			Initialize();
		}
		//===========================================================================================
	}
	//==============================================================================================
}
