using System.Collections.Generic;

namespace RingBuffer
{
	public class RingBuffer<T>
	{
		public int Capacity { get; protected set; }
		public bool IsEmpty { get; protected set; }

		public int Count
		{
			get
			{
				if (IsEmpty) { return 0; }
				if (_endIndex > _startIndex) {	return _endIndex - _startIndex; }
				if (_endIndex == 0) { return 0; }
				return Capacity - (_startIndex - _endIndex);
			}
		}

		public event System.Action<T> OnOverrideExistingItem;

		private List<T> _list;
		private int _startIndex;
		private int _endIndex;
		private bool _cleanupOnPop;
		private T _default;

		public RingBuffer(int size, bool cleanupOnPop = false)
		{
			Capacity = size;
			_list = new List<T>(size);
			IsEmpty = true;
			_cleanupOnPop = cleanupOnPop;
			_default = default(T);
		}

		public void Push(T newItem)
		{
			if (_list.Count < Capacity && _endIndex == _list.Count)
			{
				_list.Add(newItem);
				_endIndex += 1;
			}
			else
			{
				var newLastIndexCandidate = FindInsertionIndex();
				
				if (OnOverrideExistingItem != null)
				{
					OnOverrideExistingItem(_list[newLastIndexCandidate]);
				}

				_list[newLastIndexCandidate] = newItem;
				_endIndex = newLastIndexCandidate + 1;

				if (!IsEmpty && newLastIndexCandidate == _startIndex)
				{
					IncrementStartIndex();
				}
			}

			IsEmpty = false;
		}
			
		public T Pop()
		{
			if (IsEmpty) { throw new System.ArgumentException("Trying to pop from empty ringbuffer."); }

			var lastIndex = _endIndex - 1;
			var itemToReturn = _list[lastIndex];

			if (_cleanupOnPop)
			{
				_list[lastIndex] = _default;
			}

			if (lastIndex == _startIndex) { IsEmpty = true; }

			_endIndex -= 1;
			if (_endIndex == 0 && _list.Count == Capacity) // Wrapping around
			{
				_endIndex = _list.Count;
			}
			
			return itemToReturn;
		}

		public T Peek()
		{
			if (IsEmpty) { throw new System.ArgumentException("Trying to peek into empty ringbuffer."); }
			return _list[_endIndex - 1];
		}

		public void UpdateLastEntry(System.Func<T, T> updateMethod)
		{
			if (IsEmpty) { throw new System.ArgumentException("Trying to update last entry in empty ringbuffer."); }

			if (updateMethod != null)
			{
				var lastIndex = _endIndex - 1;
				_list[lastIndex] = updateMethod(_list[lastIndex]);
			}
		}

		public void Clear(bool deallocate = false)
		{
			if (deallocate)
			{
				_list.Clear();
			}

			_startIndex = (_list.Count < Capacity) ? _list.Count : 0;
			_endIndex = _startIndex;
			IsEmpty = true;
		}

		public void ToArray(T[] array, bool reverse = false)
		{
			if (array == null || array.Length < _list.Capacity)
			{
				throw new System.ArgumentException("Trying to copy ringbuffer to array of mismatching size.");
			}

			if (!IsEmpty)
			{
				var copyIndex = _startIndex;
				var destinationIndex = reverse ? array.Length - 1 : 0;
				var destIndexIncrement = reverse ? -1 : 1;

				if (_startIndex < _endIndex)
				{
					for ( ;copyIndex < _endIndex; ++copyIndex)
					{
						array[destinationIndex] = _list[copyIndex];
						destinationIndex += destIndexIncrement;
					}
				}
				else
				{
					for ( ; copyIndex < _list.Count; ++copyIndex)
					{
						array[destinationIndex] = _list[copyIndex];
						destinationIndex += destIndexIncrement;
					}

					for (copyIndex = 0; copyIndex < _endIndex; ++copyIndex)
					{
						array[destinationIndex] = _list[copyIndex];
						destinationIndex += destIndexIncrement;
					}
				}
			}
		}

		public override string ToString()
		{
			var sb = new System.Text.StringBuilder();

			sb.AppendFormat("Start: {0}, end: {1}, count: {2}\n", _startIndex, _endIndex, Count);
			foreach (var item in _list)
			{
				sb.AppendFormat("[{0}] ", item.ToString());
			}

			return sb.ToString();
		}

		private void IncrementStartIndex()
		{
			if (++_startIndex == Capacity)
			{
				_startIndex = 0;
			}
		}

		private int FindInsertionIndex()
		{
			var newLastIndexCandidate = _endIndex;
			if (newLastIndexCandidate == Capacity)
			{
				newLastIndexCandidate = 0;
			}
			return newLastIndexCandidate;
		}
	}
}