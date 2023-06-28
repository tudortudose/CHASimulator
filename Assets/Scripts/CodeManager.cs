using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.Windows;
using Unity.VisualScripting;
using UnityEditor;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Text.RegularExpressions;

public class CodeManager : MonoBehaviour
{
    public TMP_InputField inputField;
    public string[] redWords = { };
    public string[] yellowWords = { };
    public List<GameObject> logicComponents = new();
    public GameObject inputNodePrefab;
    public GameObject outputNodePrefab;
    public GameObject modulePrefab;
    public Transform logicComponentsHolder;

    public Transform errorHolder;
    public TMP_Text errorText;

    public List<CodeObject> codeObjects;

    public class CodeObject
    {
        public string name;
        public CodeObjectType type;
        public GameObject gameObject;

        public CodeObject(string name, CodeObjectType type, GameObject gameObject)
        {
            this.name = name;
            this.type = type;
            this.gameObject = gameObject;
        }
    }

    public enum CodeObjectType
    {
        inputNode, outputNode, logicGate, module
    }

    private CodeObject GetCodeObjectByName(string name)
    {
        foreach (var codeObject in codeObjects)
        {
            if (codeObject.name == name)
            {
                return codeObject;
            }
        }
        throw new Exception("Component " + name + " is not defined!");
    }

    private int getTypeIndex(string type)
    {
        switch (type)
        {
            case "and":
                return 0;
            case "nand":
                return 1;
            case "or":
                return 2;
            case "nor":
                return 3;
            case "xor":
                return 4;
            case "nxor":
                return 5;
            case "not":
                return 6;

            default:
                return -1;
        }
    }

    public void InterpretCodeButton()
    {
        StartCoroutine(InterpretCode());
    }

    IEnumerator InterpretCode()
    {
        CloseErrorHolder();
        codeObjects = new();
        string text = FormatCodeString(inputField.text);
        string[] lines = text.Split('\n');

        GameObject newModule = null;
        int lineCounter = -1;

        foreach (string line in lines)
        {
            lineCounter++;

            if (line.Length <= 1)
            {
                continue;
            }
            else if (line.StartsWith("module"))
            {
                string moduleName = line.Split(' ')[1].Trim();
                newModule = Instantiate(modulePrefab, new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, 0), Quaternion.identity, logicComponentsHolder);
                newModule.name = moduleName;
                newModule.GetComponent<LogicModule>().moduleName = moduleName;
                newModule.GetComponent<LogicModule>().moduleNameText.text = moduleName;
                newModule.GetComponent<LogicModule>().componentsNameText.text = moduleName;
            }
            else if (line.StartsWith("endmodule"))
            {
                // module end
            }
            else if (line.StartsWith("inputs:"))
            {
                string lline = line.Substring(line.IndexOf(':') + 1);
                string[] node_names = lline.Split(',').Select(x => x.Trim()).ToArray();
                int contor = 0;

                for (int i = 0; i < node_names.Length; i++)
                {
                    newModule.transform.GetChild(0).GetChild(i).gameObject.SetActive(true);
                    newModule.transform.GetChild(4).GetChild(0).GetChild(i).gameObject.SetActive(true);
                }
                yield return new WaitForEndOfFrame();

                foreach (var node_name in node_names)
                {
                    GameObject newNode = Instantiate(inputNodePrefab, newModule.GetComponent<LogicModule>().inputs);
                    newModule.GetComponent<LogicModule>().inputPins.Add(newNode.GetComponent<Pin>());
                    newNode.transform.position = newModule.transform.GetChild(4).GetChild(0).GetChild(contor++).transform.position;

                    codeObjects.Add(new CodeObject(node_name, CodeObjectType.inputNode, newNode));

                    newModule.GetComponent<LogicComponent>().inputNodes.Add(newNode.transform.GetChild(0).GetChild(0).GetComponent<Node>());
                    newModule.GetComponent<LogicModule>().innerOutputs.Add(newNode.transform.GetChild(1).GetChild(0).GetComponent<Node>());
                }
            }
            else if (line.StartsWith("outputs:"))
            {
                string lline = line.Substring(line.IndexOf(':') + 1);
                string[] node_names = lline.Split(',').Select(x => x.Trim()).ToArray();
                int contor = 0;

                for (int i = 0; i < node_names.Length; i++)
                {
                    newModule.transform.GetChild(1).GetChild(i).gameObject.SetActive(true);
                    newModule.transform.GetChild(4).GetChild(1).GetChild(i).gameObject.SetActive(true);
                }
                yield return new WaitForEndOfFrame();

                foreach (var node_name in node_names)
                {
                    GameObject newNode = Instantiate(outputNodePrefab, newModule.GetComponent<LogicModule>().outputs);
                    newModule.GetComponent<LogicModule>().outputPins.Add(newNode.GetComponent<Pin>());
                    newNode.transform.position = newModule.transform.GetChild(4).GetChild(1).GetChild(contor++).transform.position;

                    codeObjects.Add(new CodeObject(node_name, CodeObjectType.outputNode, newNode));


                    newModule.GetComponent<LogicComponent>().outputNodes.Add(newNode.transform.GetChild(1).GetChild(0).GetComponent<Node>());
                    newModule.GetComponent<LogicModule>().innerInputs.Add(newNode.transform.GetChild(0).GetChild(0).GetComponent<Node>());
                }
            }
            else
            {
                try
                {
                    string[] eq_splits = line.Split('=');
                    string var_name = eq_splits[0].Trim();
                    string var_object = eq_splits[1].Trim();

                    string component_name;
                    string[] var_params = new string[0];

                    int params_index_start = var_object.IndexOf('(');
                    int params_index_end = var_object.LastIndexOf(')');

                    if (params_index_start == -1 || params_index_end == -1)
                    {
                        component_name = var_object;
                    }
                    else
                    {
                        component_name = var_object.Substring(0, params_index_start).Trim();
                        if (params_index_start + 1 < params_index_end)
                        {
                            string params_str = var_object.Substring(params_index_start + 1, params_index_end - params_index_start - 1);
                            var_params = params_str.Split(",").Select(var => var.Trim()).ToArray();
                        }
                    }

                    if (var_name.Contains(".in")) // Assignment
                    {
                        string[] strings = var_name.Split(".in");
                        var_name = strings[0];
                        int index = int.Parse(strings[1]);

                        if (component_name.Contains(".out"))
                        {
                            string[] strings1 = component_name.Split(".out");
                            component_name = strings1[0];
                            int index1 = int.Parse(strings1[1]);

                            SetComponentInputNode(GetCodeObjectByName(var_name), index, GetCodeObjectByName(component_name), index1, newModule.transform);
                        }
                        else
                        {
                            SetComponentInputNode(GetCodeObjectByName(var_name), index, GetCodeObjectByName(component_name), 0, newModule.transform);
                        }
                    }
                    else
                    {
                        CodeObject co = null;
                        try
                        {
                            co = GetCodeObjectByName(var_name);
                        }
                        catch { }


                        if (co != null && co.type == CodeObjectType.outputNode) // output pin set
                        {
                            if (component_name.Contains(".out"))
                            {
                                string[] strings1 = component_name.Split(".out");
                                component_name = strings1[0];
                                int index1 = int.Parse(strings1[1]);

                                SetOutputNode(co, index1, new[] { component_name }, newModule.transform);
                            }
                            else
                            {
                                SetOutputNode(co, 0, new[] { component_name }, newModule.transform);
                            }
                        }
                        else // definition
                        {
                            var typeIndex = getTypeIndex(component_name);
                            if (typeIndex != -1)
                            {
                                InstantiateLogicGateCodeObject(var_name, component_name, var_params, newModule.transform);
                            }
                            else
                            {
                                InstantiateModuleCodeObject(var_name, component_name, var_params, newModule.transform);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    errorText.text = "Error at line " + lineCounter.ToString() + ":\n" + e.Message;
                    OpenErrorHolder();
                    Destroy(newModule);
                    yield break;
                }
            }
        }

        bool firstComp = true;
        for (int i = 0; i < codeObjects.Count; i++)
        {
            var obj = codeObjects[i];
            if (obj.type == CodeObjectType.inputNode || obj.type == CodeObjectType.outputNode) continue;
            var comp = obj.gameObject.GetComponent<LogicComponent>();
            float xp = -999, yp = 0;
            int cnt = 0;

            foreach (var node in comp.inputNodes)
            {
                if (node.GetComponent<Node>().attachedWires.Count > 0)
                {
                    var outputNode = node.GetComponent<Node>().attachedWires[0].outputNode;
                    Transform outputComp = null;
                    if (outputNode.transform.parent.parent.GetComponent<LogicGate>())
                    {
                        outputComp = outputNode.transform.parent.parent;
                        xp = Mathf.Max(outputComp.transform.position.x, xp);
                        yp += outputComp.transform.position.y;
                        cnt++;
                    }
                    else if (outputNode.transform.parent.parent.parent.parent.GetComponent<LogicModule>()?.moduleName == newModule.GetComponent<LogicModule>().moduleName)
                    {
                        outputComp = outputNode.transform.parent.parent;
                        xp = Mathf.Max(outputComp.transform.position.x, xp);
                        yp += outputComp.transform.position.y;
                        cnt++;
                    }
                    else
                    {
                        outputComp = outputNode.transform.parent.parent.parent.parent;
                        if (outputComp.GetComponent<LogicComponent>().finalPosition)
                        {
                            xp = Mathf.Max(outputComp.transform.position.x, xp);
                            yp += outputComp.transform.position.y;
                            cnt++;
                        }
                    }
                }
            }
            if (cnt == 0) cnt = 1;
            if (xp == -999) xp = 0;
            if (yp == -999) yp = 0;
            comp.transform.position = new Vector3(xp + 3, yp / cnt, 0);
            comp.GetComponent<LogicComponent>().finalPosition = true;

            if (firstComp)
            {
                firstComp = false;
            }
            else
            {
                while (true)
                {
                    bool ok = true;
                    for (int j = 0; j < codeObjects.Count; j++)
                    {
                        var ob = codeObjects[j];

                        if (j != i && ob.gameObject.transform.position == comp.transform.position)
                        {
                            comp.transform.position += new Vector3(3, 0, 0);
                            ok = false;
                        }
                    }
                    if (ok)
                    {
                        break;
                    }
                }
            }
            
        }

        float rightXPos = float.MinValue;

        foreach (var obj in codeObjects)
        {
            if (obj.type == CodeObjectType.outputNode) continue;
            Debug.Log(obj.type);
            rightXPos = Mathf.Max(obj.gameObject.transform.position.x, rightXPos);
        }

        newModule.GetComponent<LogicModule>().outputs.position = new Vector3(rightXPos + 3, newModule.GetComponent<LogicModule>().outputs.position.y,
                                                                             newModule.GetComponent<LogicModule>().outputs.position.z);

        newModule.transform.GetChild(4).GetChild(1).position = new Vector3(rightXPos + 3, newModule.transform.GetChild(4).GetChild(1).position.y,
                                                                             newModule.transform.GetChild(4).GetChild(1).position.z);

        foreach (var obj in codeObjects)
        {
            foreach (Node node in obj.gameObject.GetComponent<LogicComponent>().inputNodes.Concat(obj.gameObject.GetComponent<LogicComponent>().outputNodes))
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
        }

        float right = float.MinValue, left = float.MaxValue, up = float.MinValue, down = float.MaxValue;

        foreach (var obj in codeObjects)
        {
            if (obj.gameObject.transform.GetComponent<Pin>())
            {
                right = Mathf.Max(obj.gameObject.transform.position.x, right);
                left = Mathf.Min(obj.gameObject.transform.position.x, left);
                up = Mathf.Max(obj.gameObject.transform.position.y, up);
                down = Mathf.Min(obj.gameObject.transform.position.y, down);
            }
        }

        newModule.GetComponent<LogicModule>().CreateComponentSprite(right, left, up, down);


        float moduleUp = float.MinValue, moduleDown = float.MaxValue;

        for (int i = 0; i < newModule.transform.GetChild(0).childCount; i++)
        {
            if (newModule.transform.GetChild(0).GetChild(i).gameObject.activeInHierarchy == false) break;
            moduleUp = Mathf.Max(newModule.transform.GetChild(0).GetChild(i).position.y, moduleUp);
            moduleDown = Mathf.Min(newModule.transform.GetChild(0).GetChild(i).position.y, moduleDown);
        }

        for (int i = 0; i < newModule.transform.GetChild(1).childCount; i++)
        {
            if (newModule.transform.GetChild(1).GetChild(i).gameObject.activeInHierarchy == false) break;
            moduleUp = Mathf.Max(newModule.transform.GetChild(1).GetChild(i).position.y, moduleUp);
            moduleDown = Mathf.Min(newModule.transform.GetChild(1).GetChild(i).position.y, moduleDown);
        }

        newModule.GetComponent<LogicModule>().CreateModuleSprite(moduleUp, moduleDown);
    }

    public void SetComponentInputNode(CodeObject inputComp, int indexInput, CodeObject outputComp, int outputIndex, Transform newModule)
    {
        if (inputComp.gameObject.GetComponent<LogicComponent>().inputNodes.Count < indexInput + 1)
        {
            throw new Exception("Invalid input index!");
        }

        var n1 = inputComp.gameObject.GetComponent<LogicComponent>().inputNodes[indexInput];
        var n2 = GetOutputNode(outputComp, outputIndex);

        List<Transform> sourceNodes = new();

        if (n2.transform.parent.parent.GetComponent<LogicComponent>().finalPosition)
        {
            sourceNodes.Add(n2.transform.parent);
        }

        foreach (var node in inputComp.gameObject.GetComponent<LogicComponent>().inputNodes)
        {
            if (node.type == Node.Type.INPUT)
            {
                if (node.attachedWires.Count > 0)
                {
                    if (node.attachedWires[0].outputNode.transform.parent.parent.GetComponent<LogicComponent>().finalPosition)
                    {
                        sourceNodes.Add(node.attachedWires[0].outputNode.transform.parent);
                    }
                }
            }
        }

        GameObject.FindObjectOfType(typeof(WireGenerator)).GetComponent<WireGenerator>().FinishWire(n1, n2, newModule.GetComponent<LogicModule>().wiresHolder);
        inputComp.gameObject.GetComponent<LogicComponent>().RedrawWires();
    }

    public void SetOutputNode(CodeObject co, int outputIndex, string[] var_params, Transform newModule)
    {
        var output = GetOutputNode(GetCodeObjectByName(var_params[0]), outputIndex);
        var go = co.gameObject.GetComponent<Pin>();
        GameObject.FindObjectOfType(typeof(WireGenerator)).GetComponent<WireGenerator>().FinishWire(output, go.inputNodes[0], newModule.GetComponent<LogicModule>().wiresHolder);
    }

    public Node GetOutputNode(CodeObject codeObject, int outputIndex)
    {
        if (codeObject.gameObject.GetComponent<LogicComponent>().outputNodes.Count < outputIndex + 1)
        {
            throw new Exception("Incorrect output index!");
        }
        return codeObject.gameObject.GetComponent<LogicComponent>().outputNodes[outputIndex];
    }

    public void InstantiateModuleCodeObject(string name, string type, string[] var_params, Transform newModule)
    {
        if (!GameObject.FindObjectOfType<SavingManager>().userModules.ContainsKey(type))
        {
            throw new Exception("Module " + type + " does not exist!");
        }

        var go = Instantiate(GameObject.FindObjectOfType<SavingManager>().userModules[type], newModule.GetComponent<LogicModule>().componentsHolder);

        LogicModule logicModule = go.GetComponent<LogicModule>();

        if (logicModule.inputNodes.Count < var_params.Length)
        {
            throw new Exception("Invalid number of module parameters!");
        }

        for (int i = 0; i < var_params.Length; i++)
        {
            int index1 = 0;
            if (var_params[i].Contains(".out"))
            {
                string[] strings1 = var_params[i].Split(".out");
                var_params[i] = strings1[0];
                index1 = int.Parse(strings1[1]);
            }
            var output = GetOutputNode(GetCodeObjectByName(var_params[i]), index1);

            GameObject.FindObjectOfType(typeof(WireGenerator)).GetComponent<WireGenerator>().FinishWire(output, logicModule.inputNodes[i], newModule.GetComponent<LogicModule>().wiresHolder);
        }

        CodeObject codeObject = new CodeObject(name, CodeObjectType.module, go);
        codeObjects.Add(codeObject);
    }

    private void InstantiateLogicGateCodeObject(string name, string type, string[] var_params, Transform newModule)
    {
        var prefab = GameObject.FindObjectOfType<SavingManager>().GetPrefabByIndex(getTypeIndex(type));

        GameObject go = Instantiate(prefab, newModule.GetComponent<LogicModule>().componentsHolder);
        LogicGate logicGate = go.GetComponent<LogicGate>();

        if (logicGate.inputNodes.Count < var_params.Length)
        {
            throw new Exception("Invalid number of module parameters!");
        }

        for (int i = 0; i < var_params.Length; i++)
        {
            int index1 = 0;
            if (var_params[i].Contains(".out"))
            {
                string[] strings1 = var_params[i].Split(".out");
                var_params[i] = strings1[0];
                index1 = int.Parse(strings1[1]);
            }
            var output = GetOutputNode(GetCodeObjectByName(var_params[i]), index1);

            GameObject.FindObjectOfType(typeof(WireGenerator)).GetComponent<WireGenerator>().FinishWire(output, logicGate.inputNodes[i], newModule.GetComponent<LogicModule>().wiresHolder);
        }

        CodeObject codeObject = new CodeObject(name, CodeObjectType.logicGate, go);
        codeObjects.Add(codeObject);
    }

    public string FormatCodeString(string codeString)
    {
        String newString = codeString.Replace(" ", "");
        newString = Regex.Replace(newString, "<color=#[a-zA-Z0-9]*>", "");
        newString = Regex.Replace(newString, "</color>", "");

        newString = newString.Replace("=", " = ");
        newString = newString.Replace(",", ", ");
        newString = newString.Replace("inputs:", "inputs: ");
        newString = newString.Replace("outputs:", "outputs: ");
        newString = newString.Replace("module", "module ");

        return newString;
    }

    public void FormatCodeInput()
    {
        List<string> moduleRedWords = new();
        moduleRedWords.AddRange(GameObject.FindAnyObjectByType<SavingManager>().userModules.Keys);
        for (int i =0; i< moduleRedWords.Count; i++)
        {
            moduleRedWords[i] = "= " + moduleRedWords[i] + "(";
        }
        string text = inputField.text;
        string[] Lines = text.Split('\n');
        string formatedText = "";
        foreach (string line in Lines)
        {
            String newLine = line.Replace(" ", "");
            newLine = Regex.Replace(newLine, "<color=#[a-zA-Z0-9]*>", "");
            newLine = Regex.Replace(newLine, "</color>", "");

            newLine = newLine.Replace("=", " = ");
            newLine = newLine.Replace(",", ", ");
            newLine = newLine.Replace("inputs:", "inputs: ");
            newLine = newLine.Replace("outputs:", "outputs: ");
            newLine = newLine.Replace("module", "module ");
            Debug.Log("line " + newLine);
            for (int i = 0; i < newLine.Length; i++)
            {
                foreach (string word in redWords.Concat(moduleRedWords))
                {
                    if (newLine[i..].StartsWith(word))
                    {
                        newLine = newLine.Insert(i + word.Length - 1, "</color>");
                        newLine = newLine.Insert(i + 2, "<color=#FF7800FF>");
                        Debug.Log(newLine);
                        i += word.Length + "<color=#FF7800FF></color>".Length;
                        break;
                    }
                }

                foreach (string word in yellowWords)
                {
                    if (newLine[i..].StartsWith(word))
                    {
                        newLine = newLine.Insert(i + word.Length, "</color>");
                        newLine = newLine.Insert(i, "<color=#82DC32>");
                        Debug.Log(newLine);
                        i += word.Length + "<color=#FF7800FF></color>".Length;
                        break;
                    }
                }
            }

            formatedText += newLine + '\n';
        }

        inputField.text = formatedText;
        inputField.caretPosition = inputField.text.Length;
    }

    public void CloseErrorHolder()
    {
        errorHolder.gameObject.SetActive(false);
    }

    public void OpenErrorHolder()
    {
        errorHolder.gameObject.SetActive(true);
    }
}
