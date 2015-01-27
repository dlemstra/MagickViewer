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
	internal sealed class ImageManager
	{
		//===========================================================================================
		private static readonly object _Semaphore = new object();
		private static readonly string[] _GhostscriptFormats = new string[]
		{
			".EPS", ".PDF", ".PS"
		};
		//===========================================================================================
		private Dispatcher _Dispatcher;
		private OpenFileDialog _OpenDialog;
		private SaveFileDialog _SaveDialog;
		//===========================================================================================
		private void ConstructImages()
		{
			if (Images != null)
				Images.Dispose();

			Images = new MagickImageCollection();
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
			_OpenDialog = new OpenFileDialog();
			SetOpenFilter();

			_SaveDialog = new SaveFileDialog();
			SetSaveFilter();
		}
		//===========================================================================================
		private void OnLoaded()
		{
			if (Loaded == null)
				return;

			_Dispatcher.Invoke((Action)delegate()
			{
				Loaded(this, EventArgs.Empty);
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

			try
			{
				MagickReadSettings settings = new MagickReadSettings();
				if (_GhostscriptFormats.Contains(file.Extension.ToUpperInvariant()))
					settings.Density = new PointD(300);

				Images.Read(file, settings);
				FileName = file.Name;
			}
			catch (MagickErrorException)
			{
				//TODO: Handle error
			}

			OnLoaded();
		}
		//===========================================================================================
		private void Save(string fileName)
		{
			Images.Write(fileName);
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
		public event EventHandler Loaded;
		//===========================================================================================
		public string FileName
		{
			get;
			private set;
		}
		//===========================================================================================
		public MagickImageCollection Images
		{
			get;
			private set;
		}
		//===========================================================================================
		public static bool IsSupported(string fileName)
		{
			if (string.IsNullOrEmpty(fileName))
				return false;

			if (fileName.Length < 2)
				return false;

			string extension = Path.GetExtension(fileName);
			if (string.IsNullOrEmpty(extension))
				return false;

			extension = extension.Substring(1);
			MagickFormat format;
			if (!Enum.TryParse<MagickFormat>(extension, true, out format))
				return false;

			return (from formatInfo in MagickNET.SupportedFormats
					  where formatInfo.IsReadable && formatInfo.Format == format
					  select formatInfo).Any();

		}
		//===========================================================================================
		public void Load(string fileName)
		{
			Monitor.Enter(_Semaphore);

			OnLoading();

			Thread thread = new Thread(() => ReadImage(new FileInfo(fileName)));
			thread.Start();
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
	}
	//==============================================================================================
}
