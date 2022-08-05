// The MIT License (MIT)
// 
// Copyright (c) 2022-present Vincent Miller
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;

using UnityEngine;

namespace SBR {
    /// <summary>
    /// Struct for storing two values.
    /// </summary>
    /// <typeparam name="T1">Type of the first value.</typeparam>
    /// <typeparam name="T2">Type of the second value.</typeparam>
    [Serializable]
    public struct Pair<T1, T2> {
        [SerializeField] private T1 _item1;

        [SerializeField] private T2 _item2;

        /// <summary>
        /// The first value.
        /// </summary>
        public T1 Item1 {
            get => _item1;
            set => _item1 = value;
        }

        /// <summary>
        /// The second value.
        /// </summary>
        public T2 Item2 {
            get => _item2;
            set => _item2 = value;
        }

        /// <summary>
        /// Construct a new Pair.
        /// </summary>
        /// <param name="item1">The first value.</param>
        /// <param name="item2">The second value.</param>
        public Pair(T1 item1, T2 item2) {
            _item1 = item1;
            _item2 = item2;
        }

        public override int GetHashCode() {
            return _item1.GetHashCode() ^ _item2.GetHashCode();
        }

        public override bool Equals(object obj) {
            if (!(obj is Pair<T1, T2>)) {
                return false;
            }

            var o = (Pair<T1, T2>)obj;

            return _item1.Equals(o._item1) && _item2.Equals(o._item2);
        }

        public static bool operator ==(Pair<T1, T2> lhs, Pair<T1, T2> rhs) {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(Pair<T1, T2> lhs, Pair<T1, T2> rhs) {
            return !lhs.Equals(rhs);
        }
    }

    /// <summary>
    /// A pair of two values whose order doesn't matter.
    /// Equality and HashCode are not affected by the order.
    /// </summary>
    /// <typeparam name="T">The type of the values.</typeparam>
    [Serializable]
    public struct UnorderedPair<T> {
        [SerializeField] private T _item1;

        [SerializeField] private T _item2;

        /// <summary>
        /// The first value.
        /// </summary>
        public T Item1 => _item1;

        /// <summary>
        /// The second value.
        /// </summary>
        public T Item2 => _item2;

        /// <summary>
        /// Construct a new UnorderedPair.
        /// </summary>
        /// <param name="t1">The first value.</param>
        /// <param name="t2">The second value.</param>
        public UnorderedPair(T t1, T t2) {
            _item1 = t1;
            _item2 = t2;
        }

        /// <summary>
        /// Return true if either value is equal to the given value.
        /// </summary>
        /// <param name="item">Value to compare to.</param>
        /// <returns>Whether this contains item.</returns>
        public bool Contains(T item) {
            return _item1.Equals(item) || _item2.Equals(item);
        }

        /// <summary>
        /// Return true if this contains at least one of the values in the given UnorederedPair.
        /// </summary>
        /// <param name="other">Pair to compare to.</param>
        /// <returns>True if adjacent to other.</returns>
        public bool Adjacent(UnorderedPair<T> other) {
            return Contains(other._item1) || Contains(other._item2);
        }

        /// <summary>
        /// Find the value in this pair which is not equal to the given value.
        /// </summary>
        /// <exception cref="System.ArgumentException">If neither or both values in this pair are equal to the given value.</exception>
        /// <param name="item">The item to compare to.</param>
        /// <returns>A value in this pair not equal to the given value.</returns>
        public T Not(T item) {
            if (_item1.Equals(item) != _item2.Equals(item)) {
                return _item1.Equals(item) ? _item2 : _item1;
            }

            throw new ArgumentException("Argument not either element!");
        }

        public override int GetHashCode() {
            return _item1.GetHashCode() ^ _item2.GetHashCode();
        }

        public override bool Equals(object obj) {
            if (!(obj is UnorderedPair<T>)) {
                return false;
            }

            var o = (UnorderedPair<T>)obj;

            return (_item1.Equals(o._item1) && _item2.Equals(o._item2)) || (_item1.Equals(o._item2) && _item2.Equals(o._item1));
        }

        public static bool operator ==(UnorderedPair<T> lhs, UnorderedPair<T> rhs) {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(UnorderedPair<T> lhs, UnorderedPair<T> rhs) {
            return !lhs.Equals(rhs);
        }
    }
}