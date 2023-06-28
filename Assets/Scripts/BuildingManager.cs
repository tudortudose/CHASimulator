using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class BuildingManager : MonoBehaviour
{
    public int currentIndex = -1;
    public Transform contentHolder;
    public Transform logicHolder;
    public Color onColor;
    public Color offColor;

    public void SelectBuildingObject(int index)
    {
        if (index == currentIndex)
        {
            currentIndex = -1;
            contentHolder.GetChild(index).GetComponent<Image>().color = offColor;
        }
        else
        {
            if (currentIndex != -1)
            {
                contentHolder.GetChild(currentIndex).GetComponent<Image>().color = offColor;
            }
            currentIndex = index;
            contentHolder.GetChild(index).GetComponent<Image>().color = onColor;
        }
    }

    void Update()
    {
        if (currentIndex != -1)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    BuildObject();
                }
            }
        }
    }

    private void BuildObject()
    {
        GameObject prefab;
        if (currentIndex >= 0 && currentIndex <= 12)
        {
            prefab = GameObject.FindObjectOfType<SavingManager>().GetPrefabByIndex(currentIndex);
        }
        else
        {
            prefab = GameObject.FindObjectOfType<SavingManager>()
                .GetPrefabByName(contentHolder.GetChild(currentIndex).GetChild(0).GetComponent<TMP_Text>().text);
        }
        var pos = CamManager.GetMouseToWorldPoint();
        var go = Instantiate(prefab, pos, Quaternion.identity, logicHolder);
    }
}
