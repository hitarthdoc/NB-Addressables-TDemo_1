using System;
using System.Collections;
using System.Collections.Generic;

using Nukebox.Games.CC.Addressables;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace Platformer.Mechanics
{
    public class TokenSpawner : MonoBehaviour
    {
        [SerializeField]
        protected TokenController tokenController;

        [SerializeField]
        protected AssetReference tokenReference;

        [SerializeField]
        protected Transform tokenParent;

        [SerializeField]
        protected List<Vector3> tokenPositions = new List<Vector3>();

        [SerializeField]
        protected List<GameObject> spawnedTokens = new List<GameObject>();

        [Header("Loading Progress")]
        [SerializeField]
        protected GameObject LoadingPanel;
        [SerializeField]
        protected Slider percentLoadingSlider;
        [SerializeField]
        protected Text percentLoadingText;

        protected void OnEnable()
        {
            LoadingPanel.SetActive(true);

            AddressableReferenceLoader.Instance.LoadAssetReference(tokenReference, OnSuccess, OnProgress, OnFailed);
        }

        protected void OnDisable()
        {
            tokenReference.ReleaseAsset();
        }

        private void OnFailed(Exception obj)
        {
            Debug.LogWarning($"Load Failed.... Exception already logged.");
            LoadingPanel.SetActive(false);
        }

        private void OnSuccess(AssetReference obj)
        {
            if (tokenReference.RuntimeKeyIsValid ()
                && tokenReference.OperationHandle.Status == AsyncOperationStatus.Succeeded)
            {
                SpawnTokens();
            }
            LoadingPanel.SetActive(false);
        }

        private void OnProgress(float percentComplete, float downloadProgress)
        {
            if (downloadProgress < 1)
            {
                percentLoadingText.text = $"Downloading... {downloadProgress * 100}";
                percentLoadingSlider.value = downloadProgress;
            }
            else
            {
                percentLoadingText.text = $"Loading... {percentComplete * 100}";
                percentLoadingSlider.value = percentComplete;
            }
        }

        private void SpawnTokens()
        {
            int count = 0;

            async void CountSpawnedInstances(AsyncOperationHandle<GameObject> asyncOperationHandle)
            {
                await asyncOperationHandle.Task;

                GameObject token = asyncOperationHandle.Result;

                token.GetComponent<TokenInstance>().Initialize(OnTokenDestroyed);

                if (++count >= tokenPositions.Count)
                    tokenController.Initialize();

                //We can release Asset ref here as well. if we know that we are NOT going  to need it again.

            }

            foreach (Vector3 position in tokenPositions)
            {
                CountSpawnedInstances(tokenReference.InstantiateAsync(position, Quaternion.identity, tokenParent));
            }
        }

        private void OnTokenDestroyed(TokenInstance instance)
        {
            tokenReference.ReleaseInstance(instance.gameObject);
        }

        //private async void CountSpawnedInstances(AsyncOperationHandle<GameObject> asyncOperationHandle)
        //{
        //    await asyncOperationHandle.Task;
        //
        //    spawnedTokens.Add(asyncOperationHandle.Result);
        //
        //    if (spawnedTokens.Count >= tokenPositions.Count)
        //        tokenController.Initialize();
        //}

        [ContextMenu("Populate Positions")]
        protected void PopulatePositions()
        {
            var tokens = FindObjectsOfType<TokenInstance>();

            tokenPositions = new List<Vector3>(tokens.Length);
            foreach (var token in tokens)
            {
                tokenPositions.Add(token.transform.position);
            }

            //UnityEditor.Editor.SetDirty();
        }

    }
}
