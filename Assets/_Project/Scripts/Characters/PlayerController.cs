// File: Assets/_Project/Scripts/Characters/PlayerController.cs
// Version: 2.0 (Fully Refactored & Compliant)

using UnityEngine;
using Photon.Pun;
using Platformer.Core;

namespace Platformer
{
    [RequireComponent(typeof(StateMachine), typeof(PlayerMovement), typeof(PlayerCombat), typeof(PlayerScoring), typeof(CharacterStats))]
    public class PlayerController : MonoBehaviour, IPunObservable
    {
        [Header("Asset References")]
        [SerializeField] private InputReader inputReader;

        [Header("Visuals & Debugging")]
        public GameObject scoringVisual;
        public Color idleColor = Color.blue;
        public Color groundedColor = Color.green;
        public Color airborneColor = Color.red;
        public Color highJumpColor = Color.yellow;
        public Color doubleJumpColor = Color.magenta;
        public Color empoweredAttackColor = Color.white;
        public Color attackColor = new Color(1f, 0.5f, 0f);
        
        public StateMachine StateMachine { get; private set; }
        public PlayerMovement Movement { get; private set; }
        public PlayerCombat Combat { get; private set; }
        public PlayerScoring Scoring { get; private set; }
        public CharacterStats MyStats { get; private set; }
        public PhotonView PhotonView { get; private set; }
        private Material _debugMaterialInstance;
        
        public Vector2 MoveInput { get; private set; }
        public bool IsJumpButtonPressed { get; private set; }
        public bool IsScoreButtonPressed { get; private set; }
        
        private float _jumpBufferTimer;
        private bool _attackInputPressed;
        private const float JUMP_BUFFER_TIME = 0.15f;
        private Vector3 _networkPosition;
        private Quaternion _networkRotation;

        private void Awake()
        {
            StateMachine = GetComponent<StateMachine>();
            Movement = GetComponent<PlayerMovement>();
            Combat = GetComponent<PlayerCombat>();
            Scoring = GetComponent<PlayerScoring>();
            MyStats = GetComponent<CharacterStats>();
            PhotonView = GetComponent<PhotonView>();
            
            var renderer = GetComponentInChildren<Renderer>();
            if (renderer != null) _debugMaterialInstance = renderer.material;
        }

        private void Start()
        {
            StateMachine.SetState(new PlayerIdleState(this, StateMachine));
            if (PhotonView.IsMine) FindFirstObjectByType<CameraController>()?.SetTarget(this.transform);
        }

        private void OnEnable()
        {
            if (inputReader == null) return;
            inputReader.MoveEvent += OnMove;
            inputReader.JumpEvent += OnJump;
            inputReader.JumpCancelledEvent += OnJumpCancelled;
            inputReader.AttackEvent += OnAttack;
            inputReader.ScoreEvent += OnScore;
            inputReader.ScoreCancelledEvent += OnScoreCancelled;
        }

        private void OnDisable()
        {
            if (inputReader == null) return;
            inputReader.MoveEvent -= OnMove;
            inputReader.JumpEvent -= OnJump;
            inputReader.JumpCancelledEvent -= OnJumpCancelled;
            inputReader.AttackEvent -= OnAttack;
            inputReader.ScoreEvent -= OnScore;
            inputReader.ScoreCancelledEvent -= OnScoreCancelled;
        }

        private void Update()
        {
            if (!PhotonView.IsMine) { ApplyNetworkData(); return; }
            _jumpBufferTimer -= Time.deltaTime;
            StateMachine.Tick(MoveInput);
        }

        private void FixedUpdate()
        {
            if (!PhotonView.IsMine) return;
            StateMachine.FixedTick();
        }

        private void ApplyNetworkData()
        {
            transform.position = Vector3.Lerp(transform.position, _networkPosition, Time.deltaTime * 15f);
            transform.rotation = Quaternion.Lerp(transform.rotation, _networkRotation, Time.deltaTime * 15f);
        }

        private void OnMove(Vector2 move) => MoveInput = move;
        private void OnJump() { if (!IsJumpButtonPressed) _jumpBufferTimer = JUMP_BUFFER_TIME; IsJumpButtonPressed = true; }
        private void OnJumpCancelled() => IsJumpButtonPressed = false;
        private void OnAttack() => _attackInputPressed = true;
        private void OnScore() => IsScoreButtonPressed = true;
        private void OnScoreCancelled() => IsScoreButtonPressed = false;
        
        public bool ConsumeAttackPress() { bool p = _attackInputPressed; _attackInputPressed = false; return p; }
        public bool ConsumeJumpBuffer() { bool b = _jumpBufferTimer > 0f; _jumpBufferTimer = 0f; return b; }
        public void SetDebugColor(Color color) { if (_debugMaterialInstance != null) _debugMaterialInstance.color = color; }
        public void SetScoringVisual(bool active) { if (scoringVisual != null) scoringVisual.SetActive(active); }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(transform.position);
                stream.SendNext(transform.rotation);
            }
            else
            {
                _networkPosition = (Vector3)stream.ReceiveNext();
                _networkRotation = (Quaternion)stream.ReceiveNext();
            }
        }
    }
}