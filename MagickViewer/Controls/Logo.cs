// Copyright Dirk Lemstra https://github.com/dlemstra/MagickViewer.
// Licensed under the Apache License, Version 2.0.

using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MagickViewer.Controls
{
    internal sealed class Logo : Control
    {
        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent(
          "Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Logo));

        static Logo()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Logo), new FrameworkPropertyMetadata(typeof(Logo)));
            EventManager.RegisterClassHandler(typeof(Logo), Mouse.MouseDownEvent, new MouseButtonEventHandler(OnMouseDown));
        }

        public event RoutedEventHandler Click
        {
            add => AddHandler(ClickEvent, value);
            remove => RemoveHandler(ClickEvent, value);
        }

        private static void OnMouseDown(object sender, MouseButtonEventArgs arguments)
        {
            if (arguments.OriginalSource is Image)
                RaiseMouseDown(sender as Logo);
            else
                OpenWebsite();

            arguments.Handled = true;
        }

        private static void OpenWebsite()
            => Process.Start(new ProcessStartInfo("https://github.com/dlemstra/MagickViewer"));

        private static void RaiseMouseDown(Logo sender)
        {
            var eventArgs = new RoutedEventArgs(Logo.ClickEvent);
            sender.RaiseEvent(eventArgs);
        }
    }
}
