// Copyright 2014-2017 Dirk Lemstra (https://github.com/dlemstra/MagickViewer)
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

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Threading;
using ImageMagick;
using Microsoft.Win32;

namespace MagickViewer
{
    internal sealed class ImageManager : IDisposable
    {
        private static readonly object _Semaphore = new object();
        private static readonly string[] _GhostscriptFormats = new string[]
        {
            ".EPS", ".PDF", ".PS"
        };

        private Dispatcher _dispatcher;
        private FileSystemWatcher _watcher;
        private ImageIterator _imageIterator;
        private int _index;
        private MagickImageCollection _images;
        private OpenFileDialog _openDialog;
        private SaveFileDialog _saveDialog;

        public ImageManager(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;

            Initialize();
        }

        public event EventHandler Loading;

        public event EventHandler<LoadedEventArgs> Loaded;

        public string FileInfo
        {
            get
            {
                if (_imageIterator.Current == null)
                    return null;

                string fileInfo = _imageIterator.Current.FullName;

                if (_images != null)
                {
                    if (_images.Count > 1)
                        fileInfo += " (" + (_index + 1) + " of " + _images.Count + ")";

                    if (Image != null)
                        fileInfo += " " + Image.Format.ToString().ToUpperInvariant() + " " + Image.Width + "x" + Image.Height;
                }

                return fileInfo;
            }
        }

        public IMagickImage Image
        {
            get
            {
                if (_images.Count == 0)
                    return null;

                return _images[_index];
            }
        }

        public static bool IsSupported(string fileName)
        {
            return new FileInfo(fileName).IsSupported();
        }

        public void Dispose()
        {
            DisposeImages();
            DisposeWatcher();
        }

        public void Load(string fileName)
        {
            Load(new FileInfo(fileName));
        }

        public void ShowOpenDialog()
        {
            if (_openDialog.ShowDialog() != true)
                return;

            Load(_openDialog.FileName);
        }

        public void ShowSaveDialog()
        {
            if (_saveDialog.ShowDialog() != true)
                return;

            Save(_saveDialog.FileName);
        }

        public void Next()
        {
            FileInfo file = _imageIterator.Next();
            if (file != null)
                Load(file);
        }

        public void NextFrame()
        {
            if (_images.Count < 2)
                return;

            if (++_index == _images.Count)
                _index = 0;

            OnFrameChanged();
        }

        public void Previous()
        {
            FileInfo file = _imageIterator.Previous();
            if (file != null)
                Load(file);
        }

        public void PreviousFrame()
        {
            if (_images.Count < 2)
                return;

            if (--_index == -1)
                _index = _images.Count - 1;

            OnFrameChanged();
        }

        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Formats should not be in uppercase.")]
        private static string CreateFilter(IEnumerable<MagickFormatInfo> formats)
        {
            var formatNames = from formatInfo in formats
                              orderby formatInfo.Format
                              select formatInfo.Format.ToString().ToLowerInvariant();

            var formatDescriptions = from formatInfo in formats
                                     orderby formatInfo.Description
                                     group formatInfo.Format by formatInfo.Description into g
                                     select g.Key + "|*." + string.Join(";*.", g).ToLowerInvariant();

            string filter = "All supported formats (...)|*." + string.Join(";*.", formatNames);

            filter += "|" + string.Join("|", formatDescriptions);

            return filter;
        }

        private void ConstructImages()
        {
            DisposeImages();

            _images = new MagickImageCollection();
            _index = 0;
        }

        private void DisposeImages()
        {
            if (_images == null)
                return;

            _images.Dispose();
            _images = null;
        }

        private void DisposeWatcher()
        {
            if (_watcher != null)
            {
                _watcher.Dispose();
                _watcher = null;
            }
        }

        private void Initialize()
        {
            _imageIterator = new ImageIterator();

            _watcher = new FileSystemWatcher();
            _watcher.Changed += OnFileChanged;

            _openDialog = new OpenFileDialog();
            SetOpenFilter();

            _saveDialog = new SaveFileDialog();
            SetSaveFilter();
        }

        private void Load(FileInfo file)
        {
            Monitor.Enter(_Semaphore);

            _imageIterator.Current = file;

            _watcher.Path = file.DirectoryName;
            _watcher.Filter = file.Name;
            _watcher.EnableRaisingEvents = true;

            OnLoading();

            Thread thread = new Thread(() => ReadImage(file));
            thread.Start();
        }

        private void OnFileChanged(object sender, FileSystemEventArgs arguments)
        {
            _watcher.EnableRaisingEvents = false;

            _imageIterator.Current.WaitForAccess();

            _dispatcher.Invoke((Action)(() =>
            {
                Load(_imageIterator.Current);
            }));
        }

        private void OnFrameChanged()
        {
            if (Loaded == null)
                return;

            _dispatcher.Invoke((Action)(() =>
            {
                Loaded(this, new LoadedEventArgs());
            }));
        }

        private void OnLoaded(MagickErrorException exception)
        {
            if (Loaded == null)
                return;

            _dispatcher.Invoke((Action)(() =>
            {
                Loaded(this, new LoadedEventArgs(exception));
                Monitor.Exit(_Semaphore);
            }));
        }

        private void OnLoading()
        {
            if (Loading == null)
                return;

            _dispatcher.Invoke((Action)(() =>
            {
                Loading(this, EventArgs.Empty);
            }));
        }

        private void ReadImage(FileInfo file)
        {
            ConstructImages();

            MagickErrorException exception = null;

            try
            {
                MagickReadSettings settings = new MagickReadSettings();
                if (_GhostscriptFormats.Contains(file.Extension.ToUpperInvariant()))
                    settings.Density = new Density(300);

                _images.Read(file, settings);
            }
            catch (MagickErrorException ex)
            {
                exception = ex;
            }

            OnLoaded(exception);
        }

        private void Save(string fileName)
        {
            _images.Write(fileName);
        }

        private void SetOpenFilter()
        {
            var formats = from formatInfo in MagickNET.SupportedFormats
                          where formatInfo.IsReadable
                          select formatInfo;

            _openDialog.Filter = CreateFilter(formats);
        }

        private void SetSaveFilter()
        {
            var formats = from formatInfo in MagickNET.SupportedFormats
                          where formatInfo.IsWritable
                          select formatInfo;

            _saveDialog.Filter = CreateFilter(formats);
        }
    }
}
