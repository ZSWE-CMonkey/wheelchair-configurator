using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationLogic.Graphics.Types
{
    public struct CameraPosition
    {
        public float X;
        public float Y;
        public float Z;

        public CameraPosition(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public CameraPosition()
        {
            X = 0;
            Y = 0;
            Z = 0;
        }
    }
}
