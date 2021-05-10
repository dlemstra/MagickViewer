// Copyright Dirk Lemstra https://github.com/dlemstra/MagickViewer.
// Licensed under the Apache License, Version 2.0.

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

        public double HorizontalOffset { get; }

        public Point Point { get; }

        public double VerticalOffset { get; }
    }
}
