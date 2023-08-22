using UnityEngine;
using UnityEngine.UI;

namespace com.spacepuppy.UI
{

    /// <remarks>
    /// This code is based on code generated using ChatGPT.
    /// </remarks>
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class FlexibleGridLayoutGroup : GridLayoutGroup, IUIComponent
    {

        public enum Modes
        {
            AnyColumnCount = 0,
            EvenColumnCount = 1,
            OddColumnCount = 2,
        }

        #region Fields

        [SerializeField]
        private Modes _mode = Modes.EvenColumnCount;

        #endregion

        #region Properties

        public Modes Mode
        {
            get => _mode;
            set => _mode = value;
        }

        #endregion

        #region Methods

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
            FitToContainer();
        }

        public override void CalculateLayoutInputVertical()
        {
            base.CalculateLayoutInputVertical();
            FitToContainer();
        }

        private void FitToContainer()
        {
            float containerWidth = rectTransform.rect.width;
            float containerHeight = rectTransform.rect.height;
            int itemCount = rectChildren.Count;

            // Get the aspect ratio of the container from its AspectRatioFitter component
            float aspectRatio = 1f;
            AspectRatioFitter arf = GetComponentInParent<AspectRatioFitter>();
            if (arf != null && arf.aspectRatio != 0f)
            {
                aspectRatio = arf.aspectRatio;
            }

            // Calculate the optimal number of columns and rows for the given aspect ratio
            int rowCount, columnCount;
            if (aspectRatio > 1f)
            {
                columnCount = Mathf.CeilToInt(Mathf.Sqrt(itemCount * aspectRatio));
                rowCount = Mathf.CeilToInt(itemCount / (float)columnCount);
                columnCount = Mathf.CeilToInt(itemCount / (float)rowCount);
            }
            else
            {
                rowCount = Mathf.CeilToInt(Mathf.Sqrt(itemCount / aspectRatio));
                columnCount = Mathf.CeilToInt(itemCount / (float)rowCount);
                rowCount = Mathf.CeilToInt(itemCount / (float)columnCount);
            }

            switch (_mode)
            {
                case Modes.AnyColumnCount:
                    //all good
                    break;
                case Modes.EvenColumnCount:
                    if (columnCount > 1 && columnCount % 2 != 0)
                    {
                        columnCount++;
                        rowCount = Mathf.CeilToInt(itemCount / (float)columnCount);
                    }
                    break;
                case Modes.OddColumnCount:
                    if (columnCount > 1 && columnCount % 2 == 0)
                    {
                        columnCount++;
                        rowCount = Mathf.CeilToInt(itemCount / (float)columnCount);
                    }
                    break;
            }

            // Calculate the item size based on the optimal number of columns and rows
            float itemWidth = (containerWidth - padding.left - padding.right - spacing.x * (columnCount - 1)) / columnCount;
            float itemHeight = (containerHeight - padding.top - padding.bottom - spacing.y * (rowCount - 1)) / rowCount;
            float itemSize = Mathf.Min(itemWidth, itemHeight);

            // Calculate the total size of the grid based on the item size, number of rows and columns, and spacing
            float totalWidth = itemSize * columnCount + spacing.x * (columnCount - 1) + padding.left + padding.right;
            float totalHeight = itemSize * rowCount + spacing.y * (rowCount - 1) + padding.top + padding.bottom;

            // If the total size of the grid is smaller than the container size, increase the item size
            if (totalWidth < containerWidth || totalHeight < containerHeight)
            {
                float maxItemWidth = (containerWidth - padding.left - padding.right - spacing.x * (columnCount - 1)) / columnCount;
                float maxItemHeight = (containerHeight - padding.top - padding.bottom - spacing.y * (rowCount - 1)) / rowCount;
                float maxItemSize = Mathf.Min(maxItemWidth, maxItemHeight);
                float sizeIncrement = Mathf.Max((maxItemSize - itemSize) / 2f, 0f);
                itemSize += sizeIncrement;
            }

            // Set the cell size of the GridLayoutGroup to the calculated item size
            cellSize = new Vector2(itemSize, itemSize);

            // Set the constraint count to the optimal number of columns
            constraintCount = columnCount;
        }

        #endregion

        #region IUIComponent Interface

        public new RectTransform transform => base.transform as RectTransform;

        RectTransform IUIComponent.transform => base.transform as RectTransform;

        Component IComponent.component => this;

        #endregion

    }

}
