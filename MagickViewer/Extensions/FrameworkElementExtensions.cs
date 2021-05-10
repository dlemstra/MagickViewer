// Copyright Dirk Lemstra https://github.com/dlemstra/MagickViewer.
// Licensed under the Apache License, Version 2.0.

using System.Windows;
using System.Windows.Media.Animation;

namespace MagickViewer
{
    internal static class FrameworkElementExtensions
    {
        public static void BeginStoryboard(this FrameworkElement self, string resourceKey)
        {
            if (self == null)
                return;

            var storyboard = self.FindResource(resourceKey) as Storyboard;
            if (storyboard == null)
                return;

            Storyboard.SetTarget(storyboard, self);
            storyboard.Begin();
        }
    }
}
