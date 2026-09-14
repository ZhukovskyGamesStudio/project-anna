using UnityEngine;

public class BoardThreadManager : MonoBehaviour
{
    [System.Serializable]
    public class ThreadPair
    {
        public RectTransform noteA;
        public RectTransform noteB;
    }

    [SerializeField] private BoardThread threadPrefab;
    [SerializeField] private RectTransform threadContainer;
    [SerializeField] private ThreadPair[] pairs;

    private void Start()
    {
        if (threadPrefab == null || threadContainer == null || pairs == null)
        {
            return;
        }

        for (int i = 0; i < pairs.Length; i++)
        {
            if (pairs[i] == null || pairs[i].noteA == null || pairs[i].noteB == null)
            {
                continue;
            }

            BoardThread thread = Instantiate(threadPrefab, threadContainer);
            thread.Setup(pairs[i].noteA, pairs[i].noteB);
            thread.gameObject.SetActive(true);
        }
    }
}