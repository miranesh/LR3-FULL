using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PressSystem : MonoBehaviour
{
    [Header("REFERENCES")]
    public Transform startPoint;
    public Transform pressPoint;
    public Transform endPoint;
    public GameObject destroyZone;
    public GameObject canisterPrefab;

    [Header("CONVEYOR SETTINGS")]
    public float conveyorSpeed = 1f;
    public float spawnInterval = 3f;

    private List<GameObject> canisters = new List<GameObject>();
    private bool isPressing = false;
    private float lastSpawnTime = 0f;

    void Start()
    {
        Debug.Log("SYSTEM STARTED");
        lastSpawnTime = Time.time;

        if (startPoint && pressPoint && endPoint && destroyZone && canisterPrefab)
        {
            StartCoroutine(SpawnRoutine());
        }
    }

    void Update()
    {
        // ТОЧНЫЙ ИНТЕРВАЛ СПАВНА
        if (Time.time - lastSpawnTime >= spawnInterval)
        {
            SpawnCanister();
            lastSpawnTime = Time.time;
        }

        if (!isPressing)
            MoveCanisters();
    }

    // Старая корутина - можно удалить
    IEnumerator SpawnRoutine()
    {
        yield break; // Отключаем старый метод
    }

    void SpawnCanister()
    {
        if (canisterPrefab == null || startPoint == null) return;

        GameObject canister = Instantiate(canisterPrefab, startPoint.position, canisterPrefab.transform.rotation);
        canisters.Add(canister);

        Rigidbody rb = canister.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        Debug.Log("📦 Canister spawned at time: " + Time.time);
    }

    void MoveCanisters()
    {
        for (int i = canisters.Count - 1; i >= 0; i--)
        {
            GameObject canister = canisters[i];
            if (canister == null)
            {
                canisters.RemoveAt(i);
                continue;
            }

            canister.transform.Translate(Vector3.left * conveyorSpeed * Time.deltaTime, Space.World);

            if (Vector3.Distance(canister.transform.position, pressPoint.position) < 0.5f)
            {
                Debug.Log("Canister under press");
                StartCoroutine(PressCanister(canister));
                canisters.RemoveAt(i);
                break;
            }

            if (canister.transform.position.x < endPoint.position.x)
            {
                Destroy(canister);
                canisters.RemoveAt(i);
            }
        }
    }

    IEnumerator PressCanister(GameObject canister)
    {
        isPressing = true;
        Debug.Log("START PRESS");

        // Фиксируем канистру
        canister.transform.position = pressPoint.position;
        canister.transform.rotation = Quaternion.Euler(0, 0, 0);

        // Отключаем коллайдеры канистры
        Collider[] canisterColliders = canister.GetComponentsInChildren<Collider>();
        foreach (Collider collider in canisterColliders) collider.enabled = false;

        Rigidbody canisterRb = canister.GetComponent<Rigidbody>();
        if (canisterRb != null) canisterRb.isKinematic = true;

        // Ждем некоторое время (имитация прессования)
        yield return new WaitForSeconds(2f);

        // Восстанавливаем канистру
        if (canister != null)
        {
            foreach (Collider collider in canisterColliders) collider.enabled = true;

            if (destroyZone != null)
            {
                canister.transform.position = destroyZone.transform.position + Vector3.up * 0.5f;
            }

            if (canisterRb != null)
            {
                canisterRb.isKinematic = false;
                canisterRb.useGravity = true;
            }

            Destroy(canister, 2f);
        }

        isPressing = false;
        Debug.Log("PRESS COMPLETE");
    }
}