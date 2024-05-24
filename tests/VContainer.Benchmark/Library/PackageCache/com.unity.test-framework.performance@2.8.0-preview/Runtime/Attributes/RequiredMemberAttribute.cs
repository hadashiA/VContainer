namespace Unity.PerformanceTesting
{
    public class RequiredMemberAttribute
#if UNITY_2021_1_OR_NEWER
        : UnityEngine.Scripting.RequiredMemberAttribute
#else
        : System.Attribute
#endif
    {
    }
}