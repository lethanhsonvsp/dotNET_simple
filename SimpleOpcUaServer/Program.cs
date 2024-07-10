using System; // Thư viện cơ bản của C#
using System.Collections.Generic; // Thư viện cho các tập hợp chung
using System.IO; // Thư viện cho các thao tác với tệp
using System.Security.Cryptography.X509Certificates; // Thư viện cho chứng chỉ X509
using Opc.Ua; // Thư viện chính cho OPC UA
using Opc.Ua.Configuration; // Thư viện cho cấu hình OPC UA
using Opc.Ua.Server; // Thư viện cho server OPC UA

namespace SimpleOPCUAServer // Định nghĩa không gian tên cho dự án
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var application = ServerConfiguration.ConfigureAndStartServer(); // Cấu hình và khởi động server

            if (application != null) // Kiểm tra nếu server được khởi động thành công
            {
                Console.WriteLine("Server started. Press Enter to exit..."); // Hiển thị thông báo server đã khởi động
                Console.ReadLine(); // Đợi người dùng nhấn Enter để thoát

                application.Stop(); // Dừng server
            }
        }
    }

    public class MyOpcUaServer : StandardServer // Lớp MyOpcUaServer kế thừa từ StandardServer
    {
        protected override MasterNodeManager CreateMasterNodeManager(IServerInternal server, ApplicationConfiguration configuration)
        {
            // Danh sách các NodeManager
            IList<INodeManager> nodeManagers = new List<INodeManager>();

            // Tạo các NodeManager và thêm vào danh sách
            nodeManagers.Add(new MyNodeManager(server, configuration));
            //nodeManagers.Add(new MyNodeManagerRos2(server, configuration));

            // Trả về một MasterNodeManager quản lý tất cả các NodeManager
            return new MasterNodeManager(server, configuration, null, nodeManagers.ToArray());
        }
    }

    public class MyNodeManager : CustomNodeManager2
    {
        private BaseDataVariableState? waterFlowRateVariable;
        private BaseDataVariableState? pumpTemperatureVariable;
        private BaseDataVariableState? pumpStatusVariable; // Thêm biến trạng thái của bơm

        public MyNodeManager(IServerInternal server, ApplicationConfiguration configuration)
            : base(server, configuration, "http://yourorganization.org/SimpleOpcUaServer/")
        {
        }

        public override void CreateAddressSpace(IDictionary<NodeId, IList<IReference>> externalReferences)
        {
            base.CreateAddressSpace(externalReferences);

            // Tạo một folder
            var folder = new FolderState(null)
            {
                SymbolicName = "MyObjects",
                ReferenceTypeId = ReferenceTypeIds.Organizes,
                TypeDefinitionId = ObjectTypeIds.FolderType,
                NodeId = new NodeId("MyObjects", NamespaceIndex),
                BrowseName = new QualifiedName("MyObjects", NamespaceIndex),
                DisplayName = new LocalizedText("MyObjects"),
                Description = new LocalizedText("MyObjects"),
                WriteMask = AttributeWriteMask.None,
                UserWriteMask = AttributeWriteMask.None
            };

            AddPredefinedNode(SystemContext, folder);

            // Tạo biến lưu lượng nước
            waterFlowRateVariable = CreateVariable(folder, "WaterFlowRate", "Water Flow Rate", BuiltInType.Double, ValueRanks.Scalar);
            waterFlowRateVariable.Value = new Variant(0.0);
            waterFlowRateVariable.StatusCode = StatusCodes.Good;
            waterFlowRateVariable.Timestamp = DateTime.UtcNow;

            AddPredefinedNode(SystemContext, waterFlowRateVariable);

            // Tạo biến nhiệt độ của bơm
            pumpTemperatureVariable = CreateVariable(folder, "PumpTemperature", "Pump Temperature", BuiltInType.Double, ValueRanks.Scalar);
            pumpTemperatureVariable.Value = new Variant(30.0); // Giá trị ban đầu là 30 độ C
            pumpTemperatureVariable.StatusCode = StatusCodes.Good;
            pumpTemperatureVariable.Timestamp = DateTime.UtcNow;

            AddPredefinedNode(SystemContext, pumpTemperatureVariable);

            // Tạo biến trạng thái của bơm
            pumpStatusVariable = CreateVariable(folder, "PumpStatus", "Pump Status", BuiltInType.String, ValueRanks.Scalar);
            pumpStatusVariable.Value = new Variant("N"); // Giá trị ban đầu là 'N'
            pumpStatusVariable.StatusCode = StatusCodes.Good;
            pumpStatusVariable.Timestamp = DateTime.UtcNow;

            AddPredefinedNode(SystemContext, pumpStatusVariable);
        }

        private BaseDataVariableState CreateVariable(NodeState parent, string name, string description, BuiltInType dataType, int valueRank)
        {
            var variable = new BaseDataVariableState(parent)
            {
                SymbolicName = name,
                ReferenceTypeId = ReferenceTypeIds.HasComponent,
                TypeDefinitionId = VariableTypeIds.BaseDataVariableType,
                NodeId = new NodeId(name, NamespaceIndex),
                BrowseName = new QualifiedName(name, NamespaceIndex),
                DisplayName = new LocalizedText(name),
                Description = new LocalizedText(description),
                WriteMask = AttributeWriteMask.None,
                UserWriteMask = AttributeWriteMask.None,
                DataType = (uint)dataType,
                ValueRank = valueRank,
                AccessLevel = AccessLevels.CurrentReadOrWrite,
                UserAccessLevel = AccessLevels.CurrentReadOrWrite,
                Historizing = false
            };

            return variable;
        }
    }
    public partial class ServerConfiguration // Lớp ServerConfiguration chứa các phương thức cấu hình server
    {
        public static ApplicationInstance? ConfigureAndStartServer() // Phương thức cấu hình và khởi động server
        {
            ApplicationInstance? applicationInstance = null; // Khởi tạo ApplicationInstance là null
            try
            {
                Console.WriteLine("Starting OPC UA Server..."); // Hiển thị thông báo bắt đầu khởi động server

                var config = new ApplicationConfiguration()
                {
                    ApplicationName = "OpcUaServer",
                    ApplicationType = ApplicationType.Server,
                    SecurityConfiguration = new SecurityConfiguration
                    {
                        ApplicationCertificate = new CertificateIdentifier
                        {
                            StoreType = "Directory",
                            StorePath = "CertificateStores/MachineDefault",
                            SubjectName = "CN=le, O=le, C=le"
                        },
                        TrustedPeerCertificates = new CertificateTrustList
                        {
                            StoreType = "Directory",
                            StorePath = "CertificateStores/UA Applications"
                        },
                        TrustedIssuerCertificates = new CertificateTrustList
                        {
                            StoreType = "Directory",
                            StorePath = "CertificateStores/UA Certificate Authorities"
                        },
                        RejectedCertificateStore = new CertificateTrustList
                        {
                            StoreType = "Directory",
                            StorePath = "CertificateStores/Rejected Certificates"
                        },
                        AutoAcceptUntrustedCertificates = true,
                        AddAppCertToTrustedStore = true
                    },
                    ServerConfiguration = new Opc.Ua.ServerConfiguration
                    {
                        BaseAddresses = { "opc.tcp://localhost:4840" }
                    },
                    TransportConfigurations = new TransportConfigurationCollection(),
                    TransportQuotas = new TransportQuotas { OperationTimeout = 15000 },
                    ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = 60000 }
                };

                config.Validate(ApplicationType.Server).Wait(); // Xác thực cấu hình
                Console.WriteLine("Configuration validated."); // Hiển thị thông báo cấu hình đã được xác thực

                applicationInstance = new ApplicationInstance
                {
                    ApplicationName = "OpcUaServer",
                    ApplicationType = ApplicationType.Server,
                    ApplicationConfiguration = config
                };

                bool haveAppCertificate = applicationInstance.CheckApplicationInstanceCertificate(false, 0).Result;

                if (!haveAppCertificate)
                {
                    string certificatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "certificate.pfx");

                    try
                    {
                        var certificate = new X509Certificate2(certificatePath, "le"); // Thay thế bằng mật khẩu thực tế
                        config.SecurityConfiguration.ApplicationCertificate.Certificate = certificate;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error loading certificate: {ex.Message}");
                        return null;
                    }

                    haveAppCertificate = applicationInstance.CheckApplicationInstanceCertificate(false, 0).Result;
                    if (!haveAppCertificate)
                    {
                        throw new Exception("Application instance certificate invalid!");
                    }
                }

                var server = new MyOpcUaServer();
                applicationInstance.Start(server).Wait();
                Console.WriteLine("Server started successfully."); // Hiển thị thông báo server đã khởi động thành công

                return applicationInstance;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return null;
            }
        }
    }
}
