using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    private int coinCount = 5;
    // Start is called before the first frame update
    [SerializeField]private GameObject coinPrefab;
    void Start()
    {
        
        //Tache
        NetworkManager.Singleton.OnClientConnectedCallback += (id) =>
        {
            Debug.Log("A new client connected, id = " + id);
        };
        NetworkManager.Singleton.OnClientDisconnectCallback += (id) =>
        {
            Debug.Log("A Client disconnect, id = " + id);
        };
        NetworkManager.Singleton.OnServerStarted += () =>
        {
            Debug.Log("Server started");
            CreateCoins();
        };
    }

    private void CreateCoins()
    {
        for (int i = 0; i < coinCount; i++)
        {
            GameObject ob =Instantiate(coinPrefab, new Vector3(Random.Range(-10, 10), 5f, Random.Range(-10, 10)), Quaternion.identity);
            ob.GetComponent<NetworkObject>().Spawn();
        }
    }
    //Button
    public void OnStartServerBtnClick()
    {
        if (NetworkManager.Singleton.StartServer())
        {
            Debug.Log("Start Server succes");
        }
        else
        {
            Debug.Log("Start Server fail");
        }
    }
    public void OnStartClientBtnClick()
    {
        if (NetworkManager.Singleton.StartClient())
        {
            Debug.Log("Start Client succes");
        }
        else
        {
            Debug.Log("Start Client fail");
        }
    }
    public void OnStartHostBtnClick()
    {
        if (NetworkManager.Singleton.StartHost())
        {
            Debug.Log("Start Host succes");
        }
        else
        {
            Debug.Log("Start Host fail");
        }
    }
    public void OnShutdownNetworkBtnClick()
    {
            NetworkManager.Singleton.Shutdown();
            Debug.Log("Shutdown network");

    }

}
