using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SBR.Sequencing {
    public interface IRegionAwareObject {
        public RegionRoot CurrentRegion { get; set; }
        public bool CanTransitionTo(RegionRoot region);
    }
}