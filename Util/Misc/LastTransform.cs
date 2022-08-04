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

using Infohazard.Core.Runtime;
using UnityEngine;

public struct LastTransform {
    public Vector3 LastLocalPosition { get; private set; }
    public Vector3 LastPosition { get; private set; }
    public Quaternion LastLocalRotation { get; private set; }
    public Quaternion LastRotation { get; private set; }
    public Vector3 LastLocalScale { get; private set; }
    public Vector3 LastScale{ get; private set; }
    public Transform Transform { get; private set; }

    public bool LocalPositionChanged => Transform.localPosition != LastLocalPosition;
    public bool PositionChanged => Transform.position != LastPosition;
    public bool LocalRotationChanged => Transform.localRotation != LastLocalRotation;
    public bool RotationChanged => Transform.rotation != LastRotation;
    public bool LocalScaleChanged => Transform.localScale != LastLocalScale;
    public bool ScaleChanged => Transform.lossyScale != LastScale;
    public bool LocalChanged => LocalPositionChanged || LocalRotationChanged || LocalScaleChanged;
    public bool Changed => LocalChanged || PositionChanged || RotationChanged || ScaleChanged;

    public Vector3 LocalPositionOffset => Transform.localPosition - LastLocalPosition;
    public Vector3 PositionOffset => Transform.position - LastPosition;
    public Quaternion LocalRotationOffset => Quaternion.Inverse(LastLocalRotation) * Transform.localRotation;
    public Quaternion RotationOffset => Quaternion.Inverse(LastRotation) * Transform.rotation;
    public Vector3 LocalScaleRatio => MathUtility.Divide(Transform.localScale, LastLocalScale);
    public Vector3 ScaleRatio => MathUtility.Divide(Transform.lossyScale, LastScale);

    public LastTransform(Transform transform) : this() {
        Transform = transform;
        Update();
    }

    public void Update() {
        LastLocalPosition = Transform.localPosition;
        LastPosition = Transform.position;
        LastLocalRotation = Transform.rotation;
        LastRotation = Transform.rotation;
        LastLocalScale = Transform.localScale;
        LastScale = Transform.lossyScale;
    }

    public void RevertLocal() {
        Transform.localPosition = LastLocalPosition;
        Transform.localRotation = LastLocalRotation;
        Transform.localScale = LastLocalScale;
        Update();
    }

    public void RevertGlobal() {
        Transform.position = LastPosition;
        Transform.rotation = LastRotation;
        Transform.localScale = LastLocalScale;
        Update();
    }
}
