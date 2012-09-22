using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK;
namespace Sensor_Aware_PT.Forms
{
    public partial class AntiAliasedGLControl : GLControl
    {
        public AntiAliasedGLControl() : base(new GraphicsMode(32, 24, 8, 4), 2, 0, GraphicsContextFlags.ForwardCompatible)
        {
            
        }
    }
}
