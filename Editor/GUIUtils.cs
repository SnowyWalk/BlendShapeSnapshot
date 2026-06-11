using UnityEngine;

namespace SnowyWalk.BlendShapeSnapshot.Editor
{
    public static class GUIUtils
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

        public static Rect[,] SplitGrid(Rect rect, float[] rowRatios, float[] columnRatios, float horizontalSpacing, float verticalSpacing)
        {
            int rows = rowRatios.Length;
            int columns = columnRatios.Length;

            Rect[,] result = new Rect[rows, columns];

            float rowRatioSum = 0f;
            float columnRatioSum = 0f;

            for (int i = 0; i < rows; i++)
                rowRatioSum += rowRatios[i];

            for (int i = 0; i < columns; i++)
                columnRatioSum += columnRatios[i];

            float availableWidth = rect.width - horizontalSpacing * (columns - 1);
            float availableHeight = rect.height - verticalSpacing * (rows - 1);

            float y = rect.y;

            for (int row = 0; row < rows; row++)
            {
                float height = Mathf.Floor(
                    availableHeight * (rowRatios[row] / rowRatioSum)
                    );

                if (row == rows - 1)
                    height = rect.yMax - y;

                float x = rect.x;

                for (int column = 0; column < columns; column++)
                {
                    float width = Mathf.Floor(
                        availableWidth * (columnRatios[column] / columnRatioSum)
                        );

                    if (column == columns - 1)
                        width = rect.xMax - x;

                    result[row, column] = new Rect(x, y, width, height);

                    x += width + horizontalSpacing;
                }

                y += height + verticalSpacing;
            }

            return result;
        }
        
        public static Texture2D MakeTex(int w, int h, Color col)
        {
            var pix = new Color[w * h];
            for (int i = 0; i < pix.Length; i++) pix[i] = col;
            var t = new Texture2D(w, h);
            t.SetPixels(pix);
            t.Apply();
            return t;
        }
    }
}