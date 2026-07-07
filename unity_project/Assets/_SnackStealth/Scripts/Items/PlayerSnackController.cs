using SnackStealth.Gameplay;
using SnackStealth.Player;
using UnityEngine;

namespace SnackStealth.Items
{
    [DisallowMultipleComponent]
    public sealed class PlayerSnackController : MonoBehaviour
    {
        [SerializeField] private GameStateController gameState;
        [SerializeField] private PlayerSeatInteraction seatInteraction;
        [SerializeField] private GameplayEffectSpawner effects;
        [SerializeField] private FirstPersonSnackView firstPersonSnackView;
        [SerializeField] private KeyCode eatKey = KeyCode.E;
        [SerializeField, Min(0f)] private float fullnessPerSecond = 10f;
        [SerializeField, Min(0f)] private float eatingAlertPerSecond = 2.5f;
        [SerializeField, Min(0.05f)] private float effectInterval = 0.18f;

        private float effectTimer;

        public void Configure(
            GameStateController newGameState,
            PlayerSeatInteraction newSeatInteraction,
            GameplayEffectSpawner newEffects,
            FirstPersonSnackView newFirstPersonSnackView)
        {
            gameState = newGameState;
            seatInteraction = newSeatInteraction;
            effects = newEffects;
            firstPersonSnackView = newFirstPersonSnackView;
        }

        private void Awake()
        {
            if (seatInteraction == null)
            {
                seatInteraction = GetComponent<PlayerSeatInteraction>();
            }
        }

        private void Update()
        {
            if (gameState == null || !gameState.CanPlayerAct)
            {
                SetEating(false);
                return;
            }

            SeatStation currentSeat = seatInteraction != null ? seatInteraction.CurrentSeat : null;
            bool canEat = currentSeat != null && currentSeat.HasSnack;
            bool wantsToEat = canEat && Input.GetKey(eatKey);

            SetEating(wantsToEat);

            if (!wantsToEat)
            {
                return;
            }

            float consumed = currentSeat.ConsumeSnack(fullnessPerSecond * Time.deltaTime);
            if (consumed <= 0f)
            {
                gameState.SetStatus("\u8fd9\u4e2a\u684c\u6d1e\u5df2\u7ecf\u88ab\u4f60\u5403\u7a7a\u4e86\u3002");
                return;
            }

            gameState.AddFullness(consumed);
            gameState.AddEatingNoiseAlert(eatingAlertPerSecond * Time.deltaTime);

            effectTimer -= Time.deltaTime;
            if (effectTimer <= 0f)
            {
                effectTimer = effectInterval;
                Vector3 effectPosition = currentSeat.SnackPoint != null ? currentSeat.SnackPoint.position : transform.position + transform.forward * 0.5f;
                if (effects != null)
                {
                    effects.PlayEatEffect(effectPosition);
                }
            }
        }

        private void OnDisable()
        {
            SetEating(false);
        }

        private void SetEating(bool eating)
        {
            if (gameState != null)
            {
                gameState.SetEating(eating);
            }

            if (firstPersonSnackView != null)
            {
                firstPersonSnackView.SetEating(eating);
            }
        }
    }
}
