using Infohazard.Core.Editor;
using UnityEditor;

namespace SBR.Editor {
    public class SBREditorUtility {
        private static readonly BuildTargetGroup[] BuildTargetGroups = new BuildTargetGroup[] {
            BuildTargetGroup.Standalone,
            BuildTargetGroup.Android,
            BuildTargetGroup.iOS,
            BuildTargetGroup.WSA,
            BuildTargetGroup.WebGL,
        };
        
        public static void SetSymbolDefined(string symbol, bool value) {
            foreach (var group in BuildTargetGroups) {
                CoreEditorUtility.SetSymbolDefined(symbol, value, group);
            }
        }
    }
}