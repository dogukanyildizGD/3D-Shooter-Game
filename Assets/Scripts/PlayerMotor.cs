using UnityEngine;
using Mirror;
using UnityEngine.Animations.Rigging;
using StarterAssets; // FirstPersonController için namespace ekledik.

[RequireComponent(typeof(CharacterController))]
public class PlayerMotor : NetworkBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private TwoBoneIKConstraint rightHandIK;
    [SerializeField] private TwoBoneIKConstraint leftHandIK;

    // FirstPersonController bileþenine prefab içerisinden eriþmek için bir referans
    private FirstPersonController firstPersonController;
    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            Debug.LogError("CharacterController bileþeni bulunamadý!");
            return;
        }

        firstPersonController = GetComponentInParent<FirstPersonController>();
        if (firstPersonController == null)
        {
            Debug.LogError("FirstPersonController bileþeni bulunamadý!");
            return;
        }

        animator = GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator bileþeni bulunamadý!");
            return;
        }

        animator.enabled = true;
    }

    // Update is called once per frame
    private void Update()
    {
        if (!isLocalPlayer) return;

        // FirstPersonController scriptinden hareket ve zýplama bilgilerini al
        Vector2 movementInput = firstPersonController.GetComponent<StarterAssetsInputs>().move;
        bool isJumping = firstPersonController.GetComponent<StarterAssetsInputs>().jump;

        bool isWalking = movementInput.sqrMagnitude > 0;

        // Animasyonlarý güncelle
        CmdUpdateAnimation(isWalking, isJumping);
    }

    [Command]
    void CmdUpdateAnimation(bool isWalking, bool isJumping)
    {
        RpcUpdateAnimation(isWalking, isJumping);
    }

    [ClientRpc]
    void RpcUpdateAnimation(bool isWalking, bool isJumping)
    {
        if (animator != null)
        {
            animator.SetBool("IsWalking", isWalking);
            animator.SetBool("IsJump", isJumping); // Zýplama animasyonunu ayarlamak için

            // Zýplama sýrasýnda aðýrlýklarý azalt
            /*if (isJumping)
            {
                rightHandIK.weight = 0.5f;
                leftHandIK.weight = 0.5f;
            }
            else if (isWalking)
            {
                // Yürüyüþ sýrasýnda aðýrlýklarý tam yap
                rightHandIK.weight = 1f;
                leftHandIK.weight = 1f;
            }
            else
            {
                // Idle durumunda aðýrlýklarý sýfýrla
                rightHandIK.weight = 1f;
                leftHandIK.weight = 1f;
            }*/
        }
        else
        {
            Debug.LogError("Animator bileþeni bulunamadý!");
        }
    }
}
