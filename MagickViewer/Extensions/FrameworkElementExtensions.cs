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

            Storyboard storyboard = self.FindResource(resourceKey) as Storyboard;
            if (storyboard == null)
                return;

            Storyboard.SetTarget(storyboard, self);
            storyboard.Begin();
        }
    }
}
