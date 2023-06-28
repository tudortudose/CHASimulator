using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectElement : MonoBehaviour
{
    public string projectKey;
    public string projectName;
    public bool projectStatus;
    public bool isMine;
    public string ownerUID;

    public void OpenProject()
    {
        GameObject.FindObjectOfType<FirebaseManager>().OpenProjectTrigger(projectKey, ownerUID, isMine);
    }

    public void DeleteProject()
    {
        if (isMine)
        {
            GameObject.FindObjectOfType<FirebaseManager>().DeleteProjectTrigger(projectKey);
        }
    }
}
