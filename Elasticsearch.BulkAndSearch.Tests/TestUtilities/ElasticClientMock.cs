﻿using Elasticsearch.BulkAndSearch.Models;
using Moq;
using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Elasticsearch.BulkAndSearch.Tests.TestUtilities
{
    public static class ElasticClientMock
    {
        public static ElasticsearchOptions GetOptions()
        {
            return new ElasticsearchOptions
            {
                DefaultIndexName = "my-index",
                DefaultTypeName = "my-type",
                Environment = "my-env",
                MaximumRetries = 4,
                ReadUrl = "http://esread:9200",
                WriteUrl = "http://eswrite:9200",
                TimeoutInSeconds = 30
            };
        }

        public static string LastElasticClientAction = null;

        public static Person LastProcessedPerson = null;

        public static string LastProcessedIndex = null;

        public static List<Person> LastProcessedPersons = null;

        public static List<string> LastProcessedIndexes = null;

        public static string LastQueryBody = null;

        public static IElasticClient GetElasticClientMock(
            ElasticsearchOptions options, 
            Func<string, Person, string> generateIndexName, 
            Person returnedPerson = null, 
            string scrollId = null)
        {
            var searchMock = new Mock<ISearchResponse<Person>>();

            var persons = new List<Person>();
            if (returnedPerson != null)
            {
                persons.Add(returnedPerson);
            }

            searchMock.SetupGet(m => m.IsValid).Returns(true);
            searchMock.SetupGet(m => m.Total).Returns((returnedPerson == null) ? 0 : 1);
            searchMock.SetupGet(m => m.Documents).Returns(persons);
            searchMock.SetupGet(m => m.ScrollId).Returns(scrollId);

            var responseMock = new Mock<IIndexResponse>();

            responseMock.SetupGet(m => m.IsValid).Returns(true);

            var bulkMock = new Mock<IBulkResponse>();

            bulkMock.SetupGet(m => m.IsValid).Returns(true);

            var clientMock = new Mock<IElasticClient>();

            clientMock
                .Setup(m => m.Index<Person>(
                    It.IsAny<Person>(),
                    It.IsAny<Func<IndexDescriptor<Person>, IIndexRequest<Person>>>()))
                .Returns((Person document, Func<IndexDescriptor<Person>, IIndexRequest<Person>> selector) =>
                {
                    LastElasticClientAction = "Index";
                    LastProcessedPerson = document;
                    LastProcessedIndex =
                        generateIndexName?.Invoke(options.DefaultIndexName, document)
                        ?? options.DefaultIndexName;
                    return responseMock.Object;
                });

            clientMock
               .Setup(m => m.Bulk(
                   It.IsAny<BulkDescriptor>()))
               .Returns((BulkDescriptor descriptor) =>
               {
                   LastElasticClientAction = "Bulk";
                   LastProcessedPersons = new List<Person>();
                   LastProcessedIndexes = new List<string>();
                   var descriptorAsJson = SerializeUtil.Serialize(descriptor);
                   var lines = descriptorAsJson.Split('\n');

                   bool isOperation = true;
                   for (int i = 0; i < lines.Length; i++)
                   {
                       if (string.IsNullOrWhiteSpace(lines[i]))
                       {
                           continue;
                       }

                       if (isOperation)
                       {
                           var operation = JsonConvert.DeserializeObject<dynamic>(lines[i]);
                           LastProcessedIndexes.Add(operation.index._index.ToString());
                           isOperation = false;
                       }
                       else
                       {
                           var document = JsonConvert.DeserializeObject<Person>(lines[i]);
                           LastProcessedPersons.Add(document);
                           isOperation = true;
                       }
                   }

                   return bulkMock.Object;
               });

            clientMock
              .Setup(m => m.Search<Person>(It.IsAny<ISearchRequest>()))
              .Returns((ISearchRequest request) =>
              {
                  LastElasticClientAction = "Search";
                  LastQueryBody = SerializeUtil.Serialize(request);
                  return searchMock.Object;
              });


            clientMock
              .Setup(m => m.Scroll<Person>(It.IsAny<IScrollRequest>()))
              .Returns((IScrollRequest request) =>
              {
                  LastElasticClientAction = "Scroll";
                  LastQueryBody = SerializeUtil.Serialize(request);
                  return searchMock.Object;
              });

            return clientMock.Object;
        }
    }
}
