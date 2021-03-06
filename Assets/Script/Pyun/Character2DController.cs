using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class Character2DController : MonoBehaviour
{
    public float maxSpeed = 1;
    public float jumpForce = 1;
    public Text nickNameText;

    private Rigidbody2D _rigidbody;
    private SpriteRenderer spriteRenderer;
    Animator anim;
    PhotonView view;

    public GameObject LineDraw;

    private bool canExit;

    public GameObject playerObject;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        view = GetComponent<PhotonView>();
        nickNameText.text = view.IsMine ? PhotonNetwork.NickName : view.Owner.NickName;
        nickNameText.color = view.IsMine ? Color.green : Color.white;
        if(view.IsMine)
        {
            if(PhotonNetwork.CurrentRoom.Players[1].NickName==view.Owner.NickName)
            {
                Debug.Log("I'm the Shamen");
                LineDraw.SetActive(true);
                UIManager.Instance.GetDirctionary("palette").SetActive(true);
                UIManager.Instance.GetDirctionary("InkSlider").SetActive(true);
                GameManager.Instance.GetDirctionary("LineDrowing").SetActive(true);
                GameManager.Instance.GetDirctionary("LineDrowing").GetComponent<LineDrawer>().SetCursor();
            }
            else{
                Debug.Log("Not Shamen");
                LineDraw.SetActive(false);
                UIManager.Instance.GetDirctionary("palette").SetActive(false);
                UIManager.Instance.GetDirctionary("InkSlider").SetActive(false);
                GameManager.Instance.GetDirctionary("LineDrowing").SetActive(false);
            }
        }
        
    }

    void Update()
    {
        if(GameManager.Instance.playerNickList.Contains(view.Owner.NickName)){
            view.gameObject.SetActive(false);
            if(view.IsMine){
                GameManager.Instance.GetDirctionary("LineDrowing").SetActive(false);
            }
        }
        if(view.IsMine) //??? ??????????????? ??????
        {
            


            //Stop Speed - ?????? ?????? ?????? ?????? ?????? ?????? - ????????? ????????? ????????? ???
            if(Input.GetButtonUp("Horizontal")){
                _rigidbody.velocity = new Vector2(0.5f*_rigidbody.velocity.normalized.x, _rigidbody.velocity.y);
            }

            //jump ?????? ?????? ??????
            // if(Input.GetButtonDown("Jump") && Mathf.Abs(_rigidbody.velocity.y) < 0.001f)
            // {
            //     _rigidbody.AddForce(new Vector2(0, JumpForce), ForceMode2D.Impulse);
            // }
            
            //jump ??????(space) ????????? ?????? ?????????(????????????????????? ??????)??? ???????????? ??????????????? ??? ?????????(jumpForce??????)
            if (Input.GetButtonDown("Jump") && !anim.GetBool("isJumping"))
            {
                _rigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                anim.SetBool("isJumping", true);
            }

            //Landing platform - platform layer??? Ray??? ????????? ?????? ?????? ???
            if (_rigidbody.velocity.y < 0)
            {
                Debug.DrawRay(_rigidbody.position, Vector3.down, new Color(0, 1, 0));

                RaycastHit2D rayHit = Physics2D.Raycast(_rigidbody.position, Vector3.down, 1, LayerMask.GetMask("Platform"));

                if(rayHit.collider != null)
                {
                    if(rayHit.distance < 0.5f)
                        anim.SetBool("isJumping", false);
                }

            }
            //Move Speed - ?????? ???????????? ?????? ????????? ??? ?????? ????????? ??????
            float h = Input.GetAxisRaw("Horizontal");

            //Turn off Addforce when against wall - ?????? ????????? Addforce??? ?????????
            if (_rigidbody.velocity.x < 1f)
            {

                Debug.DrawRay(_rigidbody.position, Vector3.right, new Color(0, 1, 0));
                Debug.DrawRay(_rigidbody.position, Vector3.left, new Color(0, 1, 0));

                RaycastHit2D rayHit_left = Physics2D.Raycast(_rigidbody.position, Vector3.left, 1, LayerMask.GetMask("Platform"));
                RaycastHit2D rayHit_right = Physics2D.Raycast(_rigidbody.position, Vector3.right, 1, LayerMask.GetMask("Platform"));

                if (rayHit_left.collider != null)
                    if(h < 0)
                    h = 0;

                if (rayHit_right.collider != null)
                    if(h > 0)
                    h = 0;
            }
            _rigidbody.AddForce(Vector2.right * h, ForceMode2D.Impulse);
            //Max Speed - ????????? maxSpeed ?????? ??????
            if (_rigidbody.velocity.x > maxSpeed)
                _rigidbody.velocity = new Vector2(maxSpeed, _rigidbody.velocity.y);
            else if (_rigidbody.velocity.x < maxSpeed * (-1))
                _rigidbody.velocity = new Vector2(maxSpeed * (-1), _rigidbody.velocity.y);

            

            //Direction Sprite
            // if (Input.GetButtonDown("Horizontal"))
            //     spriteRenderer.flipX = Input.GetAxisRaw("Horizontal") == -1;
            float axis = Input.GetAxisRaw("Horizontal");
            if(axis != 0) view.RPC("FlipXRPC", RpcTarget.AllBuffered, axis);

            //Animation
            if (Mathf.Abs(_rigidbody.velocity.x) < 0.3)
                anim.SetBool("isWalking", false);
            else
                anim.SetBool("isWalking", true);

            if(Input.GetAxisRaw("Vertical")>0){
                if(canExit){
                    GameManager.Instance.toAllEnterPlayer(PhotonNetwork.NickName);
                }
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.tag == "Enemy"){
            Debug.Log("??????????????? ???????????????!");
            GameManager.Instance.reloadAllPlayer();
        }
    }

    [PunRPC]
    void FlipXRPC(float axis)
    {
        spriteRenderer.flipX = axis == -1;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if(other.gameObject.tag == "Finish" && view.IsMine){
            canExit = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if(other.gameObject.tag == "Finish" && view.IsMine){
            canExit = false;
        }
    }
}
