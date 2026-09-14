using UnityEngine;
using System.Collections.Generic;

public class JeepneyGame : MonoBehaviour {
    // === Road, Stations, Middles ===
    public Vector2[] ROAD = new Vector2[] {
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
    }

    public Player player;

    // === Passengers ===
    public class Passenger {
        public int id;
        public Vector2 pos;
        public bool waiting;
        public bool pickedUp;
    }

    public List<Passenger> passengers = new List<Passenger>();
    public int score = 0;

    void Start() {
        player = new Player {
            id = 0,
            spawn = new Vector2(4,8),
            pos = new Vector2(4,8),
            roadIndex = System.Array.FindIndex(ROAD, t => t == new Vector2(4,8)),
            color = Color.blue,
            choicePending = false,
            remainingSteps = 0
        };

        SpawnPassengers();
    }

    void SpawnPassengers() {
        passengers.Clear();
        for (int i = 0; i < STATIONS.Length; i++) {
            passengers.Add(new Passenger {
                id = i,
                pos = STATIONS[i],
                waiting = true,
                pickedUp = false
            });
        }
    }

    // === Dice ===
    int RollDice() {
        return Random.Range(1, 7); // Unity upper bound is exclusive
    }

    // === Movement ===
    void AnimateMove(Player p, int steps) {
        for (int i = 0; i < steps; i++) {
            p.roadIndex = (p.roadIndex + 1) % ROAD.Length;
            p.pos = ROAD[p.roadIndex];

            // Check intersection
            foreach (var inter in INTERSECTIONS) {
                if (inter.pos == p.pos) {
                    p.choicePending = true;
                    p.remainingSteps = steps - i - 1;
                    HighlightOptions(inter.options);
                    return;
                }
            }
        }
        CheckPassengerPickup();
    }

    void HighlightOptions(Vector2[] options) {
        foreach (var opt in options) {
            Debug.Log("Highlight option at " + opt);
            // In Unity, you’d visually mark these tiles (e.g. change color)
        }
    }

    void CheckPassengerPickup() {
        foreach (var p in passengers) {
            if (p.waiting && !p.pickedUp && Vector2.Distance(player.pos, p.pos) == 1f) {
                p.pickedUp = true;
                p.waiting = false;
                score++;
                Debug.Log($"Picked up passenger {p.id}! Score: {score}");
            }
        }
    }
}
