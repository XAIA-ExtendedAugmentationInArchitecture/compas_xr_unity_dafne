using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using CompasXR.Systems;


namespace CompasXR.Database.FirebaseManagment
{
    /*
    * CompasXR.Database.FirebaseManagement : A namespace to define and controll various Firebase connection,
    * configuration information, user record, and general database management.
    */

    public sealed class FirebaseManager
    {
        /*
        * FirebaseManager : Sealed class using the Singleton Pattern.
        * This class is used to manage the Firebase configuration settings.
        */

        private static FirebaseManager instance = null;
        private static readonly object padlock = new object();
        public static FirebaseManager Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new FirebaseManager();
                    }
                    return instance;
                }
            }
        }
        public string appId;
        public string apiKey;
        public string databaseUrl;
        public string storageBucket;
        public string projectId;

        FirebaseManager() 
        {
            /*
            * FirebaseManager : Constructor for the FirebaseManager class.
            * This constructor is used to set the configuration settings for Firebase.
            * It contains the required settings for connecting to the base database.
            */
            apiKey = "AIzaSyCWcMRB_oYvFrId2S-L3DUkUspg2bwx3Fw";
            databaseUrl = "https://compasxrdafne-default-rtdb.firebaseio.com";
            storageBucket = "compasxrdafne.firebasestorage.app";
            projectId = "compasxrdafne";

            CompasXR.Systems.OperatingSystem currentOS = OperatingSystemManager.GetCurrentOS();
            switch (currentOS)
            {
                case CompasXR.Systems.OperatingSystem.iOS:
                    appId = "1:956444401010:ios:4900c857201308835791eb";
                    break;
                case CompasXR.Systems.OperatingSystem.Android: 
                    appId = "1:956444401010:android:c3e7efc21711540e5791eb";
                    break;
                default:
                    appId = "1:956444401010:android:c3e7efc21711540e5791eb";
                    break;
            }
        }


    }
}
