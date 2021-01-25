using UnityEngine;
using System;
using UnityEngine.Events;

namespace SWAC
{
    public class SpeedHacks : MonoBehaviour
    {
        //Speed hack protection
        public ProtInt timeDiff = 0;
        ProtInt previousTime = 0;
        ProtInt realTime = 0;
        ProtFloat gameTime = 0;
        ProtBool detected = false;
        public class SpeedHackDetection : UnityEvent {}
        private SpeedHackDetection m_onSpeedHack = new SpeedHackDetection();
        
        public SpeedHackDetection onSpeedHackDetected
        {
            get { return m_onSpeedHack; }
            set { m_onSpeedHack = value; }
        }
        // Use this for initialization
        void Start()
        {
            previousTime = DateTime.Now.Second;
            gameTime = 1;
        }

        // Update is called once per frame 
        void FixedUpdate()
        {
            if (previousTime != DateTime.Now.Second)
            {
                realTime++;
                previousTime = DateTime.Now.Second;

                timeDiff = (int) gameTime - realTime;
                if (timeDiff > 7)
                {
                    if (!detected)
                    {
                        detected = true;
                        m_onSpeedHack.Invoke();
                    }
                }
                else
                {
                    detected = false;
                }
            }

            gameTime += Time.deltaTime;
        }
    }
}