using Kitchen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
        private bool m_isPrepTime = false;
        private List<Transform> m_phoneIndicators = new();

        private void Start()
        {
            if (Instance != null)
            {
                return;
            }
            Instance = this;
            DontDestroyOnLoad(this); // this.gameobject?
        }

        public void PlayerView_Update_Prefix()
        {
            // Check for any kind of data change and execute noclip update
            if (GameInfo.IsPreparationTime != m_isPrepTime) // TODO: No practice mode
            {
                m_isPrepTime = GameInfo.IsPreparationTime;
                // Switched to Day
                if (!m_isPrepTime)
                {
                    RemoveIndicators(); // To be safe
                    CreatePhoneIndicatorsOnUI();
                }
                else
                {
                    RemoveIndicators();
                }
            }
        }

        private const float LOCALPOSMAX = 2.5f;

        private void CreatePhoneIndicatorsOnUI([CallerMemberName] string _callerName = "")
        {
            PhoneIndicators.LogInfo($"Setting Phone Indicators called by {_callerName}");
            using (NativeArray<CScheduledCustomer> componentDataArray = PhoneIndicators.ScheduledCustomers.ToComponentDataArray<CScheduledCustomer>(Allocator.Temp))
            {
                if (componentDataArray.Length == 0) { return; }
                List<float> timesOfDay = componentDataArray.Select(x => x.TimeOfDay).ToList();
                
                if (timesOfDay.Count > 30) { return; } // Why would you want to use the mod with more than 30 groups?! -- That might already be too much

#if DEBUG
                PhoneIndicators.LogWarning(string.Join("|", timesOfDay)); // Correct
#endif

                Transform indicatorHolder = new GameObject("PhoneIndicatorHolder").transform;
                GameObject tdv = FindObjectOfType<Kitchen.TimeDisplayView>().gameObject;
                // Always search for a new one in case one gets deleted
                indicatorHolder.SetParent(tdv.GetChild("Time").transform, false);
                // Use Prep Bar as prefab for indicators
                Transform prepbar = tdv.GetChild("Time/Prep Bar").transform;

                timesOfDay.Remove(timesOfDay.Min()); // First group always comes at the start of day or immediatly at the end of extra prep time
                foreach (float customerGroupTime in timesOfDay)
                {
                    if (customerGroupTime == 0.2f || customerGroupTime == 0.5f || customerGroupTime == 0.8f) { continue; } // Rush customers have the rush symbol already
                    Transform newIndicator = Instantiate(prepbar, indicatorHolder); // TODO: Pooling
                    newIndicator.localScale = new Vector3(0.03f, 0.5f, 1);
                    newIndicator.localPosition = new Vector3(Mathf.Lerp(-LOCALPOSMAX, LOCALPOSMAX, customerGroupTime), newIndicator.localPosition.y, newIndicator.localPosition.z);
                    newIndicator.GetComponent<Shapes.Rectangle>().Color = new Color(1.21f, 1.0771f, 0.5552f, 0.5f); // Same colour as day progress bar, but less glowy
                    newIndicator.gameObject.SetActive(true);

                    m_phoneIndicators.Add(newIndicator);
                }
            }
        }

        private void RemoveIndicators()
        {
            if (m_phoneIndicators.Count > 0)
            {
                foreach (Transform existingIndicator in m_phoneIndicators)
                {
                    // They might not exist when switching restaurants
                    if (existingIndicator) { GameObject.Destroy(existingIndicator.gameObject); } // TODO: Pooling
                }
                m_phoneIndicators.Clear();
            }
        }
    }
}
