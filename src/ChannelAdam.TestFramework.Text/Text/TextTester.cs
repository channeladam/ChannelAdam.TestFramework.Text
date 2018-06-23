﻿//-----------------------------------------------------------------------
// <copyright file="TextTester.cs">
//     Copyright (c) 2016-2018 Adam Craven. All rights reserved.
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

namespace ChannelAdam.TestFramework.Text
{
    using System;
    using System.Linq;
    using System.Reflection;
    using ChannelAdam.Logging.Abstractions;
    using ChannelAdam.TestFramework.Abstractions;
    using ChannelAdam.TestFramework.Internal;
    using DiffPlex;
    using DiffPlex.DiffBuilder;
    using DiffPlex.DiffBuilder.Model;
    using Logging;
    using Text.Abstractions;

    public class TextTester
    {
        #region Fields

        private readonly ISimpleLogger logger;
        private readonly ILogAsserter logAssert;
        private readonly ITextDifferenceFormatter differenceFormatter;

        private string actualText;
        private string expectedText;
        private DiffPaneModel differences;

        #endregion

        #region Constructors

        public TextTester(ILogAsserter logAsserter) : this(new SimpleConsoleLogger(), logAsserter)
        {
        }

        public TextTester(ISimpleLogger logger, ILogAsserter logAsserter) : this(logger, logAsserter, new DefaultTextDifferenceFormatter())
        {
        }

        public TextTester(ILogAsserter logAsserter, ITextDifferenceFormatter differenceFormatter) : this(new SimpleConsoleLogger(), logAsserter, differenceFormatter)
        {
        }

        public TextTester(ISimpleLogger logger, ILogAsserter logAsserter, ITextDifferenceFormatter differenceFormatter)
        {
            this.logger = logger;
            this.logAssert = logAsserter;
            this.differenceFormatter = differenceFormatter;
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the actual text property is changed.
        /// </summary>
        public event EventHandler<TextChangedEventArgs> ActualTextChangedEvent;

        /// <summary>
        /// Occurs when expected text property is changed.
        /// </summary>
        public event EventHandler<TextChangedEventArgs> ExpectedTextChangedEvent;

        /// <summary>
        /// Occurs when a text difference is detected, allowing a listener to filter the differences and
        /// change the Line[x].Type to ChangeType.UnChanged so that the difference is no longer treated as a difference.
        /// </summary>
        /// <remarks>
        /// This event or the TextDifferenceFilter property can be used for this purpose.
        /// </remarks>
        public event EventHandler<TextDifferenceDetectedEventArgs> TextDifferenceDetectedEvent;

        #endregion

        #region Properties

        public string ActualText
        {
            get
            {
                return this.actualText;
            }

            private set
            {
                this.actualText = value;
                this.OnActualTextChanged(value);
            }
        }

        public string ExpectedText
        {
            get
            {
                return this.expectedText;
            }

            private set
            {
                this.expectedText = value;
                this.OnExpectedTextChanged(value);
            }
        }

        public DiffPaneModel Differences
        {
            get { return this.differences; }
        }

        /// <summary>
        /// Gets or sets the Action delegate to be invoked when a text difference is detected, allowing differences to be filtered out by
        /// changing the Line[x].Type to ChangeType.UnChanged - so that a difference is no longer treated as a difference.
        /// </summary>
        /// <value>The Action delegate.</value>
        /// <remarks>
        /// This property or TextDifferenceDetectedEvent can be used for this purpose.
        /// </remarks>
        public Action<DiffPaneModel> TextDifferenceFilter { get; set; }

        #endregion

        #region Public Methods

        #region Arrange Actual Text

        /// <summary>
        /// Arrange the actual text from an embedded resource in the given assembly.
        /// </summary>
        /// <param name="assembly">The assembly that contains the resource.</param>
        /// <param name="resourceName">The name of the resource.</param>
        public void ArrangeActualText(Assembly assembly, string resourceName)
        {
            this.ArrangeActualText(EmbeddedResource.GetAsString(assembly, resourceName));
        }

        /// <summary>
        /// Arrange the actual text from the given string.
        /// </summary>
        /// <param name="text">The string to set as the actual text.</param>
        public void ArrangeActualText(string text)
        {
            this.ActualText = text ?? throw new ArgumentNullException(nameof(text));
        }

        #endregion

        #region Arrange Expected Text

        /// <summary>
        /// Arrange the expected text from an embedded resource in the given assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="resourceName">Name of the resource.</param>
        public void ArrangeExpectedText(Assembly assembly, string resourceName)
        {
            this.ArrangeExpectedText(EmbeddedResource.GetAsString(assembly, resourceName));
        }

        /// <summary>
        /// Arrange the expected text from the given string.
        /// </summary>
        /// <param name="text">The text string.</param>
        public void ArrangeExpectedText(string text)
        {
            this.ExpectedText = text ?? throw new ArgumentNullException(nameof(text));
        }

        #endregion

        #region Assertions

        /// <summary>
        /// Assert the actual text against the expected text.
        /// </summary>
        public virtual void AssertActualTextEqualsExpectedText()
        {
            this.logger.Log("Asserting actual and expected text are equal");

            var isEqual = this.IsEqual(this.ExpectedText, this.ActualText);
            if (!isEqual)
            {
                var report = this.differenceFormatter.FormatDifferences(this.Differences);
                this.logger.Log("The differences are: " + Environment.NewLine + report);
            }

            this.logAssert.IsTrue("The text is as expected", isEqual);
            this.logger.Log("The text is as expected");
        }

        #endregion

        #region Utility Methods

        public bool IsEqual()
        {
            return this.IsEqual(this.ExpectedText, this.ActualText);
        }

        /// <summary>
        /// Determines if the given actual and expected text is equivalent.
        /// </summary>
        /// <param name="expected">The expected text.</param>
        /// <param name="actual">The actual text.</param>
        /// <returns>
        /// The text differences.
        /// </returns>
        public virtual bool IsEqual(string expected, string actual)
        {
            var differ = new Differ();
            var inlineBuilder = new InlineDiffBuilder(differ);
            this.differences = inlineBuilder.BuildDiffModel(expected, actual);

            if (!AreAllLinesUnchanged(this.differences))
            {
                this.OnTextDifferenceDetected(this.differences);
            }

            return AreAllLinesUnchanged(this.differences);
        }

        #endregion

        #endregion

        #region Protected Change Methods

        protected virtual void OnTextDifferenceDetected(DiffPaneModel theDifferences)
        {
            this.TextDifferenceFilter?.Invoke(theDifferences);
            this.TextDifferenceDetectedEvent?.Invoke(this, new TextDifferenceDetectedEventArgs(theDifferences));
        }

        protected virtual void OnExpectedTextChanged(string value)
        {
            if (this.ExpectedTextChangedEvent == null)
            {
                // Only log the details if the event has not been subscribed to - because it is expected that the subscriber instead will log with more contextual detail
                this.logger.Log();
                this.logger.Log($"The expected text is: {Environment.NewLine}{value}");
            }
            else
            {
                this.ExpectedTextChangedEvent.Invoke(this, new TextChangedEventArgs(value));
            }
        }

        protected virtual void OnActualTextChanged(string value)
        {
            if (this.ActualTextChangedEvent == null)
            {
                // Only log the details if the event has not been subscribed to - because it is expected that the subscriber instead will log with more contextual detail
                this.logger.Log();
                this.logger.Log($"The actual text is: {Environment.NewLine}{value}");
            }
            else
            {
                this.ActualTextChangedEvent.Invoke(this, new TextChangedEventArgs(value));
            }
        }

        #endregion

        #region Private Methods

        private static bool AreAllLinesUnchanged(DiffPaneModel differences)
        {
            return differences.Lines.All(l => l.Type == ChangeType.Unchanged);
        }

        #endregion
    }
}
