﻿using Elasticsearch.BulkAndSearch.Factories;
using Elasticsearch.BulkAndSearch.Models;
using System;
using System.Linq;
using Xunit;

namespace Elasticsearch.BulkAndSearch.Tests.Factories
{
    public static class ElasticClientFactoryTests
    {
        [Fact]
        public static void GetInstance_Should_Throws_Exception_When_Options_Is_Null()
        {
            // arrange
            ElasticsearchOptions options = null;

            // act & assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                ElasticClientFactory.GetInstance(ConnectionMode.Write, options));
        }

        [Fact]
        public static void GetInstance_Should_Return_Read_Instance()
        {
            // arrange
            ElasticsearchOptions options = new ElasticsearchOptions
            {
                DefaultIndexName = "index-d",
                DefaultTypeName = "type-d",
                Environment = "my-env",
                MaximumRetries = 4,
                ReadUrl = "http://esread:9200",
                WriteUrl = "http://eswrite:9200",
                TimeoutInSeconds = 30
            };

            // act
            var client = ElasticClientFactory.GetInstance(ConnectionMode.Read, options);

            // assert
            Assert.Equal("type-d", client.ConnectionSettings.DefaultTypeName);
            Assert.Equal("index-d", client.ConnectionSettings.DefaultIndex);
            Assert.Equal(30, client.ConnectionSettings.RequestTimeout.TotalSeconds);
            Assert.Equal(4, client.ConnectionSettings.MaxRetries);
            Assert.Equal("my-env", client.ConnectionSettings.Headers.Get("Environment"));
            Assert.Equal("http://esread:9200/", client.ConnectionSettings.ConnectionPool.Nodes.First().Uri.AbsoluteUri);
        }

        [Fact]
        public static void GetInstance_Should_Return_Write_Instance()
        {
            // arrange
            ElasticsearchOptions options = new ElasticsearchOptions
            {
                DefaultIndexName = "index-d",
                DefaultTypeName = "type-d",
                Environment = "my-env",
                MaximumRetries = 4,
                ReadUrl = "http://esread:9200",
                WriteUrl = "http://eswrite:9200",
                TimeoutInSeconds = 30
            };

            // act
            var client = ElasticClientFactory.GetInstance(ConnectionMode.Write, options);

            // assert
            Assert.Equal("type-d", client.ConnectionSettings.DefaultTypeName);
            Assert.Equal("index-d", client.ConnectionSettings.DefaultIndex);
            Assert.Equal(30, client.ConnectionSettings.RequestTimeout.TotalSeconds);
            Assert.Equal(4, client.ConnectionSettings.MaxRetries);
            Assert.Equal("my-env", client.ConnectionSettings.Headers.Get("Environment"));
            Assert.Equal("http://eswrite:9200/", client.ConnectionSettings.ConnectionPool.Nodes.First().Uri.AbsoluteUri);
        }
    }
}
