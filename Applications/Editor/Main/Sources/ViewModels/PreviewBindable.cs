﻿/* ------------------------------------------------------------------------- */
//
// Copyright (c) 2010 CubeSoft, Inc.
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
/* ------------------------------------------------------------------------- */
using Cube.FileSystem;
using Cube.Pdf.Mixin;
using Cube.Xui;
using System.Windows.Media;

namespace Cube.Pdf.Editor
{
    /* --------------------------------------------------------------------- */
    ///
    /// PreviewBindable
    ///
    /// <summary>
    /// Provides values for binding to the PreviewWindow.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class PreviewBindable
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// PreviewBindable
        ///
        /// <summary>
        /// Initializes a new instance of the PreviewBindable
        /// class with the specified arguments.
        /// </summary>
        ///
        /// <param name="file">Information of the PDF file.</param>
        /// <param name="page">Page object.</param>
        ///
        /* ----------------------------------------------------------------- */
        public PreviewBindable(Information file, Page page)
        {
            var size  = page.GetViewSize().Value;
            var magic = 14.0; // Scrollbar width
            var ratio = size.Width / size.Height;
            var diff  = size.Width > size.Height ? magic * ratio : -(magic * ratio);

            File   = new Bindable<Information>(file);
            Width  = new Bindable<int>((int)size.Width);
            Height = new Bindable<int>((int)(size.Height + diff));
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// File
        ///
        /// <summary>
        /// Gets the information of the PDF file.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Bindable<Information> File { get; }

        /* ----------------------------------------------------------------- */
        ///
        /// Image
        ///
        /// <summary>
        /// Gets the generated image object.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Bindable<ImageSource> Image { get; } = new Bindable<ImageSource>();

        /* ----------------------------------------------------------------- */
        ///
        /// Width
        ///
        /// <summary>
        /// Gets the width of the window.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Bindable<int> Width { get; }

        /* ----------------------------------------------------------------- */
        ///
        /// Height
        ///
        /// <summary>
        /// Gets the height of the window.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Bindable<int> Height { get; }

        /* ----------------------------------------------------------------- */
        ///
        /// Busy
        ///
        /// <summary>
        /// Gets a value indicating whether models are busy.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Bindable<bool> Busy { get; } = new Bindable<bool>(false);

        #endregion
    }
}
