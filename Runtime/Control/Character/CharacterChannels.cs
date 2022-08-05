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

using UnityEngine;

public class CharacterChannels : SBR.Channels {

    private Vector3 _Movement;
    public Vector3 Movement {
        get { return _Movement; }
        set {
            _Movement = value.sqrMagnitude > 1f ? value.normalized * 1f : value;
        }
    }

    private Quaternion _Rotation;
    public Quaternion Rotation {
        get { return _Rotation; }
        set {
            _Rotation = value;
        }
    }

    private bool _Jump;
    public bool Jump {
        get { return _Jump; }
        set {
            _Jump = value;
        }
    }


    public override void ClearInput(bool force = false) {
        base.ClearInput(force);
        _Movement = new Vector3(0f, 0f, 0f);
        if (force) _Rotation = new Quaternion(0f, 0f, 0f, 1f);
        if (force) _Jump = false;

    }
}
