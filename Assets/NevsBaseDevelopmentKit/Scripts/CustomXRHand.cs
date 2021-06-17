/************************************************************************************

Copyright (c) Facebook Technologies, LLC and its affiliates. All rights reserved.  

See SampleFramework license.txt for license terms.  Unless required by applicable law 
or agreed to in writing, the sample code is provided “AS IS” WITHOUT WARRANTIES OR 
CONDITIONS OF ANY KIND, either express or implied.  See the license for specific 
language governing permissions and limitations under the license.

************************************************************************************/

using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;
using System;

// Animated hand visuals for a user of a Touch controller.
public class CustomXRHand : MonoBehaviour
{
    // ANIMATION LAYER NAMES
    public const string ANIM_LAYER_NAME_POINT = "Point Layer";
    public const string ANIM_LAYER_NAME_THUMB = "Thumb Layer";
    public const string ANIM_PARAM_NAME_FLEX = "Flex";
    public const string ANIM_PARAM_NAME_POSE = "Pose";

    // CONSTANTS
    public const float INPUT_RATE_CHANGE = 20.0f;
    public const float COLLIDER_SCALE_MIN = 0.01f;
    public const float COLLIDER_SCALE_MAX = 1.0f;
    public const float COLLIDER_SCALE_PER_SECOND = 3.00f;
    public const float COLLIDER_SCALE_WAIT = 0.2f;

    [Header("Inputs and Animation")]
    [SerializeField]
    [Tooltip("This is which device that inputs should be read from. Ideally either \"Right | Controller\" or \"Left | Controller\"")]
    private XRNode m_controllerCharacteristics;
    [SerializeField]
    [Tooltip("This is which device that inputs should be read from. Ideally either \"Right | Controller\" or \"Left | Controller\"")]
    private Animator m_animator = null;
    [SerializeField]
    [Tooltip("This is which device that inputs should be read from. Ideally either \"Right | Controller\" or \"Left | Controller\"")]
    private CustomHandPose m_defaultGrabPose = null;

    [Header("Physics/Colliders")]
    [SerializeField]
    [Tooltip("Determines if \"CustomXRHand.cs\" should keep track of colliders on the hands. Used for disabling when throwing objects. NOTE: the transform scale on each collider should be 1 for the scale over time to work properly.")]
    private bool m_TrackCollisions = false;
    
    /// <summary>
    /// Keeps track of the current state of the colliders for updates.
    /// </summary>
    private bool m_collisionEnabled = false;
    /// <summary>
    /// The colliders attached to the hand objects
    /// </summary>
    private Collider[] m_colliders = null;

    /// <summary>
    /// This is which device that inputs should be read from. Ideally either "Right Controller" or "Left Controller"
    /// </summary>
    private InputDeviceWrapper targetDevice;
    /// <summary>
    /// Used for animation and collider handling.
    /// </summary>
    private bool isGrabbing;
    /// <summary>
    /// Used for object specific gabbing poses.
    /// </summary>
    private XRDirectInteractor grabberObj;

    // ANIMATION LAYER VALUES
    // The input values determine how the animator blends these different layers. Look at "UpdateAnimStates()" for specifics.
    private int m_animLayerIndexThumb = -1;
    private int m_animLayerIndexPoint = -1;
    private int m_animParamIndexFlex = -1;
    private int m_animParamIndexPose = -1;

    // INPUT VALUES
    /// <summary>
    /// Boolean for if the finger is on the trigger.
    /// </summary>
    private bool m_isPointing = false;
    /// <summary>
    /// Boolean for if the thumb is NOT on any of the touch buttons (: thumbsticks, A, X, B, or Y).
    /// </summary>
    private bool m_isGivingThumbsUp = false;
    /// <summary>
    /// The current blend between when the finger was not on the trigger and when it was. This is to prevent the hand model from snapping between two states by blending the change over time.
    /// The time is determined by the constant "CustomXRHand.Input_Rate_Change".
    /// </summary>
    private float m_pointBlend = 0.0f;
    /// <summary>
    /// The current blend between when the thumb was not on the controller buttons and when it was. This is to prevent the hand model from snapping between two states by blending the change over time.
    /// The time is determined by the constant "CustomXRHand.Input_Rate_Change".
    /// </summary>
    private float m_thumbsUpBlend = 0.0f;
    /// <summary>
    /// This is the grip button value, representing how the lower three fingers are "flexed".
    /// </summary>
    private float m_flex = 0.0f;
    /// <summary>
    /// This is the trigger button value, representing how far the index finger is being pushed in.
    /// </summary>
    private float m_pinch = 0.0f;

    private void Start()
    {
        InputDeviceWrapper.FindInputDevice(m_controllerCharacteristics, out targetDevice, this);

        //TODO:: Update this so it is not dependent on this specific instanciation.
        grabberObj = transform.parent.GetComponentInParent<XRDirectInteractor>();

        // Set up colliders for collisions.
        m_collisionEnabled = false;
        if(m_TrackCollisions)
        {
            // Collisions start disabled. We'll enable it based on custom collision requirement in the update function.
            m_colliders = this.GetComponentsInChildren<Collider>().Where(childCollider => !childCollider.isTrigger).ToArray();
            CollisionEnable(m_collisionEnabled);
        }

        // We start with no object in the users hands
        isGrabbing = false;

        // Get animator layer indices by name, for later use switching between hand visuals
        m_animLayerIndexPoint = m_animator.GetLayerIndex(ANIM_LAYER_NAME_POINT);
        m_animLayerIndexThumb = m_animator.GetLayerIndex(ANIM_LAYER_NAME_THUMB);
        m_animParamIndexFlex = Animator.StringToHash(ANIM_PARAM_NAME_FLEX);
        m_animParamIndexPose = Animator.StringToHash(ANIM_PARAM_NAME_POSE);

    }

    private void Update()
    {
        if(grabberObj)
        {
            isGrabbing = grabberObj.selectTarget != null;
        }
            
        UpdateInputStates();
            
        if (m_TrackCollisions)
        {
            // Custom collision enabling. Always true when not grabbing.
            bool collisionEnabled = isGrabbing == false;
            CollisionEnable(collisionEnabled);
            // ---------------------------------------------------------
        }



        UpdateAnimStates();
        
    }

    /// <summary>
    /// Updates the controller inputs that map to specific hand animation pose layers.
    /// </summary>
    private void UpdateInputStates()
    {
        m_isPointing = !(targetDevice.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerTouch) && triggerTouch);
        m_isGivingThumbsUp = !(
            (targetDevice.TryGetFeatureValue(CommonUsages.primary2DAxisTouch, out bool thumbstickTouch) && thumbstickTouch) ||
            (targetDevice.TryGetFeatureValue(CommonUsages.primaryTouch, out bool AXTouch) && AXTouch) ||
            (targetDevice.TryGetFeatureValue(CommonUsages.secondaryTouch, out bool BXTouch) && BXTouch)
            );
        
        // Blend the boolean values:
        m_pointBlend = InputValueRateChange(m_isPointing, m_pointBlend);
        m_thumbsUpBlend = InputValueRateChange(m_isGivingThumbsUp, m_thumbsUpBlend);

        targetDevice.TryGetFeatureValue(CommonUsages.grip, out m_flex);
        targetDevice.TryGetFeatureValue(CommonUsages.trigger, out m_pinch);
    }

    /// <summary>
    /// If hand collisions are enabled, and the colliders are not at max size, then scale up all colliders.
    /// If collision tracking is disabled, collisions are always disabled.
    /// </summary>
    private void LateUpdate()
    {
        // Hand's collision grows over a short amount of time on enable, rather than snapping to on, to help somewhat with interpenetration issues.
        if (m_collisionEnabled && m_collisionScaleCurrent + Mathf.Epsilon < COLLIDER_SCALE_MAX)
        {
            m_collisionScaleCurrent = Mathf.Min(COLLIDER_SCALE_MAX, m_collisionScaleCurrent + Time.deltaTime * COLLIDER_SCALE_PER_SECOND);
            for (int i = 0; i < m_colliders.Length; ++i)
            {
                Collider collider = m_colliders[i];
                collider.transform.localScale = new Vector3(m_collisionScaleCurrent, m_collisionScaleCurrent, m_collisionScaleCurrent);
            }
        }
    }

    /// <summary>
    /// Updates "Value" by the Input_Rate_Change. This is to prevent the hand model from snapping between two boolean states by blending the change over time. This changes is positive when "isDown" is true, negative otherwise.
    /// </summary>
    /// <param name="isDown">If the value should increase.</param>
    /// <param name="value">The current value (clamped between 0 and 1).</param>
    /// <returns></returns>
    private float InputValueRateChange(bool isDown, float value)
    {
        float rateDelta = Time.deltaTime * INPUT_RATE_CHANGE;
        float sign = isDown ? 1.0f : -1.0f;
        return Mathf.Clamp01(value + rateDelta * sign);
    }

    /// <summary>
    /// Uses the input values to update the animation layers. In addition, uses the grabbed object to update the default pose the hand should conform with.
    /// </summary>
    private void UpdateAnimStates()
    {
        CustomHandPose grabPose = m_defaultGrabPose;
        if (isGrabbing)
        {
            //TODO:: Grabbing different objects. This is where we can set different default poses depending on what the user is holding.
            // If dynamic gripping is wanted, use Animation rigs (finger IKs and animation blending with raycasting).
        }

        // Pose
        CustomHandPoseID handPoseId = grabPose.PoseId;
        m_animator.SetInteger(m_animParamIndexPose, (int)handPoseId);

        // Flex
        // blend between open hand and fully closed fist
        m_animator.SetFloat(m_animParamIndexFlex, m_flex);

        // Point
        bool canPoint = !isGrabbing || grabPose.AllowPointing;
        float point = canPoint ? m_pointBlend : 0.0f;
        m_animator.SetLayerWeight(m_animLayerIndexPoint, point);

        // Thumbs up
        bool canThumbsUp = !isGrabbing || grabPose.AllowThumbsUp;
        float thumbsUp = canThumbsUp ? m_thumbsUpBlend : 0.0f;
        m_animator.SetLayerWeight(m_animLayerIndexThumb, thumbsUp);

        m_animator.SetFloat("Pinch", m_pinch);
        
    }
    
    /// <summary>
    /// Keeps track to the current scale on the colliders.
    /// </summary>
    private float m_collisionScaleCurrent = 0.0f;
    /// <summary>
    /// Semaphore of sorts, used to only update the last call of CollisionEnabled.
    /// </summary>
    private int collisionWaitCount = 0;

    /// <summary>
    /// Enables/Disables collisions by scalling colliders.
    /// </summary>
    /// <param name="enabled">If true, colliders are scaled over a period of time after a small wait period (see LateUpdate and ColliderScaleWait). If false, colliders are immediatly disabled.</param>
    private void CollisionEnable(bool enabled)
    {
        if (m_collisionEnabled == enabled)
        {
            return;
        }
        m_collisionEnabled = enabled;

        if (enabled)
        {
            StartCoroutine(ColliderScaleWait());
        }
        else
        {

            foreach (Collider collider in m_colliders)
                collider.enabled = false;

            // The current collider scale is kept at max while the colliders are disabled. This is to prevent the LateUpdate function from updating colliders that aren't enabled.
            // Note that the LateUpdate function would only update the colliders once collisions are enabled, however, collisions are enabled for a period of time before we actually
            // want to start scalling the colliders.
            m_collisionScaleCurrent = COLLIDER_SCALE_MAX;
        }
    }

    /// <summary>
    /// We an object is let go, the hand colliders should not interact with the object until the object has left the hands.
    /// This is a simple method that waits to scale the colliders to prevent collisions. The actual scalling happens in the late update, doing so prevents multiple
    /// coroutine calls from providing unwanted outcomes.
    /// </summary>
    /// <returns>This function is a coroutine. The return is for threading.</returns>
    private IEnumerator ColliderScaleWait()
    {
        collisionWaitCount++; // Add to queue

        yield return new WaitForSeconds(COLLIDER_SCALE_WAIT); // wait

        if (m_collisionEnabled == enabled && collisionWaitCount <= 1) // if only function in queue, and collisions should still be enabled
        {
            // Enable all collider components, and set the scale to MIN.
            m_collisionScaleCurrent = COLLIDER_SCALE_MIN;
            for (int i = 0; i < m_colliders.Length; ++i)
            {
                Collider collider = m_colliders[i];
                collider.transform.localScale = new Vector3(COLLIDER_SCALE_MIN, COLLIDER_SCALE_MIN, COLLIDER_SCALE_MIN);
                collider.enabled = true;
            }
        }
        collisionWaitCount--; // Remove from queue

    }
}
