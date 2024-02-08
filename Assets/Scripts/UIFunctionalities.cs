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
using MQTTDataCompasXR;
using Unity.IO.LowLevel.Unsafe;
using Unity.VisualScripting;


public class UIFunctionalities : MonoBehaviour
{
    //Other Scripts for inuse objects
    public DatabaseManager databaseManager;
    public InstantiateObjects instantiateObjects;
    public Eventmanager eventManager;
    public MqttTrajectoryReceiver mqttTrajectoryManager;
    
    //Toggle GameObjects
    private GameObject VisibilityMenuObject;
    private GameObject MenuButtonObject;
    private GameObject EditorToggleObject;
    private GameObject ElementSearchToggleObject;
    private GameObject InfoToggleObject;
    public GameObject CommunicationToggleObject;
    
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
    public GameObject IsbuiltPriorityLockedImage;
    public Slider PreviewGeometrySlider;
    private TMP_InputField ElementSearchInputField;
    private GameObject ElementSearchObjects;
    private GameObject SearchElementButtonObject;
    private GameObject PriorityIncompleteWarningMessageObject;
    private GameObject PriorityIncorrectWarningMessageObject;
    public GameObject MQTTFailedToConnectMessageObject;
    public GameObject MQTTConnectionLostMessageObject;
    public GameObject ErrorDownloadingObjectMessageObject;

    //Visualizer Menu Toggle Objects
    private GameObject VisualzierBackground;
    private GameObject PreviewBuilderButtonObject;
    public GameObject IDToggleObject;
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

    //Communication Specific Objects
    private TMP_InputField MqttBrokerInputField;
    private TMP_InputField MqttPortInputField;
    private GameObject MqttUpdateConnectionMessage;
    public GameObject MqttConnectButtonObject;
    private TMP_InputField RosHostInputField;
    private TMP_InputField RosPortInputField;
    private GameObject RosUpdateConnectionMessage;

    //Trajectory Review UI Controls
    public GameObject ReviewTrajectoryObjects;
    public GameObject RequestTrajectoryButtonObject;
    public GameObject ApproveTrajectoryButtonObject;
    public GameObject RejectTrajectoryButtonObject;
    public GameObject TrajectoryReviewSliderObject;
    public Slider TrajectoryReviewSlider;
    public GameObject ExecuteTrajectoryButtonObject;

    
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
    public string CurrentStep = null;
    public string SearchedElement = "None";
    public string SearchedElementStepID;
    
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
        /////////////////////////////////////////// Initial Elements ////////////////////////////////////////////
        //Find Other Scripts
        databaseManager = GameObject.Find("DatabaseManager").GetComponent<DatabaseManager>();
        instantiateObjects = GameObject.Find("Instantiate").GetComponent<InstantiateObjects>();
        eventManager = GameObject.Find("EventManager").GetComponent<Eventmanager>();
        mqttTrajectoryManager = GameObject.Find("MQTTTrajectoryManager").GetComponent<MqttTrajectoryReceiver>();

        //Find Specific GameObjects
        Elements = GameObject.Find("Elements");
        QRMarkers = GameObject.Find("QRMarkers");
        CanvasObject = GameObject.Find("Canvas");
        UserObjects = GameObject.Find("UserObjects");     

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
        IsbuiltPriorityLockedImage = IsBuiltButtonObject.FindObject("PriorityLockedImage");
        IsBuiltButton = IsBuiltButtonObject.GetComponent<Button>();
        IsBuiltButton.onClick.AddListener(() => ModifyStepBuildStatus(CurrentStep));;

        //Find toggle for element search
        ElementSearchToggleObject = GameObject.Find("ElementSearchToggle");
        Toggle ElementSearchToggle = ElementSearchToggleObject.GetComponent<Toggle>();
        //Add Listners for Step Search Toggle on and off.
        ElementSearchToggle.onValueChanged.AddListener(delegate {
        ToggleElementSearch(ElementSearchToggle);
        });
        
        //Find Element Search Button Objects
        ElementSearchObjects = ConstantUIPanelObjects.FindObject("ElementSearchObjects");
        ElementSearchInputField = ElementSearchObjects.FindObject("ElementSearchInputField").GetComponent<TMP_InputField>();;
        SearchElementButtonObject = ElementSearchObjects.FindObject("SearchForElementButton");
        Button ElementSearchButton = SearchElementButtonObject.GetComponent<Button>();
        ElementSearchButton.onClick.AddListener(SearchElementButton);;
        SearchedElement = "None";

        //Find toggles for menu
        MenuButtonObject = GameObject.Find("Menu_Toggle");
        Toggle MenuToggle = MenuButtonObject.GetComponent<Toggle>();
        //Add Listners for Menu Toggle on and off.
        MenuToggle.onValueChanged.AddListener(delegate {
        ToggleMenu(MenuToggle);
        });

        //Find toggles for visibility
        VisibilityMenuObject = GameObject.Find("Visibility_Editor");
        Toggle VisibilityMenuToggle = VisibilityMenuObject.GetComponent<Toggle>();
        //Add Listners for Visibility Toggle on and off.
        VisibilityMenuToggle.onValueChanged.AddListener(delegate {
        ToggleVisibilityMenu(VisibilityMenuToggle);
        });

        //Find Text Objects
        CurrentStepTextObject = GameObject.Find("Current_Index_Text");
        CurrentStepText = CurrentStepTextObject.GetComponent<TMPro.TMP_Text>();

        GameObject LastBuiltIndexTextObject = GameObject.Find("LastBuiltIndex_Text");
        LastBuiltIndexText = LastBuiltIndexTextObject.GetComponent<TMPro.TMP_Text>();

        GameObject CurrentPriorityTextObject = GameObject.Find("CurrentPriority_Text");
        CurrentPriorityText = CurrentPriorityTextObject.GetComponent<TMPro.TMP_Text>();

        EditorSelectedTextObject = CanvasObject.FindObject("Editor_Selected_Text");
        EditorSelectedText = EditorSelectedTextObject.GetComponent<TMPro.TMP_Text>();
        
        //Find Background Images for Toggles
        VisualzierBackground = VisibilityMenuObject.FindObject("Background_Visualizer");
        MenuBackground = MenuButtonObject.FindObject("Background_Menu");

        //Find OnScreeen Warning messages
        GameObject MessagesParent = CanvasObject.FindObject("OnScreenMessages");
        PriorityIncompleteWarningMessageObject = MessagesParent.FindObject("PriorityIncompleteWarningMessage");
        PriorityIncorrectWarningMessageObject = MessagesParent.FindObject("PriorityIncorrectWarningMessage");
        MQTTFailedToConnectMessageObject = MessagesParent.FindObject("MQTTConnectionFailedMessage");
        MQTTConnectionLostMessageObject = MessagesParent.FindObject("MQTTConnectionLostMessage");
        ErrorDownloadingObjectMessageObject = MessagesParent.FindObject("ObjectFailedToDownloadMessage");

        /////////////////////////////////////////// Visualizer Menu Buttons ////////////////////////////////////////////
        //Find Object, Button, and Add Listener for OnClick method
        PreviewBuilderButtonObject = VisibilityMenuObject.FindObject("Preview_Builder");
        Button PreviewBuilderButton = PreviewBuilderButtonObject.GetComponent<Button>();
        PreviewBuilderButton.onClick.AddListener(ChangeVisualizationMode);;

        //Find Object, Button, and Add Listener for OnClick method
        IDToggleObject = VisibilityMenuObject.FindObject("ID_Toggle");
        Toggle IDButton = IDToggleObject.GetComponent<Toggle>();
        //ID Toggle on and off.
        IDButton.onValueChanged.AddListener(delegate {
        ToggleID(IDButton);
        });

        //Find Robot toggle and Objects
        RobotToggleObject = VisibilityMenuObject.FindObject("Robot_Button");
        Toggle RobotToggle = RobotToggleObject.GetComponent<Toggle>();
        //Add Listners for Object Lengths.
        RobotToggle.onValueChanged.AddListener(delegate {
        ToggleRobot(RobotToggle);
        });

        //Find Robot toggle and Objects
        PriorityViewerToggleObject = VisibilityMenuObject.FindObject("PriorityViewer");
        Toggle PriorityViewerToggle = PriorityViewerToggleObject.GetComponent<Toggle>();
        //Add Listners for Priority Viewer Toggle.
        PriorityViewerToggle.onValueChanged.AddListener(delegate {
        TogglePriority(PriorityViewerToggle);
        });

        //Find Object Lengths Toggle and Objects
        ObjectLengthsToggleObject = VisibilityMenuObject.FindObject("ObjectLength_Button");
        Toggle ObjectLengthsToggle = ObjectLengthsToggleObject.GetComponent<Toggle>();
        ObjectLengthsUIPanelObjects = CanvasObject.FindObject("ObjectLengthsPanel");
        ObjectLengthsText = ObjectLengthsUIPanelObjects.FindObject("LengthsText").GetComponent<TMP_Text>();
        ObjectLengthsTags = GameObject.Find("ObjectLengthsTags");
        //Add Listners for Object Lengths.
        ObjectLengthsToggle.onValueChanged.AddListener(delegate {
        ToggleObjectLengths(ObjectLengthsToggle);
        });

        /////////////////////////////////////////// Menu Buttons //////////////////////////////////////////////////////
        
        //Find Info Toggle, and Add Listener for OnValueChanged method
        InfoToggleObject = MenuButtonObject.FindObject("Info_Button");
        Toggle InfoToggle = InfoToggleObject.GetComponent<Toggle>();
        //Add Listners for Info Toggle on and off.
        InfoToggle.onValueChanged.AddListener(delegate {
        ToggleInfo(InfoToggle);
        });

        //Find Object, Button, and Add Listener for OnClick method
        ReloadButtonObject = MenuButtonObject.FindObject("Reload_Button");
        Button ReloadButton = ReloadButtonObject.GetComponent<Button>();
        ReloadButton.onClick.AddListener(ReloadApplication);;

        //Find communication toggle objects
        CommunicationToggleObject = MenuButtonObject.FindObject("Communication_Button");
        Toggle CommunicationToggle = CommunicationToggleObject.GetComponent<Toggle>();
        //Add Listners for Info Toggle on and off.
        CommunicationToggle.onValueChanged.AddListener(delegate {
        ToggleCommunication(CommunicationToggle);
        });

        //Find toggle for editor.
        EditorToggleObject = MenuButtonObject.FindObject("Editor_Toggle");
        Toggle EditorToggle = EditorToggleObject.GetComponent<Toggle>();
        //Add Listners for Editor Toggle on and off.
        EditorToggle.onValueChanged.AddListener(delegate {
        ToggleEditor(EditorToggle);
        });
        
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

        //Find Background Images for Toggles
        EditorBackground = EditorToggleObject.FindObject("Background_Editor");

        /////////////////////////////////////////// Communication Buttons ////////////////////////////////////////////

        //Find Pannel Objects used for connecting to a different broker and ROS Connection Set up
        MqttBrokerInputField = CommunicationPanelObject.FindObject("MqttBrokerInputField").GetComponent<TMP_InputField>();;
        MqttPortInputField = CommunicationPanelObject.FindObject("MqttPortInputField").GetComponent<TMP_InputField>();;
        MqttUpdateConnectionMessage = CommunicationPanelObject.FindObject("UpdateInputsMQTTReconnectMessage");
        MqttConnectButtonObject = CommunicationPanelObject.FindObject("MqttConnectButton");
        MqttConnectButtonObject.GetComponent<Button>().onClick.AddListener(UpdateMqttConnectionFromUserInputs);;

        RosHostInputField = CommunicationPanelObject.FindObject("ROSHostInputField").GetComponent<TMP_InputField>();;
        RosPortInputField = CommunicationPanelObject.FindObject("ROSPortInputField").GetComponent<TMP_InputField>();;
        RosUpdateConnectionMessage = CommunicationPanelObject.FindObject("UpdateInputsMQTTReconnectMessage");
        Button RosConnectButton = CommunicationPanelObject.FindObject("ROSConnectButton").GetComponent<Button>();
        RosConnectButton.onClick.AddListener(() => print_string_on_click("ROS CONNECT BUTTONPRESSED"));;

        //Find Control Objects and set up events
        GameObject TrajectoryControlObjects = GameObject.Find("TrajectoryReviewUIControls");
        ReviewTrajectoryObjects = TrajectoryControlObjects.FindObject("ReviewTrajectoryControls");

        //Find Object, request button and add event listner for on click method
        RequestTrajectoryButtonObject = TrajectoryControlObjects.FindObject("RequestTrajectoryButton");
        Button RequestTrajectoryButton = RequestTrajectoryButtonObject.GetComponent<Button>();
        RequestTrajectoryButton.onClick.AddListener(RequestTrajectoryButtonMethod);;
        // RequestTrajectoryButton.onClick.AddListener(() => mqttTrajectoryManager.PublishToTopic(mqttTrajectoryManager.compasXRTopics.publishers.getTrajectoryRequestTopic, new GetTrajectoryRequest(CurrentStep).GetData()));;
    
        //Find object, approve button and add event listner for on click method
        ApproveTrajectoryButtonObject = ReviewTrajectoryObjects.FindObject("ApproveTrajectoryButton");
        Button ApproveTrajectoryButton = ApproveTrajectoryButtonObject.GetComponent<Button>();
        ApproveTrajectoryButton.onClick.AddListener(ApproveTrajectoryButtonMethod);;

        //Find object, reject button and add event listner for on click method
        RejectTrajectoryButtonObject = ReviewTrajectoryObjects.FindObject("RejectTrajectoryButton");
        Button RejectTrajectoryButton = RejectTrajectoryButtonObject.GetComponent<Button>();
        RejectTrajectoryButton.onClick.AddListener(RejectTrajectoryButtonMethod);;

        //Find slider for trajectory review and add event listner for on value changed method
        TrajectoryReviewSliderObject = ReviewTrajectoryObjects.FindObject("TrajectoryReviewSlider");
        TrajectoryReviewSlider = TrajectoryReviewSliderObject.GetComponent<Slider>();
        TrajectoryReviewSlider.onValueChanged.AddListener(TrajectorySliderReviewMethod);;

        //Find Object, Execute button and add event listner for on click method
        ExecuteTrajectoryButtonObject = TrajectoryControlObjects.FindObject("ExecuteTrajectoryButton");
        Button ExecuteTrajectoryButton = ExecuteTrajectoryButtonObject.GetComponent<Button>();
        ExecuteTrajectoryButton.onClick.AddListener(ExecuteTrajectoryButtonMethod);;

    }
    public void ToggleVisibilityMenu(Toggle toggle)
    {
        if (VisualzierBackground != null && PreviewBuilderButtonObject != null && RobotToggleObject != null && ObjectLengthsToggleObject != null && IDToggleObject != null && PriorityViewerToggleObject != null)
        {    
            if (toggle.isOn)
            {             
                //Set Visibility of buttons
                VisualzierBackground.SetActive(true);
                PreviewBuilderButtonObject.SetActive(true);
                RobotToggleObject.SetActive(true);
                ObjectLengthsToggleObject.SetActive(true);
                IDToggleObject.SetActive(true);
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
                IDToggleObject.SetActive(false);
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
    public void SetUIObjectColor(GameObject Button, Color color)
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
                Step PreviousStep = databaseManager.BuildingPlanDataItem.steps[CurrentStep];
                string elementID = PreviousStep.data.element_ids[0];

                instantiateObjects.ObjectColorandTouchEvaluater(instantiateObjects.visulizationController.VisulizationMode, instantiateObjects.visulizationController.TouchMode, PreviousStep, previousStepElement.FindObject(elementID + " Geometry"));

                //If Priority Viewer toggle is on then color the add additional color based on priority: //TODO: IF I CHANGE PV then it checks text.
                if (PriorityViewerToggleObject.GetComponent<Toggle>().isOn)
                {
                    instantiateObjects.ColorObjectByPriority(databaseManager.CurrentPriority, PreviousStep.data.priority.ToString(), CurrentStep, previousStepElement.FindObject(elementID + " Geometry"));
                }
            }
        }

        if(CurrentStep == null)
        {
            Debug.Log("CURRENT STEP IS NULL");
        }
        else
        {
            Debug.Log($"CURRENT STEP IS {CurrentStep.ToString()}");
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
            instantiateObjects.ColorHumanOrRobot(step.data.actor, step.data.is_built, element.FindObject(step.data.element_ids[0] + " Geometry"));
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
            instantiateObjects.CalculateandSetLengthPositions(CurrentStep);
        }

        //Update Trajectory Request interactibility based on new current step
        if(RobotToggleObject.GetComponent<Toggle>().isOn)
        {
            //Set interaction based on current step.
            SetTrajectoryRequestUIFromKey(CurrentStep);
        }

        //Update Preview Geometry the visulization is remapped correctly
        PreviewGeometrySliderSetVisibilty(PreviewGeometrySlider.value);
        
        //Update Is Built Button
        IsBuiltButtonGraphicsControler(step.data.is_built, step.data.priority);
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
                    GameObject searchedElement = Elements.FindObject(SearchedElement + " Geometry");

                    if(searchedElement != null)
                    {
                        //TODO: ADD IF STATEMENT FOR IF IT IS CURRENT STEP or over arching color and colider controler.

                        //Color Previous one if it is not null
                        instantiateObjects.ColorBuiltOrUnbuilt(databaseManager.BuildingPlanDataItem.steps[SearchedElementStepID].data.is_built, searchedElement);
                    }
                }
                
                //Set Visibility of buttons
                ElementSearchObjects.SetActive(false);
                IsBuiltPanelObjects.SetActive(true);

                //Set color of toggle
                SetUIObjectColor(ElementSearchToggleObject, White);

                //Update Is Built Button
                Step currentStep = databaseManager.BuildingPlanDataItem.steps[CurrentStep];
                IsBuiltButtonGraphicsControler(currentStep.data.is_built, currentStep.data.priority);
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
            UserSearchedElement = Elements.FindObject(ElementSearchInputField.text + " Geometry");
        }

        //If the element was found color the previous object built or unbuilt and replace the variable
        if(UserSearchedElement != null)
        {
            //Color Previous one if it is not null
            if(SearchedElement != "None")
            {
                //Find Gameobject Associated with that step
                GameObject previousElement = Elements.FindObject(SearchedElementStepID);
                Step PreviousStep = databaseManager.BuildingPlanDataItem.steps[SearchedElementStepID];

                if(previousElement != null)
                {
                    //Color based on current mode
                    instantiateObjects.ObjectColorandTouchEvaluater(instantiateObjects.visulizationController.VisulizationMode, instantiateObjects.visulizationController.TouchMode, PreviousStep, previousElement.FindObject(PreviousStep.data.element_ids[0] + " Geometry"));

                    //if it is equal to current step color it human or robot
                    if(SearchedElementStepID == CurrentStep)
                    {
                        instantiateObjects.ColorHumanOrRobot(PreviousStep.data.actor, PreviousStep.data.is_built, previousElement.FindObject(PreviousStep.data.element_ids[0] + " Geometry"));
                    }
                }
            }
            
            //Set Searched Step
            SearchedElement = ElementSearchInputField.text;

            //Color it by the searched object color
            UserSearchedElement.GetComponent<MeshRenderer>().material = instantiateObjects.SearchedObjectMaterial;

            //Find the parent name so I can set that as my searched element stepID
            SearchedElementStepID = UserSearchedElement.transform.parent.name;
            Debug.Log("Searched Element Step ID: " + SearchedElementStepID);

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
    public void IsBuiltButtonGraphicsControler(bool builtStatus, int stepPriority)
    {
        if (IsBuiltPanelObjects.activeSelf)
        {
            //Check if current priority is null
            if(databaseManager.CurrentPriority != null)
            {    
                //Set priority locked image based on priority comparison
                if(stepPriority > Convert.ToInt16(databaseManager.CurrentPriority))
                {
                    IsbuiltPriorityLockedImage.SetActive(true);
                    IsBuiltButtonObject.GetComponent<Image>().color = TranspWhite;
                }
                else
                {
                    IsbuiltPriorityLockedImage.SetActive(false);
                    IsBuiltButtonObject.GetComponent<Image>().color = TranspGrey;
                }
            }

            //Set is built button graphis based on build status
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
    public bool LocalPriorityChecker(Step step)
    {
        //Check if the current priority is null
        if(databaseManager.CurrentPriority == null)
        {
            //Print out the priority tree as a check
            Debug.LogError("Current Priority is null.");
            
            //Return false to not push data.
            return false;
        }
        
        //Check if they are the same. If they are return true //TODO: THIS ONLY WORKS BECAUSE WE PUSH EVERYTHING.
        else if (databaseManager.CurrentPriority == step.data.priority.ToString())
        {
            Debug.Log($"Priority Check: Current Priority is equal to step priority. Pushing data");

            //Return true to push all the data to the database
            return true;
        }

        //Else if the current priority is higher then the step priority loop through all the elements in priority above and unbuild them. This allows you to go back in priority.
        else if (Convert.ToInt16(databaseManager.CurrentPriority) > step.data.priority)
        {
            Debug.Log($"Priority Check: Current Priority is higher then the step priority. Unbuilding elements.");
        
            //Loop from steps priority to the highest priority group and unbuild all elements above this one.
            for(int i = Convert.ToInt16(step.data.priority) + 1; i < databaseManager.PriorityTreeDict.Count; i++)
            {

                //Find the current priority in the dictionary for iteration
                List<string> PriorityDataItem = databaseManager.PriorityTreeDict[i.ToString()];

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
                    }

                    //Update color and touch depending on what is on.
                    instantiateObjects.ObjectColorandTouchEvaluater(instantiateObjects.visulizationController.VisulizationMode, instantiateObjects.visulizationController.TouchMode, stepToUnbuild, Elements.FindObject(key).FindObject(stepToUnbuild.data.element_ids[0] + " Geometry"));
                
                }
            }

            //Return true to push data to the database
            return true;
    
        }
        //The priority is higher. Check if all elements in Current Priority are built.
        else
        {
            //if the elements priority is more then 1 greater then current priority return false and signal on screen warning.
            if(step.data.priority != Convert.ToInt16(databaseManager.CurrentPriority) + 1)
            {
                Debug.Log($"Priority Check: Current Priority is more then 1 greater then the step priority. Incorrect Priority");

                SignalOnScreenPriorityIncorrectWarning(step.data.priority.ToString(), databaseManager.CurrentPriority);

                //Return false to not push data.
                return false;
            }

            //This is the next Priority.
            else
            {   
                //New empty list to store unbuilt elements
                List<string> UnbuiltElements = new List<string>();
                
                //Find the current priority in the dictionary for iteration
                List<string> PriorityDataItem = databaseManager.PriorityTreeDict[databaseManager.CurrentPriority];

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

                //If the list is empty return false because all elements of that priority are built, and we want to move on to the next priority but not write info.
                if(UnbuiltElements.Count == 0)
                {
                    Debug.Log($"Priority Check: Current Priority is complete. Unlocking Next Priority.");

                    //Set Current Priority
                    SetCurrentPriority(step.data.priority.ToString());

                    //If my CurrentStep Priority is the same as New Current Priority then update UI graphics
                    if(databaseManager.BuildingPlanDataItem.steps[CurrentStep].data.priority.ToString() == databaseManager.CurrentPriority)
                    {    
                        IsBuiltButtonGraphicsControler(step.data.is_built, step.data.priority);
                    }
                    
                    //Return false, this is for the first time that an element is changed and we only want to update a priority, but not write information.
                    return false;
                }
                
                //If the list is not empty return false because not all elements of that priority are built and signal on screen warning.
                else
                {
                    //Print out the priority tree as a check
                    SignalOnScreenPriorityIncompleteWarning(UnbuiltElements, databaseManager.CurrentPriority);
                    
                    //Return true to not push data.
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
        if (LocalPriorityChecker(step))
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
                    //Find Step in the dictionary
                    Step stepToCheck = databaseManager.BuildingPlanDataItem.steps[i.ToString()];
                    
                    //Check if step int is 0 and then set current priority to 0 and last built index do nothing
                    if(StepInt == 0)
                    {
                        //Set Current Priority but leave last built index alone.
                        SetCurrentPriority(stepToCheck.data.priority.ToString());

                        //exit if condition above this one
                        break;   
                    }

                    if(stepToCheck.data.is_built)
                    {
                        //Change LastBuiltIndex
                        databaseManager.BuildingPlanDataItem.LastBuiltIndex = i.ToString();
                        SetLastBuiltText(i.ToString());

                        //Set Current Priority
                        SetCurrentPriority(stepToCheck.data.priority.ToString());

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
                SetCurrentPriority(step.data.priority.ToString());
            }

            //Update color
            instantiateObjects.ColorHumanOrRobot(step.data.actor, step.data.is_built, Elements.FindObject(key).FindObject(step.data.element_ids[0] + " Geometry"));
            
            //If it is current element update UI graphics
            if(key == CurrentStep)
            {    
                //Update Is Built Button
                IsBuiltButtonGraphicsControler(step.data.is_built, step.data.priority);
            }

            //Push Data to the database
            databaseManager.PushAllDataBuildingPlan(key);
        }
        else
        {
            Debug.Log("Priority Check: Checked Priority and not pushing Data.");
        }

    }
    public void SetLastBuiltText(string key)
    {
        //Set Last Built Text
        LastBuiltIndexText.text = $"Last Built Step : {key}";
    }
    public void SetCurrentPriority(string Priority)
    {        
        //If Priority Viewer is on and new priority is not equal to current priority update the priority viewer (only place I can do this)
        if(PriorityViewerToggleObject.GetComponent<Toggle>().isOn && databaseManager.CurrentPriority != Priority)
        {
            //Update Priority Viewer
            instantiateObjects.ApplyColorBasedOnPriority(Priority);
        }
        
        //Current Priority Text current Priority Items
        databaseManager.CurrentPriority = Priority;

        //Update Trajectory Request interactibility based on my current step after priority check if Robot Toggle is on //TODO: THIS ONLY WORKS BECAUSE I UPDATE CURRENT PRIORITY EVERY TIME I WRITE.
        if(RobotToggleObject.GetComponent<Toggle>().isOn)
        {
            //Set interaction based on current step.
            SetTrajectoryRequestUIFromKey(CurrentStep);
        }
        
        //Set On Screen Text
        CurrentPriorityText.text = $"Current Priority : {Priority}";
        
        //Print setting current priority
        Debug.Log($"Setting Current Priority to {Priority} ");
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
    public void SignalMQTTConnectionFailed()
    {
        Debug.LogWarning("MQTT: MQTT Connection Failed.");
        
        if(MQTTFailedToConnectMessageObject != null)
        {
            //Check if the Connectoin Toggle is on and if it is turn it off.
            if(CommunicationToggleObject.GetComponent<Toggle>().isOn)
            {
                CommunicationToggleObject.GetComponent<Toggle>().isOn = false;
            }
            
            //Signal On Screen Message with Acknowledge Button
            SignalOnScreenMessageWithButton(MQTTFailedToConnectMessageObject);
        }
        else
        {
            Debug.LogWarning("MQTT Message: Could not find message object or message component.");
        }
    }
    public void SignalOnScreenMessageWithButton(GameObject messageGameObject, TMP_Text messageComponent = null, string message = "None")
    {
        if (messageGameObject != null)
        {
            if(message != "None" && messageComponent != null)
            {
                //Set Text
                messageComponent.text = message;
            }

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

    /////////////////////////////////////// Communication Buttons //////////////////////////////////////////////
    public void TestPublish()
    {
        Debug.Log("Test Publish Button Pressed");

        Dictionary<string, object> testMessage = new GetTrajectoryRequest(CurrentStep).GetData();
        Debug.Log($"Test Message: {JsonConvert.SerializeObject(testMessage)}");

        //Publish to the topic
        mqttTrajectoryManager.PublishToTopic(mqttTrajectoryManager.compasXRTopics.publishers.getTrajectoryRequestTopic, testMessage);
    }
    public void UpdateMqttConnectionFromUserInputs()
    {
        //Set UI Color
        SetUIObjectColor(MqttConnectButtonObject, White);
        
        //Check inputs and if they are not null update the connection if they are null leave the default.
        string newMqttBroker = MqttBrokerInputField.text;
        if (string.IsNullOrWhiteSpace(newMqttBroker))
        {
            newMqttBroker = "broker.hivemq.com";
        }

        string newMqttPort = MqttPortInputField.text;
        if (string.IsNullOrWhiteSpace(newMqttPort))
        {
            newMqttPort = "1883";
        }

        //Check if the manual the port or broker is different then the current one.
        if (newMqttBroker != mqttTrajectoryManager.brokerAddress || Convert.ToInt32(newMqttPort) != mqttTrajectoryManager.brokerPort)
        {
            //Unsubscibe from events
            mqttTrajectoryManager.RemoveConnectionEventListners();

            //Unsubscribe from topics
            mqttTrajectoryManager.UnsubscribeFromCompasXRTopics();

            //Update Broker and Port to the user inputs
            mqttTrajectoryManager.brokerAddress = newMqttBroker;
            mqttTrajectoryManager.brokerPort = Convert.ToInt32(newMqttPort);

            //Disconnect from current broker
            mqttTrajectoryManager.DisconnectandReconnectAsyncRoutine();
        }
        else
        {
            Debug.Log("MQTT: Broker and Port are the same as the current one. Not updating connection.");
            
            //Signal Manual Input text
            MqttUpdateConnectionMessage.SetActive(true);

        }
    }
    public void TrajectoryReviewTestMethod(float value)
    {
        Debug.Log($"Trajectory Review Slider Value Changed is value {value}");
    }
    public void TrajectoryServicesUIControler(bool requestTrajectoryVisability, bool requestTrajectoryInteractable, bool trajectoryReviewVisibility, bool trajectoryReviewInteractable, bool executeTrajectoryVisability, bool executeTrajectoryInteractable)
    {
        //Set Visability and Interactable of Trajectory Request Button.
        RequestTrajectoryButtonObject.SetActive(requestTrajectoryVisability);
        RequestTrajectoryButtonObject.GetComponent<Button>().interactable = requestTrajectoryInteractable;

        //Set Visability of Trajectory Review objects and Interactable of Approval and Reject Buttons
        ReviewTrajectoryObjects.SetActive(trajectoryReviewVisibility);
        ApproveTrajectoryButtonObject.GetComponent<Button>().interactable = trajectoryReviewInteractable;
        RejectTrajectoryButtonObject.GetComponent<Button>().interactable = trajectoryReviewInteractable;

        //Set Visability and Interactable of Execute Trajectory Button.
        ExecuteTrajectoryButtonObject.SetActive(executeTrajectoryVisability);
        ExecuteTrajectoryButtonObject.GetComponent<Button>().interactable = executeTrajectoryInteractable;

        //Adjust interactibility of Robot toggle based on visibility of other services controls //TODO: SHOULD THIS PREVENT ME FROM PUSHING NEXT OR PREVIOUS ALSO? I THINK SO.
        if ( trajectoryReviewVisibility || executeTrajectoryVisability)
        {
            //if trajectory approval or exacute trajectory is visible then robot toggle is not interactable
            RobotToggleObject.GetComponent<Toggle>().interactable = false;
        }
        else if (requestTrajectoryVisability)
        {
            //If request trajectory is visaible then robot toggle is interactable
            RobotToggleObject.GetComponent<Toggle>().interactable = true;
        }
    }
    public void RequestTrajectoryButtonMethod()
    {
        Debug.Log($"Request Trajectory Button Pressed: Requesting Trajectory for Step {CurrentStep}");

        //Publish new GetTrajectoryRequest message to the GetTrajectoryRequestTopic for CurrentStep
        mqttTrajectoryManager.PublishToTopic(mqttTrajectoryManager.compasXRTopics.publishers.getTrajectoryRequestTopic, new GetTrajectoryRequest(CurrentStep).GetData());

        //Set mqttTrajectoryManager.serviceManager.PrimaryUser to true
        mqttTrajectoryManager.serviceManager.PrimaryUser = true;

        //TODO: CHECK THIS BASED ON THE SPEED OF THE MESSAGE HANDLER.
        //Make the request button not interactable to prevent sending multiple requests.. Message Handler will set it back to true if trajectory is null.
        TrajectoryServicesUIControler(true, false, false, false, false, false);
    }
    public void ApproveTrajectoryButtonMethod()
    {
        Debug.Log($"Approve Trajectory Button Pressed: Approving Trajectory for Step {CurrentStep}");
        
        //TODO: Put this here to prevent accidentally setting it if the message is too fast.
        //Make the approval and disapproval button not interactable to prevent sending multiple approvals and disapprovals.
        TrajectoryServicesUIControler(false, false, true, false, false, false);
        
        //Publish new ApproveTrajectoryMessage to the trajectory approval topic for current step
        mqttTrajectoryManager.PublishToTopic(mqttTrajectoryManager.compasXRTopics.publishers.approveTrajectoryTopic, new ApproveTrajectory(CurrentStep, mqttTrajectoryManager.serviceManager.CurrentTrajectory, 1).GetData());

        //TODO: If I am the primary User then Include TimeOut for waiting on everyone else to approve or disapprove...
        //TODO: Possibly Publish with another approval status of 3 == Cancelation.
    }
    public void RejectTrajectoryButtonMethod()
    {
        Debug.Log($"Reject Trajectory Button Pressed: Rejecting Trajectory for Step {CurrentStep}");

        //Publish new ApproveTrajectoryMessage to the trajectory approval topic for current step
        mqttTrajectoryManager.PublishToTopic(mqttTrajectoryManager.compasXRTopics.publishers.approveTrajectoryTopic, new ApproveTrajectory(CurrentStep, mqttTrajectoryManager.serviceManager.CurrentTrajectory, 0).GetData());

        //Make the approval and disapproval button not interactable to prevent sending multiple approvals and disapprovals....
        //Just a precaustion should be handles by message handler anyway. //TODO: CHECK THIS BASED ON THE SPEED OF THE MESSAGE HANDLER.
        TrajectoryServicesUIControler(false, false, true, false, false, false);
    }
    public void TrajectorySliderReviewMethod(float value)
    {
        //Check if MQTT Service Manager is not null or count is greater then 0
        if (mqttTrajectoryManager.serviceManager.CurrentTrajectory != null)
        {
            //double check that count is not 0
            if (mqttTrajectoryManager.serviceManager.CurrentTrajectory.Count > 0)
            {
                //Remap input value to the count of the trajectory
                float SliderValue = value;
                int TrajectoryConfigurationsCount = mqttTrajectoryManager.serviceManager.CurrentTrajectory.Count; 
                float SliderMax = 1; //Input Slider Max Value == 1
                float SliderMin = 0; // Input Slider Min Value == 0
                
                float SliderValueRemaped = GameObjectExtensions.Remap(SliderValue, SliderMin, SliderMax, 0, TrajectoryConfigurationsCount); 

                //Print list item at the index of the remapped value //TODO: SERILIZE CONFIGURATION TO STRING SO YOU CAN READ IT.
                Debug.Log($"Trajectory Review: Slider Value Changed is value {value} and the item is {mqttTrajectoryManager.serviceManager.CurrentTrajectory[(int)SliderValueRemaped]}"); //TODO:CHECK SLIDER REMAP

                //TODO: Color Static Robot Image based on SliderRemapedValue
            }
            else
            {
                Debug.Log("Trajectory Review: Current Trajectory Count is 0.");
            }
        }
        else
        {
            Debug.Log("Trajectory Review: Current Trajectory is null.");
        }
    }
    public void ExecuteTrajectoryButtonMethod()
    {
        Debug.Log($"Execute Trajectory Button Pressed: Executing Trajectory for Step {CurrentStep}");

        //Publish new SendTrajectoryMessage to the trajectory execution topic for current step
        mqttTrajectoryManager.PublishToTopic(mqttTrajectoryManager.compasXRTopics.publishers.sendTrajectoryTopic, new SendTrajectory(CurrentStep, mqttTrajectoryManager.serviceManager.CurrentTrajectory).GetData());

        //Make the execute button not interactable to prevent sending multiple just a precaustion, should be handled by message handler anyway.
        TrajectoryServicesUIControler(false, false, false, false, true, false); //TODO: CHECK THIS BASED ON THE SPEED OF THE MESSAGE HANDLER.

        //Publish new ApproveTrajectoryMessage for CONSENSUS APPROVAL
        mqttTrajectoryManager.PublishToTopic(mqttTrajectoryManager.compasXRTopics.publishers.approveTrajectoryTopic, new ApproveTrajectory(CurrentStep, mqttTrajectoryManager.serviceManager.CurrentTrajectory, 2).GetData());
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
    public void ToggleID(Toggle toggle)
    {
        Debug.Log("ID Toggle Pressed");

        if (toggle != null && IDToggleObject != null)
        {
            if(toggle.isOn)
            {
                IDTextControler(true);
                SetUIObjectColor(IDToggleObject, Yellow);
            }
            else
            {
                IDTextControler(false);
                SetUIObjectColor(IDToggleObject, White);
            }
        }
        else
        {
            Debug.LogWarning("Could not find ID Toggle or ID Toggle Object.");
        }
    }
    public void IDTextControler(bool Visibility)
    {
        Debug.Log("ID Text Button Pressed");

        if (instantiateObjects != null && instantiateObjects.Elements != null)
        {

            foreach (Transform child in instantiateObjects.Elements.transform)
            {
                // Toggle Text Object
                Transform textChild = child.Find(child.name + " Text");
                if (textChild != null)
                {
                    textChild.gameObject.SetActive(Visibility);
                }
                // Toggle Circle Image Object
                Transform circleImageChild = child.Find(child.name + "IdxImage");
                if (circleImageChild != null)
                {
                    circleImageChild.gameObject.SetActive(Visibility);
                }
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
                    instantiateObjects.CalculateandSetLengthPositions(CurrentStep);
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
    public void SetObjectLengthsText(float P1distance, float P2distance)
    {
        //Update Distance Text
        ObjectLengthsText.text = $"P1 | {(float)Math.Round(P1distance, 2)} P2 | {(float)Math.Round(P2distance, 2)}";
    }
    public void ToggleRobot(Toggle toggle)
    {
        Debug.Log("Robot Toggle Pressed");

        if(toggle.isOn && RequestTrajectoryButtonObject != null)
        {
            //TODO: Set robot URDF OBJECT TO ACTIVE AT ZERO CONFIGURATION.

            //Check current step data to set visibility and interactibility of request trajectory button.
            if(CurrentStep != null)
            {
                //Set interaction based on current step.
                SetTrajectoryRequestUIFromKey(CurrentStep);
            }
            else
            {
                Debug.LogWarning("Current Step is null.");
            }
            
            //Set the color of the robot toggle to yellow.
            SetUIObjectColor(RobotToggleObject, Yellow);
        }
        else
        {
            //TODO: Set robot URDF OBJECT TO INACTIVE//DESTROY STATIC ROBOT IMAGES IF THEY EXIST.
            
            //If the request trajectory button is visable then set everything to not visable.
            if (RequestTrajectoryButtonObject.activeSelf)
            {
                //Set Visibility of Request Trajectory Button
                TrajectoryServicesUIControler(false, false, false, false, false, false);
            }

            //Set the color of the Robot toggle button to white.
            SetUIObjectColor(RobotToggleObject, White);
        }
    }
    public void SetTrajectoryRequestUIFromKey(string key)
    {
        Step step = databaseManager.BuildingPlanDataItem.steps[key];

        Debug.Log($"THIS IS YOUR CURRENT STEP PRIORITY, {step.data.priority} and THIS IS YOUR CURRENT PRIORITY {databaseManager.CurrentPriority}");

        //If step is a robot step then make the request button visible.
        if(step.data.actor == "ROBOT")
        {
            //If the step is not built and priority is current priority then make request button visible and interactable
            if (!step.data.is_built && step.data.priority.ToString() == databaseManager.CurrentPriority)
            {    
                //Set Visibility of Request Trajectory Button, and interactability to true.
                TrajectoryServicesUIControler(true, true, false, false, false, false);
            }
            else
            {
                //Set visivility to true, but interactability to false.
                TrajectoryServicesUIControler(true, false, false, false, false, false);
            }
        
        }
        else
        {
            //Set Visibility of Request Trajectory Button
            TrajectoryServicesUIControler(false, false, false, false, false, false);
        }
    }
    public void TogglePriority(Toggle toggle)
    {
        Debug.Log("Priority Toggle Pressed");

        if(toggle.isOn && PriorityViewerToggleObject != null)
        {
            //Color Elements Based on Priority
            instantiateObjects.ApplyColorBasedOnPriority(databaseManager.CurrentPriority);

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
            if(instantiateObjects.visulizationController.VisulizationMode == VisulizationMode.ActorView)
            {
                instantiateObjects.ApplyColorBasedOnActor();
            }
            else if(instantiateObjects.visulizationController.VisulizationMode == VisulizationMode.BuiltUnbuilt)
            {
                instantiateObjects.ApplyColorBasedOnBuildState();
            }
            else
            {
                Debug.LogWarning("Could not find Visulization Mode.");
            }

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
                //if the update connection message is on turn it off
                if(MqttUpdateConnectionMessage.activeSelf)
                {
                    MqttUpdateConnectionMessage.SetActive(false);
                }
                
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
        databaseManager.AssemblyDataDict.Clear();
        databaseManager.QRCodeDataDict.Clear();
        databaseManager.UserCurrentStepDict.Clear();
        databaseManager.PriorityTreeDict.Clear();

        //Unsubscribe from topics
        mqttTrajectoryManager.UnsubscribeFromCompasXRTopics();

        //Unsubscibe from connection events
        mqttTrajectoryManager.RemoveConnectionEventListners();

        //Fetch settings data again
        databaseManager.FetchSettingsData(eventManager.settings_reference);

        //Disconnect from MQTT and reconnect after new application settings are received.
        mqttTrajectoryManager.DisconnectandReconnectAsyncRoutine();

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
                TouchSearchModeController(TouchMode.ElementEditSelection);
                
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
                TouchSearchModeController(TouchMode.None);

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
    public void TouchSearchModeController(TouchMode modetype)
    {
        //Set Touch Mode
        instantiateObjects.visulizationController.TouchMode = modetype;

        // If input mode type is ElementEditSelection then we know to search for touch input on objects
        if (modetype == TouchMode.ElementEditSelection)
        {
            Debug.Log ("***TouchMode: ELEMENT EDIT MODE***");
        }

        // If input mode type is None then fix all elements for touch selection.
        else if(modetype == TouchMode.None)
        {
            Debug.Log ("***TouchMode: NONE***");

            //Destroy active bounding box
            DestroyBoundingBoxFixElementColor();
            activeGameObject = null;
            Debug.Log ("***TouchMode: NONE***");
        }

        else
        {
            Debug.LogWarning("Could not find Touch Mode.");
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
            GameObject element = Elements.FindObject(i.ToString()).FindObject(step.data.element_ids[0] + " Geometry");
            Collider ElementCollider = element.FindObject(step.data.element_ids[0] + " Geometry").GetComponent<Collider>();
            Renderer ElementRenderer = element.FindObject(step.data.element_ids[0] + " Geometry").GetComponent<Renderer>();

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
            if(databaseManager.BuildingPlanDataItem.steps[activeGameObjectParentname].data.priority > Convert.ToInt16(databaseManager.CurrentPriority))
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
                        
                        instantiateObjects.ColorBuiltOrUnbuilt(step.data.is_built, element.gameObject.FindObject(step.data.element_ids[0] + " Geometry"));                          
                        
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
        instantiateObjects.ColorHumanOrRobot(step.data.actor, step.data.is_built, Elements.FindObject(key).FindObject(step.data.element_ids[0] + " Geometry"));
        

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

