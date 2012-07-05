﻿using System;
using System.Collections.Generic;

namespace Uniform.InMemory
{
    public class InMemoryDatabase : IDatabase
    {
        private readonly DatabaseMetadata _metadata;
        private readonly Dictionary<String, ICollection> _collections = new Dictionary<string, ICollection>();

        public InMemoryDatabase(DatabaseMetadata metadata)
        {
            _metadata = metadata;
        }

        public ICollection GetCollection(String name)
        {
            ICollection value;
            if (!_collections.TryGetValue(name, out value))
                throw new Exception("Collection not exists");

            return value;
        }

        public ICollection<TDocument> GetCollection<TDocument>(string name)
        {
            ICollection value;
            if (!_collections.TryGetValue(name, out value))
                _collections[name] = value = new InMemoryCollection<TDocument>();

            return (InMemoryCollection<TDocument>) value;
        }

        public ICollection<TDocument> GetCollection<TDocument>()
        {
            return GetCollection<TDocument>(_metadata.GetCollectionName(typeof (TDocument)));
        }
    }
}