using System.IO;
using Framework;
using Simulation.Data;

namespace Simulation
{
    public struct TechnologyStatus : ISerializable
    {
        public TechnologyDefinition Definition;
        public ResearchStatus Status;

        public void Serialize(BinaryWriter stream)
        {
            stream.WriteReference(this.Definition);
            stream.Write((byte)this.Status);
        }

        public void Deserialize(BinaryReader stream)
        {
            this.Definition = stream.ReadReference<TechnologyDefinition>();
            this.Status = (ResearchStatus)stream.ReadByte();
        }
    }
}
