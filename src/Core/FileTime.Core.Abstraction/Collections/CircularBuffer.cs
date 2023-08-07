// https://github.com/ASolomatin/CircularBuffer-CSharp/blob/master/CircularBuffer/

using System.Collections;

namespace FileTime.Core.Collections;
#if (NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER)

/// <summary>
/// Reference struct that contains two read only spans of type T
/// </summary>
public ref struct SpanTuple<T>
{
    /// <summary>
    /// First span
    /// </summary>
    public ReadOnlySpan<T> A { get; }

    /// <summary>
    /// Second span
    /// </summary>
    public ReadOnlySpan<T> B { get; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="a">First span</param>
    /// <param name="b">Second span</param>
    public SpanTuple(ReadOnlySpan<T> a, ReadOnlySpan<T> b)
    {
        A = a;
        B = b;
    }

    /// <summary>
    /// Deconstructs the current <see cref="SpanTuple&lt;T&gt;"/>
    /// </summary>
    /// <param name="a">First span target</param>
    /// <param name="b">Second span target</param>
    public void Deconstruct(out ReadOnlySpan<T> a, out ReadOnlySpan<T> b)
    {
        a = A;
        b = B;
    }
}

#endif


/// <summary>
/// Circular buffer.
///
/// When writing to a full buffer:
/// PushBack -> removes this[0] / Front()
/// PushFront -> removes this[Size-1] / Back()
///
/// this implementation is inspired by
/// http://www.boost.org/doc/libs/1_53_0/libs/circular_buffer/doc/circular_buffer.html
/// because I liked their interface.
/// </summary>
public interface ICircularBuffer<T> : IReadOnlyCollection<T>
{
    /// <summary>
    /// Index access to elements in buffer.
    /// Index does not loop around like when adding elements,
    /// valid interval is [0;Size[
    /// </summary>
    /// <param name="index">Index of element to access.</param>
    /// <exception cref="IndexOutOfRangeException">Thrown when index is outside of [; Size[ interval.</exception>
    T this[int index] { get; set; }

    /// <summary>
    /// Maximum capacity of the buffer. Elements pushed into the buffer after
    /// maximum capacity is reached (IsFull = true), will remove an element.
    /// </summary>
    int Capacity { get; }

    /// <summary>
    /// Boolean indicating if Circular is at full capacity.
    /// Adding more elements when the buffer is full will
    /// cause elements to be removed from the other end
    /// of the buffer.
    /// </summary>
    bool IsFull { get; }

    /// <summary>
    /// True if has no elements.
    /// </summary>
    bool IsEmpty { get; }

    /// <summary>
    /// Current buffer size (the number of elements that the buffer has).
    /// </summary>
    [Obsolete("Use Count property instead")]
    int Size { get; }

    /// <summary>
    /// Element at the back of the buffer - this[Size - 1].
    /// </summary>
    /// <returns>The value of the element of type T at the back of the buffer.</returns>
    [Obsolete("Use Last() method instead")]
    T Back();

    /// <summary>
    /// Element at the front of the buffer - this[0].
    /// </summary>
    /// <returns>The value of the element of type T at the front of the buffer.</returns>
    [Obsolete("Use First() method instead")]
    T Front();

    /// <summary>
    /// Element at the back of the buffer - this[Size - 1].
    /// </summary>
    /// <returns>The value of the element of type T at the back of the buffer.</returns>
    T First();

    /// <summary>
    /// Element at the front of the buffer - this[0].
    /// </summary>
    /// <returns>The value of the element of type T at the front of the buffer.</returns>
    T Last();

    /// <summary>
    /// Clears the contents of the array. Size = 0, Capacity is unchanged.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    void Clear();

    /// <summary>
    /// Removes the element at the back of the buffer. Decreasing the 
    /// Buffer size by 1.
    /// </summary>
    T PopBack();

    /// <summary>
    /// Removes the element at the front of the buffer. Decreasing the 
    /// Buffer size by 1.
    /// </summary>
    T PopFront();

    /// <summary>
    /// Pushes a new element to the back of the buffer. Back()/this[Size-1]
    /// will now return this element.
    ///
    /// When the buffer is full, the element at Front()/this[0] will be
    /// popped to allow for this new element to fit.
    /// </summary>
    /// <param name="item">Item to push to the back of the buffer</param>
    void PushBack(T item);

    /// <summary>
    /// Pushes a new element to the front of the buffer. Front()/this[0]
    /// will now return this element.
    ///
    /// When the buffer is full, the element at Back()/this[Size-1] will be
    /// popped to allow for this new element to fit.
    /// </summary>
    /// <param name="item">Item to push to the front of the buffer</param>
    void PushFront(T item);

    /// <summary>
    /// Copies the buffer contents to an array, according to the logical
    /// contents of the buffer (i.e. independent of the internal
    /// order/contents)
    /// </summary>
    /// <returns>A new array with a copy of the buffer contents.</returns>
    T[] ToArray();

    /// <summary>
    /// Copies the buffer contents to the array, according to the logical
    /// contents of the buffer (i.e. independent of the internal
    /// order/contents)
    /// </summary>
    /// <param name="array">The array that is the destination of the elements copied from the current buffer.</param>
    void CopyTo(T[] array);

    /// <summary>
    /// Copies the buffer contents to the array, according to the logical
    /// contents of the buffer (i.e. independent of the internal
    /// order/contents)
    /// </summary>
    /// <param name="array">The array that is the destination of the elements copied from the current buffer.</param>
    /// <param name="index">A 32-bit integer that represents the index in array at which copying begins.</param>
    void CopyTo(T[] array, int index);

    /// <summary>
    /// Copies the buffer contents to the array, according to the logical
    /// contents of the buffer (i.e. independent of the internal
    /// order/contents)
    /// </summary>
    /// <param name="array">The array that is the destination of the elements copied from the current buffer.</param>
    /// <param name="index">A 64-bit integer that represents the index in array at which copying begins.</param>
    void CopyTo(T[] array, long index);

#if (NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER)

    /// <summary>
    /// Copies the buffer contents to the <see cref="Memory&lt;T&gt;"/>, according to the logical
    /// contents of the buffer (i.e. independent of the internal
    /// order/contents)
    /// </summary>
    /// <param name="memory">The memory that is the destination of the elements copied from the current buffer.</param>
    void CopyTo(Memory<T> memory);

    /// <summary>
    /// Copies the buffer contents to the <see cref="Span&lt;T&gt;"/>, according to the logical
    /// contents of the buffer (i.e. independent of the internal
    /// order/contents)
    /// </summary>
    /// <param name="span">The span that is the destination of the elements copied from the current buffer.</param>
    void CopyTo(Span<T> span);

#endif

    /// <summary>
    /// Get the contents of the buffer as 2 ArraySegments.
    /// Respects the logical contents of the buffer, where
    /// each segment and items in each segment are ordered
    /// according to insertion.
    ///
    /// Fast: does not copy the array elements.
    /// Useful for methods like <c>Send(IList&lt;ArraySegment&lt;Byte&gt;&gt;)</c>.
    ///
    /// <remarks>Segments may be empty.</remarks>
    /// </summary>
    /// <returns>An IList with 2 segments corresponding to the buffer content.</returns>
    IList<ArraySegment<T>> ToArraySegments();

#if (NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER)

    /// <summary>
    /// Get the contents of the buffer as ref struct with 2 read only spans (<see cref="SpanTuple&lt;T&gt;"/>).
    /// Respects the logical contents of the buffer, where
    /// each segment and items in each segment are ordered
    /// according to insertion.
    ///
    /// <remarks>Segments may be empty.</remarks>
    /// </summary>
    /// <returns>A <see cref="SpanTuple&lt;T&gt;"/> with 2 read only spans corresponding to the buffer content.</returns>
    SpanTuple<T> ToSpan();

    /// <summary>
    /// Get the contents of the buffer as tuple with 2 <see cref="ReadOnlyMemory&lt;T&gt;"/>.
    /// Respects the logical contents of the buffer, where
    /// each segment and items in each segment are ordered
    /// according to insertion.
    ///
    /// <remarks>Segments may be empty.</remarks>
    /// </summary>
    /// <returns>A tuple with 2 read only spans corresponding to the buffer content.</returns>
    (ReadOnlyMemory<T> A, ReadOnlyMemory<T> B) ToMemory();

#endif
}

/// <inheritdoc/>
public class CircularBuffer<T> : ICircularBuffer<T>
{
    private readonly T[] _buffer;

    /// <summary>
    /// The _start. Index of the first element in buffer.
    /// </summary>
    private int _start;

    /// <summary>
    /// The _end. Index after the last element in the buffer.
    /// </summary>
    private int _end;

    /// <summary>
    /// The _size. Buffer size.
    /// </summary>
    private int _size;

    /// <summary>
    /// Initializes a new instance of the <see cref="CircularBuffer{T}"/> class.
    ///
    /// </summary>
    /// <param name='capacity'>
    /// Buffer capacity. Must be positive.
    /// </param>
    public CircularBuffer(int capacity)
        : this(capacity, new T[] { })
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CircularBuffer{T}"/> class.
    ///
    /// </summary>
    /// <param name='capacity'>
    /// Buffer capacity. Must be positive.
    /// </param>
    /// <param name='items'>
    /// Items to fill buffer with. Items length must be less than capacity.
    /// Suggestion: use Skip(x).Take(y).ToArray() to build this argument from
    /// any enumerable.
    /// </param>
    public CircularBuffer(int capacity, T[] items)
    {
        if (capacity < 1)
        {
            throw new ArgumentException(
                "Circular buffer cannot have negative or zero capacity.", nameof(capacity));
        }

        if (items == null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        if (items.Length > capacity)
        {
            throw new ArgumentException(
                "Too many items to fit circular buffer", nameof(items));
        }

        _buffer = new T[capacity];

        Array.Copy(items, _buffer, items.Length);
        _size = items.Length;

        _start = 0;
        _end = _size == capacity ? 0 : _size;
    }

    /// <inheritdoc/>
    public int Capacity
    {
        get { return _buffer.Length; }
    }

    /// <inheritdoc/>
    public bool IsFull
    {
        get { return Count == Capacity; }
    }

    /// <inheritdoc/>
    public bool IsEmpty
    {
        get { return Count == 0; }
    }

    /// <inheritdoc/>
    [Obsolete("Use Count property instead")]
    public int Size => Count;

    /// <inheritdoc/>
    public int Count
    {
        get { return _size; }
    }

    /// <inheritdoc/>
    [Obsolete("Use First() method instead")]
    public T Front() => First();

    /// <inheritdoc/>
    [Obsolete("Use Last() method instead")]
    public T Back() => Last();

    /// <inheritdoc/>
    public T First()
    {
        ThrowIfEmpty();
        return _buffer[_start];
    }

    /// <inheritdoc/>
    public T Last()
    {
        ThrowIfEmpty();
        return _buffer[(_end != 0 ? _end : Capacity) - 1];
    }

    /// <inheritdoc/>
    public T this[int index]
    {
        get
        {
            if (IsEmpty)
            {
                throw new IndexOutOfRangeException(string.Format("Cannot access index {0}. Buffer is empty", index));
            }

            if (index >= _size)
            {
                throw new IndexOutOfRangeException(string.Format("Cannot access index {0}. Buffer size is {1}", index, _size));
            }

            int actualIndex = InternalIndex(index);
            return _buffer[actualIndex];
        }
        set
        {
            if (IsEmpty)
            {
                throw new IndexOutOfRangeException(string.Format("Cannot access index {0}. Buffer is empty", index));
            }

            if (index >= _size)
            {
                throw new IndexOutOfRangeException(string.Format("Cannot access index {0}. Buffer size is {1}", index, _size));
            }

            int actualIndex = InternalIndex(index);
            _buffer[actualIndex] = value;
        }
    }

    /// <inheritdoc/>
    public void PushBack(T item)
    {
        if (IsFull)
        {
            _buffer[_end] = item;
            Increment(ref _end);
            _start = _end;
        }
        else
        {
            _buffer[_end] = item;
            Increment(ref _end);
            ++_size;
        }
    }

    /// <inheritdoc/>
    public void PushFront(T item)
    {
        if (IsFull)
        {
            Decrement(ref _start);
            _end = _start;
            _buffer[_start] = item;
        }
        else
        {
            Decrement(ref _start);
            _buffer[_start] = item;
            ++_size;
        }
    }

    /// <inheritdoc/>
    public T PopBack()
    {
        ThrowIfEmpty("Cannot take elements from an empty buffer.");
        Decrement(ref _end);
        var value = _buffer[_start];
        _buffer[_end] = default(T);
        --_size;
        return value;
    }

    /// <inheritdoc/>
    public T PopFront()
    {
        ThrowIfEmpty("Cannot take elements from an empty buffer.");
        var value = _buffer[_start];
        _buffer[_start] = default(T);
        Increment(ref _start);
        --_size;
        return value;
    }

    /// <inheritdoc/>
    public void Clear()
    {
        // to clear we just reset everything.
        _start = 0;
        _end = 0;
        _size = 0;
        Array.Clear(_buffer, 0, _buffer.Length);
    }

    /// <inheritdoc/>
    public T[] ToArray()
    {
        T[] newArray = new T[Count];
        CopyToInternal(newArray, 0);
        return newArray;
    }

    /// <inheritdoc/>
    public void CopyTo(T[] array)
    {
        if (array is null)
            throw new ArgumentNullException(nameof(array));

        if (array.Length < _size)
            throw new ArgumentException($"The number of elements in the source {nameof(CircularBuffer<int>)} is greater than the available " +
                                        "number of elements of the destination array.", nameof(array));

        CopyToInternal(array, 0);
    }

    /// <inheritdoc/>
    public void CopyTo(T[] array, int index)
    {
        if (array is null)
            throw new ArgumentNullException(nameof(array));

        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index), $"{nameof(index)} is less than the lower bound of {nameof(array)}.");

        if (array.Length - index < _size)
            throw new ArgumentException($"The number of elements in the source {nameof(CircularBuffer<int>)} is greater than the available " +
                                        "number of elements from index to the end of the destination array.", nameof(array));

        CopyToInternal(array, index);
    }

    /// <inheritdoc/>
    public void CopyTo(T[] array, long index)
    {
        if (array is null)
            throw new ArgumentNullException(nameof(array));

        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index), $"{nameof(index)} is less than the lower bound of {nameof(array)}.");

        if (array.LongLength - index < _size)
            throw new ArgumentException($"The number of elements in the source {nameof(CircularBuffer<int>)} is greater than the available " +
                                        "number of elements from index to the end of the destination array.", nameof(array));

        CopyToInternal(array, index);
    }

#if (NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER)

    /// <inheritdoc/>
    public void CopyTo(Memory<T> memory)
    {
        if (memory.Length < _size)
            throw new ArgumentException($"The number of elements in the source {nameof(CircularBuffer<int>)} is greater than the available " +
                                        "number of elements of the destination Memory.", nameof(memory));

        CopyToInternal(memory);
    }

    /// <inheritdoc/>
    public void CopyTo(Span<T> span)
    {
        if (span.Length < _size)
            throw new ArgumentException($"The number of elements in the source {nameof(CircularBuffer<int>)} is greater than the available " +
                                        "number of elements of the destination Span.", nameof(span));

        CopyToInternal(span);
    }

#endif

    /// <inheritdoc/>
    public IList<ArraySegment<T>> ToArraySegments()
    {
        return new[] {ArrayOne(), ArrayTwo()};
    }

#if (NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER)

    /// <inheritdoc/>
    public SpanTuple<T> ToSpan()
    {
        return new SpanTuple<T>(SpanOne(), SpanTwo());
    }

    /// <inheritdoc/>
    public (ReadOnlyMemory<T> A, ReadOnlyMemory<T> B) ToMemory()
    {
        return (MemoryOne(), MemoryTwo());
    }

#endif

    #region IEnumerable<T> implementation

    /// <summary>
    /// Returns an enumerator that iterates through this buffer.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate this collection.</returns>
    public IEnumerator<T> GetEnumerator()
    {
        var segments = ToArraySegments();
        foreach (ArraySegment<T> segment in segments)
        {
            for (int i = 0; i < segment.Count; i++)
            {
                yield return segment.Array[segment.Offset + i];
            }
        }
    }

    #endregion

    #region IEnumerable implementation

    IEnumerator IEnumerable.GetEnumerator()
    {
        return (IEnumerator) GetEnumerator();
    }

    #endregion

#if (NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER)

    private void CopyToInternal(T[] array, int index)
    {
        CopyToInternal(array.AsSpan(index));
    }

    private void CopyToInternal(Memory<T> memory)
    {
        CopyToInternal(memory.Span);
    }

    private void CopyToInternal(Span<T> span)
    {
        var segments = ToSpan();
        segments.A.CopyTo(span);
        segments.B.CopyTo(span.Slice(segments.A.Length));
    }

#else
        private void CopyToInternal(T[] array, int index)
        {
            var segments = ToArraySegments();
            var segment = segments[0];
            Array.Copy(segment.Array, segment.Offset, array, index, segment.Count);
            index += segment.Count;
            segment = segments[1];
            Array.Copy(segment.Array, segment.Offset, array, index, segment.Count);
        }

#endif

    private void CopyToInternal(T[] array, long index)
    {
        var segments = ToArraySegments();
        var segment = segments[0];
        Array.Copy(segment.Array, segment.Offset, array, index, segment.Count);
        index += segment.Count;
        segment = segments[1];
        Array.Copy(segment.Array, segment.Offset, array, index, segment.Count);
    }

    private void ThrowIfEmpty(string message = "Cannot access an empty buffer.")
    {
        if (IsEmpty)
        {
            throw new InvalidOperationException(message);
        }
    }

    /// <summary>
    /// Increments the provided index variable by one, wrapping
    /// around if necessary.
    /// </summary>
    /// <param name="index"></param>
    private void Increment(ref int index)
    {
        if (++index == Capacity)
        {
            index = 0;
        }
    }

    /// <summary>
    /// Decrements the provided index variable by one, wrapping
    /// around if necessary.
    /// </summary>
    /// <param name="index"></param>
    private void Decrement(ref int index)
    {
        if (index == 0)
        {
            index = Capacity;
        }

        index--;
    }

    /// <summary>
    /// Converts the index in the argument to an index in <code>_buffer</code>
    /// </summary>
    /// <returns>
    /// The transformed index.
    /// </returns>
    /// <param name='index'>
    /// External index.
    /// </param>
    private int InternalIndex(int index)
    {
        return _start + (index < (Capacity - _start) ? index : index - Capacity);
    }

    // doing ArrayOne and ArrayTwo methods returning ArraySegment<T> as seen here: 
    // http://www.boost.org/doc/libs/1_37_0/libs/circular_buffer/doc/circular_buffer.html#classboost_1_1circular__buffer_1957cccdcb0c4ef7d80a34a990065818d
    // http://www.boost.org/doc/libs/1_37_0/libs/circular_buffer/doc/circular_buffer.html#classboost_1_1circular__buffer_1f5081a54afbc2dfc1a7fb20329df7d5b
    // should help a lot with the code.

    #region Array items easy access.

    // The array is composed by at most two non-contiguous segments, 
    // the next two methods allow easy access to those.

    private ArraySegment<T> ArrayOne()
    {
        if (IsEmpty)
        {
            return new ArraySegment<T>(new T[0]);
        }
        else if (_start < _end)
        {
            return new ArraySegment<T>(_buffer, _start, _end - _start);
        }
        else
        {
            return new ArraySegment<T>(_buffer, _start, _buffer.Length - _start);
        }
    }

    private ArraySegment<T> ArrayTwo()
    {
        if (IsEmpty)
        {
            return new ArraySegment<T>(new T[0]);
        }
        else if (_start < _end)
        {
            return new ArraySegment<T>(_buffer, _end, 0);
        }
        else
        {
            return new ArraySegment<T>(_buffer, 0, _end);
        }
    }

#if (NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER)

    private Span<T> SpanOne()
    {
        if (IsEmpty)
        {
            return Span<T>.Empty;
        }
        else if (_start < _end)
        {
            return _buffer.AsSpan(_start, _end - _start);
        }
        else
        {
            return _buffer.AsSpan(_start, _buffer.Length - _start);
        }
    }

    private Span<T> SpanTwo()
    {
        if (IsEmpty)
        {
            return Span<T>.Empty;
        }
        else if (_start < _end)
        {
            return _buffer.AsSpan(_end, 0);
        }
        else
        {
            return _buffer.AsSpan(0, _end);
        }
    }

    private Memory<T> MemoryOne()
    {
        if (IsEmpty)
        {
            return Memory<T>.Empty;
        }
        else if (_start < _end)
        {
            return _buffer.AsMemory(_start, _end - _start);
        }
        else
        {
            return _buffer.AsMemory(_start, _buffer.Length - _start);
        }
    }

    private Memory<T> MemoryTwo()
    {
        if (IsEmpty)
        {
            return Memory<T>.Empty;
        }
        else if (_start < _end)
        {
            return _buffer.AsMemory(_end, 0);
        }
        else
        {
            return _buffer.AsMemory(0, _end);
        }
    }

#endif

    #endregion
}