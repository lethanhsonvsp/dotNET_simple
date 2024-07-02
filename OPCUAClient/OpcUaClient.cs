using System;
using System.Threading;
using System.Threading.Tasks;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;

namespace OPCUAClient
{
    public class OpcUaClient
    {
        private readonly string _endpointUrl;
        private readonly string _namespaceUri;
        private ApplicationInstance? _application; // Khai báo nullable
        private Session? _session; // Khai báo nullable
        private int _namespaceIndex;

        public OpcUaClient(string endpointUrl, string namespaceUri)
        {
            _endpointUrl = endpointUrl;
            _namespaceUri = namespaceUri;
        }

        public async Task ConnectAsync()
        {
            _application = ConfigureApplication();
            await CheckApplicationCertificateAsync();
            _session = await CreateSessionAsync();
            _namespaceIndex = GetNamespaceIndex(_session, _namespaceUri);
        }

        public void Disconnect()
        {
            _session?.Close(); // Sử dụng null conditional operator
            Console.WriteLine("Session closed.");
        }

        public async Task WriteValueAsync(string nodeName, object value)
        {
            if (_session == null) throw new InvalidOperationException("Session is not established.");

            var nodeToWrite = new WriteValue
            {
                NodeId = new NodeId(nodeName, (ushort)_namespaceIndex),
                AttributeId = Attributes.Value,
                Value = new DataValue(new Variant(value))
            };
            var nodesToWrite = new WriteValueCollection { nodeToWrite };

            var cancellationToken = new CancellationToken();
            var results = await _session.WriteAsync(null, nodesToWrite, cancellationToken);

            foreach (var result in results.Results)
            {
                Console.WriteLine($"Write StatusCode: {result}");
            }
        }

        public async Task ReadValueAsync(string nodeName)
        {
            if (_session == null) throw new InvalidOperationException("Session is not established.");

            var nodeToRead = new ReadValueId
            {
                NodeId = new NodeId(nodeName, (ushort)_namespaceIndex),
                AttributeId = Attributes.Value
            };
            var nodesToRead = new ReadValueIdCollection { nodeToRead };

            var cancellationToken = new CancellationToken();
            var readResults = await _session.ReadAsync(null, 0, TimestampsToReturn.Both, nodesToRead, cancellationToken);

            foreach (var readResult in readResults.Results)
            {
                if (readResult.StatusCode == StatusCodes.Good)
                {
                    Console.WriteLine($"{nodeName}: {readResult.Value}");
                }
                else
                {
                    Console.WriteLine($"Error reading {nodeName}: {readResult.StatusCode}");
                }
            }
        }

        private static ApplicationInstance ConfigureApplication()
        {
            return new ApplicationInstance
            {
                ApplicationName = "Simple OPC UA Client",
                ApplicationType = ApplicationType.Client,
                ApplicationConfiguration = new ApplicationConfiguration
                {
                    ApplicationName = "Simple OPC UA Client",
                    ApplicationType = ApplicationType.Client,
                    SecurityConfiguration = new SecurityConfiguration
                    {
                        AutoAcceptUntrustedCertificates = true,
                        ApplicationCertificate = new CertificateIdentifier
                        {
                            StoreType = "Directory",
                            StorePath = "CertificateStores/MachineDefault"
                        }
                    },
                    ClientConfiguration = new ClientConfiguration(),
                    TransportConfigurations = new TransportConfigurationCollection(),
                    TransportQuotas = new TransportQuotas { OperationTimeout = 15000 }
                }
            };
        }

        private async Task CheckApplicationCertificateAsync()
        {
            if (_application == null) throw new InvalidOperationException("Application instance is not configured.");

            _application.ApplicationConfiguration.SecurityConfiguration.AutoAcceptUntrustedCertificates = true;
            await _application.CheckApplicationInstanceCertificate(false, 0);
        }

        private async Task<Session> CreateSessionAsync()
        {
            if (_application == null) throw new InvalidOperationException("Application instance is not configured.");

            var endpoint = CoreClientUtils.SelectEndpoint(_endpointUrl, useSecurity: false);
            return await Session.Create(
                _application.ApplicationConfiguration,
                new ConfiguredEndpoint(null, endpoint),
                false,
                "",
                60000,
                new UserIdentity(new AnonymousIdentityToken()),
                null
            );
        }

        private static int GetNamespaceIndex(Session session, string namespaceUri)
        {
            int namespaceIndex = session.NamespaceUris.GetIndex(namespaceUri);
            if (namespaceIndex == -1)
            {
                throw new Exception($"Namespace URI '{namespaceUri}' not found in the server's namespace table.");
            }

            Console.WriteLine($"Namespace Index: {namespaceIndex}");
            return namespaceIndex;
        }
    }
}
