using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-1)]
public class InputManager : Singleton<InputManager>
{

    #region events

    public delegate void StartTouch(Vector2 position, float time);
    public event StartTouch OnStartTouch;
    public delegate void EndTouch(Vector2 position, float time);
    public event StartTouch OnEndTouch;
    #endregion

    private Controlls playerControlls;
    private Camera mainCam => Camera.main;
    private void Awake()
    {
        Debug.Log("InputManager Awake() exec");
        Debug.Log("Main camera" + Camera.main);
        playerControlls = new Controlls();
        //mainCam = Camera.main;
    }
    
    private void OnEnable()
    {
        playerControlls.Enable();
    }

    private void OnDisable()
    {
        playerControlls.Disable();
    }


    private void Start()
    {
        playerControlls.Touch.PrimaryContatct.started += ctx => StartTouchPrimary(ctx);
        playerControlls.Touch.PrimaryContatct.canceled += ctx => EndTouchPrimary(ctx);
    }


    private void StartTouchPrimary(InputAction.CallbackContext context) 
    {
        if (OnStartTouch != null)
        {
            OnStartTouch(Utils.ScreenToWorld(mainCam, playerControlls.Touch.PrimaryPos.ReadValue<Vector2>()), (float)context.startTime);
        }
    }

    private void EndTouchPrimary(InputAction.CallbackContext context)
    {
        if (OnEndTouch != null)
        {
            OnEndTouch(Utils.ScreenToWorld(mainCam, playerControlls.Touch.PrimaryPos.ReadValue<Vector2>()), (float)context.time);
        }
    }

    public Vector2 PrimaryPosition()
    {
        return Utils.ScreenToWorld(mainCam, playerControlls.Touch.PrimaryPos.ReadValue<Vector2>());
    }

}
