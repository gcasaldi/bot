using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PopAndStack
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Gameplay")]
        [SerializeField] private int maxLevel = 6;
        [SerializeField] private float spawnHeight = 4.0f;
        [SerializeField] private float spawnImpulse = 1.2f;
        [SerializeField] private float gameOverLineOffset = 0.8f;
        [SerializeField] private float wallThickness = 0.6f;
        [SerializeField] private float gravityPhaseDuration = 10f;
        [SerializeField] private float comboWindow = 2.0f;
        [SerializeField] private int maxComboMultiplier = 5;

        [Header("Physics")]
        [SerializeField] private Vector2 gravity = new Vector2(0f, -18f);

        [Header("Audio")]
        [SerializeField] private float sfxVolume = 0.4f;

        private readonly List<Color> levelColors = new List<Color>
        {
            new Color(0.98f, 0.72f, 0.26f),
            new Color(0.31f, 0.84f, 0.63f),
            new Color(0.35f, 0.61f, 0.98f),
            new Color(0.94f, 0.42f, 0.62f),
            new Color(0.54f, 0.38f, 0.91f),
            new Color(0.98f, 0.58f, 0.23f),
            new Color(0.96f, 0.30f, 0.24f)
        };

        private Camera mainCamera;
        private Text scoreText;
        private Text stateText;
        private Text comboText;
        private Text phaseText;
        private int score;
        private bool gameOver;
        private AudioSource sfxSource;
        private AudioClip dropClip;
        private AudioClip mergeClip;
        private AudioClip gameOverClip;
        private Vector2[] gravityDirections;
        private string[] gravityLabels;
        private int gravityPhaseIndex;
        private float nextGravityPhaseTime;
        private float gravityStrength;
        private BoxCollider2D gameOverCollider;
        private Transform gameOverTransform;
        private float lastMergeTime;
        private int comboCount;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            mainCamera = Camera.main;
        }

        private void Start()
        {
            if (mainCamera == null)
            {
                mainCamera = CreateCamera();
            }

            gravityStrength = gravity.magnitude;
            CreateBounds();
            InitializeGravityPhases();
            CreateUi();
            PrepareAudio();
        }

        private void Update()
        {
            if (gameOver)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Restart();
                }
                return;
            }

            UpdateGravityPhase();
            UpdateComboDecay();

            if (Input.GetMouseButtonDown(0))
            {
                Vector2 worldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                SpawnBall(worldPos.x);
                PlayClip(dropClip, 0.35f);
            }
        }

        public void MergeBalls(Ball first, Ball second)
        {
            if (gameOver)
            {
                return;
            }

            int nextLevel = Mathf.Min(first.Level + 1, maxLevel);
            Vector2 spawnPos = (first.transform.position + second.transform.position) * 0.5f;

            Destroy(first.gameObject);
            Destroy(second.gameObject);

            GameObject merged = CreateBall(nextLevel, spawnPos);
            Rigidbody2D rb = merged.GetComponent<Rigidbody2D>();
            rb.AddForce(Vector2.up * spawnImpulse, ForceMode2D.Impulse);

            float now = Time.time;
            if (now - lastMergeTime <= comboWindow)
            {
                comboCount += 1;
            }
            else
            {
                comboCount = 1;
            }

            lastMergeTime = now;
            int multiplier = Mathf.Clamp(comboCount, 1, maxComboMultiplier);
            score += (nextLevel + 1) * 10 * multiplier;
            UpdateScore();
            UpdateComboText();
            PlayClip(mergeClip, 0.6f);
        }

        public void TriggerGameOver()
        {
            if (gameOver)
            {
                return;
            }

            gameOver = true;
            stateText.text = "Game Over\nTap to Restart";
            PlayClip(gameOverClip, 0.6f);
        }

        private void SpawnBall(float xWorld)
        {
            float clampedX = ClampSpawnX(xWorld);
            CreateBall(0, new Vector2(clampedX, spawnHeight));
        }

        private GameObject CreateBall(int level, Vector2 position)
        {
            GameObject ballObject = new GameObject($"Ball_{level}");
            ballObject.transform.position = position;

            float radius = 0.45f + 0.15f * level;

            SpriteRenderer renderer = ballObject.AddComponent<SpriteRenderer>();
            renderer.sprite = CreateCircleSprite(64, levelColors[level]);
            renderer.color = levelColors[level];
            renderer.sortingOrder = 1;

            CircleCollider2D collider = ballObject.AddComponent<CircleCollider2D>();
            collider.radius = radius;

            Rigidbody2D rb = ballObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 1.0f;
            rb.mass = 0.8f + 0.2f * level;
            rb.angularDrag = 0.8f;
            rb.drag = 0.2f;

            Ball ball = ballObject.AddComponent<Ball>();
            ball.Level = level;

            return ballObject;
        }

        private float ClampSpawnX(float worldX)
        {
            float halfWidth = mainCamera.orthographicSize * mainCamera.aspect;
            float padding = 0.6f;
            return Mathf.Clamp(worldX, -halfWidth + padding, halfWidth - padding);
        }

        private void CreateBounds()
        {
            float halfHeight = mainCamera.orthographicSize;
            float halfWidth = halfHeight * mainCamera.aspect;

            CreateWall("LeftWall", new Vector2(-halfWidth - wallThickness * 0.5f, 0f),
                new Vector2(wallThickness, halfHeight * 2f));
            CreateWall("RightWall", new Vector2(halfWidth + wallThickness * 0.5f, 0f),
                new Vector2(wallThickness, halfHeight * 2f));
            CreateWall("Floor", new Vector2(0f, -halfHeight - wallThickness * 0.5f),
                new Vector2(halfWidth * 2f, wallThickness));
            CreateWall("Ceiling", new Vector2(0f, halfHeight + wallThickness * 0.5f),
                new Vector2(halfWidth * 2f, wallThickness));

            CreateGameOverLine(new Vector2(0f, halfHeight - gameOverLineOffset),
                new Vector2(halfWidth * 2f, 0.4f));
        }

        private void CreateWall(string name, Vector2 position, Vector2 size)
        {
            GameObject wall = new GameObject(name);
            wall.transform.position = position;
            BoxCollider2D collider = wall.AddComponent<BoxCollider2D>();
            collider.size = size;
        }

        private void CreateGameOverLine(Vector2 position, Vector2 size)
        {
            GameObject line = new GameObject("GameOverLine");
            line.transform.position = position;
            BoxCollider2D collider = line.AddComponent<BoxCollider2D>();
            collider.size = size;
            collider.isTrigger = true;
            line.AddComponent<GameOverLine>();
            gameOverCollider = collider;
            gameOverTransform = line.transform;
        }

        private void CreateUi()
        {
            GameObject canvasObject = new GameObject("Canvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObject.AddComponent<GraphicRaycaster>();

            scoreText = CreateText(canvas.transform, "ScoreText", new Vector2(0.5f, 1f),
                new Vector2(0f, -20f), TextAnchor.UpperCenter, 26);
            comboText = CreateText(canvas.transform, "ComboText", new Vector2(0.5f, 1f),
                new Vector2(0f, -55f), TextAnchor.UpperCenter, 20);
            phaseText = CreateText(canvas.transform, "PhaseText", new Vector2(1f, 1f),
                new Vector2(-20f, -20f), TextAnchor.UpperRight, 18);
            stateText = CreateText(canvas.transform, "StateText", new Vector2(0.5f, 0.5f),
                Vector2.zero, TextAnchor.MiddleCenter, 30);
            stateText.text = "Tap to Drop";

            UpdateScore();
            UpdateComboText();
            UpdatePhaseText();
        }

        private Text CreateText(Transform parent, string name, Vector2 anchor, Vector2 offset, TextAnchor alignment, int fontSize)
        {
            GameObject textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);

            RectTransform rect = textObject.AddComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.anchoredPosition = offset;
            rect.sizeDelta = new Vector2(400f, 80f);

            Text text = textObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.alignment = alignment;
            text.fontSize = fontSize;
            text.color = Color.white;
            return text;
        }

        private void UpdateScore()
        {
            if (scoreText != null)
            {
                scoreText.text = $"Score: {score}";
            }
        }

        private void UpdateComboText()
        {
            if (comboText == null)
            {
                return;
            }

            if (comboCount > 1)
            {
                int multiplier = Mathf.Clamp(comboCount, 1, maxComboMultiplier);
                comboText.text = $"Combo x{multiplier}";
            }
            else
            {
                comboText.text = string.Empty;
            }
        }

        private void UpdatePhaseText()
        {
            if (phaseText != null && gravityLabels != null && gravityPhaseIndex < gravityLabels.Length)
            {
                phaseText.text = $"Gravity: {gravityLabels[gravityPhaseIndex]}";
            }
        }

        private void PrepareAudio()
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;

            dropClip = BuildTone(540f, 0.08f);
            mergeClip = BuildTone(680f, 0.10f);
            gameOverClip = BuildTone(220f, 0.18f);
        }

        private AudioClip BuildTone(float frequency, float duration)
        {
            int sampleRate = 44100;
            int sampleLength = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[sampleLength];

            for (int i = 0; i < sampleLength; i++)
            {
                float t = (float)i / sampleRate;
                samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * 0.3f;
            }

            AudioClip clip = AudioClip.Create($"tone_{frequency}", sampleLength, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private void PlayClip(AudioClip clip, float volume)
        {
            if (clip == null)
            {
                return;
            }

            sfxSource.PlayOneShot(clip, volume * sfxVolume);
        }

        private Camera CreateCamera()
        {
            GameObject cameraObject = new GameObject("MainCamera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5.5f;
            camera.backgroundColor = new Color(0.12f, 0.14f, 0.2f);
            cameraObject.tag = "MainCamera";
            return camera;
        }

        private Sprite CreateCircleSprite(int size, Color color)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;

            float radius = size * 0.5f;
            Vector2 center = new Vector2(radius, radius);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    Color pixel = distance <= radius ? color : new Color(0f, 0f, 0f, 0f);
                    texture.SetPixel(x, y, pixel);
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size / (radius * 2f));
        }

        private void InitializeGravityPhases()
        {
            gravityDirections = new[] { Vector2.down, Vector2.left, Vector2.up, Vector2.right };
            gravityLabels = new[] { "Down", "Left", "Up", "Right" };
            gravityPhaseIndex = 0;
            nextGravityPhaseTime = Time.time + gravityPhaseDuration;
            ApplyGravityPhase(gravityPhaseIndex);
        }

        private void UpdateGravityPhase()
        {
            if (Time.time < nextGravityPhaseTime)
            {
                return;
            }

            gravityPhaseIndex = (gravityPhaseIndex + 1) % gravityDirections.Length;
            nextGravityPhaseTime = Time.time + gravityPhaseDuration;
            ApplyGravityPhase(gravityPhaseIndex);
        }

        private void ApplyGravityPhase(int index)
        {
            Vector2 direction = gravityDirections[index];
            Physics2D.gravity = direction * gravityStrength;
            UpdateGameOverLine(direction);
            UpdatePhaseText();
        }

        private void UpdateGameOverLine(Vector2 gravityDirection)
        {
            if (gameOverTransform == null || gameOverCollider == null)
            {
                return;
            }

            float halfHeight = mainCamera.orthographicSize;
            float halfWidth = halfHeight * mainCamera.aspect;

            if (Mathf.Abs(gravityDirection.x) > 0.1f)
            {
                float xPos = -Mathf.Sign(gravityDirection.x) * (halfWidth - gameOverLineOffset);
                gameOverTransform.position = new Vector2(xPos, 0f);
                gameOverCollider.size = new Vector2(0.4f, halfHeight * 2f);
            }
            else
            {
                float yPos = -Mathf.Sign(gravityDirection.y) * (halfHeight - gameOverLineOffset);
                gameOverTransform.position = new Vector2(0f, yPos);
                gameOverCollider.size = new Vector2(halfWidth * 2f, 0.4f);
            }
        }

        private void UpdateComboDecay()
        {
            if (comboCount <= 1)
            {
                return;
            }

            if (Time.time - lastMergeTime > comboWindow)
            {
                comboCount = 0;
                UpdateComboText();
            }
        }

        private void Restart()
        {
            score = 0;
            gameOver = false;
            stateText.text = "Tap to Drop";
            UpdateScore();
            comboCount = 0;
            lastMergeTime = 0f;
            UpdateComboText();
            InitializeGravityPhases();

            Ball[] balls = FindObjectsOfType<Ball>();
            foreach (Ball ball in balls)
            {
                Destroy(ball.gameObject);
            }
        }
    }
}
