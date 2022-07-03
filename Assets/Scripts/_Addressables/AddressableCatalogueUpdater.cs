using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Nukebox.Games.CC.Addressables
{
    using UnityEngine.AddressableAssets;
    using UnityEngine.ResourceManagement.AsyncOperations;
    using UnityEngine.AddressableAssets.ResourceLocators;

    public class AddressableCatalogueUpdater : Singleton<AddressableCatalogueUpdater>
    {

        public bool IsInitialized
        {
            get;
            private set;
        }

        protected override void Awake()
        {
            base.Awake();
#if LOCAL_BUILD
            IsInitialized = true;
#else
            IsInitialized = false;

            UpdateCatalogue();
#endif
        }

        protected async System.Threading.Tasks.Task UpdateCatalogue()
        {
            AsyncOperationHandle<List<string>> checkForUpdateHandle = Addressables.CheckForCatalogUpdates(false);

            List<string> cataloguesToUpdate = new List<string>();

            checkForUpdateHandle.Completed += op =>
            {
                if (op.Result?.Count > 0)
                    cataloguesToUpdate.AddRange(op.Result);
            };

            await checkForUpdateHandle.Task;

            if (cataloguesToUpdate.Count > 0)
            {
                AsyncOperationHandle<List<IResourceLocator>> updateCataloguesHandle = Addressables.UpdateCatalogs(checkForUpdateHandle.Result, true);
                updateCataloguesHandle.Completed += op =>
                {
                    if (op.OperationException != null) {
                        Debug.LogError(op.OperationException);
                    }

                    IsInitialized = true;
                };

                await updateCataloguesHandle.Task;
            }

            Addressables.Release(checkForUpdateHandle);

            IsInitialized = true;
        }

    }
}
