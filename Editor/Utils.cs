using UnityEngine;

namespace SnowyWalk.BlendShapeSnapshot.Editor
{
    public static class Utils
    {
        public static Rect Inset(this Rect rect, float inset)
        {
            return new Rect(
                rect.x + inset,
                rect.y + inset,
                rect.width - inset * 2f,
                rect.height - inset * 2f
                );
        }
    }
}