using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
namespace UI.CardCarousel
{
    public class Carousel :
        MonoBehaviour,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
    {
        [SerializeField]
        private List<CarouselCard> cards = new();
        public CarouselLayout layout;

        public CarouselCard CurrentSelectedCard { private set; get; }

        [Header("Scroll Physics")]
        public float swipeSensitivity = 0.005f;
        public float snapSpeed = 8f;
        public float velocityDamping = 6f;
        public float snapVelocityThreshold = 0.05f;

        float scrollPosition;
        int previousScrollPos;

        float velocity;
        bool dragging;

        Vector2 lastDragPos;

        private int? overrideTarget = null;

        void Update()
        {
            HandleRemoveNull();

            // Apply velocity only when not dragging
            if (!dragging && overrideTarget == null)
            {
                scrollPosition += velocity * Time.unscaledDeltaTime;
                velocity = Mathf.Lerp(velocity, 0, Time.unscaledDeltaTime * velocityDamping);
            }

            ClampScroll();

            if(overrideTarget.HasValue)
            {
                float target = overrideTarget.Value;
                if (!float.IsNaN(target))
                {
                    scrollPosition = Mathf.Lerp(scrollPosition, target, Time.unscaledDeltaTime * snapSpeed);

                    if (Mathf.Abs(scrollPosition - target) < 0.001f)
                    {
                        scrollPosition = target;
                        overrideTarget = null;
                    }

                    SetSelected((int)target);
                }
            }
            // Snap ONLY when not dragging AND velocity nearly zero
            else if (!dragging && Mathf.Abs(velocity) < snapVelocityThreshold)
            {
                float target = Mathf.Round(scrollPosition);
                scrollPosition = Mathf.Lerp(scrollPosition, target, Time.unscaledDeltaTime * snapSpeed);

                SetSelected((int)target);
            }

            UpdateLayout();
        }

        void SetSelected(int index)
        {
            index = Mathf.Clamp(index, 0, cards.Count - 1);

            if (CurrentSelectedCard != null && previousScrollPos == index)
                return;

            if (cards.Count > 0)
                CurrentSelectedCard = cards[index];
            previousScrollPos = index;
        }

        private void HandleRemoveNull()
        {
            cards.RemoveAll(c => c == null);
        }

        private void OnCardClick(CarouselCard card)
        {
            overrideTarget = cards.IndexOf(card);
        }

        public void AddCards(List<CarouselCard> cardsToAdd)
        {
            foreach(CarouselCard card in cardsToAdd)
            {
                card.onButtonClick += OnCardClick;
            }

            cards.AddRange(cardsToAdd);
            cards.RemoveDuplicates();
        }

        void ClampScroll()
        {
            if (cards.Count == 0)
                return;

            scrollPosition = Mathf.Clamp(scrollPosition, 0, cards.Count - 1);

            if (overrideTarget.HasValue)
            {
                overrideTarget = Mathf.Clamp(overrideTarget.Value, 0, cards.Count - 1);
            }
        }

        void UpdateLayout()
        {
            var depthList = new List<(CarouselCard card, int depth)>();

            for (int i = 0; i < cards.Count; i++)
            {
                float rel = i - scrollPosition;

                if (!float.IsNaN(rel))
                {
                    layout.Evaluate(rel, out var pos, out var rot, out var scale, out var alpha, out var depth);

                    cards[i].Apply(pos, rot, scale, alpha);

                    depthList.Add((cards[i], depth));
                }
            }

            // Sort by depth ascending
            depthList.Sort((a, b) => a.depth.CompareTo(b.depth));

            // Assign sibling order
            for (int i = 0; i < depthList.Count; i++)
            {
                depthList[i].card.transform.SetSiblingIndex(i);
            }
        }

        public void OnBeginDrag(PointerEventData e)
        {
            dragging = true;
            velocity = 0;
            lastDragPos = e.position;
            CurrentSelectedCard = null;
            overrideTarget = null;
        }

        public void OnDrag(PointerEventData e)
        {
            Vector2 delta = e.position - lastDragPos;
            lastDragPos = e.position;

            float move = -delta.x * swipeSensitivity;

            scrollPosition += move;

            ClampScroll();

            velocity = move / Time.unscaledDeltaTime;
        }

        public void OnEndDrag(PointerEventData e)
        {
            dragging = false;
        }
    }
}

