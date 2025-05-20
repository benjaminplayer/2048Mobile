using UnityEngine;

public class SwipeDetection : MonoBehaviour
{

    [SerializeField]
    private float minDistance = .2f;
    [SerializeField]
    private float maxTime = 1f;
    [SerializeField, Range(0, 1f)] private float directionTreshold = .9f;
    [SerializeField] private GameLogic logic;
    private InputManager _inputManager;

    private Vector2 startPos;
    private float startTime;
    private Vector2 endPos;
    private float endTime;


    private void Awake()
    {
        _inputManager = InputManager.Instance;
    }

    private void OnEnable()
    {
        _inputManager.OnStartTouch += SwipeStart;
        _inputManager.OnEndTouch += SwipeEnd;
    }


    private void OnDisable()
    {
        _inputManager.OnStartTouch -= SwipeStart;
        _inputManager.OnEndTouch -= SwipeEnd;
    }

    private void SwipeStart(Vector2 pos, float time)
    { 
        startPos = pos;
        startTime = time;
    }

    private void SwipeEnd(Vector2 pos, float time)
    {
        endPos = pos;
        endTime = time;
        DetectSwipe();
    }

    private void DetectSwipe()
    {
        if (Vector3.Distance(startPos, endPos) >= minDistance && (endTime - startTime) <= maxTime)
        { 
            //Debug.DrawLine(startPos, endPos, Color.red, 5f);
            Vector3 dir = endPos - startPos;
            Vector2 dir2D = new Vector2(dir.x, dir.y).normalized;
            SwipeDirection(dir2D);
        }
    }

    private void SwipeDirection(Vector2 direction)
    {
        if(logic.GetLostStatus())
            { return; }

        if (Vector2.Dot(Vector2.up, direction) > directionTreshold) // .Dot -> skalarni produkt
        {
            logic.TryShift(0);
        }
        else if (Vector2.Dot(Vector2.down, direction) > directionTreshold)
        {
            logic.TryShift(2);
        }
        else if (Vector2.Dot(Vector2.left, direction) > directionTreshold)
        {
            logic.TryShift(1);
        }
        else if (Vector2.Dot(Vector2.right, direction) > directionTreshold)
        {
            logic.TryShift(3);
        }
        
    }

}
