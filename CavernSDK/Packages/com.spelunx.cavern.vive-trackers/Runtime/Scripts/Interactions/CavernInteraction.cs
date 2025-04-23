using UnityEngine;

namespace Spelunx.Vive {
    public abstract class CavernInteraction : Interaction {
        [SerializeField] protected CavernRenderer cavernRenderer = null;

        public CavernRenderer GetCavernRenderer() { return cavernRenderer; }
        public void SetCavernRenderer(CavernRenderer cavernRenderer) { this.cavernRenderer = cavernRenderer; }
    }
}