using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using JayMountains;
using Sirenix.OdinInspector;

public class BoulderSpawner : MonoBehaviour
{
    // Use with MineZoneLootboxDropsManager.
    public int Mine;
    // Use with MineZoneLootboxDropsManager.
    public int Zone;

    [InfoBox("The game object the spawned boulders will be child of.\nPlease make sure the transform offset accounts for any transform offsets the tilemap might have (check it's parent game objects too).")]
    [SerializeField] private GameObject bouldersParent;

    [Space(2)]
    [Header("Boulder Types")]
    [SerializeField] private GameObject[] boulderPrefabs;

    [Space(2)]
    [Header("Spawn System")]
    [SerializeField] private Tilemap tileMap;
    [SerializeField] private bool fullFill;
    [SerializeField, HideIf("@fullFill")] private int spawnCount;

    [ReadOnly, ShowInInspector] private List<Vector3> availablePlaces;


    //----------------------------------------------------------------
    private void Start()
    {
        FindLocationsOfTiles();
        SpawnBoulder();
    }


    //----------------------------------------------------------------
    private void FindLocationsOfTiles()
    {
        availablePlaces = new List<Vector3>();

        foreach (var position in tileMap.cellBounds.allPositionsWithin)
        {
            if (!tileMap.HasTile(position))
            {
                continue;
            }

            // Tile is not empty; do stuff.
            availablePlaces.Add(position);
        }
    }


    //----------------------------------------------------------------
    private void SpawnBoulder()
    {
        if (fullFill)
        {
            for (int i = 0; i < availablePlaces.Count; i++)
            {
                // Spawn prefab at the vector's position which is at the availablePlaces location and add 0.5f units.
                // As the bottom left of the CELL (square) is (0,0), the top right of the CELL (square) is (1,1), therefore, the middle is (0.5,0.5).
                var boulder = Instantiate(boulderPrefabs[Random.Range(0, boulderPrefabs.Length)], new Vector3(availablePlaces[i].x + 0.5f, availablePlaces[i].y + 0.5f, availablePlaces[i].z), Quaternion.identity);
                boulder.transform.SetParent(bouldersParent.transform, false);
                boulder.GetComponent<MineZoneLootboxDropsManager>().MineIndex = Mine - 1;
                boulder.GetComponent<MineZoneLootboxDropsManager>().ZoneIndex = Zone - 1;
                boulder.GetComponent<MineZoneLootboxDropsManager>().Init();
            }
        }
        else
        {
            for (int i = 0; i < spawnCount; i++)
            {
                bool random = true;
                var placeToSpawn = random ? availablePlaces[Random.Range(0, availablePlaces.Count)] : availablePlaces[i];
                // Spawn prefab at the vector's position which is at the availablePlaces location and add 0.5f units.
                // As the bottom left of the CELL (square) is (0,0), the top right of the CELL (square) is (1,1), therefore, the middle is (0.5,0.5).
                var boulder = Instantiate(boulderPrefabs[Random.Range(0, boulderPrefabs.Length)], new Vector3(placeToSpawn.x + 0.5f, placeToSpawn.y + 0.5f, placeToSpawn.z), Quaternion.identity);
                boulder.transform.SetParent(bouldersParent.transform, false);
                boulder.GetComponent<MineZoneLootboxDropsManager>().MineIndex = Mine - 1;
                boulder.GetComponent<MineZoneLootboxDropsManager>().ZoneIndex = Zone - 1;
                boulder.GetComponent<MineZoneLootboxDropsManager>().Init();
            }
        }
    }
}