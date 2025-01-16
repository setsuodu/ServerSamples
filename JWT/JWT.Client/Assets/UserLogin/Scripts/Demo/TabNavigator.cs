using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UserLogin.Demo
{
    public class TabNavigator : MonoBehaviour
    {
        public TabNavigator next;

        private static float timer = 0f;

        void Update()
        {
            timer += Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                TabNavigator current = EventSystem.current.currentSelectedGameObject.GetComponent<TabNavigator>();
                if (current != null && current == this && timer > 0f)
                {
                    timer = -0.2f;
                    EventSystem.current.SetSelectedGameObject(next.gameObject, new BaseEventData(EventSystem.current));
                }
            }
        }
    }
}