namespace UnityEngine.EventSystems
{
    /// <summary>
    /// This is an 4 direction movement enum.
    /// </summary>
    /// <remarks>
    /// MoveDirection provides a way of switching between moving states. You must assign these states to actions, such as moving the GameObject by an up vector when in the Up state.
    /// Having states like these are easier to identify than always having to include a large amount of vectors and calculations.Instead, you define what you want the state to do in only one part, and switch to the appropriate state when it is needed.
    /// </remarks>
    /// <example>
    /// <code>
    /// <![CDATA[
    /// //This is a full example of how a GameObject changes direction using MoveDirection states
    /// //Assign this script to a visible GameObject (with a Rigidbody attached) to see it in action
    ///
    /// using UnityEngine;
    /// using UnityEngine.EventSystems;
    ///
    /// public class Example : MonoBehaviour
    /// {
    ///     Vector3 m_StartPosition, m_StartForce;
    ///     Rigidbody m_Rigidbody;
    ///     //Use Enum for easy switching between direction states
    ///     MoveDirection m_MoveDirection;
    ///
    ///     //Use these Vectors for moving Rigidbody components
    ///     Vector3 m_ResetVector;
    ///     Vector3 m_UpVector;
    ///     Vector3 m_RightVector;
    ///     const float speed = 5.0f;
    ///
    ///     void Start()
    ///     {
    ///         //You get the Rigidbody component attached to the GameObject
    ///         m_Rigidbody = GetComponent<Rigidbody>();
    ///         //This starts with the Rigidbody not moving in any direction at all
    ///         m_MoveDirection = MoveDirection.None;
    ///
    ///         //These are the GameObject’s starting position and Rigidbody position
    ///         m_StartPosition = transform.position;
    ///         m_StartForce = m_Rigidbody.transform.position;
    ///
    ///         //This Vector is set to 1 in the y axis (for moving upwards)
    ///         m_UpVector = Vector3.up;
    ///         //This Vector is set to 1 in the x axis (for moving in the right direction)
    ///         m_RightVector = Vector3.right;
    ///         //This Vector is zeroed out for when the Rigidbody should not move
    ///         m_ResetVector = Vector3.zero;
    ///     }
    ///
    ///     void Update()
    ///     {
    ///         //This switches the direction depending on button presses
    ///         switch (m_MoveDirection)
    ///         {
    ///             //The starting state which resets the object
    ///             case MoveDirection.None:
    ///                 //Reset to the starting position of the GameObject and Rigidbody
    ///                 transform.position = m_StartPosition;
    ///                 m_Rigidbody.transform.position = m_StartForce;
    ///                 //This resets the velocity of the Rigidbody
    ///                 m_Rigidbody.velocity = m_ResetVector;
    ///                 break;
    ///
    ///             //This is for moving in an upwards direction
    ///             case MoveDirection.Up:
    ///                 //Change the velocity so that the Rigidbody travels upwards
    ///                 m_Rigidbody.velocity = m_UpVector * speed;
    ///                 break;
    ///
    ///             //This is for moving left
    ///             case MoveDirection.Left:
    ///                 //This moves the Rigidbody to the left (minus right Vector)
    ///                 m_Rigidbody.velocity = -m_RightVector * speed;
    ///                 break;
    ///
    ///             //This is for moving right
    ///             case MoveDirection.Right:
    ///                 //This moves the Rigidbody to the right
    ///                 m_Rigidbody.velocity = m_RightVector * speed;
    ///                 break;
    ///
    ///             //This is for moving down
    ///             case MoveDirection.Down:
    ///                 //This moves the Rigidbody down
    ///                 m_Rigidbody.velocity = -m_UpVector * speed;
    ///                 break;
    ///         }
    ///     }
    ///
    ///     void OnGUI()
    ///     {
    ///         //Press the reset Button to switch to no mode
    ///         if (GUI.Button(new Rect(100, 0, 150, 30), "Reset"))
    ///         {
    ///             //Switch to start/reset case
    ///             m_MoveDirection = MoveDirection.None;
    ///         }
    ///
    ///         //Press the Left button to switch the Rigidbody direction to the left
    ///         if (GUI.Button(new Rect(100, 30, 150, 30), "Move Left"))
    ///         {
    ///             //Switch to the left direction
    ///             m_MoveDirection = MoveDirection.Left;
    ///         }
    ///
    ///         //Press the Up button to switch the Rigidbody direction to upwards
    ///         if (GUI.Button(new Rect(100, 60, 150, 30), "Move Up"))
    ///         {
    ///             //Switch to Up Direction
    ///             m_MoveDirection = MoveDirection.Up;
    ///         }
    ///
    ///         //Press the Down button to switch the direction to down
    ///         if (GUI.Button(new Rect(100, 90, 150, 30), "Move Down"))
    ///         {
    ///             //Switch to Down Direction
    ///             m_MoveDirection = MoveDirection.Down;
    ///         }
    ///
    ///         //Press the right button to switch to the right direction
    ///         if (GUI.Button(new Rect(100, 120, 150, 30), "Move Right"))
    ///         {
    ///             //Switch to Right Direction
    ///             m_MoveDirection = MoveDirection.Right;
    ///         }
    ///     }
    /// }
    /// ]]>
    ///</code>
    /// </example>
    public enum MoveDirection
    {
        /// <summary>
        /// This is the Left state of MoveDirection.  Assign functionality for moving to the left.
        /// </summary>
        /// <remarks>
        /// Use the Left state for an easily identifiable way of moving a GameObject to the left (-1 , 0 , 0). This is a state without any predefined functionality. Before using this state, you should define what your GameObject will do in code.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// //Assign this script to a visible GameObject (with a Rigidbody attached) to see this in action
        ///
        /// using UnityEngine;
        /// using UnityEngine.EventSystems;
        ///
        /// public class Example : MonoBehaviour
        /// {
        ///     Vector3 m_StartPosition, m_StartForce;
        ///     Rigidbody m_Rigidbody;
        ///     //Use Enum for easy switching between direction states
        ///     MoveDirection m_MoveDirection;
        ///
        ///     //Use these Vectors for moving Rigidbody components
        ///     Vector3 m_ResetVector;
        ///     Vector3 m_RightVector;
        ///     const float speed = 5.0f;
        ///
        ///     void Start()
        ///     {
        ///         //You get the Rigidbody component attached to the GameObject
        ///         m_Rigidbody = GetComponent<Rigidbody>();
        ///         //This starts with the Rigidbody not moving in any direction at all
        ///         m_MoveDirection = MoveDirection.None;
        ///
        ///         //These are the GameObject’s starting position and Rigidbody position
        ///         m_StartPosition = transform.position;
        ///         m_StartForce = m_Rigidbody.transform.position;
        ///
        ///         //This Vector is set to 1 in the x axis (for moving in the right direction)
        ///         m_RightVector = Vector3.right;
        ///         //This Vector is zeroed out for when the Rigidbody should not move
        ///         m_ResetVector = Vector3.zero;
        ///     }
        ///
        ///     void Update()
        ///     {
        ///         //This switches the direction depending on button presses
        ///         switch (m_MoveDirection)
        ///         {
        ///             //The starting state which resets the object
        ///             case MoveDirection.None:
        ///                 //Reset to the starting position of the GameObject and Rigidbody
        ///                 transform.position = m_StartPosition;
        ///                 m_Rigidbody.transform.position = m_StartForce;
        ///                 //This resets the velocity of the Rigidbody
        ///                 m_Rigidbody.velocity = m_ResetVector;
        ///                 break;
        ///
        ///             //This is for moving left
        ///             case MoveDirection.Left:
        ///                 //This moves the Rigidbody to the left (minus right Vector)
        ///                 m_Rigidbody.velocity = -m_RightVector * speed;
        ///                 break;
        ///         }
        ///     }
        ///
        ///     void OnGUI()
        ///     {
        ///         //Press the reset Button to switch to no mode
        ///         if (GUI.Button(new Rect(100, 0, 150, 30), "Reset"))
        ///         {
        ///             //Switch to start/reset case
        ///             m_MoveDirection = MoveDirection.None;
        ///         }
        ///
        ///         //Press the Left button to switch the Rigidbody direction to the left
        ///         if (GUI.Button(new Rect(100, 30, 150, 30), "Move Left"))
        ///         {
        ///             //Switch to the left direction
        ///             m_MoveDirection = MoveDirection.Left;
        ///         }
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        Left,

        /// <summary>
        /// This is the Up state of MoveDirection.  Assign functionality for moving in an upward direction.
        /// </summary>
        /// <remarks>
        /// Use the Up state for an easily identifiable way of moving a GameObject upwards (0 , 1 , 0). This is a state without any predefined functionality. Before using this state, you should define what your GameObject will do in code.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// //Attach this script to a GameObject with a Rigidbody component. Press the "Move Up" button in Game view to see it in action.
        ///
        /// using UnityEngine;
        /// using UnityEngine.EventSystems;
        ///
        /// public class Example : MonoBehaviour
        /// {
        ///     Vector3 m_StartPosition, m_StartForce;
        ///     Rigidbody m_Rigidbody;
        ///     //Use Enum for easy switching between direction states
        ///     MoveDirection m_MoveDirection;
        ///
        ///     //Use these Vectors for moving Rigidbody components
        ///     Vector3 m_ResetVector;
        ///     Vector3 m_UpVector;
        ///     const float speed = 10.0f;
        ///
        ///     void Start()
        ///     {
        ///         //You get the Rigidbody component attached to the GameObject
        ///         m_Rigidbody = GetComponent<Rigidbody>();
        ///         //This starts with the Rigidbody not moving in any direction at all
        ///         m_MoveDirection = MoveDirection.None;
        ///
        ///         //These are the GameObject’s starting position and Rigidbody position
        ///         m_StartPosition = transform.position;
        ///         m_StartForce = m_Rigidbody.transform.position;
        ///
        ///         //This Vector is set to 1 in the y axis (for moving upwards)
        ///         m_UpVector = Vector3.up;
        ///         //This Vector is zeroed out for when the Rigidbody should not move
        ///         m_ResetVector = Vector3.zero;
        ///     }
        ///
        ///     void Update()
        ///     {
        ///         //This switches the direction depending on button presses
        ///         switch (m_MoveDirection)
        ///         {
        ///             //The starting state which resets the object
        ///             case MoveDirection.None:
        ///                 //Reset to the starting position of the GameObject and Rigidbody
        ///                 transform.position = m_StartPosition;
        ///                 m_Rigidbody.transform.position = m_StartForce;
        ///                 //This resets the velocity of the Rigidbody
        ///                 m_Rigidbody.velocity = m_ResetVector;
        ///                 break;
        ///
        ///             //This is for moving in an upwards direction
        ///             case MoveDirection.Up:
        ///                 //Change the velocity so that the Rigidbody travels upwards
        ///                 m_Rigidbody.velocity = m_UpVector * speed;
        ///                 break;
        ///         }
        ///     }
        ///
        ///     void OnGUI()
        ///     {
        ///         //Press the reset Button to switch to no mode
        ///         if (GUI.Button(new Rect(100, 0, 150, 30), "Reset"))
        ///         {
        ///             //Switch to start/reset case
        ///             m_MoveDirection = MoveDirection.None;
        ///         }
        ///
        ///         //Press the Up button to switch the Rigidbody direction to upwards
        ///         if (GUI.Button(new Rect(100, 60, 150, 30), "Move Up"))
        ///         {
        ///             //Switch to Up Direction
        ///             m_MoveDirection = MoveDirection.Up;
        ///         }
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        Up,

        /// <summary>
        /// This is the Right state of MoveDirection. Assign functionality for moving to the right.
        /// </summary>
        /// <remarks>
        /// Use the Right state for an easily identifiable way of moving a GameObject to the right (1 , 0 , 0). This is a state without any predefined functionality. Before using this state, you should define what your GameObject will do in code.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// //Attach this script to a GameObject with a Rigidbody component. Press the "Move Right" button in Game view to see it in action.
        ///
        /// using UnityEngine;
        /// using UnityEngine.EventSystems;
        ///
        /// public class MoveDirectionExample : MonoBehaviour
        /// {
        ///     Vector3 m_StartPosition, m_StartForce;
        ///     Rigidbody m_Rigidbody;
        ///     //Use Enum for easy switching between direction states
        ///     MoveDirection m_MoveDirection;
        ///
        ///     //Use these Vectors for moving Rigidbody components
        ///     Vector3 m_ResetVector;
        ///     Vector3 m_RightVector;
        ///     const float speed = 5.0f;
        ///
        ///     void Start()
        ///     {
        ///         //You get the Rigidbody component attached to the GameObject
        ///         m_Rigidbody = GetComponent<Rigidbody>();
        ///         //This starts with the Rigidbody not moving in any direction at all
        ///         m_MoveDirection = MoveDirection.None;
        ///
        ///         //These are the GameObject’s starting position and Rigidbody position
        ///         m_StartPosition = transform.position;
        ///         m_StartForce = m_Rigidbody.transform.position;
        ///
        ///         //This Vector is set to 1 in the x axis (for moving in the right direction)
        ///         m_RightVector = Vector3.right;
        ///         //This Vector is zeroed out for when the Rigidbody should not move
        ///         m_ResetVector = Vector3.zero;
        ///     }
        ///
        ///     void Update()
        ///     {
        ///         //This switches the direction depending on button presses
        ///         switch (m_MoveDirection)
        ///         {
        ///             //The starting state which resets the object
        ///             case MoveDirection.None:
        ///                 //Reset to the starting position of the GameObject and Rigidbody
        ///                 transform.position = m_StartPosition;
        ///                 m_Rigidbody.transform.position = m_StartForce;
        ///                 //This resets the velocity of the Rigidbody
        ///                 m_Rigidbody.velocity = m_ResetVector;
        ///                 break;
        ///
        ///             //This is for moving right
        ///             case MoveDirection.Right:
        ///                 //This moves the Rigidbody to the right
        ///                 m_Rigidbody.velocity = m_RightVector * speed;
        ///                 break;
        ///         }
        ///     }
        ///
        ///     void OnGUI()
        ///     {
        ///         //Press the reset Button to switch to no mode
        ///         if (GUI.Button(new Rect(100, 0, 150, 30), "Reset"))
        ///         {
        ///             //Switch to start/reset case
        ///             m_MoveDirection = MoveDirection.None;
        ///         }
        ///
        ///         //Press the Left button to switch the Rigidbody direction to the right
        ///         if (GUI.Button(new Rect(100, 30, 150, 30), "Move Right"))
        ///         {
        ///             //Switch to the left direction
        ///             m_MoveDirection = MoveDirection.Right;
        ///         }
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        Right,

        /// <summary>
        /// The Down State of MoveDirection. Assign functionality for moving in a downward direction.
        /// </summary>
        /// <remarks>
        /// Use the Down state for an easily identifiable way of moving a GameObject downwards (0 , -1 , 0). This is a state without any predefined functionality. Before using this state, you should define what your GameObject will do in code.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// //Attach this script to a GameObject with a Rigidbody component. Press the "Move Down" button in Game view to see it in action.
        ///
        /// using UnityEngine;
        /// using UnityEngine.EventSystems;
        ///
        /// public class Example : MonoBehaviour
        /// {
        ///     Vector3 m_StartPosition, m_StartForce;
        ///     Rigidbody m_Rigidbody;
        ///     //Use Enum for easy switching between direction states
        ///     MoveDirection m_MoveDirection;
        ///
        ///     //Use these Vectors for moving Rigidbody components
        ///     Vector3 m_ResetVector;
        ///     Vector3 m_UpVector;
        ///     const float speed = 10.0f;
        ///
        ///     void Start()
        ///     {
        ///         //You get the Rigidbody component attached to the GameObject
        ///         m_Rigidbody = GetComponent<Rigidbody>();
        ///         //This starts with the Rigidbody not moving in any direction at all
        ///         m_MoveDirection = MoveDirection.None;
        ///
        ///         //These are the GameObject’s starting position and Rigidbody position
        ///         m_StartPosition = transform.position;
        ///         m_StartForce = m_Rigidbody.transform.position;
        ///
        ///         //This Vector is set to 1 in the y axis (for moving upwards)
        ///         m_UpVector = Vector3.up;
        ///         //This Vector is zeroed out for when the Rigidbody should not move
        ///         m_ResetVector = Vector3.zero;
        ///     }
        ///
        ///     void Update()
        ///     {
        ///         //This switches the direction depending on button presses
        ///         switch (m_MoveDirection)
        ///         {
        ///             //The starting state which resets the object
        ///             case MoveDirection.None:
        ///                 //Reset to the starting position of the GameObject and Rigidbody
        ///                 transform.position = m_StartPosition;
        ///                 m_Rigidbody.transform.position = m_StartForce;
        ///                 //This resets the velocity of the Rigidbody
        ///                 m_Rigidbody.velocity = m_ResetVector;
        ///                 break;
        ///
        ///             //This is for moving down
        ///             case MoveDirection.Down:
        ///                 //This moves the Rigidbody down
        ///                 m_Rigidbody.velocity = -m_UpVector * speed;
        ///                 break;
        ///         }
        ///     }
        ///
        ///     void OnGUI()
        ///     {
        ///         //Press the reset Button to switch to no mode
        ///         if (GUI.Button(new Rect(100, 0, 150, 30), "Reset"))
        ///         {
        ///             //Switch to start/reset case
        ///             m_MoveDirection = MoveDirection.None;
        ///         }
        ///
        ///         //Press the Down button to switch the direction to down
        ///         if (GUI.Button(new Rect(100, 90, 150, 30), "Move Down"))
        ///         {
        ///             //Switch to Down Direction
        ///             m_MoveDirection = MoveDirection.Down;
        ///         }
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        Down,

        /// <summary>
        /// This is the None state. Assign functionality that stops movement.
        /// </summary>
        /// <remarks>
        /// Use the None state for an easily identifiable way of stopping, resetting or initialising a GameObject's movement. This is a state without any predefined functionality. Before using this state, you should define what your GameObject will do in code.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// //Attach this script to a GameObject with a Rigidbody attached. This script starts off on the ModeDirection.None state but changes depending on buttons you press.
        ///
        /// using UnityEngine;
        /// using UnityEngine.EventSystems;
        ///
        /// public class Example : MonoBehaviour
        /// {
        ///     Vector3 m_StartPosition, m_StartForce;
        ///     Rigidbody m_Rigidbody;
        ///     //Use Enum for easy switching between direction states
        ///     MoveDirection m_MoveDirection;
        ///
        ///     //Use these Vectors for moving Rigidbody components
        ///     Vector3 m_ResetVector;
        ///     Vector3 m_UpVector;
        ///     const float speed = 10.0f;
        ///
        ///     void Start()
        ///     {
        ///         //You get the Rigidbody component attached to the GameObject
        ///         m_Rigidbody = GetComponent<Rigidbody>();
        ///         //This starts with the Rigidbody not moving in any direction at all
        ///         m_MoveDirection = MoveDirection.None;
        ///
        ///         //These are the GameObject’s starting position and Rigidbody position
        ///         m_StartPosition = transform.position;
        ///         m_StartForce = m_Rigidbody.transform.position;
        ///
        ///         //This Vector is set to 1 in the y axis (for moving upwards)
        ///         m_UpVector = Vector3.up;
        ///         //This Vector is zeroed out for when the Rigidbody should not move
        ///         m_ResetVector = Vector3.zero;
        ///     }
        ///
        ///     void Update()
        ///     {
        ///         //This switches the direction depending on button presses
        ///         switch (m_MoveDirection)
        ///         {
        ///             //The starting state which resets the object
        ///             case MoveDirection.None:
        ///                 //Reset to the starting position of the GameObject and Rigidbody
        ///                 transform.position = m_StartPosition;
        ///                 m_Rigidbody.transform.position = m_StartForce;
        ///                 //This resets the velocity of the Rigidbody
        ///                 m_Rigidbody.velocity = m_ResetVector;
        ///                 break;
        ///
        ///             //This is for moving down
        ///             case MoveDirection.Down:
        ///                 //This moves the Rigidbody down
        ///                 m_Rigidbody.velocity = -m_UpVector * speed;
        ///                 break;
        ///         }
        ///     }
        ///
        ///     void OnGUI()
        ///     {
        ///         //Press the reset Button to switch to no mode
        ///         if (GUI.Button(new Rect(100, 0, 150, 30), "Reset"))
        ///         {
        ///             //Switch to start/reset case
        ///             m_MoveDirection = MoveDirection.None;
        ///         }
        ///
        ///         //Press the Down button to switch the direction to down
        ///         if (GUI.Button(new Rect(100, 90, 150, 30), "Move Down"))
        ///         {
        ///             //Switch to Down Direction
        ///             m_MoveDirection = MoveDirection.Down;
        ///         }
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        None
    }
}
