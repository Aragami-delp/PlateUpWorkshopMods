using Kitchen;
using KitchenSmartNoClip;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace KitchenPhoneIndicators
{
    public class PhoneIndicatorsMono : MonoBehaviour
    {
        public static PhoneIndicatorsMono Instance { get; private set; }

        private void Start()
        {
            if (Instance != null)
            {
                return;
            }
            Instance = this;
            DontDestroyOnLoad(this); // this.gameobject?
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                PhoneIndicators.LogWarning("Input K Works");
                using (NativeArray<CScheduledCustomer> componentDataArray = PhoneIndicators.ScheduledCustomers.ToComponentDataArray<CScheduledCustomer>(Allocator.Temp))
                {

                    string retVal = string.Join("|", componentDataArray.Select(x => x.TimeOfDay));
                    PhoneIndicators.LogWarning(retVal);
                }
            }
        }
    }
}
