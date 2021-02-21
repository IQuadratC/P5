using TMPro;
using UnityEngine;
using Utility.Variables;

namespace UI.Settings
{
    public class SettingsPage : MonoBehaviour
    {
    
        [SerializeField] private TMP_InputField ipInputField;
        [SerializeField] public StringVariable ip;
    
        [SerializeField] private TMP_InputField nickInputField;
        [SerializeField] public StringVariable nick;
    
        private void OnEnable()
        {
            ipInputField.text = ip.Value;
            nickInputField.text = nick.Value;
        }

        private void OnDisable()
        {
            ip.Value = ipInputField.text;
            nick.Value = nickInputField.text;
        }
    }
}
