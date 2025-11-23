using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ConveyerSwitch : MonoBehaviour
{
    public Transform startPoint;
    public Transform pressPoint;
    public Transform endPoint;
    public GameObject destroyZone;
    public GameObject canisterPrefab;

    public float conveyorSpeed = 2f;
    public float spawnInterval = 3f;

    private List<GameObject> canisters = new List<GameObject>();
    private bool isPressing = false;
    private bool isConveyorActive = false;
    private float lastSpawnTime = 0f;

    void Start()
    {
        if (GetComponent<Renderer>() != null)
            GetComponent<Renderer>().material.color = Color.red;
    }

    void OnMouseDown()
    {
        isConveyorActive = !isConveyorActive;

        if (isConveyorActive)
        {
            GetComponent<Renderer>().material.color = Color.green;
            lastSpawnTime = Time.time - spawnInterval;
        }
        else
        {
            GetComponent<Renderer>().material.color = Color.red;
        }
    }

    void Update()
    {
        bool canSpawn = isConveyorActive &&
                       Time.time - lastSpawnTime >= spawnInterval &&
                       canisterPrefab != null &&
                       startPoint != null;

        if (canSpawn)
        {
            SpawnCanister();
            lastSpawnTime = Time.time;
        }

        if (!isPressing && canisters.Count > 0)
            MoveCanisters();
    }

    void SpawnCanister()
    {
        GameObject canister = Instantiate(canisterPrefab, startPoint.position, canisterPrefab.transform.rotation);
        canisters.Add(canister);

        Rigidbody rb = canister.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }
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

            if (pressPoint != null)
            {
                float distanceToPress = Vector3.Distance(canister.transform.position, pressPoint.position);
                if (distanceToPress < 1f)
                {
                    StartCoroutine(PressCanister(canister));
                    canisters.RemoveAt(i);
                    continue;
                }
            }

            float distanceToEnd = Vector3.Distance(canister.transform.position, endPoint.position);
            if (distanceToEnd < 0.5f)
            {
                Destroy(canister);
                canisters.RemoveAt(i);
            }
        }
    }

    IEnumerator PressCanister(GameObject canister)
    {
        isPressing = true;

        canister.transform.position = pressPoint.position;
        canister.transform.rotation = pressPoint.rotation;

        Rigidbody rb = canister.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        yield return new WaitForSeconds(2f);

        if (canister != null)
        {
            if (destroyZone != null)
            {
                canister.transform.position = destroyZone.transform.position + Vector3.up * 0.5f;
                canister.transform.rotation = pressPoint.rotation;
            }

            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }

            Destroy(canister, 2f);
        }

        isPressing = false;
    }

    void OnMouseEnter()
    {
        if (GetComponent<Renderer>() == null) return;
        GetComponent<Renderer>().material.color = isConveyorActive ? new Color(0.2f, 0.8f, 0.2f) : Color.yellow;
    }

    void OnMouseExit()
    {
        if (GetComponent<Renderer>() == null) return;
        GetComponent<Renderer>().material.color = isConveyorActive ? Color.green : Color.red;
    }
}