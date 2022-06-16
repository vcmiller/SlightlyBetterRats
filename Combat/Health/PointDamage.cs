// MIT License
// 
// Copyright (c) 2020 Vincent Miller
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
    public class PointDamage : Damage {
        /// <summary>
        /// Create a new PointDamage instance.
        /// </summary>
        /// <param name="amount">Amount of damage.</param>
        /// <param name="dealer">The GameObject responsible.</param>
        /// <param name="point">Location of the damage.</param>
        /// <param name="direction">Direction of the damage.</param>
        /// <param name="force">Force applied by the bullet.</param>
        /// <param name="hitObject"></param>
        public PointDamage(float amount, GameObject dealer, Vector3 point, Vector3 direction, float force, GameObject hitObject) : base(amount, dealer) {
            Point = point;
            Direction = direction.normalized;
            Force = force;
            HitObject = hitObject;
        }

        /// <summary>
        /// Location at which the damage is applied.
        /// </summary>
        public Vector3 Point { get; }

        /// <summary>
        /// Direction of the damage (such as the normalized velocity of a bullet).
        /// </summary>
        public Vector3 Direction { get; }

        /// <summary>
        /// Force that is applied by the bullet in the given direction.
        /// </summary>
        public float Force { get; }

        public GameObject HitObject { get; }
    }

}