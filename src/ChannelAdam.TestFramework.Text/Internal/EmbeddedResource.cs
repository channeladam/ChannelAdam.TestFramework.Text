//-----------------------------------------------------------------------
// <copyright file="EmbeddedResource.cs">
//     Copyright (c) 2018 Adam Craven. All rights reserved.
// </copyright>
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-----------------------------------------------------------------------

namespace ChannelAdam.TestFramework.Internal
{
    using System;
    using System.IO;
    using System.Reflection;

    internal static class EmbeddedResource
    {
        /// <summary>
        /// Gets the embedded resource from the given assembly as a stream.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="resourceName">Name of the resource.</param>
        /// <returns>The embedded resource as a stream.</returns>
        /// <remarks>Ensure that you dispose of the stream appropriately.</remarks>
        internal static Stream GetAsStream(Assembly assembly, string resourceName)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                throw new System.IO.FileNotFoundException($"Cannot find the embedded resource '{resourceName}' in assembly '{assembly.FullName}'.");
            }

            return stream;
        }

        /// <summary>
        /// Gets the string contents of the embedded resource.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="resourceName">Name of the resource.</param>
        /// <returns>The embedded resource as a string.</returns>
        internal static string GetAsString(Assembly assembly, string resourceName)
        {
            Stream stream = null;
            try
            {
                stream = GetAsStream(assembly, resourceName);
                using (var reader = new StreamReader(stream))
                {
                    stream = null;
                    return reader.ReadToEnd();
                }
            }
            finally
            {
                stream?.Dispose();
            }
        }
    }
}
