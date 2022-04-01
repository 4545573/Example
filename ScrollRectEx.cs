using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ScrollTools
{
    [RequireComponent(typeof(ScrollRect))]
    public class ScrollRectEx : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        // 扩展功能开关

        #region Switch

        // 滑动冲突是否开启
        public bool enableSlidingConflict = true;
        public bool enableLoadMoreFunc;

        #endregion

        // 通用属性

        #region CommondAttribute

        private ScrollRect _thisScrollRect;

        #endregion

        // 滑动冲突解决需要的属性
        #region SlidingConflictAttribute

        private ScrollRect parentScrollRect;

        private bool enableVertical;
        private bool enableHorizontal;

        // 此次拖动方向，0为尚未确定方向，1水平，2竖直
        private int direction;
        private Vector2 m_PointerStartLocalCursor;

        private const int None = 0;
        private const int HorizontalDirection = 1;
        private const int VerticalDirection = 2;

        #endregion

        // loadMore功能实现需要的属性
        #region LoadMoreAttribute

        public Func<bool> HasMoreItemFunc;
        public Action<Action<LoadStatus>> LoadMoreFunc;
        
        private float time;
        private LoadStatus _status = LoadStatus.None;

        public enum LoadStatus
        {
            None = 0,
            Loading = 1
        }

        #endregion


        void Awake()
        {
            _thisScrollRect = GetComponent<ScrollRect>();
            parentScrollRect = transform.parent.GetComponentInParent<ScrollRect>();
            if (_thisScrollRect != null)
            {
                enableVertical = _thisScrollRect.vertical;
                enableHorizontal = _thisScrollRect.horizontal;
            }
        }


        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!enableSlidingConflict) return;
            
            direction = 0;
            m_PointerStartLocalCursor = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_thisScrollRect.viewport, eventData.position,
                eventData.pressEventCamera, out m_PointerStartLocalCursor);
            if (parentScrollRect != null)
            {
                parentScrollRect.OnBeginDrag(eventData);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!enableSlidingConflict) return;

            if (parentScrollRect != null && _thisScrollRect != null)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(_thisScrollRect.viewport, eventData.position,
                    eventData.pressEventCamera, out var localCursor);

                var pointerDelta = localCursor - m_PointerStartLocalCursor;
                float angle = Vector2.Angle(pointerDelta, Vector2.up);

                if (IsHorizontalScroll(angle) && (direction == None || direction == HorizontalDirection))
                {
                    direction = HorizontalDirection;
                    parentScrollRect.enabled = !enableHorizontal;
                    _thisScrollRect.enabled = enableHorizontal;
                    parentScrollRect.OnDrag(eventData);
                }
                else if (IsVerticalScroll(angle) && (direction == None || direction == VerticalDirection))
                {
                    direction = VerticalDirection;
                    parentScrollRect.enabled = !enableVertical;
                    _thisScrollRect.enabled = enableVertical;
                    parentScrollRect.OnDrag(eventData);
                }
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!enableSlidingConflict) return;

            if (parentScrollRect != null)
            {
                parentScrollRect.OnEndDrag(eventData);
                parentScrollRect.enabled = true;
                _thisScrollRect.enabled = true;
            }
        }

        private void LateUpdate()
        {
            if (enableLoadMoreFunc)
            {
                time += Time.deltaTime;
                if (time > 0.2f)
                {
                    time = 0f;
                    LoadMoreItems();
                }
            }
        }

        private bool IsHorizontalScroll(float angle)
        {
            return angle > 45f && angle < 135f;
        }

        private bool IsVerticalScroll(float angle)
        {
            return (angle > 0 && angle < 45f) || angle > 135f;
        }
        
        
        private void LoadMoreItems()
        {
            if (LoadMoreFunc == null || HasMoreItemFunc == null ||
                !HasMoreItemFunc() || _status != LoadStatus.None) return;

            int dir = -1;
            if (_thisScrollRect.horizontal)
            {
                dir = 0;
            }
            else if (_thisScrollRect.vertical)
            {
                dir = 1;
            }

            if (dir == -1) return;

            var threshold = dir == 0 ? 
                _thisScrollRect.normalizedPosition.x :
                _thisScrollRect.normalizedPosition.y;

            if (threshold > 0.9f)
            {
                LoadMoreFunc(UpdateLoadStatus);
            }
        }

        /// <summary>
        /// 如果需要加loading提示的话，可以在这里加
        /// </summary>
        /// <param name="loadStatus"></param>
        private void UpdateLoadStatus(LoadStatus loadStatus)
        {
            _status = loadStatus;
            if (_status == LoadStatus.None)
            {
                
            }
            else
            {
                
            }
        }
    }
}