using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnController : MonoBehaviour {

[System.Serializable]public struct SpawnPoint {
    public Transform spawnpoint;
    private GameObject player;

	public GameObject Player { get => player; set => player = value; }
}

[SerializeField] List<SpawnPoint> spawnPoints = new List<SpawnPoint>();

public static SpawnController instance;

public List<SpawnPoint> SpawnPoints { get => spawnPoints; set => spawnPoints = value; }

void Start() {
    instance = this;
}

void Update() {
        
}

}
