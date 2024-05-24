using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine.EventSystems;

namespace UnityEngine.UI.Tests
{
    public static class UIBehaviourExtensions
    {
        private static object InvokeMethodAndRethrow(Type type, Object obj, string methodName, params object[] args)
        {
            BindingFlags flags = BindingFlags.Default;
            flags |= BindingFlags.Public;
            flags |= BindingFlags.NonPublic;
            if (obj != null)
                flags |= BindingFlags.Instance;
            else
                flags |= BindingFlags.Static;
            MethodInfo method;

            try
            {
                // Attempt to get the method plainly at first
                method = type.GetMethod(methodName, flags);
            }
            catch (AmbiguousMatchException)
            {
                // If it's ambiguous, then attempt to get it using its params (though nulls would mess things up).
                method = type.GetMethod(methodName, flags, null, args.Select(a => a != null ? a.GetType() : null).Where(a => a != null).ToArray(), new ParameterModifier[0]);
            }
            Assert.NotNull(method, string.Format("Not method {0} found on object {1}", methodName, obj));
            return method.Invoke(obj, args);
        }

        public static object InvokeMethodAndRethrow<T>(Object obj, string methodName, params object[] args)
        {
            return InvokeMethodAndRethrow(typeof(T), obj, methodName, args);
        }

        public static object InvokeMethodAndRethrow(Object obj, string methodName, params object[] args)
        {
            return InvokeMethodAndRethrow(obj.GetType(), obj, methodName, args);
        }

        public static void InvokeOnEnable(this UIBehaviour behaviour)
        {
            InvokeMethodAndRethrow(behaviour, "OnEnable");
        }

        public static void InvokeOnDisable(this UIBehaviour behaviour)
        {
            InvokeMethodAndRethrow(behaviour, "OnDisable");
        }

        public static void InvokeAwake(this UIBehaviour behaviour)
        {
            InvokeMethodAndRethrow(behaviour, "Awake");
        }

        public static void InvokeRebuild(this UIBehaviour behaviour, CanvasUpdate type)
        {
            InvokeMethodAndRethrow(behaviour, "Rebuild", type);
        }

        public static void InvokeLateUpdate(this UIBehaviour behaviour)
        {
            InvokeMethodAndRethrow(behaviour, "LateUpdate");
        }

        public static void InvokeUpdate(this UIBehaviour behaviour)
        {
            InvokeMethodAndRethrow(behaviour, "Update");
        }

        public static void InvokeOnRectTransformDimensionsChange(this UIBehaviour behaviour)
        {
            InvokeMethodAndRethrow(behaviour, "OnRectTransformDimensionsChange");
        }

        public static void InvokeOnCanvasGroupChanged(this UIBehaviour behaviour)
        {
            InvokeMethodAndRethrow(behaviour, "OnCanvasGroupChanged");
        }

        public static void InvokeOnDidApplyAnimationProperties(this UIBehaviour behaviour)
        {
            InvokeMethodAndRethrow(behaviour, "OnDidApplyAnimationProperties");
        }
    }

    public static class SelectableExtensions
    {
        public static void InvokeOnPointerDown(this Selectable selectable, PointerEventData data)
        {
            UIBehaviourExtensions.InvokeMethodAndRethrow<Selectable>(selectable, "OnPointerDown", data);
        }

        public static void InvokeOnPointerUp(this Selectable selectable, PointerEventData data)
        {
            UIBehaviourExtensions.InvokeMethodAndRethrow<Selectable>(selectable, "OnPointerUp", data);
        }

        public static void InvokeOnPointerEnter(this Selectable selectable, PointerEventData data)
        {
            UIBehaviourExtensions.InvokeMethodAndRethrow<Selectable>(selectable, "OnPointerEnter", data);
        }

        public static void InvokeOnPointerExit(this Selectable selectable, PointerEventData data)
        {
            UIBehaviourExtensions.InvokeMethodAndRethrow<Selectable>(selectable, "OnPointerExit", data);
        }

        public static void InvokeTriggerAnimation(this Selectable selectable, string triggerName)
        {
            UIBehaviourExtensions.InvokeMethodAndRethrow<Selectable>(selectable, "TriggerAnimation", triggerName);
        }

        public static void InvokeOnSelect(this Selectable selectable, string triggerName)
        {
            UIBehaviourExtensions.InvokeMethodAndRethrow<Selectable>(selectable, "OnSelect", triggerName);
        }
    }

    public static class GraphicExtension
    {
        public static void InvokeOnPopulateMesh(this Graphic graphic, VertexHelper vh)
        {
            UIBehaviourExtensions.InvokeMethodAndRethrow(graphic, "OnPopulateMesh", vh);
        }
    }

    public static class GraphicRaycasterExtension
    {
        public static void InvokeRaycast(Canvas canvas, Camera eventCamera, Vector2 pointerPosition, List<Graphic> results)
        {
            UIBehaviourExtensions.InvokeMethodAndRethrow<GraphicRaycaster>(null, "Raycast", canvas, eventCamera, pointerPosition, results);
        }
    }

    public static class ToggleGroupExtension
    {
        public static void InvokeValidateToggleIsInGroup(this ToggleGroup tgroup, Toggle toggle)
        {
            UIBehaviourExtensions.InvokeMethodAndRethrow<ToggleGroup>(tgroup, "ValidateToggleIsInGroup", toggle);
        }
    }
}
