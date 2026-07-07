using UnityEngine;

namespace SnackStealth.Items
{
    [DisallowMultipleComponent]
    public sealed class SnackStash : MonoBehaviour
    {
        [SerializeField] private string snackName = "\u5de7\u514b\u529b\u68d2";
        [SerializeField, Range(0f, 20f)] private float fullnessRemaining = 20f;

        public string SnackName => snackName;
        public float FullnessRemaining => fullnessRemaining;
        public float Fullness01 => Mathf.Clamp01(fullnessRemaining / 20f);
        public bool HasSnack => fullnessRemaining > 0.01f;

        public void Configure(string newSnackName, float startingFullness)
        {
            snackName = newSnackName;
            fullnessRemaining = Mathf.Clamp(startingFullness, 0f, 20f);
        }

        public float Consume(float requestedFullness)
        {
            if (requestedFullness <= 0f || fullnessRemaining <= 0f)
            {
                return 0f;
            }

            float consumed = Mathf.Min(fullnessRemaining, requestedFullness);
            fullnessRemaining -= consumed;
            return consumed;
        }
    }
}
