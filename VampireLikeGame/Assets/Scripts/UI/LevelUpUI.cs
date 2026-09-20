using System.Collections.Generic;
using UnityEngine;

namespace VampireLike
{
    public class LevelUpUI : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private List<UpgradeCardUI> cardSlots = new();
        [SerializeField] private UpgradeSystem upgradeSystem;

        private void OnEnable() => UpgradeSystem.OnUpgradeOptionsReady += Show;
        private void OnDisable() => UpgradeSystem.OnUpgradeOptionsReady -= Show;

        private void Awake()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        private void Show(List<UpgradeData> options)
        {
            if (panelRoot != null) panelRoot.SetActive(true);

            for (int i = 0; i < cardSlots.Count; i++)
            {
                if (i < options.Count)
                {
                    cardSlots[i].gameObject.SetActive(true);
                    cardSlots[i].Setup(options[i], HandleChosen);
                }
                else
                {
                    cardSlots[i].gameObject.SetActive(false);
                }
            }
        }

        private void HandleChosen(UpgradeData data)
        {
            if (panelRoot != null) panelRoot.SetActive(false);
            upgradeSystem.ApplyUpgrade(data);
        }
    }
}
