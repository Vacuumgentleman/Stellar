using UnityEngine;
using System.Collections.Generic;

namespace bullets
{
    public class WeaponController : MonoBehaviour
    {
        [System.Serializable]
        public class GunSlot
        {
            public Guns gun;
            public WeaponType type;
            public Side side;
        }

        public enum WeaponType { Rapid, Heavy }
        public enum Side { Left, Right }

        public List<GunSlot> weapons = new List<GunSlot>();
        private int currentMode = 0;
        private const int maxModes = 5;
        /*
        private void Update()
        {
            HandleWeaponSelection();

            if (Input.GetMouseButtonDown(0))
            {
                Vector3 target = GetTargetPoint();

                foreach (var slot in weapons)
                {
                    if slot.gun.Shoot(target);
                }
            }
        }
        */
        private void HandleWeaponSelection()
        {
            if (Input.mouseScrollDelta.y > 0)
                currentMode = (currentMode + 1) % maxModes;
            else if (Input.mouseScrollDelta.y < 0)
                currentMode = (currentMode - 1 + maxModes) % maxModes;

            if (Input.GetKeyDown(KeyCode.Alpha0)) currentMode = 0;
            if (Input.GetKeyDown(KeyCode.Alpha1)) currentMode = 1;
            if (Input.GetKeyDown(KeyCode.Alpha2)) currentMode = 2;
            if (Input.GetKeyDown(KeyCode.Alpha3)) currentMode = 3;
            if (Input.GetKeyDown(KeyCode.Alpha4)) currentMode = 4;
        }

        private bool ShouldFire(GunSlot slot)
        {
            switch (currentMode)
            {
                case 0: return true; // Todas
                case 1: return slot.type == WeaponType.Rapid;
                case 2: return slot.type == WeaponType.Heavy;
                case 3: return slot.side == Side.Left;
                case 4: return slot.side == Side.Right;
                default: return true;
            }
        }

        private Vector3 GetTargetPoint()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
                return hit.point;

            return transform.position + transform.forward * 50f;
        }
    }
}
