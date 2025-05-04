using System.Collections.Generic;
using UnityEngine;
using TMPro;
using SunbaseTest.Models;
using SunbaseTest.API;
using System.Linq;
using UnityEngine.UI;

namespace SunbaseTest.UI
{
    /// <summary>
    /// Manager for the client list with filtering functionality
    /// </summary>
    public class ClientListManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ClientDataService clientDataService;
        [SerializeField] private Transform clientListContainer;
        [SerializeField] private ClientListItem clientItemPrefab;
        [SerializeField] private TMP_Dropdown filterDropdown;
        [SerializeField] private ClientDetailPopup detailPopup;
        [SerializeField] private GameObject loadingPanel;
        [SerializeField] private GameObject errorPanel;
        [SerializeField] private TextMeshProUGUI errorText;
        [SerializeField] private Button retryButton;

        [Header("Animation")]
        [SerializeField] private float itemAnimationDelay = 0.05f;

        private List<ClientData> allClients = new List<ClientData>();
        private List<ClientListItem> clientListItems = new List<ClientListItem>();
        private ClientData selectedClient;
        private ClientFilter currentFilter = ClientFilter.All;

        // Filter options enum
        private enum ClientFilter
        {
            All,
            ManagersOnly,
            NonManagers
        }

        private void Awake()
        {
            // Setup event listeners
            if (clientDataService == null)
            {
                clientDataService = FindObjectOfType<ClientDataService>();
            }

            if (clientDataService != null)
            {
                clientDataService.OnClientDataFetched += HandleClientDataFetched;
                clientDataService.OnError += HandleError;
            }
            else
            {
                Debug.LogError("ClientDataService not found!");
            }

            // Setup filter dropdown
            SetupFilterDropdown();

            // Setup retry button
            if (retryButton != null)
            {
                retryButton.onClick.AddListener(RetryFetchData);
            }

            // Initialize UI state
            ShowLoading(true);
            ShowError(false, string.Empty);
        }

        private void OnDestroy()
        {
            // Clean up event listeners
            if (clientDataService != null)
            {
                clientDataService.OnClientDataFetched -= HandleClientDataFetched;
                clientDataService.OnError -= HandleError;
            }

            if (retryButton != null)
            {
                retryButton.onClick.RemoveListener(RetryFetchData);
            }
        }

        /// <summary>
        /// Sets up the filter dropdown with options
        /// </summary>
        private void SetupFilterDropdown()
        {
            if (filterDropdown != null)
            {
                filterDropdown.ClearOptions();
                
                List<string> options = new List<string>
                {
                    "All Clients",
                    "Managers Only",
                    "Non-Managers"
                };
                
                filterDropdown.AddOptions(options);
                filterDropdown.onValueChanged.AddListener(OnFilterChanged);
                
                // Set current filter
                filterDropdown.value = (int)currentFilter;
            }
        }

        /// <summary>
        /// Handles client data fetched from API
        /// </summary>
        private void HandleClientDataFetched(List<ClientData> clientDataList)
        {
            ShowLoading(false);
            ShowError(false, string.Empty);
            
            if (clientDataList != null && clientDataList.Count > 0)
            {
                allClients = clientDataList;
                PopulateClientList();
            }
            else
            {
                ShowError(true, "No client data received");
            }
        }

        /// <summary>
        /// Handles error in fetching client data
        /// </summary>
        private void HandleError(string errorMessage)
        {
            ShowLoading(false);
            ShowError(true, errorMessage);
        }

        /// <summary>
        /// Retries fetching data from API
        /// </summary>
        private void RetryFetchData()
        {
            ShowLoading(true);
            ShowError(false, string.Empty);
            
            if (clientDataService != null)
            {
                clientDataService.FetchClientData();
            }
        }

        /// <summary>
        /// Populates the client list based on current filter
        /// </summary>
        private void PopulateClientList()
        {
            // Clear existing items
            ClearClientList();
            
            // Get filtered clients
            List<ClientData> filteredClients = FilterClients(allClients, currentFilter);
            
            // Create new items
            float delay = 0f;
            foreach (var client in filteredClients)
            {
                ClientListItem item = Instantiate(clientItemPrefab, clientListContainer);
                item.Setup(client);
                item.OnItemClicked += OnClientItemClicked;
                item.AnimateEntry(delay);
                
                clientListItems.Add(item);
                delay += itemAnimationDelay;
            }
        }

        /// <summary>
        /// Clears all client list items
        /// </summary>
        private void ClearClientList()
        {
            // Remove event listeners from existing items
            foreach (var item in clientListItems)
            {
                if (item != null)
                {
                    item.OnItemClicked -= OnClientItemClicked;
                }
            }
            
            // Clear the container
            foreach (Transform child in clientListContainer)
            {
                Destroy(child.gameObject);
            }
            
            clientListItems.Clear();
        }

        /// <summary>
        /// Filters clients based on selected filter
        /// </summary>
        private List<ClientData> FilterClients(List<ClientData> clients, ClientFilter filter)
        {
            switch (filter)
            {
                case ClientFilter.ManagersOnly:
                    return clients.Where(c => c.IsManager).ToList();
                
                case ClientFilter.NonManagers:
                    return clients.Where(c => !c.IsManager).ToList();
                
                case ClientFilter.All:
                default:
                    return clients;
            }
        }

        /// <summary>
        /// Handles filter dropdown selection change
        /// </summary>
        private void OnFilterChanged(int filterIndex)
        {
            currentFilter = (ClientFilter)filterIndex;
            PopulateClientList();
        }

        /// <summary>
        /// Handles client item click
        /// </summary>
        private void OnClientItemClicked(ClientData client)
        {
            // Show detail popup regardless of selection state
            if (detailPopup != null)
            {
                detailPopup.Show(client);
            }
            
            // Update selection state
            selectedClient = client;
            
            // Update UI selection state
            foreach (var item in clientListItems)
            {
                item.SetSelected(item.GetClientData() == client);
            }
        }

        /// <summary>
        /// Shows or hides loading UI
        /// </summary>
        private void ShowLoading(bool show)
        {
            if (loadingPanel != null)
            {
                loadingPanel.SetActive(show);
            }
        }

        /// <summary>
        /// Shows or hides error UI with message
        /// </summary>
        private void ShowError(bool show, string message)
        {
            if (errorPanel != null)
            {
                errorPanel.SetActive(show);
                
                if (errorText != null)
                {
                    errorText.text = message;
                }
            }
        }
    }
} 