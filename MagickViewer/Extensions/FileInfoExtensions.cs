//=================================================================================================
// Copyright 2014-2017 Dirk Lemstra <https://magickviewer.codeplex.com/>
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
using System.IO;
using System.Linq;
using System.Threading;
using ImageMagick;

namespace MagickViewer
{
  internal static class FileInfoExtensions
  {
    public static bool IsSupported(this FileInfo self)
    {
      if (self == null || string.IsNullOrEmpty(self.FullName))
        return false;

      if (self.Name.Length < 2)
        return false;

      if (string.IsNullOrEmpty(self.Extension))
        return false;

      MagickFormat format;
      if (!Enum.TryParse(self.Extension.Substring(1), true, out format))
        return false;

      return (from formatInfo in MagickNET.SupportedFormats
              where formatInfo.IsReadable && formatInfo.Format == format
              select formatInfo).Any();
    }

    public static bool WaitForAccess(this FileInfo file)
    {
      bool hasAccess = false;
      while (!hasAccess)
      {
        try
        {
          using (var stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None))
          {
            hasAccess = true;
          }
        }
        catch (IOException)
        {
        }

        if (!hasAccess)
          Thread.Sleep(100);
      }

      return true;
    }
  }
}
