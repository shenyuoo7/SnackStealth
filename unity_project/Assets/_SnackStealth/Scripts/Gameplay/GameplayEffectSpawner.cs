using System.Collections;
using UnityEngine;

namespace SnackStealth.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class GameplayEffectSpawner : MonoBehaviour
    {
        [SerializeField] private GameStateController gameState;
        [SerializeField] private Material caughtMaterial;
        [SerializeField] private Material eatMaterial;
        [SerializeField] private Material kickMaterial;
        [SerializeField] private Material victoryMaterial;

        public void Configure(
            GameStateController newGameState,
            Material newCaughtMaterial,
            Material newEatMaterial,
            Material newKickMaterial,
            Material newVictoryMaterial)
        {
            if (gameState != null)
            {
                gameState.Caught -= PlayCaughtEffect;
                gameState.Victory -= PlayVictoryEffect;
            }

            gameState = newGameState;
            caughtMaterial = newCaughtMaterial;
            eatMaterial = newEatMaterial;
            kickMaterial = newKickMaterial;
            victoryMaterial = newVictoryMaterial;

            if (isActiveAndEnabled && gameState != null)
            {
                gameState.Caught += PlayCaughtEffect;
                gameState.Victory += PlayVictoryEffect;
            }
        }

        private void OnEnable()
        {
            if (gameState == null)
            {
                return;
            }

            gameState.Caught += PlayCaughtEffect;
            gameState.Victory += PlayVictoryEffect;
        }

        private void OnDisable()
        {
            if (gameState == null)
            {
                return;
            }

            gameState.Caught -= PlayCaughtEffect;
            gameState.Victory -= PlayVictoryEffect;
        }

        public void PlayEatEffect(Vector3 position)
        {
            SpawnPulse(position, eatMaterial, 0.18f, 0.45f);
            SpawnCrumbBurst(position + Vector3.up * 0.08f, eatMaterial);

            if (Camera.main != null)
            {
                StartCoroutine(ShakeCamera(Camera.main.transform, 0.055f, 0.012f));
            }
        }

        public void PlaySeatKickEffect(Vector3 position)
        {
            SpawnPulse(position + Vector3.up * 0.75f, kickMaterial, 0.32f, 0.65f);
            SpawnImpactLines(position + Vector3.up * 0.85f, kickMaterial);

            if (Camera.main != null)
            {
                StartCoroutine(ShakeCamera(Camera.main.transform, 0.16f, 0.075f));
            }
        }

        private void PlayCaughtEffect()
        {
            SpawnPulse(Camera.main != null ? Camera.main.transform.position + Camera.main.transform.forward * 1.2f : Vector3.up * 1.5f, caughtMaterial, 0.45f, 0.55f);

            if (Camera.main != null)
            {
                StartCoroutine(ShakeCamera(Camera.main.transform, 0.32f, 0.11f));
            }
        }

        private void PlayVictoryEffect()
        {
            Vector3 position = Camera.main != null ? Camera.main.transform.position + Camera.main.transform.forward * 1.2f : Vector3.up * 1.5f;
            SpawnPulse(position, victoryMaterial, 0.5f, 0.9f);
            SpawnImpactLines(position, victoryMaterial);
        }

        private void SpawnPulse(Vector3 position, Material material, float startScale, float lifeSeconds)
        {
            GameObject pulse = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pulse.name = "EffectPulse_AutoDestroy";
            pulse.transform.position = position;
            pulse.transform.localScale = Vector3.one * startScale;

            Collider collider = pulse.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            MeshRenderer renderer = pulse.GetComponent<MeshRenderer>();
            if (renderer != null && material != null)
            {
                renderer.sharedMaterial = material;
            }

            StartCoroutine(AnimateAndDestroy(pulse, startScale, lifeSeconds));
        }

        private void SpawnImpactLines(Vector3 position, Material material)
        {
            const int lineCount = 10;
            for (int i = 0; i < lineCount; i++)
            {
                float angle = i * (360f / lineCount);
                Vector3 direction = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
                GameObject line = GameObject.CreatePrimitive(PrimitiveType.Cube);
                line.name = "KickImpactLine_AutoDestroy";
                line.transform.position = position + direction * 0.36f;
                line.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
                line.transform.localScale = new Vector3(0.045f, 0.045f, 0.72f);

                Collider collider = line.GetComponent<Collider>();
                if (collider != null)
                {
                    Destroy(collider);
                }

                MeshRenderer renderer = line.GetComponent<MeshRenderer>();
                if (renderer != null && material != null)
                {
                    renderer.sharedMaterial = material;
                }

                StartCoroutine(AnimateAndDestroy(line, 0.72f, 0.34f));
            }
        }

        private void SpawnCrumbBurst(Vector3 position, Material material)
        {
            const int crumbCount = 7;
            for (int i = 0; i < crumbCount; i++)
            {
                Vector3 direction = new Vector3(
                    Random.Range(-0.45f, 0.45f),
                    Random.Range(0.15f, 0.55f),
                    Random.Range(-0.45f, 0.45f)).normalized;

                GameObject crumb = GameObject.CreatePrimitive(PrimitiveType.Cube);
                crumb.name = "SnackCrumb_AutoDestroy";
                crumb.transform.position = position + direction * Random.Range(0.04f, 0.16f);
                crumb.transform.rotation = Random.rotation;
                crumb.transform.localScale = Vector3.one * Random.Range(0.025f, 0.055f);

                Collider collider = crumb.GetComponent<Collider>();
                if (collider != null)
                {
                    Destroy(collider);
                }

                MeshRenderer renderer = crumb.GetComponent<MeshRenderer>();
                if (renderer != null && material != null)
                {
                    renderer.sharedMaterial = material;
                }

                StartCoroutine(DriftAndDestroy(crumb, direction, 0.32f));
            }
        }

        private static IEnumerator AnimateAndDestroy(GameObject effect, float startScale, float lifeSeconds)
        {
            float timer = 0f;
            Vector3 baseScale = effect != null ? effect.transform.localScale : Vector3.one * startScale;
            while (timer < lifeSeconds && effect != null)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / lifeSeconds);
                effect.transform.localScale = Vector3.Lerp(baseScale, baseScale * 3.2f, t);
                yield return null;
            }

            if (effect != null)
            {
                Destroy(effect);
            }
        }

        private static IEnumerator DriftAndDestroy(GameObject effect, Vector3 direction, float lifeSeconds)
        {
            float timer = 0f;
            Vector3 startScale = effect != null ? effect.transform.localScale : Vector3.one * 0.04f;

            while (timer < lifeSeconds && effect != null)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / lifeSeconds);
                effect.transform.position += direction * (0.58f * Time.deltaTime * (1f - t));
                effect.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
                yield return null;
            }

            if (effect != null)
            {
                Destroy(effect);
            }
        }

        private static IEnumerator ShakeCamera(Transform cameraTransform, float duration, float amplitude)
        {
            float timer = 0f;
            while (timer < duration && cameraTransform != null)
            {
                timer += Time.deltaTime;
                float strength = 1f - Mathf.Clamp01(timer / duration);
                cameraTransform.position += Random.insideUnitSphere * (amplitude * strength);
                yield return null;
            }
        }
    }
}
