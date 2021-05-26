using UnityEngine;
using Utility.Variables;

namespace UI
{
    public class FakeTCP : MonoBehaviour
    {
        [SerializeField]private StringVariable msg;
        public void sendMsg()
        {
            Debug.Log(msg.Value);
        }
    }
}
