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
using System.Reflection;

namespace Cube.Pdf.App.Pinstaller
{
    /* --------------------------------------------------------------------- */
    ///
    /// ArgumentExtension
    ///
    /// <summary>
    /// Provides extended methods of the ArgumentCollection class.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public static class ArgumentExtension
    {
        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// GetCommand
        ///
        /// <summary>
        /// Gets the command name from the specified arguments.
        /// </summary>
        ///
        /// <param name="src">Source arguments.</param>
        ///
        /// <returns>Command name.</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static string GetCommand(this ArgumentCollection src) =>
            src.Options.TryGetValue("command", out var dest) ?
            dest :
            string.Empty;

        /* ----------------------------------------------------------------- */
        ///
        /// GetResourceDirectory
        ///
        /// <summary>
        /// Gets the resource directory from the specified arguments.
        /// </summary>
        ///
        /// <param name="src">Source arguments.</param>
        ///
        /// <returns>Resource directory.</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static string GetResourceDirectory(this ArgumentCollection src) =>
            src.Options.TryGetValue("resource", out var dest) ?
            dest :
            new AssemblyReader(Assembly.GetExecutingAssembly()).DirectoryName;

        /* ----------------------------------------------------------------- */
        ///
        /// GetRetryCount
        ///
        /// <summary>
        /// Gets the maximum retry count.
        /// </summary>
        ///
        /// <param name="src">Source arguments.</param>
        ///
        /// <returns>Retry count.</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static int GetRetryCount(this ArgumentCollection src)
        {
            if (!src.Options.TryGetValue("retry", out var str)) return 1;
            if (int.TryParse(str, out var dest)) return dest;
            else return 1;
        }

        #endregion
    }
}