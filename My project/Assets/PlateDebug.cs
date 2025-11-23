using UnityEngine;
using System.Collections;

public class PlateDebug : MonoBehaviour
{
    public float moveInterval = 2f;    // Интервал между движениями
    public float moveDuration = 1f;    // Длительность движения
    public float moveDistance = 0.2f;  // Расстояние движения

    private Vector3 startPos;
    private Vector3 targetPos;
    private bool isMoving = false;
    private float timer = 0f;

    void Start()
    {
        startPos = transform.position;
        targetPos = startPos + Vector3.down * moveDistance;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= moveInterval && !isMoving)
        {
            StartCoroutine(AutoMove());
            timer = 0f;
        }
    }

    IEnumerator AutoMove()
    {
        isMoving = true;

        // Движение вниз
        Vector3 currentStart = transform.position;
        Vector3 currentTarget = new Vector3(startPos.x, startPos.y - moveDistance, startPos.z);

        for (float t = 0; t < moveDuration; t += Time.deltaTime)
        {
            float progress = t / moveDuration;
            transform.position = Vector3.Lerp(currentStart, currentTarget, progress);
            yield return null;
        }

        transform.position = currentTarget;

        yield return new WaitForSeconds(0.5f);

        // Движение вверх
        for (float t = 0; t < moveDuration; t += Time.deltaTime)
        {
            float progress = t / moveDuration;
            transform.position = Vector3.Lerp(currentTarget, startPos, progress);
            yield return null;
        }

        transform.position = startPos;
        isMoving = false;
    }
}