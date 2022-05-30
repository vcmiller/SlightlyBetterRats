using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SBR {
    [CreateAssetMenu(menuName = "SBR/Unique Name List")]
    public class UniqueNameList : ScriptableObject {
        [SerializeField, EditNameOnly] private UniqueNameListEntry[] _entries;

        public IReadOnlyList<UniqueNameListEntry> Entries => _entries;
    }
}