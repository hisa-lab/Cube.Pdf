﻿/* ------------------------------------------------------------------------- */
//
// Copyright (c) 2010 CubeSoft, Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
/* ------------------------------------------------------------------------- */
using Cube.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

namespace Cube.Pdf.Pdfium
{
    /* --------------------------------------------------------------------- */
    ///
    /// ReadOnlyPageList
    ///
    /// <summary>
    /// Provides functionality to access PDF pages as read only.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    internal class ReadOnlyPageList : EnumerableBase<Page>, IReadOnlyList<Page>
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// ReadOnlyPageList
        ///
        /// <summary>
        /// Initializes a new instance of the ReadOnlyPageList class with
        /// the specified arguments.
        /// </summary>
        ///
        /// <param name="core">PDFium object.</param>
        /// <param name="file">File information of the PDF document.</param>
        ///
        /* ----------------------------------------------------------------- */
        public ReadOnlyPageList(PdfiumReader core, PdfFile file)
        {
            Debug.Assert(core != null);
            Debug.Assert(file != null);

            File  = file;
            _core = core;
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// File
        ///
        /// <summary>
        /// Gets the file information of the PDF document.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public PdfFile File { get; }

        /* ----------------------------------------------------------------- */
        ///
        /// Count
        ///
        /// <summary>
        /// Gets the number of PDF pages.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public int Count => File.Count;

        /* ----------------------------------------------------------------- */
        ///
        /// Item[int]
        ///
        /// <summary>
        /// Gets the Page object corresponding to the specified index.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Page this[int index] => GetPage(index);

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// GetEnumerator
        ///
        /// <summary>
        /// Returns an enumerator that iterates through this collection.
        /// </summary>
        ///
        /// <returns>
        /// An IEnumerator(Page) object for this collection.
        /// </returns>
        ///
        /* ----------------------------------------------------------------- */
        public override IEnumerator<Page> GetEnumerator()
        {
            for (var i = 0; i < Count; ++i) yield return this[i];
        }

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// GetPage
        ///
        /// <summary>
        /// Gets the Page object.
        /// </summary>
        ///
        /// <param name="index">Zero for the first page.</param>
        ///
        /* ----------------------------------------------------------------- */
        private Page GetPage(int index)
        {
            var page = _core.Invoke(e => PdfiumApi.FPDF_LoadPage(e, index, 5));
            if (page == IntPtr.Zero) throw _core.GetLastError();

            try
            {
                var degree = GetPageRotation(page);
                var size   = GetPageSize(page, degree);

                return new Page(
                    File,                    // File
                    index + 1,               // Number
                    size,                    // Size
                    new Angle(degree),       // Rotation
                    new PointF(72.0f, 72.0f) // Resolution
                );
            }
            finally { PdfiumApi.FPDF_ClosePage(page); }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetSize
        ///
        /// <summary>
        /// Gets the page size.
        /// </summary>
        ///
        /// <remarks>
        /// PDFium は回転後のサイズを返しますが、Page オブジェクトでは
        /// 回転前の情報として格納します。そのため、場合によって幅と
        /// 高さの情報を反転しています。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        private SizeF GetPageSize(IntPtr handle, int degree)
        {
            var w = (float)PdfiumApi.FPDF_GetPageWidth(handle);
            var h = (float)PdfiumApi.FPDF_GetPageHeight(handle);

            return (degree != 90 && degree != 270) ? new SizeF(w, h) : new SizeF(h, w);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetPageRotation
        ///
        /// <summary>
        /// Gets the degree of PDF page rotation.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private int GetPageRotation(IntPtr handle)
        {
            var dest = PdfiumApi.FPDFPage_GetRotation(handle);
            return dest == 1 ?  90 :
                   dest == 2 ? 180 :
                   dest == 3 ? 270 : 0;
        }

        #endregion

        #region Fields
        private readonly PdfiumReader _core;
        #endregion
    }
}
