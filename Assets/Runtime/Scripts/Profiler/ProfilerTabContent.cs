using UnityEngine;

namespace SmartDebugger
{
    public class ProfilerTabContent : MainTabContent
    {
        [SerializeField] private GameObject _cpuGraph;
        [SerializeField] private GameObject _memoryGraph;
        [SerializeField] private Transform _cpuIcon;
        [SerializeField] private Transform _memoryIcon;

        public void OnToggleCpu(bool isOn)
        {
            _cpuGraph.SetActive(isOn);
            _cpuIcon.localEulerAngles = Vector3.forward * (isOn ? 0 : 90);
        }

        public void OnToggleMemory(bool isOn)
        {
            _memoryGraph.SetActive(isOn);
            _memoryIcon.localEulerAngles = Vector3.forward * (isOn ? 0 : 90);
        }
    }
}