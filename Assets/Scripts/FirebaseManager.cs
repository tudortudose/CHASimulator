using System.Collections;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using TMPro;
using System.Linq;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

public class FirebaseManager : MonoBehaviour
{
    //Firebase variables
    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;
    public FirebaseUser User;
    public DatabaseReference DBreference;

    //Login variables
    [Header("Login")]
    public TMP_InputField emailLoginField;
    public TMP_InputField passwordLoginField;
    public TMP_Text warningLoginText;
    public TMP_Text confirmLoginText;

    //Register variables
    [Header("Register")]
    public TMP_InputField usernameRegisterField;
    public TMP_InputField emailRegisterField;
    public TMP_InputField passwordRegisterField;
    public TMP_InputField passwordRegisterVerifyField;
    public TMP_Text warningRegisterText;

    //User Data variables
    [Header("Projects")]
    public GameObject personalProjectPrefab;
    public GameObject otherProjectPrefab;
    public Transform myProjectsHolder;
    public Transform otherProjectsHolder;
    public TMP_InputField projectNameField;
    public Toggle publicProjectToggle;

    public string currentProjectKey = null;
    public bool foreignProjectOpen = false;
    public Transform userModulesHolder;

    public BuildingManager buildingManager;

    void Awake()
    {
        //Check that all of the necessary dependencies for Firebase are present on the system
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                //If they are avalible Initialize Firebase
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            //LoadModuleTrigger("nand3");
        }
    }

    private void InitializeFirebase()
    {
        Debug.Log("Setting up Firebase Auth");
        //Set the authentication instance object
        auth = FirebaseAuth.DefaultInstance;
        DBreference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void ClearLoginFeilds()
    {
        emailLoginField.text = "";
        passwordLoginField.text = "";
    }

    public void ClearRegisterFeilds()
    {
        usernameRegisterField.text = "";
        emailRegisterField.text = "";
        passwordRegisterField.text = "";
        passwordRegisterVerifyField.text = "";
    }

    public void LoginButton()
    {
        StartCoroutine(Login(emailLoginField.text, passwordLoginField.text));
    }

    public void RegisterButton()
    {
        StartCoroutine(Register(emailRegisterField.text, passwordRegisterField.text, usernameRegisterField.text));
    }

    public void SignOutButton()
    {
        auth.SignOut();
        ClearUserModules();
        UIManager.instance.LoginScreen();
        ClearRegisterFeilds();
        ClearLoginFeilds();
    }

    public void ClearUserModules()
    {
        GameObject.FindObjectOfType<SavingManager>().userModules = new();
        for (int i = 0; i < userModulesHolder.childCount; i++)
        {
            Destroy(userModulesHolder.GetChild(i).gameObject);
        }

        for (int i = 13; i < buildingManager.contentHolder.childCount; i++)
        {
            Destroy(buildingManager.contentHolder.GetChild(i).gameObject);
        }
    }

    public void CreateNewProjectButton()
    {
        StartCoroutine(CreateUserProject(projectNameField.text, publicProjectToggle.isOn));
    }

    private IEnumerator Login(string _email, string _password)
    {
        //Call the Firebase auth signin function passing the email and password
        var LoginTask = auth.SignInWithEmailAndPasswordAsync(_email, _password);
        //Wait until the task completes
        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);

        if (LoginTask.Exception != null)
        {
            //If there are errors handle them
            Debug.LogWarning(message: $"Failed to login task with {LoginTask.Exception}");
            FirebaseException firebaseEx = LoginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = "Login Failed!";
            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    message = "Missing Email";
                    break;
                case AuthError.MissingPassword:
                    message = "Missing Password";
                    break;
                case AuthError.WrongPassword:
                    message = "Wrong Password";
                    break;
                case AuthError.InvalidEmail:
                    message = "Invalid Email";
                    break;
                case AuthError.UserNotFound:
                    message = "Account does not exist";
                    break;
            }
            warningLoginText.text = message;
        }
        else
        {
            //User is now logged in
            //Now get the result
            User = LoginTask.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1}), {2}", User.UserId, User.Email, User.DisplayName);
            warningLoginText.text = "";
            confirmLoginText.text = "Logged In";
            StartCoroutine(LoadUserProjects());
            yield return new WaitForSecondsRealtime(0.5f);
            StartCoroutine(LoadOtherPublicProjects());
            yield return new WaitForSecondsRealtime(0.5f);
            StartCoroutine(LoadAdminModules(userModulesHolder));
            yield return new WaitForSecondsRealtime(1f);
            StartCoroutine(LoadUserModules(userModulesHolder));
            yield return new WaitForSeconds(1f);

            UIManager.instance.ProjectsScreen();
            confirmLoginText.text = "";
            ClearLoginFeilds();
            ClearRegisterFeilds();
        }
    }

    private IEnumerator Register(string _email, string _password, string _username)
    {
        if (_username == "")
        {
            //If the username field is blank show a warning
            warningRegisterText.text = "Missing Username";
        }
        else if (passwordRegisterField.text != passwordRegisterVerifyField.text)
        {
            //If the password does not match show a warning
            warningRegisterText.text = "Password Does Not Match!";
        }
        else
        {
            //Call the Firebase auth signin function passing the email and password
            var RegisterTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);
            //Wait until the task completes
            yield return new WaitUntil(predicate: () => RegisterTask.IsCompleted);

            if (RegisterTask.Exception != null)
            {
                //If there are errors handle them
                Debug.LogWarning(message: $"Failed to register task with {RegisterTask.Exception}");
                FirebaseException firebaseEx = RegisterTask.Exception.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

                string message = "Register Failed!";
                switch (errorCode)
                {
                    case AuthError.MissingEmail:
                        message = "Missing Email";
                        break;
                    case AuthError.MissingPassword:
                        message = "Missing Password";
                        break;
                    case AuthError.WeakPassword:
                        message = "Weak Password";
                        break;
                    case AuthError.EmailAlreadyInUse:
                        message = "Email Already In Use";
                        break;
                }
                warningRegisterText.text = message;
            }
            else
            {
                //User has now been created
                //Now get the result
                User = RegisterTask.Result;

                if (User != null)
                {
                    var DBTask = DBreference.Child("users").Child(User.UserId).Child("username").SetValueAsync(_username);

                    yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

                    //Create a user profile and set the username
                    UserProfile profile = new UserProfile { DisplayName = _username };

                    //Call the Firebase auth update user profile function passing the profile with the username
                    var ProfileTask = User.UpdateUserProfileAsync(profile);
                    //Wait until the task completes
                    yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

                    if (ProfileTask.Exception != null)
                    {
                        //If there are errors handle them
                        Debug.LogWarning(message: $"Failed to register task with {ProfileTask.Exception}");
                        warningRegisterText.text = "Username Set Failed!";
                    }
                    else
                    {
                        //Username is now set
                        //Now return to login screen
                        UIManager.instance.LoginScreen();
                        warningRegisterText.text = "";
                        ClearRegisterFeilds();
                        ClearLoginFeilds();
                    }
                }
            }
        }
    }

    public void LoadMyProjectsButtton()
    {
        StartCoroutine(LoadUserProjects());
    }

    private IEnumerator LoadUserProjects()
    {
        //Get the currently logged in user data
        var DBTask = DBreference.Child("users").Child(User.UserId).GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to load projects task with {DBTask.Exception}");
        }
        else if (DBTask.Result.Value == null)
        {
            // no projects
        }
        else
        {
            DataSnapshot snapshot = DBTask.Result;

            var projects = snapshot.Child("projects").Children;

            for (int i = 0; i < myProjectsHolder.childCount; i++)
            {
                Destroy(myProjectsHolder.GetChild(i).gameObject);
            }

            foreach (var projectName in projects)
            {
                var go = Instantiate(personalProjectPrefab, myProjectsHolder);
                go.GetComponent<ProjectElement>().projectName = projectName.Child("name").Value.ToString();
                go.GetComponent<ProjectElement>().projectKey = projectName.Key;
                go.GetComponent<ProjectElement>().isMine = true;
                go.GetComponent<ProjectElement>().ownerUID = User.UserId;
                go.GetComponent<ProjectElement>().projectStatus = bool.Parse(projectName.Child("status").Value.ToString());

                go.transform.GetChild(0).GetComponent<TMP_Text>().text = go.GetComponent<ProjectElement>().projectName;
                go.transform.GetChild(1).GetComponent<TMP_Text>().text = go.GetComponent<ProjectElement>().projectStatus ? "public" : "private";
            }
        }
    }

    public void LoadPublicProjectsButtton()
    {
        StartCoroutine(LoadOtherPublicProjects());
    }
    private IEnumerator LoadOtherPublicProjects()
    {
        //Get the currently logged in user data
        var DBTask = DBreference.Child("users").GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to load projects task with {DBTask.Exception}");
        }
        else if (DBTask.Result.Value == null)
        {
            // no projects
        }
        else
        {
            DataSnapshot snapshot = DBTask.Result;
            for (int i = 0; i < otherProjectsHolder.childCount; i++)
            {
                Destroy(otherProjectsHolder.GetChild(i).gameObject);
            }

            foreach (var user in snapshot.Children)
            {
                if(user.Key != User.UserId)
                {
                    var projects = user.Child("projects").Children;

                    foreach (var projectName in projects)
                    {
                        if (bool.Parse(projectName.Child("status").Value.ToString()))
                        {
                            var go = Instantiate(otherProjectPrefab, otherProjectsHolder);
                            go.GetComponent<ProjectElement>().projectName = projectName.Child("name").Value.ToString();
                            go.GetComponent<ProjectElement>().projectKey = projectName.Key;
                            go.GetComponent<ProjectElement>().isMine = false;
                            go.GetComponent<ProjectElement>().ownerUID = user.Key;
                            go.GetComponent<ProjectElement>().projectStatus = bool.Parse(projectName.Child("status").Value.ToString());

                            go.transform.GetChild(0).GetComponent<TMP_Text>().text = go.GetComponent<ProjectElement>().projectName;
                            go.transform.GetChild(1).GetComponent<TMP_Text>().text = user.Child("username").Value.ToString();
                        }
                        
                    }
                }
            }
        }
    }

    [Serializable]
    public class Project
    {
        public string name;
        public bool status;
        public string components;

        public Project(string name, bool status)
        {
            this.name = name;
            this.status = status;
            this.components = null;
        }
    }

    private IEnumerator CreateUserProject(string projectName, bool publicStatus)
    {
        var newProject = new Project(projectName, publicStatus);
        string projectKey = DBreference.Child("users").Child(User.UserId).Child("projects").Push().Key;
        var DBTask = DBreference.Child("users").Child(User.UserId).Child("projects").Child(projectKey).SetRawJsonValueAsync(JsonUtility.ToJson(newProject));

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to create project with {DBTask.Exception}");
        }
        else
        {
            StartCoroutine(LoadUserProjects());
        }
    }

    public void DeleteProjectTrigger(string projectKey)
    {
        StartCoroutine(DeleteUserProject(projectKey));
    }

    private IEnumerator DeleteUserProject(string projectKey)
    {
        var DBTask = DBreference.Child("users").Child(User.UserId).Child("projects").Child(projectKey).RemoveValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to remove project with {DBTask.Exception}");
        }
        else
        {
            StartCoroutine(LoadUserProjects());
        }
    }

    public void OpenProjectTrigger(string projectKey, string ownerUID, bool isMine)
    {
        StartCoroutine(OpenProject(projectKey, ownerUID, isMine));
    }

    private IEnumerator OpenProject(string projectKey, string ownerUID, bool isMine)
    {
        //Get the currently logged in user data
        var DBTask = DBreference.Child("users").Child(ownerUID).Child("projects").Child(projectKey).Child("components").GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to open project task with {DBTask.Exception}");
        }
        else if (DBTask.Result.Value == null)
        {
            GameObject.FindObjectOfType<UIManager>().MainSceen();
            currentProjectKey = projectKey;
            foreignProjectOpen = !isMine;
        }
        else
        {
            GameObject.FindObjectOfType<UIManager>().MainSceen();
            currentProjectKey = projectKey;
            foreignProjectOpen = !isMine;

            DataSnapshot snapshot = DBTask.Result;

            Debug.Log(snapshot.GetRawJsonValue());

            GameObject.FindObjectOfType<SavingManager>().LoadProject(snapshot.GetRawJsonValue());
        }
    }

    public void SaveProjectTrigger(string jsonValue)
    {
        StartCoroutine(SaveProject(jsonValue));
    }

    private IEnumerator SaveProject(string jsonValue)
    {
        //Get the currently logged in user data
        Debug.Log("User id: " + User.UserId);
        var DBTask = DBreference.Child("users").Child(User.UserId).Child("projects").Child(currentProjectKey).Child("components").SetRawJsonValueAsync(jsonValue);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to save project task with {DBTask.Exception}");
        }
        else
        {
            Debug.Log("Saved Successfully");
        }
    }

    public void SaveModuleTrigger(string jsonValue, string moduleName)
    {
        StartCoroutine(SaveModule(jsonValue, moduleName));
    }

    private IEnumerator SaveModule(string jsonValue, string moduleName)
    {
        //Get the currently logged in user data
        Debug.Log("User id: " + User.UserId);
        string moduleKey = null;

        var DBTaskGet = DBreference.Child("users").Child(User.UserId).Child("modules").GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTaskGet.IsCompleted);

        if (DBTaskGet.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to save module task with {DBTaskGet.Exception}");
        }
        else
        {
            DataSnapshot snapshot = DBTaskGet.Result;
            foreach (DataSnapshot moduleSnapshot in snapshot.Children)
            {
                string currentModuleName = moduleSnapshot.Child("moduleName").Value.ToString();
                if (currentModuleName == moduleName)
                {
                    moduleKey = moduleSnapshot.Key;
                }
            }
        }

        if (moduleKey == null)
        {
            moduleKey = DBreference.Child("users").Child(User.UserId).Child("modules").Push().Key;
        }

        var DBTaskSave = DBreference.Child("users").Child(User.UserId).Child("modules").Child(moduleKey).SetRawJsonValueAsync(jsonValue);


        yield return new WaitUntil(predicate: () => DBTaskSave.IsCompleted);

        if (DBTaskSave.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to save module task with {DBTaskSave.Exception}");
        }
        else
        {
            Debug.Log("Saved Successfully");
        }
    }

    public void LoadModuleTrigger(string moduleName, Transform holder)
    {
        StartCoroutine(LoadModule(moduleName, holder));
    }

    private IEnumerator LoadModule(string moduleName, Transform holder)
    {
        Debug.Log("Module loading");
        //Get the currently logged in user data
        var DBTask = DBreference.Child("users").Child(User.UserId).Child("modules").GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to load module task with {DBTask.Exception}");
        }
        else
        {
            DataSnapshot snapshot = DBTask.Result;
            foreach (DataSnapshot moduleSnapshot in snapshot.Children)
            {
                string currentModuleName = moduleSnapshot.Child("moduleName").Value.ToString();
                if (currentModuleName == moduleName)
                {
                    GameObject.FindObjectOfType<SavingManager>().LoadModuleTrigger(moduleSnapshot.GetRawJsonValue(), holder);
                }
            }
        }
    }


    private IEnumerator LoadAdminModules(Transform holder)
    {
        Debug.Log("Modules loading");
        //Get the currently logged in user data
        var DBTask = DBreference.Child("users").Child("q3HrSFxAbMhArvb7Qd1Ss8nKKq42").Child("modules").GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to load modules task with {DBTask.Exception}");
        }
        else
        {
            DataSnapshot snapshot = DBTask.Result;
            foreach (DataSnapshot moduleSnapshot in snapshot.Children)
            {
                GameObject.FindObjectOfType<SavingManager>().LoadModuleTrigger(moduleSnapshot.GetRawJsonValue(), holder);

                yield return new WaitForSecondsRealtime(0.1f);
            }
        }
    }

    private IEnumerator LoadUserModules(Transform holder)
    {
        Debug.Log("Modules loading");
        //Get the currently logged in user data
        var DBTask = DBreference.Child("users").Child(User.UserId).Child("modules").GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to load modules task with {DBTask.Exception}");
        }
        else
        {
            DataSnapshot snapshot = DBTask.Result;
            foreach (DataSnapshot moduleSnapshot in snapshot.Children)
            {
                GameObject.FindObjectOfType<SavingManager>().LoadModuleTrigger(moduleSnapshot.GetRawJsonValue(), holder);

                yield return new WaitForSecondsRealtime(0.1f);
            }
        }
    }
}