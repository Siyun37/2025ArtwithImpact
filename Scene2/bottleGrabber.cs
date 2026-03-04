using UnityEngine;

public class bottleGrabber : MonoBehaviour
{
    public FloatingObject floatingBottle;
    public Transform leftHand;
    public Vector3 positionOffset = Vector3.zero; // position offset
    public Vector3 rotationOffset = Vector3.zero; // rotation offset


    // // 애니메이션 이벤트: 0=Grab, 1=Drop
    // public void BottleEvent(int state)
    // {
    //     if (state == 0) GrabBottle();
    //     else if (state == 1) DropBottle();
    // }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void GrabBottle()
    {
        if (!floatingBottle || !leftHand) return;

        floatingBottle.AttachToHand(leftHand, positionOffset, rotationOffset);

    }

    void DropBottle()
    {
        if (floatingBottle == null) return;

        floatingBottle.Detach(false); // floating 비활성화, bottle detached from hand
    }

}
