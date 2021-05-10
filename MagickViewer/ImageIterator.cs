// Copyright Dirk Lemstra https://github.com/dlemstra/MagickViewer.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Linq;

namespace MagickViewer
{
    internal sealed class ImageIterator
    {
        public FileInfo Current { get; set; }

        internal FileInfo Next()
        {
            if (Current == null)
                return null;

            var files = GetSupportedFiles();
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

            var files = GetSupportedFiles();
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

        private FileInfo[] GetSupportedFiles()
        {
            return (from file in Current.Directory.GetFiles()
                    where file.IsSupported()
                    select file).ToArray();
        }

        private bool IsCurrent(FileInfo file)
            => Current.FullName.Equals(file.FullName, StringComparison.OrdinalIgnoreCase);
    }
}
