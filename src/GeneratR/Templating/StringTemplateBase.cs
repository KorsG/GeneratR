using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace GeneratR.Templating
{
    public abstract class StringTemplateBase
    {
        private readonly List<int> _indentLengths;
        private bool _endsWithNewline;

        public StringTemplateBase()
        {
            TemplateBuilder = new StringBuilder();
            _indentLengths = new List<int>();
        }

        /// <summary>
        /// The string builder that the template writes to.
        /// </summary>
        protected StringBuilder TemplateBuilder { get; }

        /// <summary>
        /// Set the default Indent value. Defaults to "\t" (Tab).
        /// </summary>
        public string DefaultIndent { get; set; } = "\t";

        /// <summary>
        /// Gets the current indent we use when adding lines to the output.
        /// </summary>
        public string CurrentIndent { get; private set; } = string.Empty;

        /// <summary>
        /// Write text directly to <see cref="TemplateBuilder"/>.
        /// </summary>
        public void Write(string textToAppend = "")
        {
            if (string.IsNullOrEmpty(textToAppend))
            {
                return;
            }

            // If we're starting off, or if the previous text ended with a newline, we have to append the current indent first.
            if ((TemplateBuilder.Length == 0) || _endsWithNewline)
            {
                TemplateBuilder.Append(CurrentIndent);
                _endsWithNewline = false;
            }

            // Check if the current text ends with a newline.
            if (textToAppend.EndsWith(Environment.NewLine, StringComparison.CurrentCulture))
            {
                _endsWithNewline = true;
            }

            // This is an optimization. If the current indent is "", then we don't have to do any of the more complex stuff further down.
            if (CurrentIndent.Length == 0)
            {
                TemplateBuilder.Append(textToAppend);
                return;
            }

            // Everywhere there is a newline in the text, add an indent after it.
            textToAppend = textToAppend.Replace(Environment.NewLine, Environment.NewLine + CurrentIndent);

            // If the text ends with a newline, then we should strip off the indent added at the very end
            // because the appropriate indent will be added when the next time Write() is called.
            if (_endsWithNewline)
            {
                TemplateBuilder.Append(textToAppend, 0, textToAppend.Length - CurrentIndent.Length);
            }
            else
            {
                TemplateBuilder.Append(textToAppend);
            }
        }

        /// <summary>
        /// Write formatted text directly to <see cref="TemplateBuilder"/>.
        /// </summary>
        public void Write(string format, params object[] args)
        {
            Write(string.Format(CultureInfo.CurrentCulture, format, args));
        }

        /// <summary>
        /// Write text directly to <see cref="TemplateBuilder"/>.
        /// </summary>
        public void WriteLine(string textToAppend = "")
        {
            Write(textToAppend);
            TemplateBuilder.AppendLine();
            _endsWithNewline = true;
        }

        /// <summary>
        /// Write formatted text directly to <see cref="TemplateBuilder"/>.
        /// </summary>
        public void WriteLine(string format, params object[] args)
        {
            WriteLine(string.Format(CultureInfo.CurrentCulture, format, args));
        }

        /// <summary>
        /// Increase the indent with the <see cref="DefaultIndent"/> value.
        /// </summary>
        public void PushIndent()
        {
            PushIndent(DefaultIndent);
        }

        /// <summary>
        /// Increase the indent with the provided value.
        /// </summary>
        public void PushIndent(string indent)
        {
            if (indent == null) { throw new ArgumentNullException(nameof(indent)); }

            CurrentIndent = CurrentIndent + indent;
            _indentLengths.Add(indent.Length);
        }

        /// <summary>
        /// Remove the last indent that was added with PushIndent.
        /// </summary>
        public string PopIndent()
        {
            var returnValue = string.Empty;
            if (_indentLengths.Count > 0)
            {
                var indentLength = _indentLengths[_indentLengths.Count - 1];
                _indentLengths.RemoveAt(_indentLengths.Count - 1);
                if (indentLength > 0)
                {
                    returnValue = CurrentIndent.Substring(CurrentIndent.Length - indentLength);
                    CurrentIndent = CurrentIndent.Remove(CurrentIndent.Length - indentLength);
                }
            }
            return returnValue;
        }

        /// <summary>
        /// Remove any indentation.
        /// </summary>
        public void ClearIndent()
        {
            _indentLengths.Clear();
            CurrentIndent = string.Empty;
        }

        public IDisposable IndentScope() => new DisposableIndenter(this, DefaultIndent);

        public IDisposable IndentScope(string indent) => new DisposableIndenter(this, indent);

        private sealed class DisposableIndenter : IDisposable
        {
            private readonly StringTemplateBase _template;

            public DisposableIndenter(StringTemplateBase template, string indent)
            {
                _template = template;
                _template.PushIndent(indent);
            }

            public void Dispose() => _template.PopIndent();
        }
    }
}
