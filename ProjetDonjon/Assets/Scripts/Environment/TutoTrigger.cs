using UnityEngine;

public class TutoTrigger : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private int tutoID;
    [SerializeField] private int neededTutoIDToContinue;
    [SerializeField] private bool isAdditional;

    [Header("References")]
    [SerializeField] private BoxCollider2D blockCollider;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag != "Hero") return;

        TutoManager.Instance.DisplayTutorial(tutoID, isAdditional);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.tag != "Hero") return;
        if (!isAdditional && (!TutoManager.Instance.DidTutorialStep[neededTutoIDToContinue] || TutoManager.Instance.IsDisplayingTuto)) return;
        if (isAdditional && (!TutoManager.Instance.DidAdditionalTutorialStep[neededTutoIDToContinue] || TutoManager.Instance.IsDisplayingTuto)) return;

        Destroy(gameObject);
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag != "Hero") return;
        if (!TutoManager.Instance.DidTutorialStep[neededTutoIDToContinue] || TutoManager.Instance.IsDisplayingTuto) return;

        Destroy(gameObject);
    }
}
