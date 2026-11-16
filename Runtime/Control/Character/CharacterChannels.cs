using UnityEngine;
using SBR;
using System.Collections.Generic;

public class CharacterChannels : SBR.Channels {

    private Vector3 _movement;
    public Vector3 Movement {
        get { return _movement; }
        set {
            _movement = value.sqrMagnitude > 1f ? value.normalized * 1f : value;
        }
    }

    private Quaternion _rotation;
    public Quaternion Rotation {
        get { return _rotation; }
        set {
            _rotation = value;
        }
    }

    private bool _jump;
    public bool Jump {
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

    public override void AddFields(Dictionary<string, object> values) {
        base.AddFields(values);
        values[nameof(Movement)] = Movement;
        values[nameof(Rotation)] = Rotation;
        values[nameof(Jump)] = Jump;

    }
}
