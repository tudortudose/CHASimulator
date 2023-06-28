using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.ComponentModel;
using System.Linq;

public class LogicModule : LogicComponent
{
    public Transform inputs;
    public Transform outputs;

    public Transform components;

    public Transform componentsHolder;
    public Transform wiresHolder;

    public Transform module;
    public string moduleName;
    public TMP_Text componentsNameText;
    public TMP_Text moduleNameText;


    public List<Node> innerOutputs = new();
    public List<Node> innerInputs = new();

    public List<Pin> inputPins = new();
    public List<Pin> outputPins = new();

    public float verticalPadding;
    public float horizontalPadding;

    public float modulePadding;
    public float titlePadding;

    public void SaveModule()
    {
        GameObject.FindObjectOfType<SavingManager>().SaveModule(this);
    }


    public void CreateModuleSprite(float up, float down)
    {
        float height = up - down + modulePadding;

        GetComponent<SpriteRenderer>().size = new Vector2(GetComponent<SpriteRenderer>().size.x, height);
        GetComponent<BoxCollider2D>().size = new Vector2(GetComponent<SpriteRenderer>().size.x, height);
    }

    public void CreateComponentSprite(float right, float left, float up, float down)
    {
        float length = right - left + horizontalPadding;

        float height = up - down + verticalPadding;

        transform.GetChild(4).GetChild(2).GetChild(0).GetComponent<SpriteRenderer>().size = new Vector2(length, height);

        float posX = (right + left) / 2;
        float posY = (up + down) / 2;

        transform.GetChild(4).GetChild(2).GetChild(0).position = new Vector3(posX, posY, 5);
        transform.GetChild(4).GetChild(3).GetChild(0).transform.position =
            new Vector3(transform.GetChild(4).GetChild(2).GetChild(0).transform.position.x, up + titlePadding, 7);
    }

    public void Minimize()
    {
        module.GetComponent<SpriteRenderer>().enabled = true;
        module.GetComponent<BoxCollider2D>().enabled = true;

        components.localScale = new Vector3(0, 0, 0);

        for (int i=0; i < inputPins.Count; i++)
        {
            inputPins[i].transform.position = module.GetChild(0).GetChild(i).transform.position;
        }

        for (int i = 0; i < outputPins.Count; i++)
        {
            outputPins[i].transform.position = module.GetChild(1).GetChild(i).transform.position;
        }

        foreach (Node node in transform.GetComponent<LogicComponent>().inputNodes
            .Concat(transform.GetComponent<LogicComponent>().outputNodes)
            .Concat(innerOutputs)
            .Concat(innerInputs))
        {
            if (node.attachedWires.Count > 0)
            {
                foreach (Wire wire in node.attachedWires)
                {
                    if (wire)
                    {
                        wire.Redraw();

                    }
                }
            }
        }
        componentsNameText.gameObject.SetActive(false);
        moduleNameText.gameObject.SetActive(true);
    }

    public void Maximize()
    {
        module.GetComponent<SpriteRenderer>().enabled = false;
        module.GetComponent<BoxCollider2D>().enabled = false;
        moduleNameText.gameObject.SetActive(false);
        components.localScale = new Vector3(1, 1, 1);

        for (int i = 0; i < inputPins.Count; i++)
        {
            inputPins[i].transform.position = components.GetChild(0).GetChild(i).transform.position;
        }

        for (int i = 0; i < outputPins.Count; i++)
        {
            outputPins[i].transform.position = components.GetChild(1).GetChild(i).transform.position;
        }

        foreach (Node node in transform.GetComponent<LogicComponent>().inputNodes
            .Concat(transform.GetComponent<LogicComponent>().outputNodes)
            .Concat(innerOutputs)
            .Concat(innerInputs))
        {
            if (node.attachedWires.Count > 0)
            {
                foreach (Wire wire in node.attachedWires)
                {
                    if (wire)
                    {
                        wire.Redraw();

                    }
                }
            }
        }
        componentsNameText.gameObject.SetActive(true);
        moduleNameText.gameObject.SetActive(false);
    }
}
