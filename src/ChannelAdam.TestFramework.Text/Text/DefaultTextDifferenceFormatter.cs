//-----------------------------------------------------------------------
// <copyright file="DefaultTextDifferenceFormatter.cs">
//     Copyright (c) 2016-2020 Adam Craven. All rights reserved.
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
using System;
namespace ChannelAdam.TestFramework.Text
{
    using System.Text;

    using ChannelAdam.TestFramework.Text.Abstractions;
    using DiffPlex.DiffBuilder.Model;

    public class DefaultTextDifferenceFormatter : ITextDifferenceFormatter
    {
        public string FormatDifferences(DiffPaneModel differences)
        {
            if (differences == null)
            {
                throw new ArgumentNullException(nameof(differences));
            }

            var sb = new StringBuilder();

            foreach (var line in differences.Lines)
            {
                switch (line.Type)
                {
                    case ChangeType.Inserted:
                        sb.Append("+ ");
                        break;

                    case ChangeType.Deleted:
                        sb.Append("- ");
                        break;

                    case ChangeType.Modified:
                        sb.Append("* ");
                        break;

                    case ChangeType.Imaginary:
                        sb.Append("? ");
                        break;

                    default:
                        sb.Append("  ");
                        break;
                }

                sb.AppendLine(line.Text);
            }

            return sb.ToString();
        }
    }
}
