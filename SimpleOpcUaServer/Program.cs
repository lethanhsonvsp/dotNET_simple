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
        protected override MasterNodeManager CreateMasterNodeManager(
            IServerInternal server, ApplicationConfiguration configuration) // Tạo MasterNodeManager
        {
            var nodeManagers = new List<INodeManager>
            {
                new MyNodeManager(server, configuration) // Thêm MyNodeManager vào danh sách node managers
            };
            return new MasterNodeManager(server, configuration, null, nodeManagers.ToArray()); // Trả về MasterNodeManager với danh sách node managers
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
