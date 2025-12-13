using UnityEngine;

public class TutoTrigger : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private int tutoID;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag != "Hero") return;

        TutoManager.Instance.DisplayTutorial(tutoID);
    }
}
