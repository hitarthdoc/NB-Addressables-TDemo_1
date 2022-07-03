

namespace Nukebox.Games.CC.Addressables
{
    using System;
    using System.Linq;
    using System.Collections;
    using System.Collections.Generic;

    using UnityEngine;
    using UnityEngine.AddressableAssets;
    using UnityEngine.ResourceManagement;
    using UnityEngine.ResourceManagement.AsyncOperations;

    public class AssetReferenceAndRequester
    {
        public AssetReference assetReference;
        public AsyncOperationHandle handle;
        public Action<AssetReference> onLoaded;
        public Action<Exception> onFail;

        public AssetReferenceAndRequester(AssetReference assetReference, Action<AssetReference> onLoaded, Action<Exception> onFail)
        {
            this.assetReference = assetReference;
            this.onLoaded = onLoaded;
            this.onFail = onFail;

            this.handle = assetReference.LoadAssetAsync<object> ();
            HandleLoadTask();
        }

        protected async void HandleLoadTask ()
        {
            await handle.Task;

            if (handle.IsDone)
            {
                switch (handle.Status)
                {
                    case AsyncOperationStatus.Succeeded:
                        onLoaded?.Invoke(assetReference);
                        break;
                    case AsyncOperationStatus.Failed:
                        Debug.LogException(handle.OperationException);
                        onFail?.Invoke(handle.OperationException);
                        break;
                    default:
                    case AsyncOperationStatus.None:
                        break;
                }
                    return;
            }

        }

    }

    public class AddressableReferenceLoader : Singleton<AddressableReferenceLoader>
    {
        [SerializeField]
        private HashSet<AssetReference> LoadedAssetReferences = new HashSet<AssetReference> ();
        private HashSet<AssetReferenceAndRequester> LoadingAssetReferences = new HashSet<AssetReferenceAndRequester> ();

        private object loadingCheckLock = new object();

        public AsyncOperationHandle LoadAssetReference (AssetReference assetReference, Action <AssetReference> onSuccess, Action<Exception> onFailed)
        {
            lock (loadingCheckLock)
            {
                if (LoadedAssetReferences.Contains(assetReference)) onSuccess?.Invoke(assetReference);
                var loader = LoadingAssetReferences.FirstOrDefault((handle) => handle.assetReference == assetReference);
                if (loader != null)
                {
                    loader.onLoaded += onSuccess;
                    loader.onFail += onFailed;

                    return loader.handle;
                }
            }

            AssetReferenceAndRequester loadHandle = new AssetReferenceAndRequester(assetReference, onSuccess, onFailed);
            LoadingAssetReferences.Add(loadHandle);

            WaitTillAssetReferenceIsLoaded(loadHandle);

            return assetReference.OperationHandle;
        }

        protected async void WaitTillAssetReferenceIsLoaded (AssetReferenceAndRequester request)
        {
            await request.handle.Task;

            if (request.handle.Status == AsyncOperationStatus.Succeeded)
            {
                LoadedAssetReferences.Add(request.assetReference);
            }
            LoadingAssetReferences.Remove(request);
        }

    }
}
