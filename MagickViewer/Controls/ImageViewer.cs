//=================================================================================================
// Copyright 2014-2015 Dirk Lemstra <https://magickviewer.codeplex.com/>
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MagickViewer.Controls
{
	//==============================================================================================
	internal sealed class ImageViewer : ScrollViewer
	{
		//===========================================================================================
		private MouseCapture _Capture;
		private bool _FitToScreen = true;
		//===========================================================================================
		private Image Image
		{
			get
			{
				return (Image)Content;
			}
		}
		//===========================================================================================
		private void CreateImage()
		{
			Image image = new Image();
			image.Style = (Style)FindResource("Image");

			Content = image;
			UpdateImageSize();
		}
		//===========================================================================================
		private void OnLayoutUpdated(object sender, EventArgs e)
		{
			UpdateImageSize();
		}
		//===========================================================================================
		private static void OnImageSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs arguments)
		{
			ImageViewer target = sender as ImageViewer;
			if (target == null)
				return;

			ImageSource source = (ImageSource)arguments.NewValue;
			if (source == null)
				return;

			target.Image.Source = source;
			target.SetImageSize();
		}
		//===========================================================================================
		private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs arguments)
		{
			ScrollViewer target = sender as ScrollViewer;
			if (target == null)
				return;

			_Capture = new MouseCapture(target, arguments);

			if (arguments.ClickCount == 2)
				ToggleFitToScreen();
		}
		//===========================================================================================
		private void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs arguments)
		{
			ScrollViewer target = sender as ScrollViewer;
			if (target == null)
				return;

			target.ReleaseMouseCapture();
			_Capture = null;
		}
		//===========================================================================================
		private void OnPreviewMouseMove(object sender, MouseEventArgs arguments)
		{
			if (_Capture == null)
				return;

			if (arguments.LeftButton != MouseButtonState.Pressed)
				return;

			ScrollViewer target = sender as ScrollViewer;
			if (target == null)
				return;

			Point point = arguments.GetPosition(target);

			double dx = point.X - _Capture.Point.X;
			double dy = point.Y - _Capture.Point.Y;
			if (Math.Abs(dy) > 5 || Math.Abs(dx) > 5)
				target.CaptureMouse();

			target.ScrollToHorizontalOffset(_Capture.HorizontalOffset - dx);
			target.ScrollToVerticalOffset(_Capture.VerticalOffset - dy);
		}
		//===========================================================================================
		private void SetImageSize()
		{
			if (_FitToScreen)
			{
				UpdateImageSize();
			}
			else if (Image.Source != null)
			{
				Image.Width = Image.Source.Width;
				Image.Height = Image.Source.Height;
			}
		}
		//===========================================================================================
		private void ToggleFitToScreen()
		{
			_FitToScreen = !_FitToScreen;
			Image.Stretch = _FitToScreen ? Stretch.Uniform : Stretch.None;
			SetImageSize();
		}
		//===========================================================================================
		private void UpdateImageSize()
		{
			if (!_FitToScreen || Image.Source == null)
				return;

			Image.Width = ActualWidth > Image.Source.Width ? Image.Source.Width : ActualWidth;
			Image.Height = ActualHeight > Image.Source.Height ? Image.Source.Height : ActualHeight;
		}
		//===========================================================================================
		protected override void OnKeyDown(KeyEventArgs arguments)
		{
			// Disable KeyDown on the ImageViewer.
		}
		//===========================================================================================
		public ImageViewer()
			: base()
		{
			PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
			PreviewMouseLeftButtonUp += OnPreviewMouseLeftButtonUp;
			PreviewMouseMove += OnPreviewMouseMove;
		}
		//===========================================================================================
		public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register(
			"ImageSource", typeof(ImageSource), typeof(ImageViewer),
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender,
				new PropertyChangedCallback(OnImageSourceChanged)));
		//===========================================================================================
		public static readonly DependencyProperty LoadingImageSourceProperty = DependencyProperty.Register(
			"LoadingImageSource", typeof(ImageSource), typeof(ImageViewer),
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
		//===========================================================================================
		public ImageSource ImageSource
		{
			get
			{
				return (ImageSource)GetValue(ImageSourceProperty);
			}
			set
			{
				SetValue(ImageSourceProperty, value);
			}
		}
		//===========================================================================================
		public ImageSource LoadingImageSource
		{
			get
			{
				return (ImageSource)GetValue(LoadingImageSourceProperty);
			}
			set
			{
				SetValue(LoadingImageSourceProperty, value);
			}
		}
		//===========================================================================================
		public void HideLoadingImage()
		{
			ImageSource = null;
			Image.Source = null;
			Image.Style = (Style)FindResource("Image");
		}
		//===========================================================================================
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			CreateImage();
			LayoutUpdated += OnLayoutUpdated;
		}
		//===========================================================================================
		public void ShowLoadingImage()
		{
			Image.Style = (Style)FindResource("ImageLoading");
			Image.Source = LoadingImageSource;
			Image.BeginStoryboard("Rotate");
		}
		//==========================================================================================
	}
	//==============================================================================================
}
