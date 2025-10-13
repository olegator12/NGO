using Unity.Netcode.Components;
using UnityEngine;

namespace Utils.Net
{
    [DisallowMultipleComponent]
    public class ClientAutNetworkTransform : NetworkTransform
    {
        protected override bool OnIsServerAuthoritative()
        {
            return false;
        }
    }
}