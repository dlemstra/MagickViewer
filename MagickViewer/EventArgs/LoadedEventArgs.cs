// Copyright Dirk Lemstra https://github.com/dlemstra/MagickViewer.
// Licensed under the Apache License, Version 2.0.

using System;
using ImageMagick;

namespace MagickViewer
{
    internal sealed class LoadedEventArgs : EventArgs
    {
        public LoadedEventArgs()
        {
        }

        public LoadedEventArgs(MagickErrorException exception)
            => Exception = exception;

        public MagickErrorException Exception { get; }
    }
}
