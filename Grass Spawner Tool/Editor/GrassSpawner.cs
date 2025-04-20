using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace GrassSpawnerTool
{
    public class GrassSpawnerEditor : EditorWindow
    {
        public GameObject plane; // The plane where grass will be spawned
        public List<GameObject> grassPrefabs; // List of grass prefabs to spawn
        public int grassDensity = 100; // Number of grass objects to spawn
        public GameObject parentObject; // Parent object to organize the spawned grass
        public float minHeight = 0.5f; // Minimum height of grass
        public float maxHeight = 1.5f; // Maximum height of grass
        public float checkRadius = 1.0f; // Radius to check for nearby objects
        public float minDistanceBetweenGrass = 1.0f; // Minimum distance between each grass object
        private List<Vector3> spawnedPositions = new List<Vector3>(); // List to keep track of spawned positions

        [MenuItem("Tools/Grass Spawner")]
        public static void ShowWindow()
        {
            GetWindow<GrassSpawnerEditor>("Grass Spawner");
        }

        void OnGUI()
        {
            GUILayout.Label("Grass Spawner Settings", EditorStyles.boldLabel);

            plane = (GameObject)EditorGUILayout.ObjectField("Plane", plane, typeof(GameObject), true);
            parentObject = (GameObject)EditorGUILayout.ObjectField("Parent Object", parentObject, typeof(GameObject), true);

            SerializedObject serializedObject = new SerializedObject(this);
            SerializedProperty serializedProperty = serializedObject.FindProperty("grassPrefabs");
            EditorGUILayout.PropertyField(serializedProperty, true);
            serializedObject.ApplyModifiedProperties();

            grassDensity = EditorGUILayout.IntField("Grass Density", grassDensity);
            minHeight = EditorGUILayout.FloatField("Min Height", minHeight);
            maxHeight = EditorGUILayout.FloatField("Max Height", maxHeight);
            checkRadius = EditorGUILayout.FloatField("Check Radius", checkRadius);
            minDistanceBetweenGrass = EditorGUILayout.FloatField("Min Distance Between Grass", minDistanceBetweenGrass);

            if (GUILayout.Button("Spawn Grass"))
            {
                SpawnGrass();
            }
        }

        void SpawnGrass()
        {
            if (plane == null || grassPrefabs.Count == 0 || parentObject == null)
            {
                Debug.LogError("Please assign all the necessary fields.");
                return;
            }

            MeshRenderer planeRenderer = plane.GetComponent<MeshRenderer>();
            if (planeRenderer == null)
            {
                Debug.LogError("The plane object does not have a MeshRenderer component.");
                return;
            }

            Bounds bounds = planeRenderer.bounds;
            spawnedPositions.Clear();

            for (int i = 0; i < grassDensity; i++)
            {
                Vector3 spawnPosition = Vector3.zero;
                int attempts = 0;
                bool validPosition = false;

                do
                {
                    float x = Random.Range(bounds.min.x, bounds.max.x);
                    float z = Random.Range(bounds.min.z, bounds.max.z);
                    Vector3 randomPoint = new Vector3(x, bounds.max.y + 10f, z);

                    Ray ray = new Ray(randomPoint, Vector3.down);
                    RaycastHit[] hits = Physics.RaycastAll(ray);

                    foreach (RaycastHit hit in hits)
                    {
                        if (hit.collider.gameObject == plane)
                        {
                            spawnPosition = hit.point;
                            validPosition = true;

                            foreach (Vector3 pos in spawnedPositions)
                            {
                                if (Vector3.Distance(spawnPosition, pos) < minDistanceBetweenGrass)
                                {
                                    validPosition = false;
                                    break;
                                }
                            }

                            if (validPosition)
                            {
                                break;
                            }
                        }
                    }

                    attempts++;
                } while (!validPosition && attempts < 100);

                if (!validPosition)
                {
                    Debug.LogWarning("Could not find a valid position for grass after 100 attempts.");
                    continue;
                }

                GameObject grassPrefab = grassPrefabs[Random.Range(0, grassPrefabs.Count)];
                GameObject grass = (GameObject)PrefabUtility.InstantiatePrefab(grassPrefab, parentObject.transform);

                grass.transform.position = spawnPosition;

                // Randomly adjust the height of the grass
                float scale = Random.Range(minHeight, maxHeight);
                grass.transform.localScale = new Vector3(scale, scale, scale);

                // Check for nearby objects
                Collider[] hitColliders = Physics.OverlapSphere(grass.transform.position, checkRadius);
                bool hasCollision = false;
                foreach (var hitCollider in hitColliders)
                {
                    if (hitCollider.gameObject != grass && hitCollider.gameObject != plane)
                    {
                        DestroyImmediate(grass);
                        hasCollision = true;
                        break;
                    }
                }

                if (!hasCollision)
                {
                    spawnedPositions.Add(spawnPosition);
                }
            }
        }
    }
}
