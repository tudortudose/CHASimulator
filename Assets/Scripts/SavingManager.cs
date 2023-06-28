using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SavingManager : MonoBehaviour
{
    public Dictionary<string, GameObject> userModules = new();

    public WireGenerator wireGenerator;
    public BuildingManager buildingManager;

    public Transform projectComponentsHolder;
    public Transform projectWiresHolder;

    public GameObject modulePrefab;
    public GameObject inputNodePrefab;
    public GameObject outputNodePrefab;
    public List<GameObject> prefabs;
    public GameObject buildingElementPrefab;

    [System.Serializable]
    public class SavingLogicalComponent
    {
        public string componentName;
        public Vector3 position;
        public Quaternion rotation;
    }

    [System.Serializable]
    public class SavingWire
    {
        public int outputComponentIndex;
        public string outputComponentType;

        public int inputComponentIndex;
        public string inputComponentType;

        public int outputNodeIndex;
        public int inputNodeIndex;
    }

    [System.Serializable]
    public class ProjectData
    {
        public List<SavingLogicalComponent> logicalComponents = new();
        public List<SavingWire> wires = new();
    }

    [System.Serializable]
    public class ModuleData
    {
        public string moduleName;
        public int inputCount;
        public int outputCount;

        public List<SavingLogicalComponent> logicalComponents = new();
        public List<SavingWire> wires = new();
    }

    public void ClearUserModules()
    {

    }

    public void ClearProject()
    {
        for (int i = 0; i < projectComponentsHolder.childCount; i++)
        {
            Destroy(projectComponentsHolder.GetChild(i).gameObject);
        }
        for (int i = 0; i < projectWiresHolder.childCount; i++)
        {
            Destroy(projectWiresHolder.GetChild(i).gameObject);
        }
    }

    public void LoadProject(string jsonData)
    {
        if (jsonData == "\"\"" || jsonData == null) return;
        Debug.Log(jsonData);
        ProjectData projectData = JsonUtility.FromJson<ProjectData>(jsonData);

        foreach (var component in projectData.logicalComponents)
        {
            var prefab = GetPrefabByName(component.componentName);
            Instantiate(prefab, component.position, component.rotation, projectComponentsHolder);
        }

        foreach (var wire in projectData.wires)
        {
            var outputNode = projectComponentsHolder.GetChild(wire.outputComponentIndex).GetComponent<LogicComponent>().outputNodes[wire.outputNodeIndex];
            var inputNode = projectComponentsHolder.GetChild(wire.inputComponentIndex).GetComponent<LogicComponent>().inputNodes[wire.inputNodeIndex];

            wireGenerator.FinishWire(outputNode, inputNode, projectWiresHolder);
        }
    }

    public int GetPrefabIndexByName(string name)
    {
        switch (name)
        {
            case "AND":
                return 0;
            case "NAND":
                return 1;
            case "OR":
                return 2;
            case "NOR":
                return 3;
            case "XOR":
                return 4;
            case "XNOR":
                return 5;
            case "NOT":
                return 6;

            case "SWITCH":
                return 7;
            case "LIGHT_BULB":
                return 8;
            case "SPEAKER":
                return 9;
            case "CLOCK":
                return 10;
            case "IN_NUM":
                return 11;
            case "OUT_NUM":
                return 12;

            default: return -1;
        }
    }

    public GameObject GetPrefabByIndex(int index)
    {
        if (index >= 0 && index <= 12)
        {
            return prefabs[index];
        }
        else
        {
            return null;
        }
    }

    public GameObject GetPrefabByName(string name)
    {
        int index = GetPrefabIndexByName(name);
        if (index != -1)
        {
            return prefabs[index];
        }
        else
        {
            return userModules[name];
        }
    }

    public void SaveProject()
    {
        if (GameObject.FindObjectOfType<FirebaseManager>().foreignProjectOpen)
        {
            ClearProject();
            return;
        }

        ProjectData projectData = new ProjectData();

        projectData.logicalComponents = SaveComponents(projectComponentsHolder);
        projectData.wires = SaveWires(projectWiresHolder);

        string jsonData = JsonUtility.ToJson(projectData, true);
        Debug.Log(jsonData);
        GameObject.FindObjectOfType<FirebaseManager>().SaveProjectTrigger(jsonData);

        ClearProject();
    }

    public void LoadModuleTrigger(string jsonData, Transform holder)
    {
        StartCoroutine(LoadModule(jsonData, holder));
    }

    IEnumerator LoadModule(string jsonData, Transform holder)
    {
        if (jsonData == "\"\"" || jsonData == null) yield break;
        Debug.Log(jsonData);
        ModuleData moduleData = JsonUtility.FromJson<ModuleData>(jsonData);

        var newModule = (Instantiate(modulePrefab, holder)).GetComponent<LogicModule>();
        newModule.moduleName = moduleData.moduleName;
        newModule.moduleNameText.text = moduleData.moduleName;
        newModule.componentsNameText.text = moduleData.moduleName;


        for (int i = 0; i < moduleData.inputCount; i++)
        {
            newModule.transform.GetChild(0).GetChild(i).gameObject.SetActive(true);
            newModule.transform.GetChild(4).GetChild(0).GetChild(i).gameObject.SetActive(true);
        }

        yield return new WaitForEndOfFrame();

        for (int i = 0; i < moduleData.inputCount; i++)
        {
            GameObject newNode = Instantiate(inputNodePrefab, newModule.GetComponent<LogicModule>().inputs);
            newModule.GetComponent<LogicModule>().inputPins.Add(newNode.GetComponent<Pin>());
            newNode.transform.position = newModule.transform.GetChild(4).GetChild(0).GetChild(i).transform.position;

            newModule.GetComponent<LogicComponent>().inputNodes.Add(newNode.transform.GetChild(0).GetChild(0).GetComponent<Node>());
            newModule.GetComponent<LogicModule>().innerOutputs.Add(newNode.transform.GetChild(1).GetChild(0).GetComponent<Node>());
        }

        for (int i = 0; i < moduleData.outputCount; i++)
        {
            newModule.transform.GetChild(1).GetChild(i).gameObject.SetActive(true);
            newModule.transform.GetChild(4).GetChild(1).GetChild(i).gameObject.SetActive(true);
        }

        yield return new WaitForEndOfFrame();

        for (int i = 0; i < moduleData.outputCount; i++)
        {
            GameObject newNode = Instantiate(outputNodePrefab, newModule.GetComponent<LogicModule>().outputs);
            newModule.GetComponent<LogicModule>().outputPins.Add(newNode.GetComponent<Pin>());
            newNode.transform.position = newModule.transform.GetChild(4).GetChild(1).GetChild(i).transform.position;

            newModule.GetComponent<LogicComponent>().outputNodes.Add(newNode.transform.GetChild(1).GetChild(0).GetComponent<Node>());
            newModule.GetComponent<LogicModule>().innerInputs.Add(newNode.transform.GetChild(0).GetChild(0).GetComponent<Node>());
        }

        float rightXPos = float.MinValue;

        foreach (var component in moduleData.logicalComponents)
        {
            GameObject prefab = GetPrefabByName(component.componentName);

            var go = Instantiate(prefab, component.position, component.rotation, newModule.componentsHolder);

            go.transform.localPosition = component.position;
            go.transform.localRotation = component.rotation;

            rightXPos = Mathf.Max(component.position.x, rightXPos);
        }

        newModule.GetComponent<LogicModule>().outputs.localPosition = new Vector3(rightXPos + 3, 0, 0);

        newModule.transform.GetChild(4).GetChild(1).localPosition = new Vector3(rightXPos + 3, 0, 0);

        float right = float.MinValue, left = float.MaxValue, up = float.MinValue, down = float.MaxValue;

        foreach (var obj in newModule.inputPins.Concat(newModule.outputPins))
        {
            if (obj.gameObject.transform.GetComponent<Pin>())
            {
                right = Mathf.Max(obj.gameObject.transform.position.x, right);
                left = Mathf.Min(obj.gameObject.transform.position.x, left);
                up = Mathf.Max(obj.gameObject.transform.position.y, up);
                down = Mathf.Min(obj.gameObject.transform.position.y, down);
            }
        }

        newModule.CreateComponentSprite(right, left, up, down);

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

        newModule.CreateModuleSprite(moduleUp, moduleDown);

        foreach (var wire in moduleData.wires)
        {
            Node outputNode = null;
            Node inputNode = null;

            switch (wire.outputComponentType)
            {
                case "PIN":
                    outputNode = newModule.inputs.GetChild(wire.outputComponentIndex).GetComponent<LogicComponent>().outputNodes[wire.outputNodeIndex];
                    break;
                case "COMP":
                    outputNode = newModule.componentsHolder.GetChild(wire.outputComponentIndex).GetComponent<LogicComponent>().outputNodes[wire.outputNodeIndex];
                    break;
            }

            switch (wire.inputComponentType)
            {
                case "PIN":
                    inputNode = newModule.outputs.GetChild(wire.inputComponentIndex).GetComponent<LogicComponent>().inputNodes[wire.inputNodeIndex];
                    break;
                case "COMP":
                    inputNode = newModule.componentsHolder.GetChild(wire.inputComponentIndex).GetComponent<LogicComponent>().inputNodes[wire.inputNodeIndex];
                    break;
            }

            wireGenerator.FinishWire(outputNode, inputNode, newModule.wiresHolder);
        }

        newModule.Minimize();


        if (userModules.ContainsKey(newModule.moduleName)) yield break;
        userModules.Add(newModule.moduleName, newModule.gameObject);

        var buildingContentHolder = buildingManager.contentHolder;
        int elemIndex = buildingContentHolder.childCount;
        var bElem = Instantiate(buildingElementPrefab, buildingContentHolder);
        bElem.GetComponent<Button>().onClick.AddListener(() => buildingManager.SelectBuildingObject(elemIndex));
        bElem.transform.GetChild(0).GetComponent<TMP_Text>().text = newModule.moduleName;
    }

    public void SaveModule(LogicModule module)
    {
        ModuleData moduleData = new();

        moduleData.moduleName = module.moduleName;
        moduleData.inputCount = module.inputPins.Count;
        moduleData.outputCount = module.outputPins.Count;

        moduleData.logicalComponents = SaveComponents(module.componentsHolder);
        moduleData.wires = SaveWires(module.wiresHolder, module.moduleName);

        string jsonData = JsonUtility.ToJson(moduleData, true);
        Debug.Log(jsonData);
        GameObject.FindObjectOfType<FirebaseManager>().SaveModuleTrigger(jsonData, moduleData.moduleName);

        if (!userModules.ContainsKey(module.moduleName))
        {
            LoadModuleTrigger(jsonData, GameObject.FindObjectOfType<FirebaseManager>().userModulesHolder);
        }
    }

    public List<SavingWire> SaveWires(Transform wiresHolder, string moduleName = "")
    {
        List<SavingWire> wires = new();
        for (int i = 0; i < wiresHolder.childCount; i++)
        {
            var wire = wiresHolder.GetChild(i);

            var savingWire = new SavingWire();

            var parent = wire.GetComponent<Wire>().outputNode.transform.parent.parent;
            int outputComponentIndex;
            int outputNodeIndex;
            string outputComponentType;

            if (parent.GetComponent<Pin>())
            {
                if (wire.GetComponent<Wire>().outputNode.transform.parent.parent.parent.parent.GetComponent<LogicModule>().moduleName == moduleName)
                {
                    outputComponentType = "PIN";
                    outputComponentIndex = wire.GetComponent<Wire>().outputNode.transform.parent.parent.GetSiblingIndex();
                    outputNodeIndex = 0;
                }
                else
                {
                    outputComponentType = "COMP";
                    outputComponentIndex = wire.GetComponent<Wire>().outputNode.transform.parent.parent.parent.parent.GetSiblingIndex();
                    outputNodeIndex = wire.GetComponent<Wire>().outputNode.transform.parent.parent.GetSiblingIndex();
                }
            }
            else
            {
                outputComponentIndex = wire.GetComponent<Wire>().outputNode.transform.parent.parent.GetSiblingIndex();
                outputNodeIndex = wire.GetComponent<Wire>().outputNode.transform.GetSiblingIndex();
                outputComponentType = "COMP";
            }

            parent = wire.GetComponent<Wire>().inputNode.transform.parent.parent;
            int inputComponentIndex;
            int inputNodeIndex;
            string inputComponentType;

            if (parent.GetComponent<Pin>())
            {
                if (wire.GetComponent<Wire>().inputNode.transform.parent.parent.parent.parent.GetComponent<LogicModule>().moduleName == moduleName)
                {
                    inputComponentType = "PIN";
                    inputComponentIndex = wire.GetComponent<Wire>().inputNode.transform.parent.parent.GetSiblingIndex();
                    inputNodeIndex = 0;
                }
                else
                {
                    inputComponentType = "COMP";
                    inputComponentIndex = wire.GetComponent<Wire>().inputNode.transform.parent.parent.parent.parent.GetSiblingIndex();
                    inputNodeIndex = wire.GetComponent<Wire>().inputNode.transform.parent.parent.GetSiblingIndex();
                }
            }
            else
            {
                inputComponentIndex = wire.GetComponent<Wire>().inputNode.transform.parent.parent.GetSiblingIndex();
                inputNodeIndex = wire.GetComponent<Wire>().inputNode.transform.GetSiblingIndex();
                inputComponentType = "COMP";
            }



            savingWire.outputComponentIndex = outputComponentIndex;
            savingWire.outputComponentType = outputComponentType;


            savingWire.inputComponentIndex = inputComponentIndex;
            savingWire.inputComponentType = inputComponentType;

            savingWire.outputNodeIndex = outputNodeIndex;
            savingWire.inputNodeIndex = inputNodeIndex;

            wires.Add(savingWire);
        }
        return wires;
    }

    public List<SavingLogicalComponent> SaveComponents(Transform componentsHolder)
    {
        List<SavingLogicalComponent> logicalComponents = new();
        for (int i = 0; i < componentsHolder.childCount; i++)
        {
            var component = componentsHolder.GetChild(i);

            var savingObject = new SavingLogicalComponent();
            savingObject.position = component.transform.localPosition;
            savingObject.rotation = component.transform.localRotation;

            if (component.GetComponent<LogicGate>())
            {
                savingObject.componentName = component.GetComponent<LogicGate>().gateType.ToString();
            }
            else if (component.GetComponent<LightBulb>())
            {
                savingObject.componentName = "LIGHT_BULB";
            }
            else if (component.GetComponent<Switch>())
            {
                savingObject.componentName = "SWITCH";
            }
            else if (component.GetComponent<NumberInput>())
            {
                savingObject.componentName = "IN_NUM";
            }
            else if (component.GetComponent<NumberOutput>())
            {
                savingObject.componentName = "OUT_NUM";
            }
            else if (component.GetComponent<SpeakerPerif>())
            {
                savingObject.componentName = "SPEAKER";
            }
            else if (component.GetComponent<LogicClock>())
            {
                savingObject.componentName = "CLOCK";
            }
            else if (component.GetComponent<LogicModule>())
            {
                savingObject.componentName = component.GetComponent<LogicModule>().componentsNameText.text;
            }

            logicalComponents.Add(savingObject);
        }
        return logicalComponents;
    }
}
