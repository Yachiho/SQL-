using UnityEngine;

namespace VampireLike
{
    /// <summary>
    /// Fan-work flavor layer: a playable idol's name and Vocal/Dance/Visual
    /// leaning. No official art, audio or voice data is referenced here —
    /// only free-text fields you fill in yourself. Keep this project
    /// personal-use / non-distributed if you put real character names in it.
    /// </summary>
    [CreateAssetMenu(menuName = "VampireLike/Idol", fileName = "NewIdol")]
    public class IdolData : ScriptableObject
    {
        public string idolName = "Idol";
        [TextArea] public string catchphrase = "";
        public Color themeColor = Color.white;

        [Header("Vo/Da/Vi (0-100, just a gameplay leaning - not official data)")]
        [Range(0, 100)] public int vocal = 50;
        [Range(0, 100)] public int dance = 50;
        [Range(0, 100)] public int visual = 50;
    }
}
