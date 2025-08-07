// File: Assets/_Project/Scripts/Enemies/EnemyAIController.cs
// Version: 2.1 (Corrected)
// Fix: Added the missing 'empoweredAttackColor' field to resolve CS1061.

using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using Photon.Pun;

namespace Platformer
{
    [RequireComponent(typeof(CharacterStats), typeof(NavMeshAgent), typeof(StateMachine))]
    [RequireComponent(typeof(PhotonView))]
    public class EnemyAIController : MonoBehaviour
    {
        [Header("AI Behavior")]
        public bool canFollowPlayer = true;
        public float leashRadius = 15f;
        public float attackRadius = 2f;
        public LayerMask targetLayerMask;

        [Header("Loot")]
        public GameObject coinPrefab;
        public int coinDropAmount = 3;

        [Header("Debug Colors")]
        public Material debugMaterial;
        private Material _materialInstance;
        public Color idleColor = Color.gray;
        public Color combatColor = Color.magenta;
        public Color returnColor = new Color(1f, 0.5f, 0f);
        public Color empoweredAttackColor = Color.white; // MISSING FIELD ADDED
        
        public CharacterStats MyStats { get; private set; }
        public NavMeshAgent Agent { get; private set; }
        public StateMachine StateMachine { get; private set; }
        public Transform PlayerTarget { get; private set; }
        public Vector3 StartPosition { get; private set; }
        public bool IsAggroed { get; private set; }
        private float _aggroLossTimer;

        private void Awake()
        {
            MyStats = GetComponent<CharacterStats>();
            Agent = GetComponent<NavMeshAgent>();
            StateMachine = GetComponent<StateMachine>();
            StartPosition = transform.position;
            var renderer = GetComponentInChildren<Renderer>();
            if (renderer != null) 
            {
                _materialInstance = new Material(renderer.material);
                renderer.material = _materialInstance;
                debugMaterial = _materialInstance;
            }
            
            MyStats.OnLevelUp += HandleLevelUp;
            MyStats.OnDied += HandleDeath;
        }

        private void Start()
        {
            UpdateAgentSpeed();
            SetupStateMachine();
        }

        private void OnDestroy()
        {
            if (MyStats != null) 
            {
                MyStats.OnLevelUp -= HandleLevelUp;
                MyStats.OnDied -= HandleDeath; 
            }
        }

        private void SetupStateMachine()
        {
            var idleState = new EnemyIdleState(this);
            var combatState = new EnemyCombatState(this);
            var returnState = new EnemyReturnState(this);

            StateMachine.AddTransition(idleState, combatState, new FunkPredicate(() => IsAggroed && ShouldChasePlayer()));
            StateMachine.AddTransition(combatState, returnState, new FunkPredicate(() => !ShouldChasePlayer()));
            StateMachine.AddTransition(returnState, idleState, new FunkPredicate(() => Vector3.Distance(transform.position, StartPosition) < 1f));

            StateMachine.SetState(idleState);
        }
        
        private bool ShouldChasePlayer()
        {
            if (PlayerTarget == null || !canFollowPlayer) return false;

            CharacterStats playerStats = PlayerTarget.GetComponent<CharacterStats>();
            if (playerStats == null) return false;
            
            if (playerStats.isInGrass && !playerStats.isRevealed)
            {
                if (this.MyStats.team != Team.Neutral)
                {
                    if (IsAggroed) LoseAggro();
                    return false;
                }
            }

            if (IsAggroed)
            {
                if (IsPlayerWithinLeash())
                {
                    _aggroLossTimer = 2f;
                    return true;
                }
                else
                {
                    _aggroLossTimer -= Time.deltaTime;
                    return _aggroLossTimer > 0;
                }
            }
            return false;
        }

        public void AggroOnDamage(Transform attacker)
        {
            if (canFollowPlayer)
            {
                IsAggroed = true;
                PlayerTarget = attacker;
                _aggroLossTimer = 2f;
                
                if (PhotonNetwork.InRoom && attacker.TryGetComponent<PhotonView>(out var photonView))
                {
                    GetComponent<PhotonView>().RPC(nameof(Rpc_SetTarget), RpcTarget.All, photonView.ViewID);
                }
            }
        }

        [PunRPC]
        private void Rpc_SetTarget(int targetViewID)
        {
            PhotonView targetView = PhotonView.Find(targetViewID);
            if (targetView != null)
            {
                PlayerTarget = targetView.transform;
                IsAggroed = true;
                _aggroLossTimer = 2f;
            }
        }

        public void LoseAggro() { IsAggroed = false; }
        
        private void HandleDeath() 
        { 
            StateMachine.enabled = false; 
            if(Agent.isOnNavMesh) Agent.isStopped = true;
            Agent.enabled = false; 
            foreach (Collider col in GetComponentsInChildren<Collider>()) { col.enabled = false; } 
            StartCoroutine(DeathSequence()); 
        }
        
        private IEnumerator DeathSequence() 
        { 
            if (coinPrefab != null)
            {
                if (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
                {
                    for (int i = 0; i < coinDropAmount; i++) 
                    { 
                        Vector3 randomOffset = new Vector3(Random.Range(-0.5f, 0.5f), 0.5f, Random.Range(-0.5f, 0.5f)); 
                        PhotonNetwork.Instantiate(coinPrefab.name, transform.position + randomOffset, Quaternion.identity); 
                    }
                }
                else if (!PhotonNetwork.InRoom)
                {
                    for (int i = 0; i < coinDropAmount; i++)
                    {
                        Vector3 randomOffset = new Vector3(Random.Range(-0.5f, 0.5f), 0.5f, Random.Range(-0.5f, 0.5f));
                        Instantiate(coinPrefab, transform.position + randomOffset, Quaternion.identity);
                    }
                }
            }
            
            float fadeDuration = 1.5f; 
            float timer = 0f; 
            Color startColor = _materialInstance.color; 
            while (timer < fadeDuration) 
            { 
                timer += Time.deltaTime; 
                float alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration); 
                _materialInstance.color = new Color(startColor.r, startColor.g, startColor.b, alpha); 
                yield return null; 
            } 
            
            if(PhotonNetwork.InRoom)
            {
                if(PhotonNetwork.IsMasterClient)
                {
                    PhotonNetwork.Destroy(gameObject);
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void HandleLevelUp(int newLevel)
        {
            UpdateAgentSpeed();
        }

        private void UpdateAgentSpeed() 
        { 
            if(Agent.isOnNavMesh) Agent.speed = MyStats.baseStats.Speed; 
        }
        
        private void Update() 
        { 
            if(!PhotonNetwork.IsMasterClient && PhotonNetwork.InRoom) return;

            if (StateMachine.enabled) StateMachine.Tick(); 
        }

        public bool IsPlayerInRadius(float radius) { if (PlayerTarget == null) return false; return Vector3.Distance(transform.position, PlayerTarget.position) <= radius; }
        public bool IsPlayerWithinLeash() { if (PlayerTarget == null) return false; return Vector3.Distance(StartPosition, PlayerTarget.position) <= leashRadius; }
        public void SetStateColor(Color color) { if (_materialInstance != null) _materialInstance.color = color; }
        private void OnDrawGizmosSelected() { Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(StartPosition, leashRadius); Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, attackRadius); }
    }
}