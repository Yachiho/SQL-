using System;
using UnityEngine;
using UnityEngine.UI;

namespace VampireLike
{
    public class UpgradeCardUI : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private Text titleText;
        [SerializeField] private Text descriptionText;
        [SerializeField] private Button button;

        private UpgradeData _data;
        private Action<UpgradeData> _onChosen;

        private void Awake()
        {
            if (button != null) button.onClick.AddListener(HandleClicked);
        }

        public void Setup(UpgradeData data, Action<UpgradeData> onChosen)
        {
            _data = data;
            _onChosen = onChosen;

            if (icon != null) icon.sprite = data.icon;
            if (titleText != null) titleText.text = data.displayName;
            if (descriptionText != null) descriptionText.text = data.description;
        }

        private void HandleClicked() => _onChosen?.Invoke(_data);
    }
}
