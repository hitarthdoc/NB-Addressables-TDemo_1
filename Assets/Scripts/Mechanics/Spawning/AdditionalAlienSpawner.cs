using System;
using System.Collections;
using System.Collections.Generic;

using Nukebox.Games.CC.Addressables;

using UnityEngine;

public class AdditionalAlienSpawner : MonoBehaviour
{
    [SerializeField]
    protected string alienDataKey = "AlienData";

    protected AlienDataSO DataSO;

    public void OnAdditionalCatalogueLoaded ()
    {
        AddressableAssetLoader.Instance.Load(alienDataKey, OnLoadCallComplete, false);
    }

    private void OnLoadCallComplete(AddressableAssetLoadResult result)
    {
        if (result.LoadedAsset != null && result.LoadedAsset is AlienDataSO dataSO )
        {
            DataSO = dataSO;

            SpawnAlien();
        }
    }

    private void SpawnAlien()
    {
        if (DataSO != null)
        {
            DataSO.AlienData.alien.InstantiateAsync(DataSO.AlienData.spawnPosition, Quaternion.identity);
        }
    }
}
