using SBR;
using System.Collections;
using System.Collections.Generic;
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
    public Vector3 LocalScaleRatio => MathUtil.Divide(Transform.localScale, LastLocalScale);
    public Vector3 ScaleRatio => MathUtil.Divide(Transform.lossyScale, LastScale);

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
