using Unity.Netcode;
using UnityEngine;

public class ManageMatches : MonoBehaviour
{
    public void ButtonStartHost()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void ButtonStartClient()
    {
        NetworkManager.Singleton.StartClient();
    }
}
