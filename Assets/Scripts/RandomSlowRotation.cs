using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSlowRotation : MonoBehaviour
{
    [SerializeField]
    List<Transform> m_Transforms;

    float m_Counter = 0f;
    
    List<Quaternion> m_Rotations;
    
    // Start is called before the first frame update
    IEnumerator Start()
    {
        m_Rotations = new List<Quaternion>(m_Transforms.Count);
        
        for (int i = 0; i < m_Transforms.Count; i++)
        {
            m_Rotations.Add(Random.rotation);
        }

        while (true)
        {
            m_Counter = (Mathf.Sin(Time.time) + 1) / 2;
            
            for (int i = 0; i < m_Transforms.Count; i++)
            {
                m_Transforms[i].rotation =  Quaternion.Lerp(Quaternion.identity, m_Rotations[i], m_Counter);
            }

            yield return null;
        }
    }

}
