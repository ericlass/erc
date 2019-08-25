using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace erc
{
    public class StringIterator
    {
        private char[] _items = null;
        private int _position = 0;
        private uint _line = 1;
        private uint _column = 1;

        public uint Line { get => _line; }
        public uint Column { get => _column; }

        public StringIterator(string str)
        {
            _items = str.ToCharArray();
        }

        public char Current()
        {
            if (_position >= _items.Length)
                return (char)0;

            return _items[_position];
        }

        public char Next()
        {
            if (_position + 1 >= _items.Length)
                return (char)0;

            return _items[_position + 1];
        }

        public void Step()
        {
            _position += 1;

            _column += 1;
            if (Current() == '\n')
            {
                _line += 1;
                _column = 0;
            }
        }

        public bool HasMore()
        {
            return _position < _items.Length;
        }
    }
}
