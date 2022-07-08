namespace Nukebox.Games.CC.Addressables
{
    using UnityEngine.AddressableAssets;
    using UnityEngine.ResourceManagement.AsyncOperations;
    using UnityEngine.ResourceManagement;
    using System;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using Object = UnityEngine.Object;
    using UnityEngine;
    using System.Linq;
#if UNITY_EDITOR
    using UnityEditor;
#endif

    public class AddressableAssetLoadResult
    {
        private AssetReference assetReference;
        private string key;
        private Object loadedAsset;
        private string log;

        public AssetReference AssetReference { get => assetReference; }
        public string Key { get => key; }
        public Object LoadedAsset { get => loadedAsset; }
        public string Log { get => log; }

        public AddressableAssetLoadResult()
        {
            key = null;
            assetReference = null;
            loadedAsset = null;
            log = "";
        }

        public AddressableAssetLoadResult(AssetReference assetReference, Object loadedAsset = null, string log = "")
        {
            this.assetReference = assetReference;
            this.key = null;
            this.loadedAsset = loadedAsset;
            this.log = log;
        }

        public AddressableAssetLoadResult(string key, Object loadedAsset = null, string log = "")
        {
            this.assetReference = null;
            this.key = key;
            this.loadedAsset = loadedAsset;
            this.log = log;
        }
    }

    public class AddressableAssetLoader : Singleton<AddressableAssetLoader>
    {
        private Queue<AddressableLoadHandle> addressableDownloads;
        private List<AddressableLoadHandle> addressableLoadedHandles;
        private bool isLoading;
        private string errorMessageKey = "Could not load asset key: ";
        private string errorMessageReference = "Could not load asset reference: ";

        protected override void Start()
        {
            addressableDownloads = new Queue<AddressableLoadHandle>();
            addressableLoadedHandles = new List<AddressableLoadHandle>();
            ResourceManager.ExceptionHandler = CustomExceptionHandler;
        }

        /// <summary>
        /// Exception logger for addressables
        /// </summary>
        private void CustomExceptionHandler(AsyncOperationHandle handle, Exception exception)
        {
            Addressables.LogException(handle, exception);
        }

        /// <summary>
        /// Load addressable using key
        /// </summary>
        public void Load(string key, Action<AddressableAssetLoadResult> Result, bool isPrefab = false)
        {
            // If asset is already in memory and is not a prefab
            if (isPrefab == false && addressableLoadedHandles.Exists(handle => !handle.isReference && handle.key == key))
            {
                Object loadedAsset = addressableLoadedHandles.Find(handle => !handle.isReference && handle.key == key).operationHandle.Result;
                if (loadedAsset != null)
                    Result(new AddressableAssetLoadResult(key, loadedAsset));
                else
                    Result(new AddressableAssetLoadResult(key, null, errorMessageKey + key));
                return;
            }
#if ADDRESSABLEMULTIPLELOADING
                Load(new AddressableLoadHandle(key, Result));
#else
            // If no other asset is loading
            if (!isLoading)
            {
                Debug.Log("loading " + key);
                Load(new AddressableLoadHandle(key, Result));
            }
            // If other asset is loading, add to queue
            else
            {
                addressableDownloads.Enqueue(new AddressableLoadHandle(key, Result));
            }
#endif
        }

        /// <summary>
        /// Load addressable using reference
        /// </summary>
        public void Load(AssetReference reference, Action<AddressableAssetLoadResult> Result, bool isPrefab = false)
        {
#if UNITY_EDITOR
            /*
                                    if (isPrefab)
                                    {
                                        Object loadedAsset = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(reference.RuntimeKey.ToString()));
                                    if (loadedAsset == null)
                                        Result(new AddressableAssetLoadResult(reference, loadedAsset, errorMessageReference + reference + "...UnityEditor"));
                                    else
                                        Result(new AddressableAssetLoadResult(reference, loadedAsset, errorMessageReference + reference + "...UnityEditor"));
                                        // Result(AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(reference.RuntimeKey.ToString())));
                                        return;
                                    }
                                    */
#endif
            // If asset is already in memory and is not a prefab
            if (isPrefab == false && addressableLoadedHandles.Exists(handle => handle.isReference && handle.reference == reference))
            {
                Debug.Log("Addressable " + reference.SubObjectName + " was already in memory");
                Object loadedAsset = addressableLoadedHandles.Find(handle => handle.isReference && handle.reference == reference).operationHandle.Result;
                if (loadedAsset != null)
                    Result(new AddressableAssetLoadResult(reference, loadedAsset));
                else
                    Result(new AddressableAssetLoadResult(reference, null, errorMessageReference + reference));
                return;
            }
#if ADDRESSABLEMULTIPLELOADING
                Load(new AddressableLoadHandle(reference, Result));
#else
            AddressableLoadHandle handle = addressableDownloads.FirstOrDefault((loadHandle) => loadHandle.Equals(reference));
            if (handle != null)
            {
                handle.Result += Result;
                return;
            }

            // If no other asset is loading
            if (!isLoading)
            {
                Debug.Log("loading " + JsonUtility.ToJson(reference));
                Load(new AddressableLoadHandle(reference, Result));
            }
            // If other asset is loading, add to queue
            else
            {
                Debug.Log("added to queue " + JsonUtility.ToJson(reference));
				//addressableDownloads.Enqueue(new AddressableLoadHandle(reference, Result));
				LoadAsync(new AddressableLoadHandle(reference, Result));
            }
#endif
        }

        /// <summary>
        /// Load addressable using handle
        /// </summary>
        private void Load(AddressableLoadHandle downloadHandle)
        {
            //StartCoroutine(LoadAsync(downloadHandle));
            LoadAsync(downloadHandle);
        }

        /// <summary>
        /// Task for loading asset
        /// </summary>
        private async Task LoadAsync(AddressableLoadHandle downloadHandle)
        {
            if (downloadHandle.isReference == true)
            {
                Debug.Log("loading " + downloadHandle);
            }
            else
            {
                Debug.Log("loading " + downloadHandle.key);
            }
            isLoading = true;
            AsyncOperationHandle<Object> asyncOperation = downloadHandle.LoadAssetAsync();
            await asyncOperation.Task;
            if (downloadHandle.isReference == true)
            {
                Debug.Log("loading of " + downloadHandle + "\t"+downloadHandle.reference.SubObjectName + " " + asyncOperation.Status);
            }
            else
            {
                Debug.Log("loading of " + downloadHandle.key + " " + asyncOperation.Status);
            }
            isLoading = false;
            if (addressableDownloads.Count > 0)
            {
                Debug.Log("Loading next Addressable from queue.");
                Load(addressableDownloads.Dequeue());
            }
            else
            {
                Debug.Log("Addressable load queue is EMPTY.");
            }
            if (asyncOperation.Status == AsyncOperationStatus.Succeeded)
            {
                addressableLoadedHandles.Add(downloadHandle);

                if (downloadHandle.reference != null)
                    downloadHandle.Result(new AddressableAssetLoadResult(downloadHandle.reference, asyncOperation.Result));
                else if (!string.IsNullOrEmpty(downloadHandle.key))
                    downloadHandle.Result(new AddressableAssetLoadResult(downloadHandle.key, asyncOperation.Result));
                //downloadHandle.Result(asyncOperation.Result);
            }
            else
            {
                if (downloadHandle.reference != null)
                {
                    downloadHandle.Result(new AddressableAssetLoadResult(downloadHandle.reference, null, errorMessageReference + downloadHandle.reference));
                    Debug.Log(errorMessageReference + downloadHandle.reference);
                }
                else if (!string.IsNullOrEmpty(downloadHandle.key))
                {
                    downloadHandle.Result(new AddressableAssetLoadResult(downloadHandle.key, null, errorMessageKey + downloadHandle.key));
                    Debug.Log(errorMessageKey + downloadHandle.key);
                }
            }

        }

        //private IEnumerator LoadAsync(AddressableLoadHandle downloadHandle)
        //{
        //    isLoading = true;
        //    AsyncOperationHandle<UnityEngine.Object> asyncOperation = downloadHandle.LoadAssetAsync();
        //    yield return asyncOperation;
        //    if (asyncOperation.Status == AsyncOperationStatus.Succeeded)
        //    {
        //        addressableLoadedHandles.Add(downloadHandle);
        //        downloadHandle.Result(asyncOperation.Result);
        //    }
        //    else
        //    {
        //        downloadHandle.Result(null);
        //    }
        //    isLoading = false;
        //    if (addressableDownloads.Count > 0)
        //    {
        //        Load(addressableDownloads.Dequeue());
        //    }
        //}

        /// <summary>
        /// Unload asset using key
        /// </summary>
        public void Unload(string key)
        {
            if (key == null)
            {
                Debug.Log("Couldn't unload asset as key is null.");
                return;
            }
            if (addressableLoadedHandles.Exists(handle => !handle.isReference && handle.key == key) == false)
            {
                Debug.Log("Couldn't unload asset with key: " + key + " as it is not in memory.");
                return;
            }
            AddressableLoadHandle addressableLoadHandle = addressableLoadedHandles.Find(handle => !handle.isReference && handle.key == key);
            Addressables.Release<Object>(addressableLoadHandle.operationHandle);
            addressableLoadedHandles.Remove(addressableLoadHandle);
        }

        /// <summary>
        /// Unload asset using reference
        /// </summary>
        /// <param name="reference"></param>
        public void Unload(AssetReference reference)
        {
            if (reference == null)
            {
#if UNITY_EDITOR
                Debug.Log("Couldn't unload asset as asset reference is null.");
#endif
                return;
            }
            if (addressableLoadedHandles.Exists(handle => handle.isReference && handle.reference.AssetGUID == reference.AssetGUID) == false)
            {
#if UNITY_EDITOR
                Debug.Log("Couldn't unload asset with reference: " + reference.SubObjectName + " as it is not in memory.");
#endif
                return;
            }
#if UNITY_EDITOR
            Debug.Log("Unloading " + reference);
#endif
            AddressableLoadHandle addressableLoadHandle = addressableLoadedHandles.Find(handle => handle.isReference && handle.reference.AssetGUID == reference.AssetGUID);
            Addressables.Release<Object>(addressableLoadHandle.operationHandle);
            addressableLoadedHandles.Remove(addressableLoadHandle);
        }

    }

    public class AddressableLoadHandle : IEquatable<AddressableLoadHandle>, IEquatable<AssetReference>
    {
        public string key;
        public AssetReference reference;
        public Action<AddressableAssetLoadResult> Result;
        public AsyncOperationHandle<Object> operationHandle;
        public bool isReference { get; private set; }

        /// <summary>
        /// Constructor using key
        /// </summary>
        public AddressableLoadHandle(string key, Action<AddressableAssetLoadResult> Result)
        {
            this.key = key;
            this.Result = Result;
            isReference = false;
        }

        /// <summary>
        /// Constructor using reference
        /// </summary>
        public AddressableLoadHandle(AssetReference reference, Action<AddressableAssetLoadResult> Result)
        {
            this.reference = reference;
            this.Result = Result;
            isReference = true;
        }

        /// <summary>
        /// Load asset using reference or key
        /// </summary>
        public AsyncOperationHandle<Object> LoadAssetAsync()
        {
            if (isReference)
                operationHandle = Addressables.LoadAssetAsync<Object>(reference);
            else
                operationHandle = Addressables.LoadAssetAsync<Object>(key);
            return operationHandle;
        }

        public bool Equals(AddressableLoadHandle other)
        {
            return this.isReference == other.isReference
                ? (!this.isReference && !other.isReference)
                    ? (this.key?.Equals(other?.key ?? string.Empty) ?? false)
                    : (this.reference?.Equals(other.reference) ?? false)
                : false;
        }

        public bool Equals(AssetReference other)
        {
            return !isReference
                ? false
                : reference.Equals(other);
        }

        public override string ToString()
        {
            return $"Key- {(isReference ? reference.ToString() : key)}\n" +
                $"OperationHandle-{(operationHandle.IsValid() ? operationHandle.ToString() : "OpHandle is either null or invalid")}";
        }
    }

}