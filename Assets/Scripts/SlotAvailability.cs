using UnityEngine;

public class SlotAvailability : MonoBehaviour
{
    [SerializeField] bool isAvailable = true;
    public bool IsAvailable {  get { return isAvailable; } set { isAvailable = value; } }
}
