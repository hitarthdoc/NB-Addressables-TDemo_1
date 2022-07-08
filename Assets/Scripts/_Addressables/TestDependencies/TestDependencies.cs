using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Nukebox.Games.CC.Addressables
{
    public class TestDependencies : MonoBehaviour
    {
        public string label;

        [ContextMenu("Test label")]
        private async void TestLabel()
        {
            AddressableAssetDownloader.Instance.DownloadAsync(label, Callback);
            //AddressableAssetLoader.Instance.Load(label, successCallback, true);
        }

        public AssetReference assetReference;

        [ContextMenu("Test Asset ref")]
        private async void TestAssetRef()
        {
            AddressableAssetLoader.Instance.Load(assetReference, successCallback, true);
        }

        private void successCallback(AddressableAssetLoadResult obj)
        {
            Debug.Log(obj.LoadedAsset);
        }

        private void Callback(float obj, bool hasFailed)
        {

        }
    }
}