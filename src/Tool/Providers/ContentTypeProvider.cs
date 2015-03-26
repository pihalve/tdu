using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Services;

namespace Pihalve.Tdu.Tool.Providers
{
    public class ContentTypeProvider : IUmbracoItemProvider
    {
        private readonly IContentTypeService _contentTypeService;
        private IDictionary<int, Guid> _identifierMappings;

        public ContentTypeProvider(IContentTypeService contentTypeService)
        {
            _contentTypeService = contentTypeService;
        }

        public IDictionary<int, Guid> IdentifierMappings
        {
            get
            {
                if (_identifierMappings == null)
                {
                    _identifierMappings = new Dictionary<int, Guid>();
                    var contentTypes = _contentTypeService.GetAllContentTypes();
                    _identifierMappings = contentTypes.ToDictionary(x => x.Id, x => x.Key);
                }

                return _identifierMappings;
            }
        }

        public IEntity GetEntity(int id)
        {
            return _contentTypeService.GetContentType(id);
        }
    }
}
