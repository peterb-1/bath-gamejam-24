using System.Collections.Generic;
using Core.Colour;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gameplay.Buildings
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class Building : MonoBehaviour
    {
        [SerializeField] 
        private ColourId colourId;
        
        [SerializeField] 
        private Vector2Int tileDimensions;

        [SerializeField] 
        private Vector2 randomSizeRange;
        
        [SerializeField] 
        private Vector2 backgroundOffset;

        [SerializeField, Range(0f, 1f)] 
        private float largeTileChance;
        
        [SerializeField] 
        private int maxTileSize;
        
        [SerializeField, Range(0f, 1f)] 
        private float flashChance;

        [SerializeField] 
        private int sortingOrder;
        
        [SerializeField, ReadOnly] 
        private List<Tile> tiles;

        [SerializeField] 
        private BoxCollider2D boxCollider;
        
        [SerializeField] 
        private SpriteRenderer backgroundSpriteRenderer;

        [SerializeField] 
        private ColourDatabase colourDatabase;

        private void Update()
        {
            if (Random.Range(0f, 1f) < flashChance)
            {
                var tile = ArrayUtils.RandomChoice(tiles);
            
                tile.FlashAsync().Forget();
            }
        }

#if UNITY_EDITOR
        [Button("Fill Tiles")]
        private void FillTiles()
        {
            var occupiedTiles = new HashSet<(int, int)>();

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

            var buildingSize = boxCollider.size;
            var tileSize = buildingSize / tileDimensions;
            var buildingMin = boxCollider.bounds.min.xy() + tileSize / 2f;

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

                    var offset = new Vector2((j + tileMultiplier / 2f) * tileSize.x, (i + tileMultiplier / 2f) * tileSize.y);
                    
                    tileGameObject.transform.parent = transform;
                    tile.SetSize(tileSize * tileMultiplier, randomSizeRange);
                    tile.SetPosition(buildingMin + offset);
                    tile.SetColour(colourConfig.GetColour());
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

            backgroundSpriteRenderer.transform.localScale = buildingSize;
            backgroundSpriteRenderer.transform.position = boxCollider.bounds.center.xy() + backgroundOffset;
            backgroundSpriteRenderer.color = colourConfig.Background;
            backgroundSpriteRenderer.sortingOrder = sortingOrder;
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
