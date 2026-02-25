// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.CodeAnalysis.Text;
using System.Diagnostics;

namespace Microsoft.CodeAnalysis.TypeScript.Syntax.InternalSyntax
{
    internal class SlidingTextWindow : IDisposable
    {
        private readonly SourceText _text;
        private int _basis;
        private int _offset;
        private readonly int _textLength;
        private char[] _characterWindow;
        private int _windowStart;
        private int _windowEnd;

        // The number of characters to keep in the window at any given time.
        // This is a trade-off between memory usage and the frequency of reading from the SourceText.
        private const int WindowSize = 2048;

        public const char InvalidCharacter = char.MaxValue;

        public SlidingTextWindow(SourceText text)
        {
            _text = text;
            _textLength = text.Length;
            _basis = 0;
            _offset = 0;
            _characterWindow = new char[WindowSize];
            _windowStart = 0;
            _windowEnd = 0;
        }

        public void Dispose()
        {
            _characterWindow = null!;
        }

        public int Position => _basis + _offset;

        public int Offset => _offset;

        public SourceText Text => _text;

        public char PeekChar(int delta = 0)
        {
            int position = Position + delta;
            if (position >= _textLength || position < 0)
            {
                return InvalidCharacter;
            }

            if (position >= _windowStart && position < _windowEnd)
            {
                return _characterWindow[position - _windowStart];
            }

            // Need to slide window or read directly if outside efficient range
            return _text[position];
        }

        public void AdvanceChar(int n = 1)
        {
            _offset += n;
        }

        public char NextChar()
        {
            char c = PeekChar();
            if (c != InvalidCharacter)
            {
                AdvanceChar();
            }
            return c;
        }

        public void Start()
        {
            _basis += _offset;
            _offset = 0;
        }

        public void Reset(int position)
        {
            _offset = position - _basis;
        }

        public string GetText(bool intern)
        {
            return _text.ToString(new TextSpan(_basis, _offset));
        }

        public int Width => _offset;
    }
}
