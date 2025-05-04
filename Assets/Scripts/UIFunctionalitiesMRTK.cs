using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using System;
using System.Linq;
using TMPro;
using UnityEngine.XR.ARFoundation;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using CompasXR.Core;
using CompasXR.Systems;
using CompasXR.Core.Data;
using CompasXR.Core.Extentions;
using CompasXR.AppSettings;
using CompasXR.Robots;
using CompasXR.Robots.MqttData;
using Unity.VisualScripting;

using System.Collections;
using MixedReality.Toolkit;
using MixedReality.Toolkit.UX;
using MixedReality.Toolkit.SpatialManipulation;

namespace CompasXR.UI
{
    /*
    * CompasXR.UI : Is the namespace for all Classes that
    * controll the primary functionalities releated to the User Interface in the CompasXR Application.
    * Functionalities, such as UI interaction, UI element creation, and UI element control.
    */
    public class UIFunctionalitiesMRTK : MonoBehaviour
    {
        /*
        * UIFunctionalities : Class is used to manage the User Interface and User Interface elements.
        * This class is designed to handle the all UI elements and are primariarily divided into 3 sections.
        * 1. Primary UI Elements: These are UI elements used to control the primary functionalities of the application (constantly on the screen).
        * 2. Visualizer Menu Elements: These are UI elements used to control the various visualization functionalities of the application.
        * 3. Menu Elements: These are UI elements used to control additional functionalities of the application. Ex. Info, Reload, etc.
        */

        //Other Scripts for inuse objects

        public PressableButton LocalizeToggle;
        public PressableButton NextGeometryButton;
        public PressableButton PreviousGeometryButton;
        public PressableButton IsBuiltToggleButton;
        public PressableButton IDToggle;
        public PressableButton PriorityToggle;
        public PressableButton CategoriesToggle;
        public PressableButton MoveStructureToggle;
        public PressableButton RotateStructureToggle;

        public MixedReality.Toolkit.UX.Slider BuiltSlider;
        public MixedReality.Toolkit.UX.Slider UnBuiltSlider;

        public ARMarkerLocalizer localizer;


//         /////////////////////////////////// Monobehaviour Methods ///////////////////////////////////////////////////////////        
        void Start()
        {
            /*
            * Start : Method is used to initialize the UI elements and set up the UI Object & Script Dependencies on start.
            */
            OnAwakeInitilization();
        }
//         void Update()
//         {
//             /*
//             * Update : Method is used to update the UI elements and check for touch option activation.
//             */
//             TouchSearchControler();
//         }

//         /////////////////////////////////// UI Control & OnStart methods ////////////////////////////////////////////////////
        private void OnAwakeInitilization()
        {
            /*
            * OnAwakeInitilization : Method is used to initialize the UI elements and set up the 
            * UI Objects, Script Dependencies, & set up relationships on start.
            */

        
//             //Set up UI Objects and buttons on start
            SetPrimaryUIItemsOnStart();
        }
        private void SetPrimaryUIItemsOnStart()
        {
            /*
            * SetPrimaryUIItemsOnStart : Method is used to set up the primary UI elements on start.
            * Primary UI elements constitute the UI elements that are constantly on the screen
            * & control basic fundimental functionalities of the application.
            */

            RotateStructureToggle.OnClicked.AddListener( 
                () => localizer.ToggleRotation());

            MoveStructureToggle.OnClicked.AddListener( 
                () => localizer.ToggleMove());
            
            Button IsBuiltCanvas = gameObject.GetComponent<UIFunctionalities>().IsBuiltButtonObject.GetComponent<Button>();

            NextGeometryButton.OnClicked.AddListener( 
                () => {gameObject.GetComponent<UIFunctionalities>().NextStepButton();
                    
                });
            
            PreviousGeometryButton.OnClicked.AddListener( 
                () => {gameObject.GetComponent<UIFunctionalities>().PreviousStepButton();
                });

            Toggle PrToggle = gameObject.GetComponent<UIFunctionalities>().PriorityViewerToggleObject.GetComponent<Toggle>();
            PriorityToggle.OnClicked.AddListener( 
                () => { PrToggle.isOn = PriorityToggle.IsToggled;
                PrToggle.onValueChanged.Invoke(PrToggle.isOn);
                });
            
            IsBuiltToggleButton.OnClicked.AddListener( 
                () => IsBuiltCanvas.onClick.Invoke());
            
            Toggle IDToggleCanvas = gameObject.GetComponent<UIFunctionalities>().IDToggleObject.GetComponent<Toggle>();
            IDToggle.OnClicked.AddListener( 
                () => { IDToggleCanvas.isOn = IDToggle.IsToggled;
                IDToggleCanvas.onValueChanged.Invoke(IDToggleCanvas.isOn);
                });

            Toggle CategoriesToggleCanvas = gameObject.GetComponent<UIFunctionalities>().PreviewActorToggleObject.GetComponent<Toggle>();
            CategoriesToggle.OnClicked.AddListener( 
                () => { CategoriesToggleCanvas.isOn = CategoriesToggle.IsToggled;
                CategoriesToggleCanvas.onValueChanged.Invoke(CategoriesToggleCanvas.isOn);
                });
        
            BuiltSlider.OnValueUpdated.AddListener(
                (SliderEventData data) =>{
                    gameObject.GetComponent<UIFunctionalities>().PreviewPreviousGeometrySliderSetVisibilty(data.NewValue);
                    gameObject.GetComponent<UIFunctionalities>().PreviewPreviousGeometrySlider.value =data.NewValue;
                }
            );

            UnBuiltSlider.OnValueUpdated.AddListener(
                (SliderEventData data) =>{
                    gameObject.GetComponent<UIFunctionalities>().PreviewGeometrySliderSetVisibilty(data.NewValue);
                    gameObject.GetComponent<UIFunctionalities>().PreviewGeometrySlider.value =data.NewValue;
                }
            );

            LocalizeToggle.OnClicked.AddListener(
                () => localizer.EnableLocalization()
            );


     
        }
    }
}


