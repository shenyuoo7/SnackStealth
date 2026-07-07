using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SnackStealth.Gameplay
{
    public enum GamePhase
    {
        Menu,
        Intro,
        Playing,
        Won,
        Failed
    }

    [DisallowMultipleComponent]
    public sealed class GameStateController : MonoBehaviour
    {
        [Header("Fullness")]
        [SerializeField, Min(1f)] private float maxFullness = 100f;
        [SerializeField, Range(0f, 100f)] private float caughtFullnessPenalty = 40f;

        [Header("Detection")]
        [SerializeField, Min(1f)] private float maxAlert = 100f;
        [SerializeField, Min(0f)] private float seenAlertPerSecond = 30f;
        [SerializeField, Min(0f)] private float alertDecayPerSecond = 18f;
        [SerializeField, Min(0f)] private float seenMemorySeconds = 0.12f;
        [SerializeField, Min(0f)] private float detectionGraceSeconds = 0.85f;
        [SerializeField, Min(0f)] private float caughtRecoverySeconds = 2f;

        [Header("Rules")]
        [SerializeField, Min(1)] private int maxCaughtCount = 3;
        [SerializeField, Min(0f)] private float introSeconds = 2.4f;

        private float fullness;
        private float alert;
        private float firstSeenTime = -999f;
        private float lastSeenTime = -999f;
        private float ignoreDetectionUntil = -999f;
        private int caughtCount;
        private bool isEating;
        private float introTimer;
        private string lastSpotter = "\u8001\u5e08";
        private string statusMessage = "\u6309\u56de\u8f66\u952e\u5f00\u59cb\u968f\u5802\u6d4b\u9a8c\u3002";

        public event Action StateChanged;
        public event Action Caught;
        public event Action Victory;

        public GamePhase Phase { get; private set; } = GamePhase.Menu;
        public float Fullness01 => Mathf.Clamp01(fullness / maxFullness);
        public float Alert01 => Mathf.Clamp01(alert / maxAlert);
        public float Fullness => fullness;
        public float MaxFullness => maxFullness;
        public int CaughtCount => caughtCount;
        public int MaxCaughtCount => maxCaughtCount;
        public bool IsPlayerVisible => Phase == GamePhase.Playing && Time.time - lastSeenTime <= seenMemorySeconds;
        public bool IsEating => isEating;
        public bool IsGameOver => Phase == GamePhase.Won || Phase == GamePhase.Failed;
        public bool HasWon => Phase == GamePhase.Won;
        public bool CanPlayerAct => Phase == GamePhase.Playing;
        public string StatusMessage => statusMessage;

        private void Update()
        {
            if (Phase == GamePhase.Menu)
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    StartIntro();
                }

                return;
            }

            if (Phase == GamePhase.Intro)
            {
                introTimer -= Time.deltaTime;
                if (introTimer <= 0f)
                {
                    Phase = GamePhase.Playing;
                    SetStatus("\u627e\u4e00\u5f20\u8bfe\u684c\uff1aF \u62a2\u5ea7/\u8d77\u7acb\uff0c\u6309\u4f4f E \u5728\u5ea7\u4f4d\u4e0a\u5077\u5403\u3002");
                }

                return;
            }

            if (Phase != GamePhase.Playing)
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    RestartRound();
                }

                return;
            }

            if (Time.time < ignoreDetectionUntil)
            {
                ClearSightMemory();
                DecayAlertIfSafe();
                return;
            }

            if (IsPlayerVisible)
            {
                float seenDuration = Mathf.Max(0f, Time.time - firstSeenTime);
                if (seenDuration < detectionGraceSeconds)
                {
                    SetStatus($"{lastSpotter}\u597d\u50cf\u6ce8\u610f\u5230\u4f60\u4e86\uff0c\u5feb\u907f\u5f00\u89c6\u7ebf\u3002");
                    return;
                }

                AddAlert(seenAlertPerSecond * Time.deltaTime);
                SetStatus($"{lastSpotter}\u6b63\u5728\u76ef\u7740\u4f60\uff0c\u7acb\u523b\u8eb2\u5f00\uff01");
            }
            else
            {
                ClearSightMemory();
                DecayAlertIfSafe();
            }
        }

        public void StartIntro()
        {
            if (Phase != GamePhase.Menu)
            {
                return;
            }

            Phase = GamePhase.Intro;
            introTimer = introSeconds;
            SetStatus("\u53ef\u6076\uff01\u53c8\u6ca1\u5403\u65e9\u996d\uff01");
        }

        public void ReportPlayerSpotted(string spotterName)
        {
            if (Phase != GamePhase.Playing)
            {
                return;
            }

            if (Time.time < ignoreDetectionUntil)
            {
                return;
            }

            bool wasRecentlySeen = Time.time - lastSeenTime <= seenMemorySeconds;
            lastSeenTime = Time.time;
            if (!wasRecentlySeen)
            {
                firstSeenTime = Time.time;
            }

            lastSpotter = NormalizeSpotterName(spotterName);
        }

        public void SetEating(bool eating)
        {
            if (Phase != GamePhase.Playing || isEating == eating)
            {
                return;
            }

            isEating = eating;
            SetStatus(eating ? "\u5c0f\u70b9\u58f0\uff0c\u522b\u8ba9\u8001\u5e08\u542c\u89c1\u2026\u2026" : "\u88c5\u4f5c\u8ba4\u771f\u5199\u8bd5\u5377\u3002");
        }

        public void AddFullness(float amount)
        {
            if (Phase != GamePhase.Playing || amount <= 0f)
            {
                return;
            }

            fullness = Mathf.Min(maxFullness, fullness + amount);
            RaiseChanged();

            if (fullness >= maxFullness)
            {
                Phase = GamePhase.Won;
                isEating = false;
                alert = 0f;
                SetStatus("\u9971\u8179\u503c\u5df2\u6ee1\uff0c\u4f60\u71ac\u8fc7\u4e86\u8fd9\u573a\u968f\u5802\u6d4b\u9a8c\uff01");
                Victory?.Invoke();
            }
        }

        public void AddAlert(float amount)
        {
            if (Phase != GamePhase.Playing || amount <= 0f)
            {
                return;
            }

            alert = Mathf.Min(maxAlert, alert + amount);
            RaiseChanged();

            if (alert >= maxAlert)
            {
                RegisterCaught();
            }
        }

        public void AddEatingNoiseAlert(float amount)
        {
            if (Phase == GamePhase.Playing)
            {
                alert = Mathf.Min(maxAlert, alert + amount);
                RaiseChanged();

                if (alert >= maxAlert)
                {
                    RegisterCaught();
                }
            }
        }

        public void SetStatus(string message)
        {
            if (statusMessage == message)
            {
                return;
            }

            statusMessage = message;
            RaiseChanged();
        }

        public void RestartRound()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (!string.IsNullOrEmpty(activeScene.name) && Application.CanStreamedLevelBeLoaded(activeScene.name))
            {
                SceneManager.LoadScene(activeScene.name);
                return;
            }

            fullness = 0f;
            alert = 0f;
            caughtCount = 0;
            isEating = false;
            ClearSightMemory();
            ignoreDetectionUntil = -999f;
            Phase = GamePhase.Menu;
            SetStatus("\u6309\u56de\u8f66\u952e\u5f00\u59cb\u968f\u5802\u6d4b\u9a8c\u3002");
        }

        private void RegisterCaught()
        {
            caughtCount++;
            alert = 0f;
            fullness = Mathf.Max(0f, fullness - caughtFullnessPenalty);
            isEating = false;
            ignoreDetectionUntil = Time.time + caughtRecoverySeconds;
            ClearSightMemory();
            Caught?.Invoke();

            if (caughtCount >= maxCaughtCount)
            {
                Phase = GamePhase.Failed;
                SetStatus("\u88ab\u6293\u4e09\u6b21\uff0c\u5077\u5403\u4efb\u52a1\u5931\u8d25\u3002");
                return;
            }

            SetStatus($"\u88ab\u6293\uff01\u9971\u8179\u503c -{caughtFullnessPenalty:0}%\uff08{caughtCount}/{maxCaughtCount}\uff09\uff0c\u8fd8\u6709\u673a\u4f1a\u3002");
        }

        private void RaiseChanged()
        {
            StateChanged?.Invoke();
        }

        private void ClearSightMemory()
        {
            firstSeenTime = -999f;
            lastSeenTime = -999f;
        }

        private void DecayAlertIfSafe()
        {
            if (!isEating && alert > 0f)
            {
                alert = Mathf.Max(0f, alert - alertDecayPerSecond * Time.deltaTime);
                RaiseChanged();
            }
        }

        private static string NormalizeSpotterName(string spotterName)
        {
            if (string.IsNullOrWhiteSpace(spotterName) || spotterName == "teacher")
            {
                return "\u8001\u5e08";
            }

            if (spotterName == "punished classmate")
            {
                return "\u7f5a\u7ad9\u540c\u5b66";
            }

            return spotterName;
        }
    }
}
