using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class AxisData
{
    public float min_position;        // минимальное положение оси
    public float max_position;        // максимальное положение оси
    public float current_position;    // текущее положение оси
    public string units;              // единицы измерения (mm/degrees)
}

// оси CNC машины
[Serializable]
public class AxesData
{
    public AxisData x;    
    public AxisData y;    
    public AxisData z;    
    public AxisData a;   
    public AxisData c;    
}

// данные шпинделя CNC машины
[Serializable]
public class SpindleData
{
    public float position;    // положение шпинделя
    public int speed;         // скорость вращения шпинделя
    public string units;      // единицы измерения скорости (rpm)
}

// данные смены инструмента
[Serializable]
public class ToolChangerData
{
    public int current_tool;      // текущий номер инструмента
    public string position;       // положение сменщика инструмента
}

// координаты заготовки
[Serializable]
public class WorkpieceZeroData
{
    public float x;   
    public float y;    
    public float z;   
}

// основные данные CNC машины
[Serializable]
public class CncMachineData
{
    public string machine_id;                 // уникальный идентификатор машины
    public string model;                      // модель машины
    public AxesData axes;                     // данные осей
    public SpindleData spindle;               // данные шпинделя
    public ToolChangerData tool_changer;      // данные смены инструмента
    public WorkpieceZeroData workpiece_zero;  // координаты заготовки
    public string timestamp;                  // временная метка данных
    public string status;                     // текущий статус машины
}

[Serializable]
public class RootData
{
    public CncMachineData cnc_machine;    // основной объект CNC машины
}


// запрос данных с заданным интервалом и вывод в консоль
public class CncDataReceiver : MonoBehaviour
{
    [Header("Настройки подключения")]
    [Tooltip("URL адрес для получения JSON данных")]
    [SerializeField] private string dataSourceUrl = "https://karasevv.com/test/mt_data.json";

    [Tooltip("Интервал обновления данных в секундах")]
    [SerializeField] private float dataUpdateInterval = 5f;


    private void Start()
    {
        StartCoroutine(ContinuousDataMonitoring());
    }

    private IEnumerator ContinuousDataMonitoring()
    {
        while (true)
        {
            yield return StartCoroutine(FetchAndProcessData());
            yield return new WaitForSeconds(dataUpdateInterval);
        }
    }

    // выполнение GET-запроса к серверу для получения json 
    private IEnumerator FetchAndProcessData()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(dataSourceUrl))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Ошибка сетевого запроса: {webRequest.error}");
            }
            else
            {
                string rawJsonData = webRequest.downloadHandler.text;
                ProcessAndDisplayData(rawJsonData);
            }
        }
    }


    // парсинг json и вывод в консоль
    private void ProcessAndDisplayData(string jsonData)
    {
        try
        {
            RootData parsedData = JsonUtility.FromJson<RootData>(jsonData);
            CncMachineData machineInfo = parsedData.cnc_machine;

            DisplayMachineInfo(machineInfo);

            DisplayAxesInfo(machineInfo.axes);

            DisplaySpindleAndToolInfo(machineInfo);

        }
        catch (Exception parsingError)
        {
            Debug.LogError($"Ошибка обработки JSON данных: {parsingError.Message}");
        }
    }

    // основная информация о машине
    private void DisplayMachineInfo(CncMachineData machine)
    {
        Debug.Log("ИНФОРМАЦИЯ О CNC МАШИНЕ");
        Debug.Log($"Идентификатор: {machine.machine_id}");
        Debug.Log($"Модель: {machine.model}");
        Debug.Log($"Статус: {machine.status}");
        Debug.Log($"Время данных: {machine.timestamp}");
    }

    // подробная инфомрация по осям
    private void DisplayAxesInfo(AxesData axes)
    {
        Debug.Log("ПОЗИЦИИ ОСЕЙ");
        DisplaySingleAxisInfo("X", axes.x);
        DisplaySingleAxisInfo("Y", axes.y);
        DisplaySingleAxisInfo("Z", axes.z);
        DisplaySingleAxisInfo("A", axes.a);
        DisplaySingleAxisInfo("C", axes.c);
    }

    // вывод информации по одной оси
    private void DisplaySingleAxisInfo(string axisName, AxisData axis)
    {
        Debug.Log($"{axisName}-ось: {axis.current_position} {axis.units} " +
                  $"(диапазон: {axis.min_position} - {axis.max_position} {axis.units})");
    }

    // вывод информации о шпинделе, инструменте, сменщике
    private void DisplaySpindleAndToolInfo(CncMachineData machine)
    {
        Debug.Log("СИСТЕМНЫЕ ПАРАМЕТРЫ");
        Debug.Log($"Скорость шпинделя: {machine.spindle.speed} {machine.spindle.units}");
        Debug.Log($"Текущий инструмент: №{machine.tool_changer.current_tool}");
        Debug.Log($"Состояние сменщика: {machine.tool_changer.position}");
        Debug.Log($"Нуль заготовки: X={machine.workpiece_zero.x}, Y={machine.workpiece_zero.y}, Z={machine.workpiece_zero.z}");
    }
}