using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UserLogin
{
    public class AvatarIcon : MonoBehaviour
    {
        public Image avatar;

        public UnityAction<AvatarIcon> onClick;

        private AvatarData data;

        private void Start()
        {
            Button btn = GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(OnClick);
        }

        public void SetAvatar(AvatarData avat)
        {
            data = avat;
            avatar.sprite = avat.avatar;
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            data = null;
            gameObject.SetActive(false);
        }

        public AvatarData GetData()
        {
            return data;
        }

        private void OnClick()
        {
            onClick?.Invoke(this);
        }
    }
}
