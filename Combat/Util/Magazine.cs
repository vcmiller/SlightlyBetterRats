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

namespace SBR {
    /// <summary>
    /// Used to create an action that can only happen a set number of times before it must recharge,
    /// such as bullets in a magazine that must be reloaded.
    /// </summary>
    public class Magazine {
        public ExpirationTimer reloadTimer { get; private set; }

        /// <summary>
        /// How many uses remain before a reload is necessary.
        /// </summary>
        public int remainingShots { get; private set; }

        /// <summary>
        /// How many times it can be used per reload.
        /// </summary>
        public int clipSize { get; private set; }

        /// <summary>
        /// Whether it can currently be used.
        /// </summary>
        public bool canFire => remainingShots > 0 && reloadTimer.expired;

        /// <summary>
        /// Whether is currently reloading.
        /// </summary>
        public bool reloading => !reloadTimer.expired;

        /// <summary>
        /// Construct a new Magazine with given size and reload time.
        /// </summary>
        /// <param name="size">Number of uses before reload.</param>
        /// <param name="reloadTime">Time it takes to reload.</param>
        public Magazine(int size, float reloadTime) {
            reloadTimer = new ExpirationTimer(reloadTime);
            remainingShots = size;
            clipSize = size;
        }

        /// <summary>
        /// Manually reload.
        /// </summary>
        public void Reload() {
            if (remainingShots < clipSize) {
                remainingShots = clipSize;
                reloadTimer.Set();
            }
        }

        /// <summary>
        /// Use the Magazine once if possible, and return true.
        /// Otherwise, return false.
        /// If remaining uses reaches 0, reload.
        /// </summary>
        /// <returns>Whether it could be used.</returns>
        public bool Fire() {
            if (canFire) {
                remainingShots--;

                if (remainingShots == 0) {
                    Reload();
                }

                return true;
            } else {
                return false;
            }
        }
    }
}