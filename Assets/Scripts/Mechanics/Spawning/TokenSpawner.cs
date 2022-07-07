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

        protected int startPos = 0;
        protected int endpos = 30;

        protected void OnEnable()
        {
            AddressableReferenceLoader.Instance.LoadAssetReference(tokenReference, OnSuccess, OnFailed);
            AddressableReferenceLoader.Instance.LoadAssetReference(tokenReference, OnSuccess, OnFailed);
            AddressableReferenceLoader.Instance.LoadAssetReference(tokenReference, OnSuccess, OnFailed);
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
            int start = startPos;
            int end = endpos;
            startPos = endpos;
            endpos = Mathf.Clamp ((endpos + (end - start)), start, tokenPositions.Count);

            for (int index = start; index < end; index++)
            {
                Vector3 position = tokenPositions[index];
                CountSpawnedInstances(tokenReference.InstantiateAsync(position, Quaternion.identity, tokenParent));
            }
        }

        private void OnTokenDestroyed(TokenInstance instance)
        {
            tokenReference.ReleaseInstance(instance.gameObject);
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
