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
using SBR;

public class CharacterChannels : SBR.Channels {

    private Vector3 _movement;
    public Vector3 movement {
        get { return _movement; }
        set {
            _movement = value.sqrMagnitude > 1f ? value.normalized * 1f : value;
        }
    }

    private Quaternion _rotation;
    public Quaternion rotation {
        get { return _rotation; }
        set {
            _rotation = value;
        }
    }

    private bool _jump;
    public bool jump {
        get { return _jump; }
        set {
            _jump = value;
        }
    }


    public override void ClearInput(bool force = false) {
        base.ClearInput(force);
        _movement = new Vector3(0f, 0f, 0f);
        if (force) _rotation = new Quaternion(0f, 0f, 0f, 1f);
        if (force) _jump = false;

    }
}
