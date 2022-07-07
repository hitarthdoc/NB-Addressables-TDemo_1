using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;

public class LoadAdditionalCatalouges : MonoBehaviour
{
    [SerializeField]
    protected AdditionalCataloguesSO AdditionalCatalogues;

    [SerializeField]
    protected UnityEvent OnCataloguesLoaded; 

    [ContextMenu(nameof(LoadAdditionalCatalogues))]
    public void LoadAdditionalCatalogues()
    {
        foreach (var catalog in AdditionalCatalogues.catalouges)
        {
            Addressables.LoadContentCatalogAsync(catalog, true)
                        .CompletedTypeless += Completed;
        }
    }

    private void Completed(AsyncOperationHandle obj)
    {
        OnCataloguesLoaded.Invoke();
    }
}
