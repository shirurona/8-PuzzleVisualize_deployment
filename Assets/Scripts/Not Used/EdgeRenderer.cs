using UnityEngine;

public class EdgeRenderer : MonoBehaviour
{
    [SerializeField] [Range(0.01f, 1f)] private float thickness = 0.1f;

    private void Start(){
        transform.localScale = new Vector3(thickness, transform.localScale.y, thickness);
    }

    public void Put(Vector3 from, Vector3 to)
    {
        transform.position = (from + to) / 2;
        // to - from の方向を向く回転を計算
        Quaternion lookRotation = Quaternion.LookRotation(to - from);
        // 二つの回転を合成
        transform.rotation = lookRotation * Quaternion.Euler(90, 0, 0);
        transform.localScale = new Vector3(transform.localScale.x, Vector3.Distance(from, to) / 2f, transform.localScale.z);
    }
}
