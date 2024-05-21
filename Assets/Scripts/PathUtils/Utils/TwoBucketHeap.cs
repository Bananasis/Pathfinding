using System.Collections.Generic;

namespace PathUtils
{
    public class TwoBucketHeap<T>
    {
        private Stack<T> _endStack = new();
        private Stack<T> _beginningStack = new();
        public int Count => _endStack.Count + _beginningStack.Count;

        public void InsertAtEnd(T element) => _endStack.Push(element);
        public void InsertInBeginning(T element) => _beginningStack.Push(element);

        public T Pop()
        {
            if (_beginningStack.Count != 0)
                return _beginningStack.Pop();
            SwapBuckets();
            return _beginningStack.Pop();
        }

        public bool TryPop(out T element)
        {
            element = default;
            if (_beginningStack.Count != 0)
            {
                element = _beginningStack.Pop();
                return true;
            }

            SwapBuckets();
            if (_beginningStack.Count == 0) return false;
            element = _beginningStack.Pop();
            return true;
        }

        public void Clear()
        {
            _endStack.Clear();
            _beginningStack.Clear();
        }

        private void SwapBuckets() => (_endStack, _beginningStack) = (_beginningStack, _endStack);
    }
}