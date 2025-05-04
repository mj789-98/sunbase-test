using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using SunbaseTest.Models;
using System.Collections.Generic;

namespace SunbaseTest.API
{
    /// <summary>
    /// Service for fetching client data from the API
    /// </summary>
    public class ClientDataService : MonoBehaviour
    {
        [SerializeField] private string apiUrl = "https://qa.sunbasedata.com/sunbase/portal/api/assignment.jsp?cmd=client_data";
        [SerializeField] private int maxRetryAttempts = 3;
        [SerializeField] private float retryDelay = 2f;
        [SerializeField] private bool useJsonFile = false;
        [SerializeField] private TextAsset fallbackJsonFile;

        /// <summary>
        /// Event triggered when client data is successfully fetched
        /// </summary>
        public event Action<List<ClientData>> OnClientDataFetched;
        
        /// <summary>
        /// Event triggered when there's an error fetching client data
        /// </summary>
        public event Action<string> OnError;

        private void Start()
        {
            FetchClientData();
        }

        /// <summary>
        /// Initiates the process to fetch client data from the API
        /// </summary>
        public void FetchClientData()
        {
            if (useJsonFile && fallbackJsonFile != null)
            {
                ProcessJsonData(fallbackJsonFile.text);
            }
            else
            {
                StartCoroutine(FetchClientDataCoroutine(0));
            }
        }

        /// <summary>
        /// Coroutine for fetching client data with retry mechanism
        /// </summary>
        private IEnumerator FetchClientDataCoroutine(int retryCount)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
            {
                // Send the request and wait for response
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string jsonResponse = request.downloadHandler.text;
                    ProcessJsonData(jsonResponse);
                }
                else
                {
                    // Handle error with retry mechanism
                    string errorMessage = $"Error fetching client data: {request.error}";
                    Debug.LogError(errorMessage);
                    
                    // Retry if not reached max attempts
                    if (retryCount < maxRetryAttempts)
                    {
                        Debug.LogWarning($"Retrying ({retryCount + 1}/{maxRetryAttempts}) after {retryDelay} seconds");
                        yield return new WaitForSeconds(retryDelay);
                        StartCoroutine(FetchClientDataCoroutine(retryCount + 1));
                    }
                    else
                    {
                        if (fallbackJsonFile != null)
                        {
                            Debug.Log("Using fallback JSON file after API failure");
                            ProcessJsonData(fallbackJsonFile.text);
                        }
                        else
                        {
                            OnError?.Invoke(errorMessage);
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Processes JSON data from either API or fallback file
        /// </summary>
        private void ProcessJsonData(string jsonData)
        {
            try
            {
                Debug.Log("API Response: " + jsonData);
                
                // First, deserialize the basic structure with clients array
                ClientDataResponse responseWrapper = JsonUtility.FromJson<ClientDataResponse>(jsonData);
                
                // Then manually parse the data dictionary part
                responseWrapper.ParseDataDictionary(jsonData);
                
                if (responseWrapper != null && responseWrapper.Clients != null)
                {
                    // Process the data to combine clients with their detailed information
                    List<ClientData> processedClients = responseWrapper.GetProcessedClientData();
                    
                    if (processedClients != null && processedClients.Count > 0)
                    {
                        OnClientDataFetched?.Invoke(processedClients);
                    }
                    else
                    {
                        string errorMessage = "No valid client data in response";
                        Debug.LogError(errorMessage);
                        OnError?.Invoke(errorMessage);
                    }
                }
                else
                {
                    string errorMessage = "Failed to parse client data response";
                    Debug.LogError(errorMessage);
                    OnError?.Invoke(errorMessage);
                }
            }
            catch (Exception e)
            {
                string errorMessage = $"Error parsing JSON: {e.Message}";
                Debug.LogError(errorMessage);
                OnError?.Invoke(errorMessage);
            }
        }
    }
} 