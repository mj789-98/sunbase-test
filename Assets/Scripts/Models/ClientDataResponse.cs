using System;
using System.Collections.Generic;
using UnityEngine;

namespace SunbaseTest.Models
{
    /// <summary>
    /// Wrapper class for API response containing client data
    /// </summary>
    [Serializable]
    public class ClientDataResponse
    {
        [SerializeField] private List<ClientData> clients;
        // We won't use serialization for the data dictionary - we'll handle it manually
        private Dictionary<string, ClientDetailData> dataDict;
        [SerializeField] private string label;
        
        public List<ClientData> Clients => clients;
        public string Label => label;
        
        /// <summary>
        /// Processes the response to combine client data with detail data
        /// </summary>
        public List<ClientData> GetProcessedClientData()
        {
            if (clients == null || dataDict == null)
            {
                Debug.LogWarning("Client list or data dictionary is null");
                return clients;
            }
                
            foreach (var client in clients)
            {
                string clientId = client.Id.ToString();
                if (dataDict.TryGetValue(clientId, out ClientDetailData detailData))
                {
                    Debug.Log($"Found detail data for client {clientId}: {detailData.Name}, {detailData.Points}, {detailData.Address}");
                    client.PopulateAdditionalData(detailData);
                }
                else
                {
                    Debug.LogWarning($"No detail data found for client ID: {clientId}");
                }
            }
            
            return clients;
        }
        
        /// <summary>
        /// Manual method to parse the JSON data object
        /// </summary>
        public void ParseDataDictionary(string jsonText)
        {
            dataDict = new Dictionary<string, ClientDetailData>();
            
            try
            {
                // Parse the whole JSON to find and extract the data object
                int dataStart = jsonText.IndexOf("\"data\":");
                if (dataStart == -1)
                {
                    Debug.LogError("No data object found in JSON");
                    return;
                }
                
                // Find start of the data object
                dataStart = jsonText.IndexOf('{', dataStart);
                if (dataStart == -1) return;
                
                // Find end of the data object - need to match braces
                int braceCount = 1;
                int dataEnd = -1;
                
                for (int i = dataStart + 1; i < jsonText.Length; i++)
                {
                    if (jsonText[i] == '{') braceCount++;
                    else if (jsonText[i] == '}')
                    {
                        braceCount--;
                        if (braceCount == 0)
                        {
                            dataEnd = i;
                            break;
                        }
                    }
                }
                
                if (dataEnd == -1) return;
                
                // Extract data object
                string dataJson = jsonText.Substring(dataStart, dataEnd - dataStart + 1);
                
                // Parse entries in data object
                ParseDataEntries(dataJson);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error parsing data dictionary: {e.Message}");
            }
        }
        
        private void ParseDataEntries(string dataJson)
        {
            // Split up the data object by client ID entries
            int pos = 0;
            while (true)
            {
                // Find key/ID
                int keyStart = dataJson.IndexOf('"', pos);
                if (keyStart == -1) break;
                
                int keyEnd = dataJson.IndexOf('"', keyStart + 1);
                if (keyEnd == -1) break;
                
                string key = dataJson.Substring(keyStart + 1, keyEnd - keyStart - 1);
                
                // Skip non-numeric keys (not client IDs)
                if (!IsNumeric(key))
                {
                    pos = keyEnd + 1;
                    continue;
                }
                
                // Find value object start
                int valueStart = dataJson.IndexOf('{', keyEnd);
                if (valueStart == -1) break;
                
                // Find value object end (matching braces)
                int braceCount = 1;
                int valueEnd = -1;
                
                for (int i = valueStart + 1; i < dataJson.Length; i++)
                {
                    if (dataJson[i] == '{') braceCount++;
                    else if (dataJson[i] == '}')
                    {
                        braceCount--;
                        if (braceCount == 0)
                        {
                            valueEnd = i;
                            break;
                        }
                    }
                }
                
                if (valueEnd == -1) break;
                
                // Extract and parse the client detail object
                string valueJson = dataJson.Substring(valueStart, valueEnd - valueStart + 1);
                try
                {
                    // Create a wrapper to parse with JsonUtility
                    string wrappedJson = "{\"value\":" + valueJson + "}";
                    DetailWrapper wrapper = JsonUtility.FromJson<DetailWrapper>(wrappedJson);
                    
                    if (wrapper != null && wrapper.value != null)
                    {
                        dataDict[key] = wrapper.value;
                        Debug.Log($"Parsed client {key} details: {wrapper.value.Name}, {wrapper.value.Points}, {wrapper.value.Address}");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error parsing client {key} details: {e.Message}");
                }
                
                // Move position for next iteration
                pos = valueEnd + 1;
            }
        }
        
        private bool IsNumeric(string str)
        {
            foreach (char c in str)
            {
                if (!char.IsDigit(c)) return false;
            }
            return str.Length > 0;
        }
        
        [Serializable]
        private class DetailWrapper
        {
            public ClientDetailData value;
        }
    }
} 