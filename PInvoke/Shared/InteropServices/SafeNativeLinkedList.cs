using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Vanara.Extensions;

namespace Vanara.InteropServices
{
	/// <summary>A safe unmanaged linked list of structures allocated on the global heap.</summary>
	/// <typeparam name="TElem">The type of the list elements.</typeparam>
	/// <typeparam name="TMem">The type of memory allocation to use.</typeparam>
	public class SafeNativeLinkedList<TElem, TMem> : SafeMemoryHandle<TMem>, IReadOnlyList<TElem> where TElem : struct where TMem : IMemoryMethods, new()
	{
		/// <summary>Initializes a new instance of the <see cref="SafeNativeLinkedList{TElem, TMem}"/> class.</summary>
		/// <param name="ptr">The handle.</param>
		/// <param name="size">The size of memory allocated to the handle, in bytes.</param>
		/// <param name="ownsHandle">if set to <c>true</c> if this class is responsible for freeing the memory on disposal.</param>
		/// <param name="getNextMethod">The method to use to get the next item in the list.</param>
		public SafeNativeLinkedList(IntPtr ptr, int size, bool ownsHandle, Func<TElem, IntPtr> getNextMethod) : base(ptr, size, ownsHandle) { GetNextMethod = getNextMethod; }

		/// <summary>Initializes a new instance of the <see cref="SafeNativeLinkedList{TElem, TMem}"/> class.</summary>
		/// <param name="byteCount">The number of bytes to allocate for this new array.</param>
		/// <param name="getNextMethod">The method to use to get the next item in the list.</param>
		public SafeNativeLinkedList(int byteCount, Func<TElem, IntPtr> getNextMethod) : base(byteCount) { GetNextMethod = getNextMethod; }

		/// <summary>Gets or sets the method to use to get the next item in the list.</summary>
		/// <value>The method to get the next value. It should return <see cref="IntPtr.Zero"/> if there are no more items.</value>
		public Func<TElem, IntPtr> GetNextMethod { get; set; }

		/// <summary>Gets the number of elements contained in the <see cref="SafeNativeLinkedList{TElem, TMem}"/>.</summary>
		public int Count => IsInvalid ? 0 : Items.Count();

		/// <summary>Enumerates the elements.</summary>
		/// <returns>An enumeration of values from the pointer.</returns>
		protected virtual IEnumerable<TElem> Items => handle.LinkedListToIEnum(GetNextMethod);

		/// <summary>Gets or sets the <typeparamref name="TElem"/> value at the specified index.</summary>
		/// <value>The <typeparamref name="TElem"/> value.</value>
		/// <param name="index">The index.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException">index or index</exception>
		public TElem this[int index] => Items.ElementAt(index);

		/// <summary>Determines whether this instance contains the object.</summary>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
		/// <returns>
		/// true if <paramref name="item"/> is found in the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false.
		/// </returns>
		public bool Contains(TElem item) => Items.Contains(item);

		/// <summary>Returns an enumerator that iterates through the collection.</summary>
		/// <returns>A <see cref="IEnumerator{TElem}"/> that can be used to iterate through the collection.</returns>
		public IEnumerator<TElem> GetEnumerator() => Items.GetEnumerator();

		/// <summary>Returns an enumerator that iterates through a collection.</summary>
		/// <returns>An <see cref="IEnumerator"/> object that can be used to iterate through the collection.</returns>
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}