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
using System.IO;
using System.Linq;

namespace MagickViewer
{
  internal sealed class ImageIterator
  {
    private FileInfo[] GetSupportedFiles()
    {
      return (from file in Current.Directory.GetFiles()
              where file.IsSupported()
              select file).ToArray();
    }

    private bool IsCurrent(FileInfo file)
    {
      return Current.FullName.Equals(file.FullName, StringComparison.OrdinalIgnoreCase);
    }

    public FileInfo Current
    {
      get;
      set;
    }

    internal FileInfo Next()
    {
      if (Current == null)
        return null;

      FileInfo[] files = GetSupportedFiles();
      if (files.Length == 1)
        return null;

      for (int i = 0; i < files.Length; i++)
      {
        if (IsCurrent(files[i]))
        {
          i++;
          if (i == files.Length)
            i = 0;
          return files[i];
        }
      }

      return null;
    }

    internal FileInfo Previous()
    {
      if (Current == null)
        return null;

      FileInfo[] files = GetSupportedFiles();
      if (files.Length == 1)
        return null;

      for (int i = files.Length - 1; i >= 0; i--)
      {
        if (IsCurrent(files[i]))
        {
          i--;
          if (i == -1)
            i = files.Length - 1;
          return files[i];
        }
      }

      return null;
    }
  }
}
