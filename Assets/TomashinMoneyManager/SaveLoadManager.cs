﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using UnityEngine.Networking;

public class SaveLoadManager : MonoBehaviour
{
    private static SaveLoadManager localInstance;
    public static SaveLoadManager slm { get { return localInstance; } }

    public static SaveLoadType SaveLoadType;

    private static Save save = new Save();

    private static string filePath;
    private const string fileBinaryEnd = ".save";
    private const string fileJsonEnd = ".json";

    private const string WebSaveProgress = "https://<адрес>/WebSaveProgress.php";
    private const string WebLoadProgress = "https://<адрес>/WebLoadProgress.php";


    private void Start()
    {

    }

    public static void SaveValue(Money field, int value)
    {
        switch (SaveLoadType)
        {
            case SaveLoadType.None:
                break;
            case SaveLoadType.PlayerPrefs:
                PlayerPrefs.SetInt(field.moneyType.ToString(), value);
                break;
            case SaveLoadType.Text:
                filePath = Application.persistentDataPath + "/" + field.moneyType.ToString() + fileJsonEnd;
                save.value = value;
                File.WriteAllText(filePath, JsonUtility.ToJson(save));
                break;
            case SaveLoadType.Binary:
                filePath = Application.persistentDataPath + "/" + field.moneyType.ToString() + fileBinaryEnd;
                BinaryFormatter bf = new BinaryFormatter();
                try
                {
                    using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    {
                        save.value = value;
                        bf.Serialize(fs, save);
                    }
                }
                catch (Exception ioEx)
                {
                    Debug.Log(String.Format("Сохранение {0} завершилось: {1}", field.moneyType, ioEx.Message));
                }
                break;
            case SaveLoadType.Database:
                slm.StartCoroutine(UpdateProgress(field, value));
                break;
            default:
                break;
        }
        Debug.Log(string.Format("Сохранение {0} = {1}", field.moneyType, value));
    }

    public static int LoadValue(Money field)
    {
        int value = 0;
        switch (SaveLoadType)
        {
            case SaveLoadType.None:
                break;
            case SaveLoadType.PlayerPrefs:
                value = PlayerPrefs.GetInt(field.moneyType.ToString());
                break;
            case SaveLoadType.Text:
                filePath = Application.persistentDataPath + "/" + field.moneyType.ToString() + fileJsonEnd;
                if (File.Exists(filePath))
                {
                    save = JsonUtility.FromJson<Save>(File.ReadAllText(filePath));
                    value = save.value;
                }
                break;
            case SaveLoadType.Binary:
                filePath = Application.persistentDataPath + "/" + field.moneyType.ToString() + fileBinaryEnd;
                BinaryFormatter bf = new BinaryFormatter();
                try
                {
                    using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    {
                        save = (Save)bf.Deserialize(fs);
                        value = save.value;
                    }
                }
                catch (Exception ioEx)
                {
                    Debug.Log(String.Format("Загрузка {0} завершилась: {1}", "0", ioEx.Message));
                }
                break;
            case SaveLoadType.Database:
                slm.StartCoroutine(LoadProgress());
                break;
            default:
                
                break;
        }
        Debug.Log(string.Format("Получение {0} = {1}", field.moneyType, value));
        return value;
    }

    #region Работа с сервером
    private static IEnumerator LoadProgress()
    {
        WWWForm form = new WWWForm();
        //form.AddField("platform", Bridge.platform);
        //form.AddField("uid", UID);

        using (UnityWebRequest www = UnityWebRequest.Post(WebLoadProgress, form))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                //Debug.Log(www.error);
            }
            else
            {
                if (www.downloadHandler.text != "")
                {
                    SetProgress(www.downloadHandler.text);
                }
                else
                {
                    
                }
                //yield return new WaitForSeconds(SyncTime);
                slm.StartCoroutine(LoadProgress());
            }
        }
    }
    private static void SetProgress(string progress)
    {
        string[] array = progress.Split(new char[] { ';' });
        for (int i = 0; i < array.Length; i++)
        {
            //Debug.LogFormat("Key {0} updated to {1}", keys[i], Convert.ToInt32(array[i]));
            //PlayerPrefs.SetInt(keys[i], Convert.ToInt32(array[i]));
        }
        //PuzzleMatchManager.instance.livesSystem.CheckLives();
    }
    private static IEnumerator UpdateProgress(Money field, int value)
    {
        WWWForm form = new WWWForm();
        //form.AddField("platform", Bridge.platform);
        //form.AddField("uid", UID);
        form.AddField("field", field.moneyType.ToString());
        form.AddField("value", value);

        using (UnityWebRequest www = UnityWebRequest.Post(WebSaveProgress, form))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                //Debug.Log(www.error);
            }
            else
            {
                //Debug.Log(www.downloadHandler.text);
            }
        }
    }
    #endregion
}

public enum SaveLoadType
{
    None,
    PlayerPrefs,
    Text,
    Binary,
    Database
}

[Serializable]
public class Save
{
    public int value;
}
// Изначально планировал сделать сохранение всех валют в один файл, но это было не удобно тем,
// что либо прописывать в классе выше все виды вручную (неудобно),
// либо через массив/лист/словарь (но сложность в доступе, часто приходится объявлять = обнулять)
// как я это делал могу показать, возможно сможете подсказать как надо правильнее, но пока что только так(