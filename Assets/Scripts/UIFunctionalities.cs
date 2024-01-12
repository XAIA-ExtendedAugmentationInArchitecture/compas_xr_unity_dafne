using System.Collections;
using System.Collections.Generic;
using Extentions;
using UnityEngine;
using UnityEngine.UI;
using Instantiate;
using JSON;
using Newtonsoft.Json;
using System;
using System.Linq;
using TMPro;
using System.Xml.Linq;
using UnityEngine.XR.ARFoundation;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARSubsystems;
using System.Globalization;
using ApplicationModeControler;
using Unity.IO.LowLevel.Unsafe;


public class UIFunctionalities : MonoBehaviour
{
    //Other Scripts for inuse objects
    public DatabaseManager databaseManager;
    public InstantiateObjects instantiateObjects;
    public Eventmanager eventManager;
    
    //Toggle GameObjects
    private GameObject VisibilityMenuObject;
    private GameObject MenuButtonObject;
    private GameObject EditorToggleObject;
    private GameObject ElementSearchToggleObject;
    private GameObject InfoToggleObject;
    private GameObject CommunicationToggleObject;
    
    //Primary UI Objects
    public GameObject CanvasObject;
    public GameObject ConstantUIPanelObjects;
    public GameObject NextGeometryButtonObject;
    public GameObject PreviousGeometryButtonObject;
    public GameObject PreviewGeometrySliderObject;
    public GameObject IsBuiltPanelObjects;
    public GameObject IsBuiltButtonObject;
    private Button IsBuiltButton;
    public GameObject IsbuiltButtonImage;
    public Slider PreviewGeometrySlider;
    private TMP_InputField ElementSearchInputField;
    private GameObject ElementSearchObjects;
    private GameObject SearchElementButtonObject;
    private GameObject PriorityIncompleteWarningMessageObject;
    private GameObject PriorityIncorrectWarningMessageObject;

    //Visualizer Menu Toggle Objects
    private GameObject VisualzierBackground;
    private GameObject PreviewBuilderButtonObject;
    private GameObject IDButtonObject;
    private GameObject RobotToggleObject;
    private GameObject RobotVisulizationControlObjects;
    private GameObject ObjectLengthsToggleObject;
    private GameObject ObjectLengthsUIPanelObjects;
    private TMP_Text ObjectLengthsText;
    private GameObject ObjectLengthsTags;
    public GameObject PriorityViewerToggleObject;

    //Menu Toggle Button Objects
    private GameObject MenuBackground;
    private GameObject ReloadButtonObject;
    private GameObject InfoPanelObject;
    private GameObject CommunicationPanelObject;

    //Editor Toggle Objects
    private GameObject EditorBackground;
    private GameObject BuilderEditorButtonObject;
    private GameObject BuildStatusButtonObject;
    private Button BuildStatusButton;
    
    //Object Colors
    private Color Yellow = new Color(1.0f, 1.0f, 0.0f, 1.0f);
    private Color White = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    private Color TranspWhite = new Color(1.0f, 1.0f, 1.0f, 0.4f);
    private Color TranspGrey = new Color(0.7843137f, 0.7843137f, 0.7843137f, 0.4f);

    //Parent Objects for gameObjects
    public GameObject Elements;
    public GameObject QRMarkers;
    public GameObject UserObjects;

    //AR Camera and Touch GameObjects
    public Camera arCamera;
    private GameObject activeGameObject;
    private GameObject temporaryObject;    

    //On Screen Text
    public GameObject CurrentStepTextObject;
    public GameObject EditorSelectedTextObject;
    public TMP_Text CurrentStepText;
    public TMP_Text LastBuiltIndexText;
    public TMP_Text CurrentPriorityText;
    public TMP_Text EditorSelectedText;

    //Touch Input Variables
    private ARRaycastManager rayManager;

    //In script use variables
    public string CurrentPriority = null;
    public string CurrentStep = null;
    public string SearchedElement = "None";
    public string SearchedElementStep;
    
    void Start()
    {
        //Find Initial Objects and other sctipts
        OnAwakeInitilization();
    }

    void Update()
    {
        TouchSearchControler();
    }

    /////////////////////////////////////////// UI Control ////////////////////////////////////////////////////
    private void OnAwakeInitilization()
    {
        /////////////////////////////////////////// Initial Elements & Toggles ////////////////////////////////////////////
        //Find Other Scripts
        databaseManager = GameObject.Find("DatabaseManager").GetComponent<DatabaseManager>();
        instantiateObjects = GameObject.Find("Instantiate").GetComponent<InstantiateObjects>();
        eventManager = GameObject.Find("EventManager").GetComponent<Eventmanager>();

        //Find Specific GameObjects
        Elements = GameObject.Find("Elements");
        QRMarkers = GameObject.Find("QRMarkers");
        CanvasObject = GameObject.Find("Canvas");
        UserObjects = GameObject.Find("UserObjects");
        
        //Find toggles for visibility
        VisibilityMenuObject = GameObject.Find("Visibility_Editor");
        Toggle VisibilityMenuToggle = VisibilityMenuObject.GetComponent<Toggle>();
        
        //Find toggles for menu
        MenuButtonObject = GameObject.Find("Menu_Toggle");
        Toggle MenuToggle = MenuButtonObject.GetComponent<Toggle>();

        //Find toggle for step search
        ElementSearchToggleObject = GameObject.Find("ElementSearchToggle");
        Toggle ElementSearchToggle = ElementSearchToggleObject.GetComponent<Toggle>();

        //Find toggle for editor... Slightly different then the other two because it is off on start.
        EditorToggleObject = MenuButtonObject.FindObject("Editor_Toggle");
        Toggle EditorToggle = EditorToggleObject.GetComponent<Toggle>();

        //Find Background Images for Toggles
        VisualzierBackground = VisibilityMenuObject.FindObject("Background_Visualizer");
        MenuBackground = MenuButtonObject.FindObject("Background_Menu");
        EditorBackground = EditorToggleObject.FindObject("Background_Editor");

        //Find AR Camera gameObject
        arCamera = GameObject.Find("XR Origin").FindObject("Camera Offset").FindObject("Main Camera").GetComponent<Camera>();

        //Find the Raycast manager in the script in order to use it to acquire data
        rayManager = FindObjectOfType<ARRaycastManager>();

        //Find Constant UI Pannel
        ConstantUIPanelObjects = GameObject.Find("ConstantUIPanel");

        /////////////////////////////////////////// Primary UI Buttons ////////////////////////////////////////////
       
        //Find Object, Button, and Add Listener for OnClick method
        NextGeometryButtonObject = GameObject.Find("Next_Geometry");
        Button NextGeometryButton = NextGeometryButtonObject.GetComponent<Button>();
        NextGeometryButton.onClick.AddListener(NextStepButton);;

        //Find Object, Button, and Add Listener for OnClick method
        PreviousGeometryButtonObject = GameObject.Find("Previous_Geometry");
        Button PreviousGeometryButton = PreviousGeometryButtonObject.GetComponent<Button>();
        PreviousGeometryButton.onClick.AddListener(PreviousStepButton);;

        //Find Object, Slider, and Add Listener for OnClick method
        PreviewGeometrySliderObject = GameObject.Find("GeometrySlider");
        PreviewGeometrySlider = PreviewGeometrySliderObject.GetComponent<Slider>();
        PreviewGeometrySlider.onValueChanged.AddListener(PreviewGeometrySliderSetVisibilty);;

        //Find Object, Button, and Add Listener for OnClick method
        IsBuiltPanelObjects = ConstantUIPanelObjects.FindObject("IsBuiltPanel"); 
        IsBuiltButtonObject = IsBuiltPanelObjects.FindObject("IsBuiltButton");
        IsbuiltButtonImage = IsBuiltButtonObject.FindObject("Image");
        IsBuiltButton = IsBuiltButtonObject.GetComponent<Button>();
        IsBuiltButton.onClick.AddListener(() => ModifyStepBuildStatus(CurrentStep));;

        //Find Step Search Objects
        ElementSearchObjects = ConstantUIPanelObjects.FindObject("ElementSearchObjects");
        ElementSearchInputField = ElementSearchObjects.FindObject("ElementSearchInputField").GetComponent<TMP_InputField>();;
        SearchElementButtonObject = ElementSearchObjects.FindObject("SearchForElementButton");
        Button ElementSearchButton = SearchElementButtonObject.GetComponent<Button>();
        ElementSearchButton.onClick.AddListener(SearchElementButton);;
        SearchedElement = "None";

        //Find Text Objects
        CurrentStepTextObject = GameObject.Find("Current_Index_Text");
        CurrentStepText = CurrentStepTextObject.GetComponent<TMPro.TMP_Text>();

        GameObject LastBuiltIndexTextObject = GameObject.Find("LastBuiltIndex_Text");
        LastBuiltIndexText = LastBuiltIndexTextObject.GetComponent<TMPro.TMP_Text>();

        GameObject CurrentPriorityTextObject = GameObject.Find("CurrentPriority_Text");
        CurrentPriorityText = CurrentPriorityTextObject.GetComponent<TMPro.TMP_Text>();

        EditorSelectedTextObject = CanvasObject.FindObject("Editor_Selected_Text");
        EditorSelectedText = EditorSelectedTextObject.GetComponent<TMPro.TMP_Text>();

        //Find Warning messages
        GameObject MessagesParent = CanvasObject.FindObject("Messages");
        PriorityIncompleteWarningMessageObject = MessagesParent.FindObject("PriorityIncompleteWarningMessage");
        PriorityIncorrectWarningMessageObject = MessagesParent.FindObject("PriorityIncorrectWarningMessage");


        /////////////////////////////////////////// Visualizer Menu Buttons ////////////////////////////////////////////
        //Find Object, Button, and Add Listener for OnClick method
        PreviewBuilderButtonObject = VisibilityMenuObject.FindObject("Preview_Builder");
        Button PreviewBuilderButton = PreviewBuilderButtonObject.GetComponent<Button>();
        PreviewBuilderButton.onClick.AddListener(ChangeVisualizationMode);;

        //Find Object, Button, and Add Listener for OnClick method
        IDButtonObject = VisibilityMenuObject.FindObject("ID_Button");
        Button IDButton = IDButtonObject.GetComponent<Button>();
        IDButton.onClick.AddListener(IDTextButton);;

        //Find Robot toggle and Objects
        RobotToggleObject = VisibilityMenuObject.FindObject("Robot_Button");
        Toggle RobotToggle = RobotToggleObject.GetComponent<Toggle>();
        RobotVisulizationControlObjects = ConstantUIPanelObjects.FindObject("RobotVisulizationControlObjects");

        //Find Robot toggle and Objects
        PriorityViewerToggleObject = VisibilityMenuObject.FindObject("PriorityViewer");
        Toggle PriorityViewerToggle = PriorityViewerToggleObject.GetComponent<Toggle>();

        //Find Object Lengths Toggle and Objects
        ObjectLengthsToggleObject = VisibilityMenuObject.FindObject("ObjectLength_Button");
        Toggle ObjectLengthsToggle = ObjectLengthsToggleObject.GetComponent<Toggle>();
        ObjectLengthsUIPanelObjects = CanvasObject.FindObject("ObjectLengthsPanel");
        ObjectLengthsText = ObjectLengthsUIPanelObjects.FindObject("LengthsText").GetComponent<TMP_Text>();
        ObjectLengthsTags = GameObject.Find("ObjectLengthsTags");
       
        /////////////////////////////////////////// Menu Buttons ////////////////////////////////////////////
        
        //Find Object, Button, and Add Listener for OnClick method
        InfoToggleObject = MenuButtonObject.FindObject("Info_Button");
        Toggle InfoToggle = InfoToggleObject.GetComponent<Toggle>();

        //Find Object, Button, and Add Listener for OnClick method
        ReloadButtonObject = MenuButtonObject.FindObject("Reload_Button");
        Button ReloadButton = ReloadButtonObject.GetComponent<Button>();
        ReloadButton.onClick.AddListener(ReloadApplication);;

        //Find Object, Button, and Add Listener for OnClick method
        CommunicationToggleObject = MenuButtonObject.FindObject("Communication_Button");
        Toggle CommunicationToggle = CommunicationToggleObject.GetComponent<Toggle>();

        //Find Object, Button, and Add Listener for OnClick method
        BuilderEditorButtonObject = EditorToggleObject.FindObject("Builder_Editor_Button");
        Button BuilderEditorButton = BuilderEditorButtonObject.GetComponent<Button>();
        BuilderEditorButton.onClick.AddListener(TouchModifyActor);;

        //Find Object, Button, and Add Listener for OnClick method
        BuildStatusButtonObject = EditorToggleObject.FindObject("Build_Status_Editor");
        BuildStatusButton = BuildStatusButtonObject.GetComponent<Button>();
        BuildStatusButton.onClick.AddListener(TouchModifyBuildStatus);;

        //Find Panel Objects used for Info and communication
        InfoPanelObject = CanvasObject.FindObject("InfoPanel");
        CommunicationPanelObject = CanvasObject.FindObject("CommunicationPanel");

        /////////////////////////////////////////// Set Toggles ////////////////////////////////////////////
        //Add Listners for Visibility Toggle on and off.
        VisibilityMenuToggle.onValueChanged.AddListener(delegate {
        ToggleVisibilityMenu(VisibilityMenuToggle);
        });

        //Add Listners for Menu Toggle on and off.
        MenuToggle.onValueChanged.AddListener(delegate {
        ToggleMenu(MenuToggle);
        });

        //Add Listners for Editor Toggle on and off.
        EditorToggle.onValueChanged.AddListener(delegate {
        ToggleEditor(EditorToggle);
        });

        //Add Listners for Step Search Toggle on and off.
        ElementSearchToggle.onValueChanged.AddListener(delegate {
        ToggleElementSearch(ElementSearchToggle);
        });

        //Add Listners for Info Toggle on and off.
        InfoToggle.onValueChanged.AddListener(delegate {
        ToggleInfo(InfoToggle);
        });

        //Add Listners for Info Toggle on and off.
        CommunicationToggle.onValueChanged.AddListener(delegate {
        ToggleCommunication(CommunicationToggle);
        });

        //Add Listners for Object Lengths.
        ObjectLengthsToggle.onValueChanged.AddListener(delegate {
        ToggleObjectLengths(ObjectLengthsToggle);
        });

        //Add Listners for Object Lengths.
        RobotToggle.onValueChanged.AddListener(delegate {
        ToggleRobot(RobotToggle);
        });

        //Add Listners for Priority Viewer Toggle.
        PriorityViewerToggle.onValueChanged.AddListener(delegate {
        TogglePriority(PriorityViewerToggle);
        });

    }
    public void ToggleVisibilityMenu(Toggle toggle)
    {
        if (VisualzierBackground != null && PreviewBuilderButtonObject != null && RobotToggleObject != null && ObjectLengthsToggleObject != null && IDButtonObject != null && PriorityViewerToggleObject != null)
        {    
            if (toggle.isOn)
            {             
                //Set Visibility of buttons
                VisualzierBackground.SetActive(true);
                PreviewBuilderButtonObject.SetActive(true);
                RobotToggleObject.SetActive(true);
                ObjectLengthsToggleObject.SetActive(true);
                IDButtonObject.SetActive(true);
                PriorityViewerToggleObject.SetActive(true);

                //Set color of toggle
                SetUIObjectColor(VisibilityMenuObject, Yellow);

            }
            else
            {
                //Set Visibility of buttons
                VisualzierBackground.SetActive(false);
                PreviewBuilderButtonObject.SetActive(false);
                RobotToggleObject.SetActive(false);
                ObjectLengthsToggleObject.SetActive(false);
                IDButtonObject.SetActive(false);
                PriorityViewerToggleObject.SetActive(false);

                //Set color of toggle
                SetUIObjectColor(VisibilityMenuObject, White);
            }
        }
        else
        {
            Debug.LogWarning("Could not find one of the buttons in the Visualizer Menu.");
        }   
    }
    public void ToggleMenu(Toggle toggle)
    {
        if (MenuBackground != null && InfoToggleObject != null && ReloadButtonObject != null && CommunicationToggleObject != null && EditorToggleObject != null)
        {    
            if (toggle.isOn)
            {             
                //Set Visibility of buttons
                MenuBackground.SetActive(true);
                InfoToggleObject.SetActive(true);
                ReloadButtonObject.SetActive(true);
                CommunicationToggleObject.SetActive(true);
                EditorToggleObject.SetActive(true);

                //Set color of toggle
                SetUIObjectColor(MenuButtonObject, Yellow);

            }
            else
            {
                //Control Menu Internal Toggles
                if(EditorToggleObject.GetComponent<Toggle>().isOn){
                    EditorToggleObject.GetComponent<Toggle>().isOn = false;
                }
                if(InfoToggleObject.GetComponent<Toggle>().isOn){
                    InfoToggleObject.GetComponent<Toggle>().isOn = false;
                }
                if(CommunicationToggleObject.GetComponent<Toggle>().isOn){
                    CommunicationToggleObject.GetComponent<Toggle>().isOn = false;
                }
                //Set Visibility of buttons
                MenuBackground.SetActive(false);
                InfoToggleObject.SetActive(false);
                ReloadButtonObject.SetActive(false);
                CommunicationToggleObject.SetActive(false);
                EditorToggleObject.SetActive(false);

                //Set color of toggle
                SetUIObjectColor(MenuButtonObject, White);
            }
        }
        else
        {
            Debug.LogWarning("Could not find one of the buttons in the Menu.");
        }   
    }
    private void SetUIObjectColor(GameObject Button, Color color)
    {
        Button.GetComponent<Image>().color = color;
    }
    public void print_string_on_click(string Text)
    {
        Debug.Log(Text);
    }

    /////////////////////////////////////// Primary UI Functions //////////////////////////////////////////////
    public void NextStepButton()
    {
        //Press Next Element Button
        Debug.Log("Next Element Button Pressed");
        
        //If current step is not null and smaller then the length of the list
        if(CurrentStep != null)
        {
            int CurrentStepInt = Convert.ToInt16(CurrentStep);

            if(CurrentStepInt < databaseManager.BuildingPlanDataItem.steps.Count - 1)
            {
                //Set current element as this step + 1
                SetCurrentStep((CurrentStepInt + 1).ToString());
            }  
        }

    }
    public void SetCurrentStep(string key)
    {
        //If the current step is not null find the previous current step and color it bulit or unbuilt.
        if(CurrentStep != null)
        {
            //Find Arrow and Destroy it
            instantiateObjects.RemoveObjects($"{CurrentStep} Arrow");
            
            //Find Gameobject Associated with that step
            GameObject previousStepElement = Elements.FindObject(CurrentStep);

            if(previousStepElement != null)
            {
                //Color previous object based on Visulization Mode
                instantiateObjects.ObjectColorandTouchEvaluater(instantiateObjects.visulizationController.VisulizationMode, instantiateObjects.visulizationController.TouchMode, databaseManager.BuildingPlanDataItem.steps[CurrentStep], previousStepElement.FindObject("Geometry"));
            }
        }
                
        //Set current element name
        CurrentStep = key;

        //Find the step in the dictoinary
        Step step = databaseManager.BuildingPlanDataItem.steps[key];

        //Find Gameobject Associated with that step
        GameObject element = Elements.FindObject(key);

        if(element != null)
        {
            //Color it Human or Robot Built
            instantiateObjects.ColorHumanOrRobot(step.data.actor, step.data.is_built, element.FindObject("Geometry"));
            Debug.Log($"Current Step is {CurrentStep}");
        }
        
        //Update Onscreen Text
        CurrentStepText.text = CurrentStep;
        
        //Instantiate an arrow at the current step
        instantiateObjects.ArrowInstantiator(element, CurrentStep);

        //Write Current Step to the database under my device name
        UserCurrentInfo userCurrentInfo = new UserCurrentInfo();
        userCurrentInfo.currentStep = CurrentStep;
        userCurrentInfo.timeStamp = (System.DateTime.UtcNow.ToLocalTime().ToString("dd-MM-yyyy HH:mm:ss"));

        //Add to the UserCurrentStepDict
        databaseManager.UserCurrentStepDict[SystemInfo.deviceUniqueIdentifier] = userCurrentInfo;

        //Push Current key to the firebase
        databaseManager.PushStringData(databaseManager.dbrefernece_usersCurrentSteps.Child(SystemInfo.deviceUniqueIdentifier), JsonConvert.SerializeObject(userCurrentInfo));

        //Update Lengths if Object Lengths Toggle is on
        if(ObjectLengthsToggleObject.GetComponent<Toggle>().isOn)
        {
            CalculateandSetLengthPositions(CurrentStep);
        }

        //Update Preview Geometry the visulization is remapped correctly
        PreviewGeometrySliderSetVisibilty(PreviewGeometrySlider.value);
        
        //Update Is Built Button
        IsBuiltButtonGraphicsControler(step.data.is_built);
    }
    public void ToggleElementSearch(Toggle toggle)
    {
        if (ElementSearchObjects != null && IsBuiltPanelObjects != null)
        {    
            if (toggle.isOn)
            {             
                //Set Visibility of buttons
                IsBuiltPanelObjects.SetActive(false);
                ElementSearchObjects.SetActive(true);
                
                //Set color of toggle
                SetUIObjectColor(ElementSearchToggleObject, Yellow);

            }
            else
            {
                //If searched step is not null color it built or unbuilt
                if(SearchedElement != null)
                {
                    GameObject searchedElement = Elements.FindObject(SearchedElement);

                    if(searchedElement != null)
                    {
                        //Color Previous one if it is not null
                        instantiateObjects.ColorBuiltOrUnbuilt(databaseManager.BuildingPlanDataItem.steps[SearchedElement].data.is_built, searchedElement.FindObject("Geometry"));
                    }
                }
                
                //Set Visibility of buttons
                ElementSearchObjects.SetActive(false);
                IsBuiltPanelObjects.SetActive(true);

                //Set color of toggle
                SetUIObjectColor(ElementSearchToggleObject, White);

                //Update Is Built Button
                Step currentStep = databaseManager.BuildingPlanDataItem.steps[CurrentStep];
                IsBuiltButtonGraphicsControler(currentStep.data.is_built);
            }
        }
        else
        {
            Debug.LogWarning("Could not find Step Search Objects or Is Built Panel.");
        }  
    }
    public void SearchElementButton()
    {
        //Search for step button clicked
        Debug.Log("Search for Step Button Pressed");

        //GameObject that the search is looking for
        GameObject UserSearchedElement = null;
        
        //If current step is not null and greater then Zero add subtract 1
        if(ElementSearchInputField.text != null)
        {
            UserSearchedElement = Elements.FindObject(ElementSearchInputField.text);
        }

        //If the element was found color the previous object built or unbuilt and replace the variable
        if(UserSearchedElement != null)
        {
            //Color Previous one if it is not null
            if(SearchedElement != "None")
            {
                //Find Gameobject Associated with that step
                GameObject previousElement = Elements.FindObject(SearchedElementStep);
                Step PreviousStep = databaseManager.BuildingPlanDataItem.steps[SearchedElementStep];

                if(previousElement != null)
                {
                    //Color based on current mode
                    instantiateObjects.ObjectColorandTouchEvaluater(instantiateObjects.visulizationController.VisulizationMode, instantiateObjects.visulizationController.TouchMode, PreviousStep, previousElement.FindObject("Geometry"));

                    //if it is equal to current step color it human or robot
                    if(SearchedElementStep == CurrentStep)
                    {
                        instantiateObjects.ColorHumanOrRobot(PreviousStep.data.actor, PreviousStep.data.is_built, previousElement.FindObject("Geometry"));
                    }
                }
            }
            
            //Set Searched Step
            SearchedElement = ElementSearchInputField.text;
            
            //iterate through building plan to find new searched element.
            for (int i =0 ; i < databaseManager.BuildingPlanDataItem.steps.Count; i++)
            {
                //Set data items
                Step step = databaseManager.BuildingPlanDataItem.steps[i.ToString()];

                //Check if step element id is equal to the previous searched one
                if(step.data.element_ids[0] == SearchedElement)
                {
                    //Find Gameobject Associated with that step
                    GameObject NewStepElement = Elements.FindObject(i.ToString());
                    SearchedElementStep = i.ToString();

                    if(NewStepElement != null)
                    {
                        //Color red for selection color
                        Renderer searchedObjectRenderer = NewStepElement.FindObject("Geometry").GetComponent<Renderer>();
                        searchedObjectRenderer.material = instantiateObjects.SearchedObjectMaterial;

                        //TODO: ARROW TO STEP                    

                        break;
                    }
                }
            }


        }
        else
        {
            //TODO: Trigger ON Screen Text that says the step was not found.

            Debug.Log("Trigger Onscreen text about finding the element.");
        }     
    }
    public void PreviousStepButton()
    {
        //Previous element button clicked
        Debug.Log("Previous Element Button Pressed");

        //If current step is not null and greater then Zero add subtract 1
        if(CurrentStep != null)
        {
          int CurrentStepInt = Convert.ToInt16(CurrentStep);

            if(CurrentStepInt > 0)
            {
                //Set current element as this step - 1
                SetCurrentStep((CurrentStepInt - 1).ToString());
            }  
        }       

    }
    public void PreviewGeometrySliderSetVisibilty(float value)
    {
        if (CurrentStep != null)
        {
            Debug.Log("You are changing the Preview Geometry");
            int min = Convert.ToInt16(CurrentStep);
            float SliderValue = value;
            int ElementsTotal = databaseManager.BuildingPlanDataItem.steps.Count;
            float SliderMax = 1; //Input Slider Max Value == 1
            float SliderMin = 0; // Input Slider Min Value == 0
                
            float SliderRemaped = GameObjectExtensions.Remap(SliderValue, SliderMin, SliderMax, min, ElementsTotal); 

            foreach(int index in Enumerable.Range(min, ElementsTotal))
            {
                string elementName = index.ToString();
                int InstanceNumber = Convert.ToInt16(elementName);

                GameObject element = Elements.FindObject(elementName);
                
                if (element != null)
                {
                    if (InstanceNumber > SliderRemaped)
                    {
                        element.SetActive(false); 
                    }
                    else
                    {
                        element.SetActive(true);
                    }
                }
            }
        }
    }
    public void IsBuiltButtonGraphicsControler(bool builtStatus)
    {
        if (IsBuiltPanelObjects.activeSelf)
        {
            if (builtStatus)
            {
                IsbuiltButtonImage.SetActive(true);
                IsBuiltButtonObject.GetComponent<Image>().color = TranspGrey;
            }
            else
            {
                IsbuiltButtonImage.SetActive(false);
                IsBuiltButtonObject.GetComponent<Image>().color = TranspWhite;
            }
        }
    }
    
    //Priority checker is set up for temporary priority tree.
    /*
    
    TODO: I think I have to put CurrentPriority on the DB
    Reason 1: If I only go off of last built element then it means that in order to move to the next PR I have to 
    build the first one in order to update CurrentPriority. Which is actually incorrect because if I unbuild it my LastBuiltIndex
    iterates through backwards to the first built element which would put me back on the previous priority.

    Reason 2: If I make it so the last built index doesn't do that then my last built index on the DB will be incorrect because I
    build would update LBI to build the one, and then not when I unbuild it.
    
    */
    public bool PriorityChecker(Step step)
    {
        //Check if the current priority is null
        if(CurrentPriority == null)
        {
            //Print out the priority tree as a check
            Debug.Log("THIS IS THE PRIORITY TREE DICTIONARY (PCheck0): " + JsonConvert.SerializeObject(databaseManager.PriorityTreeDict));

            Debug.LogError("Current Priority is null.");
            
            //Return False to not push data.
            return false;
        }
        
        //Check if they are the same. If they are return true //TODO: THIS ONLY WORKS BECAUSE WE PUSH EVERYTHING.
        else if (CurrentPriority == step.data.priority.ToString())
        {
            Debug.Log($"Priority Check: Current Priority is equal to step priority. Pushing data");
            
            //Print out the priority tree as a check
            Debug.Log("THIS IS THE PRIORITY TREE DICTIONARY (PCheck1): " + JsonConvert.SerializeObject(databaseManager.PriorityTreeDict));

            //Return True to push all the data to the database
            return true;
        }

        //Else if the current priority is higher then the step priority loop through all the elements in priority above and unbuild them. This allows you to go back in priority.
        else if (Convert.ToInt16(CurrentPriority) > step.data.priority)
        {
            Debug.Log($"Priority Check: Current Priority is higher then the step priority. Unbuilding elements.");
        
            //Loop from steps priority to the highest priority group and unbuild all elements above this one.
            for(int i = Convert.ToInt16(step.data.priority) + 1; i < databaseManager.PriorityTreeDict.Count; i++)
            {

                //Find the current priority in the dictionary for iteration
                List<string> PriorityDataItem = databaseManager.PriorityTreeDict[i.ToString()];

                Debug.Log($"FIX ME: I should be entering here with step {step.data.element_ids[0]}");

                //Iterate through the Priority tree dictionary to unbuild elements of a higher priority.
                foreach(string key in PriorityDataItem)
                {
                    //Find the step in the dictoinary
                    Step stepToUnbuild = databaseManager.BuildingPlanDataItem.steps[key];

                    //If step is built unbuild it
                    if(stepToUnbuild.data.is_built)
                    {                        
                        //Unbuild the element
                        stepToUnbuild.data.is_built = false;

                        //Update color and touch depending on what is on.
                        instantiateObjects.ObjectColorandTouchEvaluater(instantiateObjects.visulizationController.VisulizationMode, instantiateObjects.visulizationController.TouchMode, stepToUnbuild, Elements.FindObject(key).FindObject("Geometry"));                        
                    }
                }
            }

            //Return True to push all the data to the database
            return true;
    
        }
        //The priority is higher. Check if all elements in Current Priority are built.
        else
        {
            //if the elements priority is more then 1 greater then current priority return false and signal on screen warning.
            if(step.data.priority != Convert.ToInt16(CurrentPriority) + 1)
            {
                Debug.Log($"Priority Check: Current Priority is more then 1 greater then the step priority. Incorrect Priority");

                SignalOnScreenPriorityIncorrectWarning(step.data.priority.ToString(), CurrentPriority);

                //Return False to not push data.
                return false;
            }

            //This is the Priority that we want.
            else
            {   
                //New empty list to store unbuilt elements
                List<string> UnbuiltElements = new List<string>();
                
                //Find the current priority in the dictionary for iteration
                List<string> PriorityDataItem = databaseManager.PriorityTreeDict[CurrentPriority];

                //Iterate through the Priority tree dictionary to check the elements and if the priority is complete
                foreach(string element in PriorityDataItem)
                {
                    //Find the step in the dictoinary
                    Step stepToCheck = databaseManager.BuildingPlanDataItem.steps[element];

                    //Check if the element is built
                    if(!stepToCheck.data.is_built)
                    {
                        UnbuiltElements.Add(element);
                    }
                }

                //TODO: IF I put current element on the database it the only thing is this has to return a false here and update CurrentPriority and UIGraphics Controler.
                //If the list is empty return true because all elements of that priority are built
                if(UnbuiltElements.Count == 0)
                {
                    Debug.Log($"Priority Check: Current Priority is complete. Pushing data");
                    
                    //Print out the priority tree as a check
                    Debug.Log("THIS IS THE PRIORITY TREE DICTIONARY (PCheck2): " + JsonConvert.SerializeObject(databaseManager.PriorityTreeDict));
                    
                    //Return True to push all data to the database.
                    return true;
                }
                
                //If the list is not empty return false because not all elements of that priority are built and signal on screen warning.
                else
                {
                    Debug.Log($"Priority Check: Current Priority is not complete. Incomplete Priority");
                    
                    //Print out the priority tree as a check
                    Debug.Log("THIS IS THE PRIORITY TREE DICTIONARY (PCheck3): " + JsonConvert.SerializeObject(databaseManager.PriorityTreeDict));

                    SignalOnScreenPriorityIncompleteWarning(UnbuiltElements, CurrentPriority);
                    
                    //Return False to not push data.
                    return false;
                }
            }
        }
    }
    public void ModifyStepBuildStatus(string key)
    {
        Debug.Log($"Modifying Build Status of: {key}");

        //Find the step in the dictoinary
        Step step = databaseManager.BuildingPlanDataItem.steps[key];

        //Check if priority is correct.
        if (PriorityChecker(step))
        {
            //Change Build Status //TODO: WHAT DO I DO IF I AM UNBUILDING 0... I think most logical would be to set current priority to 0 do nothing with LastBuiltIndex.
            if(step.data.is_built)
            {
                //Change Build Status
                step.data.is_built = false;

                //Convert my key to an int
                int StepInt = Convert.ToInt16(key);

                //Iterate through steps backwards to find the last built step that is closest to my current step
                for(int i = StepInt; i >= 0; i--)
                {
                    //Check if step int is 0 and then set current priority to 0 and last built index do nothing
                    if(StepInt == 0)
                    {
                        //Set Current Priority but leave last built index alone.
                        SetCurrentPriority("0");

                        //exit if condition above this one
                        break;   
                    }

                    if(databaseManager.BuildingPlanDataItem.steps[i.ToString()].data.is_built)
                    {
                        //Change LastBuiltIndex
                        databaseManager.BuildingPlanDataItem.LastBuiltIndex = i.ToString();
                        SetLastBuiltText(i.ToString());

                        //Set Current Priority
                        SetCurrentPriority(i.ToString());

                        break;
                    }
                }

            }
            else
            {
                //Change Build Status
                step.data.is_built = true;

                //Change LastBuiltIndex
                databaseManager.BuildingPlanDataItem.LastBuiltIndex = key;
                SetLastBuiltText(key);

                //Set Current Priority
                SetCurrentPriority(key);
            }

            //Update color
            instantiateObjects.ColorHumanOrRobot(step.data.actor, step.data.is_built, Elements.FindObject(key).FindObject("Geometry"));
            
            //If it is current element update UI graphics
            if(key == CurrentStep)
            {    
                //Update Is Built Button
                IsBuiltButtonGraphicsControler(step.data.is_built);
            }

            //Push Data to the database
            databaseManager.PushAllDataBuildingPlan(key);
        }

    }
    public void SetLastBuiltText(string key)
    {
        //Set Last Built Text
        LastBuiltIndexText.text = $"Last Built Step : {key}";
    }
    public void SetCurrentPriority(string Key)
    {     
        //Find the step in the dictoinary
        Step step = databaseManager.BuildingPlanDataItem.steps[Key];
        string Priority = step.data.priority.ToString();
        
        //Current Priority Text current Priority Items
        CurrentPriority = Priority;

        //Set On Screen Text
        CurrentPriorityText.text = $"Current Priority : {Priority}";
        
        //Print setting current priority
        Debug.Log($"Setting Current Priority from Key : {Key} to Priority: {step.data.priority.ToString()} ");
    }
    private void SignalOnScreenPriorityIncompleteWarning(List<string> UnbuiltElements, string currentPriority)
    {
        
        Debug.Log($"Priority Warning: Signal On Screen Message for Incomplete Priority.");

        //Find text component for on screen message
        TMP_Text messageComponent = PriorityIncompleteWarningMessageObject.FindObject("PriorityIncompleteText").GetComponent<TMP_Text>();

        //Define message for the onscreen text
        string message = $"WARNING: This element cannot build because the following elements from Current Priority {currentPriority} are not built: {string.Join(", ", UnbuiltElements)}";
        
        if(messageComponent != null && message != null && PriorityIncompleteWarningMessageObject != null)
        {
            //Signal On Screen Message with Acknowledge Button
            SignalOnScreenMessageWithButton(PriorityIncompleteWarningMessageObject, messageComponent, message);
        }
        else
        {
            Debug.LogWarning("Priority Message: Could not find message object or message component.");
        }

    }
    private void SignalOnScreenPriorityIncorrectWarning(string elementsPriority, string currentPriority)
    {
        
        Debug.Log($"Priority Warning: Signal On Screen Message for Incorrect Priority.");

        //Find text component for on screen message
        TMP_Text messageComponent = PriorityIncorrectWarningMessageObject.FindObject("PriorityIncorrectText").GetComponent<TMP_Text>();

        //Define message for the onscreen text
        string message = $"WARNING: This elements priority is incorrect. It is priority {elementsPriority} and next priority to build is {Convert.ToInt16(currentPriority) + 1}";
        
        if(messageComponent != null && message != null && PriorityIncorrectWarningMessageObject != null)
        {
            //Signal On Screen Message with Acknowledge Button
            SignalOnScreenMessageWithButton(PriorityIncorrectWarningMessageObject, messageComponent, message);
        }
        else
        {
            Debug.LogWarning("Priority Message: Could not find message object or message component.");
        }

    }
    public void SignalOnScreenMessageWithButton(GameObject messageGameObject, TMP_Text messageComponent, string message)
    {
        if (messageGameObject != null && messageComponent != null)
        {
            //Set Text
            messageComponent.text = message;

            //Set Object Active
            messageGameObject.SetActive(true);

            //Get Acknowledge button from the child of this panel
            GameObject AcknowledgeButton = messageGameObject.FindObject("AcknowledgeButton");

            //Check if this item already has a listner or not.
            if (AcknowledgeButton.GetComponent<Button>().onClick.GetPersistentEventCount() == 0)
            {
                //Add Listner for Acknowledge Button
                AcknowledgeButton.GetComponent<Button>().onClick.AddListener(() => messageGameObject.SetActive(false));
            }
            else
            {
                Debug.LogWarning("ACKNOWLEDGE BUTTON SHOULD ALREADY HAVE A LISTNER THAT SETS IT TO FALSE.");
            }

        }
        else
        {
            Debug.LogWarning($"Message: Could not find message object or message component inside of GameObject {messageGameObject.name}.");
        }  
    }

    ////////////////////////////////////// Visualizer Menu Buttons ////////////////////////////////////////////
    public void ChangeVisualizationMode()
    {
        Debug.Log("Builder View Button Pressed");

        // Check the current mode and toggle it
        if (instantiateObjects.visulizationController.VisulizationMode != VisulizationMode.ActorView)
        {
            // If current mode is BuiltUnbuilt, switch to ActorView
            instantiateObjects.visulizationController.VisulizationMode = VisulizationMode.ActorView;
         
            instantiateObjects.ApplyColorBasedOnActor();

            // Color the button if it is on
            SetUIObjectColor(PreviewBuilderButtonObject, Yellow);

        }
        else if(instantiateObjects.visulizationController.VisulizationMode != VisulizationMode.BuiltUnbuilt)
        {
            // If current mode is not BuiltUnbuilt switch to BuiltUnbuilt
            instantiateObjects.visulizationController.VisulizationMode = VisulizationMode.BuiltUnbuilt;

            instantiateObjects.ApplyColorBasedOnBuildState();

            // Color the button if it is on
            SetUIObjectColor(PreviewBuilderButtonObject, White);
        }
        else
        {
            Debug.LogWarning("Error: Visulization Mode does not exist.");
        }
    }
    public void IDTextButton()
    {
        Debug.Log("ID Text Button Pressed");

        if (instantiateObjects != null && instantiateObjects.Elements != null)
        {
            // Update the visibility state
            instantiateObjects.visulizationController.TagsMode = !instantiateObjects.visulizationController.TagsMode;

            foreach (Transform child in instantiateObjects.Elements.transform)
            {
                // Toggle Text Object
                Transform textChild = child.Find(child.name + " Text");
                if (textChild != null)
                {
                    textChild.gameObject.SetActive(instantiateObjects.visulizationController.TagsMode);
                }
                // Toggle Circle Image Object
                Transform circleImageChild = child.Find(child.name + "IdxImage");
                if (circleImageChild != null)
                {
                    circleImageChild.gameObject.SetActive(instantiateObjects.visulizationController.TagsMode);
                }
            }

            // Color the button if it is on
            if (instantiateObjects.visulizationController.TagsMode)
            {
                SetUIObjectColor(IDButtonObject, Yellow);
            }
            else
            {
                SetUIObjectColor(IDButtonObject, White);
            }
        }
        else
        {
            Debug.LogError("InstantiateObjects script or Elements object not set.");
        }
    }
    public void ToggleObjectLengths(Toggle toggle)
    {
        Debug.Log("Object Lengths Toggle Pressed");

        if (ObjectLengthsUIPanelObjects != null && ObjectLengthsText != null && ObjectLengthsTags != null)
        {    
            if (toggle.isOn)
            {             
                //Set Visibility of buttons
                ObjectLengthsUIPanelObjects.SetActive(true);
                ObjectLengthsTags.FindObject("P1Tag").SetActive(true);
                ObjectLengthsTags.FindObject("P2Tag").SetActive(true);

                //Function to calculate distances to the ground and show them
                if (CurrentStep != null)
                {    
                    CalculateandSetLengthPositions(CurrentStep);
                }
                else
                {
                    Debug.LogWarning("Current Step is null.");
                }

                //Set color of toggle
                SetUIObjectColor(ObjectLengthsToggleObject, Yellow);

            }
            else
            {
                //Set Visibility of buttons
                ObjectLengthsUIPanelObjects.SetActive(false);
                ObjectLengthsTags.FindObject("P1Tag").SetActive(false);
                ObjectLengthsTags.FindObject("P2Tag").SetActive(false);

                //Set color of toggle
                SetUIObjectColor(ObjectLengthsToggleObject, White);
            }
        }
        else
        {
            Debug.LogWarning("Could not find Object Lengths Objects.");
        }
        
    }
    public void CalculateandSetLengthPositions(string key)
    {
        //Find Gameobject Associated with that step
        GameObject element = Elements.FindObject(key);
        Step step = databaseManager.BuildingPlanDataItem.steps[key];

        //Find gameobject center
        Vector3 center = element.FindObject("Geometry").GetComponent<Renderer>().bounds.center;

        //Find length from assembly dictionary
        float length = databaseManager.DataItemDict[step.data.element_ids[0]].attributes.length;

        //Calculate position of P1 and P2 // TODO: CHECK THIS... Other Geometries 
        Vector3 P1Position = center + element.transform.right * (length / 2)* -1;
        Vector3 P2Position = center + element.transform.right * (length / 2);

        //Set Positions of P1 and P2
        ObjectLengthsTags.FindObject("P1Tag").transform.position = P1Position;
        ObjectLengthsTags.FindObject("P2Tag").transform.position = P2Position;

        //Adjust P1 and P2 to be the same xz position as the elements for distance calculation
        Vector3 ElementsPosition = Elements.transform.position;
        Vector3 P1Adjusted = new Vector3(ElementsPosition.x, P1Position.y, ElementsPosition.z);
        Vector3 P2Adjusted = new Vector3(ElementsPosition.x, P2Position.y, ElementsPosition.z);

        //Get distance between position of P1, P2 and position of elements
        float P1distance = Vector3.Distance(P1Adjusted, ElementsPosition);
        float P2distance = Vector3.Distance(P2Adjusted, ElementsPosition);

        //Update Distance Text
        ObjectLengthsText.text = $"P1 | {(float)Math.Round(P1distance, 2)} P2 | {(float)Math.Round(P2distance, 2)}";
    }
    public void ToggleRobot(Toggle toggle)
    {
        Debug.Log("Robot Toggle Pressed");

        if(toggle.isOn && RobotVisulizationControlObjects != null)
        {
            RobotVisulizationControlObjects.SetActive(true);
            SetUIObjectColor(RobotToggleObject, Yellow);
        }
        else
        {
            RobotVisulizationControlObjects.SetActive(false);
            SetUIObjectColor(RobotToggleObject, White);
        }
    }
    public void TogglePriority(Toggle toggle)
    {
        Debug.Log("Priority Toggle Pressed");

        if(toggle.isOn && PriorityViewerToggleObject != null)
        {
            //Color Elements Based on Priority
            instantiateObjects.ApplyColorBasedOnPriority(CurrentPriority);

            //Set UI Color
            SetUIObjectColor(PriorityViewerToggleObject, Yellow);
        }
        else
        {
            Debug.Log("Priority Toggle Off");
            Debug.Log($"Toggle state is {toggle.isOn}");
            if(PriorityViewerToggleObject == null)
            {
            Debug.Log($"Priority Viewer Toggle Object is null {PriorityViewerToggleObject}");
            }
            
            //Color Elements by visulization mode


            instantiateObjects.ApplyColorBasedOnBuildState();

            //Set UI Color
            SetUIObjectColor(PriorityViewerToggleObject, White);
        }
    }
    
    ////////////////////////////////////////// Menu Buttons ///////////////////////////////////////////////////
    private void ToggleInfo(Toggle toggle)
    {
        if(InfoPanelObject != null)
        {
            if (toggle.isOn)
            {             
                //Check if communication toggle is on and if it is turn it off
                if(CommunicationToggleObject.GetComponent<Toggle>().isOn)
                {
                    CommunicationToggleObject.GetComponent<Toggle>().isOn = false;
                }
                
                //Set Visibility of Information panel
                InfoPanelObject.SetActive(true);

                //Set color of toggle
                SetUIObjectColor(InfoToggleObject, Yellow);

            }
            else
            {
                //Set Visibility of Information panel
                InfoPanelObject.SetActive(false);

                //Set color of toggle
                SetUIObjectColor(InfoToggleObject, White);
            }
        }
        else
        {
            Debug.LogWarning("Could not find Info Panel.");
        }
    }
    private void ToggleCommunication(Toggle toggle)
    {
        if(CommunicationPanelObject != null)
        {
            if (toggle.isOn)
            {             
                //Check if info toggle is on and if it is turn it off
                if(InfoToggleObject.GetComponent<Toggle>().isOn)
                {
                    InfoToggleObject.GetComponent<Toggle>().isOn = false;
                }
                
                //Set Visibility of Information panel
                CommunicationPanelObject.SetActive(true);

                //Set color of toggle
                SetUIObjectColor(CommunicationToggleObject, Yellow);

            }
            else
            {
                //Set Visibility of Information panel
                CommunicationPanelObject.SetActive(false);

                //Set color of toggle
                SetUIObjectColor(CommunicationToggleObject, White);
            }
        }
        else
        {
            Debug.LogWarning("Could not find Communication Panel.");
        }
    }
    private void ReloadApplication()
    {
        Debug.Log("Reload Button Pressed");
        
        //Remove listners - This is important to not add multiple listners in the application
        databaseManager.RemoveListners();

        //Clear all elements in the scene
        if (Elements.transform.childCount > 0)
        {
            foreach (Transform child in Elements.transform)
            {
                Destroy(child.gameObject);
            }
        }

        //Put all QR Markers back to Origin Location
        if (QRMarkers.transform.childCount > 0)
        {        
            foreach (Transform child in QRMarkers.transform)
            {
                child.transform.position = Vector3.zero;
                child.transform.rotation = Quaternion.identity;
            }
        }

        //Clear all User Current Step Objects if there are some
        if (UserObjects.transform.childCount > 0)
        {
            foreach (Transform child in UserObjects.transform)
            {
                Destroy(child.gameObject);
            }
        }        

        //Clear all dictionaries
        databaseManager.BuildingPlanDataItem.steps.Clear();
        databaseManager.DataItemDict.Clear();
        databaseManager.QRCodeDataDict.Clear();
        databaseManager.UserCurrentStepDict.Clear();
        databaseManager.PriorityTreeDict.Clear();

        //Fetch settings data again
        databaseManager.FetchSettingsData(eventManager.settings_reference);

    }
    public void ToggleEditor(Toggle toggle)
    {
        if (EditorBackground != null && BuilderEditorButtonObject != null && BuildStatusButtonObject != null)
        {    
            if (toggle.isOn)
            {             
                //Set Visibility of buttons
                EditorBackground.SetActive(true);
                BuilderEditorButtonObject.SetActive(true);
                BuildStatusButtonObject.SetActive(true);

                //Set visibility of on screen text
                CurrentStepTextObject.SetActive(false);
                EditorSelectedTextObject.SetActive(true);

                //Control Selectable objects in scene
                ColliderControler();

                //Update mode so we know to search for touch input
                TouchSearchModeController(1);
                
                //Set color of toggle
                SetUIObjectColor(EditorToggleObject, Yellow);

            }
            else
            {
                //Set Visibility of buttons
                EditorBackground.SetActive(false);
                BuilderEditorButtonObject.SetActive(false);
                BuildStatusButtonObject.SetActive(false);

                //Set visibility of on screen text
                EditorSelectedTextObject.SetActive(false);
                CurrentStepTextObject.SetActive(true);

                //Update mode so we are no longer searching for touch
                TouchSearchModeController(0);

                //Color Elements by build status
                instantiateObjects.ApplyColorBasedOnBuildState();

                //Set color of toggle
                SetUIObjectColor(EditorToggleObject, White);
            }
        }
        else
        {
            Debug.LogWarning("Could not find one of the buttons in the Editor Menu.");
        }  
    }

    ////////////////////////////////////////// Editor Buttons /////////////////////////////////////////////////
    
    //TODO: This needs an input of the mode type that you want to set.
    public void TouchSearchModeController(int modetype)
    {
        if (modetype == 1)
        {        
            //Set Visulization Mode
            instantiateObjects.visulizationController.TouchMode = TouchMode.ElementEditSelection;

            Debug.Log ("***TouchMode: ELEMENT EDIT MODE***");
        }

        else
        {
            //Set Visulization Mode
            instantiateObjects.visulizationController.TouchMode = TouchMode.None; // setting back to original mode

            //Destroy active bounding box
            DestroyBoundingBoxFixElementColor();
            activeGameObject = null;
            Debug.Log ("***TouchMode: NONE***");
        }
    }
    private void TouchSearchControler()
    {
        if (instantiateObjects.visulizationController.TouchMode == TouchMode.ElementEditSelection)
        {
            SearchInput();
        }
    }
    private void ColliderControler()
    {
        //Set data items
        Step Currentstep = databaseManager.BuildingPlanDataItem.steps[CurrentStep];
    
        for (int i =0 ; i < databaseManager.BuildingPlanDataItem.steps.Count; i++)
        {
            //Set data items
            Step step = databaseManager.BuildingPlanDataItem.steps[i.ToString()];

            //Find Gameobject Collider and Renderer
            GameObject element = Elements.FindObject(i.ToString()).FindObject("Geometry");
            Collider ElementCollider = element.FindObject("Geometry").GetComponent<Collider>();
            Renderer ElementRenderer = element.FindObject("Geometry").GetComponent<Renderer>();

            if(ElementCollider != null)
            {
                //Find the first unbuilt element
                if(step.data.priority == Currentstep.data.priority)
                {
                    //Set Collider to true
                    ElementCollider.enabled = true;

                }
                else
                {
                    //Set Collider to false
                    ElementCollider.enabled = false;

                    //Update Renderer for Objects that cannot be selected
                    ElementRenderer.material = instantiateObjects.LockedObjectMaterial;
                }
            }
            
        }
    }
    private GameObject SelectedObject(GameObject activeGameObject = null)
    {
        if (Application.isEditor)
        {
            Ray ray = arCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitObject;

            if (Physics.Raycast(ray, out hitObject))
            {
                if (hitObject.collider.tag != "plane")
                {
                    activeGameObject = hitObject.collider.gameObject;
                    Debug.Log(activeGameObject);
                }
            }
        }
        else
        {
            Touch touch = Input.GetTouch(0);
            Debug.Log("TOUCH: Your touch sir :)" + Input.touchCount);
            Debug.Log("TOUCH: Your Phase sir :)" + (touch.phase == TouchPhase.Ended));

            if (Input.touchCount == 1 && touch.phase == TouchPhase.Ended)
            {
                List<ARRaycastHit> hits = new List<ARRaycastHit>();
                Debug.Log ("TOUCH: YOU HITS SIR " + hits);
                Debug.Log ("TOUCH: YOU HITS Count SIR " + hits.Count);
                rayManager.Raycast(touch.position, hits);

                //TODO: THE PROBLEM IS HERE... HITS IS ALWAYS 0
                if (hits.Count > 0)
                {
                    Ray ray = arCamera.ScreenPointToRay(touch.position);
                    RaycastHit hitObject;
                    Debug.Log ("TOUCH: Your hits count is greater then 0" + hits.Count);

                    if (Physics.Raycast(ray, out hitObject))
                    {
                        if (hitObject.collider.tag != "plane")
                        {
                            Debug.Log("TOUCH: I HIT SOMETHING ");
                            activeGameObject = hitObject.collider.gameObject;
                            Debug.Log(activeGameObject);
                        }
                    }
                }
            }
        }

        return activeGameObject;
    }
    private void SearchInput()
    {
        if (Application.isEditor)
        {   
            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }

                if (instantiateObjects.visulizationController.TouchMode == TouchMode.ElementEditSelection) // EDIT MODE
                {
                    Debug.Log("*** ELEMENT SELECTION MODE : Editor ***");
                    EditMode();
                }

                else
                {
                    Debug.Log("Press a button to initialize a mode");
                }
            }
        }
        else
        {
            SearchTouch();
        }
    }
    private void SearchTouch()
    {
        if (Input.touchCount > 0) //if there is an input..           
        {
            if (PhysicRayCastBlockedByUi(Input.GetTouch(0).position))
            {
                if (instantiateObjects.visulizationController.TouchMode == TouchMode.ElementEditSelection) //EDIT MODE
                {
                    Debug.Log("*** ELEMENT SELECTION MODE: Touch ***");
                    EditMode();                     
                }

                else
                {
                    Debug.Log("Press a button to initialize a mode");
                }
            }
        }
    }
    private bool PhysicRayCastBlockedByUi(Vector2 touchPosition)
    {
        //creating a Boolean value if we are touching a button
        if (GameObjectExtensions.IsPointerOverUIObject(touchPosition))
        {
            Debug.Log("YOU CANT FIND YOUR OBJECT INSIDE PHYSICRAYCAST...");
            return false;
        }
        return true;
    }
    private void EditMode()
    {
        activeGameObject = SelectedObject();
        
        if (Input.touchCount == 1) //try and locate the selected object only when we click, not on update
        {
            activeGameObject = SelectedObject();
        }

        if (activeGameObject != null)
        {
            //Set On screen text object to the correct name
            EditorSelectedText.text = activeGameObject.transform.parent.name;
            
            //Set temporary object as the active game object
            temporaryObject = activeGameObject;

            //String name of the parent object to find step element in the dictionary
            string activeGameObjectParentname = activeGameObject.transform.parent.name;

            Debug.Log($"Active Game Object Priority: {databaseManager.BuildingPlanDataItem.steps[activeGameObjectParentname].data.priority}");

            //If active game objects step priority is higher then current step priority then set builder button to inactive else its active
            if(databaseManager.BuildingPlanDataItem.steps[activeGameObjectParentname].data.priority > Convert.ToInt16(CurrentPriority))
            {
                Debug.Log("Priority is higher then current priority. Setting button to inactive.");
                BuildStatusButton.interactable = false;
            }
            else
            {
                BuildStatusButton.interactable = true;
            }
            
            //Color the object based on human or robot
            instantiateObjects.ColorHumanOrRobot(databaseManager.BuildingPlanDataItem.steps[activeGameObjectParentname].data.actor, databaseManager.BuildingPlanDataItem.steps[activeGameObjectParentname].data.is_built, activeGameObject);
            
            //Add Bounding Box for Object
            addBoundingBox(temporaryObject);
        }

        else
        {
            Debug.Log("ACTIVE GAME OBJECT IS NULL");
            if (GameObject.Find("BoundingArea") != null)
            {
                DestroyBoundingBoxFixElementColor();
            }
        }
    }
    private void addBoundingBox(GameObject gameObj)
    {
        DestroyBoundingBoxFixElementColor(); //destroy the bounding box

        //create a primitive cube
        GameObject boundingArea = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        //add name
        boundingArea.name = "BoundingArea";

        //add material
        boundingArea.GetComponent<Renderer>().material = instantiateObjects.HumanUnbuiltMaterial;


        //use collider to find object bounds
        Collider collider = gameObj.GetComponent<Collider>();
        Vector3 center = collider.bounds.center;
        float radius = collider.bounds.extents.magnitude;

        // destroy any Collider component
        if (boundingArea.GetComponent<Rigidbody>() != null)
        {
            Destroy(boundingArea.GetComponent<BoxCollider>());
        }
        if (boundingArea.GetComponent<Collider>() != null)
        {
            Destroy(boundingArea.GetComponent<BoxCollider>());
        }

        // scale the bounding box according to the bounds values
        boundingArea.transform.localScale = new Vector3(radius * 0.5f, radius * 0.5f, radius * 0.5f);
        boundingArea.transform.localPosition = center;
        boundingArea.transform.rotation = gameObj.transform.rotation;

        //Set parent as the step Item from the dictionary
        var stepParent = gameObj.transform.parent;

        boundingArea.transform.SetParent(stepParent);
    }
    private void DestroyBoundingBoxFixElementColor()
    {

        //destroy the previous bounding box

        if (GameObject.Find("BoundingArea") != null)
        {
            GameObject Box = GameObject.Find("BoundingArea");
            var element = Box.transform.parent;

            if (element != null)
            {
                if (CurrentStep != null)
                {
                    if (element.name != CurrentStep)
                    {

                        Step step = databaseManager.BuildingPlanDataItem.steps[element.name];
                        
                        instantiateObjects.ColorBuiltOrUnbuilt(step.data.is_built, element.gameObject.FindObject("Geometry"));                          
                        
                    }
                }

            }

            Destroy(GameObject.Find("BoundingArea"));
        }

    }
    public void ModifyStepActor(string key)
    {
        Debug.Log($"Modifying Actor of: {key}");

        //Find the step in the dictoinary
        Step step = databaseManager.BuildingPlanDataItem.steps[key];

        //Change Build Status
        if(step.data.actor == "HUMAN")
        {
            //Change Builder
            step.data.actor = "ROBOT";
        }
        else
        {
            //Change Builder
            step.data.actor = "HUMAN";
        }

        //Update color
        instantiateObjects.ColorHumanOrRobot(step.data.actor, step.data.is_built, Elements.FindObject(key).FindObject("Geometry"));
        

        //Push Data to the database
        databaseManager.PushAllDataBuildingPlan(key);
    }
    private void TouchModifyBuildStatus()
    {
        Debug.Log("Build Status Button Pressed");
        
        if (activeGameObject != null)
        {
            ModifyStepBuildStatus(activeGameObject.transform.parent.name);
        }
    }
    private void TouchModifyActor()
    {
        Debug.Log("Actor Modifier Button Pressed");
        
        if (activeGameObject != null)
        {
            ModifyStepActor(activeGameObject.transform.parent.name);
        }
    }

}

