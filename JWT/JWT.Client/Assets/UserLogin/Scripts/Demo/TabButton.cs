using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace UserLogin.Demo
{

    public class TabButton : MonoBehaviour
    {
        public string group;
        public bool active;
        public Image highlight;

        public UnityAction onClick;

        private static List<TabButton> tab_list = new List<TabButton>();

        private void Awake()
        {
            tab_list.Add(this);
        }

        private void OnDestroy()
        {
            tab_list.Remove(this);
        }

        void Start()
        {
            Button button = GetComponent<Button>();
            if(button != null)
                button.onClick.AddListener(OnClick);
        }

        void Update()
        {
            if (highlight != null)
                highlight.enabled = active;
        }

        private void OnClick()
        {
            SetAll(group, false);
            active = true;
            onClick?.Invoke();
        }

        public void Activate()
        {
            SetAll(group, false);
            active = true;
        }

        public static void SetAll(string group, bool act)
        {
            foreach (TabButton btn in tab_list)
            {
                if (btn.group == group)
                    btn.active = act;
            }
        }

        public static List<TabButton> GetAll(string group)
        {
            List<TabButton> glist = new List<TabButton>();
            foreach (TabButton btn in tab_list)
            {
                if (btn.group == group)
                    glist.Add(btn);
            }
            return glist;
        }

        public static List<TabButton> GetAll()
        {
            return tab_list;
        }
    }
}
