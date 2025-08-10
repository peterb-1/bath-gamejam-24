using UnityEngine;
using UnityEngine.UI;

namespace Utils
{
    public static class GridLayoutGroupExtensions
    {
        public static int GetColumnCount(this GridLayoutGroup grid, RectTransform rectTransform) {

            if (grid.transform.childCount <= 1)
            {
                return grid.transform.childCount;
            }

            var maxWidth = rectTransform.rect.width;
            var cellWidth = grid.cellSize.x;
            var cellSpacing = grid.spacing.x;
            
            for (var i = 2; i < grid.transform.childCount; ++i) {
                if (i * cellWidth + (i - 1) * cellSpacing > maxWidth) {
                    return i - 1;
                }
            }

            return grid.transform.childCount;
        }
    }
}