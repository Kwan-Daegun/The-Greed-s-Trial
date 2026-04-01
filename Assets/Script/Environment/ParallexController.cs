using UnityEngine;

public class ParallexController : MonoBehaviour
{
   Transform cam;
   Vector3 MainCam;
   float distance;

   GameObject[] Background;
   Material[] mat;
   float[] Backspeed;
   float farthestBack;
   [Range(0f, 0.5f)]
   public float ParallexSpeed;
   float z;

   void Start()
    {
        cam = Camera.main.transform;
        MainCam = cam.position;
        z = transform.position.z;
        int backCount = transform.childCount;
        mat = new Material[backCount];
        Backspeed = new float [backCount];
        Background = new GameObject[backCount];

        for(int i = 0; i<backCount; i++)
        {
            Background[i] = transform.GetChild(i).gameObject;
            mat[i] = Background[i].GetComponent<Renderer>().material;
        }
        BackSPeedCalculator(backCount);
    }
    void BackSPeedCalculator(int backCount)
    {
        for(int i = 0; i<backCount; i++)
        {
            if((Background[i].transform.position.z - cam.position.z) > farthestBack)
            {
                farthestBack = Background[i].transform.position.z - cam.position.z;
            }
        }
        for(int i = 0; i<backCount; i++)
        {
            Backspeed[i] = 1 - (Background[i].transform.position.z - cam.position.z)/farthestBack;
        }
    }
    private void LateUpdate()
    {

        distance = cam.position.x - MainCam.x;
        transform.position = new Vector3(cam.position.x, transform.position.y,z);
        for(int i = 0; i<Backspeed.Length; i++)
        {
            float speed = Backspeed[i]*ParallexSpeed;
            mat[i].SetTextureOffset("_MainTex", new Vector2(distance, 0)*speed);
        } 
    }
}
