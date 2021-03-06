﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MongoDB.Bson.Serialization.Attributes;
using Uniform.Exceptions;
using Uniform.Utils;

namespace Uniform
{
    /// <summary>
    /// Database metadata, contains all document types and provides some
    /// metadata related services.
    /// </summary>
    public class DatabaseMetadata
    {
        /// <summary>
        /// Configuration of DatabaseMetadata, used to create this type
        /// </summary>
        private readonly DatabaseConfiguration _configuration;

        public DatabaseConfiguration Configuration
        {
            get { return _configuration; }
        }

        private Dictionary<String, IDocumentDatabase> _databases = new Dictionary<string, IDocumentDatabase>();

        public Dictionary<string, IDocumentDatabase> Databases
        {
            get { return _databases; }
            set { _databases = value; }
        }

        /// <summary>
        /// All registered document types. 
        /// </summary>
        private readonly Dictionary<Type, List<DocumentConfiguration>> _documentConfigurations = new Dictionary<Type, List<DocumentConfiguration>>();

        public IEnumerable<Type> DocumentTypes
        {
            get { return _documentConfigurations.Keys; }
        }

        /// <summary>
        /// Creates DatabaseMetadata with specified configuration
        /// </summary>
        /// <param name="configuration"></param>
        public DatabaseMetadata(DatabaseConfiguration configuration)
        {
            _configuration = configuration;
            _databases = configuration.Databases;
        }

        public List<DocumentConfiguration> GetDocumentConfigurations(Type documentType)
        {
            return _documentConfigurations[documentType];
        }

        /// <summary>
        /// Returns true, if type was registered as document type
        /// </summary>
        public bool IsDocumentType(Type type)
        {
            return _documentConfigurations.ContainsKey(type);
        }
        
        #region ID properties services
        /// <summary>
        /// Cache for ID properties. (DocumentType -> Id PropertyInfo) 
        /// </summary>
        private readonly ConcurrentDictionary<Type, PropertyInfo> _idPropertiesCache = new ConcurrentDictionary<Type, PropertyInfo>();

        /// <summary>
        /// Returns document id value. 
        /// </summary>
        public String GetDocumentId(Object document)
        {
            if (document == null)
                throw new ArgumentNullException("document");

            var info = GetDocumentIdPropertyInfo(document.GetType());
            return (String) info.GetValue(document, new object[0]);
        }

        /// <summary>
        /// Sets id property to specified value. 
        /// </summary>
        public void SetDocumentId(Object obj, String value)
        {
            if (obj == null) throw new ArgumentNullException("obj");

            var info = GetDocumentIdPropertyInfo(obj.GetType());
            info.SetValue(obj, value, new object[0]);
        }

        public PropertyInfo GetDocumentIdPropertyInfo(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");

            PropertyInfo info;
            if (!_idPropertiesCache.TryGetValue(type, out info))
            {
                PropertyInfo[] propertyInfos = type.GetProperties()
                    .Where(x => Attribute.IsDefined(x, typeof(DocumentIdAttribute), false))
                    .ToArray();

                if (propertyInfos.Length <= 0)
                    throw new Exception(String.Format(
                        "Document of type '{0}' does not have id property, marked with [DocumentId] attribute. Please mark property with this attribute :)", 
                        type.FullName));

                if (propertyInfos.Length != 1)
                    throw new Exception(String.Format(
                        "Document {0} has two properties with [DocumentId] attribute. This is not allowed.",
                        type.FullName));

                info = propertyInfos[0];

                if (propertyInfos[0].PropertyType != typeof(string))
                    throw new Exception(String.Format(
                        "Property {0} of type {1} (in document {2}) marked with [DocumentId] attribute, but return type should be String, not {1}",
                        info.Name, info.PropertyType.Name, type.FullName));

                _idPropertiesCache[type] = info;
            }

            return info;
        }

        #endregion
        
        #region Version properties services
        /// <summary>
        /// Cache for version properties. (DocumentType -> Version PropertyInfo) 
        /// </summary>
        private readonly ConcurrentDictionary<Type, PropertyInfo> _versionPropertiesCache = new ConcurrentDictionary<Type, PropertyInfo>();

        /// <summary>
        /// Returns document id value. 
        /// </summary>
        public Int32? GetDocumentVersion(Object document)
        {
            if (document == null)
                throw new ArgumentNullException("document");

            var info = GetDocumentVersionPropertyInfo(document.GetType());

            if (info == null)
                return null;

            return (Int32) info.GetValue(document, new object[0]);
        }

        /// <summary>
        /// Sets id property to specified value. 
        /// </summary>
        public void SetDocumentVersion(Object obj, Int32 version)
        {
            if (obj == null) throw new ArgumentNullException("obj");

            var info = GetDocumentVersionPropertyInfo(obj.GetType());

            if (info == null)
                return;

            info.SetValue(obj, version, new object[0]);
        }

        public PropertyInfo GetDocumentVersionPropertyInfo(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");

            PropertyInfo info;
            if (!_versionPropertiesCache.TryGetValue(type, out info))
            {
                PropertyInfo[] propertyInfos = type.GetProperties()
                    .Where(x => Attribute.IsDefined(x, typeof(DocumentVersionAttribute), false))
                    .ToArray();

                if (propertyInfos.Length <= 0)
                {
                    info = null;
                }
                else
                {
                    if (propertyInfos.Length != 1)
                        throw new Exception(String.Format(
                            "Document {0} has two properties with [DocumentVersion] attribute. This is not allowed.",
                            type.FullName));

                    info = propertyInfos[0];

                    if (info.PropertyType != typeof(Int32))
                        throw new Exception(String.Format(
                            "Property {0} of type {1} (in document {2}) marked with [DocumentVersion] attribute, but return type should be Int32, not {1}",
                            info.Name, info.PropertyType.Name, type.FullName));
                }
                _versionPropertiesCache[type] = info;
            }

            return info;
        }

        #endregion
    }
}