using UnityEngine.UI;

namespace UnityEngine.EventSystems
{
    /// <summary>
    /// Interface to the Input system used by the BaseInputModule. With this it is possible to bypass the Input system with your own but still use the same InputModule. For example this can be used to feed fake input into the UI or interface with a different input system.
    /// </summary>
    public class BaseInput : UIBehaviour
    {
        /// <summary>
        /// Interface to Input.compositionString. Can be overridden to provide custom input instead of using the Input class.
        /// </summary>
        public virtual string compositionString
        {
            get { return Input.compositionString; }
        }

        /// <summary>
        /// Interface to Input.imeCompositionMode. Can be overridden to provide custom input instead of using the Input class.
        /// </summary>
        public virtual IMECompositionMode imeCompositionMode
        {
            get { return Input.imeCompositionMode; }
            set { Input.imeCompositionMode = value; }
        }

        /// <summary>
        /// Interface to Input.compositionCursorPos. Can be overridden to provide custom input instead of using the Input class.
        /// </summary>
        public virtual Vector2 compositionCursorPos
        {
            get { return Input.compositionCursorPos; }
            set { Input.compositionCursorPos = value; }
        }

        /// <summary>
        /// Interface to Input.mousePresent. Can be overridden to provide custom input instead of using the Input class.
        /// </summary>
        public virtual bool mousePresent
        {
            get { return Input.mousePresent; }
        }

        /// <summary>
        /// Interface to Input.GetMouseButtonDown. Can be overridden to provide custom input instead of using the Input class.
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public virtual bool GetMouseButtonDown(int button)
        {
            return Input.GetMouseButtonDown(button);
        }

        /// <summary>
        /// Interface to Input.GetMouseButtonUp. Can be overridden to provide custom input instead of using the Input class.
        /// </summary>
        public virtual bool GetMouseButtonUp(int button)
        {
            return Input.GetMouseButtonUp(button);
        }

        /// <summary>
        /// Interface to Input.GetMouseButton. Can be overridden to provide custom input instead of using the Input class.
        /// </summary>
        public virtual bool GetMouseButton(int button)
        {
            return Input.GetMouseButton(button);
        }

        /// <summary>
        /// Interface to Input.mousePosition. Can be overridden to provide custom input instead of using the Input class.
        /// </summary>
        public virtual Vector2 mousePosition
        {
            get { return Input.mousePosition; }
        }

        /// <summary>
        /// Interface to Input.mouseScrollDelta. Can be overridden to provide custom input instead of using the Input class.
        /// </summary>
        public virtual Vector2 mouseScrollDelta
        {
            get { return Input.mouseScrollDelta; }
        }

        /// <summary>
        /// Interface to Input.touchSupported. Can be overridden to provide custom input instead of using the Input class.
        /// </summary>
        public virtual bool touchSupported
        {
            get { return Input.touchSupported; }
        }

        /// <summary>
        /// Interface to Input.touchCount. Can be overridden to provide custom input instead of using the Input class.
        /// </summary>
        public virtual int touchCount
        {
            get { return Input.touchCount; }
        }

        /// <summary>
        /// Interface to Input.GetTouch. Can be overridden to provide custom input instead of using the Input class.
        /// </summary>
        /// <param name="index">Touch index to get</param>
        public virtual Touch GetTouch(int index)
        {
            return Input.GetTouch(index);
        }

        /// <summary>
        /// Interface to Input.GetAxisRaw. Can be overridden to provide custom input instead of using the Input class.
        /// </summary>
        /// <param name="axisName">Axis name to check</param>
        public virtual float GetAxisRaw(string axisName)
        {
            return Input.GetAxisRaw(axisName);
        }

        /// <summary>
        /// Interface to Input.GetButtonDown. Can be overridden to provide custom input instead of using the Input class.
        /// </summary>
        /// <param name="buttonName">Button name to get</param>
        public virtual bool GetButtonDown(string buttonName)
        {
            return Input.GetButtonDown(buttonName);
        }
    }
}
