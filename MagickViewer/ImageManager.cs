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
	//==============================================================================================
	internal sealed class ImageManager : IDisposable
	{
		//===========================================================================================
		private static readonly object _Semaphore = new object();
		private static readonly string[] _GhostscriptFormats = new string[]
		{
			".EPS", ".PDF", ".PS"
		};
		//===========================================================================================
		private Dispatcher _Dispatcher;
		private ImageIterator _ImageIterator;
		private MagickImageCollection _Images;
		private int _Index;
		private OpenFileDialog _OpenDialog;
		private SaveFileDialog _SaveDialog;
		//===========================================================================================
		private void ConstructImages()
		{
			Dispose();

			_Images = new MagickImageCollection();
			_Index = 0;
		}
		//===========================================================================================
		[SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
		private static string CreateFilter(IEnumerable<MagickFormatInfo> formats)
		{
			string filter = "All supported formats (...)|*." + string.Join(";*.",
											from formatInfo in formats
											orderby formatInfo.Format
											select formatInfo.Format.ToString().ToLowerInvariant());

			filter += "|" + string.Join("|",
											from formatInfo in formats
											orderby formatInfo.Description
											group formatInfo.Format by formatInfo.Description into g
											select g.Key + "|*." + string.Join(";*.", g).ToLowerInvariant());
			return filter;
		}
		//===========================================================================================
		private void Initialize()
		{
			_ImageIterator = new ImageIterator();

			_OpenDialog = new OpenFileDialog();
			SetOpenFilter();

			_SaveDialog = new SaveFileDialog();
			SetSaveFilter();
		}
		//===========================================================================================
		private void Load(FileInfo file)
		{
			Monitor.Enter(_Semaphore);

			_ImageIterator.Current = file;

			OnLoading();

			Thread thread = new Thread(() => ReadImage(file));
			thread.Start();
		}
		//===========================================================================================
		private void OnFrameChanged()
		{
			if (Loaded == null)
				return;

			_Dispatcher.Invoke((Action)delegate()
			{
				Loaded(this, new LoadedEventArgs());
			});
		}
		//===========================================================================================
		private void OnLoaded(MagickErrorException exception)
		{
			if (Loaded == null)
				return;

			_Dispatcher.Invoke((Action)delegate()
			{
				Loaded(this, new LoadedEventArgs(exception));
				Monitor.Exit(_Semaphore);
			});
		}
		//===========================================================================================
		private void OnLoading()
		{
			if (Loading != null)
				Loading(this, EventArgs.Empty);
		}
		//===========================================================================================
		private void ReadImage(FileInfo file)
		{
			ConstructImages();

			MagickErrorException exception = null;

			try
			{
				MagickReadSettings settings = new MagickReadSettings();
				if (_GhostscriptFormats.Contains(file.Extension.ToUpperInvariant()))
					settings.Density = new PointD(300);

				_Images.Read(file, settings);
			}
			catch (MagickErrorException ex)
			{
				exception = ex;
			}

			OnLoaded(exception);
		}
		//===========================================================================================
		private void Save(string fileName)
		{
			_Images.Write(fileName);
		}
		//===========================================================================================
		private void SetOpenFilter()
		{
			var formats = from formatInfo in MagickNET.SupportedFormats
							  where formatInfo.IsReadable
							  select formatInfo;

			_OpenDialog.Filter = CreateFilter(formats);
		}
		//===========================================================================================
		private void SetSaveFilter()
		{
			var formats = from formatInfo in MagickNET.SupportedFormats
							  where formatInfo.IsWritable
							  select formatInfo;

			_SaveDialog.Filter = CreateFilter(formats);
		}
		//===========================================================================================
		public ImageManager(Dispatcher dispatcher)
		{
			_Dispatcher = dispatcher;

			Initialize();
		}
		//===========================================================================================
		public event EventHandler Loading;
		//===========================================================================================
		public event EventHandler<LoadedEventArgs> Loaded;
		//===========================================================================================
		public string FileName
		{
			get
			{
				if (_ImageIterator.Current == null)
					return null;

				string fileName = _ImageIterator.Current.FullName;

				if (_Images != null && _Images.Count > 1)
					fileName += " (" + (_Index + 1) + " of " + _Images.Count + ")";

				return fileName;
			}
		}
		//===========================================================================================
		public MagickImage Image
		{
			get
			{
				if (_Images.Count == 0)
					return null;

				return _Images[_Index];
			}
		}
		//===========================================================================================
		public void Dispose()
		{
			if (_Images == null)
				return;

			_Images.Dispose();
			_Images = null;
		}
		//===========================================================================================
		public static bool IsSupported(string fileName)
		{
			return new FileInfo(fileName).IsSupported();
		}
		//===========================================================================================
		public void Load(string fileName)
		{
			Load(new FileInfo(fileName));
		}
		//===========================================================================================
		public void ShowOpenDialog()
		{
			if (_OpenDialog.ShowDialog() != true)
				return;

			Load(_OpenDialog.FileName);
		}
		//===========================================================================================
		public void ShowSaveDialog()
		{
			if (_SaveDialog.ShowDialog() != true)
				return;

			Save(_SaveDialog.FileName);
		}
		//===========================================================================================
		public void Next()
		{
			FileInfo file = _ImageIterator.Next();
			if (file != null)
				Load(file);
		}
		//===========================================================================================
		public void NextFrame()
		{
			if (_Images.Count < 2)
				return;

			if (++_Index == _Images.Count)
				_Index = 0;

			OnFrameChanged();
		}
		//===========================================================================================
		public void Previous()
		{
			FileInfo file = _ImageIterator.Previous();
			if (file != null)
				Load(file);
		}
		//===========================================================================================
		public void PreviousFrame()
		{
			if (_Images.Count < 2)
				return;

			if (--_Index == -1)
				_Index = _Images.Count - 1;

			OnFrameChanged();
		}
		//===========================================================================================
	}
	//==============================================================================================
}
