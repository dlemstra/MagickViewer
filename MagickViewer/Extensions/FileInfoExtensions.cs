// Copyright Dirk Lemstra https://github.com/dlemstra/MagickViewer.
// Licensed under the Apache License, Version 2.0.

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

            if (!Enum.TryParse(self.Extension.Substring(1), true, out MagickFormat format))
                return false;

            return (from formatInfo in MagickNET.SupportedFormats
                    where formatInfo.IsReadable && formatInfo.Format == format
                    select formatInfo).Any();
        }

        public static bool WaitForAccess(this FileInfo file)
        {
            var hasAccess = false;
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
