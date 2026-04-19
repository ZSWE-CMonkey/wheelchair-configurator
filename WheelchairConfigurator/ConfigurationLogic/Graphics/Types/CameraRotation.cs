using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationLogic.Graphics.Types
{
    /// <summary>
    /// Rotation in Degrees !!!!!!!!!!!!!!!!!!!!!!!! :3
    /// </summary>
    public struct CameraRotation
    {
        public float X;
        public float Y;
        public float Z;

        public CameraRotation(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public CameraRotation()
        {
            X = 0;
            Y = 0;
            Z = 0;
        }
    }
}
