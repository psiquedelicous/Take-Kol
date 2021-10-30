using UnityEngine;
using UnityEngine.UI;

public class ScoreProgressBar : MonoBehaviour
{
    // Unity UI Public References
    [SerializeField]
    private Slider slider;
    public Text scoreValueText;

    // Create variables to handle the slider's value
    private float currentValue;
    private float targetValue;
    private int scoreOGValue;
    private int currentOGScore;

    // Time to create ratio for lerp in slider
    private float StartTime;

    public float Score
    {
        get
        {
            return currentValue;
        }

        set
        {
            currentValue = targetValue;
            targetValue = value;

            StartTime = Time.realtimeSinceStartup;
        }
    }

    public int ScoreOGValue
    {
        get
        {
            return scoreOGValue;
        }
        set
        {
            currentOGScore = scoreOGValue;
            scoreOGValue = value;
        }
    }

    
    public float adjustDuration = 1.0f;

    public void Update()
    {
        //Smooth change of values
        float ratio = 1 - ((StartTime + adjustDuration) - Time.realtimeSinceStartup) / adjustDuration;

        slider.value = Mathf.Lerp(currentValue, targetValue,ratio);
        
        scoreValueText.text = ((int)Mathf.Lerp(currentOGScore, scoreOGValue, ratio)).ToString();

    }
}