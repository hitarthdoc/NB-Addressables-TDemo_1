using System;
using System.Collections;
using System.Collections.Generic;

using Nukebox.Games.CC.Addressables;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

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

        protected void OnEnable()
        {
            AddressableReferenceLoader.Instance.LoadAssetReference(tokenReference, OnSuccess, OnFailed);
        }

        private void OnFailed(Exception obj)
        {
            Debug.LogWarning($"Load Failed.... Exception already logged.");
        }

        private void OnSuccess(AssetReference obj)
        {
            if (tokenReference.RuntimeKeyIsValid ()
                && tokenReference.OperationHandle.Status == AsyncOperationStatus.Succeeded)
            {
                SpawnTokens();
            }
        }

        private void SpawnTokens()
        {
            foreach (var position in tokenPositions)
            {
                CountSpawnedInstances(tokenReference.InstantiateAsync(position, Quaternion.identity, tokenParent));
            }
        }

        private async void CountSpawnedInstances(AsyncOperationHandle<GameObject> asyncOperationHandle)
        {
            await asyncOperationHandle.Task;

            spawnedTokens.Add(asyncOperationHandle.Result);

            if (spawnedTokens.Count >= tokenPositions.Count)
                tokenController.Initialize();
        }

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
