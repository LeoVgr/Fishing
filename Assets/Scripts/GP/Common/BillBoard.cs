using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.GP.Common
{
    public class BillBoard : MonoBehaviour
    {
        public void Update()
        {
            this.transform.forward = Camera.main.transform.forward;
        }
    }
}
