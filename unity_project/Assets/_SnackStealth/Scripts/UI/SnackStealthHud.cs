using SnackStealth.Gameplay;
using SnackStealth.Player;
using UnityEngine;

namespace SnackStealth.UI
{
    [DisallowMultipleComponent]
    public sealed class SnackStealthHud : MonoBehaviour
    {
        [SerializeField] private GameStateController gameState;
        [SerializeField] private PlayerSeatInteraction seatInteraction;

        private GUIStyle labelStyle;
        private GUIStyle promptStyle;
        private GUIStyle titleStyle;
        private GUIStyle subtitleStyle;
        private GUIStyle smallStyle;
        private GUIStyle barValueStyle;
        private Texture2D whiteTexture;

        public void Configure(GameStateController newGameState, PlayerSeatInteraction newSeatInteraction)
        {
            gameState = newGameState;
            seatInteraction = newSeatInteraction;
        }

        private void OnGUI()
        {
            if (gameState == null)
            {
                return;
            }

            EnsureGuiResources();
            DrawAlertFrame();

            if (gameState.Phase == GamePhase.Menu)
            {
                DrawOverlay(
                    "\u8bfe\u684c\u6f5c\u98df",
                    "\u968f\u5802\u6d4b\u9a8c\u5f00\u59cb\u4e86\uff0c\u800c\u4f60\u53c8\u6ca1\u5403\u65e9\u996d\u3002\n\u6309\u56de\u8f66\u952e\u5f00\u59cb");
                return;
            }

            if (gameState.Phase == GamePhase.Intro)
            {
                DrawOverlay(
                    "\u53ef\u6076\uff01\u53c8\u6ca1\u5403\u65e9\u996d\uff01",
                    "\u76ee\u6807\uff1a\u5750\u5230\u6709\u96f6\u98df\u7684\u8bfe\u684c\u524d\uff0c\u5403\u6ee1\u9971\u8179\u503c\uff0c\u907f\u5f00\u89c6\u7ebf\u3002");
                return;
            }

            DrawRuntimeHud();

            if (gameState.IsGameOver)
            {
                DrawOverlay(
                    gameState.HasWon ? "\u5403\u9971\u901a\u5173" : "\u4efb\u52a1\u5931\u8d25",
                    gameState.HasWon
                        ? "\u4f60\u5728\u8001\u5e08\u773c\u76ae\u5e95\u4e0b\u5b8c\u6210\u4e86\u65e9\u9910\u6551\u63f4\u3002\n\u6309 R \u91cd\u65b0\u5f00\u59cb"
                        : "\u88ab\u6293\u6b21\u6570\u7528\u5c3d\uff0c\u4eca\u5929\u7684\u96f6\u98df\u884c\u52a8\u7ed3\u675f\u4e86\u3002\n\u6309 R \u91cd\u65b0\u5f00\u59cb");
            }
        }

        private void DrawRuntimeHud()
        {
            Rect panel = new Rect(22f, 18f, 430f, 146f);
            DrawRect(panel, new Color(0.015f, 0.018f, 0.02f, 0.72f));
            DrawRect(new Rect(panel.x, panel.y, 4f, panel.height), new Color(0.95f, 0.72f, 0.22f, 0.95f));

            GUI.Label(new Rect(38f, 27f, 280f, 24f), "\u968f\u5802\u6d4b\u9a8c\u8fdb\u884c\u4e2d", smallStyle);
            GUI.Label(new Rect(324f, 27f, 100f, 24f), $"\u88ab\u6293 {gameState.CaughtCount}/{gameState.MaxCaughtCount}", smallStyle);

            DrawBar(
                new Rect(38f, 58f, 370f, 26f),
                "\u9971\u8179\u503c",
                gameState.Fullness01,
                new Color(0.28f, 0.95f, 0.48f),
                $"{gameState.Fullness:0}/{gameState.MaxFullness:0}");

            Color alertColor = Color.Lerp(new Color(1f, 0.78f, 0.18f), new Color(1f, 0.06f, 0.03f), gameState.Alert01);
            DrawBar(
                new Rect(38f, 94f, 370f, 26f),
                "\u8b66\u6212\u503c",
                gameState.Alert01,
                alertColor,
                $"{Mathf.RoundToInt(gameState.Alert01 * 100f)}%");

            GUI.Label(new Rect(38f, 126f, 380f, 28f), gameState.StatusMessage, labelStyle);

            DrawCrosshair();
            DrawPrompt();
        }

        private string GetPromptText()
        {
            if (gameState.IsGameOver)
            {
                return gameState.HasWon ? "\u9971\u8179\u503c\u5df2\u6ee1" : "\u884c\u52a8\u5931\u8d25";
            }

            if (gameState.IsPlayerVisible)
            {
                return "\u89c6\u7ebf\u9501\u5b9a\uff1a\u9a6c\u4e0a\u8eb2\u5f00\uff01";
            }

            if (seatInteraction != null && !string.IsNullOrWhiteSpace(seatInteraction.Prompt))
            {
                return seatInteraction.Prompt;
            }

            return "F \u952e\uff1a\u62a2\u5ea7 / \u8d77\u7acb   E \u952e\uff1a\u5750\u4e0b\u540e\u5077\u5403   \u7a7a\u683c\u952e\uff1a\u8df3\u8dc3";
        }

        private void DrawBar(Rect rect, string label, float value01, Color fillColor, string valueText)
        {
            DrawRect(rect, new Color(0f, 0f, 0f, 0.7f));
            DrawRect(new Rect(rect.x + 86f, rect.y + 5f, rect.width - 154f, rect.height - 10f), new Color(0.12f, 0.14f, 0.16f, 0.95f));

            Rect fillRect = new Rect(
                rect.x + 86f,
                rect.y + 5f,
                (rect.width - 154f) * Mathf.Clamp01(value01),
                rect.height - 10f);
            DrawRect(fillRect, fillColor);

            GUI.Label(new Rect(rect.x + 8f, rect.y + 3f, 78f, rect.height), label, labelStyle);
            GUI.Label(new Rect(rect.x + rect.width - 62f, rect.y + 3f, 58f, rect.height), valueText, barValueStyle);
        }

        private void DrawPrompt()
        {
            string prompt = GetPromptText();
            float width = Mathf.Min(Screen.width - 60f, 900f);
            Rect promptRect = new Rect((Screen.width - width) * 0.5f, Screen.height - 88f, width, 48f);
            DrawRect(promptRect, new Color(0.02f, 0.02f, 0.024f, 0.78f));
            DrawRect(new Rect(promptRect.x, promptRect.y, promptRect.width, 2f), new Color(1f, 0.78f, 0.22f, 0.82f));
            GUI.Label(promptRect, prompt, promptStyle);
        }

        private void DrawCrosshair()
        {
            if (!gameState.CanPlayerAct)
            {
                return;
            }

            float centerX = Screen.width * 0.5f;
            float centerY = Screen.height * 0.5f;
            Color color = gameState.IsPlayerVisible ? new Color(1f, 0.14f, 0.1f, 0.95f) : new Color(1f, 1f, 1f, 0.82f);

            DrawRect(new Rect(centerX - 1f, centerY - 1f, 2f, 2f), color);
            DrawRect(new Rect(centerX - 18f, centerY - 1f, 10f, 2f), color);
            DrawRect(new Rect(centerX + 8f, centerY - 1f, 10f, 2f), color);
            DrawRect(new Rect(centerX - 1f, centerY - 18f, 2f, 10f), color);
            DrawRect(new Rect(centerX - 1f, centerY + 8f, 2f, 10f), color);
        }

        private void DrawAlertFrame()
        {
            float intensity = gameState.IsPlayerVisible ? 0.75f : Mathf.Clamp01(gameState.Alert01) * 0.45f;
            if (intensity <= 0.01f)
            {
                return;
            }

            float pulse = 0.7f + Mathf.PingPong(Time.unscaledTime * 3.5f, 0.3f);
            Color color = new Color(1f, 0.03f, 0.02f, intensity * pulse);
            float thickness = Mathf.Lerp(10f, 28f, intensity);

            DrawRect(new Rect(0f, 0f, Screen.width, thickness), color);
            DrawRect(new Rect(0f, Screen.height - thickness, Screen.width, thickness), color);
            DrawRect(new Rect(0f, 0f, thickness, Screen.height), color);
            DrawRect(new Rect(Screen.width - thickness, 0f, thickness, Screen.height), color);
        }

        private void DrawOverlay(string title, string subtitle)
        {
            DrawRect(new Rect(0f, 0f, Screen.width, Screen.height), new Color(0f, 0f, 0f, 0.72f));
            DrawRect(new Rect(0f, Screen.height * 0.32f, Screen.width, 4f), new Color(0.95f, 0.72f, 0.22f, 0.9f));
            GUI.Label(new Rect(0f, Screen.height * 0.36f, Screen.width, 80f), title, titleStyle);
            GUI.Label(new Rect(Screen.width * 0.16f, Screen.height * 0.49f, Screen.width * 0.68f, 150f), subtitle, subtitleStyle);
        }

        private void DrawRect(Rect rect, Color color)
        {
            Color previousColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, whiteTexture);
            GUI.color = previousColor;
        }

        private void EnsureGuiResources()
        {
            whiteTexture ??= Texture2D.whiteTexture;

            labelStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 17,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            smallStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.92f, 0.92f, 0.86f) }
            };

            barValueStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleRight,
                normal = { textColor = Color.white }
            };

            promptStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 25,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };

            titleStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 54,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };

            subtitleStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperCenter,
                wordWrap = true,
                normal = { textColor = new Color(0.94f, 0.94f, 0.9f) }
            };
        }
    }
}
