﻿// The MIT License (MIT)
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

using UnityEngine;

namespace SBR {
    /// <summary>
    /// Used to store information about Damage being applied.
    /// This class can be extended to implement custom damage types, such as elemental damage.
    /// These damage types can then be processed via the ProcessDamage event in Health.
    /// </summary>
    public class Damage {
        /// <summary>
        /// Create a new Damage instance.
        /// </summary>
        /// <param name="amount">Amount of damage.</param>
        /// <param name="dealer">GameObject that caused the damage.</param>
        /// <param name="method">Method for dealing the damage, such as a weapon.</param>
        public Damage(float amount, GameObject dealer, object method = null) {
            OriginalAmount = amount;
            Amount = amount;
            Dealer = dealer;
            Method = method;
        }

        /// <summary>
        /// Non-modified amount of damage.
        /// </summary>
        public float OriginalAmount { get; }

        /// <summary>
        /// Amount of damage that is applied.
        /// </summary>
        public float Amount { get; set; }

        /// <summary>
        /// GameObject that caused the damage.
        /// </summary>
        public GameObject Dealer { get; set; }

        /// <summary>
        /// Method for dealing the damage, such as a weapon.
        /// </summary>
        public object Method { get; set; }
    }
}
