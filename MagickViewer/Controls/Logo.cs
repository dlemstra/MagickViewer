//=================================================================================================
// Copyright 2014-2017 Dirk Lemstra <https://github.com/dlemstra/MagickViewer>
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

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MagickViewer.Controls
{
  internal sealed class Logo : Control
  {
    public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent(
      "Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Logo));

    [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
    static Logo()
    {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(Logo), new FrameworkPropertyMetadata(typeof(Logo)));
      EventManager.RegisterClassHandler(typeof(Logo), Mouse.MouseDownEvent, new MouseButtonEventHandler(OnMouseDown));
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
    {
      Process.Start(new ProcessStartInfo("https://github.com/dlemstra/MagickViewer"));
    }

    private static void RaiseMouseDown(Logo sender)
    {
      RoutedEventArgs eventArgs = new RoutedEventArgs(Logo.ClickEvent);
      sender.RaiseEvent(eventArgs);
    }

    public event RoutedEventHandler Click
    {
      add
      {
        AddHandler(ClickEvent, value);
      }
      remove
      {
        RemoveHandler(ClickEvent, value);
      }
    }
  }
}
