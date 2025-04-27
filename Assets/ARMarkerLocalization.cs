// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using Microsoft.MixedReality.OpenXR;
// using TMPro;

// namespace CompasXR.Core
// {
//     public class ARMarkerLocalizer: MonoBehaviour
//     {

//         public GameObject XR_Rig;
//         [HideInInspector] public bool TrackingOn = false;
//         private Quaternion offsetRotation = Quaternion.Euler(90, 0, 0);
//         private GameObject Trackables;
//         private Coroutine trackingCoroutine;

//         private GameObject Elements;
//         private GameObject UserObjects;
//         public InstantiateObjects instantiateObjects;
//         public DatabaseManager databaseManager;

//         public Dictionary<string, Node> QRCodeDataDict = new Dictionary<string, Node>();


//         void Start()
//         {
//             //Find Other scripts in the scene
//             instantiateObjects = GameObject.Find("Instantiate").GetComponent<InstantiateObjects>();
//             databaseManager = GameObject.Find("DatabaseManager").GetComponent<DatabaseManager>();
            
//             //Find GameObjects that need to be transformed
//             Elements = GameObject.Find("Elements");
//             UserObjects = GameObject.Find("ActiveUserObjects");

//             Trackables = XR_Rig.transform.Find("Trackables").gameObject;
//             Trackables.SetActive(false);

//         }
        
//         public void EnableLocalization()
//         {
//             TrackingOn = true;   
//             Trackables.SetActive(true);
            
//             if (trackingCoroutine != null)
//             {
//                 StopCoroutine(trackingCoroutine);
//             }
//             trackingCoroutine = StartCoroutine(TrackingTimer());
//         }

//         private IEnumerator TrackingTimer()
//         {
//             yield return new WaitForSeconds(4);
//             if (TrackingOn)
//             {
//                 DisableLocalization();
//             }
//         }

//         public void DisableLocalization()
//         {  
//             TrackingOn = false;
//             Trackables.SetActive(false);

//             if (trackingCoroutine != null)
//             {
//                 StopCoroutine(trackingCoroutine);
//                 trackingCoroutine = null;
//             }
//             //untoggle uIController.Localize
//             if (uIController.Localize.IsToggled)
//             {
//                 uIController.Localize.ForceSetToggled(false);
//             }
            
//         }
        

//         //Update is called once per frame
//         void Update()
//         {
//             if  (TrackingOn && QRCodeDataDict.Count > 0 && Elements != null && XR_Rig != null  && Trackables != null)
//             {

//                 // Iterate through all children of Trackables
//                 foreach (Transform child in Trackables.transform)
//                 {
//                     // Check if the child has a component named ARMarker with a specific value
//                     string arMarkerText = child.GetComponent<ARMarker>().GetDecodedString();

//                     if (arMarkerText)
//                     {
//                         Debug.Log("Found marker: " + arMarkerText);
//                         string key = arMarkerText;
                        


//                         ObjectTransformations.TranslateGameObjectByImageTarget(Elements, child.gameObject, QRCodeDataDict[key].part.frame.point, QRCodeDataDict[key].part.frame.xaxis, QRCodeDataDict[key].part.frame.yaxis);
//                         ObjectTransformations.TranslateGameObjectByImageTarget(UserObjects, child.gameObject, QRCodeDataDict[key].part.frame.point, QRCodeDataDict[key].part.frame.xaxis, QRCodeDataDict[key].part.frame.yaxis);

//                         Elements.rotation *= offsetRotation;
//                         UserObjects.rotation *= offsetRotation;

//                         return; // Exit the loop once a marker is found
//                     }              
//                 }                    
//             }    
//         }

//         public void OnTrackingInformationReceived(object source, TrackingDataDictEventArgs e)
//         {
//             /*
//             * Method is used to update the QRCodeDataDict
//             * with the data received from the QR code tracking event.
//             */
//             Debug.Log("OnTrackingInformationReceived: Number of QR codes stored as a dict= " + e.QRCodeDataDict.Count);
//             QRCodeDataDict = e.QRCodeDataDict;
//         }
//     }
// }
