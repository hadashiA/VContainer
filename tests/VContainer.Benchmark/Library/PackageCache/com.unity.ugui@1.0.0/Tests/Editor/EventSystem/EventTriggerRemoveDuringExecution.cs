using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;

public class EventTriggerRemoveDuringExecution
{
    [Test]
    [Description("ArgumentOutOfRange Exception is thrown when removing handler in callback in EventTrigger (case 1401557)")]
    public void EventTrigger_DoesNotThrowExceptionWhenRemovingEventDuringExecution()
    {
        var go = new GameObject();
        var eventTrigger = go.AddComponent<EventTrigger>();
        var eventSystem = go.AddComponent<EventSystem>();

        var entry1 = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        var entry2 = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };

        bool executed1 = false;
        bool executed2 = false;
        entry1.callback.AddListener(e =>
        {
            executed1 = true;
            eventTrigger.triggers.Remove(entry2);
        });
        entry2.callback.AddListener(e => executed2 = true);

        eventTrigger.triggers.Add(entry1);
        eventTrigger.triggers.Add(entry2);

        Assert.DoesNotThrow(() => eventTrigger.OnPointerDown(new PointerEventData(eventSystem)));
        Assert.True(executed1, "Expected Event 1 to be called but it was not.");
        Assert.False(executed2, "Expected Event 2 to not be called as it was removed by event 1.");
        Assert.That(eventTrigger.triggers, Does.Not.Contains(entry2));
        Assert.That(eventTrigger.triggers, Does.Contain(entry1));

        Object.DestroyImmediate(go);
    }
}
