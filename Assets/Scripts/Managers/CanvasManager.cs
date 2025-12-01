using UnityEngine;
using UnityEngine.UI;


    [ExecuteAlways]
public class CanvasManager : MonoBehaviour
{
    public static CanvasManager instance;
    [SerializeField] private Vector2 _resolution;
    [SerializeField] private int _ppp;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }
    private void Update()
    {
        // var x = Screen.width;
        //var y = Screen.height;
        //var ppp = screen
        //_resolution =  new Vector2(x, y);
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            var childCanvas = child.GetComponent<CanvasScaler>();

            if (childCanvas == null)
            {
                for (int j = 0; j < child.transform.childCount; j++)
                {
                    childCanvas = child.GetChild(j).GetComponent<CanvasScaler>();

                    childCanvas.referenceResolution = _resolution;
                    childCanvas.referencePixelsPerUnit = _ppp;
                }
            }

            childCanvas.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            childCanvas.referenceResolution = _resolution;
            childCanvas.referencePixelsPerUnit = _ppp;
        }
    }
}
