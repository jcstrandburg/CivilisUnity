﻿using System.Collections.Generic;
using System.Linq;
using Tofu.Serialization;
using UnityEngine;

namespace Neolithica.MonoBehaviours {
    [SavableMonobehaviour(9)]
    public class Herd : MonoBehaviour {
        public GameObject rabbit;
        public Vector3[] path;
        public int currentNode = 1;
        public float nodeProgress;
        public float migrateSpeed;
        public Vector3 diff;
        public GameObject animalPrefab;
        public int maxSize = 3;
        public float respawnDelay = 10.0f;
        public float respawnProgress = 0.0f;
        public Vector3 rabbitPos;//debugging variable
        public List<AnimalController> animals = new List<AnimalController>();

        [Inject]
        public GameController GameController { get; set; }

        // Handles Start event
        void Start() {
            RandomizePath();
            UpdateRabbitPosition();
            AnimalController[] aminals = transform.GetComponentsInChildren<AnimalController>();
            foreach (AnimalController a in aminals) {
                animals.Add(a);
            }

            while (animals.Count < maxSize) {
                SpawnNewAnimal();
            }
        }

        public bool KillAnimal() {
            AnimalController firstAdult = animals.FirstOrDefault(a => a.IsAdult);

            if (firstAdult == null)
                return false;

            Destroy(firstAdult);
            animals.Remove(firstAdult);
            return true;
        }

        public void RandomizePath() {
            //Debug.Log("Randomizing path");
            int pathSize = (int)Random.Range(8, 11);
            path = new Vector3[pathSize];
            for (int i = 0; i < pathSize; i++) {
                path[i] = transform.position + Quaternion.Euler(0, (float)i / pathSize * 360.0f + Random.Range(-10, 10), 0) * transform.forward * Random.Range(25.0f, 50.0f);
                path[i] = GameController.SnapToGround(path[i]);
            }
            currentNode = 1;
            nodeProgress = 0.0f;
            //Debug.Log("Done Randomizing path");
        }

        void FixedUpdate() {
            UpdateRabbitPosition();

            if (animals.Count < maxSize) {
                respawnProgress += Time.fixedDeltaTime;
                if (respawnProgress >= respawnDelay) {
                    respawnProgress = 0.0f;
                    SpawnNewAnimal();
                }
            }
        }

        public void SpawnNewAnimal() {
            GameObject child = GameController.Factory.Instantiate(animalPrefab);
            child.transform.position = GetRabbitLocation();
            child.transform.parent = transform;
            AnimalController newAnimal = child.GetComponent<AnimalController>();
            animals.Add(newAnimal);
        }

        public void UpdateRabbitPosition() {
            int prevNode = (currentNode + path.Length - 1) % path.Length;
            diff = path[currentNode] - path[prevNode];
            nodeProgress += migrateSpeed;

            if (nodeProgress > diff.magnitude) {
                currentNode = (currentNode + 1) % path.Length;
                nodeProgress = 0;
            }
            else {
                rabbit.transform.position = Vector3.Lerp(path[prevNode], path[currentNode], nodeProgress / diff.magnitude);
                rabbit.transform.position = GameController.SnapToGround(rabbit.transform.position);
                rabbitPos = rabbit.transform.position;
            }
        }

        void Update() {
            for (int i=0; i < path.Length; ++i) {
                Vector3 start = path[i] + new Vector3(0, 1.0f, 0);
                Vector3 end = path[(i+1)%path.Length] + new Vector3(0, 1.0f, 0);
                Debug.DrawLine(start, end, Color.red);
            }
        }

        public Vector3 GetRabbitLocation() {
            return rabbit.transform.position;
        }
    }
}
