using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nukebox.Games.CC.Addressables
{
    using System;
    using System.Text;
    using System.IO;

    using UnityEngine;
    using UnityEngine.AddressableAssets;

    using UnityEngine.ResourceManagement.AsyncOperations;
    using UnityEngine.ResourceManagement.ResourceLocations;

    public class AddressableAssetDownloader : Singleton<AddressableAssetDownloader>
    {
        public const float timeToKeepAfterCompletion = 10.0f; //10 Seconds

        [System.Serializable]
        public class AssetBundleDownloadTracker
        {

            public string label;
            public AsyncOperationHandle? locations = null;
            public AsyncOperationHandle? dependencies = null;
            public Exception locationException = null;
            public Exception dependenciesException = null;
            protected float progress = 0;

            public float completionTime { get; protected set; } = 0;

            public event Action<float, bool> progressCallbacks;

            public AssetBundleDownloadTracker(string label)
            {
                this.label = label ?? throw new ArgumentNullException(nameof(label));
                this.locations = null;
                this.dependencies = null;
            }

            public AssetBundleDownloadTracker(string label, AsyncOperationHandle locations)
            {
                this.label = label ?? throw new ArgumentNullException(nameof(label));
                this.locations = locations;
                this.dependencies = null;
            }

            public AssetBundleDownloadTracker(string label, AsyncOperationHandle locations, AsyncOperationHandle dependencies)
            {
                this.label = label ?? throw new ArgumentNullException(nameof(label));
                this.locations = locations;
                this.dependencies = dependencies;
            }

            public float Progress
            {
                get {
                    progress = 0;
                    //Locations is being considered as 10% of total progress.
                    if (!locations.HasValue || locations.Value.IsValid()) {
                        progress += (locations.HasValue ? locations.Value.GetDownloadStatus().Percent : 0) * 0.1f;
                    } else
                        progress += .1f;

                    //Downloading Dependancies is being considered as 90% of total progress.
                    if (dependencies?.IsValid() ?? true) {
                        progress += (dependencies?.GetDownloadStatus().Percent ?? 0) * 0.9f;
                    } else
                        progress += 0.9f;

                    return progress;
                }
            }

            public bool IsRunning
            {
                get
                {
                    return (locations?.Status == AsyncOperationStatus.None) || (dependencies?.Status == AsyncOperationStatus.None);
                }
            }

            public bool HasFailed
            {
                get
                {
                    return locationException != null || dependenciesException != null;
                }
            }

            public void UpdateProgress ()
            {
                if (completionTime == 0)
                {
                    progressCallbacks?.Invoke(Progress, HasFailed);
                }
            }

            internal void CompleteLocations()
            {
                locationException = locations?.OperationException;
            }

            internal void Complete()
            {
                completionTime = Time.time;
                dependenciesException = dependencies?.OperationException;
                progressCallbacks?.Invoke(Progress, HasFailed);
            }
        }

        protected Dictionary<string, AssetBundleDownloadTracker> labelsBeingDownloaded = new Dictionary<string, AssetBundleDownloadTracker>();

        protected Coroutine progressUpdater = null;

        public async Task DownloadAsync(string label, Action<float, bool> progressCallback)
        {
            if (progressUpdater == null)
                progressUpdater = StartCoroutine(UpdateProgress());

            if (!labelsBeingDownloaded.ContainsKey(label))
            {
                AssetBundleDownloadTracker downloadTracker = new AssetBundleDownloadTracker(label);

                labelsBeingDownloaded.Add(label, downloadTracker);
                downloadTracker.progressCallbacks += progressCallback;

                AsyncOperationHandle<IList<IResourceLocation>> operationHandleLocations = Addressables.LoadResourceLocationsAsync(label);
                downloadTracker.locations = operationHandleLocations;

                operationHandleLocations.CompletedTypeless += (AsyncOperationHandle asyncOp) =>
                {
                    downloadTracker.CompleteLocations();
                };

                Debug.Log($"Before downloading, {label}");
                await operationHandleLocations.Task;

#if CHECK_DEPENDENCY && UNITY_EDITOR
                StringBuilder @string = new StringBuilder();

                foreach (var resourceLocation in operationHandleLocations.Result)
                {
                    @string.AppendLine();
                    @string.AppendLine(resourceLocation.PrimaryKey);
                    foreach (var dependancy in resourceLocation.Dependencies)
                    {
                        @string.AppendLine($"\t-{dependancy.InternalId}");
                    }
                }

                string filePath = Path.Combine(Application.dataPath, "Dependency_Check", label, "dependancies.txt");
                if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                File.WriteAllText(filePath, @string.ToString());
#endif
                //await Task.Delay(20000);

                AsyncOperationHandle operationHandleDownload = Addressables.DownloadDependenciesAsync(operationHandleLocations.Result);
                operationHandleDownload.Completed += (AsyncOperationHandle asyncOp) =>
                {
                    Success(asyncOp);
                    downloadTracker.Complete();
                };

                downloadTracker.dependencies = operationHandleDownload;

                await operationHandleDownload.Task;

                Debug.Log($"Downloaded, {label} {operationHandleDownload.Status}");

                Addressables.Release(operationHandleLocations);
                Addressables.Release(operationHandleDownload);
            }
            else
            {
                AssetBundleDownloadTracker downloadTracker = labelsBeingDownloaded[label];
                downloadTracker.progressCallbacks -= progressCallback;
                downloadTracker.progressCallbacks += progressCallback;
                downloadTracker.UpdateProgress();
            }
        }

        protected IEnumerator UpdateProgress()
        {
            yield return null;

            List<AssetBundleDownloadTracker> removeThese = new List<AssetBundleDownloadTracker>();
            while (labelsBeingDownloaded.Values?.Count > 0)
            {
                yield return null;
                foreach (AssetBundleDownloadTracker downloadTracker in labelsBeingDownloaded.Values)
                {

                    if (downloadTracker.completionTime != 0 && Time.time - downloadTracker.completionTime > timeToKeepAfterCompletion)
                        removeThese.Add(downloadTracker);
                }

                foreach (var removeThis in removeThese)
                {
                    labelsBeingDownloaded.Remove(removeThis.label);
                }
            }
            progressUpdater = null;
        }

        private void Success(AsyncOperationHandle obj)
        {
        }

    }

}
