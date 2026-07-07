using SnackStealth.Gameplay;
using UnityEngine;

namespace SnackStealth.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerSeatInteraction : MonoBehaviour
    {
        [SerializeField] private GameStateController gameState;
        [SerializeField] private GameplayEffectSpawner effects;
        [SerializeField] private PlayerMovementController movementController;
        [SerializeField] private Transform[] podiumSpots;
        [SerializeField] private SeatStation initialSeat;
        [SerializeField] private KeyCode sitKey = KeyCode.F;
        [SerializeField, Min(0.1f)] private float interactionRadius = 1.25f;
        [SerializeField] private Vector3 seatedLocalOffset = Vector3.zero;

        private int nextPodiumSpotIndex;

        public SeatStation CurrentSeat { get; private set; }
        public SeatStation NearbySeat { get; private set; }
        public bool IsSeated => CurrentSeat != null;

        public string Prompt
        {
            get
            {
                if (gameState != null && !gameState.CanPlayerAct)
                {
                    return string.Empty;
                }

                if (CurrentSeat != null)
                {
                    return CurrentSeat.HasSnack
                        ? $"\u6309\u4f4f E \u952e\uff1a\u5077\u5403\u300c{CurrentSeat.SnackName}\u300d\uff08\u5269\u4f59 {CurrentSeat.SnackRemaining:0.0}%\uff09  |  F \u952e\uff1a\u8d77\u7acb"
                        : "\u8fd9\u4e2a\u684c\u6d1e\u7a7a\u7a7a\u5982\u4e5f\uff0c\u6309 F \u952e\u8d77\u7acb\u3002";
                }

                if (NearbySeat == null)
                {
                    return "\u9760\u8fd1\u8bfe\u684c\uff0c\u6309 F \u952e\u62a2\u5ea7\u3002";
                }

                return NearbySeat.HasOccupant ? "\u6309 F \u952e\uff1a\u628a\u540c\u5b66\u6324\u4e0b\u5ea7\u4f4d" : "\u6309 F \u952e\uff1a\u5750\u4e0b";
            }
        }

        public void Configure(GameStateController newGameState, GameplayEffectSpawner newEffects, Transform[] newPodiumSpots)
        {
            gameState = newGameState;
            effects = newEffects;
            podiumSpots = newPodiumSpots;
            seatedLocalOffset = Vector3.zero;
            interactionRadius = 1.35f;
        }

        public void SetInitialSeat(SeatStation seat)
        {
            initialSeat = seat;
            CurrentSeat = seat;
            if (CurrentSeat != null)
            {
                CurrentSeat.SetPlayerSeated(true);
                transform.SetPositionAndRotation(GetSeatedPosition(CurrentSeat), CurrentSeat.SitPoint.rotation);
            }

            if (movementController != null)
            {
                movementController.SetMovementEnabled(CurrentSeat == null);
            }
        }

        private void Awake()
        {
            if (movementController == null)
            {
                movementController = GetComponent<PlayerMovementController>();
            }
        }

        private void Start()
        {
            if (CurrentSeat == null && initialSeat != null)
            {
                CurrentSeat = initialSeat;
                CurrentSeat.SetPlayerSeated(true);
                transform.SetPositionAndRotation(GetSeatedPosition(CurrentSeat), CurrentSeat.SitPoint.rotation);

                if (movementController != null)
                {
                    movementController.SetMovementEnabled(false);
                }
            }
        }

        private void Update()
        {
            if (gameState == null || !gameState.CanPlayerAct)
            {
                return;
            }

            NearbySeat = SeatStation.FindNearest(transform.position, interactionRadius);

            if (Input.GetKeyDown(sitKey))
            {
                if (CurrentSeat != null)
                {
                    StandUp();
                }
                else if (NearbySeat != null)
                {
                    SitAt(NearbySeat);
                }
            }

            if (CurrentSeat != null && CurrentSeat.SitPoint != null)
            {
                transform.position = GetSeatedPosition(CurrentSeat);
            }
        }

        private void SitAt(SeatStation seat)
        {
            if (seat == null)
            {
                return;
            }

            if (seat.HasOccupant)
            {
                seat.EjectOccupant(GetNextPodiumSpot(), effects);
                gameState.SetStatus("\u62a2\u5ea7\u6210\u529f\uff01\u522b\u592a\u5f97\u610f\uff0c\u8bb2\u53f0\u4e0a\u591a\u4e86\u4e00\u53cc\u773c\u775b\u3002");
            }
            else
            {
                gameState.SetStatus("\u4f60\u5750\u4e0b\u6765\uff0c\u6084\u6084\u6478\u5411\u684c\u6d1e\u3002");
            }

            if (CurrentSeat != null)
            {
                CurrentSeat.SetPlayerSeated(false);
            }

            CurrentSeat = seat;
            CurrentSeat.SetPlayerSeated(true);
            transform.SetPositionAndRotation(GetSeatedPosition(CurrentSeat), CurrentSeat.SitPoint.rotation);

            if (movementController != null)
            {
                movementController.SetMovementEnabled(false);
            }
        }

        private void StandUp()
        {
            if (CurrentSeat != null)
            {
                CurrentSeat.SetPlayerSeated(false);
            }

            CurrentSeat = null;
            if (movementController != null)
            {
                movementController.SetMovementEnabled(true);
            }

            gameState.SetStatus("\u4f60\u7ad9\u8d77\u6765\u4e86\uff0c\u53bb\u627e\u4e0b\u4e00\u4efd\u96f6\u98df\u3002");
        }

        private Transform GetNextPodiumSpot()
        {
            if (podiumSpots == null || podiumSpots.Length == 0)
            {
                return null;
            }

            if (nextPodiumSpotIndex >= podiumSpots.Length)
            {
                return null;
            }

            Transform spot = podiumSpots[nextPodiumSpotIndex];
            nextPodiumSpotIndex++;
            return spot;
        }

        private Vector3 GetSeatedPosition(SeatStation seat)
        {
            if (seat == null || seat.SitPoint == null)
            {
                return transform.position;
            }

            return seat.SitPoint.TransformPoint(seatedLocalOffset);
        }
    }
}
