// File: Assets/_Project/Scripts/Networking/NetworkOptimizer.cs
// Fix: Moved all static field initializations into a method with the
// [RuntimeInitializeOnLoadMethod] attribute to resolve all UDR0002 warnings
// and ensure stability between play sessions in the Unity Editor.

using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using System.Collections;

namespace Platformer.Networking
{
    public static class NetworkOptimizer
    {
        // Fields are now declared without initial values.
        private static Dictionary<string, float> _lastRpcTimes;
        private static Queue<System.Action> _batchedOperations;
        private static bool _isBatchProcessing;
        
        public const float MIN_RPC_INTERVAL = 0.1f;

        // This method is now the single source of truth for initializing static fields.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void InitializeOnLoad()
        {
            _lastRpcTimes = new Dictionary<string, float>();
            _batchedOperations = new Queue<System.Action>();
            _isBatchProcessing = false;
            Platformer.Core.Logger.Info(Platformer.Core.Logger.LogCategory.Networking, "NetworkOptimizer static fields reset.");
        }
        
        public static void BatchOperation(System.Action operation)
        {
            if (operation == null) return;
            
            _batchedOperations.Enqueue(operation);
            
            if (!_isBatchProcessing)
            {
                CoroutineRunner.Instance.StartCoroutine(ProcessBatch());
            }
        }
        
        private static IEnumerator ProcessBatch()
        {
            _isBatchProcessing = true;
            yield return null; 
            
            while (_batchedOperations.Count > 0)
            {
                try
                {
                    var operation = _batchedOperations.Dequeue();
                    operation?.Invoke();
                }
                catch (System.Exception ex)
                {
                    Platformer.Core.Logger.Exception(Platformer.Core.Logger.LogCategory.Networking, ex, "Exception in batched operation");
                }
                yield return null;
            }
            
            _isBatchProcessing = false;
        }
    }
    
    public class CoroutineRunner : MonoBehaviour
    {
        private static CoroutineRunner _instance;
        
        public static CoroutineRunner Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("NetworkOptimizer_CoroutineRunner");
                    _instance = go.AddComponent<CoroutineRunner>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }
        
        // This ensures the static instance is reset between play sessions in the editor.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void ResetStatics()
        {
            _instance = null;
        }
    }
}