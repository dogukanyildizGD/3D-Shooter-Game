using UnityEngine;
using Mirror;
using UnityEngine.Animations.Rigging;
using StarterAssets; // FirstPersonController i�in namespace ekledik.

[RequireComponent(typeof(CharacterController))]
public class PlayerMotor : NetworkBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private TwoBoneIKConstraint rightHandIK;
    [SerializeField] private TwoBoneIKConstraint leftHandIK;

    // FirstPersonController bile�enine prefab i�erisinden eri�mek i�in bir referans
    private FirstPersonController firstPersonController;
    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            Debug.LogError("CharacterController bile�eni bulunamad�!");
            return;
        }

        firstPersonController = GetComponentInParent<FirstPersonController>();
        if (firstPersonController == null)
        {
            Debug.LogError("FirstPersonController bile�eni bulunamad�!");
            return;
        }

        animator = GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator bile�eni bulunamad�!");
            return;
        }

        animator.enabled = true;
    }

    // Update is called once per frame
    private void Update()
    {
        if (!isLocalPlayer) return;

        // FirstPersonController scriptinden hareket ve z�plama bilgilerini al
        Vector2 movementInput = firstPersonController.GetComponent<StarterAssetsInputs>().move;
        bool isJumping = firstPersonController.GetComponent<StarterAssetsInputs>().jump;

        bool isWalking = movementInput.sqrMagnitude > 0;

        // Animasyonlar� g�ncelle
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
            animator.SetBool("IsJump", isJumping); // Z�plama animasyonunu ayarlamak i�in

            // Z�plama s�ras�nda a��rl�klar� azalt
            /*if (isJumping)
            {
                rightHandIK.weight = 0.5f;
                leftHandIK.weight = 0.5f;
            }
            else if (isWalking)
            {
                // Y�r�y�� s�ras�nda a��rl�klar� tam yap
                rightHandIK.weight = 1f;
                leftHandIK.weight = 1f;
            }
            else
            {
                // Idle durumunda a��rl�klar� s�f�rla
                rightHandIK.weight = 1f;
                leftHandIK.weight = 1f;
            }*/
        }
        else
        {
            Debug.LogError("Animator bile�eni bulunamad�!");
        }
    }
}
