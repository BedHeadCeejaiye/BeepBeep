using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class JeepneyGame : MonoBehaviour {
    // === Road, Stations, Middles ===
    public Vector2[] ROAD = {
        new Vector2(3,6), new Vector2(4,6), new Vector2(5,6),
        new Vector2(6,6), new Vector2(6,5), new Vector2(6,4),
        new Vector2(6,3), new Vector2(7,3), new Vector2(8,3),
        new Vector2(8,4), new Vector2(8,5), new Vector2(8,6),
        new Vector2(9,6), new Vector2(10,6), new Vector2(11,6),
        new Vector2(11,7), new Vector2(11,8), new Vector2(10,8),
        new Vector2(9,8), new Vector2(8,8), new Vector2(8,9),
        new Vector2(8,10), new Vector2(8,11), new Vector2(7,11),
        new Vector2(6,11), new Vector2(6,10), new Vector2(6,9),
        new Vector2(6,8), new Vector2(5,8), new Vector2(4,8),
        new Vector2(3,8), new Vector2(3,7)
    };

    public Vector2[] STATIONS = {
        new Vector2(7,2), new Vector2(7,12),
        new Vector2(2,7), new Vector2(12,7)
    };

    public Vector2[] MIDDLES = {
        new Vector2(6,7), new Vector2(7,6),
        new Vector2(7,8), new Vector2(8,7)
    };

    // === Intersections ===
    [System.Serializable]
    public class Intersection {
        public Vector2 pos;
        public Vector2[] options;
    }

    public Intersection[] INTERSECTIONS = {
        new Intersection { pos=new Vector2(6,6), options=new Vector2[]{ new Vector2(6,5), new Vector2(7,6) } },
        new Intersection { pos=new Vector2(6,8), options=new Vector2[]{ new Vector2(5,8), new Vector2(6,7) } },
        new Intersection { pos=new Vector2(8,6), options=new Vector2[]{ new Vector2(9,6), new Vector2(8,7) } },
        new Intersection { pos=new Vector2(8,8), options=new Vector2[]{ new Vector2(8,9), new Vector2(7,8) } }
    };

    // === Player ===
    public class Player {
        public int id;
        public Vector2 spawn;
        public Vector2 pos;
        public int roadIndex;
        public Color color;
        public bool choicePending;
        public int remainingSteps;
        public GameObject jeepneyObj;
    }

    public Player player;

    // === Passengers ===
    public class Passenger {
        public int id;
        public Vector2 pos;
        public bool waiting;
        public bool pickedUp;
        public GameObject passengerObj;
    }

    public List<Passenger> passengers = new List<Passenger>();
    public int score = 0;

    public GameObject jeepneyPrefab;
    public GameObject passengerPrefab;
    public GameObject tilePrefab; // NEW: assign in Inspector

    void Start() {
        // Spawn board tiles first
        SpawnTiles();

        player = new Player {
            id = 0,
            spawn = new Vector2(4,8),
            pos = new Vector2(4,8),
            roadIndex = ROAD.ToList().FindIndex(t => t.Equals(new Vector2(4,8))),
            color = Color.blue,
            choicePending = false,
            remainingSteps = 0,
            jeepneyObj = Instantiate(jeepneyPrefab, new Vector3(4,8,0), Quaternion.identity)
        };

        SpawnPassengers();
    }

    void SpawnTiles() {
    // Road tiles (default color)
    foreach (var pos in ROAD) {
        Instantiate(tilePrefab, new Vector3(pos.x, pos.y, 0), Quaternion.identity);
    }

    // Station tiles (green)
    foreach (var pos in STATIONS) {
        var stationTile = Instantiate(tilePrefab, new Vector3(pos.x, pos.y, 0), Quaternion.identity);
        stationTile.GetComponent<SpriteRenderer>().color = Color.green;
    }

    // Middle tiles (yellow)
    foreach (var pos in MIDDLES) {
        var middleTile = Instantiate(tilePrefab, new Vector3(pos.x, pos.y, 0), Quaternion.identity);
        middleTile.GetComponent<SpriteRenderer>().color = Color.yellow;
    }

    // Intersection tiles (red)
    foreach (var inter in INTERSECTIONS) {
        var interTile = Instantiate(tilePrefab, new Vector3(inter.pos.x, inter.pos.y, 0), Quaternion.identity);
        interTile.GetComponent<SpriteRenderer>().color = Color.red;
    }
}


    void SpawnPassengers() {
        passengers.Clear();
        for (int i = 0; i < STATIONS.Length; i++) {
            var obj = Instantiate(passengerPrefab, new Vector3(STATIONS[i].x, STATIONS[i].y, 0), Quaternion.identity);
            passengers.Add(new Passenger {
                id = i,
                pos = STATIONS[i],
                waiting = true,
                pickedUp = false,
                passengerObj = obj
            });
        }
    }

    // === Dice ===
    int RollDice() {
        return Random.Range(1, 7); // Unity upper bound is exclusive
    }

    // === Movement ===
    public void StartMove() {
        int steps = RollDice();
        StartCoroutine(AnimateMove(player, steps));
    }

    IEnumerator AnimateMove(Player p, int steps) {
        for (int i = 0; i < steps; i++) {
            p.roadIndex = (p.roadIndex + 1) % ROAD.Length;
            p.pos = ROAD[p.roadIndex];
            p.jeepneyObj.transform.position = new Vector3(p.pos.x, p.pos.y, 0);

            // Check intersection
            foreach (var inter in INTERSECTIONS) {
                if (inter.pos == p.pos) {
                    p.choicePending = true;
                    p.remainingSteps = steps - i - 1;
                    HighlightOptions(inter.options);
                    yield break;
                }
            }
            yield return new WaitForSeconds(0.3f);
        }
        CheckPassengerPickup();
    }

    void HighlightOptions(Vector2[] options) {
        foreach (var opt in options) {
            Debug.Log("Highlight option at " + opt);
            // TODO: visually mark these tiles (e.g. change color or spawn marker prefab)
        }
    }

    void CheckPassengerPickup() {
        foreach (var p in passengers) {
            if (p.waiting && !p.pickedUp && Vector2.Distance(player.pos, p.pos) < 0.1f) {
                p.pickedUp = true;
                p.waiting = false;
                Destroy(p.passengerObj);
                score++;
                Debug.Log($"Picked up passenger {p.id}! Score: {score}");
            }
        }
    }
}
