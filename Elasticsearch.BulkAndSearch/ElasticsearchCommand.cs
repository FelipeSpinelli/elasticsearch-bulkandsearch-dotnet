﻿using Elasticsearch.BulkAndSearch.Models;
using Nest;
using System;
using System.Collections.Generic;

namespace Elasticsearch.BulkAndSearch
{
    public class ElasticsearchCommand<TEntity> : BaseElasticsearch<TEntity>, IElasticsearchCommand<TEntity> where TEntity : class
    {
        public ElasticsearchCommand(
            ElasticsearchOptions options, 
            Func<string, TEntity, string> generateIndexName)
            : base(ConnectionMode.Write, options, generateIndexName)
        { }

        public bool Bulk(IEnumerable<TEntity> documents, string type = null)
        {
            BulkDescriptor descriptor = new BulkDescriptor();
            foreach (var document in documents)
            {
                descriptor.Index<object>(i => i
                    .Index(this.GetIndexName(document))
                    .Type(type ?? this.Options.DefaultTypeName)
                    .Document(document));
            }

            return this.ElasticClient.Bulk(descriptor).IsValid;
        }

        public bool Upsert(TEntity document, string type = null)
        {
            var index = this.GetIndexName(document);
            return this.ElasticClient.Index((object) document, i => 
                i.Index(index).Type(type ?? this.Options.DefaultTypeName)).IsValid;
        }
    }
}
