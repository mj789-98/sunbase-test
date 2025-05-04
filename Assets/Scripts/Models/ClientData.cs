using System;
using UnityEngine;

namespace SunbaseTest.Models
{
    /// <summary>
    /// Data model for client information received from the API
    /// </summary>
    [Serializable]
    public class ClientData
    {
        [SerializeField] private int id;
        [SerializeField] private string label;
        [SerializeField] private bool isManager;
        
        // These fields are not directly in the client object but will be populated from the data object
        private string name;
        private int points;
        private string address;

        public int Id => id;
        public string Label => label;
        public bool IsManager => isManager;
        
        // Additional properties from data object
        public string Name { get => name; set => name = value; }
        public int Points { get => points; set => points = value; }
        public string Address { get => address; set => address = value; }

        /// <summary>
        /// Populates additional data from the data object
        /// </summary>
        public void PopulateAdditionalData(ClientDetailData detailData)
        {
            if (detailData != null)
            {
                this.name = detailData.Name;
                this.points = detailData.Points;
                this.address = detailData.Address;
            }
        }
    }

    /// <summary>
    /// Model for client detail data from the data object
    /// </summary>
    [Serializable]
    public class ClientDetailData
    {
        [SerializeField] private string name;
        [SerializeField] private int points;
        [SerializeField] private string address;

        public string Name => name;
        public int Points => points;
        public string Address => address;
    }
} 