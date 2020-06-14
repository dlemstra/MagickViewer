// Copyright 2014-2018 Dirk Lemstra (https://github.com/dlemstra/MagickViewer)
//
// Licensed under the ImageMagick License (the "License"); you may not use this file except in
// compliance with the License. You may obtain a copy of the License at
//
//   https://www.imagemagick.org/script/license.php
//
// Unless required by applicable law or agreed to in writing, software distributed under the
// License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either
// express or implied. See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MagickViewer.Controls
{
    internal sealed class ImageViewer : ScrollViewer
    {
        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(ImageViewer), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnImageSourceChanged)));

        public static readonly DependencyProperty LoadingImageSourceProperty = DependencyProperty.Register("LoadingImageSource", typeof(ImageSource), typeof(ImageViewer), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        private MouseCapture _capture;
        private bool _fitToScreen = true;

        public ImageViewer()
          : base()
        {
            PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
            PreviewMouseLeftButtonUp += OnPreviewMouseLeftButtonUp;
            PreviewMouseMove += OnPreviewMouseMove;
        }

        public ImageSource ImageSource
        {
            get => (ImageSource)GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }

        public ImageSource LoadingImageSource
        {
            get => (ImageSource)GetValue(LoadingImageSourceProperty);
            set => SetValue(LoadingImageSourceProperty, value);
        }

        private Image Image => (Image)Content;

        public void HideLoadingImage()
        {
            ImageSource = null;
            Image.Source = null;
            Image.Style = (Style)FindResource("Image");
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            CreateImage();
            LayoutUpdated += OnLayoutUpdated;
        }

        public void ShowLoadingImage()
        {
            Image.Style = (Style)FindResource("ImageLoading");
            Image.Source = LoadingImageSource;
            Image.BeginStoryboard("Rotate");
        }

        protected override void OnKeyDown(KeyEventArgs arguments)
        {
            // Disable KeyDown on the ImageViewer.
        }

        private static void OnImageSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs arguments)
        {
            var target = sender as ImageViewer;
            if (target == null)
                return;

            var source = (ImageSource)arguments.NewValue;
            if (source == null)
                return;

            target.Image.Source = source;
            target.SetImageSize();
        }

        private void CreateImage()
        {
            var image = new Image
            {
                Style = (Style)FindResource("Image")
            };

            Content = image;
            UpdateImageSize();
        }

        private void OnLayoutUpdated(object sender, EventArgs e)
            => UpdateImageSize();

        private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs arguments)
        {
            var target = sender as ScrollViewer;
            if (target == null)
                return;

            _capture = new MouseCapture(target, arguments);

            if (arguments.ClickCount == 2)
                ToggleFitToScreen();
        }

        private void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs arguments)
        {
            var target = sender as ScrollViewer;
            if (target == null)
                return;

            target.ReleaseMouseCapture();
            _capture = null;
        }

        private void OnPreviewMouseMove(object sender, MouseEventArgs arguments)
        {
            if (_capture == null)
                return;

            if (arguments.LeftButton != MouseButtonState.Pressed)
                return;

            var target = sender as ScrollViewer;
            if (target == null)
                return;

            var point = arguments.GetPosition(target);

            var dx = point.X - _capture.Point.X;
            var dy = point.Y - _capture.Point.Y;
            if (Math.Abs(dy) > 5 || Math.Abs(dx) > 5)
                target.CaptureMouse();

            target.ScrollToHorizontalOffset(_capture.HorizontalOffset - dx);
            target.ScrollToVerticalOffset(_capture.VerticalOffset - dy);
        }

        private void SetImageSize()
        {
            if (_fitToScreen)
            {
                UpdateImageSize();
            }
            else if (Image.Source != null)
            {
                Image.Width = Image.Source.Width;
                Image.Height = Image.Source.Height;
            }
        }

        private void ToggleFitToScreen()
        {
            _fitToScreen = !_fitToScreen;
            Image.Stretch = _fitToScreen ? Stretch.Uniform : Stretch.None;
            SetImageSize();
        }

        private void UpdateImageSize()
        {
            if (!_fitToScreen || Image.Source == null)
                return;

            Image.Width = ActualWidth > Image.Source.Width ? Image.Source.Width : ActualWidth;
            Image.Height = ActualHeight > Image.Source.Height ? Image.Source.Height : ActualHeight;
        }
    }
}
