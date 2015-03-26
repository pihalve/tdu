using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Services;

namespace Pihalve.Tdu.Tool.Providers
{
    public class TemplateProvider : IUmbracoItemProvider
    {
        private readonly IFileService _fileService;
        private IDictionary<int, Guid> _identifierMappings;

        public TemplateProvider(IFileService fileService)
        {
            _fileService = fileService;
        }

        public IDictionary<int, Guid> IdentifierMappings
        {
            get
            {
                if (_identifierMappings == null)
                {
                    _identifierMappings = new Dictionary<int, Guid>();
                    var templates = _fileService.GetTemplates();
                    _identifierMappings = templates.ToDictionary(x => x.Id, x => x.Key);
                }

                return _identifierMappings;
            }
        }

        public IEntity GetEntity(int id)
        {
            return _fileService.GetTemplate(id);
        }
    }
}
