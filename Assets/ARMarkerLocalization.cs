using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.OpenXR;
using TMPro;
using CompasXR.UI;
using CompasXR.Core.Data;
using CompasXR.Core.Extentions;
using MixedReality.Toolkit;
using MixedReality.Toolkit.SpatialManipulation;
using UnityEngine.XR.Interaction.Toolkit;

namespace CompasXR.Core
{
    public class ARMarkerLocalizer: MonoBehaviour
    {

        public GameObject XR_Rig;
        
        [HideInInspector] public bool TrackingOn = false;
        private Quaternion offsetRotation = Quaternion.AngleAxis(90, Vector3.right);
        private GameObject Trackables;
        private Coroutine trackingCoroutine;

        private GameObject Elements;
        private GameObject UserObjects;
        private GameObject Structure;
        private GameObject StructureParent;
        public InstantiateObjects instantiateObjects;
        public DatabaseManager databaseManager;


        public UIFunctionalitiesMRTK uIController;
        public GameObject BoundingBox;
        public Dictionary<string, Node> QRCodeDataDict = new Dictionary<string, Node>();

        private bool MoveIsOn = false;
        private bool RotationIsOn = false;


        void Start()
        {
            //Find Other scripts in the scene
            instantiateObjects = GameObject.Find("Instantiate").GetComponent<InstantiateObjects>();
            databaseManager = GameObject.Find("DatabaseManager").GetComponent<DatabaseManager>();
            
            //Find GameObjects that need to be transformed
            Elements = GameObject.Find("Elements");
            UserObjects = GameObject.Find("ActiveUserObjects");
            StructureParent = GameObject.Find("StructureParent");
            Structure = StructureParent.FindObject("Structure");
            BoundingBox = StructureParent.FindObject("Cube");
            ObjectManipulator objManipulator = StructureParent.GetComponent<ObjectManipulator>();
            objManipulator.lastSelectExited.AddListener(OnLastSelectExited);
            BoundingBox.SetActive(false);

            var persistedObjects = GameObject.FindObjectOfType<PersistAcrossScenes>();
            if (persistedObjects != null)
            {
                // look for the children with name "MRTK XR Rig"
                foreach (Transform child in persistedObjects.transform)
                {
                    if (child.name == "MRTK XR Rig")
                    {
                        XR_Rig = child.gameObject;
                        break;
                    }
                }
            }


            Trackables = XR_Rig.transform.Find("Trackables").gameObject;
            Trackables.SetActive(false);

        }

        private void OnLastSelectExited(SelectExitEventArgs args)
        {

            Transform t = Structure.transform;
            Elements.transform.position = t.position;
            Elements.transform.rotation = t.rotation;
            UserObjects.transform.position = t.position;
            UserObjects.transform.rotation = t.rotation;
        }
        
        public void EnableLocalization()
        {
            TrackingOn = true;   
            Trackables.SetActive(true);
            Elements.SetActive(true);
            Structure.SetActive(true);
            Debug.Log("Localization is On");
            
            if (trackingCoroutine != null)
            {
                StopCoroutine(trackingCoroutine);
            }
            trackingCoroutine = StartCoroutine(TrackingTimer());
        }

        private IEnumerator TrackingTimer()
        {
            yield return new WaitForSeconds(5);
            if (TrackingOn)
            {
                DisableLocalization();
            }
        }

        public void DisableLocalization()
        {  
            TrackingOn = false;
            Trackables.SetActive(false);

            if (trackingCoroutine != null)
            {
                StopCoroutine(trackingCoroutine);
                trackingCoroutine = null;
            }
            //untoggle uIController.Localize
            if (uIController.LocalizeToggle.IsToggled)
            {
                uIController.LocalizeToggle.ForceSetToggled(false);
            }
            
        }
        
        public void ToggleMove()
        {
            MoveIsOn = !MoveIsOn;
            var moveConstraint = AxisFlags.YAxis;
            var rotationConstraint = AxisFlags.XAxis | AxisFlags.YAxis | AxisFlags.ZAxis;

            Debug.Log("Movement for Localization is: " + MoveIsOn);

            if (MoveIsOn == false)
            {
                moveConstraint = AxisFlags.XAxis | AxisFlags.YAxis | AxisFlags.ZAxis;
            }

            StructureParent.GetComponent<MoveAxisConstraint>().ConstraintOnMovement = moveConstraint;
            StructureParent.GetComponent<RotationAxisConstraint>().ConstraintOnRotation = rotationConstraint;
            ConstrainManipulation();

        }

        public void ToggleRotation()
        {
            RotationIsOn = !RotationIsOn;
            var rotationConstraint = AxisFlags.XAxis | AxisFlags.ZAxis;

            Debug.Log("Rotation for Localization is: " + RotationIsOn);

            if (RotationIsOn == false)
            {
                rotationConstraint = AxisFlags.XAxis | AxisFlags.YAxis | AxisFlags.ZAxis;
            }

            StructureParent.GetComponent<RotationAxisConstraint>().ConstraintOnRotation = rotationConstraint;
            ConstrainManipulation();

        }

        private void ConstrainManipulation()
        {
            if (MoveIsOn == false && RotationIsOn==false)
            {
                StructureParent.GetComponent<ObjectManipulator>().enabled = false;
                BoundingBox.SetActive(false);
            }
            else
            {
                StructureParent.GetComponent<ObjectManipulator>().enabled = true;
                BoundingBox.SetActive(true);
            }
        }

        //Update is called once per frame
        void Update()
        {
            if  (TrackingOn && QRCodeDataDict.Count > 0 && Elements != null && XR_Rig != null  && Trackables != null)
            {
                Debug.Log("ARMarkerLocalizer level 2");
                // Iterate through all children of Trackables
                foreach (Transform child in Trackables.transform)
                {
                    // Check if the child has a component named ARMarker with a specific value
                    string arMarkerText = child.GetComponent<ARMarker>().GetDecodedString();
                    Debug.Log("ARMarker text: " + arMarkerText);

                    if (arMarkerText!=null)
                    {
                        Debug.Log("Found marker: " + arMarkerText);
                        //from the string, get the characters after the underscore
                        string key = arMarkerText = arMarkerText.Substring(arMarkerText.IndexOf("_") + 1);
                        // Check if the key exists in the QRCodeDataDict
                        if (!QRCodeDataDict.ContainsKey(key))
                        {
                            Debug.Log("Key not found in QRCodeDataDict: " + key);
                            continue; // Skip to the next child if the key is not found
                        }
                        
                        ObjectTransformations.TranslateGameObjectByImageTarget(Elements, child.gameObject, QRCodeDataDict[key].part.frame.point, QRCodeDataDict[key].part.frame.xaxis, QRCodeDataDict[key].part.frame.yaxis);
                        ObjectTransformations.TranslateGameObjectByImageTarget(StructureParent, child.gameObject, QRCodeDataDict[key].part.frame.point, QRCodeDataDict[key].part.frame.xaxis, QRCodeDataDict[key].part.frame.yaxis);
                        ObjectTransformations.TranslateGameObjectByImageTarget(UserObjects, child.gameObject, QRCodeDataDict[key].part.frame.point, QRCodeDataDict[key].part.frame.xaxis, QRCodeDataDict[key].part.frame.yaxis);

                        // Elements.transform.rotation *= offsetRotation;
                        // StructureParent.transform.rotation *= offsetRotation;
                        // UserObjects.transform.rotation *= offsetRotation;

                        return; // Exit the loop once a marker is found
                    }              
                }                    
            }    
        }

        public void OnTrackingInformationReceived(object source, TrackingDataDictEventArgs e)
        {
            /*
            * Method is used to update the QRCodeDataDict
            * with the data received from the QR code tracking event.
            */
            Debug.Log("OnTrackingInformationReceived: Number of QR codes stored as a dict= " + e.QRCodeDataDict.Count);
            QRCodeDataDict = e.QRCodeDataDict;
        }
    }
}
