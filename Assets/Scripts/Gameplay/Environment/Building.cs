using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Gameplay.Colour;
using Gameplay.Core;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

namespace Gameplay.Environment
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class Building : MonoBehaviour
    {
        private const float DELAY = 0.03f;
        
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
        private float flashChancePerTile;
        
        [SerializeField, Range(0f, 1f)] 
        private float largeTileChance;

        [SerializeField] 
        private float deathColliderBoundary;

        [SerializeField] 
        private int mainColliderDelayFrames;
        
        [SerializeField] 
        private int maxTileSize;

        [SerializeField] 
        private int sortingOrder;

        [SerializeField] 
        private BoxCollider2D mainCollider;
        
        [SerializeField] 
        private BoxCollider2D deathCollider;
        
        [SerializeField] 
        private SpriteRenderer backgroundSpriteRenderer;

        [SerializeField] 
        private ColourDatabase colourDatabase;
        
        [SerializeField, ReadOnly] 
        private List<Tile> tiles;

        private bool isActive;
        private float flashChance;
        
        private static readonly int ScrollSpeed = Shader.PropertyToID("_ScrollSpeed");
        private static readonly int Tiling = Shader.PropertyToID("_Tiling");
        private static readonly int Strength = Shader.PropertyToID("_Strength");

        private void Awake()
        {
            ColourManager.OnColourChangeStarted += HandleColourChangeStarted;
            ColourManager.OnColourChangeInstant += HandleColourChangeInstant;

            InitialiseHologramSettings();

            flashChance = flashChancePerTile * tiles.Count;
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

        private void Update()
        {
            if (isActive && !PauseManager.Instance.IsPaused && Random.Range(0f, 1f) < flashChance)
            {
                var tile = tiles.RandomChoice();
            
                tile.FlashAsync().Forget();
            }
        }

        private async UniTask ToggleBuildingAsync(float duration)
        {
            ToggleCollidersAsync().Forget();
            
            var shuffledTiles = new List<Tile>();

            while (tiles.Count > 0)
            {
                var randomTile = tiles.RandomChoice();
                shuffledTiles.Add(randomTile);
                tiles.Remove(randomTile);
            }

            tiles = shuffledTiles;

            var totalTiles = tiles.Count;
            var currentTile = 1;
            var tilesPerCycle = duration == 0f 
                ? totalTiles + 1 
                : Mathf.Max(1, (int) (totalTiles * DELAY / duration));

            try
            {
                foreach (var tile in tiles)
                {
                    if (!isActive)
                    {
                        tile.CancelFlash();
                    }

                    tile.Toggle(isActive);

                    if (currentTile % tilesPerCycle == 0)
                    {
                        await UniTask.WaitUntil(() => !PauseManager.Instance.IsPaused);
                        await UniTask.Delay(TimeSpan.FromSeconds(DELAY), ignoreTimeScale: true);
                    }

                    currentTile++;
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
            
            mainCollider.enabled = isActive;
        }
        
        private void OnDestroy()
        {
            ColourManager.OnColourChangeStarted -= HandleColourChangeStarted;
            ColourManager.OnColourChangeInstant -= HandleColourChangeInstant;
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

            if (!colourDatabase.TryGetColourConfig(colourId, out var colourConfig))
            {
                GameLogger.LogError($"Cannot fill tiles since the colour config for {colourId} could not be found in the colour database!", colourDatabase);
                return;
            }
            
            var occupiedTiles = new HashSet<(int, int)>();

            var buildingSize = mainCollider.size;
            var tileSize = buildingSize / tileDimensions;
            var buildingMin = mainCollider.bounds.min.xy() + tileSize / 2f;

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
                    while (i + tileMultiplier > tileDimensions.y || j + tileMultiplier > tileDimensions.x)
                    {
                        tileMultiplier--;
                    }

                    var indexOffset = (tileMultiplier - 1) / 2f;
                    var offset = new Vector2((j + indexOffset) * tileSize.x, (i + indexOffset) * tileSize.y);
                    
                    tileGameObject.transform.parent = transform;
                    tile.SetSize(tileSize * tileMultiplier, randomSizeRange);
                    tile.SetPosition(buildingMin + offset);
                    tile.SetColour(colourConfig.GetRandomColour());
                    tile.SetOrder(sortingOrder);

                    tiles.Add(tile);
                    
                    for (var y = i; y < i + tileMultiplier; y++)
                    {
                        for (var x = j; x < j + tileMultiplier; x++)
                        {
                            occupiedTiles.Add((x, y));
                        }
                    }
                }
            }

            deathCollider.offset = mainCollider.offset;
            deathCollider.size = buildingSize - 2f * deathColliderBoundary * Vector2.one;

            backgroundSpriteRenderer.size = buildingSize;
            backgroundSpriteRenderer.transform.position = mainCollider.bounds.center.xy() + backgroundOffset;
            backgroundSpriteRenderer.color = colourConfig.Background;
            backgroundSpriteRenderer.sortingOrder = sortingOrder - 1;
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
            foreach (var tile in tiles)
            {
                tile.SetOrder(sortingOrder);
            }
        }
#endif
    }
}
