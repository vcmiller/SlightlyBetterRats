using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace SBR.Startup {
    public class LoadInitialRegionsStep : MonoBehaviour, IExecutionStep {
        [SerializeField] private bool _enableImmediately;
        
        public static readonly ExecutionStepParameter<LevelManifestLevelEntry> ParamLoadingLevel =
            new ExecutionStepParameter<LevelManifestLevelEntry>();

        public static readonly ExecutionStepParameter<IEnumerable<int>> ParamRegionsToLoad =
            new ExecutionStepParameter<IEnumerable<int>>();

        public bool IsFinished { get; private set; }
        
        private List<AsyncOperation> _regionOperations;
        private List<LevelManifestRegionEntry> _loadingRegions;

        protected virtual IEnumerable<int> DefaultRegionsToLoad(LevelManifestLevelEntry level) {
            foreach (LevelManifestRegionEntry region in level.Regions) {
                if (region.LoadedByDefault) yield return region.RegionID;
            }
        }
        
        public void ExecuteForward(ExecutionStepArguments arguments) {
            if (ParamLoadingLevel.Get(arguments, out LevelManifestLevelEntry level) && level != null) {
                StartCoroutine(CRT_Execution(level, arguments));
            } else {
                IsFinished = true;
            }
        }

        private IEnumerator CRT_Execution(LevelManifestLevelEntry level, ExecutionStepArguments arguments) {
            IsFinished = false;
            _regionOperations = new List<AsyncOperation>();

            HashSet<int> regionsToLoad =
                new HashSet<int>(ParamRegionsToLoad.GetOrDefault(arguments, DefaultRegionsToLoad(level)));

            _loadingRegions = new List<LevelManifestRegionEntry>();
            foreach (LevelManifestRegionEntry region in level.Regions) {
                if (!region || (!region.AlwaysLoaded && !regionsToLoad.Contains(region.RegionID))) continue;

                AsyncOperation regionOperation =
                    SceneManager.LoadSceneAsync(region.Scene.Name, LoadSceneMode.Additive);
                regionOperation.allowSceneActivation = false;
                _regionOperations.Add(regionOperation);
                _loadingRegions.Add(region);
            }

            while (_regionOperations != null && _regionOperations.Any(op => op.progress < 0.9f)) {
                yield return null;
            }
            
            if (_enableImmediately) {
                yield return EnableScenesCRT();
            }

            IsFinished = true;
        }

        public void EnableScenes() => EnableScenesCRT();

        public Coroutine EnableScenesCRT() {
            if (_regionOperations == null || _regionOperations.Count == 0 ||
                _regionOperations[0].allowSceneActivation) {
                return null;
            }
            
            foreach (AsyncOperation operation in _regionOperations) {
                operation.allowSceneActivation = true;
            }
            
            return StartCoroutine(CRT_ActivateScenes());
        }

        private IEnumerator CRT_ActivateScenes() {
            while (_regionOperations.Any(op => !op.isDone)) yield return null;

            foreach (var region in _loadingRegions) {
                var scene = SceneManager.GetSceneByName(region.Scene.Name);
                
                foreach (GameObject obj in scene.GetRootGameObjects()) {
                    if (!obj.TryGetComponentInChildren(out RegionRoot regionRoot)) continue;
                    regionRoot.Initialize();
                    break;
                }
            }
        }
    }
}