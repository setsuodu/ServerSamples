using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UserLogin
{
    [CreateAssetMenu(fileName = "AvatarData", menuName = "UserLogin/AvatarData", order = 5)]
    public class AvatarData : ScriptableObject
    {
        public string id;
        public Sprite avatar;

        private static List<AvatarData> avatar_list = new List<AvatarData>();

        public static void Load(string folder = "")
        {
            avatar_list.Clear();
            avatar_list.AddRange(Resources.LoadAll<AvatarData>(folder));
        }

        public static AvatarData Get(string id)
        {
            foreach (AvatarData avat in avatar_list)
            {
                if (avat.id == id)
                    return avat;
            }
            return GetDefault();
        }

        public static AvatarData GetDefault()
        {
            foreach (AvatarData avat in avatar_list)
            {
                if (string.IsNullOrEmpty(avat.id))
                    return avat;
            }
            return null;
        }

        public static List<AvatarData> GetAll()
        {
            return avatar_list;
        }
    }
}
