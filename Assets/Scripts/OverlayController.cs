using System;
using UnityEngine;
using UnityEngine.UIElements;

public class OverlayController : MonoBehaviour
{
    public float lowHealthStart = 0.2f;
    public Color lowHealthColor = Color.red;
    public float maxLowHealthJitter = 5.0f; // degrees
    public float hintShowTime = 5.0f;
    
    private static float MAX_HEALTH = 1.0f;
    private ProgressBar _healthBar;
    private VisualElement _healthBarFill;
    
    private Timer showHintTimer;
    
    private Label hintLabel;

    void Awake()
    {
        UIDocument doc = GetComponent<UIDocument>();
        VisualElement root = doc.rootVisualElement;
        _healthBar = root.Q<ProgressBar>("healthBar");
        
        _healthBar.lowValue = 0;
        _healthBar.highValue = MAX_HEALTH;
        
        _healthBar.value = MAX_HEALTH;
        
        _healthBarFill = _healthBar.Q<VisualElement>("unity-progress-bar");
        
        hintLabel = root.Q<Label>("hint");
        showHintTimer = new Timer(hintShowTime);
    }

    private void Update()
    {
        // Jitter the health bar at low health
        if (_healthBar.value < lowHealthStart)
        {
            var jitterAmount = maxLowHealthJitter * (1 - (_healthBar.value / lowHealthStart));
            var jitter = UnityEngine.Random.Range(-jitterAmount, jitterAmount);
            _healthBarFill.style.rotate = Quaternion.Euler(0, 0, jitter);
        }
        else
        {
            _healthBarFill.style.rotate = Quaternion.Euler(0, 0, 0);
        }

        if (showHintTimer.isFinished())
        {
            hintLabel.text = "";
        }
    }

    public void setHealth(float newValue)
    {
        _healthBar.value = Mathf.Clamp(newValue, _healthBar.lowValue, _healthBar.highValue);
        if (newValue < lowHealthStart)
        {
            _healthBarFill.style.backgroundColor = Color.Lerp(lowHealthColor, Color.white, newValue / lowHealthStart);
        }
    }

    public void displayHint(string hint)
    {
        hintLabel.text = hint;
        showHintTimer.restart();
    }
}
