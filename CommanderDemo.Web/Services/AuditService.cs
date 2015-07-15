using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using CfgDotNet;
using CommandR.Authentication;
using MongoDB.Driver;

//_container.Register<AuditService>(lifestyle);
namespace CommanderDemo.Web
{
    /// <summary>
    /// The AuditService stores a hierarchy of commands and sql executed to Mongo.
    /// This functionality will eventually be cleaned up and added as a separate Nuget package.
    /// </summary>
    public class AuditService : IDisposable
    {
        private readonly Settings _settings;
        private readonly MongoCollection<AuditDocument> _collection;
        private readonly string _process;
        private AuditDocument _auditDocument;
        private bool _disposed; // false

        public AuditService(Settings settings)
        {
            _settings = settings;
            if (_settings.IsDisabled)
                return;

            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            _process = assembly.GetName().Name;

            var mongoUrl = MongoUrl.Create(settings.ConnectionString);
            var client = new MongoClient(settings.ConnectionString);
            var server = client.GetServer();
            var database = server.GetDatabase(mongoUrl.DatabaseName);

            _collection = database.GetCollection<AuditDocument>(settings.CollectionName);
        }

        public void AddChild(AuditDocument auditDocument, AppContext context)
        {
            if (_settings.IsDisabled)
                return;

            if (_auditDocument == null)
                _auditDocument = new AuditDocument("Parent");

            if (_auditDocument.Body == null)
                _auditDocument.Body = new List<AuditDocument>();

            var children = (List<AuditDocument>)_auditDocument.Body;
            auditDocument.Context = context;
            auditDocument.Process = _process;

            if (auditDocument.DocumentType != "SQL")
            {
                children.Add(auditDocument);
            }
            else
            {
                var firstSqlAuditDocument = children.FirstOrDefault(x => x.DocumentType == "SQL");
                if (firstSqlAuditDocument != null)
                {
                    firstSqlAuditDocument.Body = (firstSqlAuditDocument.Body ?? string.Empty) + auditDocument.Body.ToString();
                }
                else
                {
                    children.Add(auditDocument);
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing && _auditDocument != null && _collection != null)
            {
                _collection.Insert(_auditDocument);
            }

            _disposed = true;
        }

        public class Settings : BaseSettings
        {
            // ex. mongodb://[Username:password@]host[:port]/[database][?options]
            public string ConnectionString { get; set; }
            public string CollectionName { get; set; }
            public string IncludeCommands { get; set; }
            public string ExcludeCommands { get; set; }

            public override void Validate()
            {
                if (IsDisabled)
                    return;

                if (string.IsNullOrWhiteSpace(CollectionName))
                    CollectionName = "Audit";

                if (string.IsNullOrWhiteSpace(ConnectionString))
                    ConnectionString = "mongodb://127.0.0.1/test";

                CollectionName = CollectionName.Replace("_MACHINE", "_" + Environment.MachineName);

                TestMongoConnection(ConnectionString);
            }

            private static void TestMongoConnection(string connectionString)
            {
                var settings = MongoClientSettings.FromUrl(new MongoUrl(connectionString));
                settings.ConnectTimeout = TimeSpan.FromSeconds(5);
                var client = new MongoClient(settings);
                var server = client.GetServer();
                server.Ping();
            }
        };
    };

    public class AuditDocument
    {
        private object _body;

        public AuditDocument(string documentType, string name = "", object body = null)
        {
            DocumentType = documentType;
            Name = name;
            Body = body;
            Created = DateTime.UtcNow;
            HostName = Dns.GetHostEntry(string.Empty).HostName;
        }

        public DateTime Created { get; set; }
        public Dictionary<string, object> Context { get; set; }
        public string HostName { get; set; }
        public string Process { get; set; }
        public string DocumentType { get; set; }
        public string Name { get; set; }
        public string BodyType { get; set; }

        public object Body
        {
            get { return _body; }
            set
            {
                // mongodb apache code
                // if we want shorter full names - TBD
                // TypeNameDiscriminator.GetDiscriminator(value.GetType());
                BodyType = value == null ? "null" : value.GetType().FullName;
                _body = value;
            }
        }
    };
}