using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;

public class DynamicPlateController : MonoBehaviour
{
    public string configUrl = "https://lab3-2-5bea.onrender.com/";

    public float checkInterval = 0.1f;

    public bool showDebug = true;

    private Vector3 startPos;          // начальная позиция плиты
    private float currentMinY = 0f;    // текущее минимальное ограничение по высоте
    private bool isMoving = false;     // флаг, предотвращающий одновременные движения
    private float currentMaxY = 1.063f;// текущее максимальное ограничение по высоте

    [Serializable]
    public class PlateConfig
    {
        public float moveDuration;    // сколько секунд длится одно движение
        public float moveDistance;    // на какое расстояние двигаться
        public bool shouldMoveDown;   // направление true = вниз, false = вверх
        public bool isEnabled;        // включено ли движение вообще
        public float minY;            // минимальная допустимая координата Y
        public float maxY;            // максимальная допустимая координата Y
    }

    void Start()
    {
        // начальная позиция плиты в мировых координатах
        startPos = transform.position;

        if (showDebug) Debug.Log($"Start position: {startPos.y:F3}");

        StartCoroutine(CheckConfigRepeatedly());
    }

    IEnumerator CheckConfigRepeatedly()
    {

        while (true)
        {
            yield return new WaitForSeconds(checkInterval);

            // если плита сейчас не движется
            if (!isMoving)
            {
                // загрузить конфигурацию с сервера и выполнять движения
                yield return StartCoroutine(LoadConfigAndMove());
            }
        }
    }

    IEnumerator LoadConfigAndMove()
    {
        // флаг движения
        isMoving = true;

        using (UnityWebRequest request = UnityWebRequest.Get(configUrl))
        {
            yield return request.SendWebRequest();

            PlateConfig config = ProcessServerResponse(request);

            if (config != null && config.isEnabled)
            {
                yield return StartCoroutine(ExecuteMovement(config));
            }
        }

        isMoving = false;
    }

    // парсинг json
    private PlateConfig ProcessServerResponse(UnityWebRequest request)
    {
        // Проверяем что запрос выполнен успешно
        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                // Преобразуем JSON-текст в объект C#
                PlateConfig config = JsonUtility.FromJson<PlateConfig>(request.downloadHandler.text);

                if (config != null)
                {
                    // Обновляем текущие ограничения движения из конфигурации
                    currentMinY = config.minY;
                    currentMaxY = config.maxY;

                    // Показываем информацию о движении
                    if (showDebug)
                    {
                        Debug.Log($"Move: {(config.shouldMoveDown ? "DOWN" : "UP")} " +
                                  $"{config.moveDistance:F2}m in {config.moveDuration:F1}s");
                    }

                    return config;
                }
            }
            catch (System.Exception e)
            {
                // обработка ошибки парсинга JSON
                Debug.LogError($"JSON Error: {e.Message}");
            }
        }
        else
        {
            // обработка ошибки сети
            if (showDebug) Debug.LogError($"Network Error: {request.error}");
        }

        return null;
    }

    // движение плиты на основе полученной конфигурации
    IEnumerator ExecuteMovement(PlateConfig config)
    {
        // направление движения (вниз или вверх)
        Vector3 direction = config.shouldMoveDown ? Vector3.down : Vector3.up;

        // позиция относительно начальной точки
        Vector3 targetPos = startPos + direction * config.moveDistance;

        // ограничения
        if (targetPos.y < currentMinY)
        {
            if (showDebug) Debug.Log($"Hit minY: {currentMinY:F3}");
            targetPos.y = currentMinY; // Ограничиваем снизу
        }
        else if (targetPos.y > currentMaxY)
        {
            if (showDebug) Debug.Log($"Hit maxY: {currentMaxY:F3}");
            targetPos.y = currentMaxY; // Ограничиваем сверху
        }

        // текущая позиция для плавного движения
        Vector3 startPosition = transform.position;
        float timer = 0f;

        while (timer < config.moveDuration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPos, timer / config.moveDuration);

            timer += Time.deltaTime;

            yield return null;
        }

        transform.position = targetPos;

        if (showDebug) Debug.Log($"Done: {transform.position.y:F3}");
    }


}