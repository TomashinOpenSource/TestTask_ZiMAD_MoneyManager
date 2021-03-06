﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoneyManager : MonoBehaviour
{
    #region SaveLoadManager
    [SerializeField] private SaveLoadManager saveLoadManager;
    public SaveLoadManager SaveLoadManager
    {
        get => (saveLoadManager == null) ? GetComponent<SaveLoadManager>() : saveLoadManager;
        set => saveLoadManager = value;
    }
    #endregion

    public SaveLoadType SaveLoadType;
    public Money[] money;

    void Start()
    {
        SaveLoadManager.SaveLoadType = SaveLoadType;

        foreach (var item in money)
        {
            item.Value = SaveLoadManager.UpdateValue(UpdateType.Load ,item);
        }
    }
}

[Serializable]
public class Money
{
    public MoneyType moneyType;
    public int value;
    public Text valueField; // Лучше всего использовать TMP_Text, но чтобы не подключать еще один плагин, использую дефолтный от Unity

    public int Value
    {
        get
        {
            //value = SaveLoadManager.LoadValue(this);
            return value;
        }
        set
        {
            this.value = value;
            valueField.text = moneyType.ToString() + ": " + value.ToString();
            SaveLoadManager.UpdateValue(UpdateType.Save, this, value);
        }
        // Загрузка и Сохранение стоит внутри внутри свойства значения валюты для упрощения работы.
        // Данный способ надежнее по защите - изменение происходит только после загрузки актуального значения
        // Но добавляется нагрузка из-за частой работы с системой
        // В случае с БД будет происходить сначала запись нулевого значения, а потом актуального - пока не нашел решения как исправить
    }
}

public enum MoneyType
{
    Coins, Crystals
}