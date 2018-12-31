using UnityEngine;
using SBR;

public class CharacterChannels : SBR.Channels {

    private Vector3 _movement;
    public Vector3 movement {
        get { return _movement; }
        set {
            movement = value.sqrMagnitude > 1f ? value.normalized * 1f : value;
        }
    }

    private Quaternion _rotation;
    public Quaternion rotation {
        get { return _rotation; }
        set {
            rotation = value;
        }
    }

    private bool _jump;
    public bool jump {
        get { return _jump; }
        set {
            jump = value;
        }
    }


    public override void ClearInput() {
        _movement = new Vector3(0f, 0f, 0f);
        _rotation = new Quaternion(0f, 0f, 0f, 1f);
        _jump = false;

    }
}
