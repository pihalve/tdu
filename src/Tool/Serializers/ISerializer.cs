using Umbraco.Core.Models.EntityBase;

namespace Pihalve.Tdu.Tool.Serializers
{
    public interface ISerializer
    {
        string Serialize(IEntity entity);
        bool CanSerialize(IEntity entity);
    }
}
