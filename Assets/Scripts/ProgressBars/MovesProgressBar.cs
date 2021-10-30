using UnityEngine;
using UnityEngine.UI;

public class MovesProgressBar : MonoBehaviour
{

    // Unity UI References
    [SerializeField]
    private Slider slider;

    // Create variables to handle the slider's value
    private float currentValue;
    private float targetValue;

    public float Moves
    {
        get
        {
            return currentValue;
        }
        set
        {
            targetValue = value;
        }
    }

    public void Update()
    {
        //Smooth change of values
        currentValue = Mathf.SmoothStep(currentValue, targetValue, Time.deltaTime * 30.0f);

        slider.value = currentValue;
    }
}
