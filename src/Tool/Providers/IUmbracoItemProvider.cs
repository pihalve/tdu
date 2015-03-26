using System;
using System.Collections.Generic;
using Umbraco.Core.Models.EntityBase;

namespace Pihalve.Tdu.Tool.Providers
{
    public interface IUmbracoItemProvider
    {
        IDictionary<int, Guid> IdentifierMappings { get; }
        IEntity GetEntity(int id);
    }
}
