using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;

/// <summary>
/// 게임을 관리해주는 매니저 다음 스테이지로 넘어가고 count를 올리고 내려줌
/// </summary>
public class GameManager : UnitySingleton<GameManager>
{
    private int playerCount;
    [SerializeField] private Dictionary<string, GameObject> dirctionary;
    [SerializeField] private List<string> sceneNames;
    [SerializeField] private List<GameObject> playerList;
    public HashSet<string> playerNickList;
    [SerializeField] private float voteRate = 0.3f;
    public float maxInk = 30f;
    public float useInk = 0f;
    public float usedInk = 0f;


    private int voteCount = 0;
    private int stageIndex;

    public PhotonView PV;

    public float UsedInk
    {
        get => usedInk;
        set => usedInk = value;
    }
    public float UseInk
    {
        get => useInk;
        set => useInk = value;
    }
    public float MaxInk{
        get => maxInk;
        set => maxInk = value;
    }
    public override void OnCreated()
    {
        //throw new System.NotImplementedException();
    }

    public override void OnInitiate()
    {
        stageIndex = 0;
        playerCount = 0;
        dirctionary = new Dictionary<string, GameObject>();
        if (sceneNames == null || sceneNames.Count == 0)
            sceneNames = new List<string>();
    }

    private void Start()
    {
        useInk = 0;
        usedInk = 0;
        playerCount = 0;
        playerNickList = new HashSet<string>();
    }
    /// <summary>
    /// 다시 시작하는 함수
    /// 카운트를 올리고 만약 카운트가 설정한 비율 이상이면 게임을 다시 시작함.
    /// </summary>
    public void RestartVote() {
        voteCount++;
        if (voteCount >= playerList.Count * voteRate)
        {
            voteCount = 0;
            ReLoadScene();
        }
    }


    public void SetDirctionary(string name, GameObject gameObject)
    {
        dirctionary.Add(name, gameObject);
    }

    public GameObject GetDirctionary(string name)
    {
        return dirctionary[name];
    }

    /// <summary>
    /// 신을 다시 연다
    /// </summary>
    [PunRPC]
    public void ReLoadScene() {
        playerCount = 0;
        voteCount = 0;
        playerNickList = new HashSet<string>();
        dirctionary = new Dictionary<string, GameObject>();
        UIManager.Instance.ResetDictionary();
        SceneManager.LoadScene(sceneNames[stageIndex]);
    }

    public void reloadAllPlayer(){
        PV.RPC("ReLoadScene", RpcTarget.All);
    }


    /// <summary>
    /// 플레이어를 리스트에 추가해주는 함수
    /// </summary>
    /// <param name="playerList"></param>
    public void SetPlayer(GameObject playerList) {
        this.playerList.Add(playerList);
    }


    /// <summary>
    /// 플레이어가 출구에 들어갈 경우 카운트를 증가 시켜줌
    /// 만약 Max에 도달하면 다음 스테이지로 이동함.
    /// 플레이어를 넘겨줘서 안보이게 해야
    /// </summary>
    [PunRPC]
    public void EnterPlayer(string nickName) {
        playerNickList.Add(nickName);
        if(PhotonNetwork.CurrentRoom.Players.Count == playerNickList.Count) {
            this.GetDirctionary("Rocket").GetComponent<Rocket>().RocketFire();
            playerCount = 0;
            voteCount = 0;
            playerNickList = new HashSet<string>();
            dirctionary = new Dictionary<string, GameObject>();
            UIManager.Instance.ResetDictionary();
            SceneManager.LoadScene(sceneNames[++stageIndex]);
        };
    }

    public void toAllEnterPlayer(string nickName) {
        PV.RPC("EnterPlayer", RpcTarget.All, nickName);
    }


}
