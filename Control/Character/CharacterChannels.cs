using UnityEngine;
using SBR;

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
