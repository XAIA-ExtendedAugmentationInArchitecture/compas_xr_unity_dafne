using System;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using Firebase.Extensions;
using Newtonsoft.Json;
using CompasXR.Core.Extentions;
using M2MqttUnity;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Threading.Tasks;
using System.Threading;
using CompasXR.Robots.MqttData;
using System.Linq;




namespace CompasXR.Communcation.MqttManagement
{
    /*
    * CompasXR.Database.FirebaseManagement : A namespace to define and controll various Firebase connection,
    * configuration information, user record and general database management.
    */

    public class DimensionDetectionManager : M2MqttUnityClient
    {

        private string userID;
        private DatabaseReference dbReference_root;
        private DatabaseReference dbReference_elements;
        private DatabaseReference dbReference_history;

        
        public enum Category
        {
        XSmall,
        Small,
        Medium,
        Large,
        Rand
        }
        
        public GameObject CorrectGroupOptionsButtons;
        private Category category;

        private FinalResult final_result = null;

        public Button computeButton;
        public Button acceptButton;
        public Button correctButton;

        public TMPro.TMP_InputField correctWidth;
        public TMPro.TMP_InputField correctHeight;
        public Button submitButton;
        public Button previousButton;
        public TMPro.TMP_InputField quantities;
        public TMPro.TMP_Text iDsText; // I need to make sure it appears correct when the app starts
        
        public TMPro.TMP_Text det_group;
        public TMPro.TMP_Text det_dims;


        public GameObject history_content;
        public GameObject history_entry_prefab;


        private int quantityToAdd = 1;
        private int currentIndex = 0;

        private int attempt = 0;

        private string m_msg;    
        public string msg
        {
            get { return m_msg; }
            set
            {
                if (m_msg == value) return;
                m_msg = value;
                OnMessageArrived?.Invoke(m_msg);
            }
        }

        public event OnMessageArrivedDelegate OnMessageArrived;
        public delegate void OnMessageArrivedDelegate(string newMsg);
        private List<string> eventMessages = new List<string>();

        protected override void Start()
        {
            userID = StaticData.User;

            dbReference_root = FirebaseDatabase.DefaultInstance.RootReference;
            dbReference_elements = dbReference_root.Child("DetectedElements");
            dbReference_history = dbReference_root.Child("RegistrationHistory");
            
            if (dbReference_root == null)
            {
                Debug.LogError("Firebase Database reference is null!");
            }
            else
            {
                print("Firebase Database reference is initialized.");
            }

            // history_entry_prefab.GetComponent<TMPro.TMP_Text>().text = "";

            
            // SetupHistoryListener();
            InitializeCategoryData();
            SetupQuantityInput();
            SetupCorrectDimsInput();

            base.Start();
            OnStartorRestartInitilization();

            //AddListenerOptionButtons(CorrectGroupOptionsButtons);

            computeButton.onClick.AddListener(() => SendComputerRequest ());
            //correctButton.onClick.AddListener(() => RegisterCorrectionAttempt());
            acceptButton.onClick.AddListener(async () => await AcceptSubmitAction());
            //submitButton.onClick.AddListener(async () => await AcceptSubmitAction(true));

            previousButton.onClick.AddListener(async () =>
            {
                await RemoveLastEntry();
                await InitializeCategoryData();
            });

        }

        protected override void Update()
        {
            base.Update();
        }
        public void OnDestroy()
        {
            // Unsubscribe from all events to prevent memory leaks
            // foreach (string name in categories_names)
            // {
            //     dbReference_root.Child(name).ValueChanged -= (sender, args) => { };
            // }

            UnsubscribeFromCompasXRTopics();
            RemoveConnectionEventListners();
            Disconnect();
            
        }

        public void OnStartorRestartInitilization(bool Restart = false)
        {
            /*
            * Method is used to initialize the MQTT connection, find dependencies, and add listners
            * for subscriptions on connected.
            */
            Connect();
            AddConnectionEventListners();
        }

       
       ////////////////////////////////Button Stuff//////////////////////////////////////
       
        public void AddListenerOptionButtons(GameObject parentGobject)
        {
            // check the children of the parentGobject and find all the buttons aand return them as a list 
            foreach (Transform child in parentGobject.transform)
            {
                Button button = child.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.AddListener(() => OnOptionButtonClicked(button));
                }
            }
        }

        public void OnOptionButtonClicked(Button button)
        {
            if (final_result == null)
            {
                Debug.LogError("Final result is null. Cannot register correction attempt.");
                return;
            }

            final_result.corrected_category = button.name;
        }
       
       
        //////////////////////////// MQTT Stuff //////////////////////////////
        
        
        private void SubscribeToTopic(string topicToSubscribe)
        {
            /*
            * Method is used to subscribe to a custom topic.
            */
            if (!string.IsNullOrEmpty(topicToSubscribe) && client != null)
            {
                Debug.Log("MQTT: SubscribeToTopic: Subscribing to topic: " + topicToSubscribe);
                client.Subscribe(new string[] { topicToSubscribe }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            }
            else
            {
                Debug.LogError("MQTT: Topic to subscribe is empty or client is null.");
            }
        }
        private void UnsubscribeFromTopic(string topicToUnsubscribe)
        {
            /*
            * Method is used to unsubscribe from a custom topic.
            */
            if (!string.IsNullOrEmpty(topicToUnsubscribe) && client != null)
            {
                client.Unsubscribe(new string[] { topicToUnsubscribe });
                Debug.Log("MQTT: UnsubscribeFromTopic: Unsubscribed from topic: " + topicToUnsubscribe);
            }
            else
            {
                Debug.LogWarning("MQTT: Topic to unsubscribe is empty or client is null.");
            }
        }
        public void PublishToTopic(string publishingTopic,  Dictionary<string, object> message)
        {   
            /*
            * Method is used to publish a message to a custom topic.
            */
            if (client != null && client.IsConnected)
            {
                string messagePublish = JsonConvert.SerializeObject(message);
                client.Publish(publishingTopic, System.Text.Encoding.UTF8.GetBytes(messagePublish), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
            }
            else
            {
                Debug.LogWarning("MQTT: PublishToTopic: Client is null or not connected. Cannot publish message.");
            }
        }
        public void SubscribeToCompasXRTopics()
        {
            /*
            * Method is used to subscribe to the custom Compas XR Topics.
            */
            Debug.Log("MQTT: SubscribeToCompasXRTopics: Subscribing to Compas XR Topics");
            SubscribeToTopic("/dafne/material_registration/result");

        }
        public void UnsubscribeFromCompasXRTopics()
        {
            /*
            * Method is used to unsubscribe from the custom Compas XR Topics.
            */
            UnsubscribeFromTopic("/dafne/material_registration/result");

        }  

        //////////////////////////////////////////// Message Managers ////////////////////////////////////////////
        private void StoreMessage(string eventMsg)
        {
            /*
            * Method is used to store the last 50 messages received from the MQTT broker.
            */
            if (eventMessages.Count > 50) eventMessages.Clear();
            eventMessages.Add(eventMsg);
        }
        protected override void DecodeMessage(string topic, byte[] message)
        {
            /*
            * Method is used to decode the message received from the MQTT broker.
            * The method will decode the message and call the appropriate message handler based on the topic.
            */
            msg = System.Text.Encoding.UTF8.GetString(message);
            Debug.Log("MQTT: DecodeMessage: Received: " + msg + " from topic: " + topic);
            CompasXRIncomingMessageHandler(topic, msg);
            StoreMessage(msg);
        }
        private void CompasXRIncomingMessageHandler(string topic, string message)
        {
            /*
            * Method is used to handle the incoming messages from the MQTT broker based on the topic.
            */
            if (topic == "/dafne/material_registration/result")
            {
                Debug.Log("MQTT: GetDetectionResult Message Handeling");
                long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                GetDetectionResult getDetectionResultmessage = GetDetectionResult.Parse(message, timestamp);

                VisualizeDetectionResult(getDetectionResultmessage);
            }
            else
            {
                Debug.LogWarning("MQTT: No message handler for topic: " + topic);
            }

        }

        private void VisualizeDetectionResult( GetDetectionResult result) //we create here the final result object
        {
            /*
            * Method is used to visualize the detection result received from the MQTT broker.
            */
            det_group.text = "Error";
            det_dims.text = $"W: 0.0 x H: 0.0 cm";

            if (result.warning == "Position Unchanged")
            {
                Debug.Log("MQTT: Position Unchanged: " + result.warning);
                return;
            }
            else if (result.warning == "No Measurements")
            {
                Debug.Log("MQTT: No Measurements: " + result.warning);
                return;
            }
            else if (result.warning == "success")
            {
                Debug.Log("MQTT: Success: " + result.warning);

                if ( result.attempt !=attempt || (currentIndex != result.ids[0]) || (currentIndex + quantityToAdd-1 != result.ids[1]))
                { 
                    Debug.Log("Synchronization error");
                    return;
                }
                
                (Category category, float confidence) = CalculateCategory(result.detected_dimensions);

                final_result = new FinalResult();
                final_result.GetFinalResult(result, userID, category.ToString() );
                det_group.text = GetCategoryTextWithColor(category);

                float minWidth = Mathf.Min(result.detected_dimensions[0], result.detected_dimensions[1]);
                float maxHeight = Mathf.Max(result.detected_dimensions[0], result.detected_dimensions[1]);

                correctWidth.placeholder.GetComponent<TMPro.TMP_Text>().text = minWidth.ToString("F2");
                correctHeight.placeholder.GetComponent<TMPro.TMP_Text>().text = maxHeight.ToString("F2");
                correctWidth.text = minWidth.ToString("F2");
                correctHeight.text = maxHeight.ToString("F2");

                if (confidence != 100.0f)
                {
                    det_dims.text = $"W: {result.detected_dimensions[0]} x H: {result.detected_dimensions[1]} cm - {confidence}%";
                }
                else
                {
                    det_dims.text = $"W: {result.detected_dimensions[0]} x H: {result.detected_dimensions[1]} cm";
                }
                
            }
        }

        private string GetCategoryTextWithColor(Category category)
        {
            /*
            * Method is used to get the category text with color.
            */
            string color = "white";
            switch (category)
            {
                case Category.XSmall: color = "#FFA500"; break;
                case Category.Small: color = "blue"; break;
                case Category.Medium: color = "green"; break;
                case Category.Large: color = "red"; break;
                case Category.Rand: color = "#FF00FF"; break;
            }
            return $"<color={color}>{category}</color>";
        }

        private (Category, float) CalculateCategory(float[] detected_dimensions)
        {
            /*
            * Method used to calculate the category based on the detected dimensions.
            */
            float height = Mathf.Max(detected_dimensions[0], detected_dimensions[1]); // in cm
            float width = Mathf.Min(detected_dimensions[0], detected_dimensions[1]);  // in cm

            float threshold_height = 5.0f;
            float expected_height = 40.0f;
            float threshold_width = 0.5f;

            float confidence  = 100.0f;

            // Check if length is outside the acceptable range
            if (height <= expected_height - threshold_height || height >= expected_height + threshold_height)
            {
                return (Category.Rand, confidence);
            }

            // Check based on width
            if ( width>= 3.50f && width <= 7.00f)
            {
                if (Mathf.Abs(width - 7.00f) <= threshold_width)
                {
                    confidence = Mathf.Round((1.0f - Mathf.Abs(width - 7.00f) / threshold_width) * 50.0f * 100f) / 100f;
                    Debug.LogWarning($"Potential false detection: Close to XSmall/Small boundary. Confidence: {confidence:F1}%.");
                }
                return (Category.XSmall, confidence);
            }
            else if (width > 7.00f && width <= 10.00f)
            {
                if (Mathf.Abs(width - 7.00f) <= threshold_width || Mathf.Abs(width - 10.00f) <= threshold_width)
                {
                    float dist = Mathf.Min(Mathf.Abs(width - 7.00f), Mathf.Abs(width - 10.00f));
                    confidence = (float)Math.Round((1.0f - dist / threshold_width) * 50.0f * 100f) / 100f;
                    Debug.LogWarning($"Potential false detection: Close to Small boundary. Confidence: {confidence:F1}%.");
                }
                return (Category.Small, confidence);
            }
            else if (width > 10.00f && width <= 13.00f)
            {
                if (Mathf.Abs(width - 10.00f) <= threshold_width || Mathf.Abs(width - 13.00f) <= threshold_width)
                {
                    float dist = Mathf.Min(Mathf.Abs(width - 10.00f), Mathf.Abs(width - 13.00f));
                    confidence = (float)Math.Round((1.0f - dist / threshold_width) * 50.0f * 100f) / 100f;
                    Debug.LogWarning($"Potential false detection: Close to Medium boundary. Confidence: {confidence:F1}%.");
                }
                return (Category.Medium, confidence);
            }
            else if (width > 13.00f)
            {
                if (Mathf.Abs(width - 13.00f) <= threshold_width)
                {
                    confidence = (float)Math.Round((1.0f - Mathf.Abs(width - 13.00f) / threshold_width) * 50.0f * 100f) / 100f;
                    Debug.LogWarning($"Potential false detection: Close to Medium/Large boundary. Confidence: {confidence:F1}%.");
                }
                return (Category.Large, confidence);
            }

            Debug.LogWarning("Width does not fit into any category. Returning Random as fallback.");
            return (Category.Rand, confidence);
        }

        [System.Serializable]
        public class FinalResult
        {
            public int attempt { get; private set; }
            public float[] detected_dimensions { get; private set; }
            public int quantities { get; private set; }
            public string userID { get; private set; }
            public string detected_category { get; private set; }
            public bool isCorrected { get; set; } // if the user corrected the dimensions
            public string corrected_category { get; set; }
            public float[] corrected_dimensions { get; private set; }
            public long timestamp_received { get; private set; }
            public List<long> timestamp_correction { get; private set; } // timestamp of when the user decided to correct the dimensions
            public long timestamp_submit { get; set; } // timestamp of when it was submitted


            //{ "timestamp": 0, "userID": "", "category": "", "isAdded": true, "quantities": 0 , "detected_dims": float[], "corrected_dims" = float[] }, 

            public void GetFinalResult(GetDetectionResult detectionResult, string userID, string detected_category, long timestamp_submit = 0, float[] corrected_dimensions = null, string corrected_category = "", List<long> timestamp_correction = null)
            {

                this.attempt = detectionResult.attempt;
                this.detected_dimensions = detectionResult.detected_dimensions;
                this.quantities = detectionResult.ids[1] - detectionResult.ids[0] + 1;
                this.userID = userID;
                this.detected_category = detected_category;
                this.corrected_category = corrected_category;
                this.corrected_dimensions = corrected_dimensions ?? new float[] { 0.0f, 0.0f };
                this.timestamp_received = detectionResult.timestamp;
                this.timestamp_correction = timestamp_correction ?? new List<long>();
                this.timestamp_submit = timestamp_submit;
            }

            public Dictionary<string, object> GetData()
            {
                /*
                * Method is used to retrieve the GetDetectionResult data as a dictionary.
                */
                return new Dictionary<string, object>
                {
                    {"attempt", attempt},
                    {"detected_dimensions", detected_dimensions},
                    {"quantities", quantities},
                    {"userID", userID},
                    {"detected_category", detected_category},
                    {"isCorrected", isCorrected},
                    {"corrected_category", corrected_category},
                    {"corrected_dimensions", corrected_dimensions},
                    {"timestamp_received", timestamp_received},
                    {"timestamp_correction", timestamp_correction},
                    {"timestamp_submit", timestamp_submit}
                };
            }

        }


        [System.Serializable]
        public class GetDetectionResult
        {
            public string warning { get; private set; }
            public int[] ids { get; private set; }
            public int attempt { get; private set; }
            public float[] detected_dimensions { get; private set; }
            public long timestamp { get; private set; }


            //{"warning": "Position Unchanged", "ids": current_ids, "attempt": attempt, "detected_dimensions": []}
            public GetDetectionResult(string waring, int[] ids, int attempt, float[] detected_dimensions, long timestamp = 0)
            {
                /*
                * GetDetectionResult : Class is used to manage the GetDetectionResult message.
                */
                this.warning = waring;
                this.ids = ids;
                this.attempt = attempt;
                this.detected_dimensions = detected_dimensions;
                this.timestamp = timestamp;
            }

            public Dictionary<string, object> GetData()
            {
                /*
                * Method is used to retrieve the GetDetectionResult data as a dictionary.
                */
                return new Dictionary<string, object>
                {
                    {"warning", warning},
                    {"ids", ids},
                    {"attempt", attempt},
                    {"detected_dimensions", detected_dimensions},
                    {"timestamp", timestamp}
                };
            }
            public static GetDetectionResult Parse(string jsonString, long timestamp)
            {
                /*
                * Method is used to parse an instance of the class from a JSON string.
                */
                var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
                var warning = jsonObject["warning"].ToString();
                var ids = JsonConvert.DeserializeObject<int[]>(JsonConvert.SerializeObject(jsonObject["ids"]));
                var attempt = Convert.ToInt32(jsonObject["attempt"]);
                var detected_dimensions = JsonConvert.DeserializeObject<float[]>(JsonConvert.SerializeObject(jsonObject["detected_dimensions"]));
                return new GetDetectionResult(warning, ids, attempt, detected_dimensions, timestamp);
            }
        }

        //{"action": "compute", "ids": [0, 24], "attempt": 0}

        public Dictionary<string, object> GetComputeMessage(int[] ids, int attempt)
        {
            /*
            * Method is used to create a message to be sent to the MQTT broker.
            */
            return new Dictionary<string, object>
            {
                {"action", "compute"},
                {"ids", ids},
                {"attempt", attempt}
            };
        }

        public void SendComputerRequest ()
        {
            /*
            * Method is used to create a message to be sent to the MQTT broker.
            */
            attempt++;
            det_group.text = "Wait";
            Dictionary<string, object> message = GetComputeMessage(new int[] { currentIndex, currentIndex + quantityToAdd - 1 }, attempt);
            PublishToTopic("/dafne/material_registration/actions", message);
        }

        public void RegisterCorrectionAttempt()
        {
            /*
            * Method is used to create a message to be sent to the MQTT broker.
            */

            if (final_result == null)
            {
                Debug.LogError("Final result is null. Cannot register correction attempt.");
                return;
            }
            final_result.timestamp_correction.Add(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        }

        public async Task AcceptSubmitAction(bool isCorrected = false)
        {
            if (final_result == null)
            {
                Debug.LogError("Final result is null. Cannot register correction attempt.");
                return;
            }
            final_result.timestamp_submit = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            if (correctWidth.text == correctWidth.placeholder.GetComponent<TMPro.TMP_Text>().text && correctHeight.text == correctHeight.placeholder.GetComponent<TMPro.TMP_Text>().text)
            {
                isCorrected = false;
            }
            else
            {
                isCorrected = true;
            }
            await RegisterMaterial(final_result, isCorrected);
            
            await InitializeCategoryData();
            

        }
        
        
        
        //////////////////////////// Monobehaviour Methods //////////////////////////////
        ///


        // void SetupHistoryListener()
        // {
        //     dbReference_root.Child("RegistrationHistory").ChildAdded += (sender, args) =>
        //     {
        //         if (args.DatabaseError != null)
        //         {
        //             Debug.LogError($"Error: {args.DatabaseError.Message}");
        //             return;
        //         }

        //         if (!args.Snapshot.Exists) return;

        //         string json = args.Snapshot.GetRawJsonValue();
        //         Dictionary<string, object> historyEntry = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
        //         Debug.Log($"History Entry: {historyEntry}");

        //         CreateHistoryEntryUI(historyEntry);
        //     };
        // }

        // void CreateHistoryEntryUI(Dictionary<string, object> historyEntry)
        // {
        //     GameObject entry = Instantiate(history_entry_prefab, history_content.transform);
        //     TMPro.TMP_Text textComponent = entry.GetComponent<TMPro.TMP_Text>();

        //     string category = historyEntry.ContainsKey("category") ? historyEntry["category"].ToString() : "Unknown";
        //     string user = historyEntry.ContainsKey("userID") ? historyEntry["userID"].ToString() : "Unknown";
        //     bool isAdded = historyEntry.ContainsKey("isAdded") && (bool)historyEntry["isAdded"];
        //     int quantityChanged = historyEntry.ContainsKey("quantityChanged") ? Convert.ToInt32(historyEntry["quantityChanged"]) : 0;

        //     long timestamp = historyEntry.ContainsKey("timestamp") ? Convert.ToInt64(historyEntry["timestamp"]) : 0;
        //     DateTime dateTime = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).ToLocalTime().DateTime;
        //     string dateStr = dateTime.ToString("yy.MM.dd");
        //     string timeStr = dateTime.ToString("HH:mm:ss");

        //     string userFormatted = (string.IsNullOrEmpty(user) || user == userID) ? $"<b>You</b>" : user;
        //     string action = isAdded ? "added" : "removed";

        //     string color = "white";
        //     switch (category)
        //     {
        //         case "Group A (5-6)": color = "red"; break;
        //         case "Group B (7-8)": color = "blue"; break;
        //         case "Group C (9-10)": color = "green"; break;
        //         case "Random": color = "#FF00FF"; break;
        //     }

        //     string actionString = $"<color={color}>{action} {quantityChanged}</color>";
        //     textComponent.text = $"{dateStr}  |  {timeStr}  | {userFormatted} {actionString} items";
        // }

        private async Task InitializeCategoryData()
        {
            currentIndex = 0;
            // Iterate through the options of the category and give this as an input to the LoadInitialCategoryQuantity method
            foreach (var categoryOption in Enum.GetValues(typeof(Category)))
            {
                string name = categoryOption.ToString();
                int quant = await LoadInitialCategoryQuantityAsync(name);
                Debug.Log($"Category: {name}, Quantity: {quant}");
                currentIndex += quant;
            }
            if (quantityToAdd == 1)
            {
                iDsText.text = $"IDs {currentIndex} ";
            }
            else 
            {
                iDsText.text = $"IDs {currentIndex} - {currentIndex + quantityToAdd -1}";
            }
            
            det_dims.text = $"W: 0.0 x H: 0.0 cm";
            correctWidth.placeholder.GetComponent<TMPro.TMP_Text>().text = "00.00";
            correctHeight.placeholder.GetComponent<TMPro.TMP_Text>().text = "00.00";
            correctWidth.text = "";
            correctHeight.text = "";
            det_group.text = "-->>";
            attempt = 0;
            final_result = null;

        }


        private async Task<int> LoadInitialCategoryQuantityAsync(string name)
        {
            int currentQuantity = 0;

            try
            {
                var snapshot = await dbReference_root.Child("DetectedElements").Child(name).GetValueAsync();

                if (snapshot.Exists && int.TryParse(snapshot.Value.ToString(), out int existingValue))
                {
                    currentQuantity = existingValue;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error occurred while reading data from the database: " + e.Message);
            }

            return currentQuantity;
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
                if (quantityToAdd == 1)
                {
                    iDsText.text = $"IDs {currentIndex} ";
                }
                else 
                {
                    iDsText.text = $"IDs {currentIndex} - {currentIndex + quantityToAdd -1}";
                }
            });
        }

        void SetupCorrectDimsInput()
        {
            correctWidth.placeholder.GetComponent<TMPro.TMP_Text>().text = "00.00";
            correctWidth.contentType = TMPro.TMP_InputField.ContentType.DecimalNumber;
            correctWidth.onValueChanged.AddListener((value) =>
            {

                if (string.IsNullOrWhiteSpace(value) || !float.TryParse(value, out float width))
                {
                    width = float.Parse(correctWidth.placeholder.GetComponent<TMPro.TMP_Text>().text);
                    
                }
                float height = string.IsNullOrWhiteSpace(correctHeight.text) || !float.TryParse(correctHeight.text, out float parsedHeight)
                    ? float.Parse(correctHeight.placeholder.GetComponent<TMPro.TMP_Text>().text)
                    : parsedHeight;

                (Category category, float confidence) = CalculateCategory(new float[] { width, height });
                
                if (final_result != null)
                {
                    final_result.corrected_dimensions[0] = width;
                    final_result.corrected_category = category.ToString(); 
                    det_group.text = GetCategoryTextWithColor(category);  
                }
                RegisterCorrectionAttempt();
            });
            correctHeight.placeholder.GetComponent<TMPro.TMP_Text>().text = "00.00";
            correctHeight.contentType = TMPro.TMP_InputField.ContentType.DecimalNumber;
            correctHeight.onValueChanged.AddListener((value) =>
            {
                if (string.IsNullOrWhiteSpace(value) || !float.TryParse(value, out float height))
                {
                    height = float.Parse(correctHeight.placeholder.GetComponent<TMPro.TMP_Text>().text);
                }

                float width = string.IsNullOrWhiteSpace(correctWidth.text) || !float.TryParse(correctWidth.text, out float parsedHeight)
                    ? float.Parse(correctWidth.placeholder.GetComponent<TMPro.TMP_Text>().text)
                    : parsedHeight;

                (Category category, float confidence) = CalculateCategory(new float[] { width, height });

                //det_group.text = GetCategoryTextWithColor(category);

                if (final_result != null)
                {
                    final_result.corrected_dimensions[1] = height;
                    final_result.corrected_category = category.ToString();
                    det_group.text = GetCategoryTextWithColor(category);
                }
                RegisterCorrectionAttempt();
            });
            
        }


        // //////////////////////////// Material Registration Management Methods //////////////////////////////

        // {
//     "DetectedElements":
//     { "XSmall": 0, "Small": 0, "Medium": 0, "Large": 0, "Rand": 0 },
//     "RegistrationHistory":
//     { "attempt": 0, "userID": "", "quantities": 0, "isRemoved": false, "isCorrected": false, "detected_category": "", "corrected_category": "" , "detected_dims": float[], "corrected_dims" = float[], 
///     "timestamp_received": 0, "timestamp_correction": [], "timestamp_submit": 0 ,"timestamp_removed": 0 }
// }

        public async Task RegisterMaterial(FinalResult result, bool isCorrected = false)
        {
            if (dbReference_root == null || dbReference_elements == null || dbReference_history == null)
            {
                Debug.LogError("dbReferences are null! Make sure Firebase is initialized and the Start method is called.");
                return;
            }

            string selectedCategory = isCorrected ? result.corrected_category : result.detected_category;
            final_result.isCorrected = isCorrected;

            try
            {
                var snapshot = await dbReference_elements.Child(selectedCategory).GetValueAsync();

                int currentQuantity = 0;
                if (snapshot.Exists && int.TryParse(snapshot.Value.ToString(), out int existingValue))
                {
                    currentQuantity = existingValue;
                }

                int updatedQuantity = currentQuantity + result.quantities;

                await dbReference_elements.Child(selectedCategory).SetValueAsync(updatedQuantity);
                Debug.Log($"Quantity updated successfully. {selectedCategory}: {updatedQuantity}");

                // Prepare history entry
                Dictionary<string, object> historyEntry = result.GetData();
                await dbReference_history.Push().SetValueAsync(historyEntry);
                Debug.Log("History entry added successfully.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error in RegisterMaterial: {ex.Message}");
            }
        }


        
        public async Task RemoveLastEntry()
        {
            try
            {
                var snapshot = await dbReference_history
                    .OrderByKey()
                    .LimitToLast(20) // grab a few to work with
                    .GetValueAsync();

                if (!snapshot.Exists)
                {
                    Debug.LogWarning("No history entries found.");
                    return;
                }

                DataSnapshot lastValidEntry = null;
                var childrenList = snapshot.Children.ToList(); // now it's a real List<DataSnapshot>

                // Loop from last to first (latest to oldest)
                foreach (var child in childrenList.AsEnumerable().Reverse())
                {
                    if (child.Child("isRemoved").Value != null)
                    {
                        bool isRemoved = bool.TryParse(child.Child("isRemoved").Value.ToString(), out var result) && result;
                        if (isRemoved)
                            continue;
                    }

                    // Found a non-removed entry
                    lastValidEntry = child;
                    break;
                }

                if (lastValidEntry == null)
                {
                    Debug.LogWarning("No non-removed history entry found.");
                    return;
                }

                string key = lastValidEntry.Key;

                // Extract values
                int quant = 0;
                bool isCorrected = false;
                string detectedCategory = "";
                string correctedCategory = "";

                int.TryParse(lastValidEntry.Child("quantities")?.Value?.ToString(), out quant);
                bool.TryParse(lastValidEntry.Child("isCorrected")?.Value?.ToString(), out isCorrected);
                detectedCategory = lastValidEntry.Child("detected_category")?.Value?.ToString();
                correctedCategory = lastValidEntry.Child("corrected_category")?.Value?.ToString();

                string finalCategory = isCorrected ? correctedCategory : detectedCategory;

                // Update category quantity
                var quantitySnapshot = await dbReference_elements.Child(finalCategory).GetValueAsync();
                int currentQuantity = 0;
                if (quantitySnapshot.Exists && int.TryParse(quantitySnapshot.Value.ToString(), out int existingValue))
                {
                    currentQuantity = existingValue;
                }

                int updatedQuantity = currentQuantity - quant;
                await dbReference_elements.Child(finalCategory).SetValueAsync(updatedQuantity);
                await Task.Delay(5);
                Debug.Log($"Quantity updated successfully. {finalCategory}: {updatedQuantity}");

                // Mark history as removed
                var historyUpdate = new Dictionary<string, object>
                {
                    { "timestamp_removed", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() },
                    { "isRemoved", true },
                    { "userIDremoved", userID }
                };

                await dbReference_history.Child(key).UpdateChildrenAsync(historyUpdate);
                Debug.Log("History entry updated successfully.");
            }
            catch (Exception ex)
            {
                Debug.LogError("Error in RemoveLastEntry: " + ex.Message);
            }
        }


        
        //////////////////////////////////////////// Event Handlers ////////////////////////////////////////////
        public void AddConnectionEventListners()
        {
            /*
            * Method is used to add event listners for the MQTT connection options.
            */
            Debug.Log("Bazingaaa");
            ConnectionSucceeded += SubscribeToCompasXRTopics;
        }
        public void RemoveConnectionEventListners()
        {
            /*
            * Method is used to remove event listners for the MQTT connection options.
            */
            ConnectionSucceeded -= SubscribeToCompasXRTopics;

        }
    }


}
