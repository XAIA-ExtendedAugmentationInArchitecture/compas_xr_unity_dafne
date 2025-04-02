using System;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using Firebase.Extensions;
using Newtonsoft.Json;
using CompasXR.Core.Extentions;


namespace CompasXR.Database.FirebaseManagment
{
    /*
    * CompasXR.Database.FirebaseManagement : A namespace to define and controll various Firebase connection,
    * configuration information, user record and general database management.
    */

    public class MaterialRegistrationManager : MonoBehaviour
    {
        /*
        * MaterialRegistrationManager : Class is used to manage the user record and configuration settings.
        * Additionally it is designed to handle the user record events, and allow users to create new user
        */
        private string userID;
        private DatabaseReference dbReference_root;

        public GameObject [] categories;
        private string [] categories_names;
        public TMPro.TMP_InputField quantities;
        public GameObject history_content;
        public GameObject history_entry_prefab;

        private int quantityToAdd = 1;


        //////////////////////////// Monobehaviour Methods //////////////////////////////
        ///
        protected virtual void OnDestroy()
        {
            // Unsubscribe from all events to prevent memory leaks
            foreach (string name in categories_names)
            {
                dbReference_root.Child(name).ValueChanged -= (sender, args) => { };
            }

        }
        void Start()
        {
            userID = StaticData.User;

            dbReference_root = FirebaseDatabase.DefaultInstance.RootReference;
            
            if (dbReference_root == null)
            {
                Debug.LogError("Firebase Database reference is null!");
            }
            else
            {
                print("Firebase Database reference is initialized.");
            }

            history_entry_prefab.GetComponent<TMPro.TMP_Text>().text = "";

            
            SetupHistoryListener();
            InitializeCategoryData();
            SetupQuantityInput();
        }

        void SetupHistoryListener()
        {
            dbReference_root.Child("RegistrationHistory").ChildAdded += (sender, args) =>
            {
                if (args.DatabaseError != null)
                {
                    Debug.LogError($"Error: {args.DatabaseError.Message}");
                    return;
                }

                if (!args.Snapshot.Exists) return;

                string json = args.Snapshot.GetRawJsonValue();
                Dictionary<string, object> historyEntry = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                Debug.Log($"History Entry: {historyEntry}");

                CreateHistoryEntryUI(historyEntry);
            };
        }

        void CreateHistoryEntryUI(Dictionary<string, object> historyEntry)
        {
            GameObject entry = Instantiate(history_entry_prefab, history_content.transform);
            TMPro.TMP_Text textComponent = entry.GetComponent<TMPro.TMP_Text>();

            string category = historyEntry.ContainsKey("category") ? historyEntry["category"].ToString() : "Unknown";
            string user = historyEntry.ContainsKey("userID") ? historyEntry["userID"].ToString() : "Unknown";
            bool isAdded = historyEntry.ContainsKey("isAdded") && (bool)historyEntry["isAdded"];
            int quantityChanged = historyEntry.ContainsKey("quantityChanged") ? Convert.ToInt32(historyEntry["quantityChanged"]) : 0;

            long timestamp = historyEntry.ContainsKey("timestamp") ? Convert.ToInt64(historyEntry["timestamp"]) : 0;
            DateTime dateTime = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).ToLocalTime().DateTime;
            string dateStr = dateTime.ToString("yy.MM.dd");
            string timeStr = dateTime.ToString("HH:mm:ss");

            string userFormatted = (string.IsNullOrEmpty(user) || user == userID) ? $"<b>You</b>" : user;
            string action = isAdded ? "added" : "removed";

            string color = "white";
            switch (category)
            {
                case "Group A (5-6)": color = "red"; break;
                case "Group B (7-8)": color = "blue"; break;
                case "Group C (9-10)": color = "green"; break;
            }

            string actionString = $"<color={color}>{action} {quantityChanged}</color>";
            textComponent.text = $"{dateStr}  |  {timeStr}  | {userFormatted} {actionString} items";
        }

        void InitializeCategoryData()
        {
            categories_names = new string[categories.Length];

            for (int i = 0; i < categories.Length; i++)
            {
                int index = i;
                string name = categories[index].FindObject("GroupName").GetComponent<TMPro.TMP_Text>().text;
                categories_names[index] = name;

                LoadInitialCategoryQuantity(index, name);
                SetupCategoryListeners(index, name);
                SetupCategoryButtons(index, name);
            }
        }

        void LoadInitialCategoryQuantity(int index, string name)
        {
            dbReference_root.Child(name).GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("Error occurred while reading data from the database.");
                    return;
                }

                int currentQuantity = 0;
                DataSnapshot snapshot = task.Result;

                if (snapshot.Exists && int.TryParse(snapshot.Value.ToString(), out int existingValue))
                {
                    currentQuantity = existingValue;
                }

                categories[index].FindObject("Quantities").GetComponent<TMPro.TMP_Text>().text = currentQuantity.ToString();
            });
        }

        void SetupCategoryListeners(int index, string name)
        {
            dbReference_root.Child(name).ValueChanged += (sender, args) =>
            {
                if (args.DatabaseError != null)
                {
                    Debug.LogError($"Error: {args.DatabaseError.Message}");
                    return;
                }

                if (args.Snapshot.Exists && int.TryParse(args.Snapshot.Value.ToString(), out int newValue))
                {
                    categories[index].FindObject("Quantities").GetComponent<TMPro.TMP_Text>().text = newValue.ToString();
                }
            };
        }

        void SetupCategoryButtons(int index, string name)
        {
            Button addButton = categories[index].FindObject("ButtonPlus").GetComponent<Button>();
            addButton.onClick.AddListener(() => RegisterMaterial(name, true));

            Button removeButton = categories[index].FindObject("ButtonMinus").GetComponent<Button>();
            removeButton.onClick.AddListener(() => RegisterMaterial(name, false));
        }

        void SetupQuantityInput()
        {
            quantities.placeholder.GetComponent<TMPro.TMP_Text>().text = "1";
            quantities.contentType = TMPro.TMP_InputField.ContentType.IntegerNumber;
            quantities.onValueChanged.AddListener((value) =>
            {
                if (string.IsNullOrWhiteSpace(value) || !int.TryParse(value, out quantityToAdd))
                {
                    quantityToAdd = 1;
                }
            });
        }


        //////////////////////////// Material Registration Management Methods //////////////////////////////
        public void RegisterMaterial(string selectedCategory, bool add = true)
        {
            if (dbReference_root == null)
            {
                Debug.LogError("dbReference_root is null! Make sure Firebase is initialized and the Start method is called.");
                return;
            }

            
            // Access the selected category in the root and try to increment its value
            dbReference_root.Child(selectedCategory).GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("Error occurred while reading data from the database.");
                    return;
                }

                int currentQuantity = 0;
                DataSnapshot snapshot = task.Result;

                if (snapshot.Exists && int.TryParse(snapshot.Value.ToString(), out int existingValue))
                {
                    currentQuantity = existingValue;
                }

                int actualChange = quantityToAdd * (add ? 1 : -1);
                int updatedQuantity = currentQuantity + actualChange;

                if (updatedQuantity < 0)
                {
                    actualChange = -currentQuantity;
                    updatedQuantity = 0; 
                }

                dbReference_root.Child(selectedCategory).SetValueAsync(updatedQuantity).ContinueWithOnMainThread(t =>
                {
                    if (t.IsFaulted)
                    {
                        Debug.LogError("Failed to update quantity.");
                    }
                    else if (t.IsCompleted)
                    {
                        if (actualChange == 0)
                        {
                            Debug.Log("No change in quantity.");
                            return;
                        }
                        // Prepare history entry
                        Dictionary<string, object> historyEntry = new Dictionary<string, object>
                        {
                            { "timestamp", ServerValue.Timestamp }, // Firebase server time
                            { "userID", userID },
                            { "category", selectedCategory },
                            { "isAdded", add },
                            { "quantityChanged", Mathf.Abs(actualChange) }
                        };

                        // Push the entry to "RegistrationHistory"
                        dbReference_root.Child("RegistrationHistory").Push().SetValueAsync(historyEntry).ContinueWithOnMainThread(historyTask =>
                        {
                            if (historyTask.IsFaulted)
                            {
                                Debug.LogError("Failed to add history entry.");
                            }
                            else
                            {
                                Debug.Log("History entry added successfully.");
                            }
                        });

                        
                        Debug.Log($"Quantity updated successfully. {selectedCategory}: {updatedQuantity}");
                        //HelpersExtensions.ChangeScene("MainGame");
                    }
                });
            });

        }
    }
}
