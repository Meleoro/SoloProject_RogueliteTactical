using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Utilities;

public enum ControllerState
{
    Idle,
    Walk,
    Jump,
    Fall
}

public class HeroController : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float holdJumpForce;
    [SerializeField] private float holdJumpDuration;

    [Header("Actions")]
    public Action EndAutoMoveAction;
    public Action<int> OnMove;
    public Action<int> OnJump;

    [Header("Private Infos")]
    private ControllerState currentControllerState;
    private bool isInBattle;
    private bool noControl;
    private bool isAutoMoving;
    private Vector2 saveSpriteLocalPos;
    private Vector2 oldPos;
    private Vector2 shadowOffset;
    public List<Vector3> savePositions = new List<Vector3>();
    private Coroutine autoMoveCoroutine;

    [Header("Public Infos")]
    public ControllerState CurrentControllerState { get { return currentControllerState; } }

    [Header("References")]
    [SerializeField] private Animator _animator;
    [SerializeField] private Rigidbody2D _rbSprite;
    [SerializeField] private ParticleSystem _walkParticleSystem;
    [SerializeField] private ParticleSystem _landParticleSystem;
    [SerializeField] private Transform _spriteParent;
    [SerializeField] private Transform _shadowTr;
    private Rigidbody2D _rb;


    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();

        saveSpriteLocalPos = _rbSprite.transform.localPosition;
        savePositions.Add(transform.position);

        shadowOffset = _shadowTr.transform.localPosition;
    }


    public void UpdateController()
    {
        if (isAutoMoving) return;
        if (currentControllerState == ControllerState.Fall) return;
        if (isInBattle) 
        {
            _rb.linearVelocity = Vector2.zero;
            Move(Vector2.zero);
            return;
        }
        if (noControl) return;

        Move(InputManager.moveDir);

        if (InputManager.wantsToJump)
        {
            switch (currentControllerState)
            {
                case ControllerState.Idle:
                    Jump();
                    break;

                case ControllerState.Walk:
                    Jump();
                    break;
            }
        }
    }


    #region Base Movement Functions

    private void Move(Vector2 inputDir)
    {
        _rb.linearVelocity = inputDir * moveSpeed;

        Rotate(inputDir);

        if(currentControllerState == ControllerState.Walk)
        {
            if (Vector2.Distance(savePositions[^1], transform.position) > 0.1f)
            {
                savePositions.Add(transform.position);
                if (savePositions.Count > 5)
                {
                    savePositions.RemoveAt(0);
                }
            }
        }

        if (inputDir != Vector2.zero)
        {
            _animator.SetBool("IsWalking", true);

            OnMove?.Invoke(0);

            if (currentControllerState == ControllerState.Jump) return;

            currentControllerState = ControllerState.Walk;
            if(!_walkParticleSystem.isPlaying)
                _walkParticleSystem.Play();

            _walkParticleSystem.transform.right = inputDir;
        }
        else
        {
            _animator.SetBool("IsWalking", false);

            if (currentControllerState == ControllerState.Jump) return;

            currentControllerState = ControllerState.Idle;
            if (_walkParticleSystem.isEmitting)
                _walkParticleSystem.Stop();
        }
    }

    private void Rotate(Vector2 inputDir)
    {
        if(shadowOffset == Vector2.zero) shadowOffset = _shadowTr.transform.localPosition;

        if (inputDir.x < -0.01)
        {
            _spriteParent.rotation = Quaternion.Euler(0, 180, 0);
            _shadowTr.localPosition = shadowOffset * new Vector2(-1, 1);
        }
        else if(inputDir.x > 0.01)
        {
            _spriteParent.rotation = Quaternion.Euler(0, 0, 0);
            _shadowTr.localPosition = shadowOffset;
        }
    }

    public void AutoMove(Vector3 aimedPos)
    {
        if (isAutoMoving) return;

        autoMoveCoroutine = StartCoroutine(AutoMoveCoroutine(aimedPos));
    }

    private IEnumerator AutoMoveCoroutine(Vector3 aimedPos)
    {
        isAutoMoving = true;

        while (Vector2.Distance(aimedPos, transform.position) > 0.1f)
        {
            Move((aimedPos - transform.position).normalized);

            yield return new WaitForEndOfFrame();
        }

        Rotate(Vector2.right);

        transform.position = aimedPos;
        isAutoMoving = false;
    }

    public void StopAutoMove()
    {
        isAutoMoving = false;

        if(autoMoveCoroutine != null)
        {
            StopCoroutine(autoMoveCoroutine);
        }
    }

    public IEnumerator AutoMoveCoroutineEndBattle(Transform aimedTr)
    {
        isAutoMoving = true;

        while (Vector2.Distance(aimedTr.position, transform.position) > 0.1f)
        {
            Move((aimedTr.position - transform.position).normalized);

            yield return new WaitForEndOfFrame();
        }

        transform.position = aimedTr.position;
        isAutoMoving = false;

        EndAutoMoveAction.Invoke();
    }

    #endregion


    #region Jump Junctions 

    private void Jump()
    {
        currentControllerState = ControllerState.Jump;

        oldPos = transform.position;

        OnJump?.Invoke(1);
        AudioManager.Instance.PlaySoundOneShot(1, 3);

        _rbSprite.bodyType = RigidbodyType2D.Dynamic;
        _rbSprite.linearVelocity = Vector2.zero;
        _rbSprite.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        _animator.ResetTrigger("JumpNext");

        _animator.SetTrigger("Jump");
        _walkParticleSystem.Stop();

        StartCoroutine(ManageJumpHoldCoroutine());
        StartCoroutine(VerifyJumpEndCoroutine());
    }

    private IEnumerator ManageJumpHoldCoroutine()
    {
        float timer = 0;

        while (timer < holdJumpDuration)
        {
            if (!InputManager.isHoldingJump) break;

            timer += Time.fixedDeltaTime;
            _rbSprite.AddForce(Vector2.up * holdJumpForce * Time.fixedDeltaTime, ForceMode2D.Force);

            yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator VerifyJumpEndCoroutine()
    {
        yield return new WaitForFixedUpdate();

        bool isGoingDown = false;

        while (_rbSprite.transform.localPosition.y > saveSpriteLocalPos.y)
        {
            if(_rbSprite.linearVelocity.y < 0 && !isGoingDown)
            {
                isGoingDown = true;
                _animator.SetTrigger("JumpNext");
            }

            float currentYDif = transform.position.y - oldPos.y;
            _rbSprite.transform.localPosition += new Vector3(0, currentYDif, 0);

            oldPos = transform.position;

            _rbSprite.transform.localPosition = new Vector3(saveSpriteLocalPos.x, _rbSprite.transform.localPosition.y, 0);

            yield return new WaitForEndOfFrame();
        }

        _animator.SetTrigger("JumpNext");
        _landParticleSystem.Play();

        AudioManager.Instance.PlaySoundOneShot(1, 4);

        _rbSprite.transform.localPosition = saveSpriteLocalPos;
        _rbSprite.linearVelocity = Vector2.zero;
        _rbSprite.bodyType = RigidbodyType2D.Kinematic;

        currentControllerState = ControllerState.Idle;
    }

    #endregion


    #region Others

    public IEnumerator FallCoroutine()
    {
        currentControllerState = ControllerState.Fall;
        _rb.linearVelocity = Vector2.zero;

        AudioManager.Instance.PlaySoundOneShot(1, 5);

        _rbSprite.transform.UChangeScale(0.5f, Vector3.zero, CurveType.EaseInOutSin);

        yield return new WaitForSeconds(0.5f);

        _rbSprite.transform.UStopChangeScale();

        currentControllerState = ControllerState.Idle;
        transform.position = savePositions[1];
        _rbSprite.transform.localScale = Vector3.one;

        HeroesManager.Instance.TakeDamage(1);
    }


    public IEnumerator TakeStairsCoroutine()
    {
        StartCoroutine(AutoMoveCoroutine(transform.position + new Vector3(0, 2, 0)));

        yield return new WaitForSeconds(1);
    }


    public void StopControl()
    {
        noControl = true;
        _rb.linearVelocity = new Vector3(0, 0, 0);
    }

    public void RestartControl()
    {
        noControl = false;  
    }

    public void EnterBattle()
    {
        isInBattle = true;
    }

    public void ExitBattle()
    {
        isInBattle = false;
    }

    #endregion
}
