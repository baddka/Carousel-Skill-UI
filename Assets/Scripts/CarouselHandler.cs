using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
public class CarouselHandler : MonoBehaviour
{
    private const float CLOCKWISE_OFFSET_DEGREES = 90f;
    private const float FULL_CIRCLE_DEGREES = 360f;
    private const float SEMICIRCLE_CENTER_ANGLE = 180f;
    private const float MINIMUM_BUTTON_COUNT = 3;
    private const float MINIMUM_ANIMATION_DURATION = 0.1f;
    private const float MAXIMUM_ANIMATION_DURATION = 5.0f;
    private const float MINIMUM_LAYOUT_RADIUS = 10f;
    private const float MAXIMUM_LAYOUT_RADIUS = 1000f;
    private const float MINIMUM_BUTTON_SCALE = 0.1f;
    private const float MAXIMUM_BUTTON_SCALE = 5.0f;

    [Header("Skill Buttons (dynamic count)")]
    [SerializeField] private List<SkillButtonController> skillButtons;

    [Header("Main (Toggle) Button")]
    [SerializeField] private MainButtonController mainButton;

    [Header("Animation & Layout Settings")]
    [Range(MINIMUM_ANIMATION_DURATION, MAXIMUM_ANIMATION_DURATION)]
    [SerializeField] private float animationDuration = 0.5f;
    
    [Range(MINIMUM_LAYOUT_RADIUS, MAXIMUM_LAYOUT_RADIUS)]
    [SerializeField] private float layoutRadius = 100f;
    
    [Range(0f, FULL_CIRCLE_DEGREES)]
    [SerializeField] private float semicircleStartAngle = 0f;
    
    [Range(30f, FULL_CIRCLE_DEGREES)]
    [SerializeField] private float semicircleArcDegrees = 180f;
    
    [Range(MINIMUM_BUTTON_SCALE, MAXIMUM_BUTTON_SCALE)]
    [SerializeField] private float centerButtonScale = 1.3f;
    
    [Range(MINIMUM_BUTTON_SCALE, MAXIMUM_BUTTON_SCALE)]
    [SerializeField] private float adjacentButtonScale = 1.0f;
    
    [Range(MINIMUM_BUTTON_SCALE, MAXIMUM_BUTTON_SCALE)]
    [SerializeField] private float normalButtonScale = 0.85f;

    [Header("Default Button Scale (Star Layout)")]
    [Range(MINIMUM_BUTTON_SCALE, MAXIMUM_BUTTON_SCALE)]
    [SerializeField] private float defaultButtonScale = 1.0f;

    [Header("Main Button Scale")]
    [SerializeField] private float mainButtonStarScale = 0.8f;
    [SerializeField] private float mainButtonSemicircleScale = 1.25f;

    [Header("UI Animator for Overlay/BG Images")]
    [SerializeField] private UIAnimatorController uiAnimatorController;

    private int totalButtonCount;
    private int centerButtonIndex;
    private bool isCurrentlyAnimating = false;
    private bool isMainButtonBig = false; 
    private int[] semicircleSlotAssignments;
    private Vector2[] starLayoutPositions;
    private float[] semicircleArcAnglePositions;
    
    // Remember the last opened skill button tab
    private int lastOpenedTabIndex = -1;

    #region Properties
    public List<SkillButtonController> SkillButtons => skillButtons;
    public MainButtonController MainButton => mainButton;
    public float AnimationDuration => animationDuration;
    public float LayoutRadius => layoutRadius;
    public bool IsAnimating => isCurrentlyAnimating;
    #endregion

    /// <summary>
    /// Initializes the carousel and sets up the initial layout.
    /// </summary>
    void Start()
    {
        if (!ValidateSetup())
        {
            enabled = false;
            return;
        }

        InitializeCarousel();
        SetupButtonEventListeners();
    }

    private bool ValidateSetup()
    {
        if (skillButtons == null || skillButtons.Count < MINIMUM_BUTTON_COUNT)
        {
            Debug.LogError($"[RadialBeltCarousel] Carousel needs at least {MINIMUM_BUTTON_COUNT} buttons. Current count: {(skillButtons?.Count ?? 0)}");
            return false;
        }

        if (mainButton == null)
        {
            Debug.LogError("[RadialBeltCarousel] Main button is not assigned!");
            return false;
        }

        // Validate button components
        for (int i = 0; i < skillButtons.Count; i++)
        {
            if (skillButtons[i] == null)
            {
                Debug.LogError($"[RadialBeltCarousel] Skill button at index {i} is null!");
                return false;
            }

            if (skillButtons[i].GetComponent<Button>() == null)
            {
                Debug.LogWarning($"[RadialBeltCarousel] Skill button at index {i} does not have a Button component!");
            }
        }

        return true;
    }

    /// <summary>
    /// Initializes the carousel and sets up the initial layout.
    /// </summary>
    private void InitializeCarousel()
    {
        totalButtonCount = skillButtons.Count;
        centerButtonIndex = totalButtonCount / 2;
        
        InitializeSemicircleSlots();
        InitializeStarLayoutPositions();
        InitializeSemicircleArcPositions();
        
        ApplyInitialStarLayout();
    }

    /// <summary>
    /// Initializes the slot assignments for the semicircle layout.
    /// </summary>
    private void InitializeSemicircleSlots()
    {
        semicircleSlotAssignments = new int[totalButtonCount];
        for (int i = 0; i < totalButtonCount; i++)
        {
            semicircleSlotAssignments[i] = i;
        }
    }

    /// <summary>
    /// Calculates and initializes the positions for the star layout.
    /// </summary>
    private void InitializeStarLayoutPositions()
    {
        starLayoutPositions = new Vector2[totalButtonCount];
        float[] starAnglePositions = CalculateStarAngles();
        
        for (int i = 0; i < totalButtonCount; i++)
        {
            float angleInRadians = starAnglePositions[i] * Mathf.Deg2Rad;
            starLayoutPositions[i] = new Vector2(-Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians)) * layoutRadius;
        }
    }


    /// <summary>
    /// Calculates the angles for positioning buttons in a star layout.
    /// </summary>
    private float[] CalculateStarAngles()
    {
        float[] angles = new float[totalButtonCount];
        
        if (totalButtonCount == 5)
        {
            angles = new float[] {90f, 20f, -50f, -130f, -200f};
        }
        else
        {
            float angleStep = FULL_CIRCLE_DEGREES / totalButtonCount;
            for (int i = 0; i < totalButtonCount; i++)
            {
                angles[i] = CLOCKWISE_OFFSET_DEGREES - i * angleStep;
            }
        }
        
        return angles;
    }

    /// <summary>
    /// Initializes the arc angles for positioning buttons in a semicircle layout.
    /// </summary>
    private void InitializeSemicircleArcPositions()
    {
        semicircleArcAnglePositions = new float[totalButtonCount];
        float angleStep = semicircleArcDegrees / (totalButtonCount - 1);
        float arcStartAngle = semicircleStartAngle - semicircleArcDegrees / 2f;
        
        for (int i = 0; i < totalButtonCount; i++)
        {
            semicircleArcAnglePositions[i] = arcStartAngle + i * angleStep;
        }
    }

    /// <summary>
    /// Applies the initial star layout to all skill buttons.
    /// </summary>
    private void ApplyInitialStarLayout()
    {
        for (int i = 0; i < totalButtonCount; i++)
        {
            skillButtons[i].SetPosition(starLayoutPositions[i]);
            skillButtons[i].SetScale(defaultButtonScale);
        }
    }

    /// <summary>
    /// Sets up event listeners for all skill buttons.
    /// </summary>
    private void SetupButtonEventListeners()
    {
        if (mainButton.Button != null)
            mainButton.Button.onClick.AddListener(OnMainButtonClicked);

        for (int i = 0; i < totalButtonCount; i++)
        {
            skillButtons[i].OnButtonClicked += HandleSkillButtonClicked;
        }
    }

    /// <summary>
    /// Handles the click event for a skill button.
    /// </summary>
    private void HandleSkillButtonClicked(SkillButtonController clickedButton)
    {
        int buttonIndex = skillButtons.IndexOf(clickedButton);
        if (buttonIndex >= 0)
        {
            // Close all tabs first
            if (uiAnimatorController != null)
                uiAnimatorController.CloseAllTabs();

            // Open the tab with the same index as the skill button
            if (uiAnimatorController != null)
                uiAnimatorController.OpenTab(buttonIndex);

            // Remember the last opened tab index
            lastOpenedTabIndex = buttonIndex;

            OnSkillButtonClicked(buttonIndex);
        }
    }

    /// <summary>
    /// Gets the scale for a button based on its slot index.
    /// </summary>
    private float GetSemicircleLayoutScale(int slotIndex)
    {
        if (slotIndex == centerButtonIndex) 
            return centerButtonScale;
        
        if (IsAdjacentToCenter(slotIndex))
            return adjacentButtonScale;
        
        return normalButtonScale;
    }

    /// <summary>
    /// Checks if a button is adjacent to the center button.
    /// </summary>
    private bool IsAdjacentToCenter(int index)
    {
        int previousCenterIndex = (centerButtonIndex - 1 + totalButtonCount) % totalButtonCount;
        int nextCenterIndex = (centerButtonIndex + 1) % totalButtonCount;
        return index == previousCenterIndex || index == nextCenterIndex;
    }

    /// <summary>
    /// Handles the click event for the main button.
    /// </summary>
    private void OnMainButtonClicked()
    {
        if (isCurrentlyAnimating) return;
        if (uiAnimatorController != null)
            uiAnimatorController.SetIsOpen(!isMainButtonBig);
        SetAllSkillButtonRaycast(true);
        float targetMainScale = isMainButtonBig ? mainButtonStarScale : mainButtonSemicircleScale;
        mainButton.AnimateScale(targetMainScale, animationDuration);
        StartCoroutine(AnimateMainButtonToggle());
    }

    /// <summary>
    /// Animates the main button between star and semicircle layouts.
    /// </summary>
    private IEnumerator AnimateMainButtonToggle()
    {
        isCurrentlyAnimating = true;
        AnimationData animationData = isMainButtonBig ? PrepareStarAnimationData() : PrepareSemicircleAnimationData();
        float elapsedTime = 0f;

        Vector3[] startScales = new Vector3[totalButtonCount];
        Vector3[] endScales = new Vector3[totalButtonCount];
        for (int i = 0; i < totalButtonCount; i++)
        {
            startScales[i] = skillButtons[i].RectTransform.localScale;
            if (!isMainButtonBig)
            {
                int slotIndex = semicircleSlotAssignments[i];
                if (slotIndex == centerButtonIndex)
                    endScales[i] = Vector3.one * centerButtonScale;
                else if (IsAdjacentToCenter(slotIndex))
                    endScales[i] = Vector3.one * adjacentButtonScale;
                else
                    endScales[i] = Vector3.one * normalButtonScale;
            }
            else
            {
                endScales[i] = Vector3.one * defaultButtonScale;
            }
        }

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / animationDuration);
            for (int i = 0; i < totalButtonCount; i++)
            {
                float staggeredTime = t;
                Vector2 pos = InterpolateCircularPosition(animationData.startPositions[i], animationData.endPositions[i], staggeredTime);
                skillButtons[i].SetPosition(pos);
                skillButtons[i].SetScale(Mathf.Lerp(startScales[i].x, endScales[i].x, t));
            }
            yield return null;
        }
        if (!isMainButtonBig)
        {
            if (uiAnimatorController != null)
            {
                uiAnimatorController.CloseAllTabs();
                // Open the last opened tab if available, otherwise open the center button tab
                int tabToOpen = lastOpenedTabIndex >= 0 ? lastOpenedTabIndex : centerButtonIndex;
                uiAnimatorController.OpenTab(tabToOpen);
            }
            ApplyFinalSemicirclePositions();
            // Open the tab for the center skill button
        }
        else
        {
            // Optionally, close all tabs when returning to star layout
            if (uiAnimatorController != null)
            {
                uiAnimatorController.CloseAllTabs();
            }
            ApplyFinalStarPositions();
        }
        isMainButtonBig = !isMainButtonBig;
        isCurrentlyAnimating = false;
    }

    /// <summary>
    /// Data structure for animation parameters.
    /// </summary>
    private struct AnimationData
    {
        public Vector2[] startPositions;
        public Vector2[] endPositions;
        public Vector3[] startScales;
        public Vector3[] endScales;
    }

    /// <summary>
    /// Prepares animation data for transitioning to a semicircle layout.
    /// </summary>
    private AnimationData PrepareSemicircleAnimationData()
    {
        var data = new AnimationData
        {
            startPositions = new Vector2[totalButtonCount],
            endPositions = new Vector2[totalButtonCount],
            startScales = new Vector3[totalButtonCount],
            endScales = new Vector3[totalButtonCount]
        };

        for (int i = 0; i < totalButtonCount; i++)
        {
            data.startPositions[i] = skillButtons[i].RectTransform.anchoredPosition;
            int slotIndex = semicircleSlotAssignments[i];
            float angle = semicircleArcAnglePositions[slotIndex];
            data.endPositions[i] = CalculateSemicirclePosition(angle, layoutRadius);
            data.startScales[i] = skillButtons[i].RectTransform.localScale;
            data.endScales[i] = Vector3.one * GetSemicircleLayoutScale(slotIndex);
        }

        return data;
    }

    /// <summary>
    /// Prepares animation data for transitioning to a star layout.
    /// </summary>
    private AnimationData PrepareStarAnimationData()
    {
        var data = new AnimationData
        {
            startPositions = new Vector2[totalButtonCount],
            endPositions = new Vector2[totalButtonCount],
            startScales = new Vector3[totalButtonCount],
            endScales = new Vector3[totalButtonCount]
        };

        for (int i = 0; i < totalButtonCount; i++)
        {
            data.startPositions[i] = skillButtons[i].RectTransform.anchoredPosition;
            data.endPositions[i] = starLayoutPositions[i];
            data.startScales[i] = skillButtons[i].RectTransform.localScale;
            data.endScales[i] = Vector3.one; 
        }

        return data;
    }

    /// <summary>
    /// Executes the animation based on the provided animation data.
    /// </summary>
    private IEnumerator ExecuteAnimation(AnimationData data)
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = Mathf.SmoothStep(0f, 1f, elapsedTime / animationDuration);
            
            for (int i = 0; i < totalButtonCount; i++)
            {
                float staggeredTime = CalculateStaggeredTime(data.startPositions[i], data.endPositions[i], normalizedTime, i);
                skillButtons[i].RectTransform.anchoredPosition = InterpolateCircularPosition(data.startPositions[i], data.endPositions[i], staggeredTime);
                skillButtons[i].RectTransform.localScale = Vector3.Lerp(data.startScales[i], data.endScales[i], normalizedTime);
            }
            
            yield return null;
        }
        
        for (int i = 0; i < totalButtonCount; i++)
        {
            skillButtons[i].RectTransform.anchoredPosition = data.endPositions[i];
            skillButtons[i].RectTransform.localScale = data.endScales[i];
        }
    }

    /// <summary>
    /// Applies the final positions and scales for the semicircle layout.
    /// </summary>
    private void ApplyFinalSemicirclePositions()
    {
        for (int i = 0; i < totalButtonCount; i++)
        {
            int slotIndex = semicircleSlotAssignments[i];
            float angle = semicircleArcAnglePositions[slotIndex];
            skillButtons[i].SetPosition(CalculateSemicirclePosition(angle, layoutRadius));
            if (slotIndex == centerButtonIndex)
                skillButtons[i].SetScale(centerButtonScale);
            else if (IsAdjacentToCenter(slotIndex))
                skillButtons[i].SetScale(adjacentButtonScale);
            else
                skillButtons[i].SetScale(normalButtonScale);
        }
    }

    /// <summary>
    /// Applies the final positions and scales for the star layout.
    /// </summary>
    private void ApplyFinalStarPositions()
    {
        for (int i = 0; i < totalButtonCount; i++)
        {
            skillButtons[i].SetPosition(starLayoutPositions[i]);
            skillButtons[i].SetScale(defaultButtonScale);
        }
        SetAllSkillButtonRaycast(false);
    }

    /// <summary>
    /// Handles the click event for a skill button.
    /// </summary>
    private void OnSkillButtonClicked(int buttonIndex)
    {
        if (isCurrentlyAnimating || !isMainButtonBig) return;
        
        int currentSlot = semicircleSlotAssignments[buttonIndex];
        if (currentSlot == centerButtonIndex)
        {
            return;
        }
        
        int shiftSteps = centerButtonIndex - currentSlot;
        StartCoroutine(AnimateSlotShift(shiftSteps));
    }

    /// <summary>
    /// Animates the shift of buttons in the semicircle layout.
    /// </summary>
    private IEnumerator AnimateSlotShift(int shiftSteps)
    {
        isCurrentlyAnimating = true;
        
        var shiftData = PrepareShiftAnimationData(shiftSteps);
        
        yield return StartCoroutine(ExecuteShiftAnimation(shiftData));
        
        ApplyShiftedSlotAssignments(shiftData.newSlotAssignments);
        isCurrentlyAnimating = false;
    }

    /// <summary>
    /// Data structure for shift animation parameters.
    /// </summary>
    private struct ShiftAnimationData
    {
        public float[] startAngles;
        public float[] endAngles;
        public Vector3[] startScales;
        public Vector3[] endScales;
        public int[] newSlotAssignments;
    }

    /// <summary>
    /// Prepares animation data for shifting buttons in the semicircle layout.
    /// </summary>
    private ShiftAnimationData PrepareShiftAnimationData(int shiftSteps)
    {
        var data = new ShiftAnimationData
        {
            startAngles = new float[totalButtonCount],
            endAngles = new float[totalButtonCount],
            startScales = new Vector3[totalButtonCount],
            endScales = new Vector3[totalButtonCount],
            newSlotAssignments = new int[totalButtonCount]
        };

        float angleStep = semicircleArcDegrees / (totalButtonCount - 1);
        
        for (int i = 0; i < totalButtonCount; i++)
        {
            int oldSlot = semicircleSlotAssignments[i];
            int newSlot = (oldSlot + shiftSteps + totalButtonCount) % totalButtonCount;
            
            data.newSlotAssignments[i] = newSlot;
            data.startAngles[i] = semicircleArcAnglePositions[oldSlot];
            data.endAngles[i] = semicircleArcAnglePositions[newSlot];
            
            if (shiftSteps > 0 && data.endAngles[i] < data.startAngles[i])
                data.endAngles[i] += FULL_CIRCLE_DEGREES;
            else if (shiftSteps < 0 && data.endAngles[i] > data.startAngles[i])
                data.endAngles[i] -= FULL_CIRCLE_DEGREES;
            
            data.startScales[i] = Vector3.one * GetSemicircleLayoutScale(oldSlot);
            data.endScales[i] = Vector3.one * GetSemicircleLayoutScale(newSlot);
        }

        return data;
    }

    /// <summary>
    /// Executes the shift animation based on the provided shift data.
    /// </summary>
    private IEnumerator ExecuteShiftAnimation(ShiftAnimationData data)
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = Mathf.SmoothStep(0f, 1f, elapsedTime / animationDuration);
            
            for (int i = 0; i < totalButtonCount; i++)
            {
                float currentAngle = Mathf.Lerp(data.startAngles[i], data.endAngles[i], normalizedTime);
                skillButtons[i].RectTransform.anchoredPosition = CalculateSemicirclePosition(currentAngle, layoutRadius);
                skillButtons[i].RectTransform.localScale = Vector3.Lerp(data.startScales[i], data.endScales[i], normalizedTime);
            }
            
            yield return null;
        }
    }

    /// <summary>
    /// Applies the new slot assignments and updates button positions and scales.
    /// </summary>
    private void ApplyShiftedSlotAssignments(int[] newSlotAssignments)
    {
        for (int i = 0; i < totalButtonCount; i++)
        {
            semicircleSlotAssignments[i] = newSlotAssignments[i];
            float angle = semicircleArcAnglePositions[semicircleSlotAssignments[i]];
            skillButtons[i].RectTransform.anchoredPosition = CalculateSemicirclePosition(angle, layoutRadius);
            skillButtons[i].RectTransform.localScale = Vector3.one * GetSemicircleLayoutScale(semicircleSlotAssignments[i]);
        }
    }

    /// <summary>
    /// Calculates the position of a button in a star layout.
    /// </summary>
    private Vector2 CalculateStarPosition(float degree, float radius)
    {
        float angleInRadians = (degree - CLOCKWISE_OFFSET_DEGREES) * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians)) * radius;
    }

    /// <summary>
    /// Calculates the position of a button in a semicircle layout.
    /// </summary>
    private Vector2 CalculateSemicirclePosition(float degree, float radius)
    {
        float angleInRadians = degree * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians)) * radius;
    }

    /// <summary>
    /// Interpolates between two circular positions.
    /// </summary>
    private Vector2 InterpolateCircularPosition(Vector2 startPos, Vector2 endPos, float t)
    {
        float startRadius = startPos.magnitude;
        float endRadius = endPos.magnitude;
        float startAngle = Mathf.Atan2(startPos.y, startPos.x) * Mathf.Rad2Deg;
        float endAngle = Mathf.Atan2(endPos.y, endPos.x) * Mathf.Rad2Deg;
        
        float angleDiff = endAngle - startAngle;
        if (angleDiff > 180f) angleDiff -= 360f;
        if (angleDiff < -180f) angleDiff += 360f;
        
        float interpolatedRadius = Mathf.Lerp(startRadius, endRadius, t);
        float interpolatedAngle = startAngle + angleDiff * t;
        
        float angleInRadians = interpolatedAngle * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians)) * interpolatedRadius;
    }

    /// <summary>
    /// Calculates the staggered time for button animations.
    /// </summary>
    private float CalculateStaggeredTime(Vector2 startPos, Vector2 endPos, float t, int buttonIndex)
    {
        float startAngle = Mathf.Atan2(startPos.y, startPos.x) * Mathf.Rad2Deg;
        float endAngle = Mathf.Atan2(endPos.y, endPos.x) * Mathf.Rad2Deg;
        
        float angleDiff = endAngle - startAngle;
        if (angleDiff > 180f) angleDiff -= 360f;
        if (angleDiff < -180f) angleDiff += 360f;
        
        bool isClockwise = angleDiff < 0;
        
        float staggerDelay = (buttonIndex * 0.01f) / totalButtonCount; 
        
        if (isClockwise)
        {
            float adjustedT = t - staggerDelay;
            return Mathf.Clamp01(adjustedT);
        }
        else
        {
            float reverseDelay = ((totalButtonCount - 1 - buttonIndex) * 0.01f) / totalButtonCount;
            float adjustedT = t - reverseDelay;
            return Mathf.Clamp01(adjustedT);
        }
    }

    /// <summary>
    /// Enables or disables raycast for all skill buttons.
    /// </summary>
    private void SetAllSkillButtonRaycast(bool enabled)
    {
        for (int i = 0; i < totalButtonCount; i++)
        {
            skillButtons[i].SetRaycast(enabled);
        }
    }

    /// <summary>
    /// Calculates the angle from a local position.
    /// </summary>
    private float CalculateAngleFromPosition(Vector2 localPosition)
    {
        return Mathf.Atan2(localPosition.y, localPosition.x) * Mathf.Rad2Deg;
    }
}
