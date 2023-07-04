using System.Collections.Generic;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;

namespace Unity.FPS.UI
{
    public class WeaponHUDManager : MonoBehaviour
    {
        [Tooltip("UI panel containing the layoutGroup for displaying weapon ammo")]
        public RectTransform AmmoPanel;

        [Tooltip("Prefab for displaying weapon ammo")]
        public GameObject AmmoCounterPrefab;

        PlayerWeaponsManagerBeta m_PlayerWeaponsManager;
        List<AmmoCounter> m_AmmoCounters = new List<AmmoCounter>();

        void Start()
        {
            m_PlayerWeaponsManager = FindObjectOfType<PlayerWeaponsManagerBeta>();
            DebugUtility.HandleErrorIfNullFindObject<PlayerWeaponsManagerBeta, WeaponHUDManager>(m_PlayerWeaponsManager,
                this);

            WeaponController activeWeapon = m_PlayerWeaponsManager.GetActiveWeapon();
            //if (activeWeapon)
            //{
            //    AddWeaponUI(activeWeapon, m_PlayerWeaponsManager.ActiveWeaponIndex);
            //    ChangeWeapon(activeWeapon);
            //    //MarkLayoutForRebuild
            //}

            //m_PlayerWeaponsManager.OnAddedWeapon += AddWeaponUI;
            //m_PlayerWeaponsManager.OnRemovedWeapon += RemoveWeapon;
            //m_PlayerWeaponsManager.OnSwitchedToWeapon += ChangeWeapon;
        }

        void AddWeaponUI(WeaponController newWeapon, int weaponIndex)
        {
            GameObject ammoCounterInstance = Instantiate(AmmoCounterPrefab, AmmoPanel);
            AmmoCounter newAmmoCounter = ammoCounterInstance.GetComponent<AmmoCounter>();
            DebugUtility.HandleErrorIfNullGetComponent<AmmoCounter, WeaponHUDManager>(newAmmoCounter, this,
                ammoCounterInstance.gameObject);

            newAmmoCounter.Initialize(newWeapon, weaponIndex);

            m_AmmoCounters.Add(newAmmoCounter);
        }

        void RemoveWeapon(WeaponController newWeapon, int weaponIndex)//典型形式上的作用
        {
            int foundCounterIndex = -1;
            for (int i = 0; i < m_AmmoCounters.Count; i++)
            {
                if (m_AmmoCounters[i].WeaponCounterIndex == weaponIndex)
                {
                    foundCounterIndex = i;
                    Destroy(m_AmmoCounters[i].gameObject);
                }
            }

            if (foundCounterIndex >= 0)
            {
                m_AmmoCounters.RemoveAt(foundCounterIndex);
            }
        }

        void ChangeWeapon(WeaponController weapon)
        {
            //UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(AmmoPanel);
            UnityEngine.UI.LayoutRebuilder.MarkLayoutForRebuild(AmmoPanel);
        }
    }
}