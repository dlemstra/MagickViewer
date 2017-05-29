// Copyright 2014-2017 Dirk Lemstra (https://github.com/dlemstra/MagickViewer)
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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MagickViewer.Controls
{
    internal sealed class MouseCapture
    {
        public MouseCapture(ScrollViewer scrollViewer, MouseButtonEventArgs arguments)
        {
            VerticalOffset = scrollViewer.VerticalOffset;
            HorizontalOffset = scrollViewer.HorizontalOffset;
            Point = arguments.GetPosition(scrollViewer);
        }

        public double HorizontalOffset
        {
            get;
            private set;
        }

        public Point Point
        {
            get;
            private set;
        }

        public double VerticalOffset
        {
            get;
            private set;
        }
    }
}
