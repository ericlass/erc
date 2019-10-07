using System;
using System.Collections.Generic;

namespace erc
{
    public class SimpleIterator<T>
    {
        private List<T> _items = null;
        private int _position = 0;
        private int _captureStart = 0;

        public SimpleIterator(List<T> items)
        {
            _items = items;
        }

        public static SimpleIterator<T> Singleton(T value)
        {
            return new SimpleIterator<T>(new List<T> { value });
        }

        public void StartCapture()
        {
            _captureStart = _position;
        }

        public List<T> GetCapture()
        {
            if (_position >= _items.Count)
                return new List<T>();

            return _items.GetRange(_captureStart + 1, _position - _captureStart);
        }

        public T Current()
        {
            if (_position >= _items.Count)
                return default(T);

            return _items[_position];
        }

        public T Next()
        {
            if (_position + 1 >= _items.Count)
                return default(T);

            return _items[_position + 1];
        }

        public T Pop()
        {
            if (_position >= _items.Count)
                return default(T);

            var result = _items[_position];
            _position += 1;
            return result;
        }

        public void Step()
        {
            _position += 1;
        }

        public bool HasMore()
        {
            return _position < _items.Count;
        }

    }
}
