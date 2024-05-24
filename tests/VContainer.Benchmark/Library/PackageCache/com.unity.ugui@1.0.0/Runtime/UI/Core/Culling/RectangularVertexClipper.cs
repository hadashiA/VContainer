namespace UnityEngine.UI
{
    internal class RectangularVertexClipper
    {
        readonly Vector3[] m_WorldCorners = new Vector3[4];
        readonly Vector3[] m_CanvasCorners = new Vector3[4];

        public Rect GetCanvasRect(RectTransform t, Canvas c)
        {
            if (c == null)
                return new Rect();

            t.GetWorldCorners(m_WorldCorners);
            var canvasTransform = c.GetComponent<Transform>();
            for (int i = 0; i < 4; ++i)
                m_CanvasCorners[i] = canvasTransform.InverseTransformPoint(m_WorldCorners[i]);

            return new Rect(m_CanvasCorners[0].x, m_CanvasCorners[0].y, m_CanvasCorners[2].x - m_CanvasCorners[0].x, m_CanvasCorners[2].y - m_CanvasCorners[0].y);
        }
    }
}
