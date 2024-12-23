using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MythosOfMoonlight.Common.Datastructures
{
    public struct VertexInfo : IVertexType
    {
        private static VertexDeclaration _vertexDeclaration = new VertexDeclaration(
        [
            new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
                new VertexElement(8, VertexElementFormat.Color, VertexElementUsage.Color, 0),
                new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0)
        ]);

        public Vector2 Position;

        public Color Color;

        public Vector3 TexCoord;

        public VertexDeclaration VertexDeclaration => _vertexDeclaration;

        public VertexInfo(Vector2 position, Vector3 texCoord, Color color)
        {
            Position = position;
            TexCoord = texCoord;
            Color = color;
        }
    }
}
