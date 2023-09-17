using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Player : NetworkBehaviour
{
    private Rigidbody rb;

    [SerializeField] private float moveSpeed = 10;
    [SerializeField] private float turnSpeed = 100;
    [SerializeField] private Text nameLabel;

    private Color[] playerColors = {Color.red, Color.blue, Color.green, Color.gray, Color.yellow};
    
    //Network variable
    private NetworkVariable<Vector3> networkPlayerPos = new NetworkVariable<Vector3>(Vector3.zero);
    private NetworkVariable<Quaternion> networkPlayerRot = new NetworkVariable<Quaternion>(Quaternion.identity);

    private NetworkVariable<int> clientId = new NetworkVariable<int>();
    // Start is called before the first frame update
    void Start()
    {
        rb = this.GetComponent<Rigidbody>();
        if (this.IsClient && this.IsOwner)
        {
            transform.position = new Vector3(Random.Range(-5, 5), 0.5f, Random.Range(-5, 5));
        }

        nameLabel.text = clientId.Value.ToString();

        Renderer renderer = GetComponent<Renderer>();
        renderer.material.color = playerColors[clientId.Value % playerColors.Length];

    }

    public override void OnNetworkSpawn()
    {
        if (this.IsServer)
        {
            clientId.Value = (int) this.OwnerClientId;
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (this.IsClient && this.IsOwner)
        {
            float v = Input.GetAxis("Vertical");
            float h = Input.GetAxis("Horizontal");

            Vector3 pos = GetTargetPos(v);
            Quaternion rot = GetTargetRot(h);
            
            //Update Network variable
            UpdatePosAndRotServerRpc(pos,rot);
            
            //Move
            Move(pos);
            Turn(rot);
        }
        else
        {
            Move(networkPlayerPos.Value);
            Turn(networkPlayerRot.Value);
        }
    }

    [ServerRpc]
    public void UpdatePosAndRotServerRpc(Vector3 pos, Quaternion rot)
    {
        networkPlayerPos.Value = pos;
        networkPlayerRot.Value = rot;
    }

    private Vector3 GetTargetPos(float v)
    {
        Vector3 delta = this.transform.forward * v * moveSpeed * Time.deltaTime;
        Vector3 pos = rb.position + delta;
        return pos;
    }

    private void Move(Vector3 pos)
    {
        rb.MovePosition(pos);
    }

    private Quaternion GetTargetRot(float h)
    {
        Quaternion delta = Quaternion.Euler(0,h * turnSpeed * Time.deltaTime,0);

        Quaternion rot = rb.rotation * delta;

        return rot;
    }

    private void Turn(Quaternion rot)
    {
        rb.MoveRotation(rot);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Coin"))
        {
            if (this.IsOwner)
            {
                Debug.Log("coin touch");
                //other.gameObject.SetActive(false);
                Coin cc = other.gameObject.GetComponent<Coin>();
                cc.SetActive(false);
            }

        }
        else if (other.gameObject.CompareTag("Player"))
        {
            if (IsClient && IsOwner)
            {
                ulong clientId = other.GetComponent<NetworkObject>().OwnerClientId;
                UpdatePlayerMeetServerRpc(this.OwnerClientId, clientId);
            }
        }
    }

    [ServerRpc]
    void UpdatePlayerMeetServerRpc(ulong from, ulong to)
    {
        ClientRpcParams p = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { to }
            }
        };
        NotifyPlayerMeetClientRpc(from, p);
    }

    [ClientRpc]
    void NotifyPlayerMeetClientRpc(ulong from, ClientRpcParams p)
    {
        if (!this.IsOwner)
        {
            Debug.Log(("Contact with player id : " + from));
        }
    }
}
