using System.Collections.Generic;
using UnityEngine;

namespace Spelunx
{
    // An interface for all DebugKey components, to make finding them in the scene easier
    public interface IDebugKeys
    {
        public List<(string Key, string Description)> KeyDescriptions();
    }
}
