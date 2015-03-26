using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Pihalve.Tdu.Tool.Providers;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Pihalve.Tdu.Tool
{
    class Program
    {
        //private class Dyr
        //{
        //}

        //private class Abe : Dyr
        //{
        //}

        //private class Ged : Dyr
        //{
        //}

        static void Main(string[] args)
        {
            //var abe = new Abe();
            //var ged = new Ged();
            //var start = DateTime.Now;
            //for (int i = 0; i < 10000000; i++)
            //{
            //    Dyr dyr;
            //    if (i % 2 == 0)
            //    {
            //        dyr = abe;
            //    }
            //    else
            //    {
            //        dyr = ged;
            //    }
            //    bool res = dyr is Abe;
            //    //bool res = dyr.GetType() == typeof(Abe);
            //}
            //Console.WriteLine("Time: {0}", (DateTime.Now - start).TotalMilliseconds);
            //Console.ReadKey();
            //return;

            Console.Title = "Umbraco Console";

            log4net.Config.XmlConfigurator.Configure();

            //Initialize the application
            var application = new ConsoleApplicationBase();
            application.Start(application, new EventArgs());
            Console.WriteLine("Application Started");

            Console.WriteLine("--------------------");
            //Write status for ApplicationContext
            var context = ApplicationContext.Current;
            Console.WriteLine("ApplicationContext is available: " + (context != null).ToString());
            //Write status for DatabaseContext
            var databaseContext = context.DatabaseContext;
            Console.WriteLine("DatabaseContext is available: " + (databaseContext != null).ToString());
            //Write status for Database object
            var database = databaseContext.Database;
            Console.WriteLine("Database is available: " + (database != null).ToString());
            Console.WriteLine("--------------------");

            //Get the ServiceContext and the two services we are going to use
            var serviceContext = context.Services;
            var contentService = serviceContext.ContentService;
            var entityService = serviceContext.EntityService;

            ListContentTypes(serviceContext, application.DataDirectory);
            Console.ReadKey();

            //var waitOrBreak = true;
            //while (waitOrBreak)
            //{
            //    //List options
            //    Console.WriteLine("-- Options --");
            //    Console.WriteLine("List content nodes: l");
            //    Console.WriteLine("Quit application: q");

            //    var input = Console.ReadLine();
            //    if (string.IsNullOrEmpty(input) == false && input.ToLowerInvariant().Equals("q"))
            //        waitOrBreak = false;//Quit the application
            //    else if (string.IsNullOrEmpty(input) == false && input.ToLowerInvariant().Equals("l"))
            //        //ListContentNodes(contentService);//Call the method that lists all the content nodes
            //        ListContentTypes(serviceContext, application.DataDirectory);
            //}
        }

        private static void ListContentTypes(ServiceContext serviceContext, string dataDirectory)
        {
            var fileService = serviceContext.FileService;
            var contentTypeService = serviceContext.ContentTypeService;

            var outputFolder = Directory.CreateDirectory(Path.Combine(dataDirectory, "SerializedItems"));

            // Setup umbraco item providers
            var templateProvider = new TemplateProvider(fileService);
            var contentTypeProvider = new ContentTypeProvider(contentTypeService);

            var providers = new Dictionary<Type, IUmbracoItemProvider>();
            providers.Add(typeof(Template), templateProvider);
            providers.Add(typeof(ContentType), contentTypeProvider);
            providers.Add(typeof(ContentTypeSort), contentTypeProvider);

            //// Populate identifier mappings
            //var identifierMappingsList = new Dictionary<Type, IDictionary<int, Guid>>();
            //identifierMappingsList.Add(typeof(ContentType), contentTypeProvider.IdentifierMappings);
            //identifierMappingsList.Add(typeof(Template), templateProvider.IdentifierMappings);

            var jsonSerializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            jsonSerializerSettings.Converters.Add(new UmbracoItemConverter(providers/*identifierMappingsList*/));
            //var serializer = new ContentTypeSerializer();

            var contentTypes = contentTypeService.GetAllContentTypes();
            foreach (IContentType contentType in contentTypes)
            {
                var serializedContentType = JsonConvert.SerializeObject(contentType, jsonSerializerSettings);
                //var serializedContentType = serializer.Serialize(contentType);
                var file = System.IO.File.CreateText(Path.Combine(outputFolder.FullName, contentType.Name));
                file.Write(serializedContentType);
                file.Flush();

                Console.WriteLine("Name: {0}, Id: {1}, DefTmp: {2}", contentType.Name, contentType.Id, contentType.DefaultTemplate != null ? contentType.DefaultTemplate.Name : "[N/A]");
                foreach (PropertyGroup propertyGroup in contentType.PropertyGroups)
                {
                    Console.WriteLine("  Name: {0}, Id: {1}", propertyGroup.Name, propertyGroup.Id);
                    foreach (PropertyType propertyType in propertyGroup.PropertyTypes)
                    {
                        Console.WriteLine("    Name: {0}, Id: {1}", propertyType.Name, propertyType.Id);
                    }
                }
            }
        }

        /// <summary>
        /// Private method to list all content nodes
        /// </summary>
        /// <param name="contentService"></param>
        private static void ListContentNodes(IContentService contentService)
        {
            //Get the Root Content
            var rootContent = contentService.GetRootContent();
            foreach (var content in rootContent)
            {
                Console.WriteLine("Root Content: " + content.Name + ", Id: " + content.Id);
                //Get Descendants of the current content and write it to the console ordered by level
                var descendants = contentService.GetDescendants(content);
                foreach (var descendant in descendants.OrderBy(x => x.Level))
                {
                    Console.WriteLine("Name: " + descendant.Name + ", Id: " + descendant.Id + " - Parent Id: " + descendant.ParentId);
                }
            }
        }
    }
}
