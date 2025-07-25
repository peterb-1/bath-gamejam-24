using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Core.Saving;
using Cysharp.Threading.Tasks;
using Gameplay.Colour;
using Gameplay.Core;
using Gameplay.Player;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;
using Random = UnityEngine.Random;

namespace Gameplay.Environment
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class Building : MonoBehaviour
    {
        private const float DELAY = 0.03f;

        [SerializeField] 
        private bool isGameplay = true;
        
        [SerializeField] 
        private ColourId colourId;
        
        [SerializeField] 
        private Vector2Int tileDimensions;

        [SerializeField] 
        private Vector2 randomSizeRange;
        
        [SerializeField] 
        private Vector2 backgroundOffset;

        [SerializeField] 
        private Vector2 hologramTilingRange;

        [SerializeField] 
        private Vector2 hologramSpeedRange;
        
        [SerializeField] 
        private Vector2 hologramStrengthRange;

        [SerializeField] 
        private float flashDelayPerTile;
        
        [SerializeField, Range(0f, 1f)] 
        private float largeTileChance;

        [SerializeField, ShowIf(nameof(isGameplay))] 
        private float deathColliderBoundary;

        [SerializeField, ShowIf(nameof(isGameplay))] 
        private int mainColliderDelayFrames;
        
        [SerializeField] 
        private int maxTileSize;

        [SerializeField] 
        private int sortingOrder;

        [SerializeField, ShowIf(nameof(isGameplay))] 
        private BoxCollider2D mainCollider;
        
        [SerializeField, ShowIf(nameof(isGameplay))] 
        private BoxCollider2D deathCollider;
        
        [SerializeField, ShowIf(nameof(isGameplay))] 
        private BoxCollider2D playerDetectionCollider;
        
        [SerializeField, ShowIf(nameof(isGameplay))] 
        private Vector2 minDeathColliderSize;
        
        [SerializeField, ShowIf(nameof(isGameplay))] 
        private Vector2 extraPlayerDetectionColliderSize;
        
        [SerializeField] 
        private SpriteRenderer backgroundSpriteRenderer;

        [SerializeField] 
        private Material backgroundTileMaterial;
        
        [SerializeField] 
        private Material lowQualityBackgroundMaterial;

        [SerializeField] 
        private ColourDatabase colourDatabase;
        
        [SerializeField] 
        private List<Tile> tiles;

        private PlayerMovementBehaviour playerMovementBehaviour;

        private bool containsPlayer;
        private bool isActive;
        private float maxFlashDelay;
        private float flashTimer;
        private float nextFlashTime;
        
        private static readonly int ScrollSpeed = Shader.PropertyToID("_ScrollSpeed");
        private static readonly int Tiling = Shader.PropertyToID("_Tiling");
        private static readonly int Strength = Shader.PropertyToID("_Strength");

        private async void Awake()
        {
            ColourManager.OnColourChangeStarted += HandleColourChangeStarted;
            ColourManager.OnColourChangeInstant += HandleColourChangeInstant;

            InitialiseHologramSettings();

            maxFlashDelay = flashDelayPerTile / tiles.Count;

            if (isGameplay)
            {
                await UniTask.WaitUntil(PlayerAccessService.IsReady);

                playerMovementBehaviour = PlayerAccessService.Instance.PlayerMovementBehaviour;
            
                playerMovementBehaviour.OnPlayerHooked += HandlePlayerHooked;
                playerMovementBehaviour.OnPlayerUnhooked += HandlePlayerUnhooked;

                playerDetectionCollider.size = mainCollider.size + extraPlayerDetectionColliderSize;
                playerDetectionCollider.offset = mainCollider.offset;
            }
            else
            {
                mainCollider.enabled = false;
                deathCollider.enabled = false;
                playerDetectionCollider.enabled = false;
                
                if (SaveManager.Instance.SaveData.PreferenceData.TryGetValue(SettingId.FogQuality, out FogQuality fogQuality) &&
                    fogQuality is FogQuality.Low)
                {
                    foreach (var tile in tiles)
                    {
                        tile.SetMaterial(lowQualityBackgroundMaterial);
                    }
                }
            }
        }
        
        private void InitialiseHologramSettings()
        {
            var hologramMaterial = backgroundSpriteRenderer.material;

            hologramMaterial.SetVector(Tiling, new Vector2(1f, Random.Range(hologramTilingRange.x, hologramTilingRange.y) * mainCollider.size.y));
            hologramMaterial.SetFloat(ScrollSpeed, Random.Range(hologramSpeedRange.x, hologramSpeedRange.y));
            hologramMaterial.SetFloat(Strength, Random.Range(hologramStrengthRange.x, hologramStrengthRange.y));
        }

        private void HandleColourChangeStarted(ColourId colour, float duration)
        {
            var shouldActivate = colourId == colour;

            if (shouldActivate != isActive)
            {
                isActive = shouldActivate;
                ToggleBuildingAsync(duration).Forget();
            }
        }
        
        private void HandleColourChangeInstant(ColourId colour)
        {
            isActive = colourId == colour;
            ToggleBuildingAsync(0f).Forget();
        }
        
        private void HandlePlayerHooked()
        {
            if (isGameplay) mainCollider.enabled = false;
        }

        private void HandlePlayerUnhooked()
        {
            if (isGameplay) mainCollider.enabled = isActive;
        }
        
        public void NotifyPlayerEntered()
        {
            containsPlayer = true;
        }
        
        public void NotifyPlayerExited()
        {
            containsPlayer = false;
        }

        private void Update()
        {
            if (isActive && !PauseManager.Instance.IsPaused)
            {
                flashTimer += Time.deltaTime;
                
                if (flashTimer >= nextFlashTime)
                {
                    var tile = tiles.RandomChoice();
                    var flashColour = isGameplay ? Color.white : Color.grey;
                    
                    tile.FlashAsync(flashColour).Forget();
                    
                    nextFlashTime = flashTimer + Random.Range(0f, maxFlashDelay);
                }
            }
        }

        private async UniTask ToggleBuildingAsync(float duration)
        {
            if (isGameplay)
            {
                ToggleCollidersAsync().Forget();
            }

            var totalTiles = tiles.Count;
            
            if (totalTiles == 0) return;

            var shuffledIndices = new int[totalTiles];
            
            for (var i = 0; i < totalTiles; i++)
            {
                shuffledIndices[i] = i;
            }

            // O(n) shuffle
            for (var i = totalTiles - 1; i > 0; i--)
            {
                var randomIndex = Random.Range(0, i + 1);
                (shuffledIndices[i], shuffledIndices[randomIndex]) = (shuffledIndices[randomIndex], shuffledIndices[i]);
            }

            var tilesPerCycle = duration == 0f 
                ? totalTiles
                : Mathf.Max(1, (int) Mathf.Ceil(totalTiles * DELAY / duration));

            try
            {
                for (var startIndex = 0; startIndex < totalTiles; startIndex += tilesPerCycle)
                {
                    var endIndex = Mathf.Min(startIndex + tilesPerCycle, totalTiles);
                    
                    for (var i = startIndex; i < endIndex; i++)
                    {
                        var tile = tiles[shuffledIndices[i]];
                        
                        if (!isActive)
                        {
                            tile.CancelFlash();
                        }

                        tile.Toggle(isActive);
                    }

                    if (endIndex < totalTiles)
                    {
                        await UniTask.WaitUntil(() => !PauseManager.Instance.IsPaused);
                        await UniTask.Delay(TimeSpan.FromSeconds(DELAY), ignoreTimeScale: true);
                    }
                }
            }
            catch (InvalidOperationException)
            {
                // do nothing - suppress the error log, it's ok if we cancel the enumeration to toggle the other way
            }
        }

        private async UniTask ToggleCollidersAsync()
        {
            // enable the death collider first
            // if it kills the player, they won't be pushed out next frame since their collider will be disabled
            
            deathCollider.enabled = isActive;

            await UniTask.DelayFrame(mainColliderDelayFrames);
            
            mainCollider.enabled = isActive && !playerMovementBehaviour.IsHooked;

            if (isActive && containsPlayer)
            {
                playerMovementBehaviour.NotifyEjectedFromBuilding(mainCollider.bounds);
            }
        }
        
        private void OnDestroy()
        {
            ColourManager.OnColourChangeStarted -= HandleColourChangeStarted;
            ColourManager.OnColourChangeInstant -= HandleColourChangeInstant;

            if (isGameplay)
            {
                playerMovementBehaviour.OnPlayerHooked -= HandlePlayerHooked;
                playerMovementBehaviour.OnPlayerUnhooked -= HandlePlayerUnhooked;
            }
        }

#if UNITY_EDITOR
        [Button("Setup Building")]
        private void SetupBuilding()
        {
            if (tileDimensions.x < 1 || tileDimensions.y < 1)
            {
                GameLogger.LogError("Cannot fill tiles since one or more dimensions is not a positive number!", this);
                return;
            }

            var activeScene = SceneManager.GetActiveScene();
            var sceneLoader = GameObject.Find("SceneLoader").GetComponent<SceneLoader>();
            var sceneConfig = sceneLoader.SceneConfigs.First(scene => scene.ScenePath == activeScene.path);
            var districtNumber = sceneConfig.LevelConfig.DistrictNumber;
            
            if (!colourDatabase.TryGetColourConfig(colourId, out var colourConfig, district: districtNumber))
            {
                GameLogger.LogError($"Cannot fill tiles since the colour config for {colourId} could not be found in the colour database!", colourDatabase);
                return;
            }
            
            var occupiedTiles = new HashSet<(int, int)>();

            var buildingSize = mainCollider.size;
            var tileSize = buildingSize / tileDimensions;
            var buildingMin = mainCollider.bounds.min.xy() + tileSize / 2f;
            var sortingLayer = isGameplay ? "Default" : "BackgroundBuildings";

            var tilePrefabGuid = AssetDatabase.FindAssets("Tile t:GameObject")[0];
            var tilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(tilePrefabGuid));

            for (var i = 0; i < tileDimensions.y; i++)
            {
                for (var j = 0; j < tileDimensions.x; j++)
                {
                    // only checking for the current coordinates, not others within a potential bigger tile
                    // so we might have some overlap, but it looks cool
                    if (occupiedTiles.Contains((j, i))) continue;
                    
                    var tileGameObject = (GameObject) PrefabUtility.InstantiatePrefab(tilePrefab);
                    var tile = tileGameObject.GetComponent<Tile>();
                    var tileMultiplier = Random.Range(0f, 1f) < largeTileChance ? Random.Range(2, maxTileSize + 1) : 1;

                    // reduce tile size if we picked one that's too big
                    while (!IsTileSizeValid())
                    {
                        tileMultiplier--;
                    }

                    var indexOffset = (tileMultiplier - 1) / 2f;
                    var offset = new Vector2((j + indexOffset) * tileSize.x, (i + indexOffset) * tileSize.y);
                    var colour = isGameplay ? colourConfig.GetRandomColour() : colourConfig.GetRandomDesaturatedColour();
                    
                    tileGameObject.transform.parent = transform;
                    tile.SetSize(tileSize * tileMultiplier, randomSizeRange);
                    tile.SetPosition(buildingMin + offset);
                    tile.SetColour(colour);
                    tile.SetOrder(sortingLayer, sortingOrder);
                    tile.SetSprite(districtNumber);

                    if (!isGameplay)
                    {
                        tile.SetMaterial(backgroundTileMaterial);
                    }

                    tiles.Add(tile);
                    
                    for (var y = i; y < i + tileMultiplier; y++)
                    {
                        for (var x = j; x < j + tileMultiplier; x++)
                        {
                            occupiedTiles.Add((x, y));
                        }
                    }

                    bool IsTileSizeValid()
                    {
                        if (i + tileMultiplier > tileDimensions.y || j + tileMultiplier > tileDimensions.x) return false;

                        for (var i2 = i; i2 < i + tileMultiplier; i2++)
                        {
                            for (var j2 = j; j2 < j + tileMultiplier; j2++)
                            {
                                if (occupiedTiles.Contains((j2, i2))) return false;
                            }
                        }

                        return true;
                    }
                }
            }

            deathCollider.offset = mainCollider.offset;
            deathCollider.size = buildingSize - 2f * deathColliderBoundary * Vector2.one;

            if (deathCollider.size.x <= minDeathColliderSize.x) deathCollider.size = new Vector2(minDeathColliderSize.x, deathCollider.size.y);
            if (deathCollider.size.y <= minDeathColliderSize.y) deathCollider.size = new Vector2(deathCollider.size.x, minDeathColliderSize.y);

            backgroundSpriteRenderer.size = buildingSize;
            backgroundSpriteRenderer.transform.position = mainCollider.bounds.center.xy() + backgroundOffset;
            backgroundSpriteRenderer.color = isGameplay ? colourConfig.Background : colourConfig.BackgroundDesaturated;
            backgroundSpriteRenderer.sortingOrder = sortingOrder - 1;
            backgroundSpriteRenderer.sortingLayerName = sortingLayer;
        }

        [Button("Clear Tiles")]
        private void ClearTiles()
        {
            for (var i = tiles.Count - 1; i >= 0; i--)
            {
                var tile = tiles[i];
                tiles.Remove(tile);
                DestroyImmediate(tile.gameObject);
            }
        }

        [Button("Update Sort Order")]
        private void UpdateSortOrder()
        {
            var sortingLayer = isGameplay ? "Default" : "BackgroundBuildings";
            
            foreach (var tile in tiles)
            {
                tile.SetOrder(sortingLayer, sortingOrder);
            }
        }
#endif
    }
}
